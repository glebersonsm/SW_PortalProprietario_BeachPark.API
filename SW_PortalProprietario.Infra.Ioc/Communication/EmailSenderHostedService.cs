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
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace SW_PortalProprietario.Infra.Ioc.Communication
{
    public class EmailSenderHostedService : IEmailSenderHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderHostedService> _logger;
        private readonly ISmtpSettingsProvider _smtpSettingsProvider;

        public EmailSenderHostedService(IConfiguration configuration,
            ILogger<EmailSenderHostedService> loger,
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

            var html = InjectTrackingPixelIfConfigured(model.ConteudoEmail, model.Id.GetValueOrDefault(), _configuration);
            await Send(model.Destinatario, model.Assunto, html, model.Anexos);

        }

        private static string InjectTrackingPixelIfConfigured(string html, int emailId, IConfiguration configuration)
        {
            var baseUrl = configuration.GetValue<string>("EmailTrackingBaseUrl");
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
                
                // Processar HTML e incorporar imagens como recursos relacionados
                var bodyBuilder = ProcessarHtmlComImagens(html);
                
                // Adicionar anexos ao email
                if (anexos != null && anexos.Any())
                {
                    _logger.LogInformation($"Adicionando {anexos.Count} anexo(s) ao email");
                    
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
                                _logger.LogInformation($"Anexo adicionado: {anexo.NomeArquivo} ({anexo.Arquivo.Length} bytes)");
                            }
                            else
                            {
                                _logger.LogWarning($"Anexo {anexo.NomeArquivo} está vazio ou nulo");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Erro ao adicionar anexo {anexo.NomeArquivo}. Continuando sem este anexo.");
                        }
                    }
                }
                
                email.Body = bodyBuilder.ToMessageBody();

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
