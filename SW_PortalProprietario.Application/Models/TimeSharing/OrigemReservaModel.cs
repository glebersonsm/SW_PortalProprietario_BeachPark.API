using CMDomain.Entities;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class OrigemReservaModel
    {
        public int? Id { get; set; }
        public int? IdOrigem { get; set; }
        public string? CodReduzido { get; set; }
        public string? Nome { get; set; }
        public string? Label => $"{Id} - {CodReduzido} - {Nome}";

    }
}
