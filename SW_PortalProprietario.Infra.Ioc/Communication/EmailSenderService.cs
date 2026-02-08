using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces.Communication;
using System.Diagnostics;

namespace SW_PortalProprietario.Infra.Ioc.Communication
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;
        private readonly ISmtpSettingsProvider _smtpSettingsProvider;

        public EmailSenderService(IConfiguration configuration,
            ILogger<EmailSenderService> loger,
            ISmtpSettingsProvider smtpSettingsProvider)
        {
            _configuration = configuration;
            _logger = loger;
            _smtpSettingsProvider = smtpSettingsProvider;
        }

        public async Task Send(EmailModel model)
        {
            if (string.IsNullOrEmpty(model.Destinatario) || !model.Destinatario.Contains("@"))
                throw new ArgumentException("Deve ser informado o destinatário do email.");

            if (string.IsNullOrEmpty(model.Assunto))
                throw new ArgumentException("Deve ser informado o assunto do email.");

            if (string.IsNullOrEmpty(model.ConteudoEmail))
                throw new ArgumentException("Deve ser informado o conteúdo do email (EmailContent).");

            if (model.Id.GetValueOrDefault() == 0)
                throw new ArgumentException("O Email deve ser persistido no banco de dados antes de ser enviado ao cliente.");

            var html = InjectTrackingPixelIfConfigured(model.ConteudoEmail, model.Id.GetValueOrDefault());
            await Send(model.Destinatario, model.Assunto, html);

        }

        private string InjectTrackingPixelIfConfigured(string html, int emailId)
        {
            var baseUrl = _configuration.GetValue<string>("EmailTrackingBaseUrl");
            if (string.IsNullOrWhiteSpace(baseUrl) || emailId <= 0) return html;
            baseUrl = baseUrl.TrimEnd('/');
            var pixel = $"<img src=\"{baseUrl}/Email/track/open?id={emailId}\" width=\"1\" height=\"1\" alt=\"\" style=\"display:block;\" />";
            return html + pixel;
        }

        private async Task Send(string destinatario, string assunto, string html)
        {
            try
            {
                if (string.IsNullOrEmpty(destinatario))
                {
                    destinatario = _configuration.GetValue<string>("SmtpEmailTo");
                }

                //if (Debugger.IsAttached)
                //    destinatario = "glebersonsm@gmail.com";

                if (string.IsNullOrEmpty(destinatario) || !destinatario.Contains("@"))
                    throw new ArgumentException("Deve ser informado o destinatário do email.");

                var smtpFromParams = await _smtpSettingsProvider.GetSmtpSettingsFromParametroSistemaAsync();
                string host;
                string remetente;
                string pass;
                int porta;
                bool useSsl;
                string? fromName = null;
                if (smtpFromParams != null)
                {
                    host = smtpFromParams.Host;
                    remetente = smtpFromParams.User;
                    pass = smtpFromParams.Pass;
                    porta = smtpFromParams.Port;
                    useSsl = smtpFromParams.UseSsl;
                    fromName = smtpFromParams.FromName;
                }
                else
                {
                    host = _configuration.GetValue<string>("SmtpHost") ?? "";
                    remetente = _configuration.GetValue<string>("SmtpUser") ?? "";
                    pass = _configuration.GetValue<string>("SmtpPass") ?? "";
                    porta = _configuration.GetValue<int>("SmtpPort", 0);
                    useSsl = _configuration.GetValue<string>("SmtpUseSsl") == "S";
                    fromName = _configuration.GetValue<string>("SmtpFromName");
                }

                if (string.IsNullOrEmpty(host))
                    throw new ArgumentException("Deve ser informado o host para envio do email (Parâmetro: 'SmtpHost' ou configuração do sistema).");
                if (string.IsNullOrEmpty(remetente))
                    throw new ArgumentException("Deve ser informado o remetente do email (Parâmetro: 'SmtpUser' ou configuração do sistema).");
                if (string.IsNullOrEmpty(pass))
                    throw new ArgumentException("Deve ser informada a senha do remetente do email (Parâmetro: 'SmtpPass' ou configuração do sistema).");
                if (porta == 0)
                    throw new ArgumentException("Deve ser informada a porta de saída do email (Parâmetro: 'SmtpPort' ou configuração do sistema).");

                // create message
                var email = new MimeMessage();
                email.From.Add(!string.IsNullOrWhiteSpace(fromName)
                    ? new MailboxAddress(fromName.Trim(), remetente)
                    : MailboxAddress.Parse(remetente));

                foreach (var item in destinatario.Split(";"))
                {
                    email.To.Add(MailboxAddress.Parse(item.TrimEnd().TrimStart().Replace(";","")));
                }
                email.Subject = assunto;
                email.Body = new TextPart(TextFormat.Html) { Text = html };

                // send email
                using var smtp = new SmtpClient();
                smtp.Connect(host, porta, (useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls));
                smtp.Authenticate(remetente, password: pass);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (Exception err)
            {
                _logger.LogError($"Não foi possível enviar o email com o assunto: {assunto}");
                throw err;
            }
        }
    }
}
