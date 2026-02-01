namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class UsuarioGrupoUsuarioModel : ModelBase
    {
        public int? UsuarioId { get; set; }
        public int? GrupoUsuarioId { get; set; }
        public string? GrupoUsuarioNome { get; set; }

    }
}
