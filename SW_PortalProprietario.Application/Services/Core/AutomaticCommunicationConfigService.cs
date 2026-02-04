using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class AutomaticCommunicationConfigService : IAutomaticCommunicationConfigService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<AutomaticCommunicationConfigService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly ICommunicationHandlerFactory _handlerFactory;
        private readonly IEmailService _emailService;
        private readonly IServiceBase _serviceBase;

        public AutomaticCommunicationConfigService(
            IRepositoryNH repository,
            ILogger<AutomaticCommunicationConfigService> logger,
            IProjectObjectMapper mapper,
            ICommunicationHandlerFactory handlerFactory,
            IEmailService emailService,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _handlerFactory = handlerFactory;
            _emailService = emailService;
            _serviceBase = serviceBase;
        }

        public async Task<AutomaticCommunicationConfigModel?> GetByCommunicationTypeAsync(EnumDocumentTemplateType? communicationType = EnumDocumentTemplateType.VoucherReserva, EnumProjetoType? projetoType = null)
        {
            try
            {
                var parameters = new List<SW_Utils.Auxiliar.Parameter>
                {
                    new SW_Utils.Auxiliar.Parameter("templateType", (int)communicationType!)
                };

                var hql = "From AutomaticCommunicationConfig a Where a.CommunicationType = :templateType";
                
                if (projetoType.HasValue)
                {
                    hql += " and a.ProjetoType = :projetoType";
                    parameters.Add(new SW_Utils.Auxiliar.Parameter("projetoType", (int)projetoType.Value));
                }

                var configs = await _repository.FindByHql<AutomaticCommunicationConfig>(hql, session: null, parameters.ToArray());
               

                var config = configs.FirstOrDefault();
                if (config == null)
                    return null;

                var model = _mapper.Map<AutomaticCommunicationConfigModel>(config);
                model.DaysBeforeCheckIn = config.GetDaysBeforeCheckIn();
                model.ExcludedStatusCrcIds = config.GetExcludedStatusCrcIds();
                model.EmpresaIds = config.GetEmpresaIds();
                model.TemplateSendMode = config.TemplateSendMode;
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar configuração de comunicação automática: {communicationType}, projetoType: {projetoType}");
                throw;
            }
        }

        public async Task<List<AutomaticCommunicationConfigModel>> GetAllAsync(EnumProjetoType? projetoType = null)
        {
            try
            {
                var parameters = new List<SW_Utils.Auxiliar.Parameter>();
                var hql = "From AutomaticCommunicationConfig a";
                
                if (projetoType.HasValue)
                {
                    hql += " Where a.ProjetoType = :projetoType";
                    parameters.Add(new SW_Utils.Auxiliar.Parameter("projetoType", (int)projetoType.Value));
                }

                hql += " Order by a.CommunicationType, a.ProjetoType";

                var configs = await _repository.FindByHql<AutomaticCommunicationConfig>(hql, session: null, parameters.ToArray());

                var models = new List<AutomaticCommunicationConfigModel>();
                foreach (var config in configs)
                {
                    var model = _mapper.Map<AutomaticCommunicationConfigModel>(config);
                    model.DaysBeforeCheckIn = config.GetDaysBeforeCheckIn();
                    model.ExcludedStatusCrcIds = config.GetExcludedStatusCrcIds();
                    model.EmpresaIds = config.GetEmpresaIds();
                    model.TemplateSendMode = config.TemplateSendMode;
                    models.Add(model);
                }

                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar todas as configurações de comunicação automática. ProjetoType: {projetoType}");
                throw;
            }
        }

        public async Task<AutomaticCommunicationConfigModel> SaveAsync(AutomaticCommunicationConfigInputModel model)
        {
            try
            {
                _repository.BeginTransaction();


                if (model.TemplateId == null || model.TemplateId <= 0)
                    throw new ArgumentException("O template deve ser informado");

                if (string.IsNullOrEmpty(model.Subject))
                    throw new ArgumentException("O assunto do email deve ser informado");

                if (string.IsNullOrEmpty(model.CommunicationType))
                    throw new ArgumentException("O tipo de comunicação deve ser informado");

                if (!Enum.TryParse<EnumDocumentTemplateType>(model.CommunicationType, out var communicationTypeEnum))
                    throw new ArgumentException($"Tipo de comunicação inválido: {model.CommunicationType}");

                if (!Enum.IsDefined(typeof(EnumProjetoType), model.ProjetoType))
                    throw new ArgumentException("Tipo de projeto inválido");

                if (model.DaysBeforeCheckIn == null || model.DaysBeforeCheckIn.Count == 0)
                    throw new ArgumentException("Pelo menos um dia deve ser informado");


                // Verificar se o template existe
                var documentTemplate = (await _repository.FindByHql<DocumentTemplate>(
                    $"From DocumentTemplate a Where a.Id = :templateId", session: null, new SW_Utils.Auxiliar.Parameter("templateId", model.TemplateId.Value))).FirstOrDefault();

                if (documentTemplate == null)
                    throw new ArgumentException($"Template com ID {model.TemplateId} não encontrado");

                var loggedUser = await _repository.GetLoggedUser();

                var config = new AutomaticCommunicationConfig
                {
                    DataHoraCriacao = DateTime.Now,
                    UsuarioCriacao = int.Parse(loggedUser.Value.userId),
                };

                config.CommunicationType = communicationTypeEnum;
                config.ProjetoType = (EnumProjetoType)model.ProjetoType;
                config.Enabled = model.Enabled;
                config.TemplateId = model.TemplateId;
                config.Subject = model.Subject;
                config.SetDaysBeforeCheckIn(model.DaysBeforeCheckIn);
                config.SetExcludedStatusCrcIds(model.ExcludedStatusCrcIds ?? new List<int>());
                config.SendOnlyToAdimplentes = model.SendOnlyToAdimplentes;
                config.AllCompanies = model.AllCompanies;
                config.SetEmpresaIds(model.AllCompanies ? new List<int>() : (model.EmpresaIds ?? new List<int>()));
                
                if (Enum.IsDefined(typeof(EnumTemplateSendMode), model.TemplateSendMode))
                {
                    config.TemplateSendMode = (EnumTemplateSendMode)model.TemplateSendMode;
                }
                else
                {
                    config.TemplateSendMode = EnumTemplateSendMode.BodyHtmlOnly;
                }

                await _repository.Save(config);
                await _repository.CommitAsync();

                var result = _mapper.Map<AutomaticCommunicationConfigModel>(config);
                result.DaysBeforeCheckIn = config.GetDaysBeforeCheckIn();
                result.ExcludedStatusCrcIds = config.GetExcludedStatusCrcIds();
                result.EmpresaIds = config.GetEmpresaIds();
                result.TemplateSendMode = config.TemplateSendMode;
                return result;
            }
            catch (Exception ex)
            {
                _repository.Rollback();
                _logger.LogError(ex, $"Erro ao salvar configuração de comunicação automática: {model.CommunicationType}");
                throw;
            }
        }

        public async Task<AutomaticCommunicationConfigModel> UpdateAsync(int id, AutomaticCommunicationConfigInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                var existingConfigs = await _repository.FindByHql<AutomaticCommunicationConfig>(
                    $"From AutomaticCommunicationConfig a Where a.Id = {id}");
                var config = existingConfigs.FirstOrDefault();

                if (config == null)
                    throw new ArgumentException($"Configuração com ID {id} não encontrada");

                if (model.TemplateId == null || model.TemplateId <= 0)
                    throw new ArgumentException("O template deve ser informado");

                if (string.IsNullOrEmpty(model.Subject))
                    throw new ArgumentException("O assunto do email deve ser informado");

                if (string.IsNullOrEmpty(model.CommunicationType))
                    throw new ArgumentException("O tipo de comunicação deve ser informado");

                if (!Enum.TryParse<EnumDocumentTemplateType>(model.CommunicationType, out var communicationTypeEnum))
                    throw new ArgumentException($"Tipo de comunicação inválido: {model.CommunicationType}");

                if (!Enum.IsDefined(typeof(EnumProjetoType), model.ProjetoType))
                    throw new ArgumentException("Tipo de projeto inválido");

                if ((model.DaysBeforeCheckIn == null || model.DaysBeforeCheckIn.Count == 0) && model.Enabled)
                    throw new ArgumentException("Pelo menos um dia deve ser informado");

                // Verificar se o template existe
                var documentTemplate = (await _repository.FindByHql<DocumentTemplate>(
                    $"From DocumentTemplate a Where a.Id = :templateId", session: null, new SW_Utils.Auxiliar.Parameter("templateId", model.TemplateId.Value))).FirstOrDefault();

                if (documentTemplate == null)
                    throw new ArgumentException($"Template com ID {model.TemplateId} não encontrado");

                var loggedUser = await _repository.GetLoggedUser();

                config.CommunicationType = communicationTypeEnum;
                config.ProjetoType = (EnumProjetoType)model.ProjetoType;
                config.Enabled = model.Enabled;
                config.TemplateId = model.TemplateId;
                config.Subject = model.Subject;
                config.SetDaysBeforeCheckIn(model.DaysBeforeCheckIn);
                config.SetExcludedStatusCrcIds(model.ExcludedStatusCrcIds ?? new List<int>());
                config.SendOnlyToAdimplentes = model.SendOnlyToAdimplentes;
                config.AllCompanies = model.AllCompanies;
                config.SetEmpresaIds(model.AllCompanies ? new List<int>() : (model.EmpresaIds ?? new List<int>()));
                
                if (Enum.IsDefined(typeof(EnumTemplateSendMode), model.TemplateSendMode))
                {
                    config.TemplateSendMode = (EnumTemplateSendMode)model.TemplateSendMode;
                }
                else
                {
                    config.TemplateSendMode = EnumTemplateSendMode.BodyHtmlOnly;
                }
                
                config.DataHoraAlteracao = DateTime.Now;
                config.UsuarioAlteracao = int.Parse(loggedUser.Value.userId);

                await _repository.Save(config);
                await _repository.CommitAsync();

                var result = _mapper.Map<AutomaticCommunicationConfigModel>(config);
                result.DaysBeforeCheckIn = config.GetDaysBeforeCheckIn();
                result.ExcludedStatusCrcIds = config.GetExcludedStatusCrcIds();
                result.EmpresaIds = config.GetEmpresaIds();
                result.TemplateSendMode = config.TemplateSendMode;
                return result;
            }
            catch (Exception ex)
            {
                _repository.Rollback();
                _logger.LogError(ex, $"Erro ao atualizar configuração de comunicação automática: {id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _repository.BeginTransaction();

                var existingConfigs = await _repository.FindByHql<AutomaticCommunicationConfig>(
                    $"From AutomaticCommunicationConfig a Where a.Id = {id}");
                var config = existingConfigs.FirstOrDefault();

                if (config == null)
                    throw new ArgumentException($"Configuração com ID {id} não encontrada");

                _repository.Remove(config);
                await _repository.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _repository.Rollback();
                _logger.LogError(ex, $"Erro ao deletar configuração de comunicação automática: {id}");
                throw;
            }
        }

        public async Task<bool> SimulateEmailAsync(int configId, string userEmail)
        {
            try
            {
                // Buscar configuração pelo ID
                var configs = await _repository.FindByHql<AutomaticCommunicationConfig>(
                    $"From AutomaticCommunicationConfig a Where a.Id = {configId}");
                var config = configs.FirstOrDefault();

                if (config == null)
                    throw new ArgumentException($"Configuração com ID {configId} não encontrada");

                // Validar se a configuração está ativa
                if (!config.Enabled)
                    throw new ArgumentException("A configuração não está ativa");

                if (config.TemplateId == null || config.TemplateId <= 0)
                    throw new ArgumentException("O template não está configurado");

                if (string.IsNullOrEmpty(config.Subject))
                    throw new ArgumentException("O assunto do email não está configurado");

                var daysBeforeCheckIn = config.GetDaysBeforeCheckIn();
                if (daysBeforeCheckIn == null || daysBeforeCheckIn.Count == 0)
                    throw new ArgumentException("Nenhum dia configurado para simulação");

                var configModel = _mapper.Map<AutomaticCommunicationConfigModel>(config);
                configModel.DaysBeforeCheckIn = daysBeforeCheckIn;
                configModel.ExcludedStatusCrcIds = config.GetExcludedStatusCrcIds();
                configModel.EmpresaIds = config.GetEmpresaIds();
                configModel.TemplateSendMode = config.TemplateSendMode;

                // Obter o handler apropriado para o tipo de comunicação
                var handler = _handlerFactory.GetHandler(config.CommunicationType);
                if (handler == null)
                {
                    _logger.LogWarning("Nenhum handler encontrado para tipo de comunicação {CommunicationType}", config.CommunicationType);
                    throw new ArgumentException($"Tipo de comunicação {config.CommunicationType} não suportado");
                }

                var loggedUser = await _repository.GetLoggedUser();
                var emailModel = await handler.GenerateSimulationEmailAsync(configModel, userEmail, !string.IsNullOrEmpty(loggedUser.Value.userId) ? int.Parse(loggedUser.Value.userId) : 1);

                foreach (var item in emailModel)
                {
                    // Salvar email (que será enviado pela fila)
                    var emailSaved = await _emailService.SaveInternal(item);
                    _logger.LogInformation($"Email de simulação enviado para {item.Destinatario} (Config ID: {configId}, Tipo: {config.CommunicationType})");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao simular envio de email para configuração {configId}");
                throw;
            }
        }
    }
}

