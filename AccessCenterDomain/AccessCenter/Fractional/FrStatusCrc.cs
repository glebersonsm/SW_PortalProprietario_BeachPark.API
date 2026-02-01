namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrStatusCrc : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string IntegracaoId { get; set; }
        public virtual string Status { get; set; } = "A";

    }
}
