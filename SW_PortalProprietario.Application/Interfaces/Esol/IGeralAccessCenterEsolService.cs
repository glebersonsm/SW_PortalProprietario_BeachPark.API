using SW_PortalProprietario.Application.Models.Esol;

namespace SW_PortalProprietario.Application.Interfaces.Esol
{
    /// <summary>
    /// Serviço de consultas gerais - migrado do SwReservaApiMain (Access Center).
    /// </summary>
    public interface IGeralAccessCenterEsolService
    {
        Task<List<EmpresaEsolModel>> ConsultarEmpresa(ConsultaEmpresaEsolModel model);
    }
}
