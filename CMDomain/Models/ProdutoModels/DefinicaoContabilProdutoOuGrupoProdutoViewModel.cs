namespace CMDomain.Models.ProdutoModels
{
    public class DefinicaoContabilProdutoOuGrupoProdutoViewModel
    {

        public int? IdArtXContaXCc { get; set; }
        public int? IdEmpresa { get; set; }
        public string? CodProduto { get; set; }
        public string? CodGrupoProd { get; set; }
        public int? IdPlanoConta { get; set; }
        public string? ContaEntrada { get; set; }
        public int? SubContaEntrada { get; set; }
        public string? ContaSaida { get; set; }
        public int? SubContaSaida { get; set; }
        public string? CentroCusto { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public int? UnidadeNegocioId { get; set; }
        public string? UnidadeNegocioNome { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

    }
}
