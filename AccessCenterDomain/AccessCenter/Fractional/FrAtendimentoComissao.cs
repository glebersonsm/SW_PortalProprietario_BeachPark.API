namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrAtendimentoComissao : EntityBase
    {
        public virtual string Origem { get; set; }
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual int? FrAtendimentoFuncao { get; set; }
        public virtual int? ContaReceberParcela { get; set; }
        public virtual int? FrRegraComissao { get; set; }
        public virtual decimal ValorBaseCalculo { get; set; }
        public virtual decimal ValorFundoCobranca { get; set; }
        public virtual decimal Valor { get; set; }
        public virtual DateTime? DataPrevista { get; set; }
        public virtual string PagamentoVinculadoBaixa { get; set; }
        public virtual string Status { get; set; }
        public virtual DateTime? DataHoraStatus { get; set; }
        public virtual int? FrAtendimentoComissaoOrigem { get; set; }
        public virtual int? FrComissaoFrUsuarioLancamento { get; set; }

    }
}
