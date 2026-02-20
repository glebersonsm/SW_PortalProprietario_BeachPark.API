namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    /// <summary>
    /// ConfiguraÃ§Ã£o padrÃ£o para reservas VHF (PMS).
    /// Armazena valores padrÃ£o para integraÃ§Ã£o com sistemas legados.
    /// </summary>
    public class ConfigReservaVhf : EntityBaseCore
    {
        public virtual string TipoUtilizacao { get; set; } = string.Empty;
        public virtual string? TipoNegocio { get; set; }
        public virtual int? HotelId { get; set; }
        public virtual string TipoHospede { get; set; } = string.Empty;
        public virtual string? TipoHospedeCrianca1 { get; set; }
        public virtual string? TipoHospedeCrianca2 { get; set; }
        public virtual string Origem { get; set; } = string.Empty;
        public virtual string TarifaHotel { get; set; } = string.Empty;
        public virtual bool EncaixarSemanaSeHouver { get; set; }
        public virtual string? Segmento { get; set; }
        public virtual string CodigoPensao { get; set; } = string.Empty;
        public virtual bool PermiteIntercambioMultipropriedade { get; set; }
        /// <summary>Percentual mÃ¡ximo de ocupaÃ§Ã£o para retorno de disponibilidade (Timesharing). Se atingido, nÃ£o retorna disponibilidade.</summary>
        public virtual decimal? OcupacaoMaxRetDispTS { get; set; }
        /// <summary>Percentual mÃ¡ximo de ocupaÃ§Ã£o para retorno de disponibilidade (Multipropriedade). Se atingido, nÃ£o retorna disponibilidade.</summary>
        public virtual decimal? OcupacaoMaxRetDispMP { get; set; }
    }
}
