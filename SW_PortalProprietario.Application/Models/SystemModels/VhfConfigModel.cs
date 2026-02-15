using SW_PortalProprietario.Application.Models;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    /// <summary>
    /// Modelo de configuração padrão para reservas VHF (PMS).
    /// </summary>
    public class VhfConfigModel : ModelBase
    {
        public string TipoUtilizacao { get; set; } = string.Empty;
        public int? HotelId { get; set; }
        public string? HotelNome { get; set; }
        public string TipoHospedeAdulto { get; set; } = string.Empty;
        public string? TipoHospedeCrianca1 { get; set; }
        public string? TipoHospedeCrianca2 { get; set; }
        public string Origem { get; set; } = string.Empty;
        public string TarifaHotel { get; set; } = string.Empty;
        public string CodigoPensao { get; set; } = string.Empty;
        public bool PermiteIntercambioMultipropriedade { get; set; }
    }
}
