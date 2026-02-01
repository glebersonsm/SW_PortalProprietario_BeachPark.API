namespace AccessCenterDomain.AccessCenter
{
    public class ClienteCartaoCredito : EntityBase
    {
        public virtual int? Cliente { get; set; }
        public virtual string Numero { get; set; }
        public virtual int? VencimentoAno { get; set; }
        public virtual int? VencimentoMes { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string CodigoSeguranca { get; set; }
        public virtual string Status { get; set; } = "A";
        public virtual string CpfTitular { get; set; }
        public virtual int? Financeira { get; set; }
        public virtual string Bandeira { get; set; }
        public virtual string UltimosDigitos { get; set; }

    }
}
