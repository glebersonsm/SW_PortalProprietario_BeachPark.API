using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IRegraPaxFreeService
    {
        Task<RegraPaxFreeModel> SaveRegraPaxFree(RegraPaxFreeInputModel model);
        Task<RegraPaxFreeModel> UpdateRegraPaxFree(AlteracaoRegraPaxFreeInputModel model);
        Task<DeleteResultModel> DeleteRegraPaxFree(int id);
        Task<IEnumerable<RegraPaxFreeModel>?> Search(SearchPadraoModel searchModel);
        Task<RegraPaxFreeModel?> GetRegraVigente(int? hotelId = null);
    }
}

