namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoTipoAlmoxarifado : EntityBase
    {
        public virtual int? Produto { get; set; }
        public virtual int? TipoAlmoxarifado { get; set; }

    }
}
