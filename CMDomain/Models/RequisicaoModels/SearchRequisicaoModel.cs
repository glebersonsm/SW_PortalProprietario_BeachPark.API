namespace CMDomain.Models.RequisicaoModels
{
    public class SearchRequisicaoModel
    {
        public string? UsuarioLogado { get; set; }
        public Int64? NumRequisicao { get; set; }
        public int? CodAlmoxarifadoSolicitante { get; set; }
        public int? CodAlmoxarifadoAtendente { get; set; }
        public List<string> Status { get; set; } = new List<string>();
        public DateTime? DataNecessidadeInicial { get; set; }
        public DateTime? DataNecessidadeFinal { get; set; }
        public DateTime? DataRequisicaoInicial { get; set; }
        public DateTime? DataRequisicaoFinal { get; set; }
        public string? UsuarioSolicitante { get; set; }
        public List<string> ProdutosContidos { get; set; } = new List<string>();

    }
}
