using SW_Utils.Auxiliar;
using SW_Utils.Enum;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IRepositoryNHAccessCenter
    {
        Task<T> Save<T>(T entity);
        Task<T> ForcedSave<T>(T entity);
        Task<decimal> GetValueFromSequenceName(string sequenceName);
        Task<IList<T>> SaveRange<T>(IList<T> entities);
        Task Remove<T>(T entity);
        void RemoveRange<T>(IList<T> entities);
        Task<T> FindById<T>(int id);
        Task<IList<T>> FindByHql<T>(string hql, params Parameter[] parameters);
        Task<IList<T>> FindByHql<T>(string hql, int pageSize, int pageNumber, params Parameter[] parameters);
        Task<IList<T>> FindBySql<T>(string sql, params Parameter[] parameters);
        Task<IList<T>> FindBySql<T>(string sql, int pageSize, int pageNumber, params Parameter[] parameters);
        CancellationToken CancellationToken { get; }
        Task ExecuteSqlCommand(string command);
        void Flush();
        void BeginTransaction();
        Task<(bool executed, Exception? exception)> CommitAsync();
        void Rollback();
        Task<(string userId, string providerKeyUser, string companyId, bool isAdm)?> GetLoggedUser();
        Task<Int64> CountTotalEntry(string sql, Parameter[] parameters);
        EnumDataBaseType DataBaseType { get; }

        Task<bool> Lock<T>(T entity, List<int> ids, NHibernate.LockMode lockMode);
        Task<bool> Lock<T>(T entity, NHibernate.LockMode lockMode);

    }
}
