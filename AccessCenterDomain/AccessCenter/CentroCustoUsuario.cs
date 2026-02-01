namespace AccessCenterDomain.AccessCenter
{
    public class CentroCustoUsuario : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? CentroCusto { get; set; }
        public virtual int? Usuario { get; set; }
    }
}
