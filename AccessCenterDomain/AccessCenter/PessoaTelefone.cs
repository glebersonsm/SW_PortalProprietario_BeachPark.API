namespace AccessCenterDomain.AccessCenter
{
    public class PessoaTelefone : EntityBase
    {
        public virtual string Numero { get; set; }
        public virtual string Preferencial { get; set; } = "N";
        public virtual string Estrangeiro { get; set; } = "N";
        public virtual string RecebeSms { get; set; } = "N";
        public virtual int? Pais { get; set; }
        public virtual int? TipoTelefone { get; set; }
        public virtual int? Pessoa { get; set; }
    }
}
