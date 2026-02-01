namespace AccessCenterDomain.AccessCenter
{
    public class TipoEstoque : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Principal { get; set; } = "N";

    }
}
