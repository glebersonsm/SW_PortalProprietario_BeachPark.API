namespace AccessCenterDomain.AccessCenter
{
    public class Cidade : EntityBase
    {
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? Estado { get; set; }
        public virtual string CodigoIbge { get; set; }
    }
}
