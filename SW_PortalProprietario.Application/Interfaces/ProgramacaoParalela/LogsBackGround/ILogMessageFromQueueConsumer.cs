namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround
{
    public interface ILogMessageFromQueueConsumer
    {
        Task RegisterConsumerAndSaveLogFromQueue();
    }
}
