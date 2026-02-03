using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Interfaces.Saga
{
    /// <summary>
    /// Interface para um step de Saga
    /// </summary>
    public interface ISagaStep<TInput, TOutput>
    {
        /// <summary>
        /// Nome do step para identificação
        /// </summary>
        string StepName { get; }

        /// <summary>
        /// Ordem de execução
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Indica se este step pode ser compensado
        /// </summary>
        bool CanCompensate { get; }

        /// <summary>
        /// Executa o step
        /// </summary>
        Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);

        /// <summary>
        /// Compensa o step (desfaz a operação)
        /// </summary>
        Task CompensateAsync(TInput input, TOutput output, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface para repositório de Saga
    /// </summary>
    public interface ISagaRepository
    {
        /// <summary>
        /// Cria uma nova execução de Saga
        /// </summary>
        Task<SagaExecution> CreateSagaAsync(string operationType, string? inputData, string? metadata = null);

        /// <summary>
        /// Adiciona um step à Saga
        /// </summary>
        Task<SagaStep> AddStepAsync(string sagaId, string stepName, int order, bool canCompensate = true);

        /// <summary>
        /// Atualiza o status de um step
        /// </summary>
        Task UpdateStepStatusAsync(int stepId, string status, string? outputData = null, string? errorMessage = null, string? stackTrace = null);

        /// <summary>
        /// Atualiza o status da Saga
        /// </summary>
        Task UpdateSagaStatusAsync(string sagaId, string status, string? outputData = null, string? errorMessage = null);

        /// <summary>
        /// Obtém uma Saga pelo ID
        /// </summary>
        Task<SagaExecution?> GetSagaAsync(string sagaId);

        /// <summary>
        /// Obtém todos os steps de uma Saga
        /// </summary>
        Task<IList<SagaStep>> GetStepsAsync(string sagaId);

        /// <summary>
        /// Obtém Sagas por status
        /// </summary>
        Task<IList<SagaExecution>> GetSagasByStatusAsync(string status, int limit = 100);

        /// <summary>
        /// Obtém Sagas por tipo de operação
        /// </summary>
        Task<IList<SagaExecution>> GetSagasByOperationTypeAsync(string operationType, DateTime? dataInicio = null, DateTime? dataFim = null);
    }

    /// <summary>
    /// Interface para o orquestrador de Saga
    /// </summary>
    public interface ISagaOrchestrator
    {
        /// <summary>
        /// Executa uma Saga com os steps fornecidos
        /// </summary>
        Task<TSagaResult> ExecuteAsync<TSagaInput, TSagaResult>(
            string operationType,
            TSagaInput input,
            Func<TSagaInput, CancellationToken, Task<TSagaResult>> sagaLogic,
            CancellationToken cancellationToken = default)
            where TSagaResult : class;

        /// <summary>
        /// Obtém o ID da Saga atual
        /// </summary>
        string? CurrentSagaId { get; }

        /// <summary>
        /// Registra um step executado
        /// </summary>
        Task RegisterStepAsync<TInput, TOutput>(
            string stepName,
            int order,
            Func<TInput, CancellationToken, Task<TOutput>> executeFunc,
            Func<TInput, TOutput, CancellationToken, Task>? compensateFunc,
            TInput input,
            CancellationToken cancellationToken = default);
    }
}
