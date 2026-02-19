namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TrocaTipoUsoResponseModel
    {
        public int ReservaId { get; set; }
        public string NovoTipoUso { get; set; } = string.Empty;
        public decimal? PontosDebitados { get; set; }
        public decimal? PontosDevolvidos { get; set; }
        public decimal? SaldoPontosAtual { get; set; }
    }
}
