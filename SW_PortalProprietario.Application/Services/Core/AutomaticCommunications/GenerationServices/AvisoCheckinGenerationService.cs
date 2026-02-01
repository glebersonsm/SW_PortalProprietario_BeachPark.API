using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using System.Text;
using System.Text.RegularExpressions;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;

/// <summary>
/// Serviço compartilhado para geração de avisos de check-in próximo
/// Usado tanto na simulação quanto no processamento automático
/// </summary>
public class AvisoCheckinGenerationService
{
    private readonly ILogger<AvisoCheckinGenerationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDocumentTemplateService _documentTemplateService;

    public AvisoCheckinGenerationService(
        ILogger<AvisoCheckinGenerationService> logger,
        IConfiguration configuration,
        IDocumentTemplateService documentTemplateService)
    {
        _logger = logger;
        _configuration = configuration;
        _documentTemplateService = documentTemplateService;
    }

    /// <summary>
    /// Gera aviso completo com HTML ou PDF baseado no modo de envio
    /// </summary>
    public async Task<AvisoCheckinEmailDataModel?> GerarAvisoCompletoAsync(
        ReservaInfo reserva,
        DadosImpressaoVoucherResultModel dadosReserva,
        int daysBefore,
        int? templateId,
        EnumTemplateSendMode sendMode)
    {
        try
        {
            _logger.LogInformation("Gerando aviso check-in para reserva {ReservaId} (Modo: {SendMode})", 
                reserva.ReservaId, sendMode);

            if (templateId.GetValueOrDefault(0) == 0)
                throw new ArgumentException("TemplateId inválido para geração de aviso de check-in.");

            string? htmlContent = await _documentTemplateService.GetTemplateContentHtmlAsync(
                EnumDocumentTemplateType.AvisoReservaCheckinProximo,
                templateId!.Value);

            if (!string.IsNullOrEmpty(htmlContent))
            {
                htmlContent = SubstituirPlaceholders(htmlContent, reserva, dadosReserva, daysBefore);
            }


            // Se não tiver template ou falhou, gerar HTML padrão
            if (string.IsNullOrEmpty(htmlContent) && 
                (sendMode == EnumTemplateSendMode.BodyHtmlOnly || 
                sendMode == EnumTemplateSendMode.BodyHtmlAndAttachment))
            {
                htmlContent = GerarHtmlPadrao(reserva, dadosReserva, daysBefore);
            }

            var renderedHtml = ApplyQuillLayout(htmlContent!);
            var pdfBytes = await ConvertHtmlToPdfAsync(renderedHtml);

            var fileNameBase = !string.IsNullOrWhiteSpace(dadosReserva.NumeroReserva)
                ? dadosReserva.NumeroReserva
                : dadosReserva.Contrato ?? dadosReserva.AgendamentoId.GetValueOrDefault(0).ToString();

            return new AvisoCheckinEmailDataModel
            {
                HtmlContent = htmlContent,
                PdfBytes = pdfBytes,
                PdfFileName = fileNameBase,
                Reserva = reserva,
                DadosReserva = dadosReserva,
                DaysBefore = daysBefore
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar aviso check-in para reserva {ReservaId}", reserva.ReservaId);
            return null;
        }
    }

    private static string ApplyQuillLayout(string htmlContent)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"pt-BR\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        sb.AppendLine("  <link href=\"https://cdn.quilljs.com/1.3.6/quill.snow.css\" rel=\"stylesheet\">");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { font-family: 'Helvetica', 'Arial', sans-serif; margin: 0; }");
        sb.AppendLine("    .ql-container.ql-snow { border: none !important; }");
        sb.AppendLine("    .ql-editor { padding: 0; min-height: 100%; }");
        sb.AppendLine("    .ql-editor > *:first-child { margin-top: 0 !important; }");
        sb.AppendLine("    table {width: 80%;margin: 20px auto;border-collapse: collapse;}");
        sb.AppendLine("    table, th, td {border: 1px solid #000;}");
        sb.AppendLine("    td, th {padding: 6px;}");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div class=\"ql-container ql-snow\">");
        sb.AppendLine("    <div class=\"ql-editor\">");
        sb.AppendLine(htmlContent);
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }


    private static async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
    {
        await new BrowserFetcher().DownloadAsync();

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(htmlContent, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        await using var pdfStream = await page.PdfStreamAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "10mm",
                Bottom = "10mm",
                Left = "15mm",
                Right = "15mm"
            }
        });

        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Substitui placeholders no texto (assunto ou corpo)
    /// </summary>
    public string SubstituirPlaceholders(
        string texto, 
        ReservaInfo reserva, 
        DadosImpressaoVoucherResultModel dadosReserva, 
        int daysBefore)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return texto;

        var resultado = texto;

        // Substituições diretas (case-insensitive)
        resultado = ReplaceIgnoreCase(resultado, "{{ReservaId}}", reserva.ReservaId > 0 ? reserva.ReservaId.ToString() : dadosReserva.NumeroReserva ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{AgendamentoId}}", reserva.AgendamentoId > 0 ? reserva.AgendamentoId.ToString() : dadosReserva.AgendamentoId?.ToString() ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{NomeCliente}}", dadosReserva.NomeCliente ?? dadosReserva.HospedePrincipal ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{EmailCliente}}", reserva.EmailCliente ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{DataCheckIn}}", reserva.DataCheckIn.ToString("dd/MM/yyyy"));
        resultado = ReplaceIgnoreCase(resultado, "{{CheckInData}}", reserva.DataCheckIn.ToString("dd/MM/yyyy"));
        resultado = ReplaceIgnoreCase(resultado, "{{DiasRestantes}}", daysBefore.ToString());
        resultado = ReplaceIgnoreCase(resultado, "{{DaysBefore}}", daysBefore.ToString());
        resultado = ReplaceIgnoreCase(resultado, "{{CotaNome}}", reserva.CotaNome ?? dadosReserva.CotaNome ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{UhNumero}}", reserva.UhCondominioNumero ?? dadosReserva.UhCondominioNumero ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{NomeHotel}}", dadosReserva.NomeHotel ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{Contrato}}", dadosReserva.Contrato ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{LocalAtendimento}}", dadosReserva.LocalAtendimento ?? "Equipe MY Mabu");

        // Regex para capturar placeholders restantes
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.IgnoreCase);
        resultado = regex.Replace(resultado, match =>
        {
            var key = match.Groups[1].Value;
            var value = ObterValorPlaceholder(reserva, dadosReserva, key, daysBefore);
            return value ?? match.Value;
        });

        return resultado;
    }

    /// <summary>
    /// Gera HTML padrão profissional para o aviso
    /// </summary>
    public string GerarHtmlPadrao(ReservaInfo reserva, DadosImpressaoVoucherResultModel dadosReserva, int daysBefore)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; background-color: #f9f9f9; }");
        sb.AppendLine(".container { max-width: 800px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine(".header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; margin: -30px -30px 30px -30px; }");
        sb.AppendLine(".countdown { text-align: center; margin: 30px 0; }");
        sb.AppendLine(".countdown-number { font-size: 72px; font-weight: bold; color: #2196f3; line-height: 1; }");
        sb.AppendLine(".countdown-text { font-size: 18px; color: #666; margin-top: 10px; }");
        sb.AppendLine(".info-box { background-color: #e3f2fd; padding: 20px; border-left: 4px solid #2196f3; margin: 20px 0; border-radius: 4px; }");
        sb.AppendLine(".info-box p { margin: 8px 0; }");
        sb.AppendLine(".highlight { background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; border-radius: 4px; margin: 20px 0; }");
        sb.AppendLine(".footer { text-align: center; margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 14px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");
        
        sb.AppendLine("<div class=\"header\">");
        sb.AppendLine("<h1 style=\"margin: 0; font-size: 32px;\">??? Seu Check-in Está Chegando!</h1>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("<div class=\"countdown\">");
        sb.AppendLine($"<div class=\"countdown-number\">{daysBefore}</div>");
        sb.AppendLine($"<div class=\"countdown-text\">dia{(daysBefore != 1 ? "s" : "")} para sua reserva!</div>");
        sb.AppendLine("</div>");

        var nomeCliente = dadosReserva.NomeCliente ?? dadosReserva.HospedePrincipal ?? "Cliente";
        sb.AppendLine($"<p style=\"font-size: 16px; line-height: 1.8;\">Olá{(!string.IsNullOrEmpty(nomeCliente) ? $" <strong>{nomeCliente}</strong>" : "")}! ??</p>");
        sb.AppendLine($"<p style=\"font-size: 16px; line-height: 1.8;\">Estamos ansiosos para recebê-lo! Faltam apenas <strong>{daysBefore} dia{(daysBefore != 1 ? "s" : "")}</strong> para o seu check-in.</p>");

        sb.AppendLine("<div class=\"info-box\">");
        sb.AppendLine("<h3 style=\"margin-top: 0; color: #1976d2;\">?? Detalhes da Reserva</h3>");
        sb.AppendLine($"<p><strong>Reserva ID:</strong> {reserva.ReservaId}</p>");
        sb.AppendLine($"<p><strong>Data do Check-in:</strong> {reserva.DataCheckIn:dd/MM/yyyy}</p>");
        
        if (!string.IsNullOrEmpty(dadosReserva.NomeHotel))
            sb.AppendLine($"<p><strong>Hotel:</strong> {dadosReserva.NomeHotel}</p>");
        
        if (!string.IsNullOrEmpty(reserva.CotaNome))
            sb.AppendLine($"<p><strong>Cota:</strong> {reserva.CotaNome}</p>");
        
        if (!string.IsNullOrEmpty(reserva.UhCondominioNumero))
            sb.AppendLine($"<p><strong>Unidade:</strong> {reserva.UhCondominioNumero}</p>");
        
        sb.AppendLine("</div>");

        sb.AppendLine("<div class=\"highlight\">");
        sb.AppendLine("?? <strong>Lembrete importante:</strong> Prepare sua documentação e certifique-se de chegar no horário programado para o check-in.");
        sb.AppendLine("</div>");

        sb.AppendLine("<p style=\"font-size: 16px; line-height: 1.8; margin-top: 30px;\">Estamos preparando tudo para que você tenha uma experiência maravilhosa!</p>");

        var localAtendimento = dadosReserva.LocalAtendimento ?? "Equipe MY Mabu";
        sb.AppendLine($"<p style=\"margin-top: 30px;\">Atenciosamente,<br/><strong>{localAtendimento}</strong></p>");

        sb.AppendLine("<div class=\"footer\">");
        sb.AppendLine("<p><em>Este é um email automático de aviso. Em caso de dúvidas, entre em contato conosco.</em></p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
        return sb.ToString();
    }

    /// <summary>
    /// Gera email simples para modo "Apenas Anexo"
    /// </summary>
    public string GerarEmailSimples(ReservaInfo reserva, DadosImpressaoVoucherResultModel dadosReserva, int daysBefore)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; background-color: #f9f9f9; }");
        sb.AppendLine(".container { max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine(".attachment-box { background-color: #e8f5e9; padding: 20px; border-left: 4px solid #4caf50; margin: 20px 0; border-radius: 4px; text-align: center; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");
        
        var nomeCliente = dadosReserva.NomeCliente ?? dadosReserva.HospedePrincipal;
        sb.AppendLine($"<p>Olá{(!string.IsNullOrEmpty(nomeCliente) ? $" <strong>{nomeCliente}</strong>" : "")},</p>");
        sb.AppendLine($"<p>Faltam <strong>{daysBefore} dia{(daysBefore != 1 ? "s" : "")}</strong> para o seu check-in!</p>");
        
        sb.AppendLine("<div class=\"attachment-box\">");
        sb.AppendLine("<p style=\"font-size: 48px; margin: 10px 0;\">??</p>");
        sb.AppendLine("<p style=\"font-size: 16px; margin: 10px 0;\"><strong>Informações em Anexo</strong></p>");
        sb.AppendLine("<p>Confira os detalhes da sua reserva no documento PDF anexado a este email.</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine($"<p style=\"margin-top: 20px;\"><strong>Reserva:</strong> {reserva.ReservaId}</p>");
        sb.AppendLine($"<p><strong>Check-in:</strong> {reserva.DataCheckIn:dd/MM/yyyy}</p>");
        
        if (!string.IsNullOrEmpty(dadosReserva.NomeHotel))
            sb.AppendLine($"<p><strong>Hotel:</strong> {dadosReserva.NomeHotel}</p>");
        
        sb.AppendLine("<p style=\"margin-top: 30px; color: #666; font-size: 14px;\">Aguardamos você!</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
        return sb.ToString();
    }

    #region Métodos Auxiliares

    private string ReplaceIgnoreCase(string text, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(oldValue))
            return text;

        var regex = new Regex(Regex.Escape(oldValue), RegexOptions.IgnoreCase);
        return regex.Replace(text, newValue ?? "");
    }

    private string? ObterValorPlaceholder(ReservaInfo reserva, DadosImpressaoVoucherResultModel dadosReserva, string key, int daysBefore)
    {
        return key.ToLowerInvariant() switch
        {
            "reservaid" => reserva.ReservaId > 0 ? reserva.ReservaId.ToString() : dadosReserva.NumeroReserva,
            "agendamentoid" => reserva.AgendamentoId > 0 ? reserva.AgendamentoId.ToString() : dadosReserva.AgendamentoId?.ToString(),
            "nomecliente" => dadosReserva.NomeCliente ?? dadosReserva.HospedePrincipal,
            "emailcliente" => reserva.EmailCliente,
            "datacheckin" or "checkindata" => reserva.DataCheckIn.ToString("dd/MM/yyyy"),
            "diasrestantes" or "daysbefore" => daysBefore.ToString(),
            "cotanome" => reserva.CotaNome ?? dadosReserva.CotaNome,
            "uhnumero" => reserva.UhCondominioNumero ?? dadosReserva.UhCondominioNumero,
            "nomehotel" or "hotel" => dadosReserva.NomeHotel,
            "contrato" => dadosReserva.Contrato,
            "localatendimento" => dadosReserva.LocalAtendimento ?? "Equipe MY Mabu",
            _ => null
        };
    }

    #endregion
}


