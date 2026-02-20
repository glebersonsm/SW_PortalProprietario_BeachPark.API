using NHibernate;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces;

/// <summary>
/// Interface para serviço de processamento automático de incentivo para agendamento
/// </summary>
public interface IAutomaticIncentivoAgendamentoService
{
    /// <summary>
    /// Processa contratos elegíveis para incentivo de agendamento - Multipropriedade
    /// </summary>
    Task ProcessarContratosMultiPropriedade(IStatelessSession session, AutomaticCommunicationConfigModel config, int? qtdeEnviar = null);

    /// <summary>
    /// Processa contratos elegíveis para incentivo de agendamento - Timesharing
    /// </summary>
    Task ProcessarContratosTimesharing(IStatelessSession session, AutomaticCommunicationConfigModel config, int? qtdeEnviar = null);
}