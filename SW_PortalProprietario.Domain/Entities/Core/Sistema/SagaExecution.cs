namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    /// <summary>
    /// Representa uma execução completa de uma Saga (transação distribuída)
    /// </summary>
    public class SagaExecution : EntityBaseCore
    {
        /// <summary>
        /// ID único da Saga (GUID)
        /// </summary>
        public virtual string SagaId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo da operação (Ex: "ReservaTimeSharing", "PagamentoCartao")
        /// </summary>
        public virtual string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Status geral da Saga: "Running", "Completed", "Compensated", "Failed"
        /// </summary>
        public virtual string Status { get; set; } = "Running";

        /// <summary>
        /// Dados de entrada da operação (JSON)
        /// </summary>
        public virtual string? InputData { get; set; }

        /// <summary>
        /// Resultado final da operação (JSON)
        /// </summary>
        public virtual string? OutputData { get; set; }

        /// <summary>
        /// Mensagem de erro se Status = "Failed"
        /// </summary>
        public virtual string? ErrorMessage { get; set; }

        /// <summary>
        /// Data/hora de início
        /// </summary>
        public virtual DateTime DataHoraInicio { get; set; }

        /// <summary>
        /// Data/hora de conclusão
        /// </summary>
        public virtual DateTime? DataHoraConclusao { get; set; }

        /// <summary>
        /// Duração total em milissegundos
        /// </summary>
        public virtual long? DuracaoMs { get; set; }

        /// <summary>
        /// Usuário que iniciou a operação
        /// </summary>
        public virtual int? UsuarioId { get; set; }

        /// <summary>
        /// Endpoint da API que iniciou a Saga
        /// </summary>
        public virtual string? Endpoint { get; set; }

        /// <summary>
        /// IP do cliente
        /// </summary>
        public virtual string? ClientIp { get; set; }

        /// <summary>
        /// Dados adicionais para contexto
        /// </summary>
        public virtual string? Metadata { get; set; }

        /// <summary>
        /// Steps executados nesta Saga
        /// </summary>
        public virtual IList<SagaStep> Steps { get; set; } = new List<SagaStep>();
    }
}
