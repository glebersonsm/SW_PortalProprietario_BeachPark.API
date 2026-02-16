namespace SW_PortalProprietario.Application.Models
{
    public class TipoUhEsolModel
    {
        public int? Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? Capacidade { get; set; }
        public int? IdHotel { get; set; }
        public string? Label => $"{Codigo} - {Nome}";

    }

}
