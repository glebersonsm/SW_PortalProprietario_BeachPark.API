using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.TimeSharing;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface ICityService
    {
        Task<CidadeModel> SaveCity(RegistroCidadeInputModel model);
        Task<CidadeModel> UpdateCity(AlteracaoCidadeInputModel model);
        Task<DeleteResultModel> DeleteCity(int id);
        Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCity(CidadeSearchModel searchModel);
        Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCityOnProvider(CidadeSearchModel searchModel);
        Task<CepResponseModel> ConsultarCep(string cep);
    }
}
