using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing
{
    /// <summary>
    /// Step 4: Confirmação final no sistema CM (Oracle)
    /// Atualiza status e confirma a operação
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
                _logger.LogInformation("[ConfirmacaoCM] Iniciando confirmação no sistema CM");

                _repositoryCm.BeginTransaction();

                // TODO: Implementar confirmação real no CM
                // Exemplo: atualizar status de reserva, gravar histórico, etc.

                var commitResult = await _repositoryCm.CommitAsync();

                if (!commitResult.executed)
                {
                    throw commitResult.exception ?? new Exception("Falha ao commitar transação CM");
                }

                _logger.LogInformation("[ConfirmacaoCM] Confirmação concluída com sucesso");

                return (true, string.Empty, new { ConfirmedAt = DateTime.Now });
            }
            catch (Exception ex)
            {
                _repositoryCm.Rollback();
                _logger.LogError(ex, "[ConfirmacaoCM] Erro ao confirmar no CM");
                return (false, $"Falha na confirmação CM: {ex.Message}", null);
            }
        }

        public async Task<bool> CompensateAsync(object? executionData)
        {
            try
            {
                _logger.LogInformation("[ConfirmacaoCM] Compensando - revertendo confirmação");

                _repositoryCm.BeginTransaction();

                // TODO: Implementar reversão real
                // Exemplo: reverter status, adicionar flag de cancelado, etc.

                var commitResult = await _repositoryCm.CommitAsync();

                if (commitResult.executed)
                {
                    _logger.LogInformation("[ConfirmacaoCM] Compensação concluída");
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
