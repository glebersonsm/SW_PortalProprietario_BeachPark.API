namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class UtilizacoesItensModel
    {
        public int? IdLancPontosTs { get; set; }
        public DateTime? DataOperacao { get; set; }
        public string? TipoLancamento { get; set; }
        public string? Motivo { get; set; }
        public string? Tipo { get; set; }
        public decimal? Pontos { get; set; }
        public string? DebitoCredito { get; set; }
        public decimal? TotalTaxa { get; set; }
        public decimal? TotalPagtoTaxa { get; set; }
        public string? Reserva { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public decimal? ValorUtilizacao { get; set; }
        public string? Rci { get; set; }
        public string? Fracionamento { get; set; }
        public string? Status { get; set; }
        public string? DataDebitoPeriodo { get; set; }
        public string? Hotel { get; set; }

    }
}
