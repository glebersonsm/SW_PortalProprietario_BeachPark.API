using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.Voucher;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.EnvioVoucher;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.Voucher;

/// <summary>
/// Handler para envio automático de vouchers de reserva
/// Usa VoucherGenerationService para geração de layout (simulação e processamento)
/// </summary>
public class VoucherReservaCommunicationHandler : ICommunicationHandler
{
    private readonly VoucherProcessingService _processingService;
    private readonly VoucherSimulationService _simulationService;
    private readonly ILogger<VoucherReservaCommunicationHandler> _logger;

    public EnumDocumentTemplateType CommunicationType => EnumDocumentTemplateType.VoucherReserva;

    public VoucherReservaCommunicationHandler(
        VoucherProcessingService processingService,
        VoucherSimulationService simulationService,
        ILogger<VoucherReservaCommunicationHandler> logger)
    {
        _processingService = processingService;
        _simulationService = simulationService;
        _logger = logger;
    }

    public async Task ProcessMultiPropriedadeAsync(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO VOUCHERS - MULTIPROPRIEDADE ===");

        if (config.DaysBeforeCheckIn == null || !config.DaysBeforeCheckIn.Any())
        {
            _logger.LogWarning("Nenhum dia antes do check-in configurado");
            return;
        }

        foreach (var daysBefore in config.DaysBeforeCheckIn)
        {
            try
            {
                _logger.LogInformation("Processando vouchers para {DaysBefore} dias antes do check-in", daysBefore);
                
                await _processingService.ProcessarVouchersMultiPropriedadeAsync(session, config, daysBefore, qtdeMaxima: null);
                
                _logger.LogInformation("Processamento concluído para {DaysBefore} dias", daysBefore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar vouchers para {DaysBefore} dias", daysBefore);
            }
        }

        _logger.LogInformation("=== FIM PROCESSAMENTO VOUCHERS - MULTIPROPRIEDADE ===");
    }

    public async Task ProcessTimesharingAsync(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO VOUCHERS - TIMESHARING ===");

        if (config.DaysBeforeCheckIn == null || !config.DaysBeforeCheckIn.Any())
        {
            _logger.LogWarning("Nenhum dia antes do check-in configurado");
            return;
        }

        foreach (var daysBefore in config.DaysBeforeCheckIn)
        {
            try
            {
                _logger.LogInformation("Processando vouchers Timesharing para {DaysBefore} dias antes do check-in", daysBefore);
                
                await _processingService.ProcessarVouchersTimesharingAsync(session, config, daysBefore, qtdeMaxima: null);
                
                _logger.LogInformation("Processamento Timesharing concluído para {DaysBefore} dias", daysBefore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar vouchers Timesharing para {DaysBefore} dias", daysBefore);
            }
        }

        _logger.LogInformation("=== FIM PROCESSAMENTO VOUCHERS - TIMESHARING ===");
    }

    public async Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId)
    {
        _logger.LogInformation("Gerando email de simulação de voucher para {UserEmail}", userEmail);
        return await _simulationService.GenerateSimulationEmailAsync(config, userEmail, userId);
    }
}
