using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface ITagsService
    {
        Task<TagsModel> SaveTags(TagsInputModel model);
        Task<DeleteResultModel> DeleteTags(int id);
        Task<IEnumerable<TagsModel>?> SearchTags(SearchPadraoModel searchModel);
    }
}
