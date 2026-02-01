namespace CMDomain.Entities
{
    public class ProcXArt : CMEntityBase
    {
        public virtual int? CodProcesso { get; set; }
        public virtual int? IdProcXArt { get; set; }
        public virtual int? IdProdVari { get; set; }
        public virtual decimal? QtdePedida { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual string? CodArtigo { get; set; }
        public virtual string? Justificativa { get; set; }
        public virtual DateTime? DataNecessidade { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdProcXArt.GetHashCode() + CodProcesso.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            ProcXArt? cc = obj as ProcXArt;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
