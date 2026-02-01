namespace AccessCenterDomain.AccessCenter
{
    public class Sequencia : EntityBase
    {
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual Int64? Inicio { get; set; }
        public virtual Int64? Fim { get; set; }
        public virtual Int64? Proximo { get; set; }
    }
}
