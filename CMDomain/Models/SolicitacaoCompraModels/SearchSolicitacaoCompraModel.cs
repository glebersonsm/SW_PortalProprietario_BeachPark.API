namespace CMDomain.Models.SolicitacaoCompraModels
{
    public class SearchSolicitacaoCompraModel
    {
        public string? UsuarioLogado { get; set; }
        public int? NumSolCompra { get; set; }
        public int? UnidadeNegocio { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public List<string> Status { get; set; } = new List<string>();
        public DateTime? DataEntregaInicial { get; set; }
        public DateTime? DataEntregaFinal { get; set; }
        public DateTime? DataEmissaoInicial { get; set; }
        public DateTime? DataEmissaoFinal { get; set; }
        public string? UsuarioSolicitante { get; set; }
        public string? Urgente { get; set; }
        public string? CustoEstoque { get; set; }
        public string? PrePronta { get; set; }
        public List<string> ProdutosContidos { get; set; } = new List<string>();

    }
}
