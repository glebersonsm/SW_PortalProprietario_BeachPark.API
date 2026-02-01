namespace AccessCenterDomain.AccessCenter
{
    public class AtividadeProjeto : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }

    }
}
