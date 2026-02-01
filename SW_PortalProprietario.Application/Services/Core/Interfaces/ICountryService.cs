using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface ICountryService
    {
        Task<PaisModel> SaveCountry(RegistroPaisInputModel model);
        Task<PaisModel> UpdateCountry(AlteracaoPaisInputModel model);
        Task<DeleteResultModel> DeleteCountry(int id);
        Task<IEnumerable<PaisModel>?> SearchCountry(CountrySearchModel searchModel);
    }
}
