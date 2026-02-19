namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TrocaPeriodoRequestModel
    {
        public int ReservaId { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public int IdVendaXContrato { get; set; }
        public string HotelId { get; set; } = string.Empty;
        public DateTime NovoCheckin { get; set; }
        public DateTime NovoCheckout { get; set; }
        public string? TipoApartamento { get; set; }
        public int? Capacidade { get; set; }
        public string? TipoDeBusca { get; set; }
    }
}
