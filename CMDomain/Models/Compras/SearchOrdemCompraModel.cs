namespace CMDomain.Models.Compras
{
    public class SearchOrdemCompraModel
    {
        public string? UsuarioLogado { get; set; }
        public string? IdEmpresa { get; set; }
        public string? NumOC { get; set; }
        public string? IdFornecedor { get; set; }
        public string? IdComprador { get; set; }
        public string? Status { get; set; }
        public string? CodProcesso { get; set; }
        public string? DataInicialOc { get; set; }
        public string? DataFinalOc { get; set; }
        public List<string> ProdutosContidos { get; set; } = new List<string>();

    }
}
