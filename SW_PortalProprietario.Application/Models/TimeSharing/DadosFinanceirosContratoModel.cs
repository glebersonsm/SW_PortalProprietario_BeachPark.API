namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class DadosFinanceirosContratoModel
    {
        public int? IdVendaXContrato { get; set; }
        public string? NumeroContrato { get; set; }
        public string? Produto { get; set; }
        public decimal? ValorCompra { get; set; }
        public decimal? ValorSaldoAtual { get; set; }
        public decimal? ValorDoPonto { get; set; }
        public decimal? PontosComprados { get; set; }
        public decimal? PontosUtilizados { get; set; }
        public decimal? SaldoDePontos { get; set; }
        public List<UtilizacoesItensModel>? UtilizacoesItens { get; set; }

    }
}
