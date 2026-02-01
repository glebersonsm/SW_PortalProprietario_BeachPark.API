using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.Default;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications;

/// <summary>
/// Factory para resolver handlers de comunicação baseado no tipo
/// </summary>
public class CommunicationHandlerFactory : ICommunicationHandlerFactory
{
    private readonly IEnumerable<ICommunicationHandler> _handlers;
    private readonly ILogger<CommunicationHandlerFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDocumentTemplateService _documentTemplateService;

    public CommunicationHandlerFactory(
        IEnumerable<ICommunicationHandler> handlers,
        ILogger<CommunicationHandlerFactory> logger,
        ILoggerFactory loggerFactory,
        IDocumentTemplateService documentTemplateService)
    {
        _handlers = handlers;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _documentTemplateService = documentTemplateService;
    }

    public ICommunicationHandler? GetHandler(EnumDocumentTemplateType communicationType)
    {
        var handler = _handlers.FirstOrDefault(h => h.CommunicationType == communicationType);
        
        if (handler == null)
        {
            _logger.LogWarning("Nenhum handler específico encontrado para {CommunicationType}. Usando handler padrão.", communicationType);
            
            // Retorna um handler padrão para tipos não implementados
            var defaultLogger = _loggerFactory.CreateLogger<DefaultCommunicationHandler>();
            return new DefaultCommunicationHandler(communicationType, defaultLogger, _documentTemplateService);
        }

        return handler;
    }

    public bool HasHandler(EnumDocumentTemplateType communicationType)
    {
        return _handlers.Any(h => h.CommunicationType == communicationType);
    }
}
