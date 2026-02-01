namespace AccessCenterDomain.AccessCenter
{
    public class TipoAlmoxarifado : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Status { get; set; }

    }
}
