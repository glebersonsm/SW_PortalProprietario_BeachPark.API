namespace AccessCenterDomain.AccessCenter
{
    public class CentroCustoFilial : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual int? CentroCusto { get; set; }
        public virtual string StatusLancamento { get; set; } = "A";
        public virtual string StatusConsulta { get; set; } = "A";

    }
}
