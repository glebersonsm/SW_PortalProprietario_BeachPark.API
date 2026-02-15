using CMDomain.Entities;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TipoHospedeModel
    {
        public int? Id { get; set; }
        public string? CodReduzido { get; set; }
        public string? Nome { get; set; }
        public int? IdHotel { get; set; }
        public string? Label => $"{Id} - {CodReduzido} - {Nome}";

    }
}
