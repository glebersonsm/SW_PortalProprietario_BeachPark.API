namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class SearchMinhasReservaTsModel
    {
        public int? NumReserva { get; set; }
        public string? NumeroContrato { get; set; }
        public string? StatusReserva { get; set; }
        public DateTime? CheckinInicial { get; set; }
        public DateTime? CheckinFinal { get; set; }
        public DateTime? CheckoutInicial { get; set; }
        public DateTime? CheckoutFinal { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }
        public int? IdCliente { get; set; }

    }
}
