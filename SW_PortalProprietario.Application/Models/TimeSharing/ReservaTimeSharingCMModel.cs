namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ReservaTimeSharingCMModel
    {
        public int? IdHotel { get; set; }
        public int? Id { get; set; }
        public string? NomeHotel { get; set; }
        public int? IdReservasFront { get; set; }
        public int? IdOrigemReserva { get; set; }
        public string? OrigemReserva { get; set; }
        public string? StatusReserva { get; set; }
        public int? Usuario { get; set; }
        public string? NomeUsuario { get; set; }
        public int? ClienteReservante { get; set; }
        public string? ClienteReservanteNome { get; set; }
        public int? TipoUhEstadia { get; set; }
        public string? NomeTipoUhEstadia { get; set; }
        public int? IdTipoUh { get; set; }
        public string? Reservante { get; set; }
        public string? TelefoneReservante { get; set; }
        public string? EmailReservante { get; set; }
        public string? CodUh { get; set; }
        public int? idTarifa { get; set; }
        public string? NomeTarifa { get; set; }
        public string? SegmentoReserva { get; set; }
        public string? MeioComunicacao { get; set; }
        public int? ContratoInicial { get; set; }
        public int? ContratoFinal { get; set; }
        public string? NumeroContrato { get; set; }
        public int? IdVendaXContrato { get; set; }
        public DateTime? DataChegadaPrevista { get; set; }
        public DateTime? DataChegadaReal { get; set; }
        public DateTime? DataPartidaPrevista { get; set; }
        public DateTime? DataPartidaReal { get; set; }
        public int? Adultos { get; set; }
        public int? Criancas1 { get; set; }
        public int? Criancas2 { get; set; }
        public decimal? PontosDebitados { get; set; }
        public string? PadraoTarifario { get; set; }
        public int? CapacidadePontos1 { get; set; }
        public decimal? PontosParaCapacidade1 { get; set; }
        public int? CapacidadePontos2 { get; set; }
        public decimal? PontosParaCapacidade2 { get; set; }
        public string? CodPensao { get; set; }
        public string? Pensao { get; set; }
        public DateTime? DataReserva { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public string? Observacoes { get; set; }
        public string? Documento { get; set; }
        public Int64? NumReserva { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public string? ObsCancelamento { get; set; }
        public DateTime? DataReativacao { get; set; }
        public string? FlgDiariaFixa { get; set; }
        public decimal? VleDiariaManual { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }
        public string? TipoDeUso { get; set; }

        public List<HospedeInputModel>? Hospedes { get; set; }
    }
}
