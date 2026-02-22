using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalProprietario.Application.Interfaces.Esol
{
    /// <summary>
    /// Serviço agregado de reservas e condomínio - migrado do SwReservaApiMain.
    /// Delega para IReservaAgendamentoService e IEmpreendimentoHybridProviderService.
    /// </summary>
    public interface IReservaEsolService
    {
        // Reservas
        Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamento);
        Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamento);
        Task<ResultWithPaginationModel<List<ReservaModel>>?> ConsultarGeralReserva(ReservasMultiPropriedadeSearchModel model);
        Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva);
        Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id);
        Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id);
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(string agendamentoId);

        // Condomínio / Agendamentos
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais(ReservasMultiPropriedadeSearchModel model);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model);
        Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel);
        Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool);
        Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel(DispobilidadeSearchModel searchModel);
        Task<ResultModel<int>?> TrocarSemana(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model);
    }
}
