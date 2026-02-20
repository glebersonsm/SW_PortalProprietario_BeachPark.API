using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces;

/// <summary>
/// Factory para resolver handlers de comunicação baseado no tipo
/// </summary>
public interface ICommunicationHandlerFactory
{
    /// <summary>
    /// Obtém o handler apropriado para o tipo de comunicação
    /// </summary>
    ICommunicationHandler? GetHandler(EnumDocumentTemplateType communicationType);

    /// <summary>
    /// Verifica se existe um handler registrado para o tipo de comunicação
    /// </summary>
    bool HasHandler(EnumDocumentTemplateType communicationType);
}
