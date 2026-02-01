using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IFaqService
    {
        Task<FaqModel> SaveFaq(FaqInputModel model);
        Task<DeleteResultModel> DeleteFaq(int id);
        Task<IEnumerable<FaqModelSimplificado>?> Search(SearchFaqModel searchModel);
        Task<bool> ReorderFaqs(List<ReorderFaqModel> faqs);
    }
}
