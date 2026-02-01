namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email
{
    public interface IBackGroundSenderEmailFromProcessingQueue
    {
        bool Stopped { get; set; }
    }
}
