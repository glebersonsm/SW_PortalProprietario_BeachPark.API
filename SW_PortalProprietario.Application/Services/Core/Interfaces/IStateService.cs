using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IStateService
    {
        Task<EstadoModel> SaveState(RegistroEstadoInputModel model);
        Task<EstadoModel> UpdateState(AlteracaoEstadoInputModel model);
        Task<DeleteResultModel> DeleteState(int id);
        Task<IEnumerable<EstadoModel>?> SearchState(EstadoSearchModel searchModel);
    }
}
