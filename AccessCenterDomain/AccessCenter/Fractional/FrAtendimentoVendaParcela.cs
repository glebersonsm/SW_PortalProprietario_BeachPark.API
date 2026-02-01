namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrAtendimentoVendaParcela : EntityBase
    {
        public virtual int? FilialParticipante { get; set; }
        public virtual int? QuantidadeParcela { get; set; }
        public virtual decimal ValorTotal { get; set; }
        public virtual decimal ValorTotalAmortizado { get; set; }
        public virtual int? TipoContaReceber { get; set; }
        public virtual int? TipoParcela { get; set; }
        public virtual DateTime? PrimeiroVencimento { get; set; }
        public virtual string TipoBaseCalculoValor { get; set; } = "A";
        public virtual decimal ValorParcela { get; set; }
        public virtual int? ClienteCartaoCredito { get; set; }
        public virtual int? FrAtendimentoVenda { get; set; }
    }
}
