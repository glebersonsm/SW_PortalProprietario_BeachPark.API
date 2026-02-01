using AccessCenterDomain;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core;
using SW_Utils.Auxiliar;
using SW_Utils.Enum;
using System.Diagnostics;

namespace SW_PortalProprietario.Infra.Data.Repositories.Core
{
    public class RepositoryHosted : IRepositoryHosted
    {
        private readonly IUnitOfWorkHosted _unitOfWork;
        private readonly IAuthenticatedBaseHostedService _authenticatedBaseHostedService;
        private readonly ILogger<RepositoryHosted> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _forceRollback = false;
        public IStatelessSession? Session => _unitOfWork.Session;

        public CancellationToken CancellationToken => _unitOfWork.CancellationToken;


        public RepositoryHosted(IUnitOfWorkHosted unitOfWork,
            IAuthenticatedBaseHostedService authenticatedBaseHostedService,
            ILogger<RepositoryHosted> logger,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
            _unitOfWork = unitOfWork;
            _authenticatedBaseHostedService = authenticatedBaseHostedService;
            _logger = logger;
            _configuration = configuration;
            _forceRollback = _configuration.GetValue<bool>("ConnectionStrings:ForceRollback", false) && Debugger.IsAttached;
        }

        public async Task<T> FindById<T>(int id, IStatelessSession? session)
        {
            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                return await Session.GetAsync<T>(id, CancellationToken);
            }
            else
            {
                return await session.GetAsync<T>(id, CancellationToken);
            }
        }

        public void Remove<T>(T entity, IStatelessSession? session)
        {
            var usuario = _authenticatedBaseHostedService.GetLoggedUserAsync().Result;
            if (entity is EntityBaseCore objEntity)
            {
                if (session == null)
                {
                    ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                    objEntity.UsuarioRemocaoId = usuario?.UserId;
                    Session.DeleteAsync(entity, CancellationToken);
                }
                else
                {
                    objEntity.UsuarioRemocaoId = usuario?.UserId;
                    session.DeleteAsync(entity, CancellationToken);
                }
            }
            else throw new ArgumentException($"Objeto: {entity} não herda da EntityBaseCore");
        }

