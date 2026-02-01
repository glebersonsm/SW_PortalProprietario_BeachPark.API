namespace CMDomain.Models.Cotacao
{
    public class CotacaoCustoAgregNotaViewModel
    {
        public int? IdValorAgregCogG { get; set; }
        public int? CodProcesso { get; set; }
        public int? Proposta { get; set; }
        public int? IdFornecedor { get; set; }
        public int? IdTipoAgregado { get; set; }
        public string? Descricao { get; set; }
        public decimal? Valor { get; set; }
        public decimal? Percentual { get; set; }
        public decimal? BaseCalculo { get; set; }
        public string? TipoCalculo { get; set; }

    }
}
