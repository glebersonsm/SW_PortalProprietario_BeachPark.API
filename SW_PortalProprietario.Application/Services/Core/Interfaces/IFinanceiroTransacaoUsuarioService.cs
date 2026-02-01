using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IFinanceiroTransacaoUsuarioService
    {
        Task<CardTokenizedModel?> TokenizeMyCard(TokenizeMyCardInputModel cardModel);
        Task<TransactionCardResultModel?> DoCardTransaction(DoTransactionCardInputModel doTransactionModel);
        Task<TransactionPixResultModel?> GeneratePixTransaction(DoTransactionPixInputModel doTransactionModel);
        Task<TransactionCardResultModel?> GetTransactionResult(string paymentId);
        Task<List<CardTokenizedModel>> GetMyTokenizedCards();
        Task<bool> RemoveMyCardTokenized(int cardTokenizedId);
    }
}
