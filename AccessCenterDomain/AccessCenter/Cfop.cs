namespace AccessCenterDomain.AccessCenter
{
    public class Cfop : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string AtivoImobilizado { get; set; } = "N";
        public virtual string LancaFinanceiro { get; set; } = "S";
        public virtual string Fomentar { get; set; } = "N";
        public virtual string LancaValorZeradoLivroIcms { get; set; } = "N";
        public virtual string EntraCalculoPercentualIseTri { get; set; } = "N";

    }
}
