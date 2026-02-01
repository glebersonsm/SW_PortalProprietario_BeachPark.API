namespace CMDomain.Models.Fornecedor
{
    public class AgenciaBancariaInputModel : ModelRequestBase
    {
        public int? IdAgenciaBancaria { get; set; }
        public int? IdBanco { get; set; }
        public string? NumAgencia { get; set; }
        public string? FlgAtivo { get; set; }

    }
}
