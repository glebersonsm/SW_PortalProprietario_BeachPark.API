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
        public string TipoHospede { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string TarifaHotel { get; set; } = string.Empty;
        public string CodigoPensao { get; set; } = string.Empty;
        public bool PermiteIntercambioMultipropriedade { get; set; }
    }
}
