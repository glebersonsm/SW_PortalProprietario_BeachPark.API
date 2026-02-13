using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using System.Text.Json;

namespace SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing
{
    /// <summary>
    /// Step 3: Criação de reserva via API externa (CM ou eSolution)
    /// </summary>
    public class CriacaoReservaApiStep : IDistributedTransactionStep
    {
        private readonly ITimeSharingProviderService _timeSharingService;
        private readonly ILogger<CriacaoReservaApiStep> _logger;
        private readonly object _reservaData;

        public string StepName => "CriacaoReservaAPI";
        public int Order => 3;

        public CriacaoReservaApiStep(
            ITimeSharingProviderService timeSharingService,
            ILogger<CriacaoReservaApiStep> logger,
            object reservaData)
        {
            _timeSharingService = timeSharingService;
            _logger = logger;
            _reservaData = reservaData;
        }

        public async Task<(bool Success, string ErrorMessage, object? Data)> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("[CriacaoReservaAPI] Iniciando criação de reserva via API");

                // Chamar o serviÃ§o para criar a reserva
                var reservaId = await _timeSharingService.Save(_reservaData as InclusaoReservaInputModel 
                    ?? throw new InvalidOperationException("Dados de reserva invÃ¡lidos"));
                
                var resultData = new
                {
                    ReservaId = reservaId,
                    CreatedAt = DateTime.Now
                };

                _logger.LogInformation("[CriacaoReservaAPI] Reserva criada com sucesso - ID: {ReservaId}", reservaId);

                return (true, string.Empty, resultData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CriacaoReservaAPI] Erro ao criar reserva via API");
                return (false, $"Falha ao criar reserva: {ex.Message}", null);
            }
        }

        public async Task<bool> CompensateAsync(object? executionData)
        {
            try
            {
                if (executionData != null)
                {
                    var data = JsonSerializer.Deserialize<dynamic>(JsonSerializer.Serialize(executionData));
                    var reservaId = data?.ReservaId?.ToString();

                    if (!string.IsNullOrEmpty(reservaId))
                    {
                        _logger.LogInformation($"[CriacaoReservaAPI] Compensando - cancelando reserva ID: {reservaId}");

                        // TODO: Implementar cancelamento real via API
                        await _timeSharingService.CancelarReserva(reservaId);

                        _logger.LogInformation("[CriacaoReservaAPI] Reserva cancelada com sucesso");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CriacaoReservaAPI] Erro ao compensar (cancelar reserva)");
                return false;
            }
        }
    }
}
