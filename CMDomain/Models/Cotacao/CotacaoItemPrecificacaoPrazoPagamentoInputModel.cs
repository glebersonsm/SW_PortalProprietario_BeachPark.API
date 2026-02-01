namespace CMDomain.Models.Cotacao
{
    public class CotacaoItemPrecificacaoPrazoPagamentoInputModel
    {
        public string? IdPrazoPgto { get; set; }
        public string? PrazoPagamento { get; set; }
        public string? TipoPrazo { get; set; }
        public string? Percentual { get; set; }
        public string? Valor { get; set; }
        public string? DataPagamento { get; set; }
        public string? FlgAdiantamento { get; set; } = "N";

    }
}
