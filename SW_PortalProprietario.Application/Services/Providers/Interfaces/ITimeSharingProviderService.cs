using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;

namespace SW_PortalProprietario.Application.Services.Providers.Interfaces
{
    public interface ITimeSharingProviderService
    {
        Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing(SearchContratosTimeSharingModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetMeusContratosTimeSharing(SearchMeusContratosTimeSharingModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos(SearchReservaTsModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral(SearchReservasGeralModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci(SearchReservasRciModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetMinhasReservasGeralComConsumoPontos(SearchMinhasReservaTsModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral(SearchMinhasReservasGeralModel searchModel);
        Task<IList<PeriodoDisponivelResultModel>?> Disponibilidade(SearchDisponibilidadeModel searchModel);
        Task<List<HotelModel>> HoteisVinculados();
        Task<ReservaTimeSharingCMModel> Visualizar(string reservanumero);
        Task<ReservaTimeSharingCMModel> VisualizarMinha(string reservanumero);
        Task<Int64> Save(InclusaoReservaInputModel model);
        Task<bool?> CancelarReserva(CancelarReservaTsModel model);
        Task<ReservaTimeSharingCMModel?> Editar(long numReserva);
        Task<DadosFinanceirosContratoModel> DadosUtilizacaoContrato(int idVendaXContrato);
        Task<ReservaTsModel?> AlterarReserva(InclusaoReservaInputModel model);
        Task<ParametroSistemaViewModel?> GetParametroSistema();
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(long numReserva);
        Task<bool> VincularReservaRCI(VincularReservaRciModel vincularModel);
        Task<IList<PeriodoDisponivelResultModel>?> DisponibilidadeParaTroca(SearchDisponibilidadeParaTrocaModel searchModel);
        Task<TrocaPeriodoResponseModel> TrocarPeriodo(TrocaPeriodoRequestModel model);
        Task<TrocaTipoUsoResponseModel> TrocarTipoUso(TrocaTipoUsoRequestModel model);
        
        /// <summary>
        /// Calcula os pontos necessários para uma reserva de forma simplificada
        /// Método público para consumo do frontend
        /// </summary>
        Task<decimal> CalcularPontosNecessariosSimplificado(
            DateTime dataInicial,
            DateTime dataFinal,
            int quantidadeAdultos,
            int quantidadeCriancas1,
            int quantidadeCriancas2,
            int hotelId,
            int tipoUhId,
            int idVendaXContrato,
            string numeroContrato,
            string? numReserva,
            List<SW_PortalProprietario.Application.Models.TimeSharing.HospedeInputModel>? hospedes = null);
        Task<CalcularPontosResponseModel> CalcularPontosNecessarios(CalcularPontosRequestModel request);
        DadosContratoModel? GetContrato(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
    }
}
