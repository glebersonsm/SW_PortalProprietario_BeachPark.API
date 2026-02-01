using AccessCenterDomain;
using Dapper;
using Microsoft.Extensions.Configuration;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Infra.Data.Audit;
using SW_Utils.Auxiliar;
using SW_Utils.Enum;
using SW_Utils.Functions;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.Repositories.Core
{
    public class RepositoryNH : IRepositoryNH
    {
        private readonly IUnitOfWorkNHDefault _unitOfWork;
        private readonly IAuthenticatedBaseService _authenticatedBaseService;
        private ParametroSistemaViewModel? _parametroSistemaViewModel = null;
        private IConfiguration _configuration;
        private readonly bool _forceRollback = false;
        private readonly AuditHelper? _auditHelper;
        public IStatelessSession? Session => _unitOfWork.Session;

        public CancellationToken CancellationToken => _unitOfWork.CancellationToken;

        public bool IsAdm
        {
            get
            {
                var _loggedUserId = GetLoggedUser().Result;
                return _loggedUserId != null && _loggedUserId.Value.isAdm;
            }
        }

        public RepositoryNH(IUnitOfWorkNHDefault unitOfWork,
            IAuthenticatedBaseService authenticatedBaseService,
            IConfiguration configuration,
            AuditHelper? auditHelper = null)
        {
            ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
            _unitOfWork = unitOfWork;
            _authenticatedBaseService = authenticatedBaseService;
            _configuration = configuration;
            _forceRollback = _configuration.GetValue<bool>("ConnectionStrings:ForceRollback", false) && Debugger.IsAttached;
            _auditHelper = auditHelper;
        }

        public async Task<T> FindById<T>(int id)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            return await Session.GetAsync<T>(id, CancellationToken);
        }

        private async Task<T?> FindEntityWithRelationships<T>(int id)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));
                var entityType = typeof(T);
                var entityName = entityType.Name;

                // Detectar relacionamentos ManyToOne automaticamente usando reflection
                var relationships = DetectManyToOneRelationships(entityType);
                
                string hql;
                if (relationships.Any())
                {
                    // Construir HQL com Inner Join Fetch para cada relacionamento
                    var fetchJoins = string.Join(" ", relationships.Select(rel => $"Inner Join Fetch e.{rel}"));
                    hql = $"From {entityName} e {fetchJoins} Where e.Id = :id";
                }
                else
                {
                    // Busca genérica sem relacionamentos
                    hql = $"From {entityName} e Where e.Id = :id";
                }
                
                // Usar FindByHql que já está implementado
                var results = await FindByHql<T>(hql, new Parameter("id", id));
                
                return results.FirstOrDefault();
            }
            catch
            {
                // Se falhar, retornar null para usar busca simples
                return default;
            }
        }

        private List<string> DetectManyToOneRelationships(Type entityType)
        {
            var relationships = new List<string>();
            
            try
            {
                // Propriedades que não devem ser consideradas como relacionamentos
                var excludedPropertyNames = new HashSet<string>
                {
                    "Id", "UsuarioCriacao", "UsuarioAlteracao", "DataHoraCriacao",
                    "DataHoraAlteracao", "ObjectGuid", "DataHoraRemocao", "UsuarioRemocao"
                };

                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var prop in properties)
                {
                    // Ignorar propriedades excluídas
                    if (excludedPropertyNames.Contains(prop.Name))
                        continue;

                    // Verificar se a propriedade é do tipo EntityBaseCore (relacionamento ManyToOne)
                    if (typeof(EntityBaseCore).IsAssignableFrom(prop.PropertyType) && 
                        prop.PropertyType != typeof(EntityBaseCore))
                    {
                        relationships.Add(prop.Name);
                    }
                }
            }
            catch
            {
                // Ignorar erros ao detectar relacionamentos
            }

            return relationships;
        }

        /// <summary>
        /// Clona uma entidade para auditoria, garantindo que temos uma cópia independente do estado original
        /// </summary>
        private EntityBaseCore? CloneEntityForAudit(EntityBaseCore entity)
        {
            if (entity == null)
                return null;

            try
            {
                // Usar serialização JSON para fazer clone profundo
                // Isso garante que temos uma cópia completamente independente do estado original
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    MaxDepth = 10,
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                    PropertyNameCaseInsensitive = false
                };
                
                var json = System.Text.Json.JsonSerializer.Serialize(entity, entity.GetType(), options);
                var cloned = System.Text.Json.JsonSerializer.Deserialize(json, entity.GetType(), options);
                return cloned as EntityBaseCore;
            }
            catch
            {
                // Se falhar a serialização JSON, tentar MemberwiseClone se disponível
                try
                {
                    if (entity is ICloneable cloneable)
                    {
                        return cloneable.Clone() as EntityBaseCore;
                    }
                }
                catch
                {
                    // Se tudo falhar, retornar a entidade original
                    // Isso é melhor do que não ter nada para comparar
                }
                
                // Último recurso: retornar a mesma instância
                // Isso pode não ser ideal, mas é melhor do que null
                return entity;
            }
        }

        public async Task Remove<T>(T entity)
        {
            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));

            // Log de auditoria para exclusão (antes de deletar)
            if (_auditHelper != null && entity is EntityBaseCore objEntity)
            {
                objEntity.UsuarioRemocaoId = !string.IsNullOrEmpty(usuario.userId) ? int.Parse(usuario.userId) : null;
                _ = Task.Run(async () => await _auditHelper.LogDeleteAsync(objEntity));
            }

            await Session.DeleteAsync(entity, CancellationToken);
        }

        public async void RemoveRange<T>(IList<T> entities)
        {
            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            foreach (var entity in entities.Cast<EntityBaseCore>())
            {
                entity.UsuarioRemocaoId = !string.IsNullOrEmpty(usuario.userId) ? int.Parse(usuario.userId) : null;
                if (_auditHelper != null)
                    _ = Task.Run(async () => await _auditHelper.LogDeleteAsync(entity));

                await Session.DeleteAsync(entity, CancellationToken);
            }
        }

        public async Task<T> Save<T>(T entity)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            if (entity is EntityBaseCore objEntity)
            {

                if (entity is IEntityValidateCore validate)
                    await validate.SaveValidate();

                if (objEntity.Id == 0)
                {
                    if (objEntity.UsuarioCriacao.GetValueOrDefault(0) == 0)
                        objEntity.UsuarioCriacao = !string.IsNullOrEmpty(usuario.userId) ? Convert.ToInt32(usuario.userId) : null;

                    objEntity.DataHoraCriacao = DateTime.Now;
                    objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await Session.InsertAsync(objEntity, CancellationToken);

                    // Log de auditoria para criação (não bloqueia a operação)
                    if (_auditHelper != null)
                    {
                        _ = Task.Run(async () => await _auditHelper.LogCreateAsync(objEntity));
                    }
                }
                else
                {
                    // Buscar entidade antiga para comparação (apenas se auditHelper estiver disponível)
                    // IMPORTANTE: Buscar ANTES de modificar qualquer propriedade da entidade
                    // Como estamos usando IStatelessSession, a busca sempre vem do banco, não da sessão
                    EntityBaseCore? oldEntity = null;
                    if (_auditHelper != null)
                    {
                        try
                        {
                            // IMPORTANTE: Usar uma nova sessão stateless para garantir que buscamos do banco
                            // e não de qualquer cache ou instância modificada na sessão atual
                            // Tentar buscar com relacionamentos carregados usando HQL
                            oldEntity = await FindEntityWithRelationships<T>(objEntity.Id) as EntityBaseCore;
                            
                            // Se falhar, tentar busca simples
                            if (oldEntity == null)
                            {
                                oldEntity = await FindById<T>(objEntity.Id) as EntityBaseCore;
                            }
                            
                            // CRÍTICO: Fazer clone profundo da entidade antiga ANTES de qualquer modificação
                            // Isso garante que temos o estado original do banco, não a instância que pode ter sido modificada
                            // O clone deve ser feito imediatamente após buscar, antes de qualquer acesso às propriedades
                            if (oldEntity != null)
                            {
                                // Forçar acesso às propriedades antes de clonar para garantir que estão carregadas
                                // Isso é especialmente importante para propriedades lazy-loaded ou nullable
                                var entityType = oldEntity.GetType();
                                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                foreach (var prop in properties)
                                {
                                    if (prop.CanRead && (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string) || 
                                        prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?) ||
                                        (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))))
                                    {
                                        try
                                        {
                                            // Acessar a propriedade para forçar carregamento (se for lazy)
                                            _ = prop.GetValue(oldEntity);
                                        }
                                        catch
                                        {
                                            // Ignorar erros ao acessar propriedades
                                        }
                                    }
                                }
                                
                                // Agora fazer o clone profundo
                                oldEntity = CloneEntityForAudit(oldEntity);
                            }
                            
                            // Se ainda não encontrou, pode ser que a entidade não exista no banco ainda
                            // Nesse caso, não há entidade antiga para comparar
                        }
                        catch
                        {
                            // Se falhar, tentar busca simples
                            try
                            {
                                oldEntity = await FindById<T>(objEntity.Id) as EntityBaseCore;
                                
                                // Fazer clone mesmo se veio da busca simples
                                if (oldEntity != null)
                                {
                                    oldEntity = CloneEntityForAudit(oldEntity);
                                }
                            }
                            catch
                            {
                                // Ignorar erro ao buscar entidade antiga
                                // A entidade pode não existir no banco ainda
                            }
                        }
                    }

                    objEntity.UsuarioAlteracao = !string.IsNullOrEmpty(usuario.userId) ? Convert.ToInt32(usuario.userId) : objEntity.UsuarioAlteracao;
                    objEntity.DataHoraAlteracao = DateTime.Now;
                    if (string.IsNullOrEmpty(objEntity.ObjectGuid))
                        objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await Session.UpdateAsync(objEntity, CancellationToken);

                    // Log de auditoria para atualização (não bloqueia a operação)
                    if (_auditHelper != null && oldEntity != null)
                    {
                        _ = Task.Run(async () => await _auditHelper.LogUpdateAsync(oldEntity, objEntity));
                    }
                }
            }
            return entity;
        }

        public async Task<IList<T>> SaveRange<T>(IList<T> entities)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));

            var usuario = await _authenticatedBaseService.GetLoggedUserAsync();
            foreach (var entity in entities.Cast<EntityBaseCore>())
            {
                if (entity is IEntityValidateCore validate)
                    await validate.SaveValidate();

                if (entity.Id == 0)
                {
                    if (entity.UsuarioCriacao.GetValueOrDefault(0) == 0)
                        entity.UsuarioCriacao = !string.IsNullOrEmpty(usuario.userId) ? Convert.ToInt32(usuario.userId) : null;

                    entity.DataHoraCriacao = DateTime.Now;
                    entity.ObjectGuid = $"{Guid.NewGuid()}";
                    await Session.InsertAsync(entity, CancellationToken);
                }
                else
                {
                    entity.UsuarioAlteracao = !string.IsNullOrEmpty(usuario.userId) ? Convert.ToInt32(usuario.userId) : entity.UsuarioAlteracao;
                    entity.DataHoraAlteracao = DateTime.Now;
                    if (string.IsNullOrEmpty(entity.ObjectGuid))
                        entity.ObjectGuid = $"{Guid.NewGuid()}";
                    await Session.UpdateAsync(entity, CancellationToken);
                }

            }
            return entities;
        }

        public async Task<IList<T>> FindByHql<T>(string hql, params Parameter[] parameters)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(Session, nameof(Session));

                hql = RepositoryUtils.NormalizeFunctions(DataBaseType, hql);

                var query = Session.CreateQuery(hql);
                if (parameters != null)
                    SetParameters(parameters, query);


                return await query.ListAsync<T>(CancellationToken);
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public async Task<IList<T>> FindByHql<T>(string hql, int pageSize, int pageNumber, params Parameter[] parameters)
        {
            ArgumentNullException.ThrowIfNull(Session, nameof(Session));
            hql = RepositoryUtils.NormalizeFunctions(DataBaseType, hql);

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
                sql = RepositoryUtils.NormalizeFunctions(DataBaseType, sql);
                sql = NormalizaParameterName(sql, parameters);
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
                sql = NormalizaParameterName(sql, parameters);
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
            if (entity is EntityBaseCore objEntity)
            {
                if (objEntity.Id == 0)
                {
                    objEntity.UsuarioCriacao = !string.IsNullOrEmpty(usuario.userId) ? Convert.ToInt32(usuario.userId) : null;
                    objEntity.DataHoraCriacao = DateTime.Now;
                    objEntity.ObjectGuid = $"{Guid.NewGuid()}";
                    await Session.InsertAsync(objEntity, CancellationToken);
                }
                else
                {
                    objEntity.UsuarioAlteracao = !string.IsNullOrEmpty(usuario.userId) ? Convert.ToInt32(usuario.userId) : null;
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
            var _loggedUserId = await _authenticatedBaseService.GetLoggedUserAsync();

            return await Task.FromResult(_loggedUserId);
        }

        public async Task<Int64> CountTotalEntry(string sql, params Parameter[]? parameters)
        {
            try
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                ArgumentNullException.ThrowIfNull(Session?.Connection, nameof(Session.Connection));
                sql = RepositoryUtils.NormalizeFunctions(DataBaseType, sql);
                sql = NormalizaParameterName(sql, parameters);

                var sqlPronto = $"Select COUNT(1) FROM ({sql}) a ";

                var valueRetorno = await Session.Connection?.ExecuteScalarAsync($"{sqlPronto}", SW_Utils.Functions.RepositoryUtils.GetParametersForSql(parameters));

                return Convert.ToInt64(valueRetorno);
            }
            catch (Exception err)
            {

                throw;
            }

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
                                    p.PermiteReservaRciApenasClientesComContratoRci
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

        private string NormalizaParameterName(string sql, Parameter[]? parameters)
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

        public async Task<string> GetToken()
        {
            return await _authenticatedBaseService.GetToken();
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
