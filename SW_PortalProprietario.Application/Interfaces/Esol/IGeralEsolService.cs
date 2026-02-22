using SW_PortalProprietario.Application.Models.Esol;

namespace SW_PortalProprietario.Application.Interfaces.Esol
{
    /// <summary>
    /// Serviço de consultas gerais - migrado do SwReservaApiMain (eSolution Portal).
    /// </summary>
    public interface IGeralEsolService
    {
        Task<List<EmpresaEsolModel>> ConsultarEmpresa(ConsultaEmpresaEsolModel model);
    }
}
