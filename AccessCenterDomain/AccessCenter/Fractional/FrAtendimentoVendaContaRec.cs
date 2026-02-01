namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrAtendimentoVendaContaRec : EntityBase
    {
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual int? ContaReceber { get; set; }
        public virtual string Origem { get; set; }
        public virtual int? FrProdutoParticipante { get; set; }
        public virtual int? Empresa { get; set; } //Empresa onde está o conta a receber
    }
}
