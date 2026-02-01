namespace CMDomain.Models.AuthModels
{
    public class GrupoUsuarioModel
    {
        public virtual int? GrupoId { get; set; }
        public virtual string? GrupoNome { get; set; }
        public virtual string? Descricao { get; set; }
        public List<UsuarioGrupoUsuarioModel>? Usuarios { get; set; }

    }
}
