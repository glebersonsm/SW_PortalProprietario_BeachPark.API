using CMDomain.Models.Financeiro;

namespace CMDomain.Models.Fornecedor
{
    public class FornecedorViewModel
    {
        public int? IdFornecedor { get; set; }
        public int? Plano { get; set; }
        public string? ContaContabilFornecedor { get; set; }
        public string? Status { get; set; }
        public string? NomeOuRazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public string? Email { get; set; }
        public string? ContaCreditoFornecedor { get; set; }
        public string? NomeContaCreditoFornecedor { get; set; }
        public string? ContaCreditoAdiantamento { get; set; }
        public string? NomeContaCreditoAdiantamento { get; set; }
        public string? ContaCreditoDespesa { get; set; }
        public string? NomeContaCreditoDespesa { get; set; }
        public int? CodSubConta { get; set; }
        public string? NomeSubConta { get; set; }
        public bool? ReinfProdutorTribFolhaPag { get; set; } = false;
        public List<DocumentoViewModel> Documentos { get; set; } = new List<DocumentoViewModel>();
        public List<EnderecoViewModel> Enderecos { get; set; } = new List<EnderecoViewModel>();
        public List<TelefoneViewModel> Telefones { get; set; } = new List<TelefoneViewModel>();
        public List<FornecedorXRamoViewModel> RamosAtividade { get; set; } = new List<FornecedorXRamoViewModel>();
        public List<FornecedorTipoDesembolsoViewModel> TiposDesembolso { get; set; } = new List<FornecedorTipoDesembolsoViewModel>();
        //public List<ContaBancariaViewModel> DadosBancarios { get; set; } = new List<ContaBancariaViewModel>();
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

    }
}
