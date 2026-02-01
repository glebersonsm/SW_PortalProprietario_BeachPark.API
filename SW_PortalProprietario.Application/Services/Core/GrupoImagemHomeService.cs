using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Functions;
using System.Linq;
using System.Text;
using SW_Utils.Auxiliar;
using Dapper;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class GrupoImagemHomeService : IGrupoImagemHomeService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<GrupoImagemHomeService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;

        public GrupoImagemHomeService(
            IRepositoryNH repository,
            ILogger<GrupoImagemHomeService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
        }

        public async Task<GrupoImagemHomeModel?> SaveGrupoImagemHome(GrupoImagemHomeInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                GrupoImagemHome grupoImagemHomeOriginal = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                    grupoImagemHomeOriginal = (await _repository.FindByHql<GrupoImagemHome>($"From GrupoImagemHome gd Where gd.Id = {model.Id}")).FirstOrDefault();

                var grupoImagemHome = grupoImagemHomeOriginal != null ? _mapper.Map(model, grupoImagemHomeOriginal) : _mapper.Map<GrupoImagemHome>(model);
                var company = (await _repository.FindBySql<EmpresaModel>("Select e.Id From Empresa e Order by e.Id")).FirstOrDefault();
                if (company == null)
                {
                    throw new ArgumentException("Não foi possível identificar a empresa padrão do sistema");
                }

                grupoImagemHome.Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = company!.Id.GetValueOrDefault() };

                // Se for novo grupo e não tiver ordem definida, definir ordem padrão
                if (grupoImagemHomeOriginal == null && (model.Ordem == null || model.Ordem == 0))
                {
                    var maxOrdem = (await _repository.FindBySql<int?>("Select Max(Ordem) From GrupoImagemHome")).FirstOrDefault();
                    grupoImagemHome.Ordem = (maxOrdem ?? 0) + 1;
                }

                var result = await _repository.Save(grupoImagemHome);
                await SincronizarTagsRequeridas(grupoImagemHome, model.TagsRequeridas, model.RemoverTagsNaoEnviadas ?? false);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Grupo de imagem home: ({result.Id} - {grupoImagemHome.Nome}) salvo com sucesso!");

                    if (result != null)
                    {
                        var itemRetornar = (await Search(new SearchGrupoImagemHomeModel() { Id = result.Id })).AsList();
                        if (itemRetornar != null && itemRetornar.Any())
                            return itemRetornar.First();
                    }
                }
                throw exception ?? new Exception($"Não foi possível salvar o Grupo de imagem home: ({grupoImagemHome.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Grupo de imagem home: ({model.Name})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<DeleteResultModel> DeleteGrupoImagemHome(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {
                var grupoImagemHome = await _repository.FindById<GrupoImagemHome>(id);
                if (grupoImagemHome is null)
                {
                    throw new ArgumentException($"Não foi encontrado o grupo de imagem home com Id: {id}!");
                }

                _repository.BeginTransaction();

                // Fazer logging detalhado das tags relacionadas antes da remoção
                var tagsRelacionadas = await _repository.FindByHql<GrupoImagemHomeTags>(
                    @$"From GrupoImagemHomeTags gita 
                            Inner Join Fetch gita.Tags t 
                            Inner Join Fetch gita.GrupoImagemHome g
                            Inner Join Fetch g.Empresa e
                       Where 
                            g.Id = {id} and 
                            gita.UsuarioRemocao is null and 
                            gita.DataHoraRemocao is null");

                var usuario = await _repository.GetLoggedUser();
                foreach (var tagRelacionada in tagsRelacionadas)
                {
                    // Log detalhado da remoção da tag durante exclusão do grupo
                    _logger.LogInformation($"[REMOÇÃO TAG - EXCLUSÃO GRUPO HOME] Removendo vínculo de tag por exclusão do grupo da home | " +
                                          $"GrupoImagemHomeID: {tagRelacionada.GrupoImagemHome?.Id} | " +
                                          $"GrupoImagemHomeNome: {tagRelacionada.GrupoImagemHome?.Nome} | " +
                                          $"TagID: {tagRelacionada.Tags?.Id} | " +
                                          $"TagNome: {tagRelacionada.Tags?.Nome} | " +
                                          $"UsuarioID: {usuario?.userId ?? "Sistema"} | " +
                                          $"DataHora: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                                          $"EmpresaID: {tagRelacionada.GrupoImagemHome?.Empresa?.Id} | " +
                                          $"TipoRemocao: Exclusão do grupo");

                    await _repository.Remove(tagRelacionada);
                }

                await _repository.ExecuteSqlCommand($"Delete From ImagemGrupoImagemHome Where GrupoImagemHome = {id}");
                await _repository.Remove(grupoImagemHome);

                var resultCommit = await _repository.CommitAsync();
                if (resultCommit.executed)
                {
                    result.Result = "Removido com sucesso!";
                }
                else
                {
                    throw resultCommit.exception ?? new Exception("Não foi possível realizar a operação");
                }

                return result;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, $"Não foi possível deletar o grupo de imagem home: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<GrupoImagemHomeModel>?> Search(SearchGrupoImagemHomeModel searchModel)
        {
            List<Parameter> parameters = new();
            StringBuilder sb = new("From GrupoImagemHome gd Inner Join Fetch gd.Empresa emp Where 1 = 1");

            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(gd.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and gd.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (searchModel.Ids != null && searchModel.Ids.Any())
            {
                sb.AppendLine($" and gd.Id in ({string.Join(",", searchModel.Ids.AsList())})");
            }

            if (searchModel.UsuarioCriacao.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and gd.UsuarioCriacao = {searchModel.UsuarioCriacao.GetValueOrDefault()}");
            }

            sb.AppendLine(" Order by gd.Ordem, gd.Nome");

            var gruposImagens = await _repository.FindByHql<GrupoImagemHome>(sb.ToString(), parameters.ToArray());
            var itensRetorno = gruposImagens.Select(a => _mapper.Map<GrupoImagemHomeModel>(a)).ToList();

            if (itensRetorno.Any())
            {
                var imagensDosGrupos = (await _repository.FindBySql<ImagemGrupoImagemHomeModel>($@"Select 
                    I.Id, 
                    i.UsuarioCriacao, 
                    i.DataHoraCriacao, 
                    i.UsuarioAlteracao, 
                    i.DataHoraAlteracao,
                    i.Imagem,
                    i.NomeBotao,
                    i.LinkBotao,
                    i.Ordem,
                    g.Id as GrupoImagemHomeId,
                    g.Nome as GrupoImagemHomeName,
                    i.Nome as Name
                    From 
                    ImagemGrupoImagemHome i 
                    Inner Join GrupoImagemHome g on i.GrupoImagemHome = g.Id 
                    Where 
                    g.Id in ({string.Join(",", gruposImagens.Select(a => a.Id).AsList())})
                    Order by i.Ordem, i.Id")).AsList();

                foreach (var itemGroup in itensRetorno)
                {
                    var imagensDoGrupo = imagensDosGrupos.Where(c => c.GrupoImagemHomeId == itemGroup.Id).OrderBy(x => x.Ordem).ThenBy(x => x.Id).AsList();
                    itemGroup.Images = imagensDoGrupo;
                }
            }

            if (itensRetorno.Any())
            {
                var grupoImagemHomeTags = (await _repository.FindByHql<GrupoImagemHomeTags>(@$"From
                    GrupoImagemHomeTags gita 
                    Inner Join Fetch gita.GrupoImagemHome gi 
                    Inner Join Fetch gita.Tags t 
                    Where 
                    gi.Id in ({string.Join(",", itensRetorno.Select(b => b.Id))})")).AsList();

                foreach (var itemGrupo in itensRetorno)
                {
                    var tagsDoGrupo = grupoImagemHomeTags.Where(a => a.GrupoImagemHome != null && a.GrupoImagemHome.Id == itemGrupo.Id && a.UsuarioRemocao == null && a.DataHoraRemocao == null);
                    if (tagsDoGrupo.Any())
                        itemGrupo.TagsRequeridas = tagsDoGrupo.Select(b => _mapper.Map<GrupoImagemHomeTagsModel>(b)).AsList();
                }
            }

            return await _serviceBase.SetUserName(itensRetorno);
        }

        private async Task SincronizarTagsRequeridas(GrupoImagemHome grupoImagemHome, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                // Buscar tags existentes para fazer soft delete (gerar log de auditoria)
                var tagsParaRemover = new List<GrupoImagemHomeTags>();

                if (listTags == null || listTags.Count == 0)
                {
                    // Remover todas as tags - usar Inner Join Fetch para carregar Tags e GrupoImagemHome com nomes
                    tagsParaRemover = (await _repository.FindByHql<GrupoImagemHomeTags>(
                        @$"From GrupoImagemHomeTags gita 
                                Inner Join Fetch gita.Tags t 
                                Inner Join Fetch gita.GrupoImagemHome g
                                Inner Join Fetch g.Empresa e
                           Where 
                                g.Id = {grupoImagemHome.Id} and 
                                gita.UsuarioRemocao is null and 
                                gita.DataHoraRemocao is null")).ToList();
                }
                else
                {
                    // Remover apenas tags que não estão na lista - usar Inner Join Fetch para carregar Tags e GrupoImagemHome com nomes
                    tagsParaRemover = (await _repository.FindByHql<GrupoImagemHomeTags>(
                        @$"From GrupoImagemHomeTags gita 
                                Inner Join Fetch gita.Tags t 
                                Inner Join Fetch gita.GrupoImagemHome g
                                Inner Join Fetch g.Empresa e
                           Where 
                                g.Id = {grupoImagemHome.Id} and 
                                t.Id not in ({string.Join(",", listTags)}) and 
                                gita.UsuarioRemocao is null and 
                                gita.DataHoraRemocao is null")).ToList();
                }

                var usuario = await _repository.GetLoggedUser();
                foreach (var tagParaRemover in tagsParaRemover)
                {
                    // Log detalhado da remoção da tag
                    _logger.LogInformation($"[REMOÇÃO TAG] Removendo vínculo de tag do grupo da home | " +
                                          $"GrupoImagemHomeID: {tagParaRemover.GrupoImagemHome?.Id} | " +
                                          $"GrupoImagemHomeNome: {tagParaRemover.GrupoImagemHome?.Nome} | " +
                                          $"TagID: {tagParaRemover.Tags?.Id} | " +
                                          $"TagNome: {tagParaRemover.Tags?.Nome} | " +
                                          $"UsuarioID: {usuario?.userId ?? "Sistema"} | " +
                                          $"DataHora: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                                          $"EmpresaID: {tagParaRemover.GrupoImagemHome?.Empresa?.Id} | " +
                                          $"TipoRemocao: Sincronização");

                    await _repository.Remove(tagParaRemover);
                }
            }

            if (listTags != null && listTags.Any())
            {
                var allTags = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)})")).AsList();
                var tagsInexistentes = listTags.Where(c => !allTags.Any(b => b.Id == c)).AsList();
                if (tagsInexistentes.Count > 0)
                {
                    throw new ArgumentException($"Tags não encontradas: {string.Join(",", tagsInexistentes)}");
                }

                // Buscar tags com seus nomes para garantir que estejam carregadas no log
                var tags = (await _repository.FindByHql<SW_PortalProprietario.Domain.Entities.Core.Geral.Tags>(
                    @$"From Tags t Where t.Id in ({string.Join(",", listTags)}) and 
                    Not Exists(Select dc.Tags From GrupoImagemHomeTags dc Where dc.GrupoImagemHome = {grupoImagemHome.Id} and dc.Tags = t.Id and dc.UsuarioRemocao is null and dc.DataHoraRemocao is null)")).ToList();

                // Buscar grupo com nome carregado usando HQL para garantir que o nome esteja disponível
                var grupoComNome = (await _repository.FindByHql<GrupoImagemHome>(
                    $"From GrupoImagemHome g Where g.Id = {grupoImagemHome.Id}")).FirstOrDefault() ?? grupoImagemHome;

                foreach (var tag in tags)
                {
                    var grupoImagemHomeTags = new GrupoImagemHomeTags()
                    {
                        GrupoImagemHome = grupoComNome,
                        Tags = tag
                    };

                    await _repository.Save(grupoImagemHomeTags);
                }
            }
        }

        public async Task ReorderGroups(List<ReorderImageGroupModel> groups)
        {
            _repository.BeginTransaction();
            foreach (var group in groups)
            {
                var grupoImagemHome = await _repository.FindById<GrupoImagemHome>(group.Id);
                if (grupoImagemHome != null)
                {
                    grupoImagemHome.Ordem = group.Ordem;
                    await _repository.Save(grupoImagemHome);
                }
            }
            await _repository.CommitAsync();
        }
    }
}

