using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface ITipoTelefoneService
    {
        Task<TipoTelefoneModel> Salvar(TipoTelefoneInputModel model);
        Task<TipoTelefoneModel> Update(TipoTelefoneInputModel model);
        Task<DeleteResultModel> Remover(int id);
        Task<IEnumerable<TipoTelefoneModel>?> Search(SearchPadraoModel searchModel);
    }
}
