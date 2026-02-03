using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.IncentivoAgendamento;

/// <summary>
/// Serviço responsável pelo processamento em lote de incentivos para agendamento
/// Usa o mesmo código de geração de layout via IncentivoAgendamentoGenerationService
/// </summary>
public class IncentivoAgendamentoProcessingService
{
    private readonly ILogger<IncentivoAgendamentoProcessingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRepositoryHosted _repository;
    private readonly IEmpreendimentoHybridProviderService _empreendimentoProviderService;
    private readonly IEmailService _emailService;
    private readonly IServiceBase _serviceBase;
    private readonly IncentivoAgendamentoGenerationService _incentivoGenerationService;

    public IncentivoAgendamentoProcessingService(
        ILogger<IncentivoAgendamentoProcessingService> logger,
        IConfiguration configuration,
        IRepositoryHosted repository,
        IEmpreendimentoHybridProviderService empreendimentoProviderService,
        IEmailService emailService,
        IServiceBase serviceBase,
        IncentivoAgendamentoGenerationService incentivoGenerationService)
    {
        _logger = logger;
        _configuration = configuration;
        _repository = repository;
        _empreendimentoProviderService = empreendimentoProviderService;
        _emailService = emailService;
        _serviceBase = serviceBase;
        _incentivoGenerationService = incentivoGenerationService;
    }

