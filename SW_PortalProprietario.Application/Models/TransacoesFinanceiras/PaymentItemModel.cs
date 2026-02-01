namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class PaymentItemModel
    {
        public int? ItemId { get; set; }
        public decimal? Valor { get; set; }
        public decimal? ValorNaTransacao { get; set; }
        public string? DescricaoDoItem { get; set; }
        public DateTime? Vencimento { get; set; }
    }
}
