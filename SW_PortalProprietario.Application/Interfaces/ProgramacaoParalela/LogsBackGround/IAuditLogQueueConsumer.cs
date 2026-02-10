namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround
{
    public interface IAuditLogQueueConsumer
    {
        /// <summary>Indica se o consumer está em execução (conectado e consumindo a fila).</summary>
        bool IsRunning { get; }
        /// <summary>Registra o consumer na fila de auditoria (só inicia se a fila estiver Ativa no painel).</summary>
        Task RegisterConsumerAndSaveAuditLogFromQueue();
        /// <summary>Para o consumer e libera conexão/canal com o RabbitMQ.</summary>
        Task StopConsumerAsync();
    }
}

