using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IDocumentGroupService
    {
        Task<GrupoDocumentoModel> SaveDocumentGroup(DocumentGroupInputModel model);
        Task<DeleteResultModel> DeleteDocumentGroup(int id);
        Task<IEnumerable<GrupoDocumentoModel>?> Search(SearchGrupoDocumentoModel searchModel);
        Task<bool> ReorderGroups(List<ReorderDocumentGroupModel> groups);
    }
}
