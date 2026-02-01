using CMDomain.Models.Financeiro;

namespace CMDomain.Models.Fornecedor
{
    public class FornecedorInputModel : ModelRequestBase
    {
        public string? NomeOuRazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public string? Email { get; set; }
        public string TipoPessoa { get; set; } = "J";
        public string? Sexo { get; set; } = "F";
        public DateTime? DataNascimento { get; set; }
        public string? ContaCreditoFornecedor { get; set; }
        public string? ContaCreditoAdiantamento { get; set; }
        public string? ContaCreditoDespesa { get; set; }
        public int? CodSubConta { get; set; }
        public int? IdEmpresa { get; set; }
        public bool? ReinfProdutorTribFolhaPag { get; set; } = false;
        public bool? CadastrarEmTodasAsEmpresas { get; set; } = false;

        public List<ContaBancariaDentroFornecedorInputModel> DadosBancarios { get; set; } = new List<ContaBancariaDentroFornecedorInputModel>();
        public List<int> ReplicarApenasNasEmpresas { get; set; } = new List<int>();
        public List<DocumentoInputModel> Documentos { get; set; } = new List<DocumentoInputModel>();
        public List<EnderecoInputModel> Enderecos { get; set; } = new List<EnderecoInputModel>();
        public List<TelefoneInputModel> Telefones { get; set; } = new List<TelefoneInputModel>();
        public List<FornecedorXRamoInputModel> RamosAtividade { get; set; } = new List<FornecedorXRamoInputModel>();
        public List<TipoDesembolsoViewModel> TiposDesembolso { get; set; } = new List<TipoDesembolsoViewModel>();

    }
}
