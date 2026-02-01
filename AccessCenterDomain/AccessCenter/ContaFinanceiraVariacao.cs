namespace AccessCenterDomain.AccessCenter
{
    public class ContaFinanceiraVariacao : EntityBase
    {
        public virtual int? ContaFinanceira { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? ContaFinanceiraVariacaoTipo { get; set; }
        public virtual string Principal { get; set; }
        public virtual string ConvenioIntegracaoContaPagar { get; set; }
        public virtual string ExigeSubVariacao { get; set; }
        public virtual string UtilizaSubVariacaoPorUsuario { get; set; }
        public virtual string Status { get; set; }

    }

}
