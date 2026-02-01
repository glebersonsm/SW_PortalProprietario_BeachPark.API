namespace CMDomain.Entities
{
    public class PrazoPgto : CMEntityBase
    {
        public virtual int? CodProcesso { get; set; }
        public virtual int? IdProcXArt { get; set; }
        public virtual int? Proposta { get; set; }
        public virtual int? IdPrazoPgto { get; set; }
        public virtual int? Prazopgto { get; set; }
        public virtual decimal? Percentual { get; set; } = 100;
        public virtual decimal? Valor { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual string? PeriodoPrazo { get; set; } = "D";
        public virtual DateTime? DataPgto { get; set; }
        public virtual string? FlgAdiantamento { get; set; } = "N";
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdProcXArt.GetHashCode() + CodProcesso.GetHashCode() + Proposta.GetHashCode() + IdPrazoPgto.GetHashCode() + IdForCli.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            PrazoPgto? cc = obj as PrazoPgto;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
