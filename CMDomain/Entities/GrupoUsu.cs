namespace CMDomain.Entities
{
    public class GrupoUsu : CMEntityBase
    {
        public virtual int? IdGrupo { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdGrupo.GetHashCode() + IdUsuario.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            GrupoUsu? grupoUsu = obj as GrupoUsu;
            if (grupoUsu is null) return false;
            return grupoUsu.Equals(this);
        }

    }
}
