namespace CMDomain.Entities
{
    public class GrupoRelatorio : CMEntityBase
    {
        public virtual int? IdGrupoRelatorio { get; set; }
        public virtual int? OrigemCM { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdGrupoRelatorio.GetHashCode() + OrigemCM.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            GrupoRelatorio? grupoRlatorio = obj as GrupoRelatorio;
            if (grupoRlatorio is null) return false;
            return grupoRlatorio.Equals(this);
        }

    }
}
