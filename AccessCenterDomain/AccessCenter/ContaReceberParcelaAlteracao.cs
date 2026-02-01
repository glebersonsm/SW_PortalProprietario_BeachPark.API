namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaAlteracao : EntityBase
    {
        public virtual int? ContaReceberParcela { get; set; }
        public virtual int? TipoContaReceber { get; set; }
        public virtual int? TipoContaReceberAnterior { get; set; }
        public virtual int? Cliente { get; set; }
        public virtual int? ClienteAnterior { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual DateTime? Data { get; set; }
        public virtual string? Estornado { get; set; } = "N";
        public virtual DateTime? VencimentoAnterior { get; set; }
        public virtual DateTime? NovoVencimento { get; set; }
    }
}
