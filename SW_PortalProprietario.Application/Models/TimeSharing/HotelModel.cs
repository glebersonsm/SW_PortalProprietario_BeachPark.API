namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class HotelModel
    {
        public string? HotelNome { get; set; }
        public int? IdHotel { get; set; }
        public int? Id { get; set; }
        public string? Label => $"{Id} - {HotelNome}";


    }
}
