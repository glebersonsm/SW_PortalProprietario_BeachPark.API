using CMDomain.Entities;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class SegmentoReservaModel
    {
        public string? IdHotel { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? Label => $"CodSegmento: {Codigo} - Nome: {Nome}";

    }
}
