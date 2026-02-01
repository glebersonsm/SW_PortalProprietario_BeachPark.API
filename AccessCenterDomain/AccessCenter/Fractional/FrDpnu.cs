namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrDpnu : EntityBase
    {
        public virtual int? Filial { get; set; } = 1;
        public virtual int? Empresa { get; set; } = 1;
        public virtual string CodigoAgrupamentoLancamento { get; set; }
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual DateTime? DataLancamento { get; set; }

    }
}
