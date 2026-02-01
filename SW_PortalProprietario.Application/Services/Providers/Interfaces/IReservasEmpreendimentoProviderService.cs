using SW_PortalProprietario.Application.Models.TimeSharing;

namespace SW_PortalProprietario.Application.Services.Providers.Interfaces
{
    public interface IReservasEmpreendimentoProviderService
    {

        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral(SearchReservasGeralModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral(SearchMinhasReservasGeralModel searchModel);


    }
}
