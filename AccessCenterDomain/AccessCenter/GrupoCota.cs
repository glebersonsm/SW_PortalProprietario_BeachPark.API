namespace AccessCenterDomain.AccessCenter
{
    public class GrupoCota : EntityBase
    {
        public virtual int? Empreendimento { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string LiberadoVenda { get; set; } = "S";

    }
}
