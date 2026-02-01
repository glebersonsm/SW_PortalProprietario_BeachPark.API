using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface ITipoDocumentoPessoaService
    {
        Task<TipoDocumentoPessoaModel> Salvar(TipoDocumentoPessoaInputModel model);
        Task<TipoDocumentoPessoaModel> Update(TipoDocumentoPessoaInputModel model);
        Task<DeleteResultModel> Remover(int id);
        Task<IEnumerable<TipoDocumentoPessoaModel>?> Search(SearchPadraoComTipoPessoaModel model);

    }
}
