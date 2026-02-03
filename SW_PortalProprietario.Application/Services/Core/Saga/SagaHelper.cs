using SW_PortalProprietario.Application.Interfaces.Saga;

namespace SW_PortalProprietario.Application.Services.Core.Saga
{
    /// <summary>
    /// Classe auxiliar para facilitar o uso de Sagas
    /// </summary>
    public static class SagaHelper
    {
        /// <summary>
        /// Executa um step dentro de uma Saga
        /// </summary>
        public static async Task<TOutput> ExecuteStepAsync<TInput, TOutput>(
            this ISagaOrchestrator orchestrator,
            string stepName,
            int order,
            TInput input,
            Func<TInput, CancellationToken, Task<TOutput>> executeFunc,
            Func<TInput, TOutput, CancellationToken, Task>? compensateFunc = null,
            CancellationToken cancellationToken = default)
        {
            TOutput? result = default;

            await orchestrator.RegisterStepAsync(
                stepName,
                order,
                async (inp, ct) =>
                {
                    result = await executeFunc(inp, ct);
                    return result;
                },
                compensateFunc,
                input,
                cancellationToken);

            return result!;
        }

        /// <summary>
        /// Executa um step sem retorno dentro de uma Saga
        /// </summary>
        public static async Task ExecuteStepAsync<TInput>(
            this ISagaOrchestrator orchestrator,
            string stepName,
            int order,
            TInput input,
            Func<TInput, CancellationToken, Task> executeFunc,
            Func<TInput, CancellationToken, Task>? compensateFunc = null,
            CancellationToken cancellationToken = default)
        {
            await orchestrator.RegisterStepAsync<TInput, bool>(
                stepName,
                order,
                async (inp, ct) =>
                {
                    await executeFunc(inp, ct);
                    return true;
                },
                compensateFunc != null
                    ? async (inp, _, ct) => await compensateFunc(inp, ct)
                    : null,
                input,
                cancellationToken);
        }
    }

    /// <summary>
    /// Builder fluente para construir Sagas
    /// </summary>
    public class SagaBuilder<TInput>
    {
        private readonly ISagaOrchestrator _orchestrator;
        private readonly string _operationType;
        private readonly TInput _input;
        private readonly List<StepDefinition> _steps = new();
        private int _currentOrder = 0;

        public SagaBuilder(ISagaOrchestrator orchestrator, string operationType, TInput input)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _operationType = operationType ?? throw new ArgumentNullException(nameof(operationType));
            _input = input;
        }

        /// <summary>
        /// Adiciona um step à Saga
        /// </summary>
        public SagaBuilder<TInput> AddStep<TOutput>(
            string stepName,
            Func<TInput, CancellationToken, Task<TOutput>> executeFunc,
            Func<TInput, TOutput, CancellationToken, Task>? compensateFunc = null)
        {
            _steps.Add(new StepDefinition
            {
                StepName = stepName,
                Order = ++_currentOrder,
                ExecuteFunc = async (input, ct) =>
                {
                    var result = await executeFunc((TInput)input, ct);
                    return result!;
                },
                CompensateFunc = compensateFunc != null
                    ? async (input, output, ct) => await compensateFunc((TInput)input, (TOutput)output, ct)
                    : null
            });

            return this;
        }

        /// <summary>
        /// Adiciona um step sem retorno à Saga
        /// </summary>
        public SagaBuilder<TInput> AddStep(
            string stepName,
            Func<TInput, CancellationToken, Task> executeFunc,
            Func<TInput, CancellationToken, Task>? compensateFunc = null)
        {
            _steps.Add(new StepDefinition
            {
                StepName = stepName,
                Order = ++_currentOrder,
                ExecuteFunc = async (input, ct) =>
                {
                    await executeFunc((TInput)input, ct);
                    return true;
                },
                CompensateFunc = compensateFunc != null
                    ? async (input, _, ct) => await compensateFunc((TInput)input, ct)
                    : null
            });

            return this;
        }

        /// <summary>
        /// Executa a Saga
        /// </summary>
        public async Task<TResult> ExecuteAsync<TResult>(
            Func<TInput, Task<TResult>> finalFunc,
            CancellationToken cancellationToken = default)
            where TResult : class
        {
            return await _orchestrator.ExecuteAsync(
                _operationType,
                _input,
                async (input, ct) =>
                {
                    // Executa todos os steps
                    foreach (var step in _steps)
                    {
                        await _orchestrator.RegisterStepAsync<object, object>(
                            step.StepName,
                            step.Order,
                            step.ExecuteFunc,
                            step.CompensateFunc,
                            input!,
                            ct);
                    }

                    // Executa função final
                    return await finalFunc(input);
                },
                cancellationToken);
        }

        private class StepDefinition
        {
            public string StepName { get; set; } = string.Empty;
            public int Order { get; set; }
            public Func<object, CancellationToken, Task<object>> ExecuteFunc { get; set; } = null!;
            public Func<object, object, CancellationToken, Task>? CompensateFunc { get; set; }
        }
    }

    /// <summary>
    /// Extensões para criar SagaBuilder
    /// </summary>
    public static class SagaBuilderExtensions
    {
        public static SagaBuilder<TInput> CreateSaga<TInput>(
            this ISagaOrchestrator orchestrator,
            string operationType,
            TInput input)
        {
            return new SagaBuilder<TInput>(orchestrator, operationType, input);
        }
    }
}
