namespace CMDomain.Models.Cotacao
{
    public class CotacaoPrecificacaoViewModel
    {
        public int? CodProcesso { get; set; }
        public DateTime? DataInicio { get; set; }
        public int? Proposta { get; set; }
        public string? Status { get; set; }
        public int? IdComprador { get; set; }
        public string? Comprador { get; set; }
        public int? IdFornecedor { get; set; }
        public string? NomeFornecedor { get; set; }
        public string? DocumentoFornecedor { get; set; }
        public decimal? ValorTotalPrecificado => Math.Round(((ItemsDaCotacao.Sum(b => b.PrecoUnitarioTotal.GetValueOrDefault(0.00m) * b.QuantidadeFornecida.GetValueOrDefault(0.00m)))), 2);

        public List<CotacaoCustoAgregNotaViewModel> CustosAgregadosNota { get; set; } = new List<CotacaoCustoAgregNotaViewModel>();
        public List<CotacaoItemPrecificacaoViewModel> ItemsDaCotacao { get; set; } = new List<CotacaoItemPrecificacaoViewModel>();

    }
}
