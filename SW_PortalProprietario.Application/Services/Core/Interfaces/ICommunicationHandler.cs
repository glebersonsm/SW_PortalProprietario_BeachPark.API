using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces;

/// <summary>
/// Interface para handlers de tipos específicos de comunicação automática
/// </summary>
public interface ICommunicationHandler
{
    /// <summary>
    /// Tipo de comunicação que este handler processa
    /// </summary>
    EnumDocumentTemplateType CommunicationType { get; }

    /// <summary>
    /// Processa comunicações para Multipropriedade
    /// </summary>
    Task ProcessMultiPropriedadeAsync(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config);

    /// <summary>
    /// Processa comunicações para Timesharing
    /// </summary>
    Task ProcessTimesharingAsync(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config);

    /// <summary>
    /// Gera um email de simulação para o tipo de comunicação
    /// </summary>
    /// <param name="config">Configuração da comunicação</param>
    /// <param name="userEmail">Email do usuário que receberá a simulação</param>
    /// <param name="userId">ID do usuário solicitante</param>
    /// <returns>Modelo de email pronto para envio</returns>
    Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        SW_PortalProprietario.Application.Models.GeralModels.AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId);
}
