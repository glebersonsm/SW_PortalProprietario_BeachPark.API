namespace CMDomain.Entities
{
    public class AgregTotOc : CMEntityBase
    {
        public virtual int? IdAgregTotOc { get; set; }
        public virtual int? CodTipoCustAgreg { get; set; }
        public virtual int? NumOc { get; set; }
        public virtual decimal? Aliquota { get; set; }
        public virtual decimal? BaseCalculo { get; set; }
        public virtual decimal? PercBaseCalculo { get; set; }
        public virtual decimal? VlrAgregTot { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

    }
}
