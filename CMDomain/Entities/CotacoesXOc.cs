namespace CMDomain.Entities
{
    public class CotacoesXOc : CMEntityBase
    {
        public virtual int? CodProcesso { get; set; }
        public virtual int? IdProcXArt { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual int? Proposta { get; set; }
        public virtual int? IdItemOc { get; set; }
        public virtual int? NumOc { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdProcXArt.GetHashCode() + IdForCli.GetHashCode() + CodProcesso.GetHashCode() + Proposta.GetHashCode() + IdItemOc.GetHashCode() + NumOc.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            Cotacoes? cc = obj as Cotacoes;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
