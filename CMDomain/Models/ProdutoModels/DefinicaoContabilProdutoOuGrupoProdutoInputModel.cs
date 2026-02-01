namespace CMDomain.Models.ProdutoModels
{
    public class DefinicaoContabilProdutoOuGrupoProdutoInputModel
    {
        public int? IdArtXContaXCc { get; set; }
        public string? DefinirPorGrupoOuProduto { get; set; } = "G";
        public string? ContaEntrada { get; set; }
        public int? SubContaEntrada { get; set; }
        public string? ContaSaida { get; set; }
        public int? SubContaSaida { get; set; }
        public string? CentroCusto { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public int? UnidadeNegocioId { get; set; }

    }
}
