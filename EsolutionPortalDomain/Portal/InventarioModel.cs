namespace EsolutionPortalDomain.Portal
{
    public class InventarioModel
    {
        public int Id { get; set; }
        public int InventarioId { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? NomeExibicao { get; set; }
        public string? Pool { get; set; }
        public int? Hotel { get; set; }
        public string? GerarReservaAutomaticamente { get; set; } = "S";
        public string? PermitirUsoConvidado { get; set; } = "S";
        public string? PemitirReservaFracionada { get; set; } = "N";
        public string? PemitirCriarSegundaReservaAgendamento { get; set; } = "N";
        public string? PermitirEfetuarReservaPortalProprietario { get; set; } = "N";
        public string? CriarReservaStatusConfirmada { get; set; } = "S";
        public int? SegmentoMercadoReserva { get; set; }
        public int? OrigemReserva { get; set; }
        public int? MeioComunicacaoReserva { get; set; }
        public string? TipoHospedeProprietarioReserva { get; set; }
        public string? TipoHospedeConvidadoReserva { get; set; }
        public string? ValidarSituacaoFinanceiraEfetuarReserva { get; set; }
        public int? DiasVencidoConsideradoInadimplente { get; set; }
        public int? DiasMinimoInicioAgendamentoAdicionar { get; set; }
        public int? DiasMinimoInicioAgendamentoRemover { get; set; }
    }
}
