using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Hosted;

/// <summary>
/// Serviço em background para processamento automático de comunicações
/// (Vouchers, Avisos de Check-in, Incentivo para Agendamento, etc.)
/// ? UNIFICADO - Processa TODOS os tipos de comunicação dinamicamente
/// </summary>
public class AutomaticCommunicationEmailHostedService : BackgroundService
{
    private readonly IRepositoryHosted _repository;
    private readonly ILogger<AutomaticCommunicationEmailHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static bool isRunning = false;

    public AutomaticCommunicationEmailHostedService(
        IRepositoryHosted repository,
        ILogger<AutomaticCommunicationEmailHostedService> logger,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory)
    {
        _repository = repository;
        _logger = logger;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("?? AutomaticCommunicationEmailHostedService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!isRunning)
            {
                isRunning = true;
                try
                {
                    await ProcessAllCommunicationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Erro ao processar comunicações automáticas");
                }
                finally
                {
                    isRunning = false;
                }
            }

            // Executar uma vez por dia (24 horas)
            var intervalHours = _configuration.GetValue<int>("AutomaticCommunicationEmailIntervalHours", 24);
            _logger.LogInformation("? Próxima execução em {Hours} horas", intervalHours);
            
            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
        }

        _logger.LogInformation("?? AutomaticCommunicationEmailHostedService finalizado");
    }

    private async Task ProcessAllCommunicationsAsync()
    {
        _logger.LogInformation("???????????????????????????????????????????????????");
        _logger.LogInformation("?? INICIANDO PROCESSAMENTO DE COMUNICAÇÕES AUTOMÁTICAS");
        _logger.LogInformation("???????????????????????????????????????????????????");

        // ? PROCESSAR TODOS OS TIPOS DE COMUNICAÇÃO DINAMICAMENTE
        var allCommunicationTypes = Enum.GetValues<EnumDocumentTemplateType>();
        
        foreach (var communicationType in allCommunicationTypes)
        {
            var friendlyName = GetFriendlyName(communicationType);
            await ProcessCommunicationTypeAsync(communicationType, friendlyName);
        }

        _logger.LogInformation("???????????????????????????????????????????????????");
        _logger.LogInformation("? PROCESSAMENTO DE COMUNICAÇÕES AUTOMÁTICAS CONCLUÍDO");
        _logger.LogInformation("???????????????????????????????????????????????????");
    }

    private async Task ProcessCommunicationTypeAsync(EnumDocumentTemplateType communicationType, string friendlyName)
    {
        _logger.LogInformation("???????????????????????????????????????????????????");
        _logger.LogInformation("?? Processando: {FriendlyName} ({CommunicationType})", friendlyName, communicationType);
        _logger.LogInformation("???????????????????????????????????????????????????");

        // Processar Multipropriedade
        if (_configuration.GetValue<bool>("MultipropriedadeAtivada", false))
        {
            await ProcessForProjectTypeAsync(communicationType, EnumProjetoType.Multipropriedade, friendlyName);
        }
        else
        {
            _logger.LogInformation("?? Multipropriedade desativada - pulando");
        }

        // Processar Timesharing
        if (_configuration.GetValue<bool>("TimeSharingAtivado", false))
        {
            await ProcessForProjectTypeAsync(communicationType, EnumProjetoType.Timesharing, friendlyName);
        }
        else
        {
            _logger.LogInformation("?? Timesharing desativado - pulando");
        }
    }

    private async Task ProcessForProjectTypeAsync(
        EnumDocumentTemplateType communicationType,
        EnumProjetoType projetoType,
        string friendlyName)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var configService = scope.ServiceProvider.GetRequiredService<IAutomaticCommunicationConfigService>();
            var handlerFactory = scope.ServiceProvider.GetRequiredService<ICommunicationHandlerFactory>();
            
            using (var session = _repository.CreateSession())
            {
                try
                {
                    var projectTypeName = projetoType == EnumProjetoType.Multipropriedade ? "Multipropriedade" : "Timesharing";
                    
                    _logger.LogInformation(" ?? Tipo: {FriendlyName} - {ProjectType}", friendlyName, projectTypeName);

                    // Buscar configuração ativa
                    var config = await configService.GetByCommunicationTypeAsync(communicationType, projetoType);
                    
                    if (config == null || !config.Enabled)
                    {
                        _logger.LogInformation(" ?? Configuração não encontrada ou desabilitada");
                        return;
                    }

                    if (config.TemplateId == null || config.TemplateId <= 0)
                    {
                        _logger.LogWarning(" ?? Template não configurado");
                        return;
                    }

                    // ? VERIFICAÇÃO CONDICIONAL PARA DaysBeforeCheckIn
                    // Alguns tipos de comunicação (como Incentivo para Agendamento) não usam DaysBeforeCheckIn
                    var requiresDaysBeforeCheckIn = communicationType == EnumDocumentTemplateType.VoucherReserva ||
                                                    communicationType == EnumDocumentTemplateType.AvisoReservaCheckinProximo || 
                                                    communicationType == EnumDocumentTemplateType.IncentivoParaAgendamento;

                    if (requiresDaysBeforeCheckIn && (config.DaysBeforeCheckIn == null || config.DaysBeforeCheckIn.Count == 0))
                    {
                        _logger.LogWarning(" ?? Nenhum dia antes do check-in configurado");
                        return;
                    }

                    // Obter handler apropriado
                    var handler = handlerFactory.GetHandler(communicationType);
                    if (handler == null)
                    {
                        _logger.LogWarning(" ?? Handler não encontrado para {CommunicationType}", communicationType);
                        return;
                    }

                    // Processar
                    if (requiresDaysBeforeCheckIn)
                    {
                        _logger.LogInformation(" ?? Dias configurados: {Days}", string.Join(", ", config.DaysBeforeCheckIn!));
                    }
                    
                    if (projetoType == EnumProjetoType.Multipropriedade)
                    {
                        await handler.ProcessMultiPropriedadeAsync(session!, config);
                    }
                    else
                    {
                        await handler.ProcessTimesharingAsync(session!, config);
                    }

                    _logger.LogInformation(" ?? Processamento concluído: {FriendlyName} - {ProjectType}", 
                        friendlyName, projectTypeName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, " ?? Erro ao processar {FriendlyName} - {ProjectType}", 
                        friendlyName, projetoType);
                }
            }
        }
    }

    /// <summary>
    /// Retorna nome amigável para o tipo de comunicação
    /// </summary>
    private string GetFriendlyName(EnumDocumentTemplateType communicationType)
    {
        return communicationType switch
        {
            EnumDocumentTemplateType.VoucherReserva => "Vouchers de Reserva",
            EnumDocumentTemplateType.ComunicacaoCancelamentoReservaRci => "Cancelamento de Reserva RCI",
            EnumDocumentTemplateType.AvisoReservaCheckinProximo => "Avisos de Check-in Próximo",
            EnumDocumentTemplateType.IncentivoParaAgendamento => "Incentivo para Agendamento",
            _ => communicationType.ToString()
        };
    }
}
