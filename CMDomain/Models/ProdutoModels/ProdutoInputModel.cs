namespace CMDomain.Models.ProdutoModels
{
    public class ProdutoInputModel : ModelRequestBase
    {
        public string? CodProduto { get; set; }
        public string? DescProd { get; set; }
        public string? DescrCompl { get; set; }
        public string? CodGrupoProd { get; set; }
        public string? CodMedCustoMedio { get; set; }
        public string? CodMedCompra { get; set; }
        public string? CodMenorMedida { get; set; }
        public string? FlgVariavel { get; set; } = "N";
        public string? ConsumoRevenda { get; set; } = "R";
        public string? ItemEstocavel { get; set; } = "S";
        public string? CodGeneroItem { get; set; }
        public string? CodTipoArtigo { get; set; }
        public string? CodigoNCM { get; set; }
        public string? Cest { get; set; }
        public int? IdGtin { get; set; }
        public string? CodEan { get; set; }
        public bool? UtilizarContasDoGrupoDeProduto { get; set; } = true;
        public DefinicaoContabilProdutoOuGrupoProdutoInputModel? DefinicaoContabilProduto { get; set; }
        public TributacaoProdutoInputModel TributacaoProduto { get; set; } = new TributacaoProdutoInputModel();


    }
}
