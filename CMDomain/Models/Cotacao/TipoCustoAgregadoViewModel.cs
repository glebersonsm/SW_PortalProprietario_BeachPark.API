namespace CMDomain.Models.Cotacao
{
    public class TipoCustoAgregadoViewModel
    {
        public int? IdTipoAgregado { get; set; }
        public string? Descricao { get; set; }
        public string? PercentualValor { get; set; } = "Valor";
        public int? IdAlteradorValor { get; set; }
        public string? TipoIncidencia { get; set; } = "T = Total";
        public string? Ativo { get; set; } = "Sim";
        public int? IdEmpresa { get; set; }

    }
}
