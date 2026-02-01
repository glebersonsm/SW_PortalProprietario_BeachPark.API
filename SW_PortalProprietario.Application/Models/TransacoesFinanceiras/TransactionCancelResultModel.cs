namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class TransactionCancelResultModel
    {
        public decimal? value { get; set; }
        public CompanyModel? company { get; set; }
        public string? status { get; set; }
        public AcquirerResponsesModel? last_aquirer_response { get; set; }
        public List<AcquirerResponseModel>? acquirer_responses { get; set; }
        public List<string> errors { get; set; } = new List<string>();
    }
}
