namespace SW_PortalProprietario.Application.Models.SystemModels
{
    /// <summary>
    /// Modelo de entrada para criar/atualizar configuração VHF.
    /// </summary>
    public class VhfConfigInputModel
    {
        public string TipoUtilizacao { get; set; } = string.Empty;
        public int? HotelId { get; set; }
        public string TipoHospede { get; set; } = string.Empty;
        public string? TipoHospedeCrianca1 { get; set; }
        public string? TipoHospedeCrianca2 { get; set; }
        public string Origem { get; set; } = string.Empty;
        public string TarifaHotel { get; set; } = string.Empty;
        public string CodigoPensao { get; set; } = string.Empty;
        public bool PermiteIntercambioMultipropriedade { get; set; }
    }
}
