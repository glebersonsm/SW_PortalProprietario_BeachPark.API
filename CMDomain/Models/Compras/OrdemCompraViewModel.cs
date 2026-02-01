namespace CMDomain.Models.Compras
{
    public class OrdemCompraViewModel
    {
        public int? NumOc { get; set; }
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
        public int? IdComprador { get; set; }
        public string? NomeComprador { get; set; }
        public List<OrdemCompraItemViewModel>? OrdemCompraItens { get; set; }
    }
}
