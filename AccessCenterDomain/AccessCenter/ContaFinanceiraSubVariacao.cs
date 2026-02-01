namespace AccessCenterDomain.AccessCenter
{
    public class ContaFinanceiraSubVariacao : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? ContaContabil { get; set; }
        public virtual int? Usuario { get; set; }
        public virtual string PermiteConsulta { get; set; }
        public virtual string PermiteLancamento { get; set; }
        public virtual int? ContaFinanceiraVariacao { get; set; }
    }

}
