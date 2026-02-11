using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IImagemGrupoImagemHomeService
    {
        Task<ImagemGrupoImagemHomeModel> SaveImagem(ImagemGrupoImagemHomeInputModel model);
        Task<DeleteResultModel> DeleteImagem(int id);
        Task<IEnumerable<ImagemGrupoImagemHomeModel>?> Search(SearchImagemGrupoImagemHomeModel searchModel);
        Task<IEnumerable<ImagemGrupoImagemHomeModel>?> SearchForHome();
        Task<IEnumerable<ImagemGrupoImagemHomeModel>?> SearchForHomePublic();
        Task ReorderImages(List<ReorderImageModel> images);
    }
}

