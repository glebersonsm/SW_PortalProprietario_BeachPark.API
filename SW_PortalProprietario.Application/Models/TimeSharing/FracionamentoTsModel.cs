namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class FracionamentoTsModel
    {
        public int? IdVendaXContrato { get; set; }
        public DateTime? DataLancamento { get; set; }
        public int? IdCliente { get; set; }
        public int? IdFracionamentoTs { get; set; }
        public int? IdReservasFront1 { get; set; }
        public Int64? NumReserva1 { get; set; }
        public DateTime? CheckinReservasFront1 { get; set; }
        public DateTime? CheckoutReservasFront1 { get; set; }
        public int? StatusReservasFront1 { get; set; }
        public int? HotelId { get; set; }
        public int? QtdePessoas { get; set; }

    }
}
