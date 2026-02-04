using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_Utils.Auxiliar;
using SW_Utils.Enum;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IRepositoryNHCm
    {
        NHibernate.IStatelessSession? CreateSession();
        Task<T> Save<T>(T entity, NHibernate.IStatelessSession? session = null);
        Task<T> ForcedSave<T>(T entity, NHibernate.IStatelessSession? session = null);
        Task<T> Insert<T>(T entity, NHibernate.IStatelessSession? session = null);
        Task<decimal> GetValueFromSequenceName(string sequenceName, NHibernate.IStatelessSession? session = null);
        Task<IList<T>> SaveRange<T>(IList<T> entities, NHibernate.IStatelessSession? session = null);
        void Remove<T>(T entity, NHibernate.IStatelessSession? session = null);
        void RemoveRange<T>(IList<T> entities, NHibernate.IStatelessSession? session = null);
        Task<T> FindById<T>(int id, NHibernate.IStatelessSession? session = null);
        Task<IList<T>> FindBySql<T>(string sql, int pageSize, int pageNumber, params Parameter[] parameters);
        Task<IList<T>> FindByHql<T>(string hql, NHibernate.IStatelessSession? session = null, params Parameter[] parameters);
        Task<IList<T>> FindByHql<T>(string hql, params Parameter[] parameters);
        Task<IList<T>> FindBySql<T>(string sql, NHibernate.IStatelessSession? session = null, params Parameter[] parameters);
        Task<IList<T>> FindBySql<T>(string sql, params Parameter[] parameters);
        Task<Int64> CountTotalEntry(string sql, NHibernate.IStatelessSession? session = null, Parameter[]? parameters = null);
        CancellationToken CancellationToken { get; }
        void BeginTransaction(NHibernate.IStatelessSession? session = null);
        Task<(bool executed, Exception? exception)> CommitAsync(NHibernate.IStatelessSession? session = null);
        void Rollback(NHibernate.IStatelessSession? session = null);
        Task<(string userId, string providerKeyUser, string companyId, bool isAdm)?> GetLoggedUser();
        EnumDataBaseType DataBaseType { get; }

        Task<bool> Lock<T>(T entity, List<int> ids, NHibernate.LockMode lockMode);
        Task<bool> Lock<T>(T entity, NHibernate.LockMode lockMode);
        bool IsAdm { get; }
        Task<ParametroSistemaViewModel?> GetParametroSistemaViewModel();
        Task ExecuteSqlCommand(string command);
        Task<string> GetToken();

    }
}
