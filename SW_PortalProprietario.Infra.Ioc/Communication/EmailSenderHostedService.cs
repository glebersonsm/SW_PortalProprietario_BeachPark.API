using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
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

        public EmailSenderHostedService(IConfiguration configuration,
            ILogger<EmailSenderHostedService> loger)
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

            await Send(model.Destinatario, model.Assunto, model.ConteudoEmail, model.Anexos);

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
