
using SW_Utils.Enum;
using System.Data;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IUnitOfWorkDapper
    {
        IDbTransaction? Transaction { get; }
        IDbConnection? Connection { get; }
        void BeginTransaction();
        Task<(bool executed, Exception? exception)> CommitAsync();
        void Rollback();
        CancellationToken CancellationToken { get; }
        void AdjustCasePattern(EnumDataBaseType dataBaseType);
    }
}
