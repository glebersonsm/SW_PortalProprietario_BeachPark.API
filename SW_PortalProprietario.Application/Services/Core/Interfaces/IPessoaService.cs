using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IPessoaService
    {
        Task<PessoaCompletaModel> SalvarPessoaFisica(PessoaFisicaInputModel model);
        Task<PessoaCompletaModel> SalvarPessoaJuridica(PessoaJuridicaInputModel model);
        Task<DeleteResultModel> Remover(int id);
        Task<IEnumerable<PessoaCompletaModel>?> Search(PessoaSearchModel searchModel);
    }
}
