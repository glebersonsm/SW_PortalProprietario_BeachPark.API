namespace AccessCenterDomain.AccessCenter
{
    public class Almoxarifado : EntityBase
    {
        public virtual Filial? Filial { get; set; }
        public virtual Empresa? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string? Ativo { get; set; } = "S";
        public virtual string? Codigo { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? NomeAbreviado { get; set; }
        public virtual TipoAlmoxarifado? TipoAlmoxarifado { get; set; }
    }
}
