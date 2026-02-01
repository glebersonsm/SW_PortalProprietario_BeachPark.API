namespace CMDomain.Models.Cotacao
{
    public class SumarioCotacaoViewModel
    {
        public int? CodProcesso { get; set; }
        public string? Status { get; set; }
        public int? IdComprador { get; set; }
        public string? Comprador { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        public List<SumarioCotacaoProdutoViewModel> ItensDaCotacao { get; set; } = new List<SumarioCotacaoProdutoViewModel>();

    }
}
