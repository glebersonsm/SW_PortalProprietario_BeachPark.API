namespace AccessCenterDomain.AccessCenter
{
    public class PessoaEndereco : EntityBase
    {
        public virtual int? TipoEndereco { get; set; }
        public virtual string? Logradouro { get; set; }
        public virtual string? Numero { get; set; }
        public virtual string? Bairro { get; set; }
        public virtual string? Cep { get; set; }
        public virtual string? Complemento { get; set; }
        public virtual string? Preferencial { get; set; } = "S";
        public virtual string? Cobranca { get; set; } = "N";
        public virtual string? Entrega { get; set; } = "N";
        public virtual int? Cidade { get; set; }
        public virtual int? Pessoa { get; set; }
        public virtual string? Estrangeiro { get; set; } = "N";
        public virtual string? EnderecoCorreto { get; set; } = "S";

    }
}
