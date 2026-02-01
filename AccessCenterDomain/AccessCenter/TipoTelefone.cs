namespace AccessCenterDomain.AccessCenter
{
    public class TipoTelefone : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Tipo { get; set; } //F = Fixo, M = Móvel
    }
}
