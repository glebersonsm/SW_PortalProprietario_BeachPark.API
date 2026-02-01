namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrSala : EntityBase
    {
        public virtual int? Filial { get; set; } = 1;
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Status { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
    }
}
