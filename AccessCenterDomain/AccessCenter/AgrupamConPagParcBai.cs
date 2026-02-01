namespace AccessCenterDomain.AccessCenter
{
    public class AgrupamConPagParcBai : EntityBaseEsol
    {
        public virtual int? ContaFinanceiraVariacao { get; set; }
        public virtual int? ContaFinanceiraSubVariacao { get; set; }
        public virtual int? TipoBaixa { get; set; }
        public virtual string? TipoPagamento { get; set; } = "B";
        public virtual int? MovimentacaoFinanceira { get; set; }
        public virtual DateTime? DataBaixa { get; set; }
        public virtual DateTime? DataCredito { get; set; }
        public virtual decimal? ValorBaixado { get; set; }
        public virtual string? ValorBaixadoDebitoCredito { get; set; } = "C";
        public virtual decimal? ValorMulta { get; set; }
        public virtual decimal? ValorJuro { get; set; }
        public virtual decimal? ValorDesconto { get; set; }
        public virtual decimal? ValorTaxaCobranca { get; set; }
        public virtual decimal? ValorAmortizado { get; set; }
        public virtual string? ValorAmortizadoDebitoCredito { get; set; } = "C";
        public virtual string? NumeroDocumento { get; set; }
        public virtual string? Observacao { get; set; }
        public virtual string? HistoricoContabil { get; set; }
        public virtual int? ContaPagarRenegociacao { get; set; }
        public virtual int? ContaReceberDestinoSaldo { get; set; }
        public virtual string? Contabilizar { get; set; } = "N";
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual string? Estornado { get; set; } = "N";
        public virtual int? UsuarioEstorno { get; set; }
        public virtual DateTime? DataHoraEstorno { get; set; }
        public virtual int? AgrupamentoContaPagParBaiEst { get; set; }


    }
}
