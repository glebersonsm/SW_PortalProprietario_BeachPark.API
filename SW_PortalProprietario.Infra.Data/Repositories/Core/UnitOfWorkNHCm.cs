using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_Utils.Enum;
using System.Data.Common;

namespace SW_PortalProprietario.Infra.Data.Repositories.Core
{
    public class UnitOfWorkNHCm : IUnitOfWorkNHCm
    {
        public IStatelessSession? Session { get; private set; }
        public ITransaction? Transaction { get; private set; }

        private readonly CancellationToken cancellationToken;
        public CancellationToken CancellationToken => cancellationToken;

        private readonly ISwCmSessionFactory? _sessionFactory;

        public UnitOfWorkNHCm(ISwCmSessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
            if (_sessionFactory != null)
            {
                Session = _sessionFactory.OpenStatelessSession();
                if (Session.Connection.ConnectionString.Replace(" ", "").Contains("(ADDRESS_LIST=(ADDRESS", StringComparison.CurrentCultureIgnoreCase))
                    AdjustCasePattern(EnumDataBaseType.Oracle, Session);
            }
            cancellationToken = new CancellationToken();
        }

        public void BeginTransaction()
        {
            var currentTransaction = Session?.GetCurrentTransaction();
            if (currentTransaction == null || !currentTransaction.IsActive || currentTransaction.WasCommitted && currentTransaction.WasCommitted)
                Session?.BeginTransaction();
        }

        public async Task<(bool executed, Exception? exception)> CommitAsync()
        {
            try
            {
                var currentTransaction = Session?.GetCurrentTransaction();

                if (currentTransaction != null && currentTransaction.IsActive && !currentTransaction.WasCommitted && !currentTransaction.WasRolledBack)
                {
                    await currentTransaction.CommitAsync();
                    return (true, null);
                }
                return (false, new Exception("A transação não estava ativa"));
            }
            catch (Exception err)
            {
                return (false, err);
            }
        }

        public async void Rollback()
        {
            var currentTransaction = Session?.GetCurrentTransaction();
            if (currentTransaction != null && currentTransaction.IsActive && !currentTransaction.WasCommitted && !currentTransaction.WasRolledBack)
            {
                await currentTransaction.RollbackAsync();
            }
        }

        public void PrepareCommandSql(DbCommand command)
        {
            var iTran = Session?.GetCurrentTransaction();
            if (iTran != null)
                iTran.Enlist(command);
        }

        public async void CloseConnection()
        {
            if (Session?.Connection != null && Session.Connection.State != System.Data.ConnectionState.Closed)
                await Session.Connection.CloseAsync();
        }

        public void AdjustCasePattern(EnumDataBaseType dataBaseType, IStatelessSession? session)
        {
            // Fix CS8600: Accept nullable parameter and validate properly
            session ??= Session;

            if (session == null)
                throw new InvalidOperationException("Session is null");

            if (dataBaseType == EnumDataBaseType.Oracle)
            {
                const string alterSessionSql = "ALTER SESSION SET NLS_COMP = 'LINGUISTIC' NLS_SORT = 'BINARY_AI'";
                session.CreateSQLQuery(alterSessionSql).ExecuteUpdate();
            }
        }

        public void Dispose()
        {
            CloseConnection();
            
            // CRITICAL FIX: Properly dispose Session to return connection to pool
            try
            {
                var currentTransaction = Session?.GetCurrentTransaction();
                if (currentTransaction != null && currentTransaction.IsActive && !currentTransaction.WasRolledBack)
                {
                    currentTransaction.Rollback();
                }
            }
            catch { /* Ignore rollback errors during dispose */ }
            
            Session?.Dispose();
            Session = null;
        }
    }
}
