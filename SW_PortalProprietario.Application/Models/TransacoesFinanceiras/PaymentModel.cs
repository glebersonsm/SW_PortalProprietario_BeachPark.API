namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class PaymentModel
    {
        public decimal? value { get; set; }
        public int? installments { get; set; } //Quantidade de parcelas
        public bool? capture { get; set; }
        public TransactionTokenizedCardModel? card { get; set; }
        public List<TransactionItemInputModel>? items { get; set; }
    }
}
