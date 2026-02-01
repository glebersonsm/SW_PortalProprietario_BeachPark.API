using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using System.Text.Json;

namespace SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing
{
    /// <summary>
    /// Step 2: Gravação de log no sistema PostgreSQL (Portal)
    /// Registra a tentativa de operação para auditoria
    /// </summary>
    public class GravacaoLogPortalStep : IDistributedTransactionStep
    {
        private readonly IRepositoryNH _repositorySystem;
        private readonly ILogger<GravacaoLogPortalStep> _logger;
        private readonly string _operationId;
        private readonly string _operationType;
        private readonly object _requestData;

        public string StepName => "GravacaoLogPortal";
        public int Order => 2;

        public GravacaoLogPortalStep(
            IRepositoryNH repositorySystem,
            ILogger<GravacaoLogPortalStep> logger,
            string operationId,
            string operationType,
            object requestData)
        {
            _repositorySystem = repositorySystem;
            _logger = logger;
            _operationId = operationId;
            _operationType = operationType;
            _requestData = requestData;
        }

        public async Task<(bool Success, string ErrorMessage, object? Data)> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("[GravacaoLogPortal] Iniciando gravação de log - OperationId: {OperationId}", _operationId);

                _repositorySystem.BeginTransaction();

                var loggedUser = await _repositorySystem.GetLoggedUser();
                var userId = loggedUser.HasValue ? int.Parse(loggedUser.Value.userId) : (int?)null;

                var transactionLog = new DistributedTransactionLog
                {
                    OperationId = _operationId,
                    OperationType = _operationType,
                    StepName = StepName,
                    StepOrder = Order,
                    Status = "Executed",
                    Payload = JsonSerializer.Serialize(_requestData),
                    DataHoraCriacao = DateTime.Now,
                    UsuarioCriacao = userId
                };

                await _repositorySystem.Save(transactionLog);
                
                var commitResult = await _repositorySystem.CommitAsync();
                
                if (!commitResult.executed)
                {
                    throw commitResult.exception ?? new Exception("Falha ao commitar transação");
                }

                _logger.LogInformation("[GravacaoLogPortal] Log gravado com sucesso - ID: {LogId}", transactionLog.Id);

                return (true, string.Empty, transactionLog.Id);
            }
            catch (Exception ex)
            {
                _repositorySystem.Rollback();
                _logger.LogError(ex, "[GravacaoLogPortal] Erro ao gravar log");
                return (false, $"Falha ao gravar log: {ex.Message}", null);
            }
        }

        public async Task<bool> CompensateAsync(object? executionData)
        {
            try
            {
                if (executionData is int logId)
                {
                    _logger.LogInformation("[GravacaoLogPortal] Compensando - atualizando status do log ID: {LogId}", logId);

                    _repositorySystem.BeginTransaction();

                    var log = (await _repositorySystem.FindByHql<DistributedTransactionLog>(
                        $"From DistributedTransactionLog dtl Where dtl.Id = {logId}")).FirstOrDefault();

                    if (log != null)
                    {
                        log.Status = "Compensated";
                        log.DataHoraCompensacao = DateTime.Now;
                        await _repositorySystem.Save(log);
                    }

                    var commitResult = await _repositorySystem.CommitAsync();
                    
                    if (commitResult.executed)
                    {
                        _logger.LogInformation("[GravacaoLogPortal] Compensação concluída");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GravacaoLogPortal] Erro ao compensar");
                return false;
            }
        }
    }
}
