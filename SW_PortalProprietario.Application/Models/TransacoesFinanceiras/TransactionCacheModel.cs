using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class TransactionCacheModel
    {
        public string? TransactionId { get; set; }
        public TransactionCardModel? TransactionCard { get; set; }
        public TransactionCardResultModel? TransactionCardResult { get; set; }
        public TransactionPixModel? TransactionPix { get; set; }
        public TransactionPixResultModel? TransactionPixResult { get; set; }
        public CardTokenized? CardTokenized { get; set; }
        public string? ContasVinculadasIds { get; set; }
        public List<ContaPendenteModel>? Contas { get; set; }
        public string? EmpresaLogadaId { get; set; }

    }
}
