namespace CMDomain.Models.Fornecedor
{
    public class SearchAgenciaBancariaModel : ModelRequestBase
    {
        public int? IdAgenciaBancaria { get; set; }
        public int? IdBanco { get; set; }
        public string? NumAgencia { get; set; }
        public bool? ApenasAtiva { get; set; } = true;

    }
}
