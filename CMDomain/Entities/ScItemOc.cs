namespace CMDomain.Entities
{
    public class ScItemOc : CMEntityBase
    {
        public virtual int? NumSolCompra { get; set; }
        public virtual int? IdItemOc { get; set; }
        public virtual int? IdItemSoli { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdItemOc.GetHashCode() + NumSolCompra.GetHashCode() + IdItemSoli.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            ScItemOc? cc = obj as ScItemOc;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