        public async void RemoveRange<T>(IList<T> entities, IStatelessSession? session)
        {

            var usuario = await _authenticatedBaseHostedService.GetLoggedUserAsync();
            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                foreach (var entity in entities.Cast<EntityBaseCore>())
                {
                    entity.UsuarioRemocaoId = usuario?.UserId;
                    await Session.DeleteAsync(entity, CancellationToken);
                }
            }
            else
            {
                foreach (var entity in entities.Cast<EntityBaseCore>())
                {
                    entity.UsuarioRemocaoId = usuario?.UserId;
                    await session.DeleteAsync(entity, CancellationToken);
                }
            }

        }

        public async Task<T> Save<T>(T entity, IStatelessSession? session)
        {

            var usuario = await _authenticatedBaseHostedService.GetLoggedUserAsync();

            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                if (entity is EntityBaseCore objEntity)
                {
                    if (objEntity.Id == 0)
                    {
                        if (objEntity.UsuarioCriacao == null)
                            objEntity.UsuarioCriacao = usuario?.UserId;

                        objEntity.DataHoraCriacao = DateTime.Now;
                        objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                        await Session.InsertAsync(objEntity, CancellationToken);
                    }
                    else
                    {
                        if (objEntity.UsuarioAlteracao == null)
                            objEntity.UsuarioAlteracao = usuario?.UserId;
                        objEntity.DataHoraAlteracao = DateTime.Now;
                        if (string.IsNullOrEmpty(objEntity.ObjectGuid))
                            objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                        await Session.UpdateAsync(objEntity, CancellationToken);
                    }
                }
            }
            else
            {
                if (entity is EntityBaseCore objEntity)
                {
                    if (objEntity.Id == 0)
                    {
                        if (objEntity.UsuarioCriacao == null)
                            objEntity.UsuarioCriacao = usuario?.UserId;
                        objEntity.DataHoraCriacao = DateTime.Now;
                        objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                        await session.InsertAsync(objEntity, CancellationToken);
                    }
                    else
                    {
                        if (objEntity.UsuarioAlteracao == null)
                            objEntity.UsuarioAlteracao = usuario?.UserId;
                        objEntity.DataHoraAlteracao = DateTime.Now;
                        if (string.IsNullOrEmpty(objEntity.ObjectGuid))
                            objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                        await session.UpdateAsync(objEntity, CancellationToken);
                    }
                }
            }
            return entity;
        }

        public async Task<IList<T>> SaveRange<T>(IList<T> entities, IStatelessSession? session)
        {
            var usuario = await _authenticatedBaseHostedService.GetLoggedUserAsync();

            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));

                foreach (var entity in entities.Cast<EntityBaseCore>())
                {
                    if (entity.Id == 0)
                    {
                        entity.UsuarioCriacao = usuario?.UserId;
                        entity.DataHoraCriacao = DateTime.Now;
                        entity.ObjectGuid = $"{Guid.NewGuid()}";
                        await Session.InsertAsync(entity, CancellationToken);
                    }
                    else
                    {
                        entity.UsuarioAlteracao = usuario?.UserId;
                        entity.DataHoraAlteracao = DateTime.Now;
                        if (string.IsNullOrEmpty(entity.ObjectGuid))
                            entity.ObjectGuid = $"{Guid.NewGuid()}";
                        await Session.UpdateAsync(entity, CancellationToken);
                    }

                }
            }
            else
            {
                foreach (var entity in entities.Cast<EntityBaseCore>())
                {
                    if (entity.Id == 0)
                    {
                        entity.UsuarioCriacao = usuario?.UserId;
                        entity.DataHoraCriacao = DateTime.Now;
                        entity.ObjectGuid = $"{Guid.NewGuid()}";
                        await session.InsertAsync(entity, CancellationToken);
                    }
                    else
                    {
                        entity.UsuarioAlteracao = usuario?.UserId;
                        entity.DataHoraAlteracao = DateTime.Now;
                        if (string.IsNullOrEmpty(entity.ObjectGuid))
                            entity.ObjectGuid = $"{Guid.NewGuid()}";
                        await session.UpdateAsync(entity, CancellationToken);
                    }

                }
            }

            return entities;
        }

        public async Task<IList<T>> FindByHql<T>(string hql, IStatelessSession? session, params Parameter[] parameters)
        {
            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                var query = Session.CreateQuery(hql);
                if (parameters != null)
                    SetParameters(parameters, query);
                return await query.ListAsync<T>();
            }
            else
            {
                var query = session.CreateQuery(hql);
                if (parameters != null)
                    SetParameters(parameters, query);
                return await query.ListAsync<T>();
            }
        }

        public async Task<IList<T>> FindBySql<T>(string sql, IStatelessSession? session, params Parameter[] parameters)
        {
            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                sql = NormalizaParameterName(sql, parameters);

                var dbCommand = Session.Connection.CreateCommand();
                dbCommand.CommandText = sql;
                _unitOfWork.PrepareCommandSql(dbCommand, Session);

                var dados = await Session.Connection.QueryAsync<T>(sql, SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters), dbCommand.Transaction);
                return dados.ToList();
            }
            else
            {
                sql = NormalizaParameterName(sql, parameters);

                var dbCommand = session.Connection.CreateCommand();
                dbCommand.CommandText = sql;
                _unitOfWork.PrepareCommandSql(dbCommand, session);

                var dados = await session.Connection.QueryAsync<T>(sql, SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters), dbCommand.Transaction);
                return dados.ToList();
            }
        }

        private static void SetParameters(Parameter[] parameters, IQuery query)
        {
            foreach (var item in parameters)
            {
                query.SetParameter(item.Name, item.Value);
            }
        }

        public async Task<decimal> GetValueFromSequenceName(string sequenceName, IStatelessSession? session)
        {
            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                var valueRetorno = await Session.CreateSQLQuery($"Select {sequenceName}.NextVal From Dual").UniqueResultAsync();
                return (decimal)valueRetorno;
            }
            else
            {
                var valueRetorno = await session.CreateSQLQuery($"Select {sequenceName}.NextVal From Dual").UniqueResultAsync();
                return (decimal)valueRetorno;
            }
        }

        public void BeginTransaction(IStatelessSession? session)
        {
            try
            {
                if (session == null)
                {
                    _unitOfWork?.BeginTransaction();
                }
                else
                {
                    var currentTransaction = session.GetCurrentTransaction();
                    if (currentTransaction == null || !currentTransaction.IsActive || currentTransaction.WasCommitted && currentTransaction.WasCommitted)
                        session?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message, err.StackTrace, err.Source);
            }
        }

        public async Task<(bool executed, Exception? exception)> CommitAsync(IStatelessSession? session)
        {
            try
            {
                if (session == null)
                {
                    if (_forceRollback)
                    {
                        _unitOfWork.Rollback();
                        return (true, null); ;
                    }
                    else return await _unitOfWork.CommitAsync();
                }
                else
                {
                    var token = CancellationToken;
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        var currentTransaction = session.GetCurrentTransaction();

                        if (currentTransaction != null && currentTransaction.IsActive && !currentTransaction.WasCommitted && !currentTransaction.WasRolledBack)
                        {
                            if (_forceRollback)
                            {
                                currentTransaction.Rollback();
                                return (true,null);
                            }
                            else 
                            { 
                                await currentTransaction.CommitAsync();
                                return (true, null);
                            }

                        }
                        return (false, new Exception("A transação não estava ativa"));
                    }
                    catch (Exception err)
                    {
                        return (false, err);
                    }
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message, err.StackTrace, err.Source);
                return (false, err);
            }
        }

        public async void Rollback(IStatelessSession? session)
        {
            try
            {
                if (session == null)
                {
                    _unitOfWork?.Rollback();
                }
                else
                {
                    var currentTransaction = session.GetCurrentTransaction();
                    if (currentTransaction != null && currentTransaction.IsActive && !currentTransaction.WasCommitted && !currentTransaction.WasRolledBack)
                    {
                        await currentTransaction.RollbackAsync();
                    }
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message, err.StackTrace, err.Source);
            }
        }

        public async Task<TokenResultModel?> GetLoggedToken()
        {
            return await _authenticatedBaseHostedService.GetLoggedUserAsync();
        }

        public async Task<T> ForcedSave<T>(T entity, IStatelessSession? session)
        {

            var usuario = await _authenticatedBaseHostedService.GetLoggedUserAsync(false);
            if (usuario != null && usuario.UserId.GetValueOrDefault(0) > 0)
                throw new Exception("O ForcedSave só pode ser utilizado quando não existir um usuário logado!");

            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));

                if (entity is EntityBaseCore objEntity)
                {
                    if (objEntity.Id == 0)
                    {
                        if (objEntity.UsuarioCriacao == null)
                            objEntity.UsuarioCriacao = usuario?.UserId;
                        objEntity.DataHoraCriacao = DateTime.Now;
                        objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                        await Session.InsertAsync(objEntity, CancellationToken);
                    }
                    else
                    {
                        if (objEntity.UsuarioAlteracao == null)
                            objEntity.UsuarioAlteracao = usuario?.UserId;

                        objEntity.DataHoraAlteracao = DateTime.Now;
                        if (string.IsNullOrEmpty(objEntity.ObjectGuid))
                            objEntity.ObjectGuid = $"{Guid.NewGuid()}";

                        await Session.UpdateAsync(objEntity, CancellationToken);
                    }
                }
            }
            else
            {
                if (entity is EntityBaseCore objEntity)
                {
                    if (objEntity.Id == 0)
                    {
                        if (objEntity.UsuarioCriacao == null)
                            objEntity.UsuarioCriacao = usuario?.UserId;
                        objEntity.DataHoraCriacao = DateTime.Now;
                        objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                        await session.InsertAsync(objEntity, CancellationToken);
                    }
                    else
                    {
                        if (objEntity.UsuarioAlteracao == null)
                            objEntity.UsuarioAlteracao = usuario?.UserId;

                        objEntity.DataHoraAlteracao = DateTime.Now;
                        if (string.IsNullOrEmpty(objEntity.ObjectGuid))
                            objEntity.ObjectGuid = $"{Guid.NewGuid()}";

                        await session.UpdateAsync(objEntity, CancellationToken);
                    }
                }
            }
            return entity;
        }

        public async Task<T> Insert<T>(T entity, IStatelessSession? session)
        {
            if (session == null)
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));

                if (entity is EntityBaseCore objEntity)
                {
                    objEntity.DataHoraAlteracao = DateTime.Now;
                    objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await Session.InsertAsync(objEntity, CancellationToken);
                }
            }
            else
            {
                if (entity is EntityBaseCore objEntity)
                {
                    objEntity.DataHoraAlteracao = DateTime.Now;
                    objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await session.InsertAsync(objEntity, CancellationToken);
                }
            }

            return entity;
        }

        private string NormalizaParameterName(string sql, Parameter[] parameters)
        {
            if (DataBaseType == EnumDataBaseType.SqlServer && parameters != null && parameters.Any())
            {
                foreach (var item in parameters)
                {
                    var doReplase = sql.Contains($":{item.Name}");
                    if (doReplase)
                        sql = sql.Replace($":{item.Name}", $"@{item.Name}");
                }
            }

            return sql;
        }

        public IStatelessSession? CreateSession()
        {
            try
            {
                return _unitOfWork.CreateSession();
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message, err.StackTrace, err.Source);
            }
            return null;
        }

        public EnumDataBaseType DataBaseType
        {
            get
            {
                var result = EnumDataBaseType.SqLite;

                if (Session?.Connection != null)
                {
                    var dataBaseType = Session?.Connection.GetType().Name.ToLower();
                    if (!string.IsNullOrEmpty(dataBaseType))
                    {
                        if (dataBaseType.Equals("sqlconnection", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return EnumDataBaseType.SqlServer;
                        }
                        else if (dataBaseType.Equals("npgsqlconnection", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return EnumDataBaseType.PostgreSql;
                        }
                        else if (dataBaseType.Equals("mysqlconnection", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return EnumDataBaseType.MySql;
                        }
                        else if (dataBaseType.Equals("sqliteconnection", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return EnumDataBaseType.SqLite;
                        }
                        else if (dataBaseType.Equals("oracleconnection", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return EnumDataBaseType.Oracle;
                        }
                        else
                        {
                            return EnumDataBaseType.Indefinido;
                        }
                    }
                }

                return result;
            }

        }

        public async Task<bool> Lock<T>(T entity, LockMode? lockMode)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            if (entity is EntityBase objEntity)
            {
                await Session.GetAsync<T>(objEntity.Id, lockMode ?? LockMode.UpgradeNoWait);
                return true;
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> Lock<T>(T entity, List<int> list, LockMode? lockMode)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                var sql = $"Select ob From {entity.GetType().Name} ob Where ob.Id in ({string.Join(",", list)})";
                var result = Session.CreateQuery(sql).SetLockMode("ob", lockMode ?? LockMode.UpgradeNoWait).List();
                return await Task.FromResult(true);
            }
            catch (Exception err)
            {
                throw;
            }
        }
    }

}
