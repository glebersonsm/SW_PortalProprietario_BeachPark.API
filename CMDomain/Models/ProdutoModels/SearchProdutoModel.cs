namespace CMDomain.Models.ProdutoModels
{
    public class SearchProdutoModel : ModelRequestBase
    {
        public string? CodProduto { get; set; }
        public string? DescProd { get; set; }
        public string? CodGrupoProd { get; set; }
        public List<string> CodProdutos { get; set; } = new List<string>();

    }
}
