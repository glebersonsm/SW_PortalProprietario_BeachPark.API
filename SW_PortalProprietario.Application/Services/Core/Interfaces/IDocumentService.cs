using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentoModel> SaveDocument(DocumentInputModel model);
        Task<DocumentoModel> UpdateDocument(AlteracaoDocumentInputModel model);
        Task<DeleteResultModel> DeleteDocument(int id);
        Task<DocumentoModel> DownloadFile(int id);
        Task<IEnumerable<DocumentoModel>?> Search(SearchPadraoModel searchModel);
        Task<IEnumerable<DocumentoHistoricoModel>?> History(int id);
        Task<bool> ReorderDocuments(List<ReorderDocumentModel> documents);
    }
}
