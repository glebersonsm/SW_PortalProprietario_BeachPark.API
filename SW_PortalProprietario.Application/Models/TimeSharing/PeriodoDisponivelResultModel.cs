namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class PeriodoDisponivelResultModel
    {
        public string? TipoApartamento { get; set; }
        public string? TipoPeriodo { get; set; }
        public string? CodTipoUh { get; set; }
        public string? NomeHotel { get; set; }
        public int? HotelId { get; set; }
        public int? TipoUhId { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public decimal? PontosNecessario { get; set; }
        public decimal? SaldoPontos { get; set; }
        public decimal? PontosIntegralDisp { get; set; }
        public int? IdContratoTs { get; set; }
        public int? Capacidade { get; set; }
        public int? CapacidadePontos1 { get; set; }
        public int? CapacidadePontos2 { get; set; }
        public string? PadraoTarifario { get; set; }
        public int? IdVendaTs { get; set; }
        public int? IdVendaXContrato { get; set; }
        public string? NumeroContrato { get; set; }
        public int? IdContrTsXPontos1 { get; set; }
        public int? IdContrTsXPontos2 { get; set; }
        public decimal? PontosParaCapacidade1 { get; set; }
        public decimal? PontosParaCapacidade2 { get; set; }

        public int? FechamentoFracionamentoPossivelId { get; set; }
        public Int64? ReservaAberturaFracionamento { get; set; }
        public DateTime? CheckinReservaAberturaFracionamento { get; set; }
        public DateTime? CheckoutReservaAberturaFracionamento { get; set; }
        public int? HotelIdAberturaFracionamento { get; set; }
        public int? QtdePessoasAberturaFechamento { get; set; }
        public int Diarias => Checkout.GetValueOrDefault().Subtract(Checkin.GetValueOrDefault()).Days;
        public DateTime? DataVenda { get; set; }
        public DateTime? ValidadeCredito { get; set; }
        public decimal? PontosUtilizados { get; set; }
        public decimal? CreditoPontos { get; set; }
        public DateTime? VencimentoContrato { get; set; }

    }
}
