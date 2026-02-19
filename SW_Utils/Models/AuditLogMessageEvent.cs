namespace SW_Utils.Models
{
    public class AuditLogMessageEvent
    {
        public AuditLogMessageEvent()
        {
            Guid = System.Guid.NewGuid().ToString();
        }
        
        public string Guid { get; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public int Action { get; set; } // EnumAuditAction
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string ChangesJson { get; set; } = "{}";
        public string? EntityDataJson { get; set; }
        public string? ObjectGuid { get; set; }
    }
}

