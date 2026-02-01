using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces.Communication
{
    public interface IEmailSenderHostedService
    {
        Task Send(EmailModel model);
    }
}
