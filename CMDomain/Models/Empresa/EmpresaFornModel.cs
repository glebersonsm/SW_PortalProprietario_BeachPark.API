namespace CMDomain.Models.Empresa
{
    public class EmpresaFornViewModel
    {
        public int? IdEmpresa { get; set; }
        public int? Plano { get; set; }
        public string? Nome { get; set; }
        public string? ContacForn { get; set; }
        public string? ContacAdiantamento { get; set; }
        public string? ContacDespesa { get; set; }
        public int? CodSubConta { get; set; }
        public string? FlgNaoContabLanc { get; set; } = "N";
        public string? FlgContBaixDesemb { get; set; } = "N";

    }

}
