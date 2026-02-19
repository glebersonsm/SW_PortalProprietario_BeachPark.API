using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.AuditModels
{
    public class AuditLogSearchModel
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public int Action { get; set; } // EnumAuditAction como int para SQL
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string ChangesJson { get; set; } = "{}";
        public string? EntityDataJson { get; set; }
        public string? ObjectGuid { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public int? UsuarioCriacao { get; set; }
    }
}

