using CMDomain.Entities;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TarifaHotelModel
    {
        public int? Id { get; set; }
        public string? IdHotel { get; set; }
        public string? Categoria { get; set; }
        public string? Nome { get; set; }
        public int? IdOrigem { get; set; }
        public string? CodSegmento { get; set; }
        public string? Label => $"IdTarifa: {Id} Categoria: {Categoria} - Nome: {Nome}";

    }
}
