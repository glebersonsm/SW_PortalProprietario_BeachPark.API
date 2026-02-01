using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Functions;
using System.Text;
using FluentNHibernate.Utils;
using Dapper;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;

/// <summary>
/// Serviço compartilhado para geração de incentivos para agendamento
/// Usado tanto na simulação quanto no processamento automático
/// </summary>
public class IncentivoAgendamentoGenerationService
{
    private readonly ILogger<IncentivoAgendamentoGenerationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDocumentTemplateService _documentTemplateService;
    private readonly IEmpreendimentoProviderService _empreendimentoProviderService;

    public IncentivoAgendamentoGenerationService(
        ILogger<IncentivoAgendamentoGenerationService> logger,
        IConfiguration configuration,
        IDocumentTemplateService documentTemplateService,
        IEmpreendimentoProviderService empreendimentoProviderService)
    {
        _logger = logger;
        _configuration = configuration;
        _documentTemplateService = documentTemplateService;
        _empreendimentoProviderService = empreendimentoProviderService;
    }

    /// <summary>
    /// Gera aviso completo com HTML ou PDF baseado no modo de envio
    /// </summary>
    public async Task<IncentivoParaAgendamentoEmailDataModel?> GerarAvisoCompletoAsync(
        AutomaticCommunicationConfigModel config,
        DadosContratoModel contrato,
        PosicaoAgendamentoViewModel statusAgendamento,
        int daysBefore)
    {
        try
        {
            _logger.LogInformation("Gerando incentivo para agendamento {ContratoNumero} (Modo: {SendMode})",
                contrato.NumeroContrato, config.TemplateSendMode);

            if (config.TemplateId.GetValueOrDefault(0) == 0)
                throw new ArgumentException("TemplateId inválido para incentivo para agendamento.");

            string? htmlContent = await _documentTemplateService.GetTemplateContentHtmlAsync(
                EnumDocumentTemplateType.IncentivoParaAgendamento,
                config.TemplateId.GetValueOrDefault(0));

            if (!string.IsNullOrEmpty(htmlContent))
            {
                htmlContent = SubstituirPlaceholders(htmlContent, statusAgendamento, contrato);
            }

            if (string.IsNullOrEmpty(htmlContent))
            {
                _logger.LogWarning("Template HTML não encontrado ou vazio para incentivo para agendamento.");
                return null;
            }

            var renderedHtml = ApplyQuillLayout(htmlContent);
            var pdfBytes = await ConvertHtmlToPdfAsync(renderedHtml);

            var fileNameBase = contrato.FrAtendimentoVendaId.GetValueOrDefault(0) > 0
                ? contrato.FrAtendimentoVendaId.GetValueOrDefault(0).ToString()
                : contrato.NumeroContrato ?? contrato.FrAtendimentoVendaId.GetValueOrDefault(0).ToString();

            return new IncentivoParaAgendamentoEmailDataModel
            {
                HtmlContent = htmlContent,
                PdfBytes = pdfBytes,
                PdfFileName = $"incentivo_agendamento_{fileNameBase}.pdf",
                DadosBaseEnvio = contrato,
                DaysBefore = daysBefore,
                Subject = SubstituirPlaceholders(config.Subject ?? "Incentivo para Agendamento", statusAgendamento, contrato)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar incentivo para agendamento do contrato {ContratoNumero}", contrato.NumeroContrato);
            return null;
        }
    }

    /// <summary>
    /// Busca contratos elegíveis para receber incentivo de agendamento
    /// </summary>
    public async Task<List<(DadosContratoModel contrato, PosicaoAgendamentoViewModel statusAgendamento, int intervalo)>> GetContratosElegiveisAsync(
        EnumProjetoType projetoType,
        AutomaticCommunicationConfigModel config,
        List<DadosContratoModel> todosContratos,
        int ano,
        bool simulacao = false)
    {
        List<(DadosContratoModel contrato, PosicaoAgendamentoViewModel statusAgendamento, int intervalo)> contratosElegiveis = new();

        if (projetoType == EnumProjetoType.Multipropriedade)
        {
            var multiPropriedadeAtivada = _configuration.GetValue("MultipropriedadeAtivada", false);
            if (!multiPropriedadeAtivada)
                throw new ArgumentException("Funcionalidade de Multipropriedade desativada");

            var contratosComPendenciaAgendamento = await _empreendimentoProviderService.GetPosicaoAgendamentoAnoAsync(ano);

            // Pré-processar contratos ativos com email válido
            var contratosAtivos = todosContratos
                .Where(c => c.Status == "A" && 
                            !string.IsNullOrEmpty(c.PessoaTitular1Email) && 
                            c.PessoaTitular1Email.Contains("@"))
                .ToList();

            // Criar índice de agendamentos por chave composta (CotaNormalizada + NumeroImovel)
            var agendamentosIndex = contratosComPendenciaAgendamento
                .Where(a => !string.IsNullOrEmpty(a.CotaNome) &&
                           !string.IsNullOrEmpty(a.UhCondominioNumero) &&
                           a.QtdeSemanasDireitoUso.GetValueOrDefault(0) > 0 &&
                           a.QtdeReservas.GetValueOrDefault(0) < a.QtdeSemanasDireitoUso.GetValueOrDefault(0))
                .GroupBy(a => $"{a.CotaNome.RemoveAccents().ToUpperInvariant()}|{a.UhCondominioNumero}")
                .ToDictionary(g => g.Key, g => g.First());

            // HashSet para rastreamento rápido de contratos já adicionados
            var contratosAdicionadosIds = new HashSet<int>();

            foreach (var daysBefore in config.DaysBeforeCheckIn)
            {
                var dataAlvo = DateTime.Today.Date.AddDays(daysBefore);

                foreach (var contrato in contratosAtivos)
                {
                    // Se simulação e já encontrou um contrato, pular
                    if (simulacao && contratosAdicionadosIds.Any())
                        break;

                    // Pular se já foi adicionado
                    var contratoId = contrato.FrAtendimentoVendaId.GetValueOrDefault();
                    if (contratosAdicionadosIds.Contains(contratoId))
                        continue;

                    // Validações rápidas antes de buscar no índice
                    if (string.IsNullOrEmpty(contrato.GrupoCotaTipoCotaNome) || 
                        string.IsNullOrEmpty(contrato.NumeroImovel))
                        continue;

                    // Criar chave para lookup O(1)
                    var chave = $"{contrato.GrupoCotaTipoCotaNome.RemoveAccents().ToUpperInvariant()}|{contrato.NumeroImovel}";

                    // Buscar agendamento correspondente no índice
                    if (!agendamentosIndex.TryGetValue(chave, out var statusAgendamento))
                        continue;

                    // Verificar data de agendamento
                    var dataInicialAgendamento = statusAgendamento.DataInicialAgendamento.GetValueOrDefault();
                    
                    bool dataValida = simulacao 
                        ? DateTime.Today.Date >= dataInicialAgendamento.Date 
                        : DateTime.Today.Date == dataInicialAgendamento.Date;

                    if (!dataValida)
                        continue;

                    // Adicionar à lista de elegíveis
                    contratosElegiveis.Add((contrato, statusAgendamento, daysBefore));
                    contratosAdicionadosIds.Add(contratoId);

                    // Em simulação, sair após encontrar o primeiro
                    if (simulacao)
                        break;
                }

                // Em simulação, sair após encontrar um contrato
                if (simulacao && contratosAdicionadosIds.Any())
                    break;
            }

            return contratosElegiveis;
        }
        else
        {
            var timeSharingAtivado = _configuration.GetValue("TimeSharingAtivado", false);
            if (!timeSharingAtivado)
                throw new ArgumentException("Funcionalidade de Timesharing desativada");

            return new List<(DadosContratoModel contrato, PosicaoAgendamentoViewModel statusAgendamento, int intervalo)>();
        }
    }

    /// <summary>
    /// Verifica se deve enviar email para um contrato específico
    /// </summary>
    public async Task<bool> ShouldSendEmailForContrato(
        DadosContratoModel contrato,
        AutomaticCommunicationConfigModel config,
        List<ClientesInadimplentes>? inadimplentes)
    {
        try
        {
            if ((config.ExcludedStatusCrcIds == null || !config.ExcludedStatusCrcIds.Any()) &&
                !config.SendOnlyToAdimplentes)
            {
                return true;
            }

            if (contrato.Status != "A")
            {
                _logger.LogDebug("Contrato inativo: {NumeroContrato}", contrato.NumeroContrato);
                return false;
            }

            if (config.ExcludedStatusCrcIds != null && config.ExcludedStatusCrcIds.Any())
            {
                var statusCrcAtivos = contrato.frAtendimentoStatusCrcModels?
                    .Where(s => s.AtendimentoStatusCrcStatus == "A" && !string.IsNullOrEmpty(s.FrStatusCrcId))
                    .Select(s => int.Parse(s.FrStatusCrcId!))
                    .ToList() ?? new List<int>();

                if (statusCrcAtivos.Any(statusId => config.ExcludedStatusCrcIds.Contains(statusId)))
                {
                    _logger.LogDebug("Contrato {NumeroContrato} possui Status CRC excluído", contrato.NumeroContrato);
                    return false;
                }
            }

            if (config.SendOnlyToAdimplentes)
            {
                var temBloqueio = contrato.frAtendimentoStatusCrcModels?.Any(s =>
                    s.AtendimentoStatusCrcStatus == "A" &&
                    (s.BloquearCobrancaPagRec == "S" || s.BloqueaRemissaoBoletos == "S")) ?? false;

                var clienteInadimplente = inadimplentes?.FirstOrDefault(c =>
                    (c.CpfCnpj != null && contrato.PessoaTitular1CPF != null &&
                     c.CpfCnpj.ToString() == contrato.PessoaTitular1CPF) ||
                    (c.CpfCnpj != null && contrato.PessoaTitular2CPF != null &&
                     c.CpfCnpj.ToString() == contrato.PessoaTitular2CPF)
                );

                if (temBloqueio || clienteInadimplente != null)
                {
                    _logger.LogDebug("Contrato {NumeroContrato} possui inadimplência ou bloqueio", contrato.NumeroContrato);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar filtros para contrato {NumeroContrato}. Enviando por padrão.",
                contrato.NumeroContrato);
            return true;
        }
    }

    /// <summary>
    /// Substitui placeholders no texto para incentivo de agendamento
    /// </summary>
    public string SubstituirPlaceholders(
        string template,
        PosicaoAgendamentoViewModel statusAgendamento,
        DadosContratoModel contrato)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        try
        {
            var placeholderPattern = new System.Text.RegularExpressions.Regex(@"\{\{([^}]+)\}\}");
            var matches = placeholderPattern.Matches(template);

            var valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["NomeCliente"] = contrato.PessoaTitular1Nome ?? "",
                ["NomeTitular1"] = contrato.PessoaTitular1Nome ?? "",
                ["NomeTitular2"] = contrato.PessoaTitular2Nome ?? "",
                ["ContratoNumero"] = contrato.NumeroContrato ?? "",
                ["NumeroImovel"] = contrato.NumeroImovel ?? "",
                ["TipoCota"] = contrato.GrupoCotaTipoCotaNome ?? "",
                ["CPFTitular1"] = contrato.PessoaTitular1CPF ?? "",
                ["CPFTitular2"] = contrato.PessoaTitular2CPF ?? "",
                ["EmailTitular1"] = contrato.PessoaTitular1Email ?? "",
                ["EmailTitular2"] = contrato.PessoaTitular2Email ?? "",
                ["StatusContrato"] = contrato.Status ?? "",
                ["CotaNome"] = statusAgendamento.CotaNome ?? "",
                ["UhCondominioNumero"] = statusAgendamento.UhCondominioNumero ?? "",
                ["QtdeSemanasDireitoUso"] = statusAgendamento.QtdeSemanasDireitoUso?.ToString() ?? "0",
                ["QtdeReservas"] = statusAgendamento.QtdeReservas?.ToString() ?? "0",
                ["QtdePendente"] = (statusAgendamento.QtdeSemanasDireitoUso.GetValueOrDefault(0) - statusAgendamento.QtdeReservas.GetValueOrDefault(0)).ToString(),
                ["DataInicialAgendamento"] = statusAgendamento.DataInicialAgendamento?.ToString("dd/MM/yyyy") ?? "",
                ["DataFinalAgendamento"] = statusAgendamento.DataFinalAgendamento?.ToString("dd/MM/yyyy") ?? "",
                ["PeriodoAgendamentoFormatado"] = statusAgendamento.DataInicialAgendamento.HasValue && statusAgendamento.DataFinalAgendamento.HasValue
                    ? $"{statusAgendamento.DataInicialAgendamento.Value:dd/MM/yyyy} - {statusAgendamento.DataFinalAgendamento.Value:dd/MM/yyyy}"
                    : "",
                ["AnoReferencia"] = DateTime.Today.Month >= 6 ? DateTime.Today.AddYears(1).Year.ToString() : DateTime.Today.Year.ToString(),
                ["Empreendimento"] = "MY MABU",
                ["DataAtual"] = DateTime.Today.ToString("dd/MM/yyyy"),
                ["DataAtualExtenso"] = DateTime.Today.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR")),
                ["AnoAtual"] = DateTime.Today.Year.ToString(),
                ["MesAtual"] = DateTime.Today.ToString("MMMM", new System.Globalization.CultureInfo("pt-BR")),
            };

            var result = template;

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var placeholder = match.Value;
                var chave = match.Groups[1].Value;

                if (valores.TryGetValue(chave, out var valor))
                {
                    result = result.Replace(placeholder, valor);
                    _logger.LogDebug("Placeholder {Placeholder} substituído por: {Valor}", placeholder, valor);
                }
                else
                {
                    _logger.LogWarning("Placeholder {Placeholder} não encontrado no dicionário de valores", placeholder);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao substituir placeholders no template");
            return template;
        }
    }

    #region Métodos Auxiliares

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

    #endregion
}
