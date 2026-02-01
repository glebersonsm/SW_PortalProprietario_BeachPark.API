namespace EsolutionPortalDomain.Portal
{
    public class PessoaEndereco : EntityBasePortal
    {
        public virtual int? Pessoa { get; set; }
        public virtual int? TipoEndereco { get; set; }
        public virtual string? Logradouro { get; set; }
        public virtual string? Bairro { get; set; }
        public virtual string? Numero { get; set; }
        public virtual string? Cep { get; set; }
        public virtual string? Complemento { get; set; }
        public virtual int? Cidade { get; set; }
        public virtual string? Preferencial { get; set; } = "N";
        public virtual string? Cobranca { get; set; } = "N";

    }
}
