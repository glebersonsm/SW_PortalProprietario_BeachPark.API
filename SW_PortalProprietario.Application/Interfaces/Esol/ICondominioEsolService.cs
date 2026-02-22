using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalProprietario.Application.Interfaces.Esol
{
    /// <summary>
    /// Serviço de condomínio - migrado do SwReservaApiMain.
    /// ConsultarContratos e ConsultarGeralContratos requerem portação da lógica do CondominioService.
    /// Demais métodos delegam para IReservaEsolService.
    /// </summary>
    public interface ICondominioEsolService
    {
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarSemanasCota(PeriodoCotaDisponibilidadeUsuarioSearchModel model);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarGeralSemanasCota(PeriodoCotaDisponibilidadeSearchModel model);
        Task LiberarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool);
        Task RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool);
        Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel);
        Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel(DispobilidadeSearchModel searchModel);
        Task<ResultModel<int>?> TrocarSemana(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model);
    }
}
