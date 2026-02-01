namespace CMDomain.Models.Cotacao
{
    public class CotacaoItemPrecificacaoViewModel
    {
        public int? CodProcesso { get; set; }
        public int? IdProcXArt { get; set; }
        public int? Proposta { get; set; }
        public DateTime? DataCotacao { get; set; }
        public string? CodigoProduto { get; set; }
        public string? NomeProduto { get; set; }
        public decimal? QuantidadePedida { get; set; }
        public decimal? QuantidadeFornecida { get; set; }
        public string? CodMedida { get; set; }
        public string? Status { get; set; }
        public string? Observacao { get; set; }
        public string? Contato { get; set; }
        public int? IdItemOc { get; set; }
        public decimal? PrecoUnitario { get; set; }
        public string? Justificativa { get; set; }
        public decimal? PrecoUnitarioTotal => (PrecoUnitario.GetValueOrDefault(0.00m) > 0.00m && QuantidadeFornecida.GetValueOrDefault(0.00m) > 0.00m ?
            (PrecoUnitario.GetValueOrDefault(0.00m) +
            (CustosAgregados.Where(c => c.Valor.GetValueOrDefault(0.00m) > 0.00m && c.Percentual.GetValueOrDefault(0.00m) == 0.00m).Sum(c => c.Valor) +
            CustosAgregados.Where(c => c.Percentual.GetValueOrDefault(0) > 0 && c.Valor.GetValueOrDefault(0.00m) == 0.00m).Sum(c => (c.Percentual.GetValueOrDefault(0.00m) * PrecoUnitario.GetValueOrDefault(0.00m) / 100)))) : 0.00m);

        public decimal PrecoTotal => Math.Round(QuantidadeFornecida.GetValueOrDefault(0.00m) * PrecoUnitarioTotal.GetValueOrDefault(0.00m), 2);

        public List<CotacaoItemPrecificacaoCustoAgregViewModel> CustosAgregados { get; set; } = new List<CotacaoItemPrecificacaoCustoAgregViewModel>();
        public List<CotacaoItemPrecificacaoEntregasViewModel> Entregas { get; set; } = new List<CotacaoItemPrecificacaoEntregasViewModel>();
        public List<CotacaoItemPrecificacaoPrazoPagamentoViewModel> PrazoPagamento { get; set; } = new List<CotacaoItemPrecificacaoPrazoPagamentoViewModel>();

    }
}
