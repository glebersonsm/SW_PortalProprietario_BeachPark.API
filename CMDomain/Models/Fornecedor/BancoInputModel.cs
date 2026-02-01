namespace CMDomain.Models.Fornecedor
{
    public class BancoInputModel : ModelRequestBase
    {
        public string? NumBanco { get; set; }
        public string? NomeBanco { get; set; }
        public string? CnpjBanco { get; set; }

    }
}
