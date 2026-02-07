using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Functions;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class FaqGroupService : IFaqGroupService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<FaqGroupService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;
        private readonly ICommunicationProvider _communicationProvider;
        private readonly IConfiguration _configuration;
        public FaqGroupService(IRepositoryNH repository,
            ILogger<FaqGroupService> logger,
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

        public async Task<DeleteResultModel> DeleteGroup(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var grupoFaq = await _repository.FindById<GrupoFaq>(id);
                if (grupoFaq is null)
                {
                    throw new ArgumentException($"Não foi encontrado o Grupo de FAQ com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.ExecuteSqlCommand($"Delete From GrupoFaqTags Where GrupoFaq = {id}");
                await _repository.ExecuteSqlCommand($"Delete From GrupoFaqTags Where GrupoFaq in (Select gf.Id From GrupoFaq gf Where gf.IdGrupoFaqPai is not null and gf.IdGrupoFaqPai = {id})");
                await _repository.ExecuteSqlCommand($"Delete From FaqTags Where Faq in (Select f.Id From Faq f Where f.GrupoFaq = {id})");
                await _repository.ExecuteSqlCommand($"Delete From Faq Where GrupoFaq is not null and GrupoFaq = {id}");
                await _repository.ExecuteSqlCommand($"Delete From GrupoFaq Where IdGrupoFaqPai is not null and IdGrupoFaqPai = {id}");
                _repository.Remove(grupoFaq);

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


        public async Task<GrupoFaqModel> SaveGroup(FaqGroupInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                GrupoFaq grupoFaqOriginal = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                    grupoFaqOriginal = (await _repository.FindByHql<GrupoFaq>($"From GrupoFaq gf Left Join Fetch gf.GrupoFaqPai gp Where gf.Id = {model.Id}")).FirstOrDefault();

                var emp = (await _repository.FindBySql<EmpresaModel>($"Select e.Id From Empresa e Order by e.Id")).FirstOrDefault();
                if (emp == null)
                    throw new ArgumentException($"Não foi possível identificar a empresa proprietária");

                var grupoFaq = grupoFaqOriginal != null ? _mapper.Map(model, grupoFaqOriginal) : _mapper.Map(model, new GrupoFaq());
                grupoFaq.Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = emp.Id.GetValueOrDefault() };

                // Definir grupo pai (subgrupo)
                if (model.GrupoFaqPaiId.GetValueOrDefault(0) > 0)
                {
                    grupoFaq.GrupoFaqPai = await _repository.FindById<GrupoFaq>(model.GrupoFaqPaiId.Value);
                }
                else
                {
                    grupoFaq.GrupoFaqPai = null;
                }

                // Se for uma inclusão e não tiver ordem definida, definir ordem padrão
                if (grupoFaqOriginal == null && (model.Ordem == null || model.Ordem == 0))
                {
                    var maxOrdem = (await _repository.FindBySql<int?>("Select Max(Ordem) From GrupoFaq")).FirstOrDefault();
                    grupoFaq.Ordem = (maxOrdem ?? 0) + 1;
                }

                var result = await _repository.Save(grupoFaq);
                await SincronizarTagsRequeridas(grupoFaq, model.TagsRequeridas ?? new List<int>(), true);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Grupo de FAQ: ({result.Id} - {grupoFaq.Nome}) salvo com sucesso!");
                    return new GrupoFaqModel() { Id = result.Id };
                }
                throw exception ?? new Exception($"Não foi possível salvar o Grupo de FAQ: ({grupoFaq.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o Grupo de FAQ: ({model.Nome})");
                _repository.Rollback();
                throw;
            }
        }

        private async Task SincronizarTagsRequeridas(GrupoFaq grupoFaq, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                if (listTags == null || listTags.Count == 0)
                {
                    await _repository.ExecuteSqlCommand($"Delete From GrupoFaqTags Where GrupoFaq = {grupoFaq.Id}");
                    return;
                }
                else
                {
                    await _repository.ExecuteSqlCommand($"Delete From GrupoFaqTags Where GrupoFaq = {grupoFaq.Id} and tags not in ({string.Join(",", listTags)})");
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

                var tags = (await _repository.FindBySql<TagsModel>(@$"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)}) and 
                Not Exists(Select dc.Tags From GrupoFaqTags dc Where dc.GrupoFaq = {grupoFaq.Id} and dc.Tags = t.Id)")).AsList();

                foreach (var t in tags)
                {
                    var grupoFaqTags = new GrupoFaqTags()
                    {
                        GrupoFaq = grupoFaq,
                        Tags = new Tags() { Id = t.Id.GetValueOrDefault(0) }
                    };

                    await _repository.Save(grupoFaqTags);
                }
            }

        }

        public async Task<IEnumerable<GrupoFaqModel>?> Search(SearchGrupoFaqModel searchModel)
        {
            var adm = _repository.IsAdm;

            if (!adm)
            {
                var loggedUser = await _repository.GetLoggedUser();
                if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentNullException("Não foi possível identificar o usuário para comunicação com o eSolution!");

                var userId = loggedUser.Value.userId;
                if (string.IsNullOrEmpty(userId) || !Helper.IsNumeric(userId))
                    throw new ArgumentNullException("Não foi possível identificar o id do usuário logado.");

                var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(userId), _communicationProvider.CommunicationProviderName);
                if (pessoaProvider == null || string.IsNullOrEmpty(pessoaProvider.PessoaProvider))
                    throw new ArgumentNullException($"Não foi possível encontrar a pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada a pessoa: {loggedUser.Value.providerKeyUser}");

            }

            List<Parameter> parameters = new();
            List<Parameter> parameters1 = new();
            StringBuilder sb = new("From GrupoFaq gf Inner Join Fetch gf.Empresa emp Left Join Fetch gf.GrupoFaqPai gfp Left Join Fetch gfp.GrupoFaqPai gfpp Left Join Fetch gfpp.GrupoFaqPai gfppp Where 1 = 1");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and gf.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (searchModel.IdGrupoFaqPai.HasValue)
            {
                if (searchModel.IdGrupoFaqPai.Value > 0)
                    sb.AppendLine($" and gf.GrupoFaqPai.Id = {searchModel.IdGrupoFaqPai.Value}");
                else
                    sb.AppendLine($" and gf.GrupoFaqPai.Id is null");
            }

            if (!string.IsNullOrEmpty(searchModel.TextoPergunta))
            {
                sb.AppendLine(" and Exists(Select gf1.Id From Faq f Inner Join f.GrupoFaq gf1 Where gf1.Id = gf.Id and " +
                    $" Lower(f.Pergunta) like '%{searchModel.TextoPergunta.ToLower()}%')");
            }

            if (!string.IsNullOrEmpty(searchModel.TextoResposta))
            {
                sb.AppendLine(" and Exists(Select gf1.Id From Faq f Inner Join f.GrupoFaq gf1 Where gf1.Id = gf.Id and " +
                    $" Lower(f.Resposta) like '%{searchModel.TextoResposta.ToLower()}%')");
            }

            sb.AppendLine(" Order By Coalesce(gf.Ordem, 999999), gf.Id");

            var grupoFaqs = await _repository.FindByHql<GrupoFaq>(sb.ToString(), null, parameters.ToArray());
            var itensRetorno = grupoFaqs.Select(a => MapWithParent(a)).AsList();

            var groupFaqsTags = (await _repository.FindByHql<GrupoFaqTags>(@$"From 
                                    GrupoFaqTags gft 
                                    Inner Join Fetch gft.GrupoFaq gf 
                                    Inner Join Fetch gft.Tags t 
                                Where 
                                    gf.Id in ({string.Join(",", grupoFaqs.Select(a => a.Id).AsList())})")).AsList();

            foreach (var item in itensRetorno)
            {
                var tagsRelacionadas = groupFaqsTags.Where(b => b.GrupoFaq != null && b.GrupoFaq.Id == item.Id).AsList();
                if (tagsRelacionadas.Any())
                {
                    item.TagsRequeridas = tagsRelacionadas.Select(b => new GrupoFaqTagsModel()
                    {
                        GrupoFaqId = b.GrupoFaq!.Id,
                        Id = b.Id,
                        Tags = _mapper.Map(b.Tags, new TagsModel())
                    }).AsList();
                }
            }

            if (searchModel.RetornarFaqs.GetValueOrDefault(false))
            {
                var sbFaq = new StringBuilder();
                sbFaq.AppendLine($"From Faq f Inner Join Fetch f.GrupoFaq gf Where gf.Id in ({string.Join(",", grupoFaqs.Select(a => a.Id).AsList())})");
                sbFaq.AppendLine(" Order By Coalesce(f.Ordem, 999999), f.Id");

                if (!string.IsNullOrEmpty(searchModel.TextoPergunta))
                {
                    sbFaq.AppendLine($" and Lower(f.Pergunta) like '%{searchModel.TextoPergunta.ToLower()}%' ");
                }

                if (!string.IsNullOrEmpty(searchModel.TextoResposta))
                {
                    sbFaq.AppendLine($" and Lower(f.Resposta) like '%{searchModel.TextoResposta.ToLower()}%' ");
                }

                var faqsDoGrupo = grupoFaqs.Any() ? (await _repository.FindByHql<Faq>(sbFaq.ToString())).AsList() :
                    new List<Faq>();

                var faqsTags = faqsDoGrupo != null && faqsDoGrupo.Any() ? (await _repository.FindByHql<FaqTags>(@$"From 
                                    FaqTags ft 
                                    Inner Join Fetch ft.Faq f
                                    Inner Join Fetch ft.Tags t 
                                Where 
                                     f.Id in ({string.Join(",", faqsDoGrupo.Select(a => a.Id).AsList())})")).AsList() : new List<FaqTags>();

                foreach (var faqGroup in itensRetorno)
                {
                    var faqsGrupoAtual = faqsDoGrupo != null && faqsDoGrupo.Any() ? faqsDoGrupo.Where(c => c.GrupoFaq != null && c.GrupoFaq.Id == faqGroup.Id).AsList() : new List<Faq>();
                    if (faqsGrupoAtual != null && faqsGrupoAtual.Any())
                    {
                        faqGroup.Faqs = faqsGrupoAtual.Select(b => _mapper.Map<FaqModelSimplificado>(b)).AsList();
                        foreach (var item in faqGroup.Faqs)
                        {
                            var tagsDaFaq = faqsTags.Where(c => c.Faq != null && c.Faq.Id == item.Id).AsList();
                            if (tagsDaFaq.Any())
                                item.TagsRequeridas = tagsDaFaq.Select(a => _mapper.Map<FaqTagsModel>(a)).AsList();
                        }
                    }
                   
                }
            }

            return await _serviceBase.SetUserName(itensRetorno);

        }

        private GrupoFaqModel MapWithParent(GrupoFaq entity)
        {
            var model = _mapper.Map(entity, new GrupoFaqModel());
            model.GrupoFaqPaiId = entity.GrupoFaqPai?.Id;
            if (entity.GrupoFaqPai != null)
            {
                model.Parent = MapWithParent(entity.GrupoFaqPai);
            }
            return model;
        }

        public async Task<bool> ReorderGroups(List<ReorderFaqGroupModel> groups)
        {
            try
            {
                _repository.BeginTransaction();

                foreach (var group in groups)
                {
                    var grupoFaq = await _repository.FindById<GrupoFaq>(group.Id);
                    if (grupoFaq != null)
                    {
                        grupoFaq.Ordem = group.Ordem;
                        await _repository.Save(grupoFaq);
                    }
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Ordem dos grupos de FAQ atualizada com sucesso!");
                    return true;
                }
                throw exception ?? new Exception("Não foi possível atualizar a ordem dos grupos de FAQ");
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao atualizar ordem dos grupos de FAQ");
                _repository.Rollback();
                throw;
            }
        }

    }
}
