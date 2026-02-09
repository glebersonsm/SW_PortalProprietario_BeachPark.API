using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces.Communication
{
    public interface IEmailSenderHostedService
    {
        Task Send(EmailModel model);
        Task SendViaSystemNetMailStaticAsync(
            string destinatario,
            string assunto,
            string html,
            List<EmailAnexoModel>? anexos,
            string host,
            int porta,
            bool useSsl,
            string remetente,
            string pass,
            string? fromName);
        Task SendViaMailKitAsync(
            string destinatario,
            string assunto,
            string html,
            List<EmailAnexoModel>? anexos,
            string host,
            int porta,
            bool useSsl,
            string remetente,
            string pass,
            string? fromName);
    }
}
