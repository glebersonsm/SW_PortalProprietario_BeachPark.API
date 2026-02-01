namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalContaPagarRetImp : EntityBase
    {
        public virtual int? NotaFiscal { get; set; }
        public virtual int? ContaPagar { get; set; }
        public virtual string TipoImpostoRetido { get; set; }

    }
}
