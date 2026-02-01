namespace AccessCenterDomain.AccessCenter
{
    public class TipoContaPagar : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Status { get; set; } = "A";
        public virtual string Adiantamento { get; set; } = "N";
        public virtual string lancaMovFinanceiraPagto { get; set; } = "N";
        public virtual string AFaturar { get; set; } = "N";
        public virtual string PermitirLactoDebito { get; set; } = "N";
        public virtual string TipoAgrupamento { get; set; } = "C";
        public virtual string ApareceExtratoContaPagar { get; set; } = "S";
        public virtual string RestringeTipoCliente { get; set; } = "N";
        public virtual string LancaBloqueado { get; set; } = "N";
        public virtual string RetemImpostoRenda { get; set; } = "N";
        public virtual string PermitirLancamentoManual { get; set; } = "S";
        public virtual string EncontroContasAutomatico { get; set; } = "N";
        public virtual string RestringeLiberacaoPorUsuario { get; set; } = "N";

    }
}
