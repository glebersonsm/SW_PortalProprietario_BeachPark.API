namespace CMDomain.Entities
{
    public class AgregItemOc : CMEntityBase
    {
        public virtual int? IdAgregItemOc { get; set; }
        public virtual int? CodTipoCustAgreg { get; set; }
        public virtual int? IdItemOc { get; set; }
        public virtual decimal? Aliquota { get; set; }
        public virtual decimal? BaseCalculo { get; set; }
        public virtual decimal? PercBaseCalculo { get; set; }
        public virtual decimal? VlrAgregItem { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

    }
}
