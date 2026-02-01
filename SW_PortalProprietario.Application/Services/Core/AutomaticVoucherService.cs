using CMDomain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core;

/// <summary>
/// Serviço de envio automático de vouchers
/// ? USA O MESMO CÓDIGO DA SIMULAÇÃO via VoucherGenerationService
/// </summary>
public class AutomaticVoucherService : IAutomaticVoucherService
{
    private readonly IRepositoryHosted _repository;
    private readonly ILogger<AutomaticVoucherService> _logger;
    private readonly IConfiguration _configuration;
    private readonly VoucherGenerationService _voucherGenerationService;
    private readonly IEmpreendimentoProviderService _empreendimentoProviderService;
    private readonly IEmailService _emailService;
    private readonly IServiceBase _serviceBase;

    public AutomaticVoucherService(
        IRepositoryHosted repository,
        ILogger<AutomaticVoucherService> logger,
        IConfiguration configuration,
        VoucherGenerationService voucherGenerationService,
        IEmpreendimentoProviderService empreendimentoProviderService,
        IEmailService emailService,
        IServiceBase serviceBase)
    {
        _repository = repository;
        _logger = logger;
        _configuration = configuration;
        _voucherGenerationService = voucherGenerationService;
        _emailService = emailService;
        _empreendimentoProviderService = empreendimentoProviderService;
        _serviceBase = serviceBase;
    }

