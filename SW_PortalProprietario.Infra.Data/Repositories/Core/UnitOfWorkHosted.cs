using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_Utils.Enum;
using System.Data.Common;

namespace SW_PortalProprietario.Infra.Data.Repositories.Core
{
    public class UnitOfWorkHosted : IUnitOfWorkHosted, IDisposable
    {
        public IStatelessSession? Session { get; private set; }
        public ITransaction? Transaction { get; private set; }

        private readonly CancellationToken cancellationToken;
        public CancellationToken CancellationToken => cancellationToken;

        private readonly ISwSessionFactoryHosted? _sessionFactory;

        public UnitOfWorkHosted(ISwSessionFactoryHosted sessionFactory)
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
            var currentTransaction = Session.GetCurrentTransaction();
            if (currentTransaction == null || !currentTransaction.IsActive || currentTransaction.WasCommitted && currentTransaction.WasCommitted)
                Session?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

        }

        public async Task<(bool executed, Exception? exception)> CommitAsync()
        {
            try
            {
                var currentTransaction = Session.GetCurrentTransaction();

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
            var currentTransaction = Session.GetCurrentTransaction();
            if (currentTransaction != null && currentTransaction.IsActive && !currentTransaction.WasCommitted && !currentTransaction.WasRolledBack)
            {
                await currentTransaction.RollbackAsync();
            }
        }

        public async void CloseConnection()
        {
            if (Session != null && Session.Connection != null && Session.Connection.State != System.Data.ConnectionState.Closed)
                await Session.Connection.CloseAsync();
        }

        public void PrepareCommandSql(DbCommand command, IStatelessSession? session = null)
        {
            if (session == null)
            {
                var iTran = Session.GetCurrentTransaction();
                if (iTran != null)
                    Session.GetCurrentTransaction().Enlist(command);
            }
            else
            {
                var iTran = session.GetCurrentTransaction();
                if (iTran != null)
                    session.GetCurrentTransaction().Enlist(command);
            }
        }

        public void Dispose()
        {
            CloseConnection();
        }

        public IStatelessSession? CreateSession()
        {
            if (_sessionFactory != null)
            {
                return _sessionFactory.OpenStatelessSession();
            }
            else throw new InvalidOperationException("SessonFactory is null");
        }

        public void AdjustCasePattern(EnumDataBaseType dataBaseType, IStatelessSession session)
        {
            if (session == null)
                session = Session;

            if (session == null)
                throw new InvalidOperationException("Session is null");


            if (dataBaseType == EnumDataBaseType.Oracle)
            {
                const string alterSessionSql = "ALTER SESSION SET NLS_COMP = 'LINGUISTIC' NLS_SORT = 'BINARY_AI'";
                session.CreateSQLQuery(alterSessionSql).ExecuteUpdate();
            }
        }
    }
}
