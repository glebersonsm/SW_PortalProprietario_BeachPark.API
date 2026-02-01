using SW_PortalProprietario.Application.Models.AuditModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IAuditCacheService
    {
        Task<AuditLogPagedResult> GetCachedAuditLogsAsync(string cacheKey, Func<Task<AuditLogPagedResult>> fetchFunc);
        Task<List<AuditLogModel>> GetCachedEntityHistoryAsync(string entityType, int entityId, Func<Task<List<AuditLogModel>>> fetchFunc);
        Task InvalidateEntityCacheAsync(string entityType, int entityId);
    }
}