    public async Task ProcessarReservasForDayMultiPropriedade(NHibernate.IStatelessSession session, 
        AutomaticCommunicationConfigModel config, 
        int daysBefore, 
        int? qtdeEnviar = null)
    {
        var contratos = await _serviceBase.GetContratos(new List<int>());
        var inadimplentes = await _empreendimentoProviderService.Inadimplentes();
        int qtdeEnviados = 0;

        try
        {
            var targetDate = DateTime.Today.AddDays(daysBefore);
            _logger.LogInformation("Processando reservas com check-in em {TargetDate} ({DaysBefore} dias)", targetDate, daysBefore);

            var reservas = await GetReservasByCheckInDateMultiPropriedade(targetDate);

            foreach (var agendamentoIdGroup in reservas.GroupBy(c => c.AgendamentoId))
            {
                var reserva = agendamentoIdGroup.First();
                try
                {
                    if (await CheckIfAlreadySentMultiPropriedade(session, reserva.ReservaId, daysBefore))
                    {
                        _logger.LogDebug("Email já enviado para reserva {ReservaId}", reserva.ReservaId);
                        continue;
                    }

                    if (!IsValidEmail(reserva.EmailCliente))
                    {
                        _logger.LogWarning("Email inválido para reserva {ReservaId}: {Email}", reserva.ReservaId, reserva.EmailCliente);
                        continue;
                    }

                    if (!await ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes))
                    {
                        _logger.LogDebug("Reserva {ReservaId} não atende critérios de filtro", reserva.ReservaId);
                        continue;
                    }

                    // ? USAR SERVIÇO COMPARTILHADO - MESMA LÓGICA DA SIMULAÇÃO
                    var voucherData = await _voucherGenerationService.GerarVoucherCompletoAsync(
                        agendamentoIdGroup.Key, false, config, contratos);

                    if (voucherData == null)
                    {
                        _logger.LogWarning("Não foi possível gerar voucher para reserva {ReservaId}", reserva.ReservaId);
                        continue;
                    }

                    var subject = _voucherGenerationService.SubstituirPlaceholders(
                        config.Subject ?? "Voucher da Reserva", voucherData.DadosReserva);

                    var emailBody = _voucherGenerationService.GerarCorpoEmailHtml(
                        voucherData.DadosReserva, daysBefore);

                    var enviarApenasPermitidos = _configuration.GetValue<bool>("EnviarEmailApenasParaDestinatariosPermitidos", true);
                    var destinatarioPermitido = _configuration.GetValue<string>("DestinatarioEmailPermitido", string.Empty);

                    var emailModel = new EmailInputInternalModel
                    {
                        Assunto = subject,
                        Destinatario = enviarApenasPermitidos ? destinatarioPermitido : reserva.EmailCliente,
                        ConteudoEmail = emailBody,
                        EmpresaId = 1,
                        UsuarioCriacao = 1,
                        Anexos = new List<EmailAnexoInputModel>
                        {
                            new EmailAnexoInputModel
                            {
                                NomeArquivo = voucherData.VoucherPdf.FileName,
                                TipoMime = "application/pdf",
                                Arquivo = voucherData.VoucherPdf.FileBytes
                            }
                        }
                    };

                    var emailSaved = await _emailService.SaveInternal(emailModel);
                    await RegisterSentEmailMultiPropriedade(session, reserva.ReservaId, daysBefore, reserva.DataCheckIn, emailSaved.Id);

                    _logger.LogInformation("Voucher enviado para reserva {ReservaId} (Email ID: {EmailId})", reserva.ReservaId, emailSaved.Id);

                    if (qtdeEnviar.HasValue && ++qtdeEnviados >= qtdeEnviar.Value)
                    {
                        _logger.LogInformation("Limite de {Limite} emails atingido", qtdeEnviar.Value);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar reserva {ReservaId}", reserva.ReservaId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar reservas para {DaysBefore} dias", daysBefore);
        }
    }

    public async Task ProcessarReservasForDayTimesharing(NHibernate.IStatelessSession session, 
        AutomaticCommunicationConfigModel config, 
        int daysBefore, 
        int? quantidadeMaximaEnvio = null)
    {
        int qtdeEnviados = 0;

        try
        {
            var targetDate = DateTime.Today.AddDays(daysBefore);
            _logger.LogInformation("Processando reservas Timesharing com check-in em {TargetDate} ({DaysBefore} dias)", targetDate, daysBefore);

            var reservas = await GetReservasByCheckInDateTimesharing(targetDate);

            foreach (var reservaIdGroup in reservas.GroupBy(r => r.ReservaId))
            {
                var reserva = reservaIdGroup.First();
                try
                {
                    if (await CheckIfAlreadySentTimesharing(session, reserva.ReservaId, daysBefore))
                    {
                        _logger.LogDebug("Email já enviado para reserva Timesharing {ReservaId}", reserva.ReservaId);
                        continue;
                    }

                    if (!IsValidEmail(reserva.EmailCliente))
                    {
                        _logger.LogWarning("Email inválido para reserva Timesharing {ReservaId}: {Email}", reserva.ReservaId, reserva.EmailCliente);
                        continue;
                    }

                    // ? USAR SERVIÇO COMPARTILHADO
                    var voucherData = await _voucherGenerationService.GerarVoucherCompletoAsync(
                        reserva.AgendamentoId, true, null);

                    if (voucherData == null)
                    {
                        _logger.LogWarning("Não foi possível gerar voucher para reserva Timesharing {ReservaId}", reserva.ReservaId);
                        continue;
                    }

                    var subject = _voucherGenerationService.SubstituirPlaceholders(
                        config.Subject ?? "Voucher da Reserva", voucherData.DadosReserva);

                    var emailBody = _voucherGenerationService.GerarCorpoEmailHtml(
                        voucherData.DadosReserva, daysBefore);

                    var enviarApenasPermitidos = _configuration.GetValue<bool>("EnviarEmailApenasParaDestinatariosPermitidos", false);
                    var destinatarioPermitido = _configuration.GetValue<string>("DestinatarioEmailPermitido", string.Empty);

                    var emailModel = new EmailInputInternalModel
                    {
                        Assunto = subject,
                        Destinatario = enviarApenasPermitidos ? destinatarioPermitido : reserva.EmailCliente,
                        ConteudoEmail = emailBody,
                        EmpresaId = 1,
                        UsuarioCriacao = 1,
                        Anexos = new List<EmailAnexoInputModel>
                        {
                            new EmailAnexoInputModel
                            {
                                NomeArquivo = voucherData.VoucherPdf.FileName,
                                TipoMime = "application/pdf",
                                Arquivo = voucherData.VoucherPdf.FileBytes
                            }
                        }
                    };

                    var emailSaved = await _emailService.SaveInternal(emailModel);
                    await RegisterSentEmailTimesharing(session, reserva.ReservaId, daysBefore, reserva.DataCheckIn, emailSaved.Id);

                    _logger.LogInformation("Voucher Timesharing enviado para reserva {ReservaId} (Email ID: {EmailId})", reserva.ReservaId, emailSaved.Id);

                    if (quantidadeMaximaEnvio.HasValue && ++qtdeEnviados >= quantidadeMaximaEnvio.Value)
                    {
                        _logger.LogInformation("Limite de {Limite} emails Timesharing atingido", quantidadeMaximaEnvio.Value);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar reserva Timesharing {ReservaId}", reserva.ReservaId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar reservas Timesharing para {DaysBefore} dias", daysBefore);
        }
    }

    #region Métodos Auxiliares

    private async Task<List<ReservaInfo>> GetReservasByCheckInDateMultiPropriedade(DateTime checkInDate)
    {
        if (!_configuration.GetValue<bool>("MultipropriedadeAtivada", false))
        {
            _logger.LogInformation("Multipropriedade desativada");
            return new List<ReservaInfo>();
        }

        try
        {
            return await _empreendimentoProviderService.GetReservasWithCheckInDateMultiPropriedadeAsync(checkInDate.Date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar reservas para {CheckInDate}", checkInDate);
            return new List<ReservaInfo>();
        }
    }

    private async Task<List<ReservaInfo>> GetReservasByCheckInDateTimesharing(DateTime checkInDate)
    {
        if (!_configuration.GetValue<bool>("TimeSharingAtivado", false))
        {
            _logger.LogInformation("Timesharing desativado");
            return new List<ReservaInfo>();
        }

        // TODO: Implementar busca Timesharing
        return new List<ReservaInfo>();
    }

    private async Task<bool> CheckIfAlreadySentMultiPropriedade(NHibernate.IStatelessSession session, long reservaId, int daysBefore)
    {
        try
        {
            var sent = await _repository.FindByHql<AutomaticCommunicationSent>(
                $"From AutomaticCommunicationSent a Where a.ReservaId = {reservaId} and a.DaysBeforeCheckIn = {daysBefore} and a.CommunicationType = 'VoucherReserva' and a.EmpreendimentoTipo = {(int)EnumProjetoType.Multipropriedade}",
                session);
            return sent.Any();
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckIfAlreadySentTimesharing(NHibernate.IStatelessSession session, long reservaId, int daysBefore)
    {
        try
        {
            var sent = await _repository.FindByHql<AutomaticCommunicationSent>(
                $"From AutomaticCommunicationSent a Where a.ReservaId = {reservaId} and a.DaysBeforeCheckIn = {daysBefore} and a.CommunicationType = 'VoucherReserva' and a.EmpreendimentoTipo = {(int)EnumProjetoType.Timesharing}",
                session);
            return sent.Any();
        }
        catch
        {
            return false;
        }
    }

    private async Task RegisterSentEmailMultiPropriedade(NHibernate.IStatelessSession session, long reservaId, int daysBefore, DateTime dataCheckIn, int? emailId)
    {
        try
        {
            _repository.BeginTransaction(session);
            var sent = new AutomaticCommunicationSent
            {
                CommunicationType = "VoucherReserva",
                ReservaId = reservaId,
                DaysBeforeCheckIn = daysBefore,
                DataCheckIn = dataCheckIn,
                DataEnvio = DateTime.Now,
                EmailId = emailId,
                DataHoraCriacao = DateTime.Now,
                EmpreendimentoTipo = EnumProjetoType.Multipropriedade
            };
            await _repository.ForcedSave(sent, session);
            await _repository.CommitAsync(session);
        }
        catch (Exception ex)
        {
            _repository.Rollback(session);
            _logger.LogError(ex, "Erro ao registrar envio para reserva {ReservaId}", reservaId);
        }
    }

    private async Task RegisterSentEmailTimesharing(NHibernate.IStatelessSession session, long reservaId, int daysBefore, DateTime dataCheckIn, int? emailId)
    {
        try
        {
            _repository.BeginTransaction(session);
            var sent = new AutomaticCommunicationSent
            {
                CommunicationType = "VoucherReserva",
                ReservaId = reservaId,
                DaysBeforeCheckIn = daysBefore,
                DataCheckIn = dataCheckIn,
                DataEnvio = DateTime.Now,
                EmailId = emailId,
                DataHoraCriacao = DateTime.Now,
                EmpreendimentoTipo = EnumProjetoType.Timesharing
            };
            await _repository.ForcedSave(sent, session);
            await _repository.CommitAsync(session);
        }
        catch (Exception ex)
        {
            _repository.Rollback(session);
            _logger.LogError(ex, "Erro ao registrar envio Timesharing para reserva {ReservaId}", reservaId);
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

    private async Task<bool> ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<Models.DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
    {
        try
        {
            if ((config.ExcludedStatusCrcIds == null || !config.ExcludedStatusCrcIds.Any()) && !config.SendOnlyToAdimplentes)
                return true;

            contratos ??= await _serviceBase.GetContratos(new List<int>());

            var contrato = contratos?.FirstOrDefault(c => 
                (!string.IsNullOrEmpty(reserva.CotaNome) && !string.IsNullOrEmpty(c.GrupoCotaTipoCotaNome) && c.GrupoCotaTipoCotaNome.Equals(reserva.CotaNome, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(reserva.UhCondominioNumero) && !string.IsNullOrEmpty(c.NumeroImovel) && c.NumeroImovel.Equals(reserva.UhCondominioNumero, StringComparison.OrdinalIgnoreCase))
            );

            if (contrato == null)
            {
                _logger.LogWarning("Contrato não encontrado para reserva {ReservaId}. Enviando por padrão.", reserva.ReservaId);
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
                    (c.CpfCnpj != null && contrato.PessoaTitular1CPF != null && c.CpfCnpj.ToString() == contrato.PessoaTitular1CPF) ||
                    (c.CpfCnpj != null && contrato.PessoaTitular2CPF != null && c.CpfCnpj.ToString() == contrato.PessoaTitular2CPF)
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
            _logger.LogError(ex, "Erro ao verificar filtros para reserva {ReservaId}. Enviando por padrão.", reserva.ReservaId);
            return true;
        }
    }

    #endregion
}
