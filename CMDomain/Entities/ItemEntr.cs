namespace CMDomain.Entities
{
    public class ItemEntr : CMEntityBase
    {
        public virtual int? IdItemEntrega { get; set; }
        public virtual long? NumRequisicao { get; set; }
        public virtual string? CodArtigo { get; set; }
        public virtual decimal? QtdeEntrega { get; set; }
        public virtual DateTime? DataEntrega { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual decimal? ValorUn { get; set; }
        public virtual string? FlgStatus { get; set; } = "F";
        public virtual int? IdAtendente { get; set; }
        public virtual int? IdUsuarioConfDev { get; set; }
        public virtual string? ObsDevol { get; set; }
        public virtual DateTime? DataConfDevol { get; set; }
        public virtual string? FlgConfDevol { get; set; } = "N";
        public virtual int? IdMovDevol { get; set; }
        public virtual int? IdMov { get; set; }
        public virtual DateTime? DataReceb { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
