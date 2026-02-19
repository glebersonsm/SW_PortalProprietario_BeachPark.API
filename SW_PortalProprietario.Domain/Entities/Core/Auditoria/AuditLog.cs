using SW_PortalProprietario.Domain.Entities.Core;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Auditoria
{
    public class AuditLog : EntityBaseCore
    {
        public virtual string EntityType { get; set; } = string.Empty;
        public virtual int EntityId { get; set; }
        public virtual EnumAuditAction Action { get; set; }
        public virtual int? UserId { get; set; }
        public virtual string? UserName { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual string? IpAddress { get; set; }
        public virtual string? UserAgent { get; set; }
        public virtual string ChangesJson { get; set; } = "{}";
        public virtual string? EntityDataJson { get; set; }
        public virtual string? ObjectGuid { get; set; }
    }
}

