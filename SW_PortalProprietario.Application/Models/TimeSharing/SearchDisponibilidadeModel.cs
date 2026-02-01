namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class SearchDisponibilidadeModel
    {
        public string? NumeroContrato { get; set; }
        public int? IdVendaXContrato { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public string? HotelId { get; set; }
        public string? TipoDeBusca { get; set; }
        public string? NumReserva { get; internal set; }
        public int? IdReservasFront { get; set; }
    }
}
