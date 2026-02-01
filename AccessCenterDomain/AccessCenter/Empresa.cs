namespace AccessCenterDomain.AccessCenter
{
    public class Empresa : EntityBase
    {
        public virtual string? Codigo { get; set; }
        public virtual int? Pessoa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string? Status { get; set; }

    }
}
