using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.AvisoCheckin;

/// <summary>
/// Serviço auxiliar para simulação de emails de aviso de check-in próximo
/// ? USA O MESMO CÓDIGO DO PROCESSAMENTO AUTOMÁTICO via AvisoCheckinGenerationService
/// </summary>
public class AvisoCheckinSimulationService
{
    private readonly ILogger<AvisoCheckinSimulationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmpreendimentoHybridProviderService _empreendimentoProviderService;
    private readonly IServiceBase _serviceBase;
    private readonly AvisoCheckinGenerationService _avisoGenerationService;

    public AvisoCheckinSimulationService(
        ILogger<AvisoCheckinSimulationService> logger,
        IConfiguration configuration,
        IEmpreendimentoHybridProviderService empreendimentoProviderService,
        IServiceBase serviceBase,
        AvisoCheckinGenerationService avisoGenerationService)
    {
        _logger = logger;
        _configuration = configuration;
        _empreendimentoProviderService = empreendimentoProviderService;
        _serviceBase = serviceBase;
        _avisoGenerationService = avisoGenerationService;
    }

    public async Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId)
    {
        if (config.TemplateSendMode == null)
            throw new ArgumentException("TemplateSendMode não configurado");

        List<EmailInputInternalModel> emailListResult = new List<EmailInputInternalModel>();

        _logger.LogInformation("=== INÍCIO SIMULAÇÃO AVISO CHECK-IN ===");

        // Usar o primeiro dia configurado para buscar reservas
        var daysBefore = config.DaysBeforeCheckIn?.FirstOrDefault() ?? 0;
        var targetDate = DateTime.Today.AddDays(daysBefore);

        _logger.LogInformation("Buscando reservas para data: {TargetDate} ({DaysBefore} dias)", 
            targetDate.ToString("dd/MM/yyyy"), daysBefore);

        // Buscar reservas compatíveis
        var reservaItens = await GetReservasAsync((EnumProjetoType)config.ProjetoType, config, true);

        if (reservaItens == null || !reservaItens.Any())
            throw new ArgumentException($"Nenhuma reserva compatível encontrada para simulação (check-in: {targetDate:dd/MM/yyyy}, {string.Join(",", config.DaysBeforeCheckIn ?? new List<int>())} dias)");

        _logger.LogInformation("Encontradas {Count} reservas para análise", reservaItens.Count);

        // Buscar o primeiro registro compatível que atenda aos filtros
        var resultItens = await FindCompatibleReservaAsync(reservaItens, config);

        if (resultItens.reserva == null || resultItens.dadosReserva == null)
            throw new ArgumentException("Nenhuma reserva compatível encontrada que atenda aos filtros configurados");

        _logger.LogInformation("Reserva selecionada: AgendamentoId={AgendamentoId}, ReservaId={ReservaId}", 
            resultItens.reserva.AgendamentoId, resultItens.reserva.ReservaId);

        // ? DETERMINAR MODO DE ENVIO
        var sendMode = (EnumTemplateSendMode)config.TemplateSendMode;
        _logger.LogInformation("Modo de envio configurado: {SendMode}", sendMode);

        // ? GERAR AVISO COMPLETO USANDO SERVIÇO COMPARTILHADO (MESMA LÓGICA DO PROCESSAMENTO AUTOMÁTICO)
        var avisoData = await _avisoGenerationService.GerarAvisoCompletoAsync(
            resultItens.reserva,
            resultItens.dadosReserva,
            daysBefore,
            config.TemplateId,
            sendMode);

        if (avisoData == null || string.IsNullOrEmpty(avisoData.HtmlContent))
            throw new ArgumentException("Não foi possível gerar aviso para simulação");

        _logger.LogInformation("Aviso gerado com sucesso");

        // ? SUBSTITUIR PLACEHOLDERS NO ASSUNTO USANDO SERVIÇO COMPARTILHADO
        var subject = _avisoGenerationService.SubstituirPlaceholders(
            config.Subject ?? "Aviso de Check-in Próximo", 
            resultItens.reserva, 
            resultItens.dadosReserva, 
            daysBefore);

        _logger.LogInformation("Assunto processado: {Subject}", subject);
        _logger.LogInformation("Corpo do email gerado - Tamanho: {Size} chars", avisoData.HtmlContent.Length);

        // ? CRIAR EMAIL COM ANEXO SE NECESSÁRIO (MESMO COMPORTAMENTO DO PROCESSAMENTO AUTOMÁTICO)
        var result = new EmailInputInternalModel
        {
            Assunto = $"[SIMULAÇÃO] {subject}",
            Destinatario = userEmail,
            ConteudoEmail = avisoData.HtmlContent,
            EmpresaId = 1,
            UsuarioCriacao = userId
        };

        // Adicionar anexo se modo configurado incluir anexo E tiver PDF
        if ((sendMode == EnumTemplateSendMode.AttachmentOnly || sendMode == EnumTemplateSendMode.BodyHtmlAndAttachment) 
            && avisoData.PdfBytes != null && avisoData.PdfBytes.Length > 0)
        {
            result.Anexos = new List<EmailAnexoInputModel>
            {
                new EmailAnexoInputModel
                {
                    NomeArquivo = avisoData.PdfFileName!,
                    TipoMime = "application/pdf",
                    Arquivo = avisoData.PdfBytes
                }
            };

            _logger.LogInformation("Anexo PDF adicionado ao email: {FileName} ({Size} bytes)", 
                avisoData.PdfFileName, avisoData.PdfBytes.Length);
        }

        _logger.LogInformation("=== FIM SIMULAÇÃO AVISO CHECK-IN ===");

        emailListResult.Add(result);

        return emailListResult;
    }


    #region Métodos Auxiliares (Filtros e Validações)

    private async Task<List<(ReservaInfo reserva, int intervalo)>> GetReservasAsync(EnumProjetoType projetoType, AutomaticCommunicationConfigModel config, bool simulacao = true)
    {
        List<(ReservaInfo reserva, int intervalo)> reservasElegiveis = new List<(ReservaInfo reserva, int intervalo)>();

        if (projetoType == EnumProjetoType.Multipropriedade)
        {
            var multiPropriedadeAtivada = _configuration.GetValue("MultipropriedadeAtivada", false);
            if (!multiPropriedadeAtivada)
                throw new ArgumentException("Funcionalidade de Multipropriedade desativada");

            foreach (var item in config.DaysBeforeCheckIn)
            {
                var checkinNaData = await _empreendimentoProviderService.GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime.Today.AddDays(item).Date, simulacao);
                if (checkinNaData != null && checkinNaData.Any())
                {
                    reservasElegiveis.AddRange(checkinNaData.Select(r => (r, item)));
                }
            }

            return reservasElegiveis;
        }
        else
        {
            var timeSharingAtivado = _configuration.GetValue("TimeSharingAtivado", false);
            if (!timeSharingAtivado)
                throw new ArgumentException("Funcionalidade de Timesharing desativada");

            // TODO: Implementar busca de reservas Timesharing quando disponível
            return default;
        }
    }

    private async Task<(ReservaInfo? reserva, DadosImpressaoVoucherResultModel? dadosReserva, int intervalo)> FindCompatibleReservaAsync(
        List<(ReservaInfo reserva, int intervalo)> reservas,
        AutomaticCommunicationConfigModel config)
    {
        var contratos = await _serviceBase.GetContratos(new List<int>());
        var inadimplentes = await _empreendimentoProviderService.Inadimplentes();

        foreach (var reservaItem in reservas.GroupBy(c=> c.reserva.AgendamentoId))
        {
            var reserva = reservaItem.First().reserva;
            // Verificar se o cliente tem email válido
            if (!IsValidEmail(reserva.EmailCliente))
            {
                _logger.LogDebug("Email inválido para reserva {ReservaId}: {Email}", 
                    reserva.ReservaId, reserva.EmailCliente);
                continue;
            }

            // Verificar filtros de status CRC e adimplência
            if (!await ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes))
            {
                _logger.LogDebug("Reserva {ReservaId} não atende critérios de filtro", reserva.ReservaId);
                continue;
            }

            // Buscar dados completos da reserva
            try
            {
                var dadosReserva = await _empreendimentoProviderService.GetDadosImpressaoVoucher($"{reserva.AgendamentoId}");
                if (dadosReserva != null)
                {
                    return (reserva, dadosReserva, reservaItem.First().intervalo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar dados da reserva {ReservaId}", reserva.ReservaId);
            }
        }

        return (null, null, 0);
    }

    private async Task<bool> ShouldSendEmailForReserva(
        ReservaInfo reserva,
        AutomaticCommunicationConfigModel config,
        List<DadosContratoModel>? contratos,
        List<ClientesInadimplentes>? inadimplentes)
    {
        try
        {
            if ((config.ExcludedStatusCrcIds == null || !config.ExcludedStatusCrcIds.Any()) && 
                !config.SendOnlyToAdimplentes)
            {
                return true;
            }

            contratos ??= await _serviceBase.GetContratos(new List<int>());

            var contrato = contratos?.FirstOrDefault(c =>
                !string.IsNullOrEmpty(reserva.CotaNome) && !string.IsNullOrEmpty(c.GrupoCotaTipoCotaNome) && 
                 c.GrupoCotaTipoCotaNome.Equals(reserva.CotaNome, StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrEmpty(reserva.UhCondominioNumero) && !string.IsNullOrEmpty(c.NumeroImovel) && 
                 c.NumeroImovel.Equals(reserva.UhCondominioNumero, StringComparison.OrdinalIgnoreCase)
            );

            if (contrato == null)
            {
                _logger.LogWarning("Contrato não encontrado para reserva {ReservaId}. Considerando compatível para simulação.", 
                    reserva.ReservaId);
                return true;
            }

            if (contrato.Status != "A")
            {
                _logger.LogDebug("Contrato inativo para reserva {ReservaId}", reserva.ReservaId);
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
                    _logger.LogDebug("Reserva {ReservaId} possui Status CRC excluído", reserva.ReservaId);
                    return false;
                }
            }

            if (config.SendOnlyToAdimplentes)
            {
                var temBloqueio = contrato.frAtendimentoStatusCrcModels?.Any(s =>
                    s.AtendimentoStatusCrcStatus == "A" &&
                    (s.BloquearCobrancaPagRec == "S" || s.BloqueaRemissaoBoletos == "S")) ?? false;

                var clienteInadimplente = inadimplentes?.FirstOrDefault(c =>
                    c.CpfCnpj != null && contrato.PessoaTitular1CPF != null && 
                     c.CpfCnpj.ToString() == contrato.PessoaTitular1CPF ||
                    c.CpfCnpj != null && contrato.PessoaTitular2CPF != null && 
                     c.CpfCnpj.ToString() == contrato.PessoaTitular2CPF
                );

                if (temBloqueio || clienteInadimplente != null)
                {
                    _logger.LogDebug("Reserva {ReservaId} possui inadimplência ou bloqueio", reserva.ReservaId);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar filtros para reserva {ReservaId}. Considerando compatível para simulação.", 
                reserva.ReservaId);
            return true;
        }
    }

    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
