namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoItem : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual Produto? Produto { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string CodigoBarras { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string NomeProduto { get; set; }
        public virtual string NomeProdutoPesquisa { get; set; }
        public virtual decimal? PrecoVenda { get; set; }
        public virtual decimal? PrecoPauta { get; set; }
        public virtual Int64? CodigoBalanca { get; set; }
        public virtual int? Sequencia { get; set; }
        public virtual string TipoSubsidio { get; set; }
        public virtual decimal? ValorSubsidio { get; set; }

    }
}
