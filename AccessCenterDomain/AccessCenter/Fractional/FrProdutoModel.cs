namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrProdutoModel : EntityBaseEsol
    {
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? NomePesquisa { get; set; }
        public int? Filial { get; set; } = 1;
        public int? Empreendimento { get; set; }
        public int? TipoCota { get; set; }
        public int? TipoImovel { get; set; }
        public string? TipoImovelCodigo { get; set; }
        public string? TipoImovelNome { get; set; }
        public int? ImovelVista { get; set; }
        public string? Tipo { get; set; } = "C";
        public int? FrTipoProduto { get; set; }
        public string? TipoValor { get; set; } = "F";
        public decimal? ValorFixo { get; set; }
        public decimal? ValorBaseComissao { get; set; }
        public string? InformaCodigoContrato { get; set; } = "S";
        public string? NumeroContrato { get; set; }
        public int? Sequencia { get; set; }
        public int? QuantidadeDigitoSequencia { get; set; }
        public decimal? QuantidadePontos { get; set; }
        public decimal? QuantideDiasCanResNaoPagTax { get; set; }
        public int? TempoUtilizacao { get; set; }
        public decimal? PercentualIntegralizacaoUti { get; set; }
        public string? UtilizaTarifarioPontos { get; set; } = "N";
        public int? FrTarifarioPonto { get; set; }
        public string? FormatoDataEntrega { get; set; } = "D";
        public int? GrupoEmpresa { get; set; }
        public int? Empresa { get; set; }
        public string? TipoReajusteContrato { get; set; } = "A";
        public string? SwVinculosTse { get; set; }

        public List<FrProdutoParticipante> ProdutosParticipantes { get; set; } = new List<FrProdutoParticipante>();
    }
}
