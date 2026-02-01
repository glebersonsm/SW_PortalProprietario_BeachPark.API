using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using System.Text.Json;

namespace SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing
{
    /// <summary>
    /// Step 1: Validação inicial no sistema CM (Oracle)
    /// Verifica disponibilidade e pré-requisitos antes de iniciar a transação
    /// </summary>
    public class ValidacaoCmStep : IDistributedTransactionStep
    {
        private readonly IRepositoryNHCm _repositoryCm;
        private readonly ILogger<ValidacaoCmStep> _logger;
        private readonly object _requestData;

        public string StepName => "ValidacaoCM";
        public int Order => 1;

        public ValidacaoCmStep(
            IRepositoryNHCm repositoryCm,
            ILogger<ValidacaoCmStep> logger,
            object requestData)
        {
            _repositoryCm = repositoryCm;
            _logger = logger;
            _requestData = requestData;
        }

        public async Task<(bool Success, string ErrorMessage, object? Data)> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("[ValidacaoCM] Iniciando validações no sistema CM");

                // TODO: Implementar validações específicas do CM
                // Exemplo: verificar disponibilidade, validar dados, etc.
                
                var validationData = new
                {
                    ValidatedAt = DateTime.Now,
                    RequestData = JsonSerializer.Serialize(_requestData)
                };

                _logger.LogInformation("[ValidacaoCM] Validações concluídas com sucesso");
                
                return (true, string.Empty, validationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ValidacaoCM] Erro durante validação");
                return (false, $"Falha na validação CM: {ex.Message}", null);
            }
        }

        public async Task<bool> CompensateAsync(object? executionData)
        {
            // Não há compensação necessária para validação
            _logger.LogInformation("[ValidacaoCM] Compensação não necessária - apenas validação");
            return await Task.FromResult(true);
        }
    }
}
