namespace SW_PortalProprietario.Application.Models.AuditModels
{
    public class AuditLogPagedResult
    {
        public List<AuditLogModel> Data { get; set; } = new List<AuditLogModel>();
        public int PageNumber { get; set; }
        public int LastPageNumber { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
    }
}

