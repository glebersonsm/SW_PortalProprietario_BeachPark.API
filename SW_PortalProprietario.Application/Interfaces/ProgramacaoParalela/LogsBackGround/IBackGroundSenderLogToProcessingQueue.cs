namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround
{
    public interface IBackGroundSenderLogToProcessingQueue
    {
        bool Stopped { get; set; }
    }
}
