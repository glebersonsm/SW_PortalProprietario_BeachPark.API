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
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.AvisoCheckin;

/// <summary>
/// Serviço responsável pelo processamento em lote de avisos de check-in próximo
/// ? USA O MESMO CÓDIGO DA SIMULAÇÃO via AvisoCheckinGenerationService
/// </summary>
public class AvisoCheckinProcessingService
{
    private readonly ILogger<AvisoCheckinProcessingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRepositoryHosted _repository;
    private readonly IEmpreendimentoHybridProviderService _empreendimentoProviderService;
    private readonly IEmailService _emailService;
    private readonly IServiceBase _serviceBase;
    private readonly AvisoCheckinGenerationService _avisoGenerationService;

    public AvisoCheckinProcessingService(
        ILogger<AvisoCheckinProcessingService> logger,
        IConfiguration configuration,
        IRepositoryHosted repository,
        IEmpreendimentoHybridProviderService empreendimentoProviderService,
        IEmailService emailService,
        IServiceBase serviceBase,
        AvisoCheckinGenerationService avisoGenerationService)
    {
        _logger = logger;
        _configuration = configuration;
        _repository = repository;
        _empreendimentoProviderService = empreendimentoProviderService;
        _emailService = emailService;
        _serviceBase = serviceBase;
        _avisoGenerationService = avisoGenerationService;
    }

    /// <summary>
    /// Processa avisos Multipropriedade para um dia específico antes do check-in
    /// </summary>
    public async Task ProcessarAvisosMultiPropriedadeAsync(
        IStatelessSession session,
        AutomaticCommunicationConfigModel config,
        int daysBefore,
        int? qtdeMaxima = null)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO AVISOS MULTIPROPRIEDADE - {DaysBefore} dias ===", daysBefore);

        var contratos = await _serviceBase.GetContratos(new List<int>());
        var inadimplentes = await _empreendimentoProviderService.Inadimplentes();
        int qtdeEnviados = 0;

