namespace AccessCenterDomain.AccessCenter
{
    public class PDV : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? Caixa { get; set; }
        public virtual int? Loja { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? CentroCusto { get; set; }
        public virtual int? CaixaPrincipal { get; set; } // ContaFinanceiraVariacao
        public virtual int? CaixaPrincipalSubVariacao { get; set; } // ContaFinanceiraSubVariacao
        public virtual ContaFinanceiraVariacao CaixaObj { get; set; }
        public virtual ContaFinanceiraVariacao CaixaPrincipalObj { get; set; }
        public virtual ContaFinanceiraSubVariacao CaixaPrincipalSubVariacaoObj { get; set; }
        public virtual Filial FilialObject { get; set; }
    }
}
