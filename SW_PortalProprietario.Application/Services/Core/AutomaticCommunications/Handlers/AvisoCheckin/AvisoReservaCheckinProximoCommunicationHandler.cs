using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.AvisoCheckin;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.AvisoCheckin;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.AvisoCheckin;

/// <summary>
/// Handler para avisos de check-in próximo
/// </summary>
public class AvisoReservaCheckinProximoCommunicationHandler : ICommunicationHandler
{
    private readonly AvisoCheckinProcessingService _processingService;
    private readonly AvisoCheckinSimulationService _simulationService;
    private readonly ILogger<AvisoReservaCheckinProximoCommunicationHandler> _logger;

    public EnumDocumentTemplateType CommunicationType => EnumDocumentTemplateType.AvisoReservaCheckinProximo;

    public AvisoReservaCheckinProximoCommunicationHandler(
        AvisoCheckinProcessingService processingService,
        AvisoCheckinSimulationService simulationService,
        ILogger<AvisoReservaCheckinProximoCommunicationHandler> logger)
    {
        _processingService = processingService;
        _simulationService = simulationService;
        _logger = logger;
    }

    public async Task ProcessMultiPropriedadeAsync(IStatelessSession session, AutomaticCommunicationConfigModel config)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO AVISOS CHECK-IN - MULTIPROPRIEDADE ===");
        
        if (config.DaysBeforeCheckIn == null || !config.DaysBeforeCheckIn.Any())
        {
            _logger.LogWarning("Nenhum dia antes do check-in configurado");
            return;
        }
        
        foreach (var daysBefore in config.DaysBeforeCheckIn)
        {
            try
            {
                _logger.LogInformation("Processando avisos para {DaysBefore} dias antes do check-in", daysBefore);
                
                await _processingService.ProcessarAvisosMultiPropriedadeAsync(
                    session, 
                    config, 
                    daysBefore,
                    qtdeMaxima: null);
                
                _logger.LogInformation("Processamento concluído para {DaysBefore} dias", daysBefore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar avisos para {DaysBefore} dias", daysBefore);
            }
        }
        
        _logger.LogInformation("=== FIM PROCESSAMENTO AVISOS CHECK-IN - MULTIPROPRIEDADE ===");
    }

    public async Task ProcessTimesharingAsync(IStatelessSession session, AutomaticCommunicationConfigModel config)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO AVISOS CHECK-IN - TIMESHARING ===");
        
        if (config.DaysBeforeCheckIn == null || !config.DaysBeforeCheckIn.Any())
        {
            _logger.LogWarning("Nenhum dia antes do check-in configurado");
            return;
        }
        
        foreach (var daysBefore in config.DaysBeforeCheckIn)
        {
            try
            {
                _logger.LogInformation("Processando avisos Timesharing para {DaysBefore} dias antes do check-in", daysBefore);
                
                await _processingService.ProcessarAvisosTimesharingAsync(
                    session, 
                    config, 
                    daysBefore,
                    qtdeMaxima: null);
                
                _logger.LogInformation("Processamento Timesharing concluído para {DaysBefore} dias", daysBefore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar avisos Timesharing para {DaysBefore} dias", daysBefore);
            }
        }
        
        _logger.LogInformation("=== FIM PROCESSAMENTO AVISOS CHECK-IN - TIMESHARING ===");
    }

    public async Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId)
    {
        _logger.LogInformation("Gerando email de simulação de aviso de check-in próximo");
        return await _simulationService.GenerateSimulationEmailAsync(config, userEmail, userId);
    }
}

