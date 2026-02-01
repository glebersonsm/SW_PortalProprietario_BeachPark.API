using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using System.Collections.Generic;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.EnvioVoucher;

/// <summary>
/// Serviço auxiliar para simulação de emails de voucher
/// ? USA O MESMO CÓDIGO DO PROCESSAMENTO AUTOMÁTICO via VoucherGenerationService
/// </summary>
public class VoucherSimulationService
{
    private readonly ILogger<VoucherSimulationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceBase _serviceBase;
    private readonly VoucherGenerationService _voucherGenerationService;
    private readonly ICommunicationProvider _communicationProvider;

    public VoucherSimulationService(
        ILogger<VoucherSimulationService> logger,
        IConfiguration configuration,
        IServiceBase serviceBase,
        VoucherGenerationService voucherGenerationService,
        ICommunicationProvider communicationProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceBase = serviceBase;
        _voucherGenerationService = voucherGenerationService;
        _communicationProvider = communicationProvider;
    }

    public async Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId)
    {
        List<EmailInputInternalModel> emailListResult = new List<EmailInputInternalModel>();

        _logger.LogInformation("=== INÍCIO SIMULAÇÃO VOUCHER ===");
        
        var contratos = await _serviceBase.GetContratos(new List<int>());
        var inadimplentes = await _communicationProvider.Inadimplentes();

        var daysBefore = config.DaysBeforeCheckIn?.FirstOrDefault() ?? 0;
        var targetDate = DateTime.Today.AddDays(daysBefore);

        _logger.LogInformation("Buscando reservas para data: {TargetDate} ({DaysBefore} dias)", 
            targetDate.ToString("dd/MM/yyyy"), daysBefore);

        //Busca as reservas com base no filtro e data corrente
        var reservas = await GetReservasElegiveisAsync((EnumProjetoType)config.ProjetoType, config,true);

        if (reservas == null || !reservas.Any())
            throw new ArgumentException($"Nenhuma reserva compatível encontrada para simulação (check-in: {targetDate:dd/MM/yyyy}, {string.Join(",", config.DaysBeforeCheckIn ?? new List<int>())} dias)");

        _logger.LogInformation("Encontradas {Count} reservas para análise", reservas.Count);

        // Buscar o primeiro registro compatível que atenda aos filtros
        var resultItens = await FindCompatibleReservaAsync(reservas, config, contratos, inadimplentes);
        if (resultItens.reserva == null)
            throw new ArgumentException("Nenhuma reserva compatível encontrada que atenda aos filtros configurados");

        if (resultItens.reserva == null || !resultItens.agendamentoId.HasValue)
            throw new ArgumentException("Nenhuma reserva compatível encontrada que atenda aos filtros configurados");

        _logger.LogInformation("Reserva selecionada: AgendamentoId={AgendamentoId}, ReservaId={ReservaId}", 
            resultItens.agendamentoId, resultItens.reserva.ReservaId);

        // ? USAR SERVIÇO COMPARTILHADO - MESMA LÓGICA DO PROCESSAMENTO AUTOMÁTICO
        var isTimesharing = (EnumProjetoType)config.ProjetoType == EnumProjetoType.Timesharing;
        var voucherData = await _voucherGenerationService.GerarVoucherCompletoAsync(
            resultItens.agendamentoId.Value, 
            isTimesharing, 
            config,
            contratos);

        if (voucherData == null)
            throw new ArgumentException("Não foi possível gerar voucher para simulação");

        _logger.LogInformation("Voucher gerado com sucesso");

        // ? SUBSTITUIR PLACEHOLDERS USANDO SERVIÇO COMPARTILHADO
        var subject = _voucherGenerationService.SubstituirPlaceholders(
            config.Subject ?? "Voucher da Reserva", 
            voucherData.DadosReserva);

        _logger.LogInformation("Assunto processado: {Subject}", subject);

        // ? GERAR HTML USANDO SERVIÇO COMPARTILHADO
        var emailBody = _voucherGenerationService.GerarCorpoEmailHtml(
            voucherData.DadosReserva, 
            daysBefore);

        _logger.LogInformation("Corpo do email gerado - Tamanho: {Size} chars", emailBody.Length);

        var result = new EmailInputInternalModel
        {
            Assunto = $"[SIMULAÇÃO] {subject}",
            Destinatario = userEmail,
            ConteudoEmail = config.TemplateSendMode != EnumTemplateSendMode.AttachmentOnly ? voucherData.VoucherHtml : emailBody,
            EmpresaId = 1,
            UsuarioCriacao = userId,
            Anexos = config.TemplateSendMode != EnumTemplateSendMode.BodyHtmlOnly ? new List<EmailAnexoInputModel>
            {
                new EmailAnexoInputModel
                {
                    NomeArquivo = voucherData.VoucherPdf.FileName,
                    TipoMime = "application/pdf",
                    Arquivo = voucherData.VoucherPdf.FileBytes
                }
            } : null
        };

        _logger.LogInformation("=== FIM SIMULAÇÃO VOUCHER ===");

        emailListResult.Add(result);


        return emailListResult;
    }

    #region Métodos Auxiliares (Filtros e Validações)

    private async Task<List<(ReservaInfo reserva, int intervalo)>?> GetReservasElegiveisAsync(EnumProjetoType projetoType, AutomaticCommunicationConfigModel config, bool simulacao = false)
    {
        List<(ReservaInfo reserva, int intervalo)> reservasElegiveis = new List<(ReservaInfo reserva, int intervalo)>();

        if (config == null || config.DaysBeforeCheckIn == null || !config.DaysBeforeCheckIn.Any())
            throw new ArgumentException("Configuração inválida: Dias antes do check-in não especificados");

        if (projetoType == EnumProjetoType.Multipropriedade)
        {
            var multiPropriedadeAtivada = _configuration.GetValue("MultipropriedadeAtivada", false);
            if (!multiPropriedadeAtivada)
                throw new ArgumentException("Funcionalidade de Multipropriedade desativada");

            foreach (var item in config.DaysBeforeCheckIn)
            {
                var checkinNaData = await _communicationProvider.GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime.Today.AddDays(item).Date, simulacao);
                if (checkinNaData != null && checkinNaData.Any())
                {
                    reservasElegiveis.AddRange((IEnumerable<(ReservaInfo reserva, int intervalo)>)checkinNaData.Select(r => (r, item)));
                }
            }


            return reservasElegiveis;
        }
        else if (projetoType == EnumProjetoType.Timesharing)
        {
            var timeSharingAtivado = _configuration.GetValue("TimeSharingAtivado", false);
            if (!timeSharingAtivado)
                throw new ArgumentException("Funcionalidade de Timesharing desativada");

            foreach (var item in config.DaysBeforeCheckIn)
            {
                var checkinNaData = await _communicationProvider.GetReservasWithCheckInDateTimeSharingAsync(DateTime.Today.AddDays(item).Date, simulacao);
                if (checkinNaData != null && checkinNaData.Any())
                {
                    reservasElegiveis.AddRange((IEnumerable<(ReservaInfo reserva, int intervalo)>)checkinNaData.Select(r => (r, item)));
                }
            }

            return reservasElegiveis;
        }

        return default;
    }

    private async Task<(ReservaInfo? reserva, int? agendamentoId, int intervalo)> FindCompatibleReservaAsync(
        List<(ReservaInfo reserva, int intervalo)> reservas,
        AutomaticCommunicationConfigModel config,
        List<DadosContratoModel> contratos,
        List<ClientesInadimplentes> inadimplentes)
    {
        foreach (var agendamentoIdGroup in reservas.GroupBy(c => c.reserva.AgendamentoId))
        {
            var reservaItem = agendamentoIdGroup.First();

            if (!IsValidEmail(reservaItem.reserva.EmailCliente))
                continue;

            if (await ShouldSendEmailForReserva(reservaItem.reserva, config, contratos, inadimplentes) == false)
                continue;

            return (reservaItem.reserva, Convert.ToInt32(reservaItem.reserva.AgendamentoId), reservaItem.intervalo);
        }

        return (null, null, 0);
    }

    private async Task<bool?> ShouldSendEmailForReserva(
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

            return _communicationProvider.ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes);

           
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
