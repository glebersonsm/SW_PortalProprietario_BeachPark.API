namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email
{
    public interface IEmailSenderFromQueueConsumer
    {
        Task RegisterAndSendEmailFromQueue();
    }
}
