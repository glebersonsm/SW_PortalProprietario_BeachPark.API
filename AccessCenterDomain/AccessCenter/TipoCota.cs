namespace AccessCenterDomain.AccessCenter
{
    public class TipoCota : EntityBase
    {
        public virtual int? Empreendimento { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string LiberadoVenda { get; set; } = "S";
        public virtual int? QuantidadeSemana { get; set; }

    }
}
