using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IFinanceiroTransacaoService
    {
        Task<CardTokenizedModel?> Tokenize(CardTokenizeRequestModel cardModel);
        Task<List<CardTokenizedModel>> GetAllTokenizedCardFromUser(SearchTokenizedCardFromUserModel searchModel);
        Task<TransactionCardResultModel?> DoCardTransaction(DoTransactionCardInputModel doTransactionModel);
        Task<TransactionPixResultModel?> GeneratePixTransaction(DoTransactionPixInputModel doTransactionModel);
        Task<bool?> CancelCardTransaction(string paymentId);
        Task<TransactionCardResultModel?> GetTransactionResult(string paymentId);
        Task<(int pageNumber, int lastPageNumber, List<TransactionSimplifiedResultModel> transactionResult)?> SearchTransacoes(SearchTransacoesModel searchTransacoesModel);
    }
}
