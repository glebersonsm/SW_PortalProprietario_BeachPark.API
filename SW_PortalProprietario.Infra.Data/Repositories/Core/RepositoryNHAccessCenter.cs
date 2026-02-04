using AccessCenterDomain;
using Dapper;
using Microsoft.Extensions.Configuration;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core;
using SW_Utils.Auxiliar;
using SW_Utils.Enum;
using SW_Utils.Functions;
using System.Diagnostics;

namespace SW_PortalProprietario.Infra.Data.Repositories.Core
{
    public class RepositoryNHAccessCenter : IRepositoryNHAccessCenter
    {
        private readonly IUnitOfWorkNHAccessCenter _unitOfWork;
        private readonly IAuthenticatedBaseService _authenticatedBaseService;
        private readonly IConfiguration _configuration;
        private (string userId, string providerKeyUser, string companyId, bool isAdm)? _loggedUserId;
        public IStatelessSession? Session => _unitOfWork.Session;
        private readonly bool _forceRollback = false;

        public CancellationToken CancellationToken => _unitOfWork.CancellationToken;

        public bool IsAdm
        {
            get
            {
                if (_loggedUserId == null)
                    _loggedUserId = GetLoggedUser().Result;

                return _loggedUserId != null && _loggedUserId.Value.isAdm;
            }
        }

        public RepositoryNHAccessCenter(IUnitOfWorkNHAccessCenter unitOfWork,
            IAuthenticatedBaseService authenticatedBaseService,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
            _unitOfWork = unitOfWork;
            _authenticatedBaseService = authenticatedBaseService;
            _configuration = configuration;
            _forceRollback = _configuration.GetValue<bool>("ConnectionStrings:ForceRollback", false) && Debugger.IsAttached;
        }

        public async Task<T> FindById<T>(int id)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            return await Session.GetAsync<T>(id, CancellationToken);
        }

