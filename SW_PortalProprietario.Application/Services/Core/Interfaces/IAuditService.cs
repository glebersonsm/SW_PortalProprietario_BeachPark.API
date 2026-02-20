using SW_PortalProprietario.Application.Models.AuditModels;
using SW_Utils.Models;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IAuditService
    {
        Task SaveAuditLogAsync(AuditLogMessageEvent message);
        Task<AuditLogPagedResult> GetAuditLogsAsync(AuditLogFilterModel filter);
        Task<List<AuditLogModel>> GetAuditLogByEntityAsync(string entityType, int entityId);
        Task<AuditLogModel?> GetAuditLogByIdAsync(int id);
    }
}

