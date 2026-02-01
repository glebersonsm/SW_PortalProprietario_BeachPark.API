namespace CMDomain.Entities
{
    public class Autoriza : CMEntityBase
    {
        public virtual int? IdEspAcesso { get; set; }
        public virtual int? IdOperFunc { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? FlgHabilita { get; set; }
        public virtual string? FlgVisualiza { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdEspAcesso.GetHashCode() + IdOperFunc.GetHashCode() + IdPessoa.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            Autoriza? autoriza = obj as Autoriza;
            if (autoriza is null) return false;
            return autoriza.Equals(this);
        }

    }
}
