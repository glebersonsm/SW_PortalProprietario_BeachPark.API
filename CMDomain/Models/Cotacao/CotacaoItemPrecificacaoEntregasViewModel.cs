namespace CMDomain.Models.Cotacao
{
    public class CotacaoItemPrecificacaoEntregasViewModel
    {
        public int? CodProcesso { get; set; }
        public int? IdProcXArt { get; set; }
        public int? Proposta { get; set; }
        public int? IdPrazoEntrega { get; set; }
        public int? PrazoEntrega { get; set; }
        public string? CodMedida { get; set; }
        public string? TipoPrazo { get; set; }
        public decimal? QuantidadeDaEntrega { get; set; }
        public DateTime? DataEntrada { get; set; }

    }
}
