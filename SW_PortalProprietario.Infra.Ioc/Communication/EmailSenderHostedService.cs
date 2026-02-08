using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces.Communication;
using SW_PortalProprietario.Domain.Enumns;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using MailMessage = System.Net.Mail.MailMessage;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace SW_PortalProprietario.Infra.Ioc.Communication
{
    public class EmailSenderHostedService : IEmailSenderHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderHostedService> _logger;
        private readonly ISmtpSettingsProvider _smtpSettingsProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EmailSenderHostedService(IConfiguration configuration,
            ILogger<EmailSenderHostedService> loger,
            ISmtpSettingsProvider smtpSettingsProvider,
            IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _logger = loger;
            _smtpSettingsProvider = smtpSettingsProvider;
            _serviceScopeFactory = serviceScopeFactory;
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

            var ctx = await _smtpSettingsProvider.GetSmtpContextAsync();
            var trackingBaseUrl = !string.IsNullOrWhiteSpace(ctx.EmailTrackingBaseUrl) ? ctx.EmailTrackingBaseUrl.Trim() : _configuration.GetValue<string>("EmailTrackingBaseUrl");
            var html = InjectTrackingPixelIfConfigured(model.ConteudoEmail, model.Id.GetValueOrDefault(), trackingBaseUrl);
            await Send(model.Destinatario, model.Assunto, html, model.Anexos);

        }

        private static string InjectTrackingPixelIfConfigured(string html, int emailId, string? baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl) || emailId <= 0) return html;
            baseUrl = baseUrl.TrimEnd('/');
            var pixel = $"<img src=\"{baseUrl}/Email/track/open?id={emailId}\" width=\"1\" height=\"1\" alt=\"\" style=\"display:block;\" />";
            return html + pixel;
        }

        private async Task Send(string destinatario, string assunto, string html, List<EmailAnexoModel>? anexos = null)
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
                        await SendViaSystemNetMailStaticAsync(destinatario, assunto, html, anexos, host, porta, useSsl, remetente, pass, fromName);
                    else
                        await SendViaMailKitAsync(destinatario, assunto, html, anexos, host, porta, useSsl, remetente, pass, fromName);
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
                        await SendViaMailKitAsync(destinatario, assunto, html, anexos, host, porta, useSsl, remetente, pass, fromName);
                    else
                        await SendViaSystemNetMailStaticAsync(destinatario, assunto, html, anexos, host, porta, useSsl, remetente, pass, fromName);
                    _logger.LogInformation("Email enviado com sucesso pelo método alternativo (assunto: {Assunto}).", assunto);
                    // Persistir o tipo que funcionou para evitar nova falha nos próximos envios
                    if (ctx.Settings != null)
                    {
                        var tipoQueFuncionou = preferSystemNetMail ? EnumTipoEnvioEmail.ClienteEmailDireto : EnumTipoEnvioEmail.ClienteEmailApp;
                        try
                        {
                            using var scope = _serviceScopeFactory.CreateScope();
                            var paramService = scope.ServiceProvider.GetRequiredService<IParametroSistemaService>();
                            await paramService.UpdateTipoEnvioEmailOnlyAsync(tipoQueFuncionou);
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

        /// <summary>
        /// Envio via MailKit (Cliente de email direto). Usado como método principal ou como fallback.
        /// </summary>
        private async Task SendViaMailKitAsync(
            string destinatario,
            string assunto,
            string html,
            List<EmailAnexoModel>? anexos,
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
            {
                email.To.Add(MailboxAddress.Parse(item.TrimEnd().TrimStart().Replace(";", "")));
            }
            email.Subject = assunto;

            var bodyBuilder = ProcessarHtmlComImagens(html);

            if (anexos != null && anexos.Any())
            {
                _logger.LogInformation("Adicionando {Count} anexo(s) ao email", anexos.Count);
                foreach (var anexo in anexos)
                {
                    try
                    {
                        if (anexo.Arquivo != null && anexo.Arquivo.Length > 0)
                        {
                            var attachment = new MimePart(anexo.TipoMime)
                            {
                                Content = new MimeContent(new MemoryStream(anexo.Arquivo)),
                                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                                ContentTransferEncoding = ContentEncoding.Base64,
                                FileName = anexo.NomeArquivo
                            };
                            bodyBuilder.Attachments.Add(attachment);
                            _logger.LogInformation("Anexo adicionado: {NomeArquivo} ({Length} bytes)", anexo.NomeArquivo, anexo.Arquivo.Length);
                        }
                        else
                            _logger.LogWarning("Anexo {NomeArquivo} está vazio ou nulo", anexo.NomeArquivo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao adicionar anexo {NomeArquivo}. Continuando sem este anexo.", anexo.NomeArquivo);
                    }
                }
            }

            email.Body = bodyBuilder.ToMessageBody();

            // Porta 465 = SSL implícito (SslOnConnect). Porta 587 (e outras) = STARTTLS (conexão em texto e depois upgrade).
            var secureOption = (porta == 465) ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(host, porta, secureOption);
            smtp.Authenticate(remetente, password: pass);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        /// <summary>
        /// Envio via System.Net.Mail (Cliente de email APP), estilo Sw_ClimberIntegration. Compartilhado com EmailSenderService.
        /// </summary>
        public static async Task SendViaSystemNetMailStaticAsync(
            string destinatario,
            string assunto,
            string html,
            List<EmailAnexoModel>? anexos,
            string host,
            int porta,
            bool useSsl,
            string remetente,
            string pass,
            string? fromName)
        {
            var fromAddress = string.IsNullOrWhiteSpace(fromName) ? remetente : $"{fromName.Trim()} <{remetente}>";
            using var mensagem = new MailMessage();
            mensagem.From = new System.Net.Mail.MailAddress(remetente, fromName ?? "");
            foreach (var to in destinatario.Split(";", StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = to.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    mensagem.To.Add(trimmed);
            }
            mensagem.Subject = assunto;
            mensagem.Body = html;
            mensagem.IsBodyHtml = true;

            if (anexos != null)
            {
                foreach (var anexo in anexos)
                {
                    if (anexo?.Arquivo != null && anexo.Arquivo.Length > 0)
                        mensagem.Attachments.Add(new Attachment(new MemoryStream(anexo.Arquivo), anexo.NomeArquivo, anexo.TipoMime));
                }
            }

            using var clienteSmtp = new SmtpClient(host, porta);
            clienteSmtp.EnableSsl = useSsl;
            clienteSmtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            clienteSmtp.UseDefaultCredentials = false;
            clienteSmtp.Credentials = new NetworkCredential(remetente, pass);

            await clienteSmtp.SendMailAsync(mensagem);
        }

        private BodyBuilder ProcessarHtmlComImagens(string html)
        {
            var bodyBuilder = new BodyBuilder();
            var htmlProcessado = new StringBuilder(html);
            var imageCounter = 0;
            var replacements = new List<(int index, int length, string replacement)>();

            // Processar imagens base64 (data:image/...) - suporta aspas simples e duplas
            var base64Pattern = @"<img\s+[^>]*src\s*=\s*[""']?(data:image/([^;""']+);base64,([^""'\s>]+))[""']?[^>]*>";
            var base64Matches = Regex.Matches(html, base64Pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            foreach (Match match in base64Matches)
            {
                var imageType = match.Groups[2].Value;
                var base64Data = match.Groups[3].Value;
                var fullDataUri = match.Groups[1].Value;
                var fullMatch = match.Value;

                try
                {
                    var imageBytes = Convert.FromBase64String(base64Data);
                    var contentType = $"image/{imageType.ToLower()}";
                    var contentId = $"img_{imageCounter++}";

                    // Normalizar tipo de imagem
                    if (contentType == "image/jpg")
                        contentType = "image/jpeg";

                    // Criar recurso relacionado
                    var imageStream = new MemoryStream(imageBytes);
                    var imageResource = new MimePart(contentType)
                    {
                        Content = new MimeContent(imageStream),
                        ContentId = contentId,
                        ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = $"image.{imageType}"
                    };

                    bodyBuilder.LinkedResources.Add(imageResource);

                    // Criar nova tag img com cid:
                    var newImgTag = Regex.Replace(fullMatch, 
                        Regex.Escape(fullDataUri), 
                        $"cid:{contentId}", 
                        RegexOptions.IgnoreCase);
                    
                    // Armazenar substituição para aplicar depois (em ordem reversa)
                    replacements.Add((match.Index, match.Length, newImgTag));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao processar imagem base64 no HTML do email. Continuando sem a imagem. Tipo: {ImageType}", imageType);
                }
            }

            // Aplicar substituições em ordem reversa para manter os índices corretos
            foreach (var (index, length, replacement) in replacements.OrderByDescending(r => r.index))
            {
                htmlProcessado.Remove(index, length);
                htmlProcessado.Insert(index, replacement);
            }

            bodyBuilder.HtmlBody = htmlProcessado.ToString();
            return bodyBuilder;
        }
    }
}
