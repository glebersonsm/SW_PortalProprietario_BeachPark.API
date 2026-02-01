using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Auxiliar;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class EmailHostedService : IEmailHostedService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<EmailService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly ISenderEmailToQueueProducer _emailQueue;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;
        private readonly ICommunicationProvider _communicationProvider;
        public EmailHostedService(IRepositoryNH repository,
            ILogger<EmailService> logger,
            IProjectObjectMapper mapper,
            ISenderEmailToQueueProducer emailQueue,
            IConfiguration configuration,
            IServiceBase serviceBase,
            ICommunicationProvider communicationProvider)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _emailQueue = emailQueue;
            _configuration = configuration;
            _serviceBase = serviceBase;
            _communicationProvider = communicationProvider;
        }

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<EmailModel> emails)?> Search(SearchEmailModel searchModel)
        {
            List<Parameter> parameters = new();
            StringBuilder sb = new(@"Select 
                                        e.Id, 
                                        e.DataHoraCriacao, 
                                        e.UsuarioCriacao, 
                                        e.DataHoraAlteracao, 
                                        e.UsuarioAlteracao,
                                        emp.Id as EmpresaId,
                                        e.Assunto,
                                        e.Destinatario,
                                        e.Enviado,
                                        e.ConteudoEmail
                                        From 
                                        Email e 
                                        Left Outer Join Empresa emp on e.Empresa = emp.Id
                                    Where 1 = 1");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
                sb.AppendLine($" and e.Id = {searchModel.Id} ");

            if (!string.IsNullOrEmpty(searchModel.Destinatario))
                sb.AppendLine($" and Lower(e.Destinatario) like '%{searchModel.Destinatario.ToLower()}%'");

            if (!string.IsNullOrEmpty(searchModel.Assunto))
                sb.AppendLine($" and Lower(e.Assunto) like '%{searchModel.Assunto.ToLower()}%'");

            if (searchModel.Enviado.HasValue)
                sb.AppendLine($" and e.Enviado = {(int)searchModel.Enviado.GetValueOrDefault(EnumSimNao.Não)}");

            if (searchModel.DataHoraCriacaoInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                sb.AppendLine($" and e.DataHoraCriacao >= :dataCriacaoInicial");
                parameters.Add(new Parameter("dataCriacaoInicial", searchModel.DataHoraCriacaoInicial.GetValueOrDefault()));
            }

            if (searchModel.DataHoraCriacaoFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                sb.AppendLine($" and e.DataHoraCriacao <= :dataCriacaoFinal");
                parameters.Add(new Parameter("dataCriacaoFinal", searchModel.DataHoraCriacaoFinal.GetValueOrDefault().Date.AddDays(1).AddSeconds(-1)));
            }

            if (searchModel.DataHoraEnvioInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                sb.AppendLine($" and e.DataHoraEnvio >= :dataEnvioInicial");
                parameters.Add(new Parameter("dataEnvioInicial", searchModel.DataHoraEnvioInicial.GetValueOrDefault()));
            }

            if (searchModel.DataHoraEnvioFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                sb.AppendLine($" and e.DataHoraEnvio <= :dataEnvioFinal");
                parameters.Add(new Parameter("dataEnvioFinal", searchModel.DataHoraEnvioFinal.GetValueOrDefault()));
            }

            var sql = sb.ToString();

            int totalRegistros = 0;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 15;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
            {
                totalRegistros = Convert.ToInt32(await _repository.CountTotalEntry(sql, parameters.ToArray()));
            }

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine(" Order by e.Id ");

            var emails = searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 ?
                await _repository.FindBySql<EmailModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(1), parameters.ToArray())
                : await _repository.FindBySql<EmailModel>(sb.ToString(), parameters.ToArray());


            if (emails.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    Int64 totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);

                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), await _serviceBase.SetUserName(emails.AsList()));
                }

                return (1, 1, await _serviceBase.SetUserName(emails.AsList()));
            }

            return default;
        }


        public async Task<DeleteResultModel> DeleteEmail(int id)
        {
            var result = new DeleteResultModel();
            result.Id = id;

            try
            {

                var email = await _repository.FindById<Email>(id);
                if (email is null)
                {
                    throw new ArgumentException($"Não foi encontrado o Email com Id: {id}!");
                }


                _repository.BeginTransaction();
                await _repository.Remove(email);

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
                _logger.LogError(err, $"Não foi possível deletar o Email: {id}");
                throw;
            }
        }

        public async Task<bool> Save(EmailInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                Email email = _mapper.Map<Email>(model);
                email.Enviado = EnumSimNao.Não;

                var result = await _repository.Save(email);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Email salvo(s) com sucesso!");

                    await _emailQueue.AddEmailMessageToQueue(_mapper.Map<EmailModel>(result));
                    return true;
                }
                throw exception ?? new Exception($"Não foi possível salvar o email: ({model.Assunto})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o email: ({model.Assunto})");
                _repository.Rollback();
                throw;
            }
        }


        public async Task<EmailModel> SaveInternal(EmailInputInternalModel model)
        {
            try
            {
                Email email = _mapper.Map<Email>(model);
                email.Enviado = EnumSimNao.Não;
                if (!string.IsNullOrEmpty(email.ConteudoEmail))
                    email.ConteudoEmail = email.ConteudoEmail.Replace("  ", "");
                var result = await _repository.Save(email);
                var emailModel = _mapper.Map<EmailModel>(result);
                await _emailQueue.AddEmailMessageToQueue(emailModel);
                emailModel.Id = email.Id;
                return emailModel;
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o email: ({model.Assunto})");
                throw;
            }
        }

        public async Task<EmailModel> Update(AlteracaoEmailInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                var emailExistente = (await _repository.FindByHql<Email>($"From Email e Inner Join Fetch e.Empresa emp Where e.Id = {model.Id}")).FirstOrDefault();
                if (emailExistente == null)
                    throw new ArgumentException($"Não foi encontrado o email com Id: {model.Id}");

                Email email = _mapper.Map(model, emailExistente);

                var result = await _repository.Save(email);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Email: ({result.Id}) salvo com sucesso!");

                    if (result != null)
                    {
                        var emailModel = _mapper.Map<EmailModel>(result);
                        if (result.Enviado == EnumSimNao.Não)
                            await _emailQueue.AddEmailMessageToQueue(emailModel);

                        return emailModel;
                    }

                }
                throw exception ?? new Exception($"Não foi possível salvar o email: ({model.Assunto})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o email: ({model.Assunto})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task MarcarComoEnviado(int id)
        {
            try
            {
                var usuarioDefault = _configuration.GetValue<int>("UsuarioSistemaId", 1);

                Email email = await _repository.FindById<Email>(id);
                if (email.Enviado == EnumSimNao.Sim)
                    throw new ArgumentException($"O email id: {id}, já foi enviado anteriormente em: {email.DataHoraEnvio.GetValueOrDefault():dd/MM/yyyy:HH:mm:ss}");

                email.Enviado = EnumSimNao.Sim;
                email.DataHoraEnvio = DateTime.Now;
                email.UsuarioAlteracao = email.UsuarioCriacao.GetValueOrDefault(usuarioDefault);

                var result = await _repository.Save(email);

                _logger.LogInformation($"Email: ({result.Id}) salvo com sucesso!");
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        public async Task<bool> Send(int id)
        {
            try
            {
                _repository.BeginTransaction();

                var emailExistente = (await _repository.FindByHql<Email>($"From Email e Left Join Fetch e.Empresa emp Where e.Id = {id}")).FirstOrDefault();
                if (emailExistente == null)
                    throw new ArgumentException($"Não foi encontrado o email com Id: {id}");

                await _repository.Lock(emailExistente, NHibernate.LockMode.UpgradeNoWait);

                if (emailExistente.Enviado == EnumSimNao.Sim || emailExistente.NaFila == EnumSimNao.Sim)
                {
                    _repository.Rollback();
                    return true;
                }
                else
                {
                    emailExistente.NaFila = EnumSimNao.Sim;
                    await _repository.Save(emailExistente);
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    await _emailQueue.AddEmailMessageToQueue(new EmailModel()
                    {
                        Id = emailExistente.Id,
                        Assunto = emailExistente.Assunto,
                        Destinatario = emailExistente.Destinatario,
                        ConteudoEmail = emailExistente.ConteudoEmail,
                        Enviado = emailExistente.Enviado
                    });
                    return true;
                }
                throw exception ?? new Exception($"Não foi possível salvar o email: ({emailExistente.Assunto})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o email");
                _repository.Rollback();
                throw;
            }
        }
    }
}
