namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoUnidadeMedida : EntityBase
    {
        public virtual Produto? Produto { get; set; }
        public virtual string CodigoBarras { get; set; }
        public virtual UnidadeMedida? UnidadeMedida { get; set; }
        public virtual string UnidadeVenda { get; set; } = "N";
        public virtual string UnidadeRelatorio { get; set; } = "N";
        public virtual string UnidadeEntradaNotaFiscal { get; set; } = "N";
        public virtual string UnidadeMedidaPrincipal { get; set; } = "N";
        public virtual decimal? PesoLiquido { get; set; }
        public virtual decimal? PesoBruto { get; set; }

    }
}
