using StackExchange.Redis.Extensions.Core.Abstractions;
using SW_PortalProprietario.Application.Interfaces;
using System.Text.Json;

namespace SW_PortalProprietario.Infra.Data.Caching
{
    public class CacheStore : ICacheStore
    {
        private readonly IRedisClient _cache;
        public CacheStore(IRedisClient cache)
        {
            _cache = cache;
        }

        public async Task<bool> AddAsync(string key, object value, DateTimeOffset? valideFor, int dbId = 0, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(value);
            await _cache.GetDb(dbId).AddAsync(key, json, valideFor ?? DateTimeOffset.Now.AddDays(1));
            return true;
        }

        private async Task<bool> AddAsyncExecute(string key, string json, DateTimeOffset? valideFor, int dbId = 0, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _cache.GetDb(dbId).AddAsync(key, json, valideFor ?? DateTimeOffset.Now.AddDays(1));
            return true;
        }

        public async Task<bool> AddRangeAsync(string key, IDictionary<string, object> itens, DateTimeOffset? valideFor, int dbId = 0, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var kvp in itens)
            {
                var json = JsonSerializer.Serialize(kvp.Value);
                await _cache.GetDb(dbId).AddAsync(kvp.Key, json, valideFor ?? DateTimeOffset.Now.AddDays(1));

            }
            return true;
        }

        public async Task<bool> DeleteByKey(string key, int dbId = 0, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _cache.GetDb(dbId).RemoveAsync(key);
            return true;
        }

        public async Task<T?> GetAsync<T>(string key, int dbId = 0, CancellationToken cancellationToken = default)
        {
            var strObject = await _cache.GetDb(dbId).GetAsync<string>(key);
            if (!string.IsNullOrEmpty(strObject))
            {
                var objectT = JsonSerializer.Deserialize<T>(strObject);
                return (T?)objectT;
            }
            else return default;
        }

        public async Task<IDictionary<string, T>> GetListAsync<T>(List<string> keys, int dbId = 0, CancellationToken cancellationToken = default)
        {
            IDictionary<string, T> result = new Dictionary<string, T>();
            if (keys.Any())
            {
                foreach (var key in keys)
                {
                    var strObject = await _cache.GetDb(dbId).GetAsync<string>(key);
                    if (!string.IsNullOrEmpty(strObject))
                    {
                        var objectT = JsonSerializer.Deserialize<T>(strObject);
                        result.Add(key, objectT);
                    }

                }
            }
            return result;
        }

        public async Task<List<string>> GetKeysAsync<T>(string contains, int dbId = 0, CancellationToken cancellationToken = default)
        {
            var resul = await _cache.GetDb(dbId).HashGetAllAsync<string>(contains,StackExchange.Redis.CommandFlags.None);
            return (await _cache.GetDb(dbId).SearchKeysAsync(pattern: $"{contains}*")).ToList();

        }
    }
}
