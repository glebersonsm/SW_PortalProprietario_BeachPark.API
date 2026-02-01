namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class TransactionPixResultModel
    {
        public string? payment_id { get; set; }
        public CustomerModel? customer { get; set; }
        public WebPaymentModel? web_payment { get; set; }
        public string? description { get; set; } //Breve descrição do produto
        public DateTime? expiration_date { get; set; }
        public int? expiration { get; set; }
        public string? date { get; set; }
        public List<dynamic>? recurring_merchant_ids { get; set; }
        public CompanyModel? company { get; set; }
        public string? status { get; set; }
        public AcquirerResponsesPixModel? last_acquirer_response { get; set; }
        public List<AcquirerResponsesPixModel>? acquirer_responses { get; set; }
        public string? qrCode { get; set; }
        public List<string> errors { get; set; } = new List<string>();

    }
}
