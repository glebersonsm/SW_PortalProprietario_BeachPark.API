namespace CMDomain.Entities
{
    public class ItemSoli : CMEntityBase
    {
        public virtual int? NumSolCompra { get; set; }
        public virtual int? IdItemSoli { get; set; }
        public virtual string? CodArtigo { get; set; }
        public virtual int? IdProcXArt { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual int? CodProcesso { get; set; }
        public virtual int? IdContratoProd { get; set; }
        public virtual int? IdProdVari { get; set; }
        public virtual decimal? QtdePedida { get; set; }
        public virtual decimal? SaldoAComprar { get; set; }
        public virtual decimal? QtdePendente { get; set; }
        public virtual string? SoliciAceita { get; set; }
        public virtual int? IdComprador { get; set; }
        public virtual string? ObsItemSolic { get; set; }
        public virtual string? StatusItem { get; set; }
        public virtual DateTime? DataCancel { get; set; }
        public virtual int? IdUsuarioCancel { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
