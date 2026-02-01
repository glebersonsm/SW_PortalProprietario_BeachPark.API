namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class TransactionCardModel
    {
        public string? merchant_id { get; set; }
        public string? channel { get; set; }
        public CustomerModel? customer { get; set; }
        public string? description { get; set; } //Breve descrição do produto
        public PaymentModel? payment { get; set; }
        public int? ip { get; set; }
        public bool? vip { get; set; } = false;
        public bool? fligth { get; set; } = false;
        public string? finger_print { get; set; } = null;


    }
}
