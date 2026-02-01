namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    /// <summary>
    /// Interface para definir um step de transação distribuída no padrão Saga
    /// </summary>
    public interface IDistributedTransactionStep
    {
        /// <summary>
        /// Nome do step para identificação e logs
        /// </summary>
        string StepName { get; }

        /// <summary>
        /// Ordem de execução do step (menor = primeiro)
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Executa o step da transação
        /// </summary>
        /// <returns>
        /// Tupla contendo:
        /// - Success: true se executado com sucesso
        /// - ErrorMessage: mensagem de erro caso Success = false
        /// - Data: dados necessários para compensação posterior
        /// </returns>
        Task<(bool Success, string ErrorMessage, object? Data)> ExecuteAsync();

        /// <summary>
        /// Executa a compensação (rollback) do step em caso de falha
        /// </summary>
        /// <param name="executionData">Dados retornados durante a execução</param>
        /// <returns>true se compensado com sucesso</returns>
        Task<bool> CompensateAsync(object? executionData);
    }
}
