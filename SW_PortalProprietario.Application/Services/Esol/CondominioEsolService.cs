using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces.Esol;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalProprietario.Application.Services.Esol
{
    /// <summary>
    /// Serviço de condomínio - migrado do SwReservaApiMain.
    /// Delega para IReservaEsolService.
    /// </summary>
    public class CondominioEsolService : ICondominioEsolService
    {
        private readonly IReservaEsolService _reservaEsolService;
        private readonly ILogger<CondominioEsolService> _logger;

        public CondominioEsolService(IReservaEsolService reservaEsolService, ILogger<CondominioEsolService> logger)
        {
            _reservaEsolService = reservaEsolService;
            _logger = logger;
        }

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarSemanasCota(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
            => _reservaEsolService.ConsultarMeusAgendamentos(model);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarGeralSemanasCota(PeriodoCotaDisponibilidadeSearchModel model)
        {
            var searchModel = new ReservasMultiPropriedadeSearchModel
            {
                CotaProprietarioId = model.CotaProprietarioId,
                Ano = model.Ano,
                NomeProprietario = model.NomeProprietario,
                DocumentoProprietario = model.DocumentoProprietario,
                NumeroApartamento = model.NumeroApartamento,
                NumeroApartamentos = model.NumeroApartamentos,
                NomeCotas = model.NomeCotas,
                CheckinInicial = model.CheckinInicial,
                CheckinFinal = model.CheckinFinal,
                CheckoutInicial = model.CheckoutInicial,
                CheckoutFinal = model.CheckoutFinal,
                DataUtilizacaoInicial = model.DataUtilizacaoInicial ?? model.CheckinInicial,
                DataUtilizacaoFinal = model.DataUtilizacaoFinal ?? model.CheckoutFinal,
                Reserva = model.Reserva,
                ComReservas = model.ComReservas,
                PeriodoCotaDisponibilidadeId = model.PeriodoCotaDisponibilidadeId,
                DataAquisicaoContrato = model.DataAquisicaoContrato,
                QuantidadeRegistrosRetornar = model.QuantidadeRegistrosRetornar,
                NumeroDaPagina = model.NumeroDaPagina
            };
            return _reservaEsolService.ConsultarAgendamentosGerais(searchModel);
        }

        public async Task LiberarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool)
        {
            var liberacaoModel = new LiberacaoAgendamentoInputModel
            {
                AgendamentoId = modelAgendamentoPool.AgendamentoId,
                InventarioId = modelAgendamentoPool.InventarioId
            };
            var result = await _reservaEsolService.LiberarSemanaPool(liberacaoModel);
            if (result == null || !result.Success)
                throw new ArgumentException(result?.Message ?? result?.Errors?.FirstOrDefault() ?? "Não foi possível liberar semana para o Pool");
        }

        public async Task RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool)
        {
            var result = await _reservaEsolService.RetirarSemanaPool(modelAgendamentoPool);
            if (result == null || !result.Success)
                throw new ArgumentException(result?.Message ?? result?.Errors?.FirstOrDefault() ?? "Não foi possível retirar semana do Pool");
        }

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel)
            => _reservaEsolService.ConsultarInventarios(searchModel);

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel(DispobilidadeSearchModel searchModel)
            => _reservaEsolService.ConsultarDisponibilidadeCompativel(searchModel);

        public Task<ResultModel<int>?> TrocarSemana(TrocaSemanaInputModel model)
            => _reservaEsolService.TrocarSemana(model);

        public Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model)
            => _reservaEsolService.IncluirSemana(model);
    }
}
