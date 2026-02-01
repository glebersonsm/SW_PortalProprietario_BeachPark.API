namespace CMDomain.Models.AuthModels
{
    public class UsuarioModel
    {
        public int? UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }
        public string? NomeCompleto { get; set; }
        public string? CpfCnpj { get; set; }
        public bool? Bloqueado { get; set; }
        public bool? Desativado { get; set; }
        public List<GrupoDeUsuarioDoUsuarioModel>? GruposDeUsuario { get; set; }

    }
}
