using NHibernate;
using SW_Utils.Enum;
using System.Data.Common;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IUnitOfWorkHosted
    {
        NHibernate.IStatelessSession? Session { get; }
        void BeginTransaction();
        Task<(bool executed, Exception? exception)> CommitAsync();
        void Rollback();
        IStatelessSession? CreateSession();
        void PrepareCommandSql(DbCommand command, IStatelessSession? session = null);
        void AdjustCasePattern(EnumDataBaseType dataBaseType, IStatelessSession session);

        CancellationToken CancellationToken { get; }
    }
}
