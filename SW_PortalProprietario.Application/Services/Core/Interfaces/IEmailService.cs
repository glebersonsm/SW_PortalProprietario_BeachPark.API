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
        /// <summary>
        /// Registra a primeira abertura do e-mail (quando o tracking pixel Ã© carregado).
        /// SÃ³ atualiza se DataHoraPrimeiraAbertura ainda for null.
        /// </summary>
        Task RecordEmailOpen(int emailId);
    }
}
