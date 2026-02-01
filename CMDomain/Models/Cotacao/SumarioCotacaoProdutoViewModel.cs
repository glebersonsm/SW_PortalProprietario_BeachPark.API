namespace CMDomain.Models.Cotacao
{
    public class SumarioCotacaoProdutoViewModel
    {
        public int? CodProcesso { get; set; }
        public int? IdProcXArt { get; set; }
        public string? CodigoProduto { get; set; }
        public string? NomeProduto { get; set; }
        public string? CodigoGrupoProduto { get; set; }
        public string? NomeGrupoProduto { get; set; }
        public string? CodMedida { get; set; }
        public decimal? QuantidadePedida { get; set; }
        public List<SumarioCotacaoItemViewModel> Cotacoes { get; set; } = new List<SumarioCotacaoItemViewModel>();

    }
}
