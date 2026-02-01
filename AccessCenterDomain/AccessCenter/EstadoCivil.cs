namespace AccessCenterDomain.AccessCenter
{
    public class EstadoCivil : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        //public virtual int? Estado { get; set; }
        public virtual string Categoria { get; set; }
        public virtual string PossuiConjuge { get; set; }
    }
}
