using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface ITransacaoFinanceiraService
    {
        Task<TransactionPixResultModel> GerarPix(int clienteId, List<int> contasIds);
        Task<TransactionCardResultModel> PagarComCartao(int clienteId, List<int> contasIds);
        Task<CardTokenizedModel> TokenizeCard(int clienteId, CardModel cardModel);

    }
}
