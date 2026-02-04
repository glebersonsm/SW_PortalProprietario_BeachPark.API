using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.Application.Services.Core
{
    /// <summary>
    /// Orquestrador de transações distribuídas usando o padrão Saga
    /// Garante atomicidade entre múltiplos bancos de dados e APIs externas
    /// </summary>
    public class SagaOrchestrator
    {
        private readonly ILogger<SagaOrchestrator> _logger;
        private readonly List<(IDistributedTransactionStep Step, object? Data)> _executedSteps = new();

        public SagaOrchestrator(ILogger<SagaOrchestrator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executa uma saga com os steps fornecidos
        /// Em caso de falha, executa compensação em ordem reversa
        /// </summary>
        /// <param name="steps">Steps a serem executados</param>
        /// <param name="operationId">ID único da operação para rastreamento</param>
        /// <returns>Tupla indicando sucesso e mensagem de erro</returns>
        public async Task<(bool Success, string ErrorMessage)> ExecuteAsync(
            IEnumerable<IDistributedTransactionStep> steps,
            string operationId)
        {
            var orderedSteps = steps.OrderBy(s => s.Order).ToList();
            
            _logger.LogInformation("?? [SAGA-START] OperationId: {OperationId} | Total Steps: {StepCount}", 
                operationId, orderedSteps.Count);

            try
            {
                foreach (var step in orderedSteps)
                {
                    _logger.LogInformation("?? [SAGA-EXECUTE] Step: {StepName} | Order: {Order} | OperationId: {OperationId}", 
                        step.StepName, step.Order, operationId);

                    var (success, errorMessage, data) = await step.ExecuteAsync();

                    if (!success)
                    {
                        _logger.LogWarning("? [SAGA-FAILED] Step: {StepName} | Error: {Error} | OperationId: {OperationId}", 
                            step.StepName, errorMessage, operationId);
                        
                        await CompensateAllAsync(operationId);
                        return (false, $"Falha no step '{step.StepName}': {errorMessage}");
                    }

                    _executedSteps.Add((step, data));
                    
                    _logger.LogInformation("? [SAGA-SUCCESS] Step: {StepName} | OperationId: {OperationId}", 
                        step.StepName, operationId);
                }

                _logger.LogInformation("?? [SAGA-COMPLETE] OperationId: {OperationId} | All steps executed successfully", 
                    operationId);
                
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? [SAGA-ERROR] Unexpected error | OperationId: {OperationId}", 
                    operationId);
                
                await CompensateAllAsync(operationId);
                return (false, $"Erro inesperado na transação distribuída: {ex.Message}");
            }
        }

        /// <summary>
        /// Executa compensação de todos os steps executados em ordem reversa
        /// </summary>
        private async Task CompensateAllAsync(string operationId)
        {
            _logger.LogWarning("?? [SAGA-COMPENSATE-START] OperationId: {OperationId} | Steps to compensate: {Count}", 
                operationId, _executedSteps.Count);

            foreach (var (step, data) in _executedSteps.AsEnumerable().Reverse())
            {
                try
                {
                    _logger.LogInformation("?? [SAGA-COMPENSATE] Step: {StepName} | OperationId: {OperationId}", 
                        step.StepName, operationId);
                    
                    var compensated = await step.CompensateAsync(data);
                    
                    if (compensated)
                    {
                        _logger.LogInformation("? [SAGA-COMPENSATE-SUCCESS] Step: {StepName} | OperationId: {OperationId}", 
                            step.StepName, operationId);
                    }
                    else
                    {
                        _logger.LogWarning("?? [SAGA-COMPENSATE-PARTIAL] Step: {StepName} | OperationId: {OperationId}", 
                            step.StepName, operationId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "?? [SAGA-COMPENSATE-ERROR] Step: {StepName} | OperationId: {OperationId}", 
                        step.StepName, operationId);
                }
            }

            _logger.LogInformation("?? [SAGA-COMPENSATE-END] OperationId: {OperationId}", operationId);
        }
    }
}
