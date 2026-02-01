namespace AccessCenterDomain.AccessCenter
{
    public class MovimentacaoFinanceira : EntityBase
    {
        public virtual int? ContaFinanceiraVariacao { get; set; }
        public virtual int? ContaFinanceiraSubVariacao { get; set; }
        public virtual int? MovimentacaoFinanceiraOrigem { get; set; }
        public virtual int? OperacaoMovFin { get; set; }
        public virtual DateTime? Data { get; set; }
        public virtual Int64? Sequencia { get; set; }
        public virtual string? Documento { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual string? ValorDebitoCredito { get; set; } = "C";
        public virtual decimal? Saldo { get; set; } = 0;
        public virtual string? SaldoDebitoCredito { get; set; } = "C";
        public virtual string? Historico { get; set; }
        public virtual string? HistoricoContabil { get; set; }
        public virtual string? Observacao { get; set; }
        public virtual string? TipoDocumento { get; set; } = "D";
        public virtual int? AgrupamConRecParcBai { get; set; }
        public virtual string? ChequePredatado { get; set; } = "N";
        public virtual string? ChequeAvulso { get; set; } = "N";
        public virtual string? LancamentoAutomatico { get; set; } = "S";
        public virtual int? ChequeNumero { get; set; } = 0;
        public virtual string? ChequeStatus { get; set; } = "N";


    }
}
