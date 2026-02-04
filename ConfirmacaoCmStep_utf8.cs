using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing
{
    /// <summary>
    /// Step 4: Confirma��o final no sistema CM (Oracle)
    /// Atualiza status e confirma a opera��o
    /// </summary>
    public class ConfirmacaoCmStep : IDistributedTransactionStep
    {
        private readonly IRepositoryNHCm _repositoryCm;
        private readonly ILogger<ConfirmacaoCmStep> _logger;
        private readonly object _confirmationData;

        public string StepName => "ConfirmacaoCM";
        public int Order => 4;

        public ConfirmacaoCmStep(
            IRepositoryNHCm repositoryCm,
            ILogger<ConfirmacaoCmStep> logger,
            object confirmationData)
        {
            _repositoryCm = repositoryCm;
            _logger = logger;
            _confirmationData = confirmationData;
        }

        public async Task<(bool Success, string ErrorMessage, object? Data)> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("[ConfirmacaoCM] Iniciando confirma��o no sistema CM");

                _repositoryCm.BeginTransaction();

                // TODO: Implementar confirma��o real no CM
                // Exemplo: atualizar status de reserva, gravar hist�rico, etc.

                var commitResult = await _repositoryCm.CommitAsync();

                if (!commitResult.executed)
                {
                    throw commitResult.exception ?? new Exception("Falha ao commitar transa��o CM");
                }

                _logger.LogInformation("[ConfirmacaoCM] Confirma��o conclu�da com sucesso");

                return (true, string.Empty, new { ConfirmedAt = DateTime.Now });
            }
            catch (Exception ex)
            {
                _repositoryCm.Rollback();
                _logger.LogError(ex, "[ConfirmacaoCM] Erro ao confirmar no CM");
                return (false, $"Falha na confirma��o CM: {ex.Message}", null);
            }
        }

        public async Task<bool> CompensateAsync(object? executionData)
        {
            try
            {
                _logger.LogInformation("[ConfirmacaoCM] Compensando - revertendo confirma��o");

                _repositoryCm.BeginTransaction();

                // TODO: Implementar revers�o real
                // Exemplo: reverter status, adicionar flag de cancelado, etc.

                var commitResult = await _repositoryCm.CommitAsync();

                if (commitResult.executed)
                {
                    _logger.LogInformation("[ConfirmacaoCM] Compensa��o conclu�da");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ConfirmacaoCM] Erro ao compensar");
                return false;
            }
        }
    }
}
