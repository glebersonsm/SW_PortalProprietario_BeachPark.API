namespace AccessCenterDomain.AccessCenter
{
    public class MovimentacaoFinanceiraAplCai : EntityBase
    {
        public virtual int? MovimentacaoFinanceira { get; set; }
        public virtual int? AplicacaoCaixa { get; set; }
        public virtual decimal? Valor { get; set; }

    }
}
