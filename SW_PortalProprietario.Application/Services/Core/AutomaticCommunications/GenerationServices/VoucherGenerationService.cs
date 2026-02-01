using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using System.Text;
using System.Text.RegularExpressions;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;

/// <summary>
/// Serviço compartilhado para geração de vouchers
/// Usado tanto na simulação quanto no processamento automático
/// </summary>
public class VoucherGenerationService
{
    private readonly ILogger<VoucherGenerationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IVoucherReservaService _voucherReservaService;

    public VoucherGenerationService(
        ILogger<VoucherGenerationService> logger,
        IConfiguration configuration,
        IVoucherReservaService voucherReservaService)
    {
        _logger = logger;
        _configuration = configuration;
        _voucherReservaService = voucherReservaService;
    }

    /// <summary>
    /// Gera voucher completo com todas as informações
    /// </summary>
    public async Task<VoucherEmailDataModel?> GerarVoucherCompletoAsync(
        long agendamentoId,
        bool isTimesharing,
        AutomaticCommunicationConfigModel config,
        List<DadosContratoModel>? contratos = null)
    {
        try
        {
            _logger.LogInformation("Gerando voucher para agendamento {AgendamentoId} (Timesharing: {IsTimesharing})", 
                agendamentoId, isTimesharing);

            // Gerar voucher PDF
            var voucherResult = await _voucherReservaService.GerarVoucherAsync(
                agendamentoId, 
                isTimesharing,
                contratos,
                config);

            if (voucherResult == null || voucherResult.FileBytes == null || voucherResult.FileBytes.Length == 0)
            {
                _logger.LogWarning("Não foi possível gerar voucher para agendamento {AgendamentoId}", agendamentoId);
                return null;
            }

            if (voucherResult.DadosImpressao == null)
            {
                _logger.LogWarning("Voucher gerado mas sem dados de impressão para agendamento {AgendamentoId}", agendamentoId);
                return null;
            }

            return new VoucherEmailDataModel
            {
                VoucherPdf = voucherResult,
                DadosReserva = voucherResult.DadosImpressao,
                VoucherHtml = voucherResult.HtmlFull
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar voucher para agendamento {AgendamentoId}", agendamentoId);
            return null;
        }
    }

    /// <summary>
    /// Substitui placeholders no texto (assunto ou corpo)
    /// </summary>
    public string SubstituirPlaceholders(string texto, DadosImpressaoVoucherResultModel dadosReserva)
    {
        if (string.IsNullOrWhiteSpace(texto) || dadosReserva == null)
            return texto;

        var resultado = texto;

        // Substituições diretas (case-insensitive)
        resultado = ReplaceIgnoreCase(resultado, "{{NumeroReserva}}", dadosReserva.NumeroReserva ?? dadosReserva.NumReserva?.ToString() ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{Contrato}}", dadosReserva.Contrato ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{NomeCliente}}", dadosReserva.NomeCliente ?? dadosReserva.HospedePrincipal ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{CessionarioNome}}", dadosReserva.NomeCliente ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{HospedePrincipal}}", dadosReserva.HospedePrincipal ?? dadosReserva.NomeCliente ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{DataCheckIn}}", ParseDateSafe(dadosReserva.DataChegada));
        resultado = ReplaceIgnoreCase(resultado, "{{DataCheckOut}}", ParseDateSafe(dadosReserva.DataPartida));
        resultado = ReplaceIgnoreCase(resultado, "{{CheckInData}}", ParseDateSafe(dadosReserva.DataChegada));
        resultado = ReplaceIgnoreCase(resultado, "{{CheckOutData}}", ParseDateSafe(dadosReserva.DataPartida));
        resultado = ReplaceIgnoreCase(resultado, "{{LocalAtendimento}}", dadosReserva.LocalAtendimento ?? "Equipe MY Mabu");
        resultado = ReplaceIgnoreCase(resultado, "{{Hotel}}", dadosReserva.NomeHotel ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{HotelNome}}", dadosReserva.NomeHotel ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{TipoApartamento}}", dadosReserva.TipoApartamento ?? "");
        resultado = ReplaceIgnoreCase(resultado, "{{QuantidadePax}}", dadosReserva.QuantidadePax ?? "");

        // Regex para capturar placeholders restantes
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.IgnoreCase);
        resultado = regex.Replace(resultado, match =>
        {
            var key = match.Groups[1].Value;
            var value = ObterValorPlaceholder(dadosReserva, key);
            return value ?? match.Value; // Se não encontrar, mantém o placeholder
        });

        return resultado;
    }

    /// <summary>
    /// Gera corpo HTML do email com voucher
    /// </summary>
    public string GerarCorpoEmailHtml(DadosImpressaoVoucherResultModel dadosReserva, int? diasAntes = null)
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
        sb.AppendLine(".info-box { background-color: #e3f2fd; padding: 20px; border-left: 4px solid #2196f3; margin: 20px 0; border-radius: 4px; }");
        sb.AppendLine(".info-box p { margin: 8px 0; }");
        sb.AppendLine(".voucher-info { margin: 25px 0; padding: 25px; background-color: #f5f5f5; border-radius: 8px; border: 2px solid #2196f3; text-align: center; }");
        sb.AppendLine(".attachment-note { font-size: 14px; color: #666; margin-top: 20px; padding: 15px; background-color: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px; }");
        sb.AppendLine(".countdown { text-align: center; margin: 20px 0; padding: 20px; background-color: #e8f5e9; border-radius: 8px; }");
        sb.AppendLine(".countdown-number { font-size: 48px; font-weight: bold; color: #2e7d32; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");
        
        // Header
        sb.AppendLine("<div class=\"header\">");
        sb.AppendLine("<h1 style=\"margin: 0; font-size: 28px;\">📄 Voucher da sua Reserva</h1>");
        sb.AppendLine("</div>");
        
        // Saudação
        var nomeCliente = dadosReserva.NomeCliente ?? dadosReserva.HospedePrincipal ?? "Cliente";
        sb.AppendLine($"<p style=\"font-size: 16px;\">Olá <strong>{nomeCliente}</strong>,</p>");
        
        // Contagem regressiva (se aplicável)
        if (diasAntes.HasValue && diasAntes.Value > 0)
        {
            sb.AppendLine("<div class=\"countdown\">");
            sb.AppendLine($"<div class=\"countdown-number\">{diasAntes.Value}</div>");
            sb.AppendLine($"<p style=\"margin: 10px 0 0 0; font-size: 18px; color: #2e7d32;\">dia{(diasAntes.Value != 1 ? "s" : "")} para o seu check-in!</p>");
            sb.AppendLine("</div>");
        }
        
        sb.AppendLine("<p style=\"font-size: 16px;\">Segue o voucher da sua reserva.</p>");
        
        // Box de informações
        sb.AppendLine("<div class=\"info-box\">");
        sb.AppendLine("<h3 style=\"margin-top: 0; color: #1976d2;\">📋 Detalhes da Reserva</h3>");
        sb.AppendLine($"<p><strong>Número da Reserva:</strong> {dadosReserva.NumeroReserva ?? dadosReserva.NumReserva?.ToString() ?? "N/A"}</p>");
        
        if (!string.IsNullOrEmpty(dadosReserva.NomeHotel))
            sb.AppendLine($"<p><strong>Hotel:</strong> {dadosReserva.NomeHotel}</p>");
        
        if (!string.IsNullOrEmpty(dadosReserva.DataChegada))
            sb.AppendLine($"<p><strong>Check-in:</strong> {dadosReserva.DataChegada}</p>");
        
        if (!string.IsNullOrEmpty(dadosReserva.DataPartida))
            sb.AppendLine($"<p><strong>Check-out:</strong> {dadosReserva.DataPartida}</p>");
        
        if (!string.IsNullOrEmpty(dadosReserva.TipoApartamento))
            sb.AppendLine($"<p><strong>Tipo de Apartamento:</strong> {dadosReserva.TipoApartamento}</p>");
        
        if (!string.IsNullOrEmpty(dadosReserva.QuantidadePax))
            sb.AppendLine($"<p><strong>Hóspedes:</strong> {dadosReserva.QuantidadePax}</p>");
        
        sb.AppendLine("</div>");
        
        // Info sobre voucher em anexo
        sb.AppendLine("<div class=\"voucher-info\">");
        sb.AppendLine("<h3 style=\"color: #1976d2; margin-top: 0;\">📎 Voucher em Anexo</h3>");
        sb.AppendLine("<p style=\"font-size: 16px;\">O <strong>voucher da sua reserva</strong> está disponível como <strong>anexo em PDF</strong> neste email.</p>");
        sb.AppendLine("<p style=\"font-size: 14px; color: #666;\">Por favor, apresente o voucher no check-in.</p>");
        sb.AppendLine("</div>");
        
        // Nota sobre anexo
        sb.AppendLine("<div class=\"attachment-note\">");
        sb.AppendLine("⚠️ <strong>Importante:</strong> Verifique a seção de anexos do seu cliente de email para visualizar ou baixar o voucher em PDF.");
        sb.AppendLine("</div>");
        
        // Assinatura
        var localAtendimento = dadosReserva.LocalAtendimento ?? "Equipe MY Mabu";
        sb.AppendLine($"<p style=\"margin-top: 30px; font-size: 16px;\">Atenciosamente,<br/><strong>{localAtendimento}</strong></p>");
        
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

    private string? ObterValorPlaceholder(DadosImpressaoVoucherResultModel dados, string key)
    {
        return key.ToLowerInvariant() switch
        {
            "numeroreserva" => dados.NumeroReserva ?? dados.NumReserva?.ToString(),
            "contrato" => dados.Contrato,
            "nomecliente" or "cessionarionome" => dados.NomeCliente ?? dados.HospedePrincipal,
            "hospedeprincipal" => dados.HospedePrincipal ?? dados.NomeCliente,
            "datacheckin" or "checkindata" => ParseDateSafe(dados.DataChegada),
            "datacheckout" or "checkoutdata" => ParseDateSafe(dados.DataPartida),
            "localatendimento" => dados.LocalAtendimento ?? "Equipe MY Mabu",
            "hotel" or "hotelnome" => dados.NomeHotel,
            "tipoapartamento" => dados.TipoApartamento,
            "quantidadepax" => dados.QuantidadePax,
            "datachegada" => dados.DataChegada,
            "datapartida" => dados.DataPartida,
            "horachegada" => dados.HoraChegada,
            "horapartida" => dados.HoraPartida,
            "tipoutilizacao" => dados.TipoUtilizacao,
            "observacao" => dados.Observacao,
            "acomodacao" => dados.Acomodacao,
            _ => null
        };
    }

    private string ParseDateSafe(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return "";
        
        if (DateTime.TryParse(dateStr, out var date))
            return date.ToString("dd/MM/yyyy");
        
        return dateStr;
    }

    internal async Task GerarVoucherCompletoAsync(ReservaInfo reserva, bool isTimesharing, AutomaticCommunicationConfigModel config, List<DadosContratoModel> contratos)
    {
        throw new NotImplementedException();
    }

    #endregion
}
