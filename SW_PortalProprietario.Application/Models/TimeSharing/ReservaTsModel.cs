namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ReservaTsModel
    {
        public int? Id { get; set; }
        public DateTime? DataReserva { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? DataCheckin { get; set; }
        public DateTime? Checkout { get; set; }
        public DateTime? DataCheckout { get; set; }
        public string? StatusReserva { get; set; }
        public string? TipoPensao { get; set; }
        public string? ProjetoXContrato { get; set; }
        public string? NumeroContrato { get; set; }
        public int? IdReservasFront { get; set; }
        public Int64? NumReserva { get; set; }
        public int? IdVendaTs { get; set; }
        public int? IdVendaXContrato { get; set; }
        public string? ListaEspera { get; set; }
        public string? TaxaIsenta { get; set; }
        public string? NomeCliente { get; set; }
        public string? Hotel { get; set; }
        public int? IdHotel { get; set; }
        public string? CodTipoUh { get; set; }
        public string? TipoUH { get; set; }
        public string? DataConfirmacao { get; set; }
        public string? DataCancelamento { get; set; }
        public string? Fracionamento { get; set; }
        public int? Adultos { get; set; }
        public int? Criancas1 { get; set; }
        public int? Criancas2 { get; set; }
        public decimal? PontoReserva { get; set; }
        public decimal? ValorTaxa { get; set; }
        public string? CriadaPor { get; set; }
        public string? TipoLancamento { get; set; }
        public string? TipoReserva { get; set; }
        public decimal? ValorPensao { get; set; }
        public decimal? ValorPonto { get; set; }
        public decimal? ValorPontos { get; set; }
        public int? TipoUhEstadia { get; set; }
        public int? TipoUhTarifa { get; set; }
        public int? IdOrigem { get; set; }
        public string? CodSegmento { get; set; }
        public int? IdRoomList { get; set; }
        public string? NomeGrupo { get; set; }
        public int? LocReserva { get; set; }
        public int? AgendamentoId { get; set; }
        public string? TipoDeUso { get; set; }
        public string? Observacoes { get; set; }
        public List<HospedeResultModel>? Hospedes { get; set; } = new List<HospedeResultModel>();

    }
}
