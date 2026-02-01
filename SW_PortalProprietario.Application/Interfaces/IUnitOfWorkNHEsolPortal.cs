using NHibernate;
using SW_Utils.Enum;
using System.Data.Common;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IUnitOfWorkNHEsolPortal
    {
        IStatelessSession? Session { get; }
        void BeginTransaction();
        void PrepareCommandSql(DbCommand command);
        Task<(bool executed, Exception? exception)> CommitAsync();
        void Rollback();
        CancellationToken CancellationToken { get; }
        void AdjustCasePattern(EnumDataBaseType dataBaseType, IStatelessSession session);
    }
}
