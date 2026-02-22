using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces.Esol;
using SW_PortalProprietario.Application.Interfaces.ReservasApi;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.Application.Services.Esol
{
    /// <summary>
    /// Serviço agregado de reservas - migrado do SwReservaApiMain.
    /// Delega para IReservaAgendamentoService e IEmpreendimentoHybridProviderService.
    /// </summary>
    public class ReservaEsolService : IReservaEsolService
    {
        private readonly IReservaAgendamentoService _reservaAgendamentoService;
        private readonly IEmpreendimentoHybridProviderService _empreendimentoService;
        private readonly ILogger<ReservaEsolService> _logger;

        public ReservaEsolService(
            IReservaAgendamentoService reservaAgendamentoService,
            IEmpreendimentoHybridProviderService empreendimentoService,
            ILogger<ReservaEsolService> logger)
        {
            _reservaAgendamentoService = reservaAgendamentoService;
            _empreendimentoService = empreendimentoService;
            _logger = logger;
        }

        public Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamento)
            => _reservaAgendamentoService.ConsultarReservaByAgendamentoId(agendamento);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamento)
            => _reservaAgendamentoService.ConsultarMinhasReservaByAgendamentoId(agendamento);

        public async Task<ResultWithPaginationModel<List<ReservaModel>>?> ConsultarGeralReserva(ReservasMultiPropriedadeSearchModel model)
        {
            var semanasResult = await _empreendimentoService.ConsultarAgendamentosGerais(model);
            if (semanasResult?.Data == null || !semanasResult.Data.Any())
                return new ResultWithPaginationModel<List<ReservaModel>>
                {
                    Data = new List<ReservaModel>(),
                    Success = true,
                    PageNumber = model.NumeroDaPagina ?? 1,
                    LastPageNumber = 1
                };

            var reservas = semanasResult.Data
                .Where(s => s.Reservas != null && s.Reservas.Any())
                .SelectMany(s => s.Reservas!)
                .Distinct()
                .ToList();

            return new ResultWithPaginationModel<List<ReservaModel>>(reservas)
            {
                Success = true,
                PageNumber = semanasResult.PageNumber,
                LastPageNumber = semanasResult.LastPageNumber,
                NumberRecords = reservas.Count
            };
        }

        public Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva)
            => _reservaAgendamentoService.SalvarReservaEmAgendamento(modelReserva);

        public Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model)
            => _reservaAgendamentoService.CancelarReservaAgendamento(model);

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model)
            => _reservaAgendamentoService.CancelarMinhaReservaAgendamento(model);

        public Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id)
            => _reservaAgendamentoService.EditarReserva(id);

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id)
            => _reservaAgendamentoService.EditarMinhaReserva(id);

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(string agendamentoId)
            => _empreendimentoService.GetDadosImpressaoVoucher(agendamentoId);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais(ReservasMultiPropriedadeSearchModel model)
            => _reservaAgendamentoService.ConsultarAgendamentosGerais(model);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
            => _reservaAgendamentoService.ConsultarMeusAgendamentos(model);

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel)
            => _reservaAgendamentoService.ConsultarInventarios(searchModel);

        public Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool)
            => _reservaAgendamentoService.RetirarSemanaPool(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool)
            => _reservaAgendamentoService.LiberarSemanaPool(modelAgendamentoPool);

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel(DispobilidadeSearchModel searchModel)
            => _empreendimentoService.ConsultarDisponibilidadeCompativel(searchModel);

        public Task<ResultModel<int>?> TrocarSemana(TrocaSemanaInputModel model)
            => _empreendimentoService.TrocarSemana(model);

        public Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model)
            => _empreendimentoService.IncluirSemana(model);
    }
}
