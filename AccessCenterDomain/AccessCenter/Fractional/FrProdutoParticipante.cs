namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrProdutoParticipante : EntityBase
    {
        public virtual int FrProduto { get; set; }
        public virtual int GrupoEmpresa { get; set; } = 1;
        public virtual int Empresa { get; set; } = 724;
        public virtual int? Filial { get; set; } = 824;
        public virtual int? FilialParticipante { get; set; } = 824;
        public virtual decimal Valor { get; set; } = 0.00m;
        public virtual decimal ValorEntrada { get; set; } = 0.00m;
        public virtual string CancelamentoDevolverJurosRec { get; set; } = "N";
        public virtual decimal CancelamentoValorMulta { get; set; } = 0;
        public virtual decimal CancelamentoTaxaMulta { get; set; } = 0;
        public virtual int? QuantidadeMaximaDiasCanSemMul { get; set; } = 0;
        public virtual string LancarAlteradorValorJuro { get; set; } = "N";
        public virtual int? TipoContaReceberCreditoCan { get; set; } = 3283;
        public virtual int? TipoContaReceberCreditoRev { get; set; } = 3283;
        public virtual int? TipoContaReceberTaxaCan { get; set; } = 3284;
        public virtual int? TipoContaReceberValorIntAnt { get; set; } = 3283;
        public virtual int? TipoBaixaEncontroContasRev { get; set; } = 2025;
        public virtual int? OperacaoFinanceira { get; set; } = 2442;
        public virtual int? AplicacaoCaixa { get; set; } = 441;
        public virtual int? AlteradorValorJuroPar { get; set; } = 441;
        public virtual int? AlteradorValorAjusteArr { get; set; } = 1512;
        public virtual int? OperacaoFinanceiraMultaCan { get; set; } = 2443;
        public virtual string? TipoCalculoAntecipacao { get; set; } = "P";

    }
}
