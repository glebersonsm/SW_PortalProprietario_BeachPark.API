using AccessCenterDomain;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_Utils.Auxiliar;
using SW_Utils.Enum;
using SW_Utils.Functions;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;

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

        public async Task<T> FindById<T>(int id, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            return await sessionToUse.GetAsync<T>(id, CancellationToken);
        }

        public void Remove<T>(T entity, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var usuario = _authenticatedBaseHostedService.GetLoggedUserAsync().Result;
            if (entity is EntityBaseCore objEntity)
            {
                objEntity.UsuarioRemocaoId = usuario?.UserId;
                sessionToUse.DeleteAsync(entity, CancellationToken);
            }
            else throw new ArgumentException($"Objeto: {entity} não herda da EntityBaseCore");
        }

        public async void RemoveRange<T>(IList<T> entities, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var usuario = await _authenticatedBaseHostedService.GetLoggedUserAsync();
            foreach (var entity in entities.Cast<EntityBaseCore>())
            {
                entity.UsuarioRemocaoId = usuario?.UserId;
                await sessionToUse.DeleteAsync(entity, CancellationToken);
            }
        }

        public async Task<T> Save<T>(T entity, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var usuario = await _authenticatedBaseHostedService.GetLoggedUserAsync();

            if (entity is EntityBaseCore objEntity)
            {
                if (objEntity.Id == 0)
                {
                    if (objEntity.UsuarioCriacao == null)
                        objEntity.UsuarioCriacao = usuario?.UserId;

                    objEntity.DataHoraCriacao = DateTime.Now;
                    objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await sessionToUse.InsertAsync(objEntity, CancellationToken);
                }
                else
                {
                    if (objEntity.UsuarioAlteracao == null)
                        objEntity.UsuarioAlteracao = usuario?.UserId;
                    objEntity.DataHoraAlteracao = DateTime.Now;
                    if (string.IsNullOrEmpty(objEntity.ObjectGuid))
                        objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await sessionToUse.UpdateAsync(objEntity, CancellationToken);
                }
            }
            return entity;
        }

        public async Task<IList<T>> SaveRange<T>(IList<T> entities, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var usuario = await _authenticatedBaseHostedService.GetLoggedUserAsync();

            foreach (var entity in entities.Cast<EntityBaseCore>())
            {
                if (entity.Id == 0)
                {
                    entity.UsuarioCriacao = usuario?.UserId;
                    entity.DataHoraCriacao = DateTime.Now;
                    entity.ObjectGuid = $"{Guid.NewGuid()}";
                    await sessionToUse.InsertAsync(entity, CancellationToken);
                }
                else
                {
                    entity.UsuarioAlteracao = usuario?.UserId;
                    entity.DataHoraAlteracao = DateTime.Now;
                    if (string.IsNullOrEmpty(entity.ObjectGuid))
                        entity.ObjectGuid = $"{Guid.NewGuid()}";
                    await sessionToUse.UpdateAsync(entity, CancellationToken);
                }
            }

            return entities;
        }

        public async Task<IList<T>> FindByHql<T>(string hql, IStatelessSession? session = null, params Parameter[] parameters)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var query = sessionToUse.CreateQuery(hql);
            if (parameters != null)
                SetParameters(parameters, query);
            return await query.ListAsync<T>();
        }

        public async Task<IList<T>> FindBySql<T>(string sql, IStatelessSession? session = null, params Parameter[] parameters)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            sql = NormalizaParameterName(sql, parameters);

            var dbCommand = sessionToUse.Connection.CreateCommand();
            dbCommand.CommandText = sql;
            _unitOfWork.PrepareCommandSql(dbCommand, sessionToUse);

            var dados = await sessionToUse.Connection.QueryAsync<T>(sql, SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters), dbCommand.Transaction);
            return dados.ToList();
        }

        private static void SetParameters(Parameter[] parameters, IQuery query)
        {
            foreach (var item in parameters)
            {
                query.SetParameter(item.Name, item.Value);
            }
        }

        public async Task<decimal> GetValueFromSequenceName(string sequenceName, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            var valueRetorno = await sessionToUse.CreateSQLQuery($"Select {sequenceName}.NextVal From Dual").UniqueResultAsync();
            return (decimal)valueRetorno;
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
            catch (Exception err)
            {
                _logger.LogError(err, err.Message, err.StackTrace, err.Source);
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
                _logger.LogError(err, err.Message, err.StackTrace, err.Source);
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
            catch (Exception err)
            {
                _logger.LogError(err, err.Message, err.StackTrace, err.Source);
            }
        }

        public async Task<TokenResultModel?> GetLoggedToken()
        {
            return await _authenticatedBaseHostedService.GetLoggedUserAsync();
        }

        public async Task<T> ForcedSave<T>(T entity, IStatelessSession? session = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));

            var usuario = await _authenticatedBaseHostedService.GetLoggedUserAsync(false);
            if (usuario != null && usuario.UserId.GetValueOrDefault(0) > 0)
                throw new Exception("O ForcedSave só pode ser utilizado quando não existir um usuário logado!");

            if (entity is EntityBaseCore objEntity)
            {
                if (objEntity.Id == 0)
                {
                    if (objEntity.UsuarioCriacao == null)
                        objEntity.UsuarioCriacao = usuario?.UserId;
                    objEntity.DataHoraCriacao = DateTime.Now;
                    objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await sessionToUse.InsertAsync(objEntity, CancellationToken);
                }
                else
                {
                    if (objEntity.UsuarioAlteracao == null)
                        objEntity.UsuarioAlteracao = usuario?.UserId;

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

            if (entity is EntityBaseCore objEntity)
            {
                objEntity.DataHoraAlteracao = DateTime.Now;
                objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                await sessionToUse.InsertAsync(objEntity, CancellationToken);
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

        public bool IsAdm => throw new NotImplementedException();

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

        public async Task<IList<T>> FindBySql<T>(string sql, int pageSize, int pageNumber, params Parameter[] parameters)
        {
            return await FindBySql<T>(sql, null, parameters);
        }

        public async Task<long> CountTotalEntry(string sql, IStatelessSession? session = null, Parameter[]? parameters = null)
        {
            var sessionToUse = session ?? Session;
            ArgumentNullException.ThrowIfNull(sessionToUse, nameof(sessionToUse));
            
            sql = NormalizaParameterName(sql, parameters);
            
            var sqlPronto = $"Select COUNT(1) FROM ({sql}) a ";
            
            var dbCommand = sessionToUse.Connection.CreateCommand();
            dbCommand.CommandText = sqlPronto;
            _unitOfWork.PrepareCommandSql(dbCommand, sessionToUse);
            
            var valueRetorno = await sessionToUse.Connection.ExecuteScalarAsync(sqlPronto, SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters), dbCommand.Transaction);
            return Convert.ToInt64(valueRetorno);
        }

        public async Task<ParametroSistemaViewModel?> GetParametroSistemaViewModel()
        {
            var empresas = (await FindByHql<Empresa>("From Empresa e Inner Join Fetch e.Pessoa p")).AsList();
            if (empresas.Count() > 1 || empresas.Count() == 0)
                throw new ArgumentException($"Não foi possível salvar os parâmetros do sistema empCount = {empresas.Count()}");

            var empFirst = empresas.First();

            List<Parameter> parameters = new();
            StringBuilder sb = new(@$"Select 
                                    p.Id, 
                                    p.SiteParaReserva,
                                    p.Empresa as EmpresaId,
                                    p.AgruparCertidaoPorCliente,
                                    p.EmitirCertidaoPorUnidCliente,
                                    p.HabilitarBaixarBoleto,
                                    p.HabilitarPagamentosOnLine,
                                    p.HabilitarPagamentoEmPix,
                                    p.HabilitarPagamentoEmCartao,
                                    p.ExibirContasVencidas,
                                    p.QtdeMaximaDiasContasAVencer,
                                    p.PermitirUsuarioAlterarSeuEmail,
                                    p.PermitirUsuarioAlterarSeuDoc,
                                    Coalesce(p.IntegradoComMultiPropriedade,0) as IntegradoComMultiPropriedade,
                                    Coalesce(p.IntegradoComTimeSharing,0) as IntegradoComTimeSharing,
                                    p.NomeCondominio,
                                    p.CnpjCondominio,
                                    p.EnderecoCondominio,
                                    p.NomeAdministradoraCondominio,
                                    p.CnpjAdministradoraCondominio,
                                    p.EnderecoAdministradoraCondominio,
                                    p.ExibirFinanceirosDasEmpresaIds,
                                    p.ImagemHomeUrl1,
                                    p.ImagemHomeUrl2,
                                    p.ImagemHomeUrl3,
                                    p.ImagemHomeUrl4,
                                    p.ImagemHomeUrl5,
                                    p.ImagemHomeUrl6,
                                    p.ImagemHomeUrl7,
                                    p.ImagemHomeUrl8,
                                    p.ImagemHomeUrl9,
                                    p.ImagemHomeUrl10,
                                    p.ImagemHomeUrl11,
                                    p.ImagemHomeUrl12,
                                    p.ImagemHomeUrl13,
                                    p.ImagemHomeUrl14,
                                    p.ImagemHomeUrl15,
                                    p.ImagemHomeUrl16,
                                    p.ImagemHomeUrl17,
                                    p.ImagemHomeUrl18,
                                    p.ImagemHomeUrl19,
                                    p.ImagemHomeUrl20,
                                    p.PontosRci,
                                    p.ExigeEnderecoHospedeConvidado,
                                    p.ExigeTelefoneHospedeConvidado,
                                    p.ExigeDocumentoHospedeConvidado,
                                    p.PermiteReservaRciApenasClientesComContratoRci,
                                    p.ExibirMensagemLogin,
                                    p.MensagemLogin,
                                    p.Habilitar2FAPorEmail,
                                    p.Habilitar2FAPorSms,
                                    p.Habilitar2FAParaCliente,
                                    p.Habilitar2FAParaAdministrador,
                                    p.EndpointEnvioSms2FA,
                                    p.SmtpHost,
                                    p.SmtpPort,
                                    p.SmtpUseSsl,
                                    p.SmtpUser,
                                    p.SmtpPass,
                                    p.SmtpFromName,
                                    p.TipoEnvioEmail,
                                    p.EmailTrackingBaseUrl
                                    From 
                                    ParametroSistema p
                                    Where 1 = 1 ");

            sb.AppendLine($" and p.Empresa = {empFirst.Id}");


            var parametroSistema = (await FindBySql<ParametroSistemaViewModel>(sb.ToString())).FirstOrDefault();
            if (parametroSistema == null)
                return parametroSistema;

            parametroSistema.HabilitarPagamentosOnLine =
                (parametroSistema.HabilitarPagamentoEmPix.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Sim ||
                parametroSistema.HabilitarPagamentoEmCartao.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Sim) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Não;


            return parametroSistema;
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

        public async Task<string> GetToken()
        {
            return await Task.FromResult("");
        }

        public void Flush()
        {
            Session?.GetSessionImplementation()?.Flush();
        }
    }

}
