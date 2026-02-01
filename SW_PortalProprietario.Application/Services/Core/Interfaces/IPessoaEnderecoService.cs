using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IPessoaEnderecoService
    {
        Task<PessoaEnderecoModel> Salvar(PessoaEnderecoInputModel model);
        Task<List<PessoaEnderecoModel>> SalvarLista(List<PessoaEnderecoInputModel> pessoaEnderecos);
        Task<PessoaEnderecoModel> Update(PessoaEnderecoInputModel model);
        Task<DeleteResultModel> Remover(int id);
        Task<IEnumerable<PessoaEnderecoModel>?> Search(SearchPadraoComListaIdsModel searchModel);
    }
}


