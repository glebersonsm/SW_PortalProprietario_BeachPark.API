using SW_PortalProprietario.Application.Models.GeralModels;
namespace SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email
{
    public interface ISenderEmailToQueueProducer
    {
        Task AddEmailMessageToQueue(EmailModel model);
    }
}
