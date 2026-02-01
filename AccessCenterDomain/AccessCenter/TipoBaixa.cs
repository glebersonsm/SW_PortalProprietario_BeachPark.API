namespace AccessCenterDomain.AccessCenter
{
    public class TipoBaixa : EntityBase
    {

        public virtual int? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string LancaMovimentacaoFinanceira { get; set; }
        public virtual string TipoBaixaAplicacao { get; set; }
        public virtual int? OperacaoMovimentacaoFinanceira { get; set; }
        public virtual string Contabilizar { get; set; }
        public virtual string ConciliacaoRecebiveis { get; set; }
        public virtual string LocalContabilizacao { get; set; }
        public virtual int? ContabilizacaoRegra { get; set; }
        public virtual string Renegociar { get; set; }
        public virtual string Transferencia { get; set; }
        public virtual string TipoTransferencia { get; set; }
        public virtual string PermiteLancamentoManual { get; set; }
        public virtual string DepositoNaoIdentificado { get; set; }
    }
}
