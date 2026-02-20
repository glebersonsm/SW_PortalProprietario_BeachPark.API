using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Application.Interfaces;

namespace SW_PortalProprietario.Application.Services.Core;

public class IncentivoParaAgendamentoService : IIncentivoParaAgendamentoDocumentService
{
    private readonly IDocumentTemplateService _documentTemplateService;
    private readonly IRepositoryNH _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IncentivoParaAgendamentoService> _logger;

    public IncentivoParaAgendamentoService(
        IDocumentTemplateService documentTemplateService,
        IRepositoryNH repository,
        IConfiguration configuration,
        ILogger<IncentivoParaAgendamentoService> logger)
    {
        _documentTemplateService = documentTemplateService ?? throw new ArgumentNullException(nameof(documentTemplateService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IncentivoAgendamentoDocumentResultModel> GerarDocumentoIncentivoAsync(int contratoId, int anoReferencia)
    {
        var templateType = EnumDocumentTemplateType.IncentivoParaAgendamento;

        var templateHtml = await _documentTemplateService.GetTemplateContentHtmlAsync(templateType, null);
        if (string.IsNullOrWhiteSpace(templateHtml))
        {
            throw new InvalidOperationException("Nenhum template HTML ativo encontrado para o incentivo de agendamento.");
        }

        var dadosIncentivo = await ObterDadosIncentivoAsync(contratoId, anoReferencia);
        if (dadosIncentivo == null)
        {
            throw new InvalidOperationException("NÃ£o foi possÃ­vel obter os dados do contrato para geraÃ§Ã£o do incentivo.");
        }

        var placeholders = BuildPlaceholderDictionary(dadosIncentivo);

        _logger.LogInformation("Iniciando geraÃ§Ã£o do documento de incentivo para contrato {ContratoId}, ano {AnoReferencia}", contratoId, anoReferencia);

        var populatedHtml = PopulateHtmlTemplate(templateHtml, placeholders);
        var renderedHtml = ApplyQuillLayout(populatedHtml);
        var pdfBytes = await ConvertHtmlToPdfAsync(renderedHtml);

        return new IncentivoAgendamentoDocumentResultModel
        {
            FileBytes = pdfBytes,
            FileName = $"Incentivo_Agendamento_{dadosIncentivo.ContratoNumero}_{anoReferencia}.pdf",
            ContentType = "application/pdf",
            DadosImpressao = dadosIncentivo,
            HtmlFull = renderedHtml
        };
    }

    public async Task<DadosIncentivoAgendamentoModel?> ObterDadosIncentivoAsync(int contratoId, int anoReferencia)
    {
        try
        {
            // TODO: Implementar busca real dos dados do contrato
            // Por enquanto, retornando dados mock para demonstraÃ§Ã£o
            var dados = new DadosIncentivoAgendamentoModel
            {
                // Dados do Cliente/Contrato - estes devem ser buscados do banco
                NomeCliente = "Cliente Exemplo",
                DocumentoCliente = "000.000.000-00",
                ContratoNumero = $"CT{contratoId:D6}",
                EmailCliente = "cliente@exemplo.com",
                TelefoneCliente = "(11) 99999-9999",

                // Dados do PerÃ­odo de Agendamento
                DataInicioAgendamento = new DateTime(anoReferencia, 1, 1),
                DataFinalAgendamento = new DateTime(anoReferencia, 12, 31),
                PeriodoAgendamentoFormatado = $"01/01/{anoReferencia} a 31/12/{anoReferencia}",
                AnoReferencia = anoReferencia,

                // Dados das Semanas - estes devem ser calculados baseado nas reservas
                QuantidadeSemanasDireito = 52,
                QuantidadeSemanasAgendadas = 10,
                QuantidadeSemanasDisponiveis = 42,
                PercentualUtilizacao = 19.23m,

                // Sistema
                DataGeracao = DateTime.Now,
                LinkPortal = _configuration.GetValue<string>("PortalUrl", "https://portal.exemplo.com"),
                ContatoSuporte = _configuration.GetValue<string>("ContatoSuporte", "suporte@exemplo.com"),

                // Dados de incentivo
                MensagemIncentivo = "Aproveite suas semanas disponÃ­veis e faÃ§a suas reservas!",
                DataLimiteAgendamento = new DateTime(anoReferencia, 11, 30),
                BeneficiosAgendamento = new List<string>
                {
                    "Acesso prioritÃ¡rio a melhores acomodaÃ§Ãµes",
                    "Descontos em serviÃ§os do resort",
                    "Upgrades gratuitos quando disponÃ­veis"
                }
            };

            // Calcular resumo das semanas por ano
            dados.ListaSemanasDisponiveis = new List<SemanaPorAnoModel>
            {
                new SemanaPorAnoModel
                {
                    Ano = anoReferencia,
                    SemanasDireito = dados.QuantidadeSemanasDireito,
                    SemanasAgendadas = dados.QuantidadeSemanasAgendadas
                }
            };

            dados.ResumoSemanasAnual = GerarResumoSemanasAnual(dados.ListaSemanasDisponiveis);

            return dados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dados do incentivo para contrato {ContratoId}, ano {AnoReferencia}", contratoId, anoReferencia);
            return null;
        }
    }

    public IReadOnlyCollection<PlaceholderDescriptionIncentivo> ListarPlaceholders()
        => IncentivoParaAgendamentoPlaceholder.All;

    private static Dictionary<string, string> BuildPlaceholderDictionary(DadosIncentivoAgendamentoModel dados)
    {
        return new Dictionary<string, string>
        {
            // Dados do Cliente/Contrato
            ["{{NomeCliente}}"] = dados.NomeCliente,
            ["{{DocumentoCliente}}"] = dados.DocumentoCliente,
            ["{{ContratoNumero}}"] = dados.ContratoNumero,
            ["{{EmailCliente}}"] = dados.EmailCliente,
            ["{{TelefoneCliente}}"] = dados.TelefoneCliente,

            // Dados do PerÃ­odo de Agendamento
            ["{{DataInicioAgendamento}}"] = dados.DataInicioAgendamento.ToString("dd/MM/yyyy"),
            ["{{DataFinalAgendamento}}"] = dados.DataFinalAgendamento.ToString("dd/MM/yyyy"),
            ["{{PeriodoAgendamentoFormatado}}"] = dados.PeriodoAgendamentoFormatado,
            ["{{AnoReferencia}}"] = dados.AnoReferencia.ToString(),

            // Dados das Semanas
            ["{{QuantidadeSemanasDireito}}"] = dados.QuantidadeSemanasDireito.ToString(),
            ["{{QuantidadeSemanasAgendadas}}"] = dados.QuantidadeSemanasAgendadas.ToString(),
            ["{{QuantidadeSemanasDisponiveis}}"] = dados.QuantidadeSemanasDisponiveis.ToString(),
            ["{{PercentualUtilizacao}}"] = $"{dados.PercentualUtilizacao:F2}%",

            // Dados das Semanas por Ano
            ["{{ListaSemanasDisponiveis}}"] = FormatarListaSemanasDisponiveis(dados.ListaSemanasDisponiveis),
            ["{{ResumoSemanasAnual}}"] = dados.ResumoSemanasAnual,

            // Dados de Incentivo
            ["{{MensagemIncentivo}}"] = dados.MensagemIncentivo,
            ["{{DataLimiteAgendamento}}"] = dados.DataLimiteAgendamento?.ToString("dd/MM/yyyy") ?? "NÃ£o definida",
            ["{{BeneficiosAgendamento}}"] = FormatarBeneficios(dados.BeneficiosAgendamento),

            // Dados do Sistema
            ["{{DataGeracao}}"] = dados.DataGeracao.ToString("dd/MM/yyyy HH:mm"),
            ["{{LinkPortal}}"] = dados.LinkPortal,
            ["{{ContatoSuporte}}"] = dados.ContatoSuporte
        };
    }

    private static string FormatarListaSemanasDisponiveis(List<SemanaPorAnoModel> semanas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<ul>");

        foreach (var semana in semanas)
        {
            sb.AppendLine($"<li><strong>Ano {semana.Ano}:</strong> {semana.SemanasDisponiveis} semanas disponÃ­veis de {semana.SemanasDireito} totais ({semana.PercentualUtilizacao:F1}% utilizado)</li>");
        }

        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    private static string FormatarBeneficios(List<string> beneficios)
    {
        if (!beneficios.Any())
            return "Nenhum benefÃ­cio especÃ­fico configurado.";

        var sb = new StringBuilder();
        sb.AppendLine("<ul>");

        foreach (var beneficio in beneficios)
        {
            sb.AppendLine($"<li>{beneficio}</li>");
        }

        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    private static string GerarResumoSemanasAnual(List<SemanaPorAnoModel> semanas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<table style='border-collapse: collapse; width: 100%;'>");
        sb.AppendLine("<tr style='background-color: #f2f2f2;'>");
        sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Ano</th>");
        sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: center;'>Direito</th>");
        sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: center;'>Agendadas</th>");
        sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: center;'>DisponÃ­veis</th>");
        sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: center;'>% UtilizaÃ§Ã£o</th>");
        sb.AppendLine("</tr>");

        foreach (var semana in semanas)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{semana.Ano}</td>");
            sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{semana.SemanasDireito}</td>");
            sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{semana.SemanasAgendadas}</td>");
            sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{semana.SemanasDisponiveis}</td>");
            sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{semana.PercentualUtilizacao:F1}%</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
        return sb.ToString();
    }

    private static string PopulateHtmlTemplate(string templateHtml, Dictionary<string, string> placeholders)
    {
        var result = templateHtml;
        foreach (var placeholder in placeholders)
        {
            result = result.Replace(placeholder.Key, placeholder.Value);
        }
        return result;
    }

    private static string ApplyQuillLayout(string htmlContent)
    {
        // Layout similar ao usado no VoucherReservaService
        return $@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Incentivo para Agendamento</title>
    <style>
        @page {{
            margin: 1cm;
            size: A4;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 12px;
            line-height: 1.4;
            color: #333;
            margin: 0;
            padding: 20px;
        }}
        h1, h2, h3 {{
            color: #2c5aa0;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
            margin: 10px 0;
        }}
        th, td {{
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }}
        th {{
            background-color: #f2f2f2;
            font-weight: bold;
        }}
        ul {{
            padding-left: 20px;
        }}
        li {{
            margin: 5px 0;
        }}
        .highlight {{
            background-color: #fff3cd;
            padding: 10px;
            border-left: 4px solid #856404;
            margin: 15px 0;
        }}
    </style>
</head>
<body>
    {htmlContent}
</body>
</html>";
    }

    private async Task<byte[]> ConvertHtmlToPdfAsync(string html)
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

        var pdfOptions = new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "1cm",
                Right = "1cm",
                Bottom = "1cm",
                Left = "1cm"
            }
        };

        return await page.PdfDataAsync(pdfOptions);
    }
}