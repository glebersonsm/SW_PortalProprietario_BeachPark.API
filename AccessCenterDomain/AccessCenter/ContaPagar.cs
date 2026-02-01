namespace AccessCenterDomain.AccessCenter
{
    public class ContaPagar : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual string Documento { get; set; }
        public virtual string Observacao { get; set; }
        public virtual DateTime? Emissao { get; set; }
        public virtual int? Cliente { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? ValorOriginal { get; set; }
        public virtual int? OperacaoFinanceira { get; set; } = 1;
        public virtual DateTime? DataMovimento { get; set; }
        public virtual int? EmprestimoReceber { get; set; }
        public virtual int? Contrato { get; set; }
        public virtual string OrdemPagamentoAgenciaDigito { get; set; }
        public virtual int? ContaPagarOrigem { get; set; }

    }
}
