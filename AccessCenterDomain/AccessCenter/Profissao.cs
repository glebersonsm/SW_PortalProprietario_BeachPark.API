namespace AccessCenterDomain.AccessCenter
{
    public class Profissao : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Status { get; set; } = "A";
        public virtual string QualificacaoAutomaticaQua { get; set; } = "N";

    }
}
