using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.Application.Services.Providers.Esolution
{
    public class TimeSharingEsolutionService : ITimeSharingProviderService
    {
        private readonly IRepositoryNHAccessCenter _repository;
        private readonly ILogger<TimeSharingEsolutionService> _logger;
        public TimeSharingEsolutionService(IRepositoryNHAccessCenter repository,
            ILogger<TimeSharingEsolutionService> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing(SearchContratosTimeSharingModel searchModel)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetMeusContratosTimeSharing(SearchMeusContratosTimeSharingModel searchModel)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos(SearchReservaTsModel searchModel)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral(SearchReservasGeralModel searchModel)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetMinhasReservasGeralComConsumoPontos(SearchMinhasReservaTsModel searchModel)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral(SearchMinhasReservasGeralModel searchModel)
        {
            throw new NotImplementedException();
        }


        public async Task<ReservaTimeSharingCMModel> Visualizar(string reservanumero)
        {
            throw new NotImplementedException();
        }

        public async Task<ReservaTimeSharingCMModel> VisualizarMinha(string reservanumero)
        {
            throw new NotImplementedException();
        }

        public Task<IList<PeriodoDisponivelResultModel>?> Disponibilidade(SearchDisponibilidadeModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> Save(InclusaoReservaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelarReserva(long reserva)
        {
            throw new NotImplementedException();
        }

        public async Task<ReservaTimeSharingCMModel> Editar(long reservaId)
        {
            throw new NotImplementedException();
        }

        public Task<List<HotelModel>> HoteisVinculados()
        {
            throw new NotImplementedException();
        }

        public Task<bool?> CancelarReserva(CancelarReservaTsModel model)
        {
            throw new NotImplementedException();
        }

        public Task<DadosFinanceirosContratoModel> DadosUtilizacaoContrato(int idVendaXContrato)
        {
            throw new NotImplementedException();
        }

        public Task<ReservaTsModel?> AlterarReserva(InclusaoReservaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ParametroSistemaViewModel?> GetParametroSistema()
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci(SearchReservasRciModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(long numReserva)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VincularReservaRCI(VincularReservaRciModel vincularModel)
        {
            throw new NotImplementedException();
        }

        public Task<IList<PeriodoDisponivelResultModel>?> DisponibilidadeParaTroca(SearchDisponibilidadeParaTrocaModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<TrocaPeriodoResponseModel> TrocarPeriodo(TrocaPeriodoRequestModel model)
        {
            throw new NotImplementedException();
        }

        public Task<TrocaTipoUsoResponseModel> TrocarTipoUso(TrocaTipoUsoRequestModel model)
        {
            throw new NotImplementedException();
        }


        public Task<decimal> CalcularPontosNecessariosSimplificado(DateTime dataInicial, DateTime dataFinal, int quantidadeAdultos, int quantidadeCriancas1, int quantidadeCriancas2, int hotelId, int tipoUhId, int idVendaXContrato, string numeroContrato, string? numReserva, List<Models.TimeSharing.HospedeInputModel>? hospedes = null)
        {
            throw new NotImplementedException();
        }

        public Task<CalcularPontosResponseModel> CalcularPontosNecessarios(CalcularPontosRequestModel request)
        {
            throw new NotImplementedException();
        }

        public DadosContratoModel GetContrato(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
        {
            throw new NotImplementedException();
        }
    }
}
