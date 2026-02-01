namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround
{
    public interface IAuditLogQueueConsumer
    {
        Task RegisterConsumerAndSaveAuditLogFromQueue();
    }
}

