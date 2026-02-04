using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Functions;
using System.ComponentModel;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class DocumentGroupService : IDocumentGroupService
    {
        // DTO interno para projeção leve de DocumentoTags + Tags via SQL
        private class DocumentoTagRow
        {
            public int Id { get; set; }
            public int DocumentoId { get; set; }
            public int TagsId { get; set; }
            public string? TagsNome { get; set; }
            public string? TagsPath { get; set; }
            public int? TagsParentId { get; set; }
        }

        private readonly IRepositoryNH _repository;
        private readonly ILogger<DocumentGroupService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;
        private readonly ICommunicationProvider _communicationProvider;
        private readonly IConfiguration _configuration;
        public DocumentGroupService(IRepositoryNH repository,
            ILogger<DocumentGroupService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase,
            ICommunicationProvider communicationProvider,
            IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
            _communicationProvider = communicationProvider;
            _configuration = configuration;
        }

        public async Task<DeleteResultModel> DeleteDocumentGroup(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            var loggedUser = (await _repository.GetLoggedUser());

            try
            {

                var grupoDocumento = (await _repository.FindByHql<GrupoDocumento>($"From GrupoDocumento gd Left Join Fetch gd.Empresa emp Left Join Fetch emp.Pessoa pe Where gd.Id = {id} and gd.UsuarioRemocao is null and gd.DataHoraRemocao is null")).FirstOrDefault();
                if (grupoDocumento is null)
                {
                    throw new ArgumentException($"Não foi encontrado o grupo de documento com Id: {id}!");
                }


                _repository.BeginTransaction();
                var documentosVinculados = (await _repository.FindBySql<DocumentoModel>($"Select d.Id, d.Nome From Documento d Where d.DataHoraRemocao is null and d.UsuarioRemocao is null and d.GrupoDocumento = {grupoDocumento.Id}")).AsList();
                if (documentosVinculados != null && documentosVinculados.Any())
                {
                    throw new ArgumentException("Para deletar o grupo de documentos, antes delete os documentos vinculados a ele.");
                }

                grupoDocumento.DataHoraRemocao = DateTime.Now;
                grupoDocumento.UsuarioRemocao = Convert.ToInt32(loggedUser.Value.userId);

                await _repository.Save(grupoDocumento);

                var tagsRemover = (await _repository.FindByHql<GrupoDocumentoTags>($"From GrupoDocumentoTags dt Inner Join Fetch dt.GrupoDocumento d Where d.Id = {grupoDocumento.Id}")).AsList();
                foreach (var tag in tagsRemover)
                {
                    tag.UsuarioRemocao = grupoDocumento.UsuarioRemocao;
                    tag.DataHoraRemocao = grupoDocumento.DataHoraRemocao;
                    await _repository.Save(tag);

                }


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
                _logger.LogError(err, $"Não foi possível deletar o grupo de documento: {id}");
                throw;
            }

        }

        public async Task<GrupoDocumentoModel> SaveDocumentGroup(DocumentGroupInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                var empresa = (await _repository.FindByHql<Empresa>("From Empresa emp Inner Join Fetch emp.Pessoa p")).AsList();
                if (empresa.Count > 1 || empresa.Count == 0)
                    throw new ArgumentException($"Não foi possível salvar o grupo de documento - Emp count:{empresa.Count}");

                GrupoDocumento grupoDocumentoOriginal = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                    grupoDocumentoOriginal = (await _repository.FindByHql<GrupoDocumento>($"From GrupoDocumento gd Where gd.UsuarioRemocao is null and gd.DataHoraRemocao is null and gd.Id = {model.Id}")).FirstOrDefault();

                var grupoDocumento = grupoDocumentoOriginal != null ? _mapper.Map(model, grupoDocumentoOriginal) : _mapper.Map(model, new GrupoDocumento());
                grupoDocumento.Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresa.First().Id };

                // Se for uma inclusão e não tiver ordem definida, definir ordem padrão
                if (grupoDocumentoOriginal == null && (model.Ordem == null || model.Ordem == 0))
                {
                    var maxOrdem = (await _repository.FindBySql<int?>("Select Max(Ordem) From GrupoDocumento Where UsuarioRemocao is null and DataHoraRemocao is null")).FirstOrDefault();
                    grupoDocumento.Ordem = (maxOrdem ?? 0) + 1;
                }

                var result = await _repository.Save(grupoDocumento);
                if (model.TagsRequeridas != null)
                    await SincronizarTagsRequeridas(grupoDocumento, model.TagsRequeridas ?? new List<int>(), model.RemoverTagsNaoEnviadas.GetValueOrDefault(false));

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Grupo de documento: ({result.Id} - {grupoDocumento.Nome}) salvo com sucesso!");

                    if (result != null)
                    {
                        return (await Search(new SearchGrupoDocumentoModel() { Id = result.Id, RetornarDocumentosDoGrupo = false })).FirstOrDefault();
                    }

                }
                throw exception ?? new Exception($"Não foi possível salvar o Grupo de Documento: ({grupoDocumento.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Grupo de documento: ({model.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        private async Task SincronizarTagsRequeridas(GrupoDocumento grupoDocumento, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                if (listTags == null || listTags.Count == 0)
                {
                    await _repository.ExecuteSqlCommand($"Delete From GrupoDocumentoTags Where UsuarioRemocao is null and DataHoraRemocao is null and GrupoDocumento = {grupoDocumento.Id}");
                    return;
                }
                else
                {
                    await _repository.ExecuteSqlCommand($"Delete From GrupoDocumentoTags Where UsuarioRemocao is null and DataHoraRemocao is null and GrupoDocumento = {grupoDocumento.Id} and  tags not in ({string.Join(",", listTags)})");
                }
            }

            if (listTags.Any())
            {

                var allTags = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)})")).AsList();
                var tagsInexistentes = listTags.Where(c => !allTags.Any(b => b.Id == c)).AsList();
                if (tagsInexistentes.Count > 0)
                {
                    throw new ArgumentException($"Tags não encontradas: {string.Join(",", tagsInexistentes)}");
                }

                var tags = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)}) and Not Exists(Select dc.Tags From GrupoDocumentoTags dc Where dc.UsuarioRemocao is null and dc.DataHoraRemocao is null and dc.GrupoDocumento = {grupoDocumento.Id} and dc.Tags = t.Id)")).AsList();


                foreach (var t in tags)
                {
                    var grupoDocumentoTag = new GrupoDocumentoTags()
                    {
                        GrupoDocumento = grupoDocumento,
                        Tags = new Tags() { Id = t.Id.GetValueOrDefault(0) }
                    };

                    await _repository.Save(grupoDocumentoTag);
                }
            }

        }

        public async Task<IEnumerable<GrupoDocumentoModel>?> Search(SearchGrupoDocumentoModel searchModel)
        {
            var loggedUser = await _repository.GetLoggedUser();
            if (!loggedUser.HasValue)
                throw new ArgumentException("Não foi possível retornar os grupos de documentos");
            
            if (!loggedUser.Value.isAdm)
            {
                if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentNullException("Não foi possível identificar o usuário para comunicação com o eSolution!");

                if (string.IsNullOrEmpty(loggedUser.Value.userId) || !Helper.IsNumeric(loggedUser.Value.userId))
                    throw new ArgumentNullException("Não foi possível identificar o id do usuário logado.");

                var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
                if (pessoaProvider == null || string.IsNullOrEmpty(pessoaProvider.PessoaProvider))
                    throw new ArgumentNullException($"Não foi possível encontrar a pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada a pessoa: {loggedUser.Value.providerKeyUser}");

            }

            List<SW_Utils.Auxiliar.Parameter> parameters = new();
            List<SW_Utils.Auxiliar.Parameter> parameters1 = new();
            StringBuilder sb = new("From GrupoDocumento gd Inner Join Fetch gd.Empresa emp Inner Join Fetch emp.Pessoa ep Where 1 = 1 and gd.UsuarioRemocao is null and gd.DataHoraRemocao is null ");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(gd.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and gd.Id = :id");
                parameters.Add(new SW_Utils.Auxiliar.Parameter("id", searchModel.Id.GetValueOrDefault()));
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
                sb.AppendLine(" and Coalesce(gd.Disponivel,0) = 1 ");
                sb.AppendLine(@$" and (not exists(Select gdt.GrupoDocumento From GrupoDocumentoTags gdt Where gdt.GrupoDocumento = gd.Id and gdt.UsuarioRemocao is null and gdt.DataHoraRemocao is null) 
                                      or Exists(Select gdt.GrupoDocumento From GrupoDocumentoTags gdt Inner Join UsuarioTags ut on gdt.Tags = ut.Tags Where gdt.UsuarioRemocao is null and gdt.DataHoraRemocao is null and gdt.GrupoDocumento = gd.Id and ut.Usuario = {loggedUser.Value.userId}))");
            }

            sb.AppendLine(" Order By Coalesce(gd.Ordem, 999999), gd.Id ");
            var grupoDocumentos = await _repository.FindByHql<GrupoDocumento>(sb.ToString(), session: null, parameters.ToArray());


            var itensRetorno = grupoDocumentos.Select(a => _mapper.Map(a, new GrupoDocumentoModel())).ToList();
            if (itensRetorno.Any())
            {

                if (searchModel.RetornarDocumentosDoGrupo.GetValueOrDefault(false))
                {
                    var grupoIds = grupoDocumentos.Select(a => a.Id).AsList();
                    if (grupoIds.Any())
                    {
                        List<SW_Utils.Auxiliar.Parameter> parametersDoc = new();

                        // Consulta projetando apenas os campos necessários (sem campo Arquivo)
                        var sbDocumentos = new StringBuilder();
                        sbDocumentos.AppendLine("Select ");
                        sbDocumentos.AppendLine("  d.Id, ");
                        sbDocumentos.AppendLine("  d.GrupoDocumento as GrupoDocumentoId, ");
                        sbDocumentos.AppendLine("  d.Nome, ");
                        sbDocumentos.AppendLine("  d.NomeArquivo, ");
                        sbDocumentos.AppendLine("  d.TipoMime, ");
                        sbDocumentos.AppendLine("  d.Disponivel, ");
                        sbDocumentos.AppendLine("  d.Ordem, ");
                        sbDocumentos.AppendLine("  d.DataInicioVigencia, ");
                        sbDocumentos.AppendLine("  d.DataFimVigencia ");
                        sbDocumentos.AppendLine("From Documento d ");
                        sbDocumentos.AppendLine("Inner Join GrupoDocumento gd on d.GrupoDocumento = gd.Id ");
                        sbDocumentos.AppendLine("Where ");
                        sbDocumentos.AppendLine("  d.UsuarioRemocao is null ");
                        sbDocumentos.AppendLine("  and d.DataHoraRemocao is null ");
                        sbDocumentos.AppendLine("  and gd.UsuarioRemocao is null ");
                        sbDocumentos.AppendLine("  and gd.DataHoraRemocao is null ");
                        sbDocumentos.AppendLine($"  and gd.Id in ({string.Join(",", grupoIds)}) ");

                        if (!loggedUser.Value.isAdm)
                        {
                            var dataAtual = DateTime.Now;
                            sbDocumentos.AppendLine("  and Coalesce(d.Disponivel,0) = 1 ");
                            // Filtro de vigência: documentos dentro do período ou sem período definido
                            sbDocumentos.AppendLine("  and ((d.DataInicioVigencia is null or d.DataInicioVigencia <= :dataAtual) ");
                            sbDocumentos.AppendLine("       and (d.DataFimVigencia is null or d.DataFimVigencia >= :dataAtual)) ");
                            parametersDoc.Add(new SW_Utils.Auxiliar.Parameter("dataAtual", dataAtual));
                            sbDocumentos.AppendLine(@$"  and (not exists(Select dt.Documento From DocumentoTags dt Where dt.Documento = d.Id and dt.UsuarioRemocao is null and dt.DataHoraRemocao is null) 
                          or Exists(Select dt.Documento From DocumentoTags dt Inner Join UsuarioTags ut on dt.Tags = ut.Tags Where dt.UsuarioRemocao is null and dt.DataHoraRemocao is null and dt.Documento = d.Id and ut.Usuario = {loggedUser.Value.userId}))");
                        }

                        sbDocumentos.AppendLine("Order By Coalesce(d.Ordem, 999999), d.Id ");

                        // Projeção direta para DocumentoModelSimplificado (sem Arquivo)
                        var documentosDosGrupos = await _repository.FindBySql<DocumentoModelSimplificado>(sbDocumentos.ToString(), session: null, parametersDoc.ToArray());
                        var itensRetornoDocumentos = documentosDosGrupos.AsList();

                        List<DocumentoTagRow> tagsDosDocumentos = new List<DocumentoTagRow>();

                        if (itensRetornoDocumentos.Any())
                        {
                            var docIds = documentosDosGrupos.Select(b => b.Id).AsList();

                            var sqlTags = $@"
                                Select
                                    dt.Id,
                                    dt.Documento as DocumentoId,
                                    t.Id        as TagsId,
                                    t.Nome      as TagsNome,
                                    t.Path      as TagsPath,
                                    t.Parent    as TagsParentId
                                From DocumentoTags dt
                                Inner Join Documento d on dt.Documento = d.Id
                                Inner Join Tags t      on dt.Tags     = t.Id
                                Where
                                    dt.UsuarioRemocao is null and dt.DataHoraRemocao is null and
                                    d.UsuarioRemocao  is null and d.DataHoraRemocao  is null and
                                    d.Id in ({string.Join(",", docIds)})
                            ";

                            tagsDosDocumentos = (await _repository.FindBySql<DocumentoTagRow>(sqlTags)).AsList();
                        }

                        foreach (var itemGroup in itensRetorno)
                        {
                            var documentosDoGrupo = itensRetornoDocumentos.Where(c => c.GrupoDocumentoId == itemGroup.Id).AsList();

                            foreach (var itemDocumento in documentosDoGrupo)
                            {
                                // Complementa apenas as tags do documento, sem carregar a entidade Documento completa
                                var tagsDoDocumento = tagsDosDocumentos
                                    .Where(a => a.DocumentoId == itemDocumento.Id)
                                    .AsList();

                                if (tagsDoDocumento.Any())
                                {
                                    itemDocumento.TagsRequeridas = tagsDoDocumento.Select(b => new DocumentoTagsModel()
                                    {
                                        DocumentoId = itemDocumento.Id,
                                        Id = b.Id,
                                        Tags = new TagsModel
                                        {
                                            Id       = b.TagsId,
                                            Nome     = b.TagsNome,
                                            Path     = b.TagsPath,
                                            ParentId = b.TagsParentId
                                        }
                                    }).AsList();
                                }
                            }

                            itemGroup.Documentos = documentosDoGrupo;
                        }
                    }
                }


                var tagsDoGrupo = (await _repository.FindByHql<GrupoDocumentoTags>($"From GrupoDocumentoTags gdt Inner Join Fetch gdt.GrupoDocumento gd Inner Join Fetch gdt.Tags t Where gdt.UsuarioRemocao is null and gdt.DataHoraRemocao is null and gd.DataHoraRemocao is null and gd.UsuarioRemocao is null and  gd.Id in ({string.Join(",", grupoDocumentos.Select(a => a.Id).AsList())})")).AsList();
                foreach (var item in itensRetorno)
                {
                    var tagsRelacionadas = tagsDoGrupo.Where(b => b.GrupoDocumento != null && b.GrupoDocumento.Id == item.Id).AsList();
                    if (tagsRelacionadas.Any())
                    {
                        item.TagsRequeridas = tagsRelacionadas.Select(b => new GrupoDocumentoTagsModel()
                        {
                            GrupoDocumentoId = b.GrupoDocumento.Id,
                            Id = b.Id,
                            Tags = _mapper.Map(b.Tags, new TagsModel())
                        }).AsList();
                    }
                }
            }

            return await _serviceBase.SetUserName(itensRetorno);
        }

        public async Task<bool> ReorderGroups(List<ReorderDocumentGroupModel> groups)
        {
            try
            {
                _repository.BeginTransaction();

                foreach (var group in groups)
                {
                    var grupoDocumento = await _repository.FindById<GrupoDocumento>(group.Id);
                    if (grupoDocumento != null && grupoDocumento.UsuarioRemocao == null && grupoDocumento.DataHoraRemocao == null)
                    {
                        grupoDocumento.Ordem = group.Ordem;
                        await _repository.Save(grupoDocumento);
                    }
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Ordem dos grupos de documentos atualizada com sucesso!");
                    return true;
                }
                throw exception ?? new Exception("Não foi possível atualizar a ordem dos grupos de documentos");
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao atualizar ordem dos grupos de documentos");
                _repository.Rollback();
                throw;
            }
        }

    }
}
