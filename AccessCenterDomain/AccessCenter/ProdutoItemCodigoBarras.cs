namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoItemCodigoBarras : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual ProdutoItem? ProdutoItem { get; set; }
        public virtual string? CodigoBarrasAnterior { get; set; }
        public virtual string? CodigoBarrasAtual { get; set; }

    }
}
