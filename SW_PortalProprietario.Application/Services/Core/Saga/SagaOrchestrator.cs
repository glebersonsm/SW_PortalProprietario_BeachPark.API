using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces.Saga;
using System.Diagnostics;
using System.Text.Json;

namespace SW_PortalProprietario.Application.Services.Core.Saga
{
    /// <summary>
    /// Orquestrador de Sagas - gerencia transações distribuídas com compensação automática
    /// </summary>
    public class SagaOrchestrator : ISagaOrchestrator
    {
        private readonly ISagaRepository _sagaRepository;
        private readonly ILogger<SagaOrchestrator> _logger;
        private readonly List<ExecutedStep> _executedSteps = new();
        private string? _currentSagaId;

        public string? CurrentSagaId => _currentSagaId;

        public SagaOrchestrator(
            ISagaRepository sagaRepository,
            ILogger<SagaOrchestrator> logger)
        {
            _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TSagaResult> ExecuteAsync<TSagaInput, TSagaResult>(
            string operationType,
            TSagaInput input,
            Func<TSagaInput, CancellationToken, Task<TSagaResult>> sagaLogic,
            CancellationToken cancellationToken = default)
            where TSagaResult : class
        {
            var stopwatch = Stopwatch.StartNew();
            TSagaResult? result = null;

            try
            {
                // Serializa input para log
                var inputJson = JsonSerializer.Serialize(input, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                // Cria a Saga
                var saga = await _sagaRepository.CreateSagaAsync(operationType, inputJson);
                _currentSagaId = saga.SagaId;

                _logger.LogInformation(
                    "🚀 Iniciando Saga {SagaId} - Tipo: {OperationType}",
                    _currentSagaId, operationType);

                // Executa a lógica da Saga
                result = await sagaLogic(input, cancellationToken);

                // Saga completada com sucesso
                var outputJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                await _sagaRepository.UpdateSagaStatusAsync(_currentSagaId, "Completed", outputJson);

                stopwatch.Stop();
                _logger.LogInformation(
                    "✅ Saga {SagaId} completada com sucesso em {Duration}ms - {StepCount} steps executados",
                    _currentSagaId, stopwatch.ElapsedMilliseconds, _executedSteps.Count);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "❌ Erro na Saga {SagaId} após {Duration}ms - Iniciando compensação de {StepCount} steps",
                    _currentSagaId, stopwatch.ElapsedMilliseconds, _executedSteps.Count);

                // Compensa os steps executados (em ordem reversa)
                await CompensateAsync(cancellationToken);

                // Atualiza status da Saga
                if (_currentSagaId != null)
                {
                    await _sagaRepository.UpdateSagaStatusAsync(
                        _currentSagaId,
                        "Compensated",
                        errorMessage: $"{ex.GetType().Name}: {ex.Message}");
                }

                throw new SagaException(
                    $"Saga {operationType} falhou e foi compensada. Erro: {ex.Message}",
                    _currentSagaId,
                    ex);
            }
            finally
            {
                // Limpa o contexto
                _executedSteps.Clear();
                _currentSagaId = null;
            }
        }

        public async Task RegisterStepAsync<TInput, TOutput>(
            string stepName,
            int order,
            Func<TInput, CancellationToken, Task<TOutput>> executeFunc,
            Func<TInput, TOutput, CancellationToken, Task>? compensateFunc,
            TInput input,
            CancellationToken cancellationToken = default)
        {
            if (_currentSagaId == null)
                throw new InvalidOperationException("Nenhuma Saga ativa. Execute uma Saga primeiro.");

            var stopwatch = Stopwatch.StartNew();
            TOutput? output = default;
            Domain.Entities.Core.Sistema.SagaStep? stepEntity = null;

            try
            {
                // Registra o step no banco
                stepEntity = await _sagaRepository.AddStepAsync(
                    _currentSagaId,
                    stepName,
                    order,
                    canCompensate: compensateFunc != null);

                // Marca como executando
                await _sagaRepository.UpdateStepStatusAsync(stepEntity.Id!.Value, "Executing");

                _logger.LogDebug(
                    "⚙️ Executando step {StepName} (Ordem: {Order}) - Saga {SagaId}",
                    stepName, order, _currentSagaId);

                // Executa o step
                output = await executeFunc(input, cancellationToken);

                stopwatch.Stop();

                // Serializa output
                var outputJson = JsonSerializer.Serialize(output, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                // Marca como executado
                await _sagaRepository.UpdateStepStatusAsync(
                    stepEntity.Id.Value,
                    "Executed",
                    outputData: outputJson);

                // Armazena para possível compensação
                _executedSteps.Add(new ExecutedStep
                {
                    StepId = stepEntity.Id.Value,
                    StepName = stepName,
                    Order = order,
                    Input = input!,
                    Output = output!,
                    CompensateFunc = compensateFunc != null
                        ? async (ct) => await compensateFunc(input, output, ct)
                        : null
                });

                _logger.LogDebug(
                    "✓ Step {StepName} executado com sucesso em {Duration}ms",
                    stepName, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "✗ Falha no step {StepName} após {Duration}ms",
                    stepName, stopwatch.ElapsedMilliseconds);

                // Marca como falho
                if (stepEntity?.Id != null)
                {
                    await _sagaRepository.UpdateStepStatusAsync(
                        stepEntity.Id.Value,
                        "Failed",
                        errorMessage: ex.Message,
                        stackTrace: ex.StackTrace);
                }

                throw;
            }
        }

        private async Task CompensateAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                "🔄 Iniciando compensação de {Count} steps executados",
                _executedSteps.Count);

            // Compensa em ordem reversa
            for (int i = _executedSteps.Count - 1; i >= 0; i--)
            {
                var step = _executedSteps[i];

                if (step.CompensateFunc == null)
                {
                    _logger.LogWarning(
                        "⚠️ Step {StepName} não possui função de compensação - pulando",
                        step.StepName);
                    continue;
                }

                var stopwatch = Stopwatch.StartNew();

                try
                {
                    // Marca como compensando
                    await _sagaRepository.UpdateStepStatusAsync(step.StepId, "Compensating");

                    _logger.LogDebug(
                        "↩️ Compensando step {StepName} (Ordem: {Order})",
                        step.StepName, step.Order);

                    // Executa compensação
                    await step.CompensateFunc(cancellationToken);

                    stopwatch.Stop();

                    // Marca como compensado
                    await _sagaRepository.UpdateStepStatusAsync(step.StepId, "Compensated");

                    _logger.LogDebug(
                        "✓ Step {StepName} compensado com sucesso em {Duration}ms",
                        step.StepName, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    _logger.LogError(ex,
                        "❌ Falha ao compensar step {StepName} após {Duration}ms - Continuando com próximo step",
                        step.StepName, stopwatch.ElapsedMilliseconds);

                    // Marca falha na compensação mas continua
                    await _sagaRepository.UpdateStepStatusAsync(
                        step.StepId,
                        "CompensationFailed",
                        errorMessage: ex.Message,
                        stackTrace: ex.StackTrace);
                }
            }

            _logger.LogInformation(
                "✅ Compensação concluída - {Count} steps processados",
                _executedSteps.Count);
        }

        private class ExecutedStep
        {
            public int StepId { get; set; }
            public string StepName { get; set; } = string.Empty;
            public int Order { get; set; }
            public object Input { get; set; } = null!;
            public object Output { get; set; } = null!;
            public Func<CancellationToken, Task>? CompensateFunc { get; set; }
        }
    }

    /// <summary>
    /// Exceção específica para falhas em Sagas
    /// </summary>
    public class SagaException : Exception
    {
        public string? SagaId { get; }

        public SagaException(string message, string? sagaId, Exception? innerException = null)
            : base(message, innerException)
        {
            SagaId = sagaId;
        }
    }
}
