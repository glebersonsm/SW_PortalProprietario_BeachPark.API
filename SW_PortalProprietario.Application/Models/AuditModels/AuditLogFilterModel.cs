using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.AuditModels
{
    public class AuditLogFilterModel
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public int? UserId { get; set; }
        public EnumAuditAction? Action { get; set; }
        public string? IpAddress { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

