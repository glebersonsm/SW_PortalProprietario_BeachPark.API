using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Services.Core;

public class VoucherReservaService : IVoucherReservaService
{
    private readonly IDocumentTemplateService _documentTemplateService;
    private readonly IEmpreendimentoHybridProviderService _empreendimentoProviderService;
    private readonly ITimeSharingProviderService _timeSharingProviderService;
    private readonly ILogger<VoucherReservaService> _logger;

    public VoucherReservaService(
        IDocumentTemplateService documentTemplateService,
        IEmpreendimentoHybridProviderService empreendimentoProviderService, 
        ITimeSharingProviderService timeSharingService,
        ILogger<VoucherReservaService> logger)
    {
        _documentTemplateService = documentTemplateService ?? throw new ArgumentNullException(nameof(documentTemplateService));
        _empreendimentoProviderService = empreendimentoProviderService ?? throw new ArgumentNullException(nameof(empreendimentoProviderService));
        _timeSharingProviderService = timeSharingService ?? throw new ArgumentNullException(nameof(timeSharingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VoucherDocumentResultModel> GerarVoucherAsync(int agendamentoId, bool isTimeSharing)
    {
        var templateType = EnumDocumentTemplateType.VoucherReserva;


        var templateHtml = await _documentTemplateService.GetTemplateContentHtmlAsync(templateType, null);
        if (string.IsNullOrWhiteSpace(templateHtml))
        {
            var templateTypeName = isTimeSharing ? "voucher de reserva" : "voucher de agendamento multiownership";
            throw new InvalidOperationException($"Nenhum template HTML ativo encontrado para o {templateTypeName}.");
        }

        var dadosReserva = await ObterDadosReservaAsync(agendamentoId, isTimeSharing);
        if (dadosReserva == null)
        {
            throw new InvalidOperationException("NÃ£o foi possÃ­vel localizar os dados da reserva para geraÃ§Ã£o do voucher.");
        }

        var placeholders = BuildPlaceholderDictionary(dadosReserva);

        _logger.LogInformation("Iniciando geraÃ§Ã£o do voucher para agendamento {AgendamentoId}", agendamentoId);

        var populatedHtml = PopulateHtmlTemplate(templateHtml, placeholders);
        var renderedHtml = ApplyQuillLayout(populatedHtml);

        byte[]? pdfBytes = null;
        var fileNameBase = !string.IsNullOrWhiteSpace(dadosReserva.NumeroReserva)
            ? dadosReserva.NumeroReserva
            : dadosReserva.Contrato ?? agendamentoId.ToString();

        pdfBytes = await ConvertHtmlToPdfAsync(renderedHtml);

        return new VoucherDocumentResultModel
        {
            FileBytes = pdfBytes,
            FileName = pdfBytes != null ? $"VoucherReserva_{fileNameBase}.pdf" : null,
            ContentType = pdfBytes != null ? "application/pdf" : null,
            DadosImpressao = dadosReserva,
            HtmlFull = renderedHtml
        };
    }

    public async Task<VoucherDocumentResultModel> GerarVoucherAsync(long reservaCmIdOuAgendamentIdMultipropriedade, 
        bool isTimeSharing, 
        List<Models.DadosContratoModel>? contratos,
        AutomaticCommunicationConfigModel config)
    {
        var templateType = EnumDocumentTemplateType.VoucherReserva;
            
        
        var templateHtml = await _documentTemplateService.GetTemplateContentHtmlAsync(templateType, null);
        if (string.IsNullOrWhiteSpace(templateHtml))
        {
            var templateTypeName = isTimeSharing ? "voucher de reserva" : "voucher de agendamento multiownership";
            throw new InvalidOperationException($"Nenhum template HTML ativo encontrado para o {templateTypeName}.");
        }

        var dadosReserva = await ObterDadosReservaAsync(reservaCmIdOuAgendamentIdMultipropriedade, isTimeSharing);
        if (dadosReserva == null)
        {
            throw new InvalidOperationException("NÃ£o foi possÃ­vel localizar os dados da reserva para geraÃ§Ã£o do voucher.");
        }

        if (contratos != null && contratos.Count > 0 && dadosReserva != null)
        {
            var result = isTimeSharing
            ? _timeSharingProviderService.GetContrato(dadosReserva,contratos)
            : _empreendimentoProviderService.GetContrato(dadosReserva,contratos);
        }

        var placeholders = BuildPlaceholderDictionary(dadosReserva);

        _logger.LogInformation("Iniciando geraÃ§Ã£o do voucher para agendamento {AgendamentoId}", reservaCmIdOuAgendamentIdMultipropriedade);

        var populatedHtml = PopulateHtmlTemplate(templateHtml, placeholders);
        var renderedHtml = ApplyQuillLayout(populatedHtml);

        byte[]? pdfBytes = null;
        var fileNameBase = !string.IsNullOrWhiteSpace(dadosReserva.NumeroReserva)
            ? dadosReserva.NumeroReserva
            : dadosReserva.Contrato ?? reservaCmIdOuAgendamentIdMultipropriedade.ToString();

        // Gera PDF apenas se a configuraÃ§Ã£o exigir
        if (config.TemplateSendMode == EnumTemplateSendMode.BodyHtmlAndAttachment || 
            config.TemplateSendMode == EnumTemplateSendMode.AttachmentOnly)
        {
            _logger.LogInformation("Convertendo voucher para PDF conforme configuraÃ§Ã£o");
            pdfBytes = await ConvertHtmlToPdfAsync(renderedHtml);
        }

        return new VoucherDocumentResultModel
        {
            FileBytes = pdfBytes,
            FileName = pdfBytes != null ? $"VoucherReserva_{fileNameBase}.pdf" : null,
            ContentType = pdfBytes != null ? "application/pdf" : null,
            DadosImpressao = dadosReserva,
            HtmlFull = renderedHtml
        };
    }


    public IReadOnlyCollection<PlaceholderDescriptionReservas> ListarPlaceholders()
        => VoucherReservaPlaceholder.All;

    public async Task<DadosImpressaoVoucherResultModel> ObterDadosReservaAsync(long reservaCmIdOuAgendamentIdMultipropriedade, bool isTimeSharing)
    {
        var result = isTimeSharing
            ? await _timeSharingProviderService.GetDadosImpressaoVoucher(reservaCmIdOuAgendamentIdMultipropriedade)
            : await _empreendimentoProviderService.GetDadosImpressaoVoucher(reservaCmIdOuAgendamentIdMultipropriedade.ToString(CultureInfo.InvariantCulture));
        
        if (result == null)
        {
            throw new InvalidOperationException("NÃ£o foi possÃ­vel localizar os dados da reserva para geraÃ§Ã£o do voucher.");
        }

        var dados = result;

        if (!dados.AgendamentoId.HasValue && reservaCmIdOuAgendamentIdMultipropriedade > 0)
        {
            dados.AgendamentoId = (int)reservaCmIdOuAgendamentIdMultipropriedade;
        }

        if (string.IsNullOrWhiteSpace(dados.NumeroReserva) && reservaCmIdOuAgendamentIdMultipropriedade > 0)
        {
            dados.NumeroReserva = reservaCmIdOuAgendamentIdMultipropriedade.ToString(CultureInfo.InvariantCulture);
        }

        return dados;
    }

    private static string PopulateHtmlTemplate(string templateHtml, IDictionary<string, string?> placeholders)
    {
        return Regex.Replace(
            templateHtml,
            @"\{\{\s*([^\}]+?)\s*\}\}",
            match =>
            {
                var key = match.Groups[1].Value.Trim();
                return placeholders.TryGetValue(key, out var value)
                    ? value ?? string.Empty
                    : match.Value;
            },
            RegexOptions.IgnoreCase);
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

    private static IDictionary<string, string?> BuildPlaceholderDictionary(DadosImpressaoVoucherResultModel dados)
    {
        var hospedesLista = dados.Hospedes != null && dados.Hospedes.Count > 0
            ? string.Join(Environment.NewLine, dados.Hospedes.Select((hospede, index) =>
                $"{index + 1}. {hospede.Nome} - {hospede.Documento}"))
            : string.Empty;

        var placeholders = new Dictionary<string, string?>(StringComparer.InvariantCultureIgnoreCase);

        void Add(string placeholderKey, string? value)
        {
            var normalizedKey = NormalizePlaceholderKey(placeholderKey);
            if (!string.IsNullOrEmpty(normalizedKey))
            {
                placeholders[normalizedKey] = value;
            }
        }


        // Placeholders para Time-Sharing/Multipropriedade (VoucherReserva)
        Add(VoucherReservaPlaceholder.NomeCliente, dados.NomeCliente);
        Add(VoucherReservaPlaceholder.DocumentoCliente, dados.DocumentoCliente);
        Add(VoucherReservaPlaceholder.NomeCoCessionario, dados.NomeCocessionario);
        Add(VoucherReservaPlaceholder.DocumentoCoCessionario, dados.DocumentoCoCessionario);
        Add(VoucherReservaPlaceholder.ContratoNumero, dados.Contrato);
        Add(VoucherReservaPlaceholder.ReservaNumero, !string.IsNullOrEmpty(dados.NumeroReserva) ? dados.NumeroReserva : dados.NumReserva.GetValueOrDefault().ToString() );
        Add(VoucherReservaPlaceholder.HotelNome, dados.NomeHotel);
        Add(VoucherReservaPlaceholder.CheckInData, dados.DataChegada);
        Add(VoucherReservaPlaceholder.CheckInHora, dados.HoraChegada);
        Add(VoucherReservaPlaceholder.CheckOutData, dados.DataPartida);
        Add(VoucherReservaPlaceholder.CheckOutHora, dados.HoraPartida);
        Add(VoucherReservaPlaceholder.HospedePrincipalNome, dados.HospedePrincipalNome ?? dados.HospedePrincipal);
        Add(VoucherReservaPlaceholder.HospedePrincipalDocumento, dados.HospedePrincipalDocumento);
        Add(VoucherReservaPlaceholder.TipoUtilizacao, dados.TipoUtilizacao);
        Add(VoucherReservaPlaceholder.TipoDisponibilizacao, dados.TipoDisponibilizacao ?? dados.TipoUso);
        Add(VoucherReservaPlaceholder.TipoApartamento, dados.TipoApartamento ?? dados.Acomodacao);
        Add(VoucherReservaPlaceholder.OcupacaoMaxima, dados.OcupacaoMaxima?.ToString() ?? dados.QuantidadePax);
        Add(VoucherReservaPlaceholder.VagaEstacionamento, dados.VagaEstacionamento);
        Add(VoucherReservaPlaceholder.HospedesLista, hospedesLista);
        Add(VoucherReservaPlaceholder.TermoCessao, dados.TermoCessao);
        Add(VoucherReservaPlaceholder.Observacoes, !string.IsNullOrEmpty(dados.Observacoes) ? dados.Observacoes : dados.Observacao);
        Add(VoucherReservaPlaceholder.QuantidadePaxPorFaixaEtaria, dados.QuantidadePaxPorFaixaEtaria);


        return placeholders;
    }

    private static string NormalizePlaceholderKey(string placeholder)
    {
        if (string.IsNullOrWhiteSpace(placeholder))
        {
            return string.Empty;
        }

        var trimmed = placeholder.Trim();

        if (trimmed.StartsWith("{{", StringComparison.InvariantCulture) &&
            trimmed.EndsWith("}}", StringComparison.InvariantCulture) &&
            trimmed.Length > 4)
        {
            trimmed = trimmed.Substring(2, trimmed.Length - 4);
        }

        return trimmed.Trim();
    }
}

