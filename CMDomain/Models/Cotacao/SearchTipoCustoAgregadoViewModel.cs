namespace CMDomain.Models.Cotacao
{
    public class SearchTipoCustoAgregadoViewModel
    {
        public string? UsuarioLogado { get; set; }
        public int? IdEmpresa { get; set; }
        public int? IdTipoAgregado { get; set; }
        public string? Descricao { get; set; }
        public string? TipoIncidencia { get; set; }
        public bool? ApenasAtivos { get; set; } = true;

    }
}
