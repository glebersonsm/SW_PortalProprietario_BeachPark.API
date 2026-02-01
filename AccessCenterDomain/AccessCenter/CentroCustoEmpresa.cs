namespace AccessCenterDomain.AccessCenter
{
    public class CentroCustoEmpresa : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? CentroCusto { get; set; }
        public virtual int? Empresa { get; set; }

    }
}
