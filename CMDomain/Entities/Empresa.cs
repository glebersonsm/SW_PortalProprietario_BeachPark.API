namespace CMDomain.Entities
{
    public class Empresa : CMEntityBase
    {
        public virtual int? IdEmpresa { get; set; }
        public virtual string? NomeEmpresa { get; set; }
    }
}
