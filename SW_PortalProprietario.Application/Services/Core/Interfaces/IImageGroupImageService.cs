using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IImageGroupImageService
    {
        Task<ImageGroupImageModel> SaveImage(ImageGroupImageInputModel model);
        Task<DeleteResultModel> DeleteImage(int id);
        Task<IEnumerable<ImageGroupImageModel>?> Search(SearchImageGroupImageModel searchModel);
        Task ReorderImages(List<ReorderImageModel> images);
    }
}
