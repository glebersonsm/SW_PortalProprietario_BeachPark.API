using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuditModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Auditoria;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Auxiliar;
using SW_Utils.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class AuditService : IAuditService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<AuditService> _logger;
        private readonly ISwSessionFactoryDefault _sessionFactory;

        public AuditService(
            IRepositoryNH repository,
            ILogger<AuditService> logger,
            ISwSessionFactoryDefault sessionFactory)
        {
            _repository = repository;
            _logger = logger;
            _sessionFactory = sessionFactory;
        }

        public async Task SaveAuditLogAsync(AuditLogMessageEvent message)
        {
            // Usar uma sessão isolada para não interferir com a transação principal
            IStatelessSession? isolatedSession = null;
            ITransaction? isolatedTransaction = null;
            
            try
            {
                // Criar uma sessão stateless isolada
                isolatedSession = _sessionFactory.OpenStatelessSession();
                isolatedTransaction = isolatedSession.BeginTransaction();

                var auditLog = new AuditLog
                {
                    EntityType = message.EntityType,
                    EntityId = message.EntityId,
                    Action = (EnumAuditAction)message.Action,
                    UserId = message.UserId,
                    UserName = message.UserName,
                    Timestamp = message.Timestamp,
                    IpAddress = message.IpAddress,
                    UserAgent = message.UserAgent,
                    ChangesJson = message.ChangesJson,
                    EntityDataJson = message.EntityDataJson,
                    ObjectGuid = message.ObjectGuid,
                    DataHoraCriacao = DateTime.Now,
                    UsuarioCriacao = message.UserId
                };

                // Inserir usando a sessão isolada com CancellationToken
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await isolatedSession.InsertAsync(auditLog, cts.Token);
                await isolatedTransaction.CommitAsync(cts.Token);
            }
            catch (Exception ex)
            {
                try
                {
                    if (isolatedTransaction != null && isolatedTransaction.IsActive)
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await isolatedTransaction.RollbackAsync(cts.Token);
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Erro ao fazer rollback da transação isolada de auditoria");
                }

                _logger.LogError(ex, "Erro ao salvar log de auditoria: EntityType={EntityType}, EntityId={EntityId}", 
                    message.EntityType, message.EntityId);
                // Não relançar a exceção para não quebrar a operação principal
            }
            finally
            {
                try
                {
                    isolatedTransaction?.Dispose();
                    isolatedSession?.Dispose();
                }
                catch (Exception disposeEx)
                {
                    _logger.LogError(disposeEx, "Erro ao liberar recursos da sessão isolada de auditoria");
                }
            }
        }

        public async Task<AuditLogPagedResult> GetAuditLogsAsync(AuditLogFilterModel filter)
        {
            try
            {
                var parameters = new List<Parameter>();
                var sqlBase = "Select " +
                    "a.Id, " +
                    "a.EntityType, " +
                    "a.EntityId, " +
                    "a.Action, " +
                    "a.UserId, " +
                    "a.UserName, " +
                    "a.Timestamp, " +
                    "a.IpAddress, " +
                    "a.UserAgent, " +
                    "a.ChangesJson, " +
                    "a.EntityDataJson, " +
                    "a.ObjectGuid, " +
                    "a.DataHoraCriacao, " +
                    "a.UsuarioCriacao " +
                    "From AuditLog a Where 1=1";

                var whereClause = "";

                if (filter.DataInicio.HasValue)
                {
                    whereClause += " and a.Timestamp >= :dataInicio";
                    parameters.Add(new Parameter("dataInicio", filter.DataInicio.Value));
                }

                if (filter.DataFim.HasValue)
                {
                    whereClause += " and a.Timestamp <= :dataFim";
                    parameters.Add(new Parameter("dataFim", filter.DataFim.Value));
                }

                if (!string.IsNullOrEmpty(filter.EntityType))
                {
                    whereClause += " and a.EntityType = :entityType";
                    parameters.Add(new Parameter("entityType", filter.EntityType));
                }

                if (filter.EntityId.HasValue)
                {
                    whereClause += " and a.EntityId = :entityId";
                    parameters.Add(new Parameter("entityId", filter.EntityId.Value));
                }

                if (filter.UserId.HasValue)
                {
                    whereClause += " and a.UserId = :userId";
                    parameters.Add(new Parameter("userId", filter.UserId.Value));
                }

                if (filter.Action.HasValue)
                {
                    whereClause += " and a.Action = :action";
                    parameters.Add(new Parameter("action", (int)filter.Action.Value));
                }

                if (!string.IsNullOrEmpty(filter.IpAddress))
                {
                    whereClause += " and a.IpAddress like :ipAddress";
                    parameters.Add(new Parameter("ipAddress", $"%{filter.IpAddress}%"));
                }

                var sql = sqlBase + whereClause + " Order By a.Timestamp Desc";

                // 🔥 MELHORIA: Calcular total de registros para paginação usando CountTotalEntry
                var countSql = "Select * From AuditLog a Where 1=1" + whereClause;
                var totalRecords = await _repository.CountTotalEntry(countSql, session: null, parameters.ToArray());

                // Calcular última página
                var lastPageNumber = totalRecords > 0 
                    ? (int)Math.Ceiling((double)totalRecords / filter.PageSize) 
                    : 1;

                var logs = await _repository.FindBySql<AuditLogSearchModel>(
                    sql,
                    filter.PageSize,
                    filter.PageNumber,
                    parameters.ToArray());

                var result = new List<AuditLogModel>();
                foreach (var log in logs)
                {
                    var model = new AuditLogModel
                    {
                        Id = log.Id,
                        EntityType = log.EntityType,
                        EntityId = log.EntityId,
                        Action = (EnumAuditAction)log.Action,
                        UserId = log.UserId,
                        UserName = log.UserName,
                        Timestamp = log.Timestamp,
                        IpAddress = log.IpAddress,
                        UserAgent = log.UserAgent,
                        ChangesJson = log.ChangesJson,
                        EntityDataJson = log.EntityDataJson,
                        ObjectGuid = log.ObjectGuid,
                        DataHoraCriacao = log.DataHoraCriacao,
                        UsuarioCriacao = log.UsuarioCriacao
                    };
                    
                    // Buscar nome do usuário se não estiver disponível
                    if (string.IsNullOrEmpty(model.UserName) && model.UserId.HasValue)
                    {
                        model.UserName = await GetUserNameByIdAsync(model.UserId.Value);
                    }
                    
                    // Deserializar ChangesJson para lista de mudanças
                    if (!string.IsNullOrEmpty(log.ChangesJson))
                    {
                        try
                        {
                            var changesDict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(log.ChangesJson);
                            if (changesDict != null)
                            {
                                model.Changes = changesDict.Select(kvp => new AuditChangeModel
                                {
                                    PropertyName = kvp.Key,
                                    OldValue = kvp.Value.ContainsKey("oldValue") ? kvp.Value["oldValue"]?.ToString() : null,
                                    NewValue = kvp.Value.ContainsKey("newValue") ? kvp.Value["newValue"]?.ToString() : null,
                                    FriendlyMessage = kvp.Value.ContainsKey("friendlyMessage") ? kvp.Value["friendlyMessage"]?.ToString() : null
                                }).ToList();
                            }
                        }
                        catch
                        {
                            // Ignorar erros de deserialização
                        }
                    }

                    result.Add(model);
                }

                return new AuditLogPagedResult
                {
                    Data = result,
                    PageNumber = filter.PageNumber,
                    LastPageNumber = lastPageNumber,
                    TotalCount = (int)totalRecords,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar logs de auditoria");
                throw;
            }
        }

        public async Task<List<AuditLogModel>> GetAuditLogByEntityAsync(string entityType, int entityId)
        {
            try
            {
                var sql = "Select " +
                    "a.Id, " +
                    "a.EntityType, " +
                    "a.EntityId, " +
                    "a.Action, " +
                    "a.UserId, " +
                    "a.UserName, " +
                    "a.Timestamp, " +
                    "a.IpAddress, " +
                    "a.UserAgent, " +
                    "a.ChangesJson, " +
                    "a.EntityDataJson, " +
                    "a.ObjectGuid, " +
                    "a.DataHoraCriacao, " +
                    "a.UsuarioCriacao " +
                    "From AuditLog a " +
                    "Where a.EntityType = :entityType and a.EntityId = :entityId " +
                    "Order By a.Timestamp Desc";

                var logs = await _repository.FindBySql<AuditLogSearchModel>(
                    sql,session:null,
                    new Parameter("entityType", entityType),
                    new Parameter("entityId", entityId));

                var result = new List<AuditLogModel>();
                foreach (var log in logs)
                {
                    var model = new AuditLogModel
                    {
                        Id = log.Id,
                        EntityType = log.EntityType,
                        EntityId = log.EntityId,
                        Action = (EnumAuditAction)log.Action,
                        UserId = log.UserId,
                        UserName = log.UserName,
                        Timestamp = log.Timestamp,
                        IpAddress = log.IpAddress,
                        UserAgent = log.UserAgent,
                        EntityDataJson = log.EntityDataJson,
                        ObjectGuid = log.ObjectGuid,
                        DataHoraCriacao = log.DataHoraCriacao,
                        UsuarioCriacao = log.UsuarioCriacao
                    };
                    
                    // Buscar nome do usuário se não estiver disponível
                    if (string.IsNullOrEmpty(model.UserName) && model.UserId.HasValue)
                    {
                        model.UserName = await GetUserNameByIdAsync(model.UserId.Value);
                    }
                    
                    // Deserializar ChangesJson
                    if (!string.IsNullOrEmpty(log.ChangesJson))
                    {
                        try
                        {
                            var changesDict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(log.ChangesJson);
                            if (changesDict != null)
                            {
                                model.Changes = changesDict.Select(kvp => new AuditChangeModel
                                {
                                    PropertyName = kvp.Key,
                                    OldValue = kvp.Value.ContainsKey("oldValue") ? kvp.Value["oldValue"]?.ToString() : null,
                                    NewValue = kvp.Value.ContainsKey("newValue") ? kvp.Value["newValue"]?.ToString() : null,
                                    FriendlyMessage = kvp.Value.ContainsKey("friendlyMessage") ? kvp.Value["friendlyMessage"]?.ToString() : null
                                }).ToList();
                            }
                        }
                        catch
                        {
                            // Ignorar erros de deserialização
                        }
                    }

                    result.Add(model);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar histórico de auditoria: EntityType={EntityType}, EntityId={EntityId}", 
                    entityType, entityId);
                throw;
            }
        }

        public async Task<AuditLogModel?> GetAuditLogByIdAsync(int id)
        {
            try
            {
                var sql = "Select " +
                    "a.Id, " +
                    "a.EntityType, " +
                    "a.EntityId, " +
                    "a.Action, " +
                    "a.UserId, " +
                    "a.UserName, " +
                    "a.Timestamp, " +
                    "a.IpAddress, " +
                    "a.UserAgent, " +
                    "a.ChangesJson, " +
                    "a.EntityDataJson, " +
                    "a.ObjectGuid, " +
                    "a.DataHoraCriacao, " +
                    "a.UsuarioCriacao " +
                    "From AuditLog a " +
                    "Where a.Id = :id";

                var logs = await _repository.FindBySql<AuditLogSearchModel>(
                    sql,
                    session: null, new Parameter("id", id));

                var log = logs.FirstOrDefault();
                if (log == null)
                    return null;

                var model = new AuditLogModel
                {
                    Id = log.Id,
                    EntityType = log.EntityType,
                    EntityId = log.EntityId,
                    Action = (EnumAuditAction)log.Action,
                    UserId = log.UserId,
                    UserName = log.UserName,
                    Timestamp = log.Timestamp,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    EntityDataJson = log.EntityDataJson,
                    ObjectGuid = log.ObjectGuid,
                    DataHoraCriacao = log.DataHoraCriacao,
                    UsuarioCriacao = log.UsuarioCriacao
                };
                
                // Buscar nome do usuário se não estiver disponível
                if (string.IsNullOrEmpty(model.UserName) && model.UserId.HasValue)
                {
                    model.UserName = await GetUserNameByIdAsync(model.UserId.Value);
                }
                
                // Deserializar ChangesJson
                if (!string.IsNullOrEmpty(log.ChangesJson))
                {
                    try
                    {
                        var changesDict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(log.ChangesJson);
                        if (changesDict != null)
                        {
                            model.Changes = changesDict.Select(kvp => new AuditChangeModel
                            {
                                PropertyName = kvp.Key,
                                OldValue = kvp.Value.ContainsKey("oldValue") ? kvp.Value["oldValue"]?.ToString() : null,
                                NewValue = kvp.Value.ContainsKey("newValue") ? kvp.Value["newValue"]?.ToString() : null,
                                FriendlyMessage = kvp.Value.ContainsKey("friendlyMessage") ? kvp.Value["friendlyMessage"]?.ToString() : null
                            }).ToList();
                        }
                    }
                    catch
                    {
                        // Ignorar erros de deserialização
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar log de auditoria: Id={Id}", id);
                throw;
            }
        }

        private async Task<string?> GetUserNameByIdAsync(int userId)
        {
            try
            {
                // Buscar nome do usuário usando SQL com model especializado
                var usuarios = await _repository.FindBySql<UsuarioPessoaModel>(
                    "Select u.Id as UsuarioId, p.Id as PessoaId, p.Nome as NomePessoa, u.Login " +
                    "From Usuario u " +
                    "Inner Join Pessoa p on u.Pessoa = p.Id " +
                    "Where u.Id = :userId",
                    session: null, new Parameter("userId", userId));
                
                var usuario = usuarios.FirstOrDefault();
                return usuario?.NomePessoa;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar nome do usuário: UserId={UserId}", userId);
                return null;
            }
        }
    }
}

