using CMDomain.Models.Fornecedor;

namespace CMDomain.Models.AuthModels
{
    public class UserInputModel : ModelRequestBase
    {
        public bool? Ativo { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string TipoPessoa { get; set; } = "F";
        public string? Sexo { get; set; } = "F";
        public DateTime? DataNascimento { get; set; }
        public List<ContaBancariaDentroUsuarioInputModel> DadosBancarios { get; set; } = new List<ContaBancariaDentroUsuarioInputModel>();
        public List<DocumentoInputModel> Documentos { get; set; } = new List<DocumentoInputModel>();
        public List<EnderecoInputModel> Enderecos { get; set; } = new List<EnderecoInputModel>();
        public List<TelefoneInputModel> Telefones { get; set; } = new List<TelefoneInputModel>();

    }
}
