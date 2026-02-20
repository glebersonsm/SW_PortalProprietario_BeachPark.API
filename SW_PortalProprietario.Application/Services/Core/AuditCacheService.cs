using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuditModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class AuditCacheService : IAuditCacheService
    {
        private readonly ICacheStore _cache;
        private readonly ILogger<AuditCacheService> _logger;

        // TTL em segundos (10 segundos conforme solicitado)
        private const int TTL_LISTAS_SEGUNDOS = 10;
        private const int TTL_HISTORICO_SEGUNDOS = 10;

        public AuditCacheService(
            ICacheStore cache,
            ILogger<AuditCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<AuditLogPagedResult> GetCachedAuditLogsAsync(string cacheKey, Func<Task<AuditLogPagedResult>> fetchFunc)
        {
            try
            {
                // Gerar chave de cache com hash
                var cacheKeyHash = GenerateCacheKey($"audit:logs:{cacheKey}");
                
                // Tentar buscar do cache
                var cached = await _cache.GetAsync<AuditLogPagedResult>(cacheKeyHash);
                if (cached != null && cached.Data != null && cached.Data.Any())
                {
                    return cached;
                }

                // Se nÃ£o encontrou no cache, buscar do banco
                var result = await fetchFunc();

                // Armazenar no cache
                if (result != null && result.Data != null && result.Data.Any())
                {
                    await _cache.AddAsync(
                        cacheKeyHash,
                        result,
                        DateTimeOffset.Now.AddSeconds(TTL_LISTAS_SEGUNDOS));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar/armazenar cache de logs de auditoria");
                // Em caso de erro no cache, buscar diretamente do banco
                return await fetchFunc();
            }
        }

        public async Task<List<AuditLogModel>> GetCachedEntityHistoryAsync(string entityType, int entityId, Func<Task<List<AuditLogModel>>> fetchFunc)
        {
            try
            {
                var cacheKey = $"audit:entity:{entityType}:{entityId}";
                
                // Tentar buscar do cache
                var cached = await _cache.GetAsync<List<AuditLogModel>>(cacheKey);
                if (cached != null && cached.Any())
                {
                    return cached;
                }

                // Se nÃ£o encontrou no cache, buscar do banco
                var result = await fetchFunc();

                // Armazenar no cache
                if (result != null && result.Any())
                {
                    await _cache.AddAsync(
                        cacheKey,
                        result,
                        DateTimeOffset.Now.AddSeconds(TTL_HISTORICO_SEGUNDOS));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar/armazenar cache de histÃ³rico de entidade: EntityType={EntityType}, EntityId={EntityId}", 
                    entityType, entityId);
                // Em caso de erro no cache, buscar diretamente do banco
                return await fetchFunc();
            }
        }

        public async Task InvalidateEntityCacheAsync(string entityType, int entityId)
        {
            try
            {
                var cacheKey = $"audit:entity:{entityType}:{entityId}";
                await _cache.DeleteByKey(cacheKey);

                // TambÃ©m invalidar cache de listas que podem conter esta entidade
                // (buscar todas as chaves que comeÃ§am com "audit:logs:" e deletar)
                // Por simplicidade, vamos invalidar apenas o cache especÃ­fico da entidade
                // O cache de listas serÃ¡ invalidado automaticamente pelo TTL
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao invalidar cache de entidade: EntityType={EntityType}, EntityId={EntityId}", 
                    entityType, entityId);
            }

            await Task.CompletedTask;
        }

        private string GenerateCacheKey(string key)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                return Convert.ToBase64String(hashBytes).Replace("/", "_").Replace("+", "-").Substring(0, 32);
            }
        }
    }
}

