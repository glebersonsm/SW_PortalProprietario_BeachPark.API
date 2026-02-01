using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.Voucher;

/// <summary>
/// Serviço de processamento automático de vouchers de reserva
/// ? Usado pelo handler para envio em background
/// </summary>
public class VoucherProcessingService
{
    private readonly ILogger<VoucherProcessingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceBase _serviceBase;
    private readonly ICommunicationProvider _communicationProvider;
    private readonly IEmailService _emailService;
    private readonly VoucherGenerationService _generationService;

    public VoucherProcessingService(
        ILogger<VoucherProcessingService> logger,
        IConfiguration configuration,
        IServiceBase serviceBase,
        ICommunicationProvider communicationProvider,
        IEmailService emailService,
        VoucherGenerationService generationService)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceBase = serviceBase;
        _emailService = emailService;
        _generationService = generationService;
        _communicationProvider = communicationProvider;
    }

    public async Task ProcessarVouchersMultiPropriedadeAsync(
        IStatelessSession session,
        AutomaticCommunicationConfigModel config,
        int daysBefore,
        int? qtdeMaxima = null)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO VOUCHERS MULTIPROPRIEDADE - {DaysBefore} dias ===", daysBefore);

        try
        {
            var multiPropriedadeAtivada = _configuration.GetValue("MultipropriedadeAtivada", false);
            if (!multiPropriedadeAtivada)
            {
                _logger.LogWarning("Multipropriedade não está ativada");
                return;
            }

            var targetDate = DateTime.Today.AddDays(daysBefore);
            _logger.LogInformation("Data alvo: {TargetDate}", targetDate.ToString("dd/MM/yyyy"));

            // Buscar reservas
            var reservas = await _communicationProvider.GetReservasWithCheckInDateMultiPropriedadeAsync(targetDate, false);
            if (reservas == null || !reservas.Any())
            {
                _logger.LogInformation("Nenhuma reserva encontrada para {TargetDate}", targetDate.ToString("dd/MM/yyyy"));
                return;
            }

            _logger.LogInformation("Encontradas {Count} reservas para processar", reservas.Count);

            // Buscar contratos e inadimplentes
            var contratos = await _serviceBase.GetContratos(new List<int>());
            var inadimplentes = await _communicationProvider.Inadimplentes();

            var enviados = 0;
            var erros = 0;

            foreach (var agendamentoGroup in reservas.GroupBy(r => r.AgendamentoId))
            {
                if (qtdeMaxima.HasValue && enviados >= qtdeMaxima.Value)
                {
                    _logger.LogInformation("Limite de {Limite} envios atingido", qtdeMaxima.Value);
                    break;
                }

                var reserva = agendamentoGroup.First();

                try
                {
                    // Validar email
                    if (string.IsNullOrWhiteSpace(reserva.EmailCliente) || !IsValidEmail(reserva.EmailCliente))
                    {
                        _logger.LogDebug("Email inválido para reserva {ReservaId}", reserva.ReservaId);
                        continue;
                    }

                    // Aplicar filtros
                    if (!_communicationProvider.ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes).GetValueOrDefault(false))
                        continue;

                    // Gerar voucher completo
                    var voucherData = await _generationService.GerarVoucherCompletoAsync(
                        agendamentoGroup.Key,
                        false,
                        config,
                        contratos);

                    if (voucherData == null)
                    {
                        _logger.LogWarning("Não foi possível gerar voucher para agendamento {AgendamentoId}", agendamentoGroup.Key);
                        erros++;
                        continue;
                    }

                    var enviarEmailApenasParaEmailsAutorizados = _configuration.GetValue<bool>("EnviarEmailApenasParaDestinatariosPermitidos", true);
                    var emailsPermitidos = enviarEmailApenasParaEmailsAutorizados ? _configuration.GetValue<string>("DestinatarioEmailPermitido", "glebersonsm@gmail.com") : null;

                    // Montar email
                    var subject = _generationService.SubstituirPlaceholders(
                        config.Subject ?? "Voucher da Reserva",
                        voucherData.DadosReserva);

                    var emailModel = new EmailInputInternalModel
                    {
                        Assunto = subject,
                        Destinatario = enviarEmailApenasParaEmailsAutorizados ? emailsPermitidos : reserva.EmailCliente,
                        ConteudoEmail = config.TemplateSendMode != EnumTemplateSendMode.AttachmentOnly
                            ? voucherData.VoucherHtml
                            : _generationService.GerarCorpoEmailHtml(voucherData.DadosReserva, daysBefore),
                        EmpresaId = 1,
                        UsuarioCriacao = 1,
                        Anexos = config.TemplateSendMode != EnumTemplateSendMode.BodyHtmlOnly
                            ? new List<EmailAnexoInputModel>
                            {
                                new EmailAnexoInputModel
                                {
                                    NomeArquivo = voucherData.VoucherPdf.FileName,
                                    TipoMime = "application/pdf",
                                    Arquivo = voucherData.VoucherPdf.FileBytes
                                }
                            }
                            : null
                    };

                    await _emailService.SaveInternal(emailModel); // ✅ Método correto

                    enviados++;
                    _logger.LogInformation("Voucher enviado - Reserva: {ReservaId}, Email: {Email}",
                        reserva.ReservaId, reserva.EmailCliente);
                }
                catch (Exception ex)
                {
                    erros++;
                    _logger.LogError(ex, "Erro ao processar voucher para reserva {ReservaId}", reserva.ReservaId);
                }
            }

            _logger.LogInformation("=== FIM PROCESSAMENTO - Enviados: {Enviados}, Erros: {Erros} ===", enviados, erros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no processamento de vouchers Multipropriedade");
            throw;
        }
    }

    public async Task ProcessarVouchersTimesharingAsync(
        IStatelessSession session,
        AutomaticCommunicationConfigModel config,
        int daysBefore,
        int? qtdeMaxima = null)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO VOUCHERS TIMESHARING - {DaysBefore} dias ===", daysBefore);

        try
        {
            var timeSharingAtivado = _configuration.GetValue("TimeSharingAtivado", false);
            if (!timeSharingAtivado)
            {
                _logger.LogWarning("Timesharing não está ativado");
                return;
            }


            var targetDate = DateTime.Today.AddDays(daysBefore);
            _logger.LogInformation("Data alvo: {TargetDate}", targetDate.ToString("dd/MM/yyyy"));

            // Buscar reservas
            var reservas = await _communicationProvider.GetReservasWithCheckInDateTimeSharingAsync(targetDate, false);

            if (reservas == null || !reservas.Any())
            {
                _logger.LogInformation("Nenhuma reserva encontrada para {TargetDate}", targetDate.ToString("dd/MM/yyyy"));
                return;
            }

            _logger.LogInformation("Encontradas {Count} reservas para processar", reservas.Count);

            // Buscar contratos e inadimplentes
            var contratos = await _serviceBase.GetContratos(new List<int>());
            var inadimplentes = await _communicationProvider.Inadimplentes();

            var enviados = 0;
            var erros = 0;

            foreach (var agendamentoGroup in reservas.GroupBy(r => r.AgendamentoId))
            {
                if (qtdeMaxima.HasValue && enviados >= qtdeMaxima.Value)
                {
                    _logger.LogInformation("Limite de {Limite} envios atingido", qtdeMaxima.Value);
                    break;
                }

                var reserva = agendamentoGroup.First();

                try
                {
                    // Validar email
                    if (string.IsNullOrWhiteSpace(reserva.EmailCliente) || !IsValidEmail(reserva.EmailCliente))
                    {
                        _logger.LogDebug("Email inválido para reserva {ReservaId}", reserva.ReservaId);
                        continue;
                    }

                    // Aplicar filtros
                    if (!_communicationProvider.ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes).GetValueOrDefault(false))
                        continue;

                    // Gerar voucher completo
                    var voucherData = await _generationService.GerarVoucherCompletoAsync(
                        agendamentoGroup.Key,
                        false,
                        config,
                        contratos);

                    if (voucherData == null)
                    {
                        _logger.LogWarning("Não foi possível gerar voucher para agendamento {AgendamentoId}", agendamentoGroup.Key);
                        erros++;
                        continue;
                    }

                    var enviarEmailApenasParaEmailsAutorizados = _configuration.GetValue<bool>("EnviarEmailApenasParaDestinatariosPermitidos", true);
                    var emailsPermitidos = enviarEmailApenasParaEmailsAutorizados ? _configuration.GetValue<string>("DestinatarioEmailPermitido", "glebersonsm@gmail.com") : null;

                    // Montar email
                    var subject = _generationService.SubstituirPlaceholders(
                        config.Subject ?? "Voucher da Reserva",
                        voucherData.DadosReserva);

                    var emailModel = new EmailInputInternalModel
                    {
                        Assunto = subject,
                        Destinatario = enviarEmailApenasParaEmailsAutorizados ? emailsPermitidos : reserva.EmailCliente,
                        ConteudoEmail = config.TemplateSendMode != EnumTemplateSendMode.AttachmentOnly
                            ? voucherData.VoucherHtml
                            : _generationService.GerarCorpoEmailHtml(voucherData.DadosReserva, daysBefore),
                        EmpresaId = 1,
                        UsuarioCriacao = 1,
                        Anexos = config.TemplateSendMode != EnumTemplateSendMode.BodyHtmlOnly
                            ? new List<EmailAnexoInputModel>
                            {
                                new EmailAnexoInputModel
                                {
                                    NomeArquivo = voucherData.VoucherPdf.FileName,
                                    TipoMime = "application/pdf",
                                    Arquivo = voucherData.VoucherPdf.FileBytes
                                }
                            }
                            : null
                    };

                    await _emailService.SaveInternal(emailModel); // ✅ Método correto

                    enviados++;
                    _logger.LogInformation("Voucher enviado - Reserva: {ReservaId}, Email: {Email}",
                        reserva.ReservaId, reserva.EmailCliente);
                }
                catch (Exception ex)
                {
                    erros++;
                    _logger.LogError(ex, "Erro ao processar voucher para reserva {ReservaId}", reserva.ReservaId);
                }
            }

            _logger.LogInformation("=== FIM PROCESSAMENTO - Enviados: {Enviados}, Erros: {Erros} ===", enviados, erros);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no processamento de vouchers Timesharing");
            throw;
        }
    }

    #region Métodos Auxiliares

    //private async Task<bool> ShouldSendVoucherAsync(
    //    ReservaInfo reserva,
    //    AutomaticCommunicationConfigModel config,
    //    List<DadosContratoModel>? contratos,
    //    List<ClientesInadimplentes>? inadimplentes)
    //{
    //    try
    //    {
    //        // Se não há filtros, enviar
    //        if ((config.ExcludedStatusCrcIds == null || !config.ExcludedStatusCrcIds.Any()) &&
    //            !config.SendOnlyToAdimplentes)
    //        {
    //            return true;
    //        }

    //        contratos ??= await _serviceBase.GetContratos(new List<int>());

    //        var contrato = contratos?.FirstOrDefault(c =>
    //            !string.IsNullOrEmpty(reserva.CotaNome) && !string.IsNullOrEmpty(c.GrupoCotaTipoCotaNome) &&
    //             c.GrupoCotaTipoCotaNome.Equals(reserva.CotaNome, StringComparison.OrdinalIgnoreCase) ||
    //            !string.IsNullOrEmpty(reserva.UhCondominioNumero) && !string.IsNullOrEmpty(c.NumeroImovel) &&
    //             c.NumeroImovel.Equals(reserva.UhCondominioNumero, StringComparison.OrdinalIgnoreCase)
    //        );

    //        if (contrato == null)
    //        {
    //            _logger.LogDebug("Contrato não encontrado para reserva {ReservaId}", reserva.ReservaId);
    //            return true; // Enviar mesmo sem contrato
    //        }

    //        if (contrato.Status != "A")
    //        {
    //            _logger.LogDebug("Contrato inativo para reserva {ReservaId}", reserva.ReservaId);
    //            return false;
    //        }

    //        // Filtrar por Status CRC
    //        if (config.ExcludedStatusCrcIds != null && config.ExcludedStatusCrcIds.Any())
    //        {
    //            var statusCrcAtivos = contrato.frAtendimentoStatusCrcModels?
    //                .Where(s => s.AtendimentoStatusCrcStatus == "A" && !string.IsNullOrEmpty(s.FrStatusCrcId))
    //                .Select(s => int.Parse(s.FrStatusCrcId!))
    //                .ToList() ?? new List<int>();

    //            if (statusCrcAtivos.Any(statusId => config.ExcludedStatusCrcIds.Contains(statusId)))
    //            {
    //                _logger.LogDebug("Reserva {ReservaId} possui Status CRC excluído", reserva.ReservaId);
    //                return false;
    //            }
    //        }

    //        // Filtrar inadimplentes
    //        if (config.SendOnlyToAdimplentes)
    //        {
    //            var temBloqueio = contrato.frAtendimentoStatusCrcModels?.Any(s =>
    //                s.AtendimentoStatusCrcStatus == "A" &&
    //                (s.BloquearCobrancaPagRec == "S" || s.BloqueaRemissaoBoletos == "S")) ?? false;

    //            var clienteInadimplente = inadimplentes?.FirstOrDefault(c =>
    //                c.CpfCnpj != null && contrato.PessoaTitular1CPF != null &&
    //                 c.CpfCnpj.ToString() == contrato.PessoaTitular1CPF ||
    //                c.CpfCnpj != null && contrato.PessoaTitular2CPF != null &&
    //                 c.CpfCnpj.ToString() == contrato.PessoaTitular2CPF
    //            );

    //            if (temBloqueio || clienteInadimplente != null)
    //            {
    //                _logger.LogDebug("Reserva {ReservaId} possui inadimplência ou bloqueio", reserva.ReservaId);
    //                return false;
    //            }
    //        }

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Erro ao verificar filtros para reserva {ReservaId}", reserva.ReservaId);
    //        return false;
    //    }
    //}

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