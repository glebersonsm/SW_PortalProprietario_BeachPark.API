using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Functions;
using SW_Utils.Auxiliar;
using System.Text;
using ZXing.OneD;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class ImageGroupService : IImageGroupService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<ImageGroupService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICommunicationProvider _communicationProvider;
        private readonly IConfiguration _configuration;

        public ImageGroupService(IRepositoryNH repository,
            ILogger<ImageGroupService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase,
            IHttpContextAccessor httpContextAccessor,
            ICommunicationProvider communicationProvider,
            IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
            _contextAccessor = httpContextAccessor;
            _communicationProvider = communicationProvider;
            _configuration = configuration;
        }

        public async Task<ImageGroupModel?> SaveImageGroup(ImageGroupInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                GrupoImagem grupoImagemOriginal = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                    grupoImagemOriginal = (await _repository.FindByHql<GrupoImagem>($"From GrupoImagem gd Where gd.Id = {model.Id}")).FirstOrDefault();

                var loggedUser = await _repository.GetLoggedUser();

                var grupoImagem = grupoImagemOriginal != null ? _mapper.Map(model, grupoImagemOriginal) : _mapper.Map<GrupoImagem>(model);
                var company = (await _repository.FindBySql<EmpresaModel>("Select e.Id From Empresa e Order by e.Id")).FirstOrDefault();
                if (company == null)
                {
                    throw new ArgumentException("Não foi possível identificar a empresa padrão do sistema");
                }

                grupoImagem.Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = company!.Id.GetValueOrDefault() };

                // Definir Ordem padrão se for um novo grupo e Ordem não foi informada
                if (grupoImagemOriginal == null && (model.Ordem == null || model.Ordem == 0))
                {
                    var maxOrdem = await _repository.CountTotalEntry("Select Max(gd.Ordem) as Ordem From GrupoImagem gd", session: null, parameters: Array.Empty<Parameter>());
                    grupoImagem.Ordem = Convert.ToInt32(maxOrdem) + 1;
                }

                var result = await _repository.Save(grupoImagem);
                await SincronizarTagsRequeridas(grupoImagem, model.TagsRequeridas, true);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Grupo de imagem: ({result.Id} - {grupoImagem.Nome}) salvo com sucesso!");

                    if (result != null)
                    {
                        var itemRetornar = (await Search(new SearchImageGroupModel() { Id = result.Id, RetornarImagens = false })).AsList();
                        if (itemRetornar != null && itemRetornar.Any())
                            return itemRetornar.First();
                    }

                }
                throw exception ?? new Exception($"Não foi possível salvar o Grupo de imagem: ({grupoImagem.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Grupo de imagem: ({model.Name})");
                _repository.Rollback();
                throw;
            }
        }

        private async Task SincronizarTagsRequeridas(GrupoImagem grupoImagem, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                // Buscar tags existentes para fazer soft delete (gerar log de auditoria)
                var tagsParaRemover = new List<GrupoImagemTags>();

                if (listTags == null || listTags.Count == 0)
                {
                    // Remover todas as tags - usar Inner Join Fetch para carregar Tags, GrupoImagem e Empresa com nomes
                    tagsParaRemover = (await _repository.FindByHql<GrupoImagemTags>(
                        @$"From GrupoImagemTags gita 
                                Inner Join Fetch gita.Tags t 
                                Inner Join Fetch gita.GrupoImagem g
                                Inner Join Fetch g.Empresa e
                           Where 
                                g.Id = {grupoImagem.Id} and 
                                gita.UsuarioRemocao is null and 
                                gita.DataHoraRemocao is null")).ToList();
                }
                else
                {
                    // Remover apenas tags que não estão na lista - usar Inner Join Fetch para carregar Tags, GrupoImagem e Empresa com nomes
                    tagsParaRemover = (await _repository.FindByHql<GrupoImagemTags>(
                        @$"From GrupoImagemTags gita 
                                Inner Join Fetch gita.Tags t 
                                Inner Join Fetch gita.GrupoImagem g
                                Inner Join Fetch g.Empresa e
                           Where 
                                g.Id = {grupoImagem.Id} and 
                                t.Id not in ({string.Join(",", listTags)}) and 
                                gita.UsuarioRemocao is null and 
                                gita.DataHoraRemocao is null")).ToList();
                }

                var usuario = await _repository.GetLoggedUser();
                foreach (var tagParaRemover in tagsParaRemover)
                {
                    // Log detalhado da remoção da tag
                    _logger.LogInformation($"[REMOÇÃO TAG] Removendo vínculo de tag do grupo da galeria | " +
                                          $"GrupoImagemID: {tagParaRemover.GrupoImagem?.Id} | " +
                                          $"GrupoImagemNome: {tagParaRemover.GrupoImagem?.Nome} | " +
                                          $"TagID: {tagParaRemover.Tags?.Id} | " +
                                          $"TagNome: {tagParaRemover.Tags?.Nome} | " +
                                          $"UsuarioID: {usuario?.userId ?? "Sistema"} | " +
                                          $"DataHora: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                                          $"EmpresaID: {tagParaRemover.GrupoImagem?.Empresa?.Id} | " +
                                          $"TipoRemocao: Sincronização");

                    _repository.Remove(tagParaRemover);
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
                var tags = (await _repository.FindByHql<Tags>(
                    @$"From Tags t Where t.Id in ({string.Join(",", listTags)}) and 
                    Not Exists(Select dc.Tags From GrupoImagemTags dc Where dc.GrupoImagem = {grupoImagem.Id} and dc.Tags = t.Id and dc.UsuarioRemocao is null and dc.DataHoraRemocao is null)")).ToList();

                // Buscar grupo com nome carregado usando HQL para garantir que o nome esteja disponível
                var grupoComNome = (await _repository.FindByHql<GrupoImagem>(
                    $"From GrupoImagem g Where g.Id = {grupoImagem.Id}")).FirstOrDefault() ?? grupoImagem;

                foreach (var tag in tags)
                {
                    var grupoImagemTags = new GrupoImagemTags()
                    {
                        GrupoImagem = grupoComNome,
                        Tags = tag
                    };

                    await _repository.Save(grupoImagemTags);
                }
            }
        }

        public async Task<DeleteResultModel> DeleteImageGroup(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var grupoImagem = await _repository.FindById<GrupoImagem>(id);
                if (grupoImagem is null)
                {
                    throw new ArgumentException($"Não foi encontrado o grupo de imagem com Id: {id}!");
                }

                _repository.BeginTransaction();

                // Fazer soft delete das tags relacionadas
                var tagsRelacionadas = await _repository.FindByHql<GrupoImagemTags>(
                    @$"From GrupoImagemTags gita 
                            Inner Join Fetch gita.Tags t 
                            Inner Join Fetch gita.GrupoImagem g
                            Inner Join Fetch g.Empresa e
                       Where 
                            g.Id = {id} and 
                            gita.UsuarioRemocao is null and 
                            gita.DataHoraRemocao is null");

                var usuario = await _repository.GetLoggedUser();
                foreach (var tagRelacionada in tagsRelacionadas)
                {
                    // Log detalhado da remoção da tag durante exclusão do grupo
                    _logger.LogInformation($"[REMOÇÃO TAG - EXCLUSÃO GRUPO] Removendo vínculo de tag por exclusão do grupo da galeria | " +
                                          $"GrupoImagemID: {tagRelacionada.GrupoImagem?.Id} | " +
                                          $"GrupoImagemNome: {tagRelacionada.GrupoImagem?.Nome} | " +
                                          $"TagID: {tagRelacionada.Tags?.Id} | " +
                                          $"TagNome: {tagRelacionada.Tags?.Nome} | " +
                                          $"UsuarioID: {usuario?.userId ?? "Sistema"} | " +
                                          $"DataHora: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                                          $"EmpresaID: {tagRelacionada.GrupoImagem?.Empresa?.Id} | " +
                                          $"TipoRemocao: Exclusão do grupo");

                    _repository.Remove(tagRelacionada);
                }

                _repository.Remove(grupoImagem);

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
                _logger.LogError(err, $"Não foi possível deletar o grupo de imagem: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<ImageGroupModel>?> Search(SearchImageGroupModel searchModel)
        {
            var httpContext = _contextAccessor?.HttpContext?.Request;

            if (httpContext == null)
                throw new Exception("Não foi possível identifica a URL do servidor!");

            var complemento = _configuration.GetValue<string>("ComplementoUrlApi", string.Empty);
            var complementoUrlApiComplementoParaHttps = _configuration.GetValue<string>("ComplementoUrlApiComplementoParaHttps", string.Empty);

            var loggedUser = await _repository.GetLoggedUser();
            if (!loggedUser.HasValue) throw new ArgumentException("Não foi possível carregar os grupos de imagem da geleria.");

            if (!loggedUser.Value.isAdm)
            {
                if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentNullException("Não foi possível identificar o usuário para comunicação com o eSolution!");

                if (string.IsNullOrEmpty(loggedUser.Value.userId) || !Helper.IsNumeric(loggedUser.Value.userId))
                    throw new ArgumentNullException("Não foi possível identificar o id do usuário logado.");

                var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
                if (pessoaProvider == null || !pessoaProvider.Any() || pessoaProvider.Any(a => string.IsNullOrEmpty(a.PessoaProvider)))
                    throw new ArgumentNullException($"Não foi possível encontrar a pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada a pessoa: {loggedUser.Value.providerKeyUser}");

            }

            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From GrupoImagem gd Inner Join Fetch gd.Empresa emp Where 1 = 1");
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

            if (!loggedUser.Value.isAdm)
            {
                sb.AppendLine(@$" and (
                    not exists(Select gdt.GrupoImagem 
                               From GrupoImagemTags gdt 
                               Where gdt.GrupoImagem = gd.Id and gdt.UsuarioRemocao is null and gdt.DataHoraRemocao is null) 
                    
                    or
                    
                    Exists(Select git.GrupoImagem 
                           From GrupoImagemTags git 
                           Inner Join UsuarioTags ut on git.Tags = ut.Tags 
                           Where git.UsuarioRemocao is null and git.DataHoraRemocao is null and git.GrupoImagem = gd.Id and ut.Usuario = {loggedUser.Value.userId})
                    
                    or
                    
                    Exists(Select igi.GrupoImagem 
                           From ImagemGrupoImagem igi 
                           Inner Join ImagemGrupoImagemTags igita on igita.ImagemGrupoImagem = igi.Id
                           Inner Join UsuarioTags ut on igita.Tags = ut.Tags 
                           Where igita.UsuarioRemocao is null and igita.DataHoraRemocao is null and igi.GrupoImagem = gd.Id and ut.Usuario = {loggedUser.Value.userId})
                    
                    or
                    
                    Exists(Select igi.Id
                           From ImagemGrupoImagem igi 
                           Where igi.GrupoImagem = gd.Id 
                           and not exists(Select igita.ImagemGrupoImagem 
                                          From ImagemGrupoImagemTags igita 
                                          Where igita.ImagemGrupoImagem = igi.Id and igita.UsuarioRemocao is null and igita.DataHoraRemocao is null))
                )");
            }

            sb.AppendLine(" Order by gd.Ordem, gd.Nome ");

            var gruposImagens = await _repository.FindByHql<GrupoImagem>(sb.ToString(), session: null, parameters.ToArray());


            var itensRetorno = gruposImagens.Select(a => _mapper.Map<ImageGroupModel>(a)).ToList();
            if (itensRetorno.Any())
            {

                var grupoImagemTags = (await _repository.FindByHql<GrupoImagemTags>(@$"From
                    GrupoImagemTags gita 
                    Inner Join Fetch gita.GrupoImagem gi 
                    Inner Join Fetch gita.Tags t 
                    Where 
                    gi.Id in ({string.Join(",", itensRetorno.Select(b => b.Id))}) 
                    and gita.UsuarioRemocao is null and gita.DataHoraRemocao is null")).AsList();

                foreach (var itemGrupo in itensRetorno)
                {
                    var tagsDoGrupo = grupoImagemTags.Where(a => a.GrupoImagem != null && a.GrupoImagem.Id == itemGrupo.Id);
                    if (tagsDoGrupo.Any())
                        itemGrupo.TagsRequeridas = tagsDoGrupo.Select(b => _mapper.Map<GrupoImagemTagsModel>(b)).AsList();
                }

                if (searchModel.RetornarImagens.GetValueOrDefault(false))
                {
                    var grupoIds = gruposImagens.Select(a => a.Id).AsList();
                    if (grupoIds.Any())
                    {
                        var imagensDosGruposImagens = await _repository.FindByHql<ImagemGrupoImagem>(
                            $"From ImagemGrupoImagem i Inner Join Fetch i.GrupoImagem g Where g.Id in ({string.Join(",", grupoIds)}) Order by i.Ordem, i.Nome");

                        var itensRetornoImagens = imagensDosGruposImagens.Select(a => _mapper.Map<ImageGroupImageModel>(a)).ToList();

                        List<ImagemGrupoImagemTags> tagsDasImagens = new List<ImagemGrupoImagemTags>();

                        if (itensRetornoImagens.Any())
                        {
                            tagsDasImagens = (await _repository.FindByHql<ImagemGrupoImagemTags>(@$"From
                                ImagemGrupoImagemTags igita 
                                Inner Join Fetch igita.ImagemGrupoImagem igi 
                                Inner Join Fetch igita.Tags t 
                                Where 
                                igi.Id in ({string.Join(",", imagensDosGruposImagens.Select(b => b.Id).AsList())}) 
                                and igita.UsuarioRemocao is null and igita.DataHoraRemocao is null")).AsList();
                        }

                        var tagsDoUsuario = !loggedUser.Value.isAdm ? (await _repository.FindBySql<TagsModel>($"Select ut.Tags AS Id From UsuarioTags ut Where ut.Usuario = {loggedUser.Value.userId}")).AsList() : new();

                        foreach (var itemGroup in itensRetorno)
                        {
                            var imagensDoGrupo = itensRetornoImagens.Where(c => c.ImageGroupId == itemGroup.Id).AsList();

                            foreach (var imagem in imagensDoGrupo)
                            {
                                var tagsDaImagemAtual = tagsDasImagens.Where(a => a != null && a.ImagemGrupoImagem != null && a.ImagemGrupoImagem.Id == imagem.Id).AsList();
                                if (tagsDaImagemAtual.Any())
                                    imagem.TagsRequeridas = tagsDaImagemAtual.Select(b => _mapper.Map<ImagemGrupoImagemTagsModel>(b)).AsList();
                            }

                            if (!loggedUser.Value.isAdm)
                            {
                                for (int i = imagensDoGrupo.Count - (1); i >= 0; i--)
                                {
                                    var itemImagem = imagensDoGrupo[i];
                                    if (itemImagem != null)
                                    {
                                        if (itemImagem.TagsRequeridas != null && itemImagem.TagsRequeridas.Any())
                                        {
                                            if (tagsDoUsuario == null || !tagsDoUsuario.Any())
                                            {
                                                imagensDoGrupo.Remove(itemImagem);
                                            }
                                            else if (!itemImagem.TagsRequeridas.Any(c => tagsDoUsuario!.Any(b => b.Id == c.Tags.Id)))
                                            {
                                                imagensDoGrupo.Remove(itemImagem);
                                            }
                                        }

                                        if (itemImagem.DataInicioVigencia.HasValue && itemImagem.DataInicioVigencia.Value.Date > DateTime.Today.Date)
                                        {
                                            imagensDoGrupo.Remove(itemImagem);
                                        }
                                        else if (itemImagem.DataFimVigencia.HasValue && itemImagem.DataFimVigencia.Value.Date < DateTime.Today.Date)
                                        {
                                            imagensDoGrupo.Remove(itemImagem);
                                        }
                                    }
                                }

                            }


                            itemGroup.Images = imagensDoGrupo;
                        }

                    }
                }
            }

            return await _serviceBase.SetUserName(itensRetorno);
        }

        public async Task ReorderGroups(List<ReorderImageGroupModel> groups)
        {
            _repository.BeginTransaction();
            foreach (var group in groups)
            {
                var grupoImagem = await _repository.FindById<GrupoImagem>(group.Id);
                if (grupoImagem != null)
                {
                    grupoImagem.Ordem = group.Ordem;
                    await _repository.Save(grupoImagem);
                }
            }
            await _repository.CommitAsync();
        }
    }
}
