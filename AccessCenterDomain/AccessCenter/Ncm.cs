namespace AccessCenterDomain.AccessCenter
{
    public class Ncm : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual Cest? Cest { get; set; }

    }
}
