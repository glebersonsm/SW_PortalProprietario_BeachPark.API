using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalProprietario.Application.Interfaces.ReservasApi
{
    public interface IReservaAgendamentoService
    {
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais(ReservasMultiPropriedadeSearchModel model);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model);
        Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamento);
        Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos(int agendamentoId);
        Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamento);
        Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva);
        Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id);
        Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id);
        Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel);
        Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarMinhaSemanaPool(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool);
    }
}

