namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrEquipe : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual string Codigo { get; set; } //<TSE_IDCASAL=VALOR>
        public virtual string Nome { get; set; } //<TSE_IDCASAL=VALOR>

    }
}
