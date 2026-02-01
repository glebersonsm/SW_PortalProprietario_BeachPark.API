
namespace CMDomain.Models.Fornecedor
{
    public class AgenciaBancariaViewModel
    {
        public int? IdAgenciaBancaria { get; set; }
        public int? IdBanco { get; set; }
        public string? NumBanco { get; set; }
        public string? NomeBanco { get; set; }
        public string? NumAgencia { get; set; }
        public string? FlgAtivo { get; set; }
        public string? TrgUserInclusao { get; set; }
        public DateTime? TrgDtInclusao { get; set; }

    }
}
