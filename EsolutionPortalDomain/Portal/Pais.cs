namespace EsolutionPortalDomain.Portal
{
    public class Pais : EntityBasePortal
    {
        public virtual string? Nome { get; set; }
        public virtual int? DDI { get; set; }
        public virtual string? CodigoPais { get; set; }
        public virtual string? CodigoFNRH { get; set; }

    }
}
