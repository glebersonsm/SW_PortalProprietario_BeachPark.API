namespace EsolutionPortalDomain.Portal
{
    public class Cidade : EntityBasePortal
    {
        public virtual string? Nome { get; set; }
        public virtual string? UF { get; set; }
        public virtual int? Pais { get; set; }
        public virtual string? CodigoIbge { get; set; }
        public virtual int? Estado { get; set; }
        public virtual string? CodigoMunicipioNFSe { get; set; }

    }
}
