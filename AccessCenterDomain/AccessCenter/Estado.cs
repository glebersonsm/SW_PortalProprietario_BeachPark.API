namespace AccessCenterDomain.AccessCenter
{
    public class Estado : EntityBase
    {
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? Pais { get; set; }
        public virtual string CodigoIbge { get; set; }
        public virtual string Uf { get; set; }
    }
}
