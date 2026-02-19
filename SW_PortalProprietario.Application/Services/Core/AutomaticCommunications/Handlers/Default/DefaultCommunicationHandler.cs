using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.Default;

/// <summary>
/// Handler base para tipos de comunicação ainda não implementados
/// </summary>
public class DefaultCommunicationHandler : ICommunicationHandler
{
    private readonly ILogger<DefaultCommunicationHandler> _logger;
    private readonly IDocumentTemplateService _documentTemplateService;
    private readonly EnumDocumentTemplateType _communicationType;

    public EnumDocumentTemplateType CommunicationType => _communicationType;

    public DefaultCommunicationHandler(
        EnumDocumentTemplateType communicationType,
        ILogger<DefaultCommunicationHandler> logger,
        IDocumentTemplateService documentTemplateService)
    {
        _communicationType = communicationType;
        _logger = logger;
        _documentTemplateService = documentTemplateService;
    }

    public Task ProcessMultiPropriedadeAsync(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config)
    {
        _logger.LogInformation("Processamento de comunicação {CommunicationType} - Multipropriedade ainda não implementado completamente", _communicationType);

        foreach (var daysBefore in config.DaysBeforeCheckIn)
        {
            _logger.LogDebug("Processando comunicação {CommunicationType} com {DaysBefore} dias - Multipropriedade", _communicationType, daysBefore);
        }

        return Task.CompletedTask;
    }

    public Task ProcessTimesharingAsync(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config)
    {
        _logger.LogInformation("Processamento de comunicação {CommunicationType} - Timesharing ainda não implementado completamente", _communicationType);

        foreach (var daysBefore in config.DaysBeforeCheckIn)
        {
            _logger.LogDebug("Processando comunicação {CommunicationType} com {DaysBefore} dias - Timesharing", _communicationType, daysBefore);
        }

        return Task.CompletedTask;
    }

    public async Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId)
    {
        _logger.LogInformation("Gerando email de simulação padrão para tipo {CommunicationType}", _communicationType);

        // Buscar template HTML se configurado
        var templateHtml = config.TemplateId.HasValue
            ? await _documentTemplateService.GetTemplateContentHtmlAsync(_communicationType, config.TemplateId.Value)
            : string.Empty;

        var emailBody = !string.IsNullOrEmpty(templateHtml)
            ? templateHtml
            : $@"
                <html>
                <body>
                    <h2>Simulação de Envio Automático</h2>
                    <p>Este é um email de teste da configuração de envio automático.</p>
                    <p><strong>Tipo de Comunicação:</strong> {_communicationType}</p>
                    <p><strong>Tipo de Projeto:</strong> {config.ProjetoType}</p>
                    <p><strong>Assunto Configurado:</strong> {config.Subject}</p>
                    <p style='margin-top: 20px; color: #666; font-size: 12px;'>
                        Este é um email de simulação. O email real será enviado conforme a configuração estabelecida.
                        A implementação específica para este tipo de comunicação está pendente.
                    </p>
                </body>
                </html>";


        return new List<EmailInputInternalModel>() { new EmailInputInternalModel
        {
            Assunto = $"[SIMULAÇÃO] {config.Subject}",
            Destinatario = userEmail,
            ConteudoEmail = emailBody,
            EmpresaId = 1,
            UsuarioCriacao = userId
        } };
    }
}
