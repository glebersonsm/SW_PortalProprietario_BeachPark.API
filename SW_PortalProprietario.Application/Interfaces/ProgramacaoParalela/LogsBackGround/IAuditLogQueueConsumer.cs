namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround
{
    public interface IAuditLogQueueConsumer
    {
        /// <summary>Indica se o consumer estÃ¡ em execuÃ§Ã£o (conectado e consumindo a fila).</summary>
        bool IsRunning { get; }
        /// <summary>Registra o consumer na fila de auditoria (sÃ³ inicia se a fila estiver Ativa no painel).</summary>
        Task RegisterConsumerAndSaveAuditLogFromQueue();
        /// <summary>Para o consumer e libera conexÃ£o/canal com o RabbitMQ.</summary>
        Task StopConsumerAsync();
    }
}

