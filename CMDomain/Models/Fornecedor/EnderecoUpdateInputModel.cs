namespace CMDomain.Models.Fornecedor
{
    public class EnderecoUpdateInputModel : ModelRequestBase
    {
        public int? IdEndereco { get; set; }
        public bool? Comercial { get; set; } = false;
        public bool? Residencial { get; set; } = false;
        public bool? Entrega { get; set; } = false;
        public bool? Cobranca { get; set; } = false;
        public int? IdFornecedor { get; set; }
        public int? IdEmpresa { get; set; }
        public string? Logradouro { get; set; }
        public string? Complemento { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cep { get; set; }
        public int? IdCidade { get; set; }

    }
}
