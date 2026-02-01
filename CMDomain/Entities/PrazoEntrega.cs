namespace CMDomain.Entities
{
    public class PrazoEntrega : CMEntityBase
    {
        public virtual int? CodProcesso { get; set; }
        public virtual int? IdProcXArt { get; set; }
        public virtual int? Proposta { get; set; }
        public virtual int? IdPrazoEnt { get; set; }
        public virtual decimal? QtdeEnt { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual string? PeriodoPrazo { get; set; } = "D";
        public virtual int? PrazoEnt { get; set; }
        public virtual DateTime? DataEnt { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdProcXArt.GetHashCode() + CodProcesso.GetHashCode() + Proposta.GetHashCode() + IdPrazoEnt.GetHashCode() + IdForCli.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            PrazoEntrega? cc = obj as PrazoEntrega;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
