namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TipoUhCmModel
    {
        public int? IdHotel { get; set; }
        public string? CodReduzido { get; set; }
        public string? Codigo { get; set; }
        public int? IdTipoUh { get; set; }
        public int? Id { get; set; }
        public int? Qtde { get; set; }
        public string? Descricao { get; set; }
        public string? Nome { get; set; }
        public int Capacidade { get; set; }
        public string? Label => $"IdHotel: {IdHotel} - Cod: {Codigo} - Nome: {Nome}";

    }
}
