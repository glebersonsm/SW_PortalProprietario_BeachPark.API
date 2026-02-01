namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    /// <summary>
    /// Entidade para rastrear transações distribuídas (Saga Pattern)
    /// Permite auditoria e investigação de problemas
    /// </summary>
    public class DistributedTransactionLog
    {
        public virtual int? Id { get; set; }
        
        /// <summary>
        /// ID único da operação (GUID)
        /// </summary>
        public virtual string OperationId { get; set; }
        
        /// <summary>
        /// Tipo da operação (Ex: "ReservaTimeSharing", "LiberacaoPool", "TrocaPeriodo")
        /// </summary>
        public virtual string OperationType { get; set; }
        
        /// <summary>
        /// Nome do step executado
        /// </summary>
        public virtual string StepName { get; set; }
        
        /// <summary>
        /// Ordem de execução do step
        /// </summary>
        public virtual int StepOrder { get; set; }
        
        /// <summary>
        /// Status: "Executed", "Compensated", "Failed", "Pending"
        /// </summary>
        public virtual string Status { get; set; }
        
        /// <summary>
        /// Dados da execução serializado em JSON
        /// </summary>
        public virtual string? Payload { get; set; }
        
        /// <summary>
        /// Mensagem de erro se Status = "Failed"
        /// </summary>
        public virtual string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Data/hora de criação do registro
        /// </summary>
        public virtual DateTime DataHoraCriacao { get; set; }
        
        /// <summary>
        /// Data/hora da compensação (se houver)
        /// </summary>
        public virtual DateTime? DataHoraCompensacao { get; set; }
        
        /// <summary>
        /// Usuário que iniciou a operação
        /// </summary>
        public virtual int? UsuarioCriacao { get; set; }
        
        /// <summary>
        /// Dados adicionais para debug
        /// </summary>
        public virtual string? AdditionalData { get; set; }
    }
}
