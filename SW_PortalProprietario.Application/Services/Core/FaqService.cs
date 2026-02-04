using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Domain.Functions;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class FaqService : IFaqService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<FaqService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IServiceBase _serviceBase;
        private readonly ICommunicationProvider _communicationProvider;
        public FaqService(IRepositoryNH repository,
            ILogger<FaqService> logger,
            IProjectObjectMapper mapper,
            IEmailService emailService,
            IServiceBase serviceBase,
            ICommunicationProvider communicationProvider)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _emailService = emailService;
            _serviceBase = serviceBase;
            _communicationProvider = communicationProvider;
        }



        public async Task<DeleteResultModel> DeleteFaq(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var faq = await _repository.FindById<Faq>(id);
                if (faq is null)
                {
                    throw new ArgumentException($"Não foi encontrado o FAQ com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.ExecuteSqlCommand($"Delete From FaqTags Where Faq = {id}");
                _repository.Remove(faq);
                
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
                _logger.LogError(err, $"Não foi possível deletar o FAQ: {id}");
                throw;
            }
        }

        public async Task<FaqModel> SaveFaq(FaqInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                bool inclusao = model.Id.GetValueOrDefault(0) == 0;

                if (!_repository.IsAdm)
                {
                    if (!string.IsNullOrEmpty(model.Resposta))
                        model.Resposta = null;

                    if (inclusao)
                    {
                        model.Disponivel = EnumSimNao.Não;
                    }
                }

                Faq faq = null;
                if (model.Id.GetValueOrDefault(0) > 0)
                    faq = (await _repository.FindByHql<Faq>($"From Faq f Inner Join Fetch f.GrupoFaq gf Inner Join Fetch gf.Empresa emp Where f.Id = {model.Id}")).FirstOrDefault();

                var faqSalvar = faq != null ? _mapper.Map(model, faq) : _mapper.Map(model, new Faq());

                // Se for uma inclusão e não tiver ordem definida, definir ordem padrão dentro do grupo
                if (faq == null && (model.Ordem == null || model.Ordem == 0) && model.GrupoFaqId.HasValue)
                {
                    var maxOrdem = (await _repository.FindBySql<int?>($"Select Max(Ordem) From Faq Where GrupoFaq = {model.GrupoFaqId.Value}")).FirstOrDefault();
                    faqSalvar.Ordem = (maxOrdem ?? 0) + 1;
                }

                var result = await _repository.Save(faqSalvar);
                faq = result;
                await SincronizarTagsRequeridas(faq, model.TagsRequeridas ?? new List<int>(), true);

                #region Parte que envia email ao usuário que adicionou a pergunta e ou teve sua dúvida respondida (Retirada do escopo do projeto)
                //var grupoFaq = (await _repository.FindByHql<GrupoFaq>($"From GrupoFaq gf Inner Join Fetch gf.Empresa emp Where gf.Id = {result.GrupoFaq?.Id}")).FirstOrDefault();

                //if (grupoFaq != null)
                //{
                //    var usuarioCriadorFaq = (await _repository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {result.UsuarioCriacao.GetValueOrDefault()} and ((p.EmailPreferencial is not null and p.EmailPreferencial like '%@%') or (p.EmailAlternativo is not null and p.EmailAlternativo like '%@%'))")).FirstOrDefault();
                //    if (usuarioCriadorFaq != null)
                //    {
                //        var emailUtilizar = usuarioCriadorFaq.Pessoa?.EmailPreferencial ?? usuarioCriadorFaq.Pessoa?.EmailAlternativo;
                //        if (!string.IsNullOrEmpty(emailUtilizar))
                //        {

                //            if (inclusao)
                //            {
                //                if (grupoFaq.EnviarPerguntaAoCliente.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                //                {

                //                    if (grupoFaq.EnviarPerguntaAoCliente.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                //                    {

                //                        await _emailService.SaveInternal(new EmailInputInternalModel()
                //                        {
                //                            UsuarioCriacao = usuarioCriadorFaq.Id,
                //                            Assunto = @$"Sua pergunta/dúvida foi adicionada com sucesso no Portal do Proprietário",
                //                            Destinatario = emailUtilizar,
                //                            ConteudoEmail = @$"Olá, {usuarioCriadorFaq.Pessoa?.Nome}!
                //                            <br>
                //                            Você está recebendo esse email por ter adicionado uma pergunta no Portal do Proprietário!
                //                            <br>
                //                            Pergunta adicionada: 
                //                            <b> ({result.Pergunta})!</b>
                //                            <br>
                //                            Assim que sua pergunta for respondida, você receberá um novo email com a resposta.
                //                            <br>
                //                            Agradecemos o seu contato!"
                //                        });



                //                    }
                //                    else
                //                    {
                //                        await _emailService.SaveInternal(new EmailInputInternalModel()
                //                        {
                //                            UsuarioCriacao = usuarioCriadorFaq.Id,
                //                            Assunto = @$"Sua pergunta/dúvida foi adicionada com sucesso no Portal do Proprietário",
                //                            Destinatario = emailUtilizar,
                //                            ConteudoEmail = @$"Olá, {usuarioCriadorFaq.Pessoa?.Nome}!
                //                            <br>
                //                            Você está recebendo esse email por ter adicionado uma pergunta no Portal do Proprietário!
                //                            <br>
                //                            Pergunta adicionada: 
                //                            <b> ({result.Pergunta})!</b>
                //                            <br>
                //                            Agradecemos o seu contato!"
                //                        });

                //                    }

                //                    result.PerguntaEnviadaAoCliente = EnumSimNao.Sim;
                //                    await _repository.Save(result);
                //                }
                //            }
                //            else if (grupoFaq.EnviarRespostaAoCliente.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim && !string.IsNullOrEmpty(faq.Resposta)) 
                //            {
                //                await _emailService.SaveInternal(new EmailInputInternalModel()
                //                {
                //                    UsuarioCriacao = usuarioCriadorFaq.Id,
                //                    Assunto = @$"Sua pergunta/dúvida foi respondida pelo Portal do Proprietário",
                //                    Destinatario = emailUtilizar,
                //                    ConteudoEmail = @$"Olá, {usuarioCriadorFaq.Pessoa?.Nome}!
                //                            <br>
                //                            Você está recebendo esse email por ter adicionado uma pergunta no Portal do Proprietário e agora ela ter sido respondida!
                //                            <br>
                //                            Pergunta adicionada: 
                //                            <b> ({result.Pergunta})!</b>
                //                            <br>
                //                            Resposta: 
                //                            <b> ({result.Resposta})!</b>
                //                            <br>
                //                            Agradecemos o seu contato!"
                //                });

                //                result.RespostaEnviadaAoCliente = EnumSimNao.Sim;
                //                await _repository.Save(result);
                //            }

                //        }
                //    }
                //} 
                #endregion

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"FAQ: ({result.Id}) salvo com sucesso!");

                    if (result != null)
                        return _mapper.Map(result, new FaqModel());

                }
                throw exception ?? new Exception($"Não foi possível salvar a FAQ: ({faqSalvar.Pergunta})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a FAQ: ({model.Pergunta})");
                _repository.Rollback();
                throw;
            }
        }

        private async Task SincronizarTagsRequeridas(Faq faq, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                if (listTags == null || listTags.Count == 0)
                {
                    await _repository.ExecuteSqlCommand($"Delete From FaqTags Where Faq = {faq.Id}");
                    return;
                }
                else
                {
                    await _repository.ExecuteSqlCommand($"Delete From FaqTags Where Faq = {faq.Id} and tags not in ({string.Join(",", listTags)})");
                }
            }

            if (listTags != null && listTags.Any())
            {
                var allTags = (await _repository.FindBySql<Tags>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)})")).AsList();
                var tagsInexistentes = listTags.Where(c => !allTags.Any(b => b.Id == c)).AsList();
                if (tagsInexistentes.Count > 0)
                {
                    throw new ArgumentException($"Tags não encontradas: {string.Join(",", tagsInexistentes)}");
                }

                var tags = (await _repository.FindBySql<TagsModel>(@$"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)}) and 
                Not Exists(Select dc.Tags From FaqTags dc Where dc.Faq = {faq.Id} and dc.Tags = t.Id)")).AsList();

                foreach (var t in tags)
                {
                    var faqTags = new FaqTags()
                    {
                        Faq = faq,
                        Tags = new Tags() { Id = t.Id.GetValueOrDefault(0) }
                    };

                    await _repository.Save(faqTags);
                }
            }

        }

        public async Task<IEnumerable<FaqModelSimplificado>?> Search(SearchFaqModel searchModel)
        {
            var adm = _repository.IsAdm;

            bool userFrom = false;
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
            StringBuilder sb = new("From Faq f Inner Join Fetch f.GrupoFaq gf Inner Join Fetch gf.Empresa emp Where 1 = 1");
            sb.AppendLine(" Order By Coalesce(f.Ordem, 999999), f.Id");
            if (!string.IsNullOrEmpty(searchModel.TextoPergunta))
                sb.AppendLine($" and Lower(f.Pergunta) like '%{searchModel.TextoPergunta.ToLower()}%'");

            if (!string.IsNullOrEmpty(searchModel.TextoResposta))
                sb.AppendLine($" and Lower(f.Resposta) like '%{searchModel.TextoResposta.ToLower()}%'");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and f.Id = :id ");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (!adm)
            {
                sb.AppendLine(" and Coalesce(f.Disponivel,0) = 1 ");
            }

            var faqs = await _repository.FindByHql<Faq>(sb.ToString(), session: null, parameters.ToArray());

            List<FaqModelSimplificado> faqsRetorno = new();

            if (faqs.Any())
            {
                var faqsTags = (await _repository.FindByHql<FaqTags>(@$"From 
                                    FaqTags ft 
                                    Inner Join Fetch ft.Faq f
                                    Inner Join Fetch ft.Tags t 
                                Where 
                                     f.Id in ({string.Join(",", faqs.Select(a => a.Id).AsList())})")).AsList();

                foreach (var faq in faqs)
                {
                    var faqRetorno = _mapper.Map<FaqModelSimplificado>(faq);
                    var faqTagsAtual = faqsTags.Where(c => c.Faq != null && c.Faq.Id == faq.Id).AsList();
                    if (faqTagsAtual.Any())
                    {
                        faqRetorno.TagsRequeridas = faqTagsAtual.Select(b=> _mapper.Map<FaqTagsModel>(b)).ToList();
                    }

                    faqsRetorno.Add(faqRetorno);
                }
            }

            return faqsRetorno;
        }

        public async Task<bool> ReorderFaqs(List<ReorderFaqModel> faqs)
        {
            try
            {
                _repository.BeginTransaction();

                foreach (var faq in faqs)
                {
                    var faqEntity = await _repository.FindById<Faq>(faq.Id);
                    if (faqEntity != null)
                    {
                        faqEntity.Ordem = faq.Ordem;
                        await _repository.Save(faqEntity);
                    }
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Ordem das FAQs atualizada com sucesso!");
                    return true;
                }
                throw exception ?? new Exception("Não foi possível atualizar a ordem das FAQs");
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao atualizar ordem das FAQs");
                _repository.Rollback();
                throw;
            }
        }
    }
}