    /// <summary>
    /// Processa incentivos Multipropriedade para um intervalo específico
    /// </summary>
    public async Task ProcessarIncentivosMultiPropriedadeAsync(
        IStatelessSession session,
        AutomaticCommunicationConfigModel config,
        int intervalo,
        int? qtdeMaxima = null)
    {
        var contratos = await _serviceBase.GetContratos(new List<int>());
        var inadimplentes = await _empreendimentoProviderService.Inadimplentes();
        int qtdeEnviados = 0;

        try
        {
            _logger.LogInformation("Processando incentivos Multipropriedade para intervalo {Intervalo} dias", intervalo);

            // Buscar contratos elegíveis usando o método do serviço de geração
            var ano = DateTime.Now.Month >= 6 ? DateTime.Now.Year + 1 : DateTime.Now.Year;
            var contratosElegiveis = await _incentivoGenerationService.GetContratosElegiveisAsync(
                EnumProjetoType.Multipropriedade,
                config,
                contratos ?? new List<DadosContratoModel>(),
                ano,
                simulacao: false);

            if (contratosElegiveis == null || !contratosElegiveis.Any())
            {
                _logger.LogInformation("Nenhum contrato elegível encontrado para intervalo {Intervalo}", intervalo);
                return;
            }

            _logger.LogInformation("Encontrados {Count} contratos elegíveis para intervalo {Intervalo}",
                contratosElegiveis.Count, intervalo);

            foreach (var contratoItem in contratosElegiveis)
            {
                try
                {
                    // Verificar se já foi enviado para este contrato e intervalo
                    if (await CheckIfAlreadySentAsync(session, contratoItem.contrato.FrAtendimentoVendaId.GetValueOrDefault(0), intervalo, EnumProjetoType.Multipropriedade))
                    {
                        _logger.LogDebug("Incentivo já enviado para contrato {ContratoId} no intervalo {Intervalo}",
                            contratoItem.contrato.FrAtendimentoVendaId, intervalo);
                        continue;
                    }

                    // Validar email
                    var email = contratoItem.contrato.PessoaTitular1Email ?? contratoItem.contrato.PessoaTitular2Email;
                    if (!IsValidEmail(email))
                    {
                        _logger.LogWarning("Email inválido para contrato {ContratoId}: {Email}",
                            contratoItem.contrato.FrAtendimentoVendaId, email);
                        continue;
                    }

                    // Verificar filtros de negócio usando método do serviço de geração
                    if (!await _incentivoGenerationService.ShouldSendEmailForContrato(contratoItem.contrato, config, inadimplentes))
                    {
                        _logger.LogDebug("Contrato {ContratoId} não atende critérios de filtro",
                            contratoItem.contrato.FrAtendimentoVendaId);
                        continue;
                    }

                    // Gerar e enviar incentivo usando o serviço de geração
                    var emailId = await GerarEEnviarIncentivoAsync(contratoItem.contrato, contratoItem.statusAgendamento, config, intervalo);

                    if (emailId.HasValue)
                    {
                        // Registrar envio
                        await RegisterSentEmailAsync(session, contratoItem.contrato.FrAtendimentoVendaId.GetValueOrDefault(), intervalo,
                            contratoItem.statusAgendamento.DataInicialAgendamento.GetValueOrDefault(DateTime.Now),
                            emailId, EnumProjetoType.Multipropriedade);

                        _logger.LogInformation("Incentivo enviado para FrAtendimentoVendaId {FfrAtendimentoVendaId} (Email ID: {EmailId})",
                            contratoItem.contrato.FrAtendimentoVendaId, emailId);

                        qtdeEnviados++;
                        if (qtdeMaxima.HasValue && qtdeEnviados >= qtdeMaxima.Value)
                        {
                            _logger.LogInformation("Limite de {Limite} incentivos atingido", qtdeMaxima.Value);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar contrato {ContratoId}", contratoItem.contrato.FrAtendimentoVendaId);
                }
            }

            _logger.LogInformation("Processamento concluído: {QtdeEnviados} incentivos enviados para intervalo {Intervalo}",
                qtdeEnviados, intervalo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar incentivos Multipropriedade para intervalo {Intervalo}", intervalo);
        }
    }

    /// <summary>
    /// Processa incentivos Timesharing para um intervalo específico
    /// </summary>
    public async Task ProcessarIncentivosTimesharingAsync(
        IStatelessSession session,
        AutomaticCommunicationConfigModel config,
        int intervalo,
        int? qtdeMaxima = null)
    {
        int qtdeEnviados = 0;

        try
        {
            _logger.LogInformation("Processando incentivos Timesharing para intervalo {Intervalo} dias", intervalo);

            var timeSharingAtivado = _configuration.GetValue("TimeSharingAtivado", false);
            if (!timeSharingAtivado)
            {
                _logger.LogInformation("Timesharing desativado");
                return;
            }

            // TODO: Implementar busca de contratos Timesharing quando disponível
            var contratos = await _serviceBase.GetContratos(new List<int>());
            var ano = DateTime.Now.Month >= 6 ? DateTime.Now.Year + 1 : DateTime.Now.Year;
            var contratosElegiveis = await _incentivoGenerationService.GetContratosElegiveisAsync(
                EnumProjetoType.Timesharing,
                config,
                contratos ?? new List<DadosContratoModel>(),
                ano,
                simulacao: false);

            if (contratosElegiveis == null || !contratosElegiveis.Any())
            {
                _logger.LogInformation("Nenhum contrato Timesharing elegível encontrado para intervalo {Intervalo}", intervalo);
                return;
            }

            foreach (var contratoItem in contratosElegiveis)
            {
                try
                {
                    if (await CheckIfAlreadySentAsync(session, contratoItem.contrato.FrAtendimentoVendaId.GetValueOrDefault(0), intervalo, EnumProjetoType.Timesharing))
                    {
                        _logger.LogDebug("Incentivo já enviado para contrato Timesharing {ContratoId} no intervalo {Intervalo}",
                            contratoItem.contrato.FrAtendimentoVendaId, intervalo);
                        continue;
                    }

                    var email = contratoItem.contrato.PessoaTitular1Email ?? contratoItem.contrato.PessoaTitular2Email;
                    if (!IsValidEmail(email))
                    {
                        _logger.LogWarning("Email inválido para contrato Timesharing {ContratoId}: {Email}",
                            contratoItem.contrato.FrAtendimentoVendaId, email);
                        continue;
                    }

                    var emailId = await GerarEEnviarIncentivoAsync(contratoItem.contrato, contratoItem.statusAgendamento, config, intervalo);

                    if (emailId.HasValue)
                    {
                        await RegisterSentEmailAsync(session, contratoItem.contrato.FrAtendimentoVendaId.GetValueOrDefault(0), intervalo,
                            contratoItem.statusAgendamento.DataInicialAgendamento.GetValueOrDefault(DateTime.Now),
                            emailId, EnumProjetoType.Timesharing);

                        _logger.LogInformation("Incentivo Timesharing enviado para contrato {ContratoId} (Email ID: {EmailId})",
                            contratoItem.contrato.FrAtendimentoVendaId, emailId);

                        qtdeEnviados++;
                        if (qtdeMaxima.HasValue && qtdeEnviados >= qtdeMaxima.Value)
                        {
                            _logger.LogInformation("Limite de {Limite} incentivos Timesharing atingido", qtdeMaxima.Value);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar contrato Timesharing {ContratoId}", contratoItem.contrato.FrAtendimentoVendaId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar incentivos Timesharing para intervalo {Intervalo}", intervalo);
        }
    }

    #region Métodos Privados

    private async Task<int?> GerarEEnviarIncentivoAsync(
        DadosContratoModel contrato,
        PosicaoAgendamentoViewModel statusAgendamento,
        AutomaticCommunicationConfigModel config,
        int intervalo)
    {
        try
        {
            // Usar o serviço de geração para gerar o conteúdo completo
            var emailData = await _incentivoGenerationService.GerarAvisoCompletoAsync(config, contrato, statusAgendamento, intervalo);

            if (emailData == null)
            {
                _logger.LogWarning("Erro ao gerar conteúdo do email para contrato {ContratoId}", contrato.FrAtendimentoVendaId);
                return null;
            }

            // Determinar destinatário
            var enviarApenasPermitidos = _configuration.GetValue("EnviarEmailApenasParaDestinatariosPermitidos", true);
            var destinatarioPermitido = enviarApenasPermitidos ? _configuration.GetValue("DestinatarioEmailPermitido", "glebersonsm@gmail.com") : null;
            var email = contrato.PessoaTitular1Email ?? contrato.PessoaTitular2Email ?? "";
            var destinatario = enviarApenasPermitidos ? destinatarioPermitido : email;

            var emailModel = new EmailInputInternalModel
            {
                Assunto = emailData.Subject ?? "Incentivo para Agendamento",
                Destinatario = destinatario,
                ConteudoEmail = emailData.HtmlContent ?? "",
                EmpresaId = 1,
                UsuarioCriacao = 1 // Sistema
            };

            // Adicionar anexo se modo configurado incluir anexo E tiver PDF
            if ((config.TemplateSendMode == EnumTemplateSendMode.AttachmentOnly ||
                config.TemplateSendMode == EnumTemplateSendMode.BodyHtmlAndAttachment)
                && emailData.PdfBytes != null && emailData.PdfBytes.Length > 0)
            {
                emailModel.Anexos = new List<EmailAnexoInputModel>
                {
                    new EmailAnexoInputModel
                    {
                        NomeArquivo = $"{emailData.PdfFileName ?? "Incentivo"}.pdf",
                        TipoMime = "application/pdf",
                        Arquivo = emailData.PdfBytes
                    }
                };

                _logger.LogInformation("Anexo PDF adicionado ao email: {FileName} ({Size} bytes)",
                    emailData.PdfFileName, emailData.PdfBytes.Length);
            }

            var emailSaved = await _emailService.SaveInternal(emailModel);
            return emailSaved.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar e enviar incentivo para contrato {ContratoId}", contrato.FrAtendimentoVendaId);
            return null;
        }
    }

    private async Task<bool> CheckIfAlreadySentAsync(
        IStatelessSession session,
        long contratoId,
        int intervalo,
        EnumProjetoType projetoType)
    {
        try
        {
            // Para incentivo de agendamento, usamos o contratoId no campo ReservaId (já que não há reserva)
            var sent = await _repository.FindByHql<AutomaticCommunicationSent>(
                $"From AutomaticCommunicationSent a Where a.ReservaId = {contratoId} " +
                $"and a.DaysBeforeCheckIn = {intervalo} " +
                $"and a.CommunicationType = 'IncentivoParaAgendamento' " +
                $"and a.EmpreendimentoTipo = {(int)projetoType}",
                session);
            return sent.Any();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao verificar se já foi enviado para contrato {ContratoId}", contratoId);
            return false;
        }
    }

    private async Task RegisterSentEmailAsync(
        IStatelessSession session,
        int frAtendimentoVendaId,
        int intervalo,
        DateTime dataReferencia,
        int? emailId,
        EnumProjetoType projetoType)
    {
        try
        {
            _repository.BeginTransaction(session);

            var sent = new AutomaticCommunicationSent
            {
                CommunicationType = "IncentivoParaAgendamento",
                FrAtendimentoVendaId = frAtendimentoVendaId,
                DaysBeforeCheckIn = intervalo,
                DataCheckIn = dataReferencia,
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
            _logger.LogError(ex, "Erro ao registrar envio para FrAtendimentoVendaId {frAtendimentoVendaId}", frAtendimentoVendaId);
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
