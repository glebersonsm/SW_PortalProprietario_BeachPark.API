namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class CardTokenizedModel : ModelBase
    {
        public int? PessoaId { get; set; }
        public string? PessoaNome { get; set; }
        public string? CardHolder { get; set; }
        public CardInputModel? card { get; set; }
        public string? token { get; set; }
        public string? token2 { get; set; }
        public CompanyModel? company { get; set; }
        public string? status { get; set; }
        public List<AcquirerResponsesCardModel>? acquirer_responses { get; set; }
        public List<string> errors { get; set; } = new List<string>();
    }
}
