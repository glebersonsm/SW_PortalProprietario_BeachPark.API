namespace CMDomain.Models.AuthModels
{
    public class GetDireitoUsuarioModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }
        public int? EmpresaId { get; set; }
        public int? ModuloId { get; set; }
        public string? ModuloNome { get; set; }

    }
}
