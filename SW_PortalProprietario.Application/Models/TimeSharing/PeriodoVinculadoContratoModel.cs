namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class PeriodoVinculadoContratoModel
    {
        public DateTime? Data { get; set; }
        public int? IdHotel { get; set; }
        public string? FlgFeriado { get; set; }
        public int? IdTipoUh { get; set; }
        public string? FlgDesconto { get; set; }
        public int? IdContrTsXPontos { get; set; }
        public int? IdTemporadaTs { get; set; }
        public string? TipoPeriodo { get; set; }

    }
}
