using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalProprietario.Application.Interfaces.Esol
{
    /// <summary>
    /// Serviço interno para operações de reserva/agendamento diretamente no banco eSolution Portal.
    /// Substitui todas as chamadas HTTP à SwReserva/Esolution API.
    /// </summary>
    public interface IReservaEsolInternalService
    {
        Task<ResultWithPaginationModel<List<SemanaModel>>?> GetAgendamentosGerais(ReservasMultiPropriedadeSearchModel model);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> GetConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model);
        Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamentoId);
        Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamentoId);
        Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva);
        Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id);
        Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id);
        Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel);
        Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool);
        Task<ResultModel<int>?> TrocarSemana(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model);
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(string agendamentoId);
    }
}
