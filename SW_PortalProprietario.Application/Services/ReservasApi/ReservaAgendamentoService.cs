using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Interfaces.ReservasApi;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.Application.Services.ReservasApi
{
    public class ReservaAgendamentoService : IReservaAgendamentoService
    {

        private readonly IEmpreendimentoHybridProviderService _empreendimentoService;


        public ReservaAgendamentoService(IEmpreendimentoHybridProviderService empreendimentoService)
        {
            _empreendimentoService = empreendimentoService;
        }

        public async Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva)
        {
            return await _empreendimentoService.SalvarReservaEmAgendamento(modelReserva);
        }

        public async Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais(ReservasMultiPropriedadeSearchModel model)
        {
            return await _empreendimentoService.ConsultarAgendamentosGerais(model);
        }

        public async Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
        {
            return await _empreendimentoService.ConsultarMeusAgendamentos(model);
        }

        public async Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamento)
        {
            return await _empreendimentoService.ConsultarReservaByAgendamentoId(agendamento);
        }

        public async Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamento)
        {
            return await _empreendimentoService.ConsultarMinhasReservaByAgendamentoId(agendamento);
        }

        public async Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model)
        {
            return await _empreendimentoService.CancelarReservaAgendamento(model);
        }

        public async Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model)
        {
            return await _empreendimentoService.CancelarMinhaReservaAgendamento(model);
        }

        public async Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id)
        {
            return await _empreendimentoService.EditarReserva(id);
        }

        public async Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id)
        {
            return await _empreendimentoService.EditarMinhaReserva(id);
        }

        public async Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel)
        {
            return await _empreendimentoService.ConsultarInventarios(searchModel);
        }

        public async Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool)
        {
            return await _empreendimentoService.RetirarSemanaPool(modelAgendamentoPool);
        }

        public async Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool)
        {
            return await _empreendimentoService.LiberarSemanaPool(modelAgendamentoPool);
        }

        public async Task<ResultModel<bool>?> LiberarMinhaSemanaPool(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
        {
            return await _empreendimentoService.LiberarMinhaSemanaPool(modelAgendamentoPool);
        }

        public async Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos(int agendamentoId)
        {
            return await _empreendimentoService.ConsultarHistoricos(agendamentoId);
        }

    }
}
