namespace CMDomain.Models.AuthModels
{
    public class UsuarioGrupoUsuarioModel
    {
        public virtual int? GrupoId { get; set; }
        public virtual string? GrupoNome { get; set; }
        public virtual int? UsuarioId { get; set; }
        public virtual string? UsuarioNome { get; set; }

    }
}
