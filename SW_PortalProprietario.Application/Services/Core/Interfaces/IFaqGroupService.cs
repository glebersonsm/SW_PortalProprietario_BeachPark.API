using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IFaqGroupService
    {
        Task<GrupoFaqModel> SaveGroup(FaqGroupInputModel model);
        Task<DeleteResultModel> DeleteGroup(int id);
        Task<IEnumerable<GrupoFaqModel>?> Search(SearchGrupoFaqModel searchModel);
        Task<bool> ReorderGroups(List<ReorderFaqGroupModel> groups);
    }
}
