namespace AccessCenterDomain.AccessCenter
{
    public class ContaPagarParcela : EntityBase
    {
        public virtual int? ContaPagar { get; set; }
        public virtual string DocumentoOutros { get; set; }
        public virtual int? Parcela { get; set; }
        public virtual DateTime? VencimentoOriginal { get; set; }
        public virtual DateTime? Vencimento { get; set; }
        public virtual string NomeFavorecido { get; set; }
        public virtual int? TipoContaPagarOriginal { get; set; }
        public virtual int? TipoContaPagar { get; set; }
        public virtual int? AplicacaoCaixa { get; set; }
        public virtual decimal? ValorOriginal { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? ValorDesconto { get; set; }
        public virtual decimal? ValorJuros { get; set; }
        public virtual decimal? ValorMulta { get; set; }
        public virtual decimal? ValorTaxaCobranca { get; set; }
        public virtual decimal? SaldoPendente { get; set; }
        public virtual decimal? ValorBaixado { get; set; }
        public virtual decimal? ValorAmortizado { get; set; }
        public virtual string Status { get; set; }
        public virtual int? UsuarioBaixa { get; set; }
        public virtual DateTime? DataHoraBaixa { get; set; }
        public virtual decimal? PercentualMulta { get; set; }
        public virtual decimal? PercentualJuroDiario { get; set; }
        public virtual decimal? PercentualDescPagAntecipado { get; set; }
        public virtual int? DiasDescontoPagAntecipado { get; set; }
        public virtual int? ContaPagarLote { get; set; }
        public virtual int? ContaPagarRemessa { get; set; }
        public virtual string BoletoCodigoBarras { get; set; }
        public virtual string BoletoLinhaDigitavel { get; set; }
        public virtual string EmitirOrdemProtesto { get; set; } = "N";
        public virtual int? DiasProtesto { get; set; }
        public virtual int? BancoTransferencia { get; set; }
        public virtual string AgenciaTransferencia { get; set; }
        public virtual string DigitoAgenciaTransferencia { get; set; }
        public virtual string ContaTransferencia { get; set; }
        public virtual string DigitoContaTransferencia { get; set; }
        public virtual string VariacaoTransferencia { get; set; }
        public virtual string DocumentoOrdemPagamento { get; set; }
        public virtual string UtilizaComposicaoContabilLan { get; set; } = "N";
        public virtual int? ComposicaoContabilLancamento { get; set; }
        public virtual string UtilizaComposicaoContabilBaixa { get; set; } = "N";
        public virtual int? ComposicaoContabilBaixa { get; set; }
        public virtual string BloqueadoParaBaixa { get; set; } = "N";
        public virtual string ObservacaoBloqueio { get; set; }
        public virtual int? LocalPagamento { get; set; }
        public virtual int? CidadeAgencia { get; set; }
        public virtual string InformarFavorecido { get; set; } = "N";
        public virtual string FavorecidoPessoaTipo { get; set; } = "F";
        public virtual string CpfFavorecido { get; set; }
        public virtual string OrigemContaBancaria { get; set; }
        public virtual string TipoContaTransferencia { get; set; }
        public virtual DateTime? PrevisaoPagamento { get; set; }
        public virtual int? ModalidadePagamento { get; set; }
        public virtual int? ClienteContaBancaria { get; set; }
        public virtual int? IntegracaoBancariaConPagReg { get; set; }
        public virtual string StatusIntegracaoBancaria { get; set; } = "A";
        public virtual int? IdentificadorRemessaPagamento { get; set; }
        public virtual string NossoNumero { get; set; }
        public virtual int? IntegracaoBancariaRetDDAIte { get; set; }
        public virtual string OrdemPagamentoAgenciaDigito { get; set; }
        public virtual int? OrdemPagamentoBanco { get; set; }
        public virtual string OrdemPagamentoAgencia { get; set; }
        public virtual string Mabu_BloqueioAutomatico { get; set; }
        public virtual decimal? ValorDescontoParaBaixa { get; set; }
        public virtual decimal? ValorJurosParaBaixa { get; set; }
        public virtual decimal? ValorMultaParaBaixa { get; set; }
        public virtual decimal? ValorTaxaCobrancaParaBaixa { get; set; }


    }
}
