namespace AccessCenterDomain.AccessCenter
{
    public class AlteradorValor : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string AlteradorValorAplicacao { get; set; }
        public virtual string Contabilizar { get; set; }
        public virtual string Categoria { get; set; }
        public virtual string Provisao { get; set; }
        public virtual string ExigeFilial { get; set; }
        public virtual string ExigeCentroCusto { get; set; }
        public virtual string Status { get; set; }

    }
}
