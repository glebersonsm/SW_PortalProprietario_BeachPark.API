namespace AccessCenterDomain.AccessCenter
{
    public class MovimentacaoProItemAlm : EntityBase
    {
        public virtual int? TipoEstoque { get; set; }
        public virtual int? ProdutoItemAlmoxarifado { get; set; }
        public virtual DateTime? Data { get; set; }
        public virtual int? TipoMovimentacao { get; set; }
        public virtual string Descricao { get; set; }
        public virtual decimal? QuantidadeMovimento { get; set; }
        public virtual decimal? QuantidadeMovimentoPeca { get; set; }
        public virtual int? UnidadeMedidaMovimento { get; set; }
        public virtual int? UnidadeConvertida { get; set; }
        public virtual decimal? ValorMovimento { get; set; }
        public virtual string EntradaSaida { get; set; }
        public virtual DateTime? DataDocumentoOrigem { get; set; }
        public virtual string TipoValorMovimentacao { get; set; }
        public virtual int? RequisicaoAlmAteProItem { get; set; }
        public virtual int? AjusteEstoqueProItemAlm { get; set; }
        public virtual int? NotaFiscalProItemAlm { get; set; }
        public virtual int? NotaFiscalProItemAlmExc { get; set; }
        public virtual int? MovProItemAlmEstornada { get; set; }
        public virtual int? MovProItemAlmEstorno { get; set; }
        public virtual decimal? QuantidadeSaldo { get; set; }
        public virtual decimal? ValorSaldo { get; set; }
        public virtual decimal? PrecoCusto { get; set; }
        public virtual int? Ordem { get; set; }


    }
}
