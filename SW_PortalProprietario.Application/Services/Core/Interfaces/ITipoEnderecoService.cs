using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface ITipoEnderecoService
    {
        Task<TipoEnderecoModel> Salvar(TipoEnderecoInputModel model);
        Task<TipoEnderecoModel> Update(TipoEnderecoInputModel model);
        Task<DeleteResultModel> Remover(int id);
        Task<IEnumerable<TipoEnderecoModel>?> Search(SearchPadraoModel searchModel);
    }

}
