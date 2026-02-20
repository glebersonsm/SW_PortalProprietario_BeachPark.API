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
        private readonly IEmailSenderHostedService _emailSenderHostedService;

        public EmailSenderService(IConfiguration configuration,
            ILogger<EmailSenderService> loger,
            ISmtpSettingsProvider smtpSettingsProvider,
            IParametroSistemaService parametroSistemaService,
            IEmailSenderHostedService emailSenderHostedService)
        {
            _configuration = configuration;
            _logger = loger;
            _smtpSettingsProvider = smtpSettingsProvider;
            _parametroSistemaService = parametroSistemaService;
            _emailSenderHostedService = emailSenderHostedService;
        }

        public async Task Send(EmailModel model)
        {
            if (string.IsNullOrEmpty(model.Destinatario) || !model.Destinatario.Contains("@"))
                throw new ArgumentException("Deve ser informado o destinatÃ¡rio do email.");

            if (string.IsNullOrEmpty(model.Assunto))
                throw new ArgumentException("Deve ser informado o assunto do email.");

            if (string.IsNullOrEmpty(model.ConteudoEmail))
                throw new ArgumentException("Deve ser informado o conteÃºdo do email (EmailContent).");

            if (model.Id.GetValueOrDefault() == 0)
                throw new ArgumentException("O Email deve ser persistido no banco de dados antes de ser enviado ao cliente.");

            var ctx = await _smtpSettingsProvider.GetSmtpContextAsync();
            var trackingBaseUrl = !string.IsNullOrWhiteSpace(ctx.EmailTrackingBaseUrl) ? ctx.EmailTrackingBaseUrl.Trim() : _configuration.GetValue<string>("EmailTrackingBaseUrl");
            var html = InjectTrackingPixelIfConfigured(model.ConteudoEmail, model.Id.GetValueOrDefault(), trackingBaseUrl);
            await Send(model.Destinatario, model.Assunto, html);

        }

        private static string InjectTrackingPixelIfConfigured(string html, int emailId, string? baseUrl)
        {
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
                    throw new ArgumentException("Deve ser informado o destinatÃ¡rio do email.");

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
                    throw new ArgumentException("Deve ser informado o host para envio do email (ParÃ¢metro: 'SmtpHost' ou configuraÃ§Ã£o do sistema).");
                if (string.IsNullOrEmpty(remetente))
                    throw new ArgumentException("Deve ser informado o remetente do email (ParÃ¢metro: 'SmtpUser' ou configuraÃ§Ã£o do sistema).");
                if (string.IsNullOrEmpty(pass))
                    throw new ArgumentException("Deve ser informada a senha do remetente do email (ParÃ¢metro: 'SmtpPass' ou configuraÃ§Ã£o do sistema).");
                if (porta == 0)
                    throw new ArgumentException("Deve ser informada a porta de saÃ­da do email (ParÃ¢metro: 'SmtpPort' ou configuraÃ§Ã£o do sistema).");

                // AWS SES: envio direto via MailKit sem fallback (credenciais especÃ­ficas da AWS)
                if (ctx.TipoEnvioEmail == EnumTipoEnvioEmail.AwsSes)
                {
                    _logger.LogInformation("Enviando email via AWS SES SMTP. Host: {Host}, Porta: {Porta}, User: {User}, Assunto: {Assunto}", host, porta, remetente, assunto);
                    await SendViaMailKitAsync(destinatario, assunto, html, host, porta, useSsl, remetente, pass, fromName);
                    return;
                }

                var preferSystemNetMail = ctx.TipoEnvioEmail == EnumTipoEnvioEmail.ClienteEmailApp;
                Exception? firstException = null;

                // Tentativa pelo mÃ©todo configurado
                try
                {
                    if (preferSystemNetMail)
                        await _emailSenderHostedService.SendViaSystemNetMailStaticAsync(destinatario, assunto, html, null, host, porta, useSsl, remetente, pass, fromName);
                    else
                        await SendViaMailKitAsync(destinatario, assunto, html, host, porta, useSsl, remetente, pass, fromName);
                    return;
                }
                catch (Exception ex)
                {
                    firstException = ex;
                    _logger.LogWarning(ex, "Falha no envio por {Metodo}. Tentando mÃ©todo alternativo.", preferSystemNetMail ? "System.Net.Mail" : "MailKit");
                }

                // Fallback: tentar pelo outro mÃ©todo
                try
                {
                    if (preferSystemNetMail)
                        await SendViaMailKitAsync(destinatario, assunto, html, host, porta, useSsl, remetente, pass, fromName);
                    else
                        await _emailSenderHostedService.SendViaSystemNetMailStaticAsync(destinatario, assunto, html, null, host, porta, useSsl, remetente, pass, fromName);
                    _logger.LogInformation("Email enviado com sucesso pelo mÃ©todo alternativo (assunto: {Assunto}).", assunto);
                    // SÃ³ atualizar parÃ¢metro se o tipo atual for MailKit ou System.Net.Mail (nunca sobrescrever AwsSes)
                    if (ctx.Settings != null && ctx.TipoEnvioEmail != EnumTipoEnvioEmail.AwsSes)
                    {
                        var tipoQueFuncionou = preferSystemNetMail ? EnumTipoEnvioEmail.ClienteEmailDireto : EnumTipoEnvioEmail.ClienteEmailApp;
                        try
                        {
                            await _parametroSistemaService.UpdateTipoEnvioEmailOnlyAsync(tipoQueFuncionou);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "NÃ£o foi possÃ­vel atualizar TipoEnvioEmail nos parÃ¢metros do sistema.");
                        }
                    }
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Falha tambÃ©m no mÃ©todo alternativo (assunto: {Assunto}).", assunto);
                    throw firstException != null
                        ? new InvalidOperationException("Envio falhou pelo mÃ©todo configurado e pelo mÃ©todo alternativo.", new AggregateException(firstException, ex2))
                        : ex2;
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, "NÃ£o foi possÃ­vel enviar o email com o assunto: {Assunto}", assunto);
                throw;
            }
        }

        private async Task SendViaMailKitAsync(
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

            // Porta 465 = SSL implÃ­cito. Porta 587 (ex.: Gmail) = STARTTLS.
            var secureOption = (porta == 465) ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            
            try
            {
                _logger.LogInformation("Conectando ao servidor SMTP {Host}:{Porta} com seguranÃ§a {SecureOption}", host, porta, secureOption);
                await smtp.ConnectAsync(host, porta, secureOption);
                
                _logger.LogInformation("Autenticando com usuÃ¡rio: {User}", remetente);
                await smtp.AuthenticateAsync(remetente, pass);
                
                _logger.LogInformation("Enviando email para: {Destinatario}", destinatario);
                await smtp.SendAsync(email);
                
                await smtp.DisconnectAsync(true);
                _logger.LogInformation("Email enviado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email via MailKit. Host: {Host}, Porta: {Porta}, User: {User}", host, porta, remetente);
                throw;
            }
        }
    }
}
