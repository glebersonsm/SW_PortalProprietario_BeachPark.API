namespace CMDomain.Models.ProdutoModels
{
    public class ProdutosInputModel : ModelRequestBase
    {

        public List<ProdutoItemInputModel> Produtos { get; set; } = new List<ProdutoItemInputModel>();
    }

    public class ProdutoItemInputModel
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
        public virtual string? ProprietarioDoItem { get; set; } = "0"; //0 = Propriedade do informante ou em seu poder, 1 = Conselheiro de Administração do Informante e em posso de Terceiros, 2 = Propriedade de terceiros em posse do informante
        public TributacaoProdutoInputModel TributacaoProduto { get; set; } = new TributacaoProdutoInputModel();
        public bool? UtilizarContasDoGrupoDeProduto { get; set; } = true;
        public DefinicaoContabilProdutoOuGrupoProdutoInputModel? DefinicaoContabilProduto { get; set; }
    }
}
