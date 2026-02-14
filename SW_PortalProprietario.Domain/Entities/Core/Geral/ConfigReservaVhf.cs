namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    /// <summary>
    /// Configuração padrão para reservas VHF (PMS).
    /// Armazena valores padrão para integração com sistemas legados.
    /// </summary>
    public class ConfigReservaVhf : EntityBaseCore
    {
        public virtual string TipoUtilizacao { get; set; } = string.Empty;
        public virtual int? HotelId { get; set; }
        public virtual string TipoHospede { get; set; } = string.Empty;
        public virtual string Origem { get; set; } = string.Empty;
        public virtual string TarifaHotel { get; set; } = string.Empty;
        public virtual string CodigoPensao { get; set; } = string.Empty;
        public virtual bool PermiteIntercambioMultipropriedade { get; set; }
    }
}