        public async Task Remove<T>(T entity)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));

            await Session.DeleteAsync(entity, CancellationToken);
        }

        public async void RemoveRange<T>(IList<T> entities)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            foreach (var entity in entities)
            {
                await Session.DeleteAsync(entity, CancellationToken);
            }
        }

        public async Task<T> Save<T>(T entity)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            if (entity is EntityBase objEntity)
            {

                if (entity is IEntityValidateCore validate)
                    await validate.SaveValidate();

                if (objEntity.Id.GetValueOrDefault(0) == 0)
                {
                    objEntity.DataHoraCriacao = DateTime.Now;
                    await Session.InsertAsync(objEntity, CancellationToken);

                }
                else
                {
                    objEntity.DataHoraAlteracao = DateTime.Now;
                    await Session.UpdateAsync(objEntity, CancellationToken);
                }
            }
            return entity;
        }

        public async Task<IList<T>> SaveRange<T>(IList<T> entities)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));

            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            foreach (var entity in entities.Cast<EntityBase>())
            {
                if (entity is IEntityValidateCore validate)
                    await validate.SaveValidate();

                if (entity.Id.GetValueOrDefault(0) == 0)
                {
                    entity.DataHoraCriacao = DateTime.Now;
                    await Session.InsertAsync(entity, CancellationToken);
                }
                else
                {
                    entity.DataHoraAlteracao = DateTime.Now;
                    await Session.UpdateAsync(entity, CancellationToken);
                }

            }
            return entities;
        }

        public async Task<IList<T>> FindByHql<T>(string hql, params Parameter[] parameters)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            var query = Session.CreateQuery(hql);
            if (parameters != null)
                SetParameters(parameters, query);


            return await query.ListAsync<T>(CancellationToken);
        }

        public async Task<IList<T>> FindByHql<T>(string hql, int pageSize, int pageNumber, params Parameter[] parameters)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            var query = Session.CreateQuery(hql)
                .SetFirstResult((pageNumber > 0 ? pageNumber - 1 : 0) * pageSize)
                .SetMaxResults(pageSize);

            if (parameters != null)
                SetParameters(parameters, query);

            return await query.ListAsync<T>(CancellationToken);
        }

        public async Task<IList<T>> FindBySql<T>(string sql, params Parameter[] parameters)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                Flush();

                var dbCommand = Session.Connection.CreateCommand();
                sql = NormalizaParameterName(sql, parameters);
                sql = RepositoryUtils.NormalizeFunctions(DataBaseType, sql);

                dbCommand.CommandText = sql;
                _unitOfWork.PrepareCommandSql(dbCommand);

                var dados = await Session.Connection.QueryAsync<T>(sql, SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters), dbCommand.Transaction);
                return dados.ToList();
            }
            catch (Exception err)
            {

                throw;
            }
        }


        public async Task<IList<T>> FindBySql<T>(string sql, int pageSize, int pageNumber, params Parameter[] parameters)
        {
            try
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                ArgumentNullException.ThrowIfNull(Session?.Connection, nameof(Session.Connection));

                if (!sql.Contains("order by", StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentException("A consulta deve conter ordenação para utilização de paginação.");

                sql = NormalizaParameterName(sql, parameters);
                sql = RepositoryUtils.NormalizeFunctions(DataBaseType, sql);

                sql = PaginationHelper.GetPaginatedQuery(sql, DataBaseType, pageNumber, pageSize);

                Flush();

                var dbCommand = Session.Connection.CreateCommand();
                dbCommand.CommandText = sql;
                _unitOfWork.PrepareCommandSql(dbCommand);

                var dados = await Session.Connection.QueryAsync<T>(sql, SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters), dbCommand.Transaction);
                return dados.AsList();
            }
            catch (Exception err)
            {

                throw;
            }
        }

        private static void SetParameters(Parameter[] parameters, IQuery query)
        {
            foreach (var item in parameters)
            {
                query.SetParameter(item.Name, item.Value);
            }
        }

        public async Task<decimal> GetValueFromSequenceName(string sequenceName)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            var valueRetorno = await Session.CreateSQLQuery($"Select {sequenceName}.NextVal From Dual").UniqueResultAsync();
            return (decimal)valueRetorno;
        }

        public void BeginTransaction()
        {
            _unitOfWork?.BeginTransaction();
        }

        public async Task<(bool executed, Exception? exception)> CommitAsync()
        {
            if (_forceRollback)
            {
                _unitOfWork.Rollback();
                return (true, null); ;
            }
            else return await _unitOfWork.CommitAsync();
        }

        public void Rollback()
        {
            _unitOfWork?.Rollback();
        }

        public async Task<T> ForcedSave<T>(T entity)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));

            var usuario = await _authenticatedBaseService.GetLoggedUserAsync(false);
            if (!string.IsNullOrEmpty(usuario.userId))
                throw new Exception("O ForcedSave só pode ser utilizado quando não existir um usuário logado!");

            if (entity is EntityBaseCore objEntity)
            {
                if (objEntity.Id == 0)
                {
                    objEntity.DataHoraCriacao = DateTime.Now;
                    objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await Session.InsertAsync(objEntity, CancellationToken);
                }
                else
                {
                    objEntity.DataHoraAlteracao = DateTime.Now;
                    if (string.IsNullOrEmpty(objEntity.ObjectGuid))
                        objEntity.ObjectGuid = $"{Guid.NewGuid()}";

                    await Session.UpdateAsync(objEntity, CancellationToken);
                }
            }
            return entity;
        }

        public void Flush()
        {
            Session?.GetSessionImplementation()?.Flush();
        }

        public async Task ExecuteSqlCommand(string command)
        {
            Flush();

            var dbCommand = Session.Connection.CreateCommand();
            command = RepositoryUtils.NormalizeFunctions(DataBaseType, command);

            dbCommand.CommandText = command;
            _unitOfWork.PrepareCommandSql(dbCommand);

            await Session?.Connection?.ExecuteAsync(command, null, dbCommand.Transaction);
        }


        public async Task<(string userId, string providerKeyUser, string companyId, bool isAdm)?> GetLoggedUser()
        {
            if (_loggedUserId == null || string.IsNullOrEmpty(_loggedUserId.Value.userId))
            {
                _loggedUserId = await _authenticatedBaseService.GetLoggedUserAsync();

            }
            return await Task.FromResult(_loggedUserId);
        }

        public async Task<Int64> CountTotalEntry(string sql, params Parameter[] parameters)
        {
            try
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                ArgumentNullException.ThrowIfNull(Session?.Connection, nameof(Session.Connection));

                sql = NormalizaParameterName(sql, parameters);
                sql = RepositoryUtils.NormalizeFunctions(DataBaseType, sql);

                var sqlPronto = $"Select COUNT(1) FROM ({sql}) a ";

                var valueRetorno = await Session.Connection?.ExecuteScalarAsync($"{sqlPronto}", SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters));

                return Convert.ToInt64(valueRetorno);
            }
            catch (Exception err)
            {
                throw;
            }

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

        public IStatelessSession? CreateSession()
        {
            throw new NotImplementedException("CreateSession não está disponível para este repositório");
        }

        public async Task<T> Save<T>(T entity, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            if (entity is EntityBase objEntity)
            {
                if (entity is IEntityValidateCore validate)
                    await validate.SaveValidate();

                if (objEntity.Id.GetValueOrDefault(0) == 0)
                {
                    objEntity.DataHoraCriacao = DateTime.Now;
                    await sessionToUse.InsertAsync(objEntity, CancellationToken);
                }
                else
                {
                    objEntity.DataHoraAlteracao = DateTime.Now;
                    await sessionToUse.UpdateAsync(objEntity, CancellationToken);
                }
            }
            return entity;
        }

        public async Task<T> ForcedSave<T>(T entity, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));

            var usuario = await _authenticatedBaseService.GetLoggedUserAsync(false);
            if (!string.IsNullOrEmpty(usuario.userId))
                throw new Exception("O ForcedSave só pode ser utilizado quando não existir um usuário logado!");

            if (entity is EntityBaseCore objEntity)
            {
                if (objEntity.Id == 0)
                {
                    objEntity.DataHoraCriacao = DateTime.Now;
                    objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await sessionToUse.InsertAsync(objEntity, CancellationToken);
                }
                else
                {
                    objEntity.DataHoraAlteracao = DateTime.Now;
                    if (string.IsNullOrEmpty(objEntity.ObjectGuid))
                        objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await sessionToUse.UpdateAsync(objEntity, CancellationToken);
                }
            }
            return entity;
        }

        public async Task<T> Insert<T>(T entity, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));

            if (entity is EntityBase objEntity)
            {
                objEntity.DataHoraAlteracao = DateTime.Now;
                await sessionToUse.InsertAsync(objEntity, CancellationToken);
            }

            return entity;
        }

        public async Task<decimal> GetValueFromSequenceName(string sequenceName, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var valueRetorno = await sessionToUse.CreateSQLQuery($"Select {sequenceName}.NextVal From Dual").UniqueResultAsync();
            return (decimal)valueRetorno;
        }

        public async Task<IList<T>> SaveRange<T>(IList<T> entities, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));

            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            foreach (var entity in entities.Cast<EntityBase>())
            {
                if (entity is IEntityValidateCore validate)
                    await validate.SaveValidate();

                if (entity.Id.GetValueOrDefault(0) == 0)
                {
                    entity.DataHoraCriacao = DateTime.Now;
                    await sessionToUse.InsertAsync(entity, CancellationToken);
                }
                else
                {
                    entity.DataHoraAlteracao = DateTime.Now;
                    await sessionToUse.UpdateAsync(entity, CancellationToken);
                }
            }
            return entities;
        }

        public void Remove<T>(T entity, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            sessionToUse.DeleteAsync(entity, CancellationToken);
        }

        public async void RemoveRange<T>(IList<T> entities, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            foreach (var entity in entities)
            {
                await sessionToUse.DeleteAsync(entity, CancellationToken);
            }
        }

        public async Task<T> FindById<T>(int id, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            return await sessionToUse.GetAsync<T>(id, CancellationToken);
        }

        public async Task<IList<T>> FindByHql<T>(string hql, IStatelessSession? session = null, params Parameter[] parameters)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var query = sessionToUse.CreateQuery(hql);
            if (parameters != null)
                SetParameters(parameters, query);
            return await query.ListAsync<T>(CancellationToken);
        }

        public async Task<IList<T>> FindBySql<T>(string sql, IStatelessSession? session = null, params Parameter[] parameters)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            sql = NormalizaParameterName(sql, parameters);

            var dbCommand = sessionToUse.Connection.CreateCommand();
            dbCommand.CommandText = sql;
            _unitOfWork.PrepareCommandSql(dbCommand);

            var dados = await sessionToUse.Connection.QueryAsync<T>(sql, SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters), dbCommand.Transaction);
            return dados.ToList();
        }

        public async Task<long> CountTotalEntry(string sql, IStatelessSession? session = null, Parameter[]? parameters = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            sql = NormalizaParameterName(sql, parameters);
            
            var sqlPronto = $"Select COUNT(1) FROM ({sql}) a ";
            
            var dbCommand = sessionToUse.Connection.CreateCommand();
            dbCommand.CommandText = sqlPronto;
            _unitOfWork.PrepareCommandSql(dbCommand);
            
            var valueRetorno = await sessionToUse.Connection.ExecuteScalarAsync(sqlPronto, SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters), dbCommand.Transaction);
            return Convert.ToInt64(valueRetorno);
        }

        public void BeginTransaction(IStatelessSession? session = null)
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
                    if (currentTransaction == null || !currentTransaction.IsActive || (currentTransaction.WasCommitted && currentTransaction.WasCommitted))
                        session?.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool executed, Exception? exception)> CommitAsync(IStatelessSession? session = null)
        {
            try
            {
                if (session == null)
                {
                    if (_forceRollback)
                    {
                        _unitOfWork.Rollback();
                        return (true, null);
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
                                return (true, null);
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
                return (false, err);
            }
        }

        public async void Rollback(IStatelessSession? session = null)
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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TokenResultModel?> GetLoggedToken()
        {
            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            if (usuario.userId == null)
                return null;

            return new TokenResultModel
            {
                UserId = !string.IsNullOrEmpty(usuario.userId) ? int.Parse(usuario.userId) : null,
                ProviderKeyUser = usuario.providerChaveUsuario,
                CompanyId = usuario.companyId
            };
        }

        public async Task<ParametroSistemaViewModel?> GetParametroSistemaViewModel()
        {
            throw new NotImplementedException("GetParametroSistemaViewModel não está implementado para RepositoryNHAccessCenter");
        }

        public async Task<string> GetToken()
        {
            return await _authenticatedBaseService.GetToken();
        }
    }

}
