namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class DisponibilidadeFixaModel
    {
        public int? IdHotel { get; set; }
        public int? IdTipoUh { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public int? QtdaDisp { get; set; }
        public decimal? PercDisp { get; set; }
        public string? FlgPerc { get; set; }

    }
}
