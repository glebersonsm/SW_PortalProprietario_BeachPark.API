namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ConsumoDisponibilidadeReservas
    {
        public DateTime? Data { get; set; }
        public int? IdHotel { get; set; }
        public string? CodReduzido { get; set; }
        public int? IdTipoUh { get; set; }
        public int? QtdReal { get; set; }
        public int? TotOcupada { get; set; }
        public int? TotWaitList { get; set; }
        public int? TotAConfirmar { get; set; }
        public int? TotConfirmada { get; set; }
        public int? QtdeBloqueadaManutencao { get; set; }
        public int? QuantidadeUtilizada =>
            TotOcupada.GetValueOrDefault();

        public int QtdeDisponivel
        {
            get
            {
                return QtdReal.GetValueOrDefault() - QuantidadeUtilizada.GetValueOrDefault();
            }
        }

    }
}
