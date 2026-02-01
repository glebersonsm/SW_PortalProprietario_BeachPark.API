namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround
{
    public interface IBackGroundSaveLogFromProcessingQueue
    {
        bool Stopped { get; set; }
    }
}
