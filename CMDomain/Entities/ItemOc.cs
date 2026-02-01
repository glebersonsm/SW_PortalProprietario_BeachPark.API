namespace CMDomain.Entities
{
    public class ItemOc : CMEntityBase
    {
        public virtual int? NumOc { get; set; }
        public virtual int? IdItemOc { get; set; }
        public virtual string? CodArtigo { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual decimal? QtdePedida { get; set; }
        public virtual decimal? QtdeRecebida { get; set; }
        public virtual string? FlgItemAtendido { get; set; } = "F";
        public virtual string? ObsItemOc { get; set; }
        public virtual decimal? ValorInicial { get; set; }
        public virtual decimal? ValorUn { get; set; }
        public virtual string? CanceladoPor { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
