namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class WebPaymentModel
    {
        public decimal? value { get; set; }
        public List<TransactionItemInputModel>? items { get; set; }
    }
}
