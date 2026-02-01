namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrComissaoLancamento : EntityBase
    {
        public virtual int? FrAtendimento { get; set; }
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual int? FrAtendimentoFuncao { get; set; }
        public virtual int? FrComissaoCabecalho { get; set; }
        public virtual int? FrComissaoFechamento { get; set; }

        public virtual string Previsao { get; set; } = "S";
        public virtual string Bloqueada { get; set; } = "N";
        public virtual string Cancelada { get; set; } = "N";
        public virtual string LancamentoManual { get; set; } = "N";
        public virtual DateTime? Data { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual string MemoriaCalculo { get; set; }
        public virtual string MemoriaFechamento { get; set; }
        public virtual int? ContaReceberParcela { get; set; }

    }
}
