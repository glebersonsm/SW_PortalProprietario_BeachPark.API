using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces.Communication;
using SW_PortalProprietario.Domain.Enumns;
using System.Net;
using System.Net.Mail;

namespace SW_PortalProprietario.Infra.Ioc.Communication
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;
        private readonly ISmtpSettingsProvider _smtpSettingsProvider;
        private readonly IParametroSistemaService _parametroSistemaService;

        public EmailSenderService(IConfiguration configuration,
            ILogger<EmailSenderService> loger,
            ISmtpSettingsProvider smtpSettingsProvider,
            IParametroSistemaService parametroSistemaService)
        {
            _configuration = configuration;
            _logger = loger;
            _smtpSettingsProvider = smtpSettingsProvider;
            _parametroSistemaService = parametroSistemaService;
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

                var ctx = await _smtpSettingsProvider.GetSmtpContextAsync();
                string host;
                string remetente;
                string pass;
                int porta;
                bool useSsl;
                string? fromName = null;
                if (ctx.Settings != null)
                {
                    host = ctx.Settings.Host;
                    remetente = ctx.Settings.User;
                    pass = ctx.Settings.Pass;
                    porta = ctx.Settings.Port;
                    useSsl = ctx.Settings.UseSsl;
                    fromName = ctx.Settings.FromName;
                }
                else
                {
                    host = _configuration.GetValue<string>("SmtpHost") ?? "";
                    remetente = _configuration.GetValue<string>("SmtpUser") ?? "";
                    pass = _configuration.GetValue<string>("SmtpPass") ?? _configuration.GetValue<string>("SmptPass") ?? "";
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

                var preferSystemNetMail = ctx.TipoEnvioEmail == EnumTipoEnvioEmail.ClienteEmailApp;
                Exception? firstException = null;

                // Tentativa pelo método configurado
                try
                {
                    if (preferSystemNetMail)
                        await EmailSenderHostedService.SendViaSystemNetMailStaticAsync(destinatario, assunto, html, null, host, porta, useSsl, remetente, pass, fromName);
                    else
                        await SendViaMailKitAsync(destinatario, assunto, html, host, porta, useSsl, remetente, pass, fromName);
                    return;
                }
                catch (Exception ex)
                {
                    firstException = ex;
                    _logger.LogWarning(ex, "Falha no envio por {Metodo}. Tentando método alternativo.", preferSystemNetMail ? "System.Net.Mail" : "MailKit");
                }

                // Fallback: tentar pelo outro método
                try
                {
                    if (preferSystemNetMail)
                        await SendViaMailKitAsync(destinatario, assunto, html, host, porta, useSsl, remetente, pass, fromName);
                    else
                        await EmailSenderHostedService.SendViaSystemNetMailStaticAsync(destinatario, assunto, html, null, host, porta, useSsl, remetente, pass, fromName);
                    _logger.LogInformation("Email enviado com sucesso pelo método alternativo (assunto: {Assunto}).", assunto);
                    if (ctx.Settings != null)
                    {
                        var tipoQueFuncionou = preferSystemNetMail ? EnumTipoEnvioEmail.ClienteEmailDireto : EnumTipoEnvioEmail.ClienteEmailApp;
                        try
                        {
                            await _parametroSistemaService.UpdateTipoEnvioEmailOnlyAsync(tipoQueFuncionou);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Não foi possível atualizar TipoEnvioEmail nos parâmetros do sistema.");
                        }
                    }
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Falha também no método alternativo (assunto: {Assunto}).", assunto);
                    throw firstException != null
                        ? new InvalidOperationException("Envio falhou pelo método configurado e pelo método alternativo.", new AggregateException(firstException, ex2))
                        : ex2;
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Não foi possível enviar o email com o assunto: {Assunto}", assunto);
                throw;
            }
        }

        private static async Task SendViaMailKitAsync(
            string destinatario,
            string assunto,
            string html,
            string host,
            int porta,
            bool useSsl,
            string remetente,
            string pass,
            string? fromName)
        {
            var email = new MimeMessage();
            email.From.Add(!string.IsNullOrWhiteSpace(fromName)
                ? new MailboxAddress(fromName.Trim(), remetente)
                : MailboxAddress.Parse(remetente));
            foreach (var item in destinatario.Split(";"))
                email.To.Add(MailboxAddress.Parse(item.TrimEnd().TrimStart().Replace(";", "")));
            email.Subject = assunto;
            email.Body = new TextPart(TextFormat.Html) { Text = html };

            // Porta 465 = SSL implícito. Porta 587 (ex.: Gmail) = STARTTLS.
            var secureOption = (porta == 465) ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(host, porta, secureOption);
            smtp.Authenticate(remetente, password: pass);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
