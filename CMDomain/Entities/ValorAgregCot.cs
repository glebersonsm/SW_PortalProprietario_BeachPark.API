namespace CMDomain.Entities
{
    public class ValorAgregCot : CMEntityBase
    {
        public virtual int? CodProcesso { get; set; }
        public virtual int? IdProcXArt { get; set; }
        public virtual int? Proposta { get; set; }
        public virtual int? CodTipoCustAgreg { get; set; }
        public virtual decimal? BaseCalculo { get; set; }
        public virtual decimal? Percentual { get; set; }
        public virtual decimal? PercBaseCalculo { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdProcXArt.GetHashCode() + CodProcesso.GetHashCode() + Proposta.GetHashCode() + CodTipoCustAgreg.GetHashCode() + IdForCli.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            ValorAgregCot? cc = obj as ValorAgregCot;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
