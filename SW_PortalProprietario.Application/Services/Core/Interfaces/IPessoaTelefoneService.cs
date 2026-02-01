using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IPessoaTelefoneService
    {
        Task<PessoaTelefoneModel> Salvar(PessoaTelefoneInputModel model);
        Task<List<PessoaTelefoneModel>> SalvarLista(List<PessoaTelefoneInputModel> pessoaTelefones);

        Task<PessoaTelefoneModel> Update(PessoaTelefoneInputModel model);
        Task<DeleteResultModel> Remover(int id);
        Task<IEnumerable<PessoaTelefoneModel>?> Search(SearchPadraoComListaIdsModel searchModel);
    }
}
