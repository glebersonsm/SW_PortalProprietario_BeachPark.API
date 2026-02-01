namespace AccessCenterDomain.AccessCenter
{
    public class ContaPagarParcelaAltVal : EntityBase
    {
        public virtual int? ContaPagarParcela { get; set; }
        public virtual int? AlteradorValor { get; set; }
        public virtual DateTime? Data { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual DateTime? DataOriginal { get; set; }
        public virtual DateTime? DataProvisao { get; set; }
        public virtual DateTime? DataProvisaoOriginal { get; set; }
        public virtual string Estornado { get; set; } = "N";
        public virtual int? ItemEstornado { get; set; }
        public virtual string LancamentoEstorno { get; set; } = "N";
        public virtual DateTime? DataEstorno { get; set; }
        public virtual decimal? ValorIntegralizado { get; set; }
        public virtual string Observacao { get; set; }
        public virtual int? TipoContaPagar { get; set; }
        public virtual int? FilialDestino { get; set; }
        public virtual int? CentroCustoDestino { get; set; }
        public virtual string Contabilizar { get; set; } = "N";

    }
}
