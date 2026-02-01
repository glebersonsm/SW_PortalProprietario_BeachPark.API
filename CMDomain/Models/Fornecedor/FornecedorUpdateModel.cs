namespace CMDomain.Models.Fornecedor
{
    public class FornecedorUpdateModel : ModelRequestBase
    {
        public int? IdFornecedor { get; set; }
        public int? IdEmpresa { get; set; }
        public string? NomeOuRazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public string? Email { get; set; }
        public bool? Ativo { get; set; }
        public bool? ReinfProdutorTribFolhaPag { get; set; } = false;
        public List<ContaBancariaDentroFornecedorInputModel> DadosBancarios { get; set; } = new List<ContaBancariaDentroFornecedorInputModel>();
        public List<FornecedorXRamoInputModel> RamosAtividade { get; set; } = new List<FornecedorXRamoInputModel>();
        public List<FornecedorTipoDesembolsoInputModel> TiposDesembolso { get; set; } = new List<FornecedorTipoDesembolsoInputModel>();


    }
}
