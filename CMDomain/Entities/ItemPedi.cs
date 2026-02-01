namespace CMDomain.Entities
{
    public class ItemPedi : CMEntityBase
    {
        public virtual long NumRequisicao { get; set; }
        public virtual string? CodArtigo { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual decimal? ValorUn { get; set; }
        public virtual decimal? QtdePedida { get; set; }
        public virtual decimal? QtdePendente { get; set; }
        public virtual string? FlgSci { get; set; }
        public virtual string? Obs { get; set; }
        public virtual decimal? QtdePendVenda { get; set; }
        public virtual DateTime? DtCancelamento { get; set; }
        public virtual decimal? QtdeCancelada { get; set; }
        public virtual decimal? QtdeAprovadaRad { get; set; }
        public virtual DateTime? DataNecessidade { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual decimal? QtdeAtender { get; set; }

        public override int GetHashCode()
        {
            return NumRequisicao.GetHashCode() + CodArtigo.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            ItemPedi? cc = obj as ItemPedi;
            if (cc is null) return false;
            return cc.Equals(this);
        }

    }
}
