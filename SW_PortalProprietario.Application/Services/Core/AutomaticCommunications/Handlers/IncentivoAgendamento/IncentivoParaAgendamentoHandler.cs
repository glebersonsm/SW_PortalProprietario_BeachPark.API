using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.IncentivoAgendamento;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.IncentivoAgendamento;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.IncentivoAgendamento;

/// <summary>
/// Handler para envio automï¿½tico de incentivo para realizaï¿½ï¿½o de agendamentos
/// Usa IncentivoAgendamentoGenerationService para geraï¿½ï¿½o de layout (simulaï¿½ï¿½o e processamento)
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
        _logger.LogInformation("=== INÃCIO PROCESSAMENTO INCENTIVO AGENDAMENTO - MULTIPROPRIEDADE ===");

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
                
                _logger.LogInformation("Processamento concluÃ­do para intervalo {Intervalo}", intervalo);
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
        _logger.LogInformation("=== INÃCIO PROCESSAMENTO INCENTIVO AGENDAMENTO - TIMESHARING ===");

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
                
                _logger.LogInformation("Processamento Timesharing concluÃ­do para intervalo {Intervalo}", intervalo);
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
        _logger.LogInformation("Gerando email de simulaÃ§Ã£o de incentivo para agendamento para {UserEmail}", userEmail);
        return await _simulationService.GenerateSimulationEmailAsync(config, userEmail, userId);
    }
}
