namespace AccessCenterDomain.AccessCenter
{
    public class UnidadeMedidaSigla : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual UnidadeMedida? UnidadeMedida { get; set; }
        public virtual string Sigla { get; set; }
        public virtual string Padrao { get; set; } = "N";

    }
}
