namespace AccessCenterDomain.AccessCenter
{
    public class TipoContaReceber : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Status { get; set; } = "A";
        public virtual string Adiantamento { get; set; } = "N";
        public virtual string Finalizadora { get; set; } = "O";
        public virtual string BaixaAutomatica { get; set; } = "N";
        public virtual string lancaMovFinanceiraRecbto { get; set; } = "S";
        public virtual string AFaturar { get; set; } = "N";
        public virtual string PermitirLactoDebito { get; set; } = "N";
        public virtual string PermitirEncontroContas { get; set; } = "N";
        public virtual string PermitirCondicional { get; set; } = "N";
        public virtual int FinQtdeDiasPagto { get; set; }
        public virtual decimal FinanceiraPercTaxa { get; set; }
        public virtual decimal? FinanceiraPercTaxaParc { get; set; }
        public virtual string Irrestrito { get; set; } = "N";
        public virtual string BaixaLimite { get; set; } = "N";
        public virtual string ExigeCartao { get; set; } = "N";
        public virtual string PermiteAlterarVencimento { get; set; } = "N";
        public virtual string PermiteVenderFazendaInativa { get; set; } = "N";
        public virtual string MomentoBaixaLimite { get; set; } = "E";
        public virtual string BaixarLimiteTodosDocAbe { get; set; } = "N";
        public virtual string UtilizadoFrete { get; set; } = "N";
        public virtual string PermitirLancamentoManual { get; set; } = "S";
        public virtual string BloqueiaPorFaltaDePagamento { get; set; } = "N";
        public virtual string ExigePdv { get; set; } = "N";
        public virtual string ObrigatorioEncontroContas { get; set; } = "N";
        public virtual string EmiteDuplicata { get; set; } = "N";
        public virtual string InformaTipoDeClientePermitido { get; set; } = "N";
        public virtual string PermiteLancarEmDevolucao { get; set; } = "N";
        public virtual string PermiteVencimentoSemanalRateio { get; set; } = "N";
        public virtual string VerificaLimite { get; set; } = "N";
        public virtual string LocalBaixa { get; set; } = "M";
        public virtual string PermiteBaixar { get; set; } = "S";
        public virtual string BaixaLimiteVenda { get; set; } = "N";
        public virtual string BaixaLimiteCondicional { get; set; } = "N";
        public virtual string CondicaoCalculoJuros { get; set; } = "A";
        public virtual string ParticipaConvenio { get; set; } = "N";
        public virtual string EncontroContasAutomatico { get; set; } = "N";
        public virtual string PermiteNumParMaiQueVenCar { get; set; } = "N";
        public virtual string ApareceExtratoFinanceiro { get; set; } = "S";
        public virtual string PermiteLinkPagamento { get; set; } = "N";
        public virtual int? AplicacaoCaixa { get; set; }

    }
}
