namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class TransactionCardResultModel
    {
        public string? payment_id { get; set; }
        public string? merchant_id { get; set; }
        public string? channel { get; set; }
        public CustomerModel? customer { get; set; }
        public PaymentModel? payment { get; set; }
        public string? description { get; set; } //Breve descrição do produto
        public string? date { get; set; }
        public List<dynamic>? splits { get; set; }
        public List<dynamic>? recurring_merchant_ids { get; set; }
        public bool? vip { get; set; } = false;
        public CompanyModel? company { get; set; }
        public string? status { get; set; }
        public AcquirerResponsesCardModel? last_acquirer_response { get; set; }
        public List<AcquirerResponsesCardModel>? acquirer_responses { get; set; }
        public List<string> errors { get; set; } = new List<string>();
        public string? statusCode { get; set; }

    }
}
