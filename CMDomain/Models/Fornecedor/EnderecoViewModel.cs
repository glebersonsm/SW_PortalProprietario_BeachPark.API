
namespace CMDomain.Models.Fornecedor
{
    public class EnderecoViewModel
    {
        public int? IdEndereco { get; set; }
        public bool? Comercial { get; set; } = false;
        public bool? Residencial { get; set; } = false;
        public bool? Entrega { get; set; } = false;
        public bool? Cobranca { get; set; } = false;
        public string? IdFornecedor { get; set; }
        public string? Logradouro { get; set; }
        public string? Complemento { get; set; }
        public string? Nome { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cep { get; set; }
        public int? IdCidade { get; set; }
        public string? NomeCidade { get; set; }
        public string? Uf { get; set; }
        public string? NomeEstado { get; set; }
        public string? CodigoMunicipio { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        //C = Comercial
        //R = Residencial
        //E = Entrega
        //B = Cobranca
        //P = Correspondencia

    }
}
