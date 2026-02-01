namespace EsolutionPortalDomain.ReservasApiModels.Hotel
{
    public class ReservaForEditModel
    {
        public int? Id { get; set; }
        public DateTime? DataReserva { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public string? Status { get; set; }
        public string? TipoPensao { get; set; }
        public string? TipoHospede { get; set; }
        public string? TipoUhNome { get; set; }
        public int Adultos { get; set; }
        public int Criancas1 { get; set; }
        public int Criancas2 { get; set; }
        public string? HotelNome { get; set; }
        public string? NomeHospede { get; set; }
        public int? PeriodoCotaDisponibilidadeId { get; set; }
        public string? Cota { get; set; }
        public string? ProprietarioNome { get; set; }
        public string? ProprietarioCpfCnpj { get; set; }
        public int? ProprietarioId { get; set; }
        public int? UhCondominioId { get; set; }
        public List<HospedesReservaAgendamentoModel>? Hospedes { get; set; }
        public int? Capacidade { get; set; }
        public string? TipoUtilizacao { get; set; } //UsoProprio, UsoConvidado, Intercambiadora

    }
}
