namespace CMDomain.Entities
{
    public class PrazoEntregaOc : CMEntityBase
    {
        public virtual int? IdItemOc { get; set; }
        public virtual int? ParcelaEntrega { get; set; }
        public virtual int? PrazoEntrega { get; set; }
        public virtual decimal? QtdeEntrega { get; set; }
        public virtual string? PeriodoPrazo { get; set; } = "D";
        public virtual DateTime? DataEntrega { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdItemOc.GetHashCode() + ParcelaEntrega.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            PrazoEntregaOc? cc = obj as PrazoEntregaOc;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
