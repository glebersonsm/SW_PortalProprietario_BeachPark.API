namespace CMDomain.Models.AuthModels
{
    public class SearchPessoaModel : ModelRequestBase
    {
        public string? UsuarioNomeCompleto { get; set; }
        public string? DocumentoNumero { get; set; }

    }
}
