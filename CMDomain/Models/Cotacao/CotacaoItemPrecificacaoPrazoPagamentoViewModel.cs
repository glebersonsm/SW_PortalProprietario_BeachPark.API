namespace CMDomain.Models.Cotacao
{
    public class CotacaoItemPrecificacaoPrazoPagamentoViewModel
    {
        public int? CodProcesso { get; set; }
        public int? IdProcXArt { get; set; }
        public int? Proposta { get; set; }
        public int? IdPrazoPgto { get; set; }
        public int? PrazoPagamento { get; set; }
        public string? TipoPrazo { get; set; }
        public decimal? Percentual { get; set; }
        public DateTime? DataPgto { get; set; }
        public decimal? Valor { get; set; }

    }
}
