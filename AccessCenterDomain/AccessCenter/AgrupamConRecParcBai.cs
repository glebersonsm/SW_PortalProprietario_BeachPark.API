namespace AccessCenterDomain.AccessCenter
{
    public class AgrupamConRecParcBai : EntityBase
    {
        public virtual int? ContaFinanceiraVariacao { get; set; }
        public virtual int? ContaFinanceiraSubVariacao { get; set; }
        public virtual int? TipoBaixa { get; set; }
        public virtual int? MovimentacaoFinanceira { get; set; }
        public virtual DateTime? DataBaixa { get; set; }
        public virtual DateTime? DataCredito { get; set; }
        public virtual decimal? ValorRecebido { get; set; }
        public virtual string ValorRecebidoDebitoCredito { get; set; } = "C";
        public virtual decimal? ValorMulta { get; set; }
        public virtual decimal? ValorJuro { get; set; }
        public virtual decimal? ValorDesconto { get; set; }
        public virtual decimal? ValorTaxaCobranca { get; set; }
        public virtual decimal? ValorAmortizado { get; set; }
        public virtual string ValorAmortizadoDebitoCredito { get; set; } = "C";
        public virtual string? NumeroDocumento { get; set; }
        public virtual string? Observacao { get; set; }
        public virtual string? HistoricoMovimentacao { get; set; }
        public virtual int? ContaReceberRenegociacao { get; set; }
        public virtual int? ContaPagarDestinoSaldo { get; set; }
        public virtual string? Contabilizar { get; set; } = "N";
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual string Estornado { get; set; } = "N";
        public virtual int? UsuarioEstorno { get; set; }
        public virtual DateTime? DataHoraEstorno { get; set; }
        public virtual int? AgrupamentoContaRecParBaiEst { get; set; }

        public virtual void Validate()
        {
            if (DataBaixa.HasValue && DataBaixa.GetValueOrDefault() < DateTime.Now.AddYears(-150))
                DataBaixa = DateTime.Now.AddDays(-10).Date;

            if (DataCredito.HasValue && DataCredito.GetValueOrDefault() < DateTime.Now.AddYears(-150))
                DataCredito = DateTime.Now.AddDays(-10).Date;

        }


    }
}
