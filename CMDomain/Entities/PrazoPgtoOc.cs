namespace CMDomain.Entities
{
    public class PrazoPgtoOc : CMEntityBase
    {
        public virtual int? IdItemOc { get; set; }
        public virtual int? ParcelaPgto { get; set; }
        public virtual int? PrazoPgto { get; set; }
        public virtual decimal? PercPagto { get; set; }
        public virtual string? PeriodoPrazo { get; set; } = "D";
        public virtual decimal? ValorPagto { get; set; }
        public virtual DateTime? DataPagto { get; set; }
        public virtual string? FlgAdiantamento { get; set; } = "N";
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdItemOc.GetHashCode() + ParcelaPgto.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            PrazoPgtoOc? cc = obj as PrazoPgtoOc;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