        try
        {
            var targetDate = DateTime.Today.AddDays(daysBefore);
            _logger.LogInformation("Data alvo: {TargetDate}", targetDate.ToString("dd/MM/yyyy"));

            var reservas = await GetReservasMultiPropriedadeAsync(targetDate);

            if (reservas == null || !reservas.Any())
            {
                _logger.LogInformation("Nenhuma reserva encontrada para {TargetDate}", targetDate.ToString("dd/MM/yyyy"));
                return;
            }

            _logger.LogInformation("Encontradas {Count} reservas para processar", reservas.Count);

            foreach (var reservaGroup in reservas.GroupBy(r => r.ReservaId))
            {
                var reserva = reservaGroup.First();
                
                try
                {
                    // ? Verificar se já foi enviado
                    if (await CheckIfAlreadySentAsync(session, reserva.ReservaId, daysBefore, EnumProjetoType.Multipropriedade))
                    {
                        _logger.LogDebug("Aviso já enviado para reserva {ReservaId}", reserva.ReservaId);
                        continue;
                    }

                    // Validar email
                    if (!IsValidEmail(reserva.EmailCliente))
                    {
                        _logger.LogWarning("Email inválido para reserva {ReservaId}: {Email}", 
                            reserva.ReservaId, reserva.EmailCliente);
                        continue;
                    }

                    // Verificar filtros
                    if (!await ShouldSendAvisoAsync(reserva, config, contratos, inadimplentes))
                    {
                        _logger.LogDebug("Reserva {ReservaId} não atende critérios de filtro", reserva.ReservaId);
                        continue;
                    }

                    // ? GERAR E ENVIAR AVISO (USA GenerationService)
                    var emailId = await GerarEEnviarAvisoAsync(reserva, config, daysBefore);
                    
                    if (emailId.HasValue)
                    {
                        // ? Registrar envio
                        await RegisterSentEmailAsync(session, reserva.ReservaId, daysBefore,
                            reserva.DataCheckIn, emailId, EnumProjetoType.Multipropriedade);

                        _logger.LogInformation("Aviso enviado para reserva {ReservaId} (Email ID: {EmailId})",
                            reserva.ReservaId, emailId);

                        qtdeEnviados++;
                        if (qtdeMaxima.HasValue && qtdeEnviados >= qtdeMaxima.Value)
                        {
                            _logger.LogInformation("Limite de {Limite} avisos atingido", qtdeMaxima.Value);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar reserva {ReservaId}", reserva.ReservaId);
                }
            }

            _logger.LogInformation("=== FIM PROCESSAMENTO - Enviados: {Enviados} ===", qtdeEnviados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar avisos Multipropriedade para {DaysBefore} dias", daysBefore);
        }
    }

    /// <summary>
    /// Processa avisos Timesharing para um dia específico antes do check-in
    /// </summary>
    public async Task ProcessarAvisosTimesharingAsync(
        IStatelessSession session,
        AutomaticCommunicationConfigModel config,
        int daysBefore,
        int? qtdeMaxima = null)
    {
        _logger.LogInformation("=== INÍCIO PROCESSAMENTO AVISOS TIMESHARING - {DaysBefore} dias ===", daysBefore);

        int qtdeEnviados = 0;

        try
        {
            var targetDate = DateTime.Today.AddDays(daysBefore);
            _logger.LogInformation("Data alvo: {TargetDate}", targetDate.ToString("dd/MM/yyyy"));

            var reservas = await GetReservasTimesharingAsync(targetDate);

            if (reservas == null || !reservas.Any())
            {
                _logger.LogInformation("Nenhuma reserva Timesharing encontrada para {TargetDate}", targetDate.ToString("dd/MM/yyyy"));
                return;
            }

            foreach (var reservaGroup in reservas.GroupBy(r => r.ReservaId))
            {
                var reserva = reservaGroup.First();
                
                try
                {
                    if (await CheckIfAlreadySentAsync(session, reserva.ReservaId, daysBefore, EnumProjetoType.Timesharing))
                    {
                        _logger.LogDebug("Aviso já enviado para reserva Timesharing {ReservaId}", reserva.ReservaId);
                        continue;
                    }

                    if (!IsValidEmail(reserva.EmailCliente))
                    {
                        _logger.LogWarning("Email inválido para reserva Timesharing {ReservaId}: {Email}", 
                            reserva.ReservaId, reserva.EmailCliente);
                        continue;
                    }

                    var emailId = await GerarEEnviarAvisoAsync(reserva, config, daysBefore);
                    
                    if (emailId.HasValue)
                    {
                        await RegisterSentEmailAsync(session, reserva.ReservaId, daysBefore,
                            reserva.DataCheckIn, emailId, EnumProjetoType.Timesharing);

                        _logger.LogInformation("Aviso Timesharing enviado para reserva {ReservaId} (Email ID: {EmailId})",
                            reserva.ReservaId, emailId);

                        qtdeEnviados++;
                        if (qtdeMaxima.HasValue && qtdeEnviados >= qtdeMaxima.Value)
                        {
                            _logger.LogInformation("Limite de {Limite} avisos Timesharing atingido", qtdeMaxima.Value);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar reserva Timesharing {ReservaId}", reserva.ReservaId);
                }
            }

            _logger.LogInformation("=== FIM PROCESSAMENTO - Enviados: {Enviados} ===", qtdeEnviados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar avisos Timesharing para {DaysBefore} dias", daysBefore);
        }
    }

    #region Métodos Privados

    private async Task<List<ReservaInfo>> GetReservasMultiPropriedadeAsync(DateTime checkInDate)
    {
        if (!_configuration.GetValue("MultipropriedadeAtivada", false))
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

    private async Task<List<ReservaInfo>> GetReservasTimesharingAsync(DateTime checkInDate)
    {
        if (!_configuration.GetValue("TimeSharingAtivado", false))
        {
            _logger.LogInformation("Timesharing desativado");
            return new List<ReservaInfo>();
        }

        // TODO: Implementar busca Timesharing
        return new List<ReservaInfo>();
    }

    private async Task<int?> GerarEEnviarAvisoAsync(
        ReservaInfo reserva,
        AutomaticCommunicationConfigModel config,
        int daysBefore)
    {
        try
        {
            // Buscar dados completos da reserva
            var dadosReserva = await _empreendimentoProviderService.GetDadosImpressaoVoucher($"{reserva.AgendamentoId}");
            if (dadosReserva == null)
            {
                _logger.LogWarning("Dados da reserva não encontrados para reserva {ReservaId}", reserva.ReservaId);
                return null;
            }

            // ? USAR GenerationService para gerar aviso completo (MESMA LÓGICA DA SIMULAÇÃO)
            var avisoData = await _avisoGenerationService.GerarAvisoCompletoAsync(
                reserva,
                dadosReserva,
                daysBefore,
                config.TemplateId.GetValueOrDefault(),
                config.TemplateSendMode ?? EnumTemplateSendMode.BodyHtmlOnly);

            if (avisoData == null || string.IsNullOrEmpty(avisoData.HtmlContent))
            {
                _logger.LogWarning("Não foi possível gerar aviso para reserva {ReservaId}", reserva.ReservaId);
                return null;
            }

            // ? Substituir placeholders no assunto usando GenerationService
            var subject = _avisoGenerationService.SubstituirPlaceholders(
                config.Subject ?? "Aviso de Check-in Próximo",
                reserva,
                dadosReserva,
                daysBefore);

            // Determinar destinatário
            var enviarApenasPermitidos = _configuration.GetValue("EnviarEmailApenasParaDestinatariosPermitidos", true);
            var destinatarioPermitido = enviarApenasPermitidos ? _configuration.GetValue("DestinatarioEmailPermitido", "glebersonsm@gmail.com") : null;
            var destinatario = enviarApenasPermitidos ? destinatarioPermitido : reserva.EmailCliente;

            var emailModel = new EmailInputInternalModel
            {
                Assunto = subject,
                Destinatario = destinatario,
                ConteudoEmail = avisoData.HtmlContent,
                EmpresaId = reserva.EmpresaId ?? 1,
                UsuarioCriacao = 1 // Sistema
            };

            // Adicionar anexo se modo configurado incluir anexo E tiver PDF
            if ((config.TemplateSendMode == EnumTemplateSendMode.AttachmentOnly ||
                config.TemplateSendMode == EnumTemplateSendMode.BodyHtmlAndAttachment)
                && avisoData.PdfBytes != null && avisoData.PdfBytes.Length > 0)
            {
                emailModel.Anexos = new List<EmailAnexoInputModel>
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

            var emailSaved = await _emailService.SaveInternal(emailModel);
            return emailSaved.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar e enviar aviso para reserva {ReservaId}", reserva.ReservaId);
            return null;
        }
    }

    private async Task<bool> ShouldSendAvisoAsync(
        ReservaInfo reserva,
        AutomaticCommunicationConfigModel config,
        List<Models.DadosContratoModel>? contratos,
        List<ClientesInadimplentes>? inadimplentes)
    {
        try
        {
            if ((config.ExcludedStatusCrcIds == null || !config.ExcludedStatusCrcIds.Any()) && 
                !config.SendOnlyToAdimplentes)
                return true;

            contratos ??= await _serviceBase.GetContratos(new List<int>());

            var contrato = contratos?.FirstOrDefault(c =>
                (!string.IsNullOrEmpty(reserva.CotaNome) && !string.IsNullOrEmpty(c.GrupoCotaTipoCotaNome) && 
                 c.GrupoCotaTipoCotaNome.Equals(reserva.CotaNome, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(reserva.UhCondominioNumero) && !string.IsNullOrEmpty(c.NumeroImovel) && 
                 c.NumeroImovel.Equals(reserva.UhCondominioNumero, StringComparison.OrdinalIgnoreCase))
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
                    (c.CpfCnpj != null && contrato.PessoaTitular1CPF != null && 
                     c.CpfCnpj.ToString() == contrato.PessoaTitular1CPF) ||
                    (c.CpfCnpj != null && contrato.PessoaTitular2CPF != null && 
                     c.CpfCnpj.ToString() == contrato.PessoaTitular2CPF)
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

    private async Task<bool> CheckIfAlreadySentAsync(
        IStatelessSession session,
        long reservaId,
        int daysBefore,
        EnumProjetoType projetoType)
    {
        try
        {
            var sent = await _repository.FindByHql<AutomaticCommunicationSent>(
                $"From AutomaticCommunicationSent a Where a.ReservaId = {reservaId} " +
                $"and a.DaysBeforeCheckIn = {daysBefore} " +
                $"and a.CommunicationType = 'AvisoReservaCheckinProximo' " +
                $"and a.EmpreendimentoTipo = {(int)projetoType}",
                session);
            return sent.Any();
        }
        catch
        {
            return false;
        }
    }

    private async Task RegisterSentEmailAsync(
        IStatelessSession session,
        long reservaId,
        int daysBefore,
        DateTime dataCheckIn,
        int? emailId,
        EnumProjetoType projetoType)
    {
        try
        {
            _repository.BeginTransaction(session);
            
            var sent = new AutomaticCommunicationSent
            {
                CommunicationType = "AvisoReservaCheckinProximo",
                ReservaId = reservaId,
                DaysBeforeCheckIn = daysBefore,
                DataCheckIn = dataCheckIn,
                DataEnvio = DateTime.Now,
                EmailId = emailId,
                DataHoraCriacao = DateTime.Now,
                EmpreendimentoTipo = projetoType
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
