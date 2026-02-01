using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IImageGroupService
    {
        Task<ImageGroupModel?> SaveImageGroup(ImageGroupInputModel model);
        Task<DeleteResultModel> DeleteImageGroup(int id);
        Task<IEnumerable<ImageGroupModel>?> Search(SearchImageGroupModel searchModel);
        Task ReorderGroups(List<ReorderImageGroupModel> groups);
    }
}
