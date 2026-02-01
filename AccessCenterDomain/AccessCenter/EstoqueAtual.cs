namespace AccessCenterDomain.AccessCenter
{
    public class EstoqueAtual : EntityBase
    {
        public virtual int? ProdutoItemAlmoxarifado { get; set; }
        public virtual int? TipoEstoque { get; set; }
        public virtual decimal? PrecoCusto { get; set; }
        public virtual decimal? QuantidadeSaldo { get; set; }
        public virtual int? UnidadeMedida { get; set; }
        public virtual decimal? ValorSaldo { get; set; }
        public virtual int? Ordem { get; set; }

    }
}
