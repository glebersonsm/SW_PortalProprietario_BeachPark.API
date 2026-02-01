using SW_PortalProprietario.Application.Models.AuthModels;
using SW_Utils.Auxiliar;
using SW_Utils.Enum;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IRepositoryHosted
    {
        NHibernate.IStatelessSession? CreateSession();
        Task<T> Save<T>(T entity, NHibernate.IStatelessSession? session);
        Task<T> ForcedSave<T>(T entity, NHibernate.IStatelessSession? session);
        Task<T> Insert<T>(T entity, NHibernate.IStatelessSession? session);
        Task<decimal> GetValueFromSequenceName(string sequenceName, NHibernate.IStatelessSession? session);
        Task<IList<T>> SaveRange<T>(IList<T> entities, NHibernate.IStatelessSession? session);
        void Remove<T>(T entity, NHibernate.IStatelessSession? session);
        void RemoveRange<T>(IList<T> entities, NHibernate.IStatelessSession? session);
        Task<T> FindById<T>(int id, NHibernate.IStatelessSession? session);
        Task<IList<T>> FindByHql<T>(string hql, NHibernate.IStatelessSession? session, params Parameter[] parameters);
        Task<IList<T>> FindBySql<T>(string sql, NHibernate.IStatelessSession? session, params Parameter[] parameters);
        CancellationToken CancellationToken { get; }
        void BeginTransaction(NHibernate.IStatelessSession? session);
        Task<(bool executed, Exception? exception)> CommitAsync(NHibernate.IStatelessSession? session);
        void Rollback(NHibernate.IStatelessSession? session);
        Task<TokenResultModel?> GetLoggedToken();
        EnumDataBaseType DataBaseType { get; }

        Task<bool> Lock<T>(T entity, List<int> ids, NHibernate.LockMode lockMode);
        Task<bool> Lock<T>(T entity, NHibernate.LockMode lockMode);
    }
}
