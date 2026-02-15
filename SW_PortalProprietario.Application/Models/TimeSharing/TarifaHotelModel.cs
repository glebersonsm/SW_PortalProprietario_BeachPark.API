using CMDomain.Entities;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TarifaHotelModel
    {
        public int? Id { get; set; }
        public string? IdHotel { get; set; }
        public string? Categoria { get; set; }
        public string? Nome { get; set; }
        public string? Label => $"{IdHotel} - {Categoria} - {Nome}";

    }
}
