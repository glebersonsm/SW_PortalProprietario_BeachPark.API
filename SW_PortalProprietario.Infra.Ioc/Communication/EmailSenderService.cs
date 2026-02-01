using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces.Communication;
using System.Diagnostics;

namespace SW_PortalProprietario.Infra.Ioc.Communication
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IConfiguration configuration,
            ILogger<EmailSenderService> loger)
        {
            _configuration = configuration;
            _logger = loger;
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

            await Send(model.Destinatario, model.Assunto, model.ConteudoEmail);

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

                var host = _configuration.GetValue<string>("SmtpHost");
                if (string.IsNullOrEmpty(host))
                    throw new ArgumentException("Deve ser informado o host para envio do email (Parâmetro: 'SmtpHost').");

                var remetente = _configuration.GetValue<string>("SmtpUser");
                if (string.IsNullOrEmpty(remetente))
                    throw new ArgumentException("Deve ser informado o remetente do email (Parâmetro: 'SmtpUser').");

                var pass = _configuration.GetValue<string>("SmtpPass");
                if (string.IsNullOrEmpty(pass))
                    throw new ArgumentException("Deve ser informada a senha do remetente do email (Parâmetro: 'SmtpPass').");

                var porta = _configuration.GetValue<int>("SmtpPort");
                if (porta == 0)
                    throw new ArgumentException("Deve ser informada a porta de saída do email (Parâmetro: 'SmtpPort').");

                // create message
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(remetente));
                var useSsl = _configuration.GetValue<string>("SmtpUseSsl") == "S";

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
