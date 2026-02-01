using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.IncentivoAgendamento;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.IncentivoAgendamento;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.IncentivoAgendamento;

/// <summary>
/// Handler para envio autom�tico de incentivo para realiza��o de agendamentos
/// Usa IncentivoAgendamentoGenerationService para gera��o de layout (simula��o e processamento)
/// </summary>
public class IncentivoParaAgendamentoHandler : ICommunicationHandler
{
    private readonly IncentivoAgendamentoProcessingService _processingService;
    private readonly IncentivoAgendamentoSimulationService _simulationService;
    private readonly ILogger<IncentivoParaAgendamentoHandler> _logger;

    public EnumDocumentTemplateType CommunicationType => EnumDocumentTemplateType.IncentivoParaAgendamento;

    public IncentivoParaAgendamentoHandler(
        IncentivoAgendamentoProcessingService processingService,
        IncentivoAgendamentoSimulationService simulationService,
        ILogger<IncentivoParaAgendamentoHandler> logger)
    {
        _processingService = processingService;
        _simulationService = simulationService;
        _logger = logger;
    }

    public async Task ProcessMultiPropriedadeAsync(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO INCENTIVO AGENDAMENTO - MULTIPROPRIEDADE ===");

        if (config.DaysBeforeCheckIn == null || !config.DaysBeforeCheckIn.Any())
        {
            _logger.LogWarning("Nenhum intervalo configurado para processamento de incentivo");
            return;
        }

        foreach (var intervalo in config.DaysBeforeCheckIn)
        {
            try
            {
                _logger.LogInformation("Processando incentivos para intervalo de {Intervalo} dias", intervalo);
                
                await _processingService.ProcessarIncentivosMultiPropriedadeAsync(session, config, intervalo);
                
                _logger.LogInformation("Processamento concluído para intervalo {Intervalo}", intervalo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar incentivos para intervalo {Intervalo}", intervalo);
            }
        }

        _logger.LogInformation("=== FIM PROCESSAMENTO INCENTIVO AGENDAMENTO - MULTIPROPRIEDADE ===");
    }

    public async Task ProcessTimesharingAsync(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO INCENTIVO AGENDAMENTO - TIMESHARING ===");

        if (config.DaysBeforeCheckIn == null || !config.DaysBeforeCheckIn.Any())
        {
            _logger.LogWarning("Nenhum intervalo configurado para processamento de incentivo");
            return;
        }

        foreach (var intervalo in config.DaysBeforeCheckIn)
        {
            try
            {
                _logger.LogInformation("Processando incentivos Timesharing para intervalo de {Intervalo} dias", intervalo);
                
                await _processingService.ProcessarIncentivosTimesharingAsync(session, config, intervalo);
                
                _logger.LogInformation("Processamento Timesharing concluído para intervalo {Intervalo}", intervalo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar incentivos Timesharing para intervalo {Intervalo}", intervalo);
            }
        }

        _logger.LogInformation("=== FIM PROCESSAMENTO INCENTIVO AGENDAMENTO - TIMESHARING ===");
    }

    public async Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId)
    {
        _logger.LogInformation("Gerando email de simulação de incentivo para agendamento para {UserEmail}", userEmail);
        return await _simulationService.GenerateSimulationEmailAsync(config, userEmail, userId);
    }
}
