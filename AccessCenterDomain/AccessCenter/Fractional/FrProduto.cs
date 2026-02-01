namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrProduto : EntityBase
    {
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? Filial { get; set; } = 1;
        public virtual int? Empreendimento { get; set; }
        public virtual int? TipoCota { get; set; }
        public virtual int? TipoImovel { get; set; }
        public virtual int? ImovelVista { get; set; }
        public virtual string Tipo { get; set; } = "C";
        public virtual int? FrTipoProduto { get; set; }
        public virtual string TipoValor { get; set; } = "F";
        public virtual decimal? ValorFixo { get; set; }
        public virtual decimal? ValorBaseComissao { get; set; }
        public virtual string InformaCodigoContrato { get; set; } = "S";
        public virtual string NumeroContrato { get; set; }
        public virtual int? Sequencia { get; set; }
        public virtual int? QuantidadeDigitoSequencia { get; set; }
        public virtual decimal? QuantidadePontos { get; set; }
        public virtual decimal? QuantideDiasCanResNaoPagTax { get; set; }
        public virtual int? TempoUtilizacao { get; set; }
        public virtual decimal? PercentualIntegralizacaoUti { get; set; }
        public virtual string UtilizaTarifarioPontos { get; set; } = "N";
        public virtual int? FrTarifarioPonto { get; set; }
        public virtual string FormatoDataEntrega { get; set; } = "D";
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual string TipoReajusteContrato { get; set; } = "A";
        public virtual string SwVinculosTse { get; set; }
        public virtual int? ProdutoConfiguracaoFinanceira { get; set; }

        public virtual List<FrProdutoParticipante> ProdutosParticipantes { get; set; } = new List<FrProdutoParticipante>();
    }
}
