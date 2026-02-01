namespace EsolutionPortalDomain.Portal
{
    public class PessoaTelefone : EntityBasePortal
    {
        public virtual int? Pessoa { get; set; }
        public virtual string? Tipo { get; set; }
        public virtual string? Numero { get; set; }
        public virtual string? Preferencial { get; set; } = "N";

    }
}
