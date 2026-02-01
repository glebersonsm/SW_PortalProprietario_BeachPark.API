namespace CMDomain.Models.Cotacao
{
    public class SearchCotacaoModel
    {
        public string? UsuarioLogado { get; set; }
        public int? NumSolCompra { get; set; }
        public DateTime? DataInicialCotacao { get; set; }
        public DateTime? DataFinalCotacao { get; set; }
        /// <summary>
        /// F = Finalizada, S = Sumario calculado, C = Cotando
        /// </summary>
        public string? StatusCotacao { get; set; }
        public int? CodProcesso { get; set; }
        public bool ApenasFornecedoresQuePreencheramValor { get; set; } = true;
        public List<int> ItensDaSolicitacao { get; set; } = new List<int>();

    }
}
