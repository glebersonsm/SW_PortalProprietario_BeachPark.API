namespace CMDomain.Models.ProdutoModels
{
    public class ProdutoAlmoxaEstoqueViewModel
    {
        public int? IdMov { get; set; }
        public string? CodProduto { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public decimal? QuantidadeEmEstoque { get; set; }
        public decimal? CustoMedio { get; set; }
        public string? Placonta { get; set; }
        public int? Plano { get; set; }
        public string? PlacontaEntrada { get; set; }
        public int? PlanoEntrada { get; set; }
        public int? IdLoteArtigo { get; set; }
        public string? NumLote { get; set; }

    }
}
