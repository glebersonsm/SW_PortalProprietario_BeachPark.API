namespace CMDomain.Models.AuthModels
{
    public class SearchUsuarioModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }
        public string? NomeCompleto { get; set; }
        public string? CpfCnpj { get; set; }

    }
}
