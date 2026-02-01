namespace AccessCenterDomain.AccessCenter
{
    public class CentroCusto : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string StatusLancamento { get; set; } = "A";
        public virtual string StatusConsulta { get; set; } = "A";
        public virtual string AnaliticoSintetico { get; set; } = "A";
        public virtual string RestringeFilial { get; set; } = "N";
        public virtual string EmailResponsavel { get; set; }
        public virtual int? Parent { get; set; }
        public virtual int? CentroResultado { get; set; }
        public virtual string RestringeUsuario { get; set; } = "N";
    }
}
