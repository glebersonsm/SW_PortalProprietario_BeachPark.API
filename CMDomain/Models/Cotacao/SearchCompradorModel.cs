namespace CMDomain.Models.Cotacao
{
    public class SearchCompradorModel
    {
        public string? UsuarioLogado { get; set; }
        public string? NomeComprador { get; set; }
        public bool? ApenasAtivos { get; set; } = true;

    }
}
