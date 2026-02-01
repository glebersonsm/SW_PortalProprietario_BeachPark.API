namespace AccessCenterDomain.AccessCenter
{
    public class CategoriaCota : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual string? Codigo { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? NomePesquisa { get; set; }
        public virtual int? TipoContaReceber { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string? Pool { get; set; } = "N";
        public virtual string? CategoriaForaPool { get; set; } = "N";

    }
}
