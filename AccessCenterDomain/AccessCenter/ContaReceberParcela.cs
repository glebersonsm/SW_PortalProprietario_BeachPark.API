namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcela : EntityBase
    {
        public virtual int? ContaReceber { get; set; }
        public virtual string? Documento { get; set; }
        public virtual int? Parcela { get; set; }
        public virtual int? ParcelaCartao { get; set; }
        public virtual DateTime? VencimentoOriginal { get; set; }
        public virtual DateTime? Vencimento { get; set; }
        public virtual int? TipoContaReceberOriginal { get; set; }
        public virtual int? TipoContaReceber { get; set; }
        public virtual int? TipoParcela { get; set; }
        public virtual int? AplicacaoCaixa { get; set; }
        public virtual int? QuantidadeParcelasCartao { get; set; }
        public virtual decimal? ValorOriginal { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? ValorDesconto { get; set; }
        public virtual decimal? ValorJuros { get; set; }
        public virtual decimal? ValorMulta { get; set; }
        public virtual decimal? ValorTaxaCobranca { get; set; }
        public virtual decimal? SaldoPendente { get; set; }
        public virtual decimal? SaldoPendenteFpp { get; set; } = 0;
        public virtual decimal? ValorBaixado { get; set; }
        public virtual decimal? ValorAmortizado { get; set; }
        public virtual string? Status { get; set; }
        public virtual string? Nsu { get; set; }
        public virtual int? UsuarioBaixa { get; set; }
        public virtual DateTime? DataHoraBaixa { get; set; }
        public virtual DateTime? DataCobranca { get; set; }
        public virtual DateTime? Devolucao1Data { get; set; }
        public virtual DateTime? Devolucao2Data { get; set; }
        public virtual string? IntegracaoId { get; set; }
        public virtual int? ClienteCartaoCredito { get; set; }
        public virtual string? NumeroCartaoCriptografado { get; set; }
        public virtual string? CartaoCreditoRecorrenteStatus { get; set; } = null;
        public virtual string? DocumentoFinanceira { get; set; }
        public virtual string? DocumentoOutros { get; set; }
        public virtual string? AutorizacaoCartao { get; set; }
        public virtual string? FinanceiraAutorizacao { get; set; }
        public virtual string? NumeroTransacaoCartao { get; set; }
        public virtual string? CodigoAutorizacaoCartao { get; set; }
        public virtual int? Financeira { get; set; }
        public virtual string UtilizaComposicaoContabilBaixa { get; set; } = "N";
        public virtual string UtilizaComposicaoContabilLan { get; set; } = "N";
    }
}
