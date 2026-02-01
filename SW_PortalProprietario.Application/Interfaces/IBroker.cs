using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IBroker
    {
        Task<CardTokenizedModel?> Tokenize(CardTokenizeInputModel cardModel, int empresaLegadoId);
        Task<TransactionCardResultModel?> DoCardTransaction(TransactionCardModel transactionCardModel, int empresaLegadoId);
        Task<TransactionPixResultModel?> GeneratePixTransaction(TransactionPixModel transactionPixModel, int empresaLegadoId);
        Task<TransactionCancelResultModel?> CancelCardTransaction(TransactionCancelModel transactionCancelModel, int empresaLegadoId);
        Task<TransactionCardResultModel?> GetTransactionResult(string paymentId, int empresaLegadoId);
        Task GetTransactionPixResult(PaymentPix item, NHibernate.IStatelessSession? session);
        Task GetTransactionCardResult(PaymentCardTokenized item, NHibernate.IStatelessSession? session);
    }
}
