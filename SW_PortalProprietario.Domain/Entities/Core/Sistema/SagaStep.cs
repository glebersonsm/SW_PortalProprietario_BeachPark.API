namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    /// <summary>
    /// Representa um passo individual dentro de uma Saga
    /// </summary>
    public class SagaStep : EntityBaseCore
    {
        public virtual int? Id { get; set; }

        /// <summary>
        /// ReferÃªncia para a Saga pai
        /// </summary>
        public virtual SagaExecution? SagaExecution { get; set; }

        /// <summary>
        /// Nome do step (Ex: "ValidarDisponibilidade", "ReservarQuarto")
        /// </summary>
        public virtual string StepName { get; set; } = string.Empty;

        /// <summary>
        /// Ordem de execuÃ§Ã£o (1, 2, 3...)
        /// </summary>
        public virtual int StepOrder { get; set; }

        /// <summary>
        /// Status: "Pending", "Executing", "Executed", "Compensating", "Compensated", "Failed"
        /// </summary>
        public virtual string Status { get; set; } = "Pending";

        /// <summary>
        /// Dados de entrada do step (JSON)
        /// </summary>
        public virtual string? InputData { get; set; }

        /// <summary>
        /// Resultado do step (JSON)
        /// </summary>
        public virtual string? OutputData { get; set; }

        /// <summary>
        /// Mensagem de erro se falhou
        /// </summary>
        public virtual string? ErrorMessage { get; set; }

        /// <summary>
        /// Stack trace se falhou
        /// </summary>
        public virtual string? StackTrace { get; set; }

        /// <summary>
        /// Data/hora de inÃ­cio da execuÃ§Ã£o
        /// </summary>
        public virtual DateTime? DataHoraInicio { get; set; }

        /// <summary>
        /// Data/hora de conclusÃ£o
        /// </summary>
        public virtual DateTime? DataHoraConclusao { get; set; }

        /// <summary>
        /// DuraÃ§Ã£o em milissegundos
        /// </summary>
        public virtual long? DuracaoMs { get; set; }

        /// <summary>
        /// Data/hora de inÃ­cio da compensaÃ§Ã£o
        /// </summary>
        public virtual DateTime? DataHoraInicioCompensacao { get; set; }

        /// <summary>
        /// Data/hora de conclusÃ£o da compensaÃ§Ã£o
        /// </summary>
        public virtual DateTime? DataHoraConclusaoCompensacao { get; set; }

        /// <summary>
        /// DuraÃ§Ã£o da compensaÃ§Ã£o em milissegundos
        /// </summary>
        public virtual long? DuracaoCompensacaoMs { get; set; }

        /// <summary>
        /// NÃºmero de tentativas de execuÃ§Ã£o
        /// </summary>
        public virtual int Tentativas { get; set; } = 0;

        /// <summary>
        /// NÃºmero de tentativas de compensaÃ§Ã£o
        /// </summary>
        public virtual int TentativasCompensacao { get; set; } = 0;

        /// <summary>
        /// Indica se o step pode ser compensado
        /// </summary>
        public virtual bool PodeSerCompensado { get; set; } = true;

        /// <summary>
        /// Dados adicionais para debug
        /// </summary>
        public virtual string? Metadata { get; set; }
    }
}
