using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IPessoaDocumentoService
    {
        Task<PessoaDocumentoModel> Salvar(PessoaDocumentoInputModel model);
        Task<List<PessoaDocumentoModel>> SalvarLista(List<PessoaDocumentoInputModel> pessoaDocumentos);
        Task<PessoaDocumentoModel> Update(PessoaDocumentoInputModel model);
        Task<DeleteResultModel> Remover(int id);
        Task<IEnumerable<PessoaDocumentoModel>?> Search(SearchPadraoComListaIdsModel searchModel);
    }
}
