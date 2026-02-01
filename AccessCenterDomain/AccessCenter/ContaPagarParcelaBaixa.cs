namespace AccessCenterDomain.AccessCenter
{
    public class ContaPagarParcelaBaixa : EntityBaseEsol
    {
        public virtual int? ContaPagarParcela { get; set; }
        public virtual DateTime? DataBaixa { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? Multa { get; set; }
        public virtual decimal? Juro { get; set; }
        public virtual decimal? Desconto { get; set; }
        public virtual decimal? TaxaCobranca { get; set; }
        public virtual decimal? ValorAmortizado { get; set; }
        public virtual int? AgrupamConPagParcBai { get; set; }
        public virtual int? TipoContaPagar { get; set; }

    }
}
