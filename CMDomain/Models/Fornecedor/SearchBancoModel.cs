namespace CMDomain.Models.Fornecedor
{
    public class SearchBancoModel : ModelRequestBase
    {
        public int? IdBanco { get; set; }
        public string? NomeBanco { get; set; }
        public string? NumBanco { get; set; }

    }
}
