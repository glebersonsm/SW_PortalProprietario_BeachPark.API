namespace CMDomain.Models.Compras
{
    public class OrdemCompraDtoModel
    {
        public int? IdItemOc { get; set; }
        public int? NumOc { get; set; }
        public string? Status { get; set; } //T = Atendida total, F = Não atendida, C = Cancelada
        public string? TipoOc { get; set; } //Decode(o.FlgComSemOc,'S','Automática/Manual sem Cot','Normal') as TipoOc
        public int? IdFornecedor { get; set; }
        public string? NomeOuRazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public string? CpfCnpjFornecedor { get; set; }
        public int? IdEmpresa { get; set; }
        public int? CodProcesso { get; set; }
        public DateTime? DataOc { get; set; }
        public string? ObsOc { get; set; }
        public string? TipoFrete { get; set; }
        public string? CodProduto { get; set; }
        public decimal? QuantidadePedida { get; set; }
        public decimal? QuantidadeRecebida { get; set; }
        public decimal? ValorUnitario { get; set; }
        public string? CodMedida { get; set; }
        public string? NomeProduto { get; set; }
        public int? IdComprador { get; set; }
        public string? NomeComprador { get; set; }


    }
}
