using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces;

public interface IAutomaticVoucherService
{
    Task ProcessarReservasForDayMultiPropriedade(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config, int daysBefore, int? qtdeEnviar = null);
    Task ProcessarReservasForDayTimesharing(NHibernate.IStatelessSession session, AutomaticCommunicationConfigModel config, int daysBefore, int? quantidadeMaximaEnvio = null);
}
