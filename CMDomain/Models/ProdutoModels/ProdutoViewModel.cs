namespace CMDomain.Models.ProdutoModels
{
    public class ProdutoViewModel
    {
        public string? CodProduto { get; set; }
        public string? DescProd { get; set; }
        public string? DescrCompl { get; set; }
        public string? CodGrupoProd { get; set; }
        public string? NomeGrupoProd { get; set; }
        public string? FlgIndicaServico { get; set; }
        public string? CodigoNCM { get; set; }
        public string? Cest { get; set; }
        public int? IdGtin { get; set; }
        public string? CodEan { get; set; }
        public string? CodMedCusto { get; set; }
        public string? CodMedAnalise { get; set; }
        public string? CodMenorMed { get; set; }
        public string? FlgVariavel { get; set; }
        public string? ConsumoRevenda { get; set; }
        public string? LoteValidade { get; set; }
        public string? ItemEstocavel { get; set; }
        public string? CodGenero { get; set; }
        public string? DescricaoGenero { get; set; }
        public string? CodTipoArtigo { get; set; }
        public string? DescTipoArtigo { get; set; }
        public List<ProdutoUnidadeMedidaViewModel> UnidadesDeMedidasDoProduto { get; set; } = new List<ProdutoUnidadeMedidaViewModel>();
        public List<DefinicaoContabilProdutoOuGrupoProdutoViewModel> DefinicaoContabilProdutoOuGrupoProduto { get; set; } = new List<DefinicaoContabilProdutoOuGrupoProdutoViewModel>();
        public List<TributacaoProdutoViewModel> TributacaoProdutoPorEmpresa { get; set; } = new List<TributacaoProdutoViewModel>();


    }
}
