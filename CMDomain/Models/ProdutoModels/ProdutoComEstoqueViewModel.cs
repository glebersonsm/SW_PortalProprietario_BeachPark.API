namespace CMDomain.Models.ProdutoModels
{
    public class ProdutoComEstoqueViewModel
    {
        public ProdutoViewModel? ProdutoViewModel { get; set; }
        public decimal? QuantidadeEmEstoque { get; set; }
        public int? CodigoAlmoxarifado { get; set; }
        public decimal? CustoMedio { get; set; }
        public int? IdLoteArtigo { get; set; }
        public string? NumLote { get; set; }
    }
}
