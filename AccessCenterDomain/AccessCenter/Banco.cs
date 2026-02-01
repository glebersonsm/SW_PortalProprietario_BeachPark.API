namespace AccessCenterDomain.AccessCenter
{
    public class Banco : EntityBase
    {
        public virtual string? Codigo { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? NomePesquisa { get; set; }

    }
}
