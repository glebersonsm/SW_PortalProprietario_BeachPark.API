namespace SW_PortalProprietario.Application.Interfaces
{
    public interface ICacheStore
    {
        Task<bool> DeleteByKey(string key, int dbId = 0, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string key, int dbId = 0, CancellationToken cancellationToken = default);
        Task<IDictionary<string, T>> GetListAsync<T>(List<string> keys, int dbId = 0, CancellationToken cancellationToken = default);
        Task<List<string>> GetKeysAsync<T>(string contains, int dbId = 0, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(string key, object value, DateTimeOffset? valideFor, int dbId = 0, CancellationToken cancellationToken = default);
        Task<bool> AddRangeAsync(string key, IDictionary<string, object> itens, DateTimeOffset? valideFor, int dbId = 0, CancellationToken cancellationToken = default);

    }
}
