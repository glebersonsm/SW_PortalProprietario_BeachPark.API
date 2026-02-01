using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IEmailService
    {
        Task<bool> Save(EmailInputModel model);
        Task<bool> Send(int id);
        Task<EmailModel> SaveInternal(EmailInputInternalModel model);
        Task<EmailModel> Update(AlteracaoEmailInputModel model);
        Task MarcarComoEnviado(int id);
        Task<DeleteResultModel> DeleteEmail(int id);
        Task<(int pageNumber, int lastPageNumber, IEnumerable<EmailModel> emails)?> Search(SearchEmailModel searchModel);
    }
}
