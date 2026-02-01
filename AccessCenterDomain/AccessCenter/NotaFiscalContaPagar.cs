namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalContaPagar : EntityBase
    {
        public virtual int? NotaFiscal { get; set; }
        public virtual int? ContaPagar { get; set; }
        public virtual string LancaContasAPagar { get; set; } = "S";
        public virtual string LancamentoOriginal { get; set; } = "S";
        public virtual int? AlteradorValorPis { get; set; }
        public virtual int? AlteradorValorCofins { get; set; }
        public virtual int? AlteradorValorInss { get; set; }
        public virtual int? AlteradorValorIss { get; set; }
        public virtual int? AlteradorValorIrrf { get; set; }
        public virtual int? AlteradorValorContribuicao { get; set; }

    }
}
