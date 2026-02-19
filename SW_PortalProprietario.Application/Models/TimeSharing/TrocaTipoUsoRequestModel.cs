namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TrocaTipoUsoRequestModel
    {
        public int ReservaId { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public int IdVendaXContrato { get; set; }
        public string NovoTipoUso { get; set; } = string.Empty; // "UP", "UC", "I"
    }
}
