namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class SearchReservaTsModel
    {
        public int? NumReserva { get; set; }
        public string? NumeroContrato { get; set; }
        public string? NomeCliente { get; set; }
        public string? Hotel { get; set; }
        public string? NumDocumentoCliente { get; set; }
        public string? StatusReserva { get; set; }
        public DateTime? CheckinInicial { get; set; }
        public DateTime? CheckinFinal { get; set; }
        public DateTime? CheckoutInicial { get; set; }
        public DateTime? CheckoutFinal { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }

    }
}
