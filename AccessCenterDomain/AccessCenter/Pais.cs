namespace AccessCenterDomain.AccessCenter
{
    public class Pais : EntityBase
    {
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string DDI { get; set; }
        public virtual string CodigoPais { get; set; }
    }
}
