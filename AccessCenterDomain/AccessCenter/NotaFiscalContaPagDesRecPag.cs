namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalContaPagDesRecPag : EntityBase
    {
        public virtual int? NotaFiscal { get; set; }
        public virtual int? ContaPagar { get; set; }
        public virtual int? Filial { get; set; }
        public virtual int? DestinoContabil { get; set; }
        public virtual int? CentroCusto { get; set; }
        public virtual int? AtividadeProjeto { get; set; }
        public virtual string Historico { get; set; }
        public virtual string HistoricoContabil { get; set; }
        public virtual string Observacao { get; set; }
        public virtual decimal? Valor { get; set; }


    }
}
