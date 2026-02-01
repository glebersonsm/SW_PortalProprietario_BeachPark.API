namespace EsolutionPortalDomain.Portal
{
    public class Estado : EntityBasePortal
    {
        public virtual string? Nome { get; set; }
        public virtual string? UF { get; set; }
        public virtual int? Pais { get; set; }
        public virtual string? CodigoIbge { get; set; }

    }
}
