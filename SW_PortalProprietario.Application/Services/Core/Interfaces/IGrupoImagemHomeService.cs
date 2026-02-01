using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IGrupoImagemHomeService
    {
        Task<GrupoImagemHomeModel?> SaveGrupoImagemHome(GrupoImagemHomeInputModel model);
        Task<DeleteResultModel> DeleteGrupoImagemHome(int id);
        Task<IEnumerable<GrupoImagemHomeModel>?> Search(SearchGrupoImagemHomeModel searchModel);
        Task ReorderGroups(List<ReorderImageGroupModel> groups);
    }
}

