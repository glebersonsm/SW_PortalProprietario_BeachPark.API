namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TrocaPeriodoResponseModel
    {
        public int ReservaId { get; set; }
        public DateTime NovoCheckin { get; set; }
        public DateTime NovoCheckout { get; set; }
        public decimal PontosDebitados { get; set; }
        public decimal PontosDevolvidos { get; set; }
        public decimal PontosAdicionais { get; set; }
        public decimal SaldoPontosAtual { get; set; }
    }
}
