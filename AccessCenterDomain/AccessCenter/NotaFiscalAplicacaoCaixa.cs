namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalAplicacaoCaixa : EntityBase
    {
        public virtual int? NotaFiscal { get; set; }
        public virtual int? AplicacaoCaixa { get; set; }
        public virtual decimal? Valor { get; set; }
    }
}
