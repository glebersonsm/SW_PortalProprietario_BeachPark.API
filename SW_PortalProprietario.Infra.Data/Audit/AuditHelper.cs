using Microsoft.AspNetCore.Http;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Domain.Entities.Core;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SW_PortalProprietario.Infra.Data.Audit
{
    public class AuditHelper
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
        private static readonly HashSet<string> _excludedProperties = new()
        {
            "Id", "DataHoraCriacao", "UsuarioCriacao", "DataHoraAlteracao",
            "UsuarioAlteracao", "ObjectGuid"
        };

        // Opções de serialização JSON sem escape de caracteres Unicode
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };

        private readonly IAuditLogQueueProducer _auditQueueProducer;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AuditHelper(
            IAuditLogQueueProducer auditQueueProducer,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _auditQueueProducer = auditQueueProducer;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogCreateAsync<T>(T entity) where T : EntityBaseCore
        {
            try
            {
                var context = GetAuditContext();

                // Para entidades com relacionamentos, garantir que estejam carregadas
                // Isso é especialmente importante para GrupoImagemHomeTags
                EnsureRelatedEntitiesLoaded(entity);

                // Gerar mensagem amigável para criação
                var friendlyMessage = GenerateFriendlyMessageForOperation(entity, EnumAuditAction.Create, null);

                // 🔥 MELHORIA: Incluir dados da entidade criada no ChangesJson
                // Isso é especialmente importante para entidades relacionadas como PessoaEndereco, PessoaTelefone
                var changes = new Dictionary<string, Dictionary<string, object?>>();

                if (!string.IsNullOrEmpty(friendlyMessage))
                {
                    changes["_operation"] = new Dictionary<string, object?>
                    {
                        { "friendlyMessage", friendlyMessage }
                    };
                }

                // Adicionar dados da entidade criada
                var entityData = SerializeEntityDataForChanges(entity);
                if (entityData != null && entityData.Count > 0)
                {
                    changes["_createdData"] = entityData;
                }

                var changesJson = changes.Count > 0
                    ? JsonSerializer.Serialize(changes, _jsonOptions)
                    : "{}";

                // 🔥 FIX: Obter tipo real removendo sufixo "Proxy"
                var entityType = GetRealEntityType(entity.GetType());

                var message = new AuditLogMessageEvent
                {
                    EntityType = entityType.Name,
                    EntityId = entity.Id,
                    Action = (int)EnumAuditAction.Create,
                    UserId = entity.UsuarioCriacao,
                    Timestamp = entity.DataHoraCriacao ?? DateTime.Now,
                    IpAddress = context.IpAddress,
                    UserAgent = context.UserAgent,
                    ChangesJson = changesJson,
                    EntityDataJson = SerializeEntity(entity),
                    ObjectGuid = entity.ObjectGuid
                };

                await _auditQueueProducer.EnqueueAuditLogAsync(message);
            }
            catch (Exception)
            {
                // Não lançar exceção para não quebrar a operação principal
                // Log pode ser feito em outro lugar se necessário
            }
        }

        private void EnsureRelatedEntitiesLoaded<T>(T entity) where T : EntityBaseCore
        {
            try
            {
                var entityType = entity.GetType();

                // Detectar relacionamentos ManyToOne automaticamente
                var relationships = DetectManyToOneRelationships(entityType);

                foreach (var relationshipName in relationships)
                {
                    var relationshipProperty = entityType.GetProperty(relationshipName);
                    if (relationshipProperty != null)
                    {
                        var relatedEntity = relationshipProperty.GetValue(entity);
                        if (relatedEntity != null)
                        {
                            // Acessar propriedades comuns de nome para forçar o carregamento (NHibernate lazy loading)
                            var nameProperties = new[] { "Nome", "Name", "Descricao", "Description", "Titulo", "Title" };
                            foreach (var nameProp in nameProperties)
                            {
                                var nomeProperty = relatedEntity.GetType().GetProperty(nameProp);
                                if (nomeProperty != null)
                                {
                                    nomeProperty.GetValue(relatedEntity);
                                    break; // Encontrou uma propriedade de nome, pode parar
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignorar erros ao garantir carregamento
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

        private string? GenerateFriendlyMessageForOperation<T>(
            T entity,
            EnumAuditAction action,
            Dictionary<string, Dictionary<string, object?>>? changes = null) where T : EntityBaseCore
        {
            try
            {
                // 🔥 FIX: Obter tipo real removendo sufixo "Proxy"
                var entityType = GetRealEntityType(entity.GetType());
                var entityTypeName = entityType.Name;
                var entityDisplayName = GetEntityDisplayName(entity);

                // Detectar entidade pai (relacionamento)
                var parentInfo = DetectParentEntity(entity);

                // Casos específicos para entidades de relacionamento com tags (detecção genérica)
                if (entityTypeName.EndsWith("Tags"))
                {
                    return GenerateTagOperationMessage(entity, action, changes, parentInfo);
                }

                // Mensagens genéricas baseadas no tipo de ação
                switch (action)
                {
                    case EnumAuditAction.Create:
                        if (parentInfo.HasValue)
                        {
                            var parentMsg = !string.IsNullOrEmpty(parentInfo.Value.ParentName)
                                ? $" vinculado ao {parentInfo.Value.ParentType} '{parentInfo.Value.ParentName}'"
                                : $" vinculado ao {parentInfo.Value.ParentType} (ID: {parentInfo.Value.ParentId})";

                            if (!string.IsNullOrEmpty(entityDisplayName))
                            {
                                return $"Criado {entityTypeName} '{entityDisplayName}'{parentMsg}";
                            }
                            return $"Criado {entityTypeName} (ID: {entity.Id}){parentMsg}";
                        }

                        if (!string.IsNullOrEmpty(entityDisplayName))
                        {
                            return $"Criado: {entityDisplayName}";
                        }
                        return $"Criado registro do tipo {entityTypeName} (ID: {entity.Id})";

                    case EnumAuditAction.Update:
                        return GenerateUpdateMessage(entity, entityDisplayName, changes, parentInfo);

                    case EnumAuditAction.Delete:
                        if (parentInfo.HasValue)
                        {
                            var parentMsg = !string.IsNullOrEmpty(parentInfo.Value.ParentName)
                                ? $" do {parentInfo.Value.ParentType} '{parentInfo.Value.ParentName}'"
                                : $" do {parentInfo.Value.ParentType} (ID: {parentInfo.Value.ParentId})";

                            if (!string.IsNullOrEmpty(entityDisplayName))
                            {
                                return $"Removido {entityTypeName} '{entityDisplayName}'{parentMsg}";
                            }
                            return $"Removido {entityTypeName} (ID: {entity.Id}){parentMsg}";
                        }

                        if (!string.IsNullOrEmpty(entityDisplayName))
                        {
                            return $"Removido: {entityDisplayName}";
                        }
                        return $"Removido registro do tipo {entityTypeName} (ID: {entity.Id})";

                    default:
                        return null;
                }
            }
            catch
            {
                // Ignorar erros ao gerar mensagem amigável
            }

            return null;
        }

        private string? GenerateTagOperationMessage<T>(T entity, EnumAuditAction action, Dictionary<string, Dictionary<string, object?>>? changes, (string ParentType, int ParentId, string? ParentName)? parentInfo = null) where T : EntityBaseCore
        {
            try
            {
                var entityType = typeof(T);

                // Detectar automaticamente propriedades de relacionamento usando reflection
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Procurar propriedade Tags (pode ter nomes variados: Tags, Tag, etc.)
                PropertyInfo? tagsProperty = null;
                PropertyInfo? parentProperty = null;
                PropertyInfo? dataHoraRemocaoProperty = null;

                foreach (var prop in properties)
                {
                    var propName = prop.Name.ToLower();
                    var propTypeName = prop.PropertyType.Name.ToLower();

                    // Detectar propriedade de tag (Tags, Tag, etc.)
                    if ((propName.Contains("tag") || propTypeName.Contains("tag")) &&
                        typeof(EntityBaseCore).IsAssignableFrom(prop.PropertyType))
                    {
                        tagsProperty = prop;
                    }
                    // Detectar propriedade pai (GrupoImagemHome, Grupo, Usuario, etc.)
                    // Excluir apenas Tags, Pessoa e Empresa (mas incluir Usuario para UsuarioTags)
                    else if (!propName.Contains("tag") &&
                             !propName.Contains("pessoa") &&
                             !propName.Contains("empresa") &&
                             typeof(EntityBaseCore).IsAssignableFrom(prop.PropertyType) &&
                             prop.PropertyType != typeof(EntityBaseCore))
                    {
                        // Preferir a primeira propriedade que não seja tag/intermediária
                        if (parentProperty == null)
                            parentProperty = prop;
                    }

                    if (prop.Name == "DataHoraRemocao")
                        dataHoraRemocaoProperty = prop;
                }

                if (tagsProperty == null || parentProperty == null)
                    return null;

                var tag = tagsProperty.GetValue(entity);
                var parent = parentProperty.GetValue(entity);
                var dataHoraRemocao = dataHoraRemocaoProperty?.GetValue(entity) as DateTime?;

                // 🔥 MELHORIA: Garantir que o nome da tag seja carregado antes de usar
                // Forçar carregamento da tag para garantir que o nome esteja disponível
                if (tag is EntityBaseCore tagEntity)
                {
                    // Forçar acesso ao nome da tag para garantir lazy loading
                    var tagNomeProperty = tagEntity.GetType().GetProperty("Nome");
                    if (tagNomeProperty != null)
                    {
                        _ = tagNomeProperty.GetValue(tagEntity);
                    }
                }

                EnsureRelatedEntitiesLoaded(entity);

                var tagNome = GetEntityName(tag, "Nome");
                var parentTypeName = parentProperty.PropertyType.Name;

                // 🔥 MELHORIA: Para Usuario, usar Login ou Pessoa.Nome como nome
                string? parentNome = null;
                if (parentTypeName == "Usuario" && parent is EntityBaseCore usuarioEntity)
                {
                    // Tentar Login primeiro
                    parentNome = GetEntityName(parent, "Login");

                    // Se não tiver Login, tentar Pessoa.Nome
                    if (string.IsNullOrEmpty(parentNome))
                    {
                        var pessoaProp = parent.GetType().GetProperty("Pessoa");
                        if (pessoaProp != null)
                        {
                            var pessoa = pessoaProp.GetValue(parent);
                            parentNome = GetEntityName(pessoa, "Nome");
                        }
                    }
                }
                else
                {
                    parentNome = GetEntityName(parent, "Nome");
                }

                // 🔥 MELHORIA: Adicionar informações da tag nos changes para facilitar consulta e exibição
                if (changes != null && tag is EntityBaseCore tagEntityForChanges)
                {
                    // Adicionar informações da tag nos changes se ainda não existir
                    // Isso garante que o nome da tag esteja sempre disponível nos logs
                    if (!changes.ContainsKey("Tags"))
                    {
                        changes["Tags"] = new Dictionary<string, object?>
                        {
                            { "tagId", tagEntityForChanges.Id },
                            { "tagNome", tagNome ?? $"Tag (ID: {tagEntityForChanges.Id})" },
                            { "tagType", tagEntityForChanges.GetType().Name }
                        };
                    }
                    else
                    {
                        // Atualizar informações se já existir
                        if (!changes["Tags"].ContainsKey("tagNome") || string.IsNullOrEmpty(changes["Tags"]["tagNome"]?.ToString()))
                        {
                            changes["Tags"]["tagNome"] = tagNome ?? $"Tag (ID: {tagEntityForChanges.Id})";
                        }
                    }
                }

                if (action == EnumAuditAction.Create && dataHoraRemocao == null)
                {
                    // 🔥 MELHORIA: Mensagens mais específicas baseadas no tipo de parent
                    if (!string.IsNullOrEmpty(tagNome))
                    {
                        if (!string.IsNullOrEmpty(parentNome))
                        {
                            // Usar nome do parent quando disponível
                            // Para Usuario, usar "ao usuário", para outros usar "no item"
                            if (parentTypeName == "Usuario")
                            {
                                return $"Adicionada a tag \"{tagNome}\" ao usuário \"{parentNome}\"";
                            }
                            else
                            {
                                return $"Vinculada a tag \"{tagNome}\" no item \"{parentNome}\"";
                            }
                        }
                        else if (parent != null && parent is EntityBaseCore parentEntity)
                        {
                            // Usar ID do parent quando nome não estiver disponível
                            if (parentTypeName == "Usuario")
                            {
                                return $"Adicionada a tag \"{tagNome}\" ao usuário (ID: {parentEntity.Id})";
                            }
                            else
                            {
                                return $"Vinculada a tag \"{tagNome}\" no item (ID: {parentEntity.Id})";
                            }
                        }
                        else
                        {
                            // Apenas nome da tag
                            return $"Vinculada a tag \"{tagNome}\"";
                        }
                    }
                    else if (tag is EntityBaseCore tagEntityFallback)
                    {
                        // Fallback: usar ID se nome não estiver disponível
                        if (!string.IsNullOrEmpty(parentNome))
                        {
                            if (parentTypeName == "Usuario")
                            {
                                return $"Adicionada a tag (ID: {tagEntityFallback.Id}) ao usuário \"{parentNome}\"";
                            }
                            else
                            {
                                return $"Vinculada a tag (ID: {tagEntityFallback.Id}) no item \"{parentNome}\"";
                            }
                        }
                        else
                        {
                            return $"Vinculada a tag (ID: {tagEntityFallback.Id})";
                        }
                    }
                }
                else if (action == EnumAuditAction.Update && dataHoraRemocao != null && changes != null)
                {
                    // Verificar se DataHoraRemocao foi preenchido (soft delete)
                    if (changes.ContainsKey("DataHoraRemocao"))
                    {
                        if (!string.IsNullOrEmpty(tagNome) && !string.IsNullOrEmpty(parentNome))
                        {
                            return $"Removida a tag \"{tagNome}\" do item \"{parentNome}\"";
                        }
                        else if (!string.IsNullOrEmpty(tagNome))
                        {
                            return $"Removida a tag \"{tagNome}\"";
                        }
                        else if (tag is EntityBaseCore tagEntityFallback)
                        {
                            // Fallback: usar ID se nome não estiver disponível
                            return $"Removida a tag (ID: {tagEntityFallback.Id})";
                        }
                    }
                }
            }
            catch
            {
                // Ignorar erros
            }

            return null;
        }

        private string? GenerateUpdateMessage<T>(T entity, string? entityDisplayName, Dictionary<string, Dictionary<string, object?>>? changes, (string ParentType, int ParentId, string? ParentName)? parentInfo = null) where T : EntityBaseCore
        {
            if (changes == null || changes.Count == 0)
                return null;

            // 🔥 FIX: Obter tipo real removendo sufixo "Proxy"
            var entityType = GetRealEntityType(entity.GetType());
            var entityTypeName = entityType.Name;
            var changedFields = new List<string>();

            // Lista de propriedades que devem gerar mensagens específicas
            var importantFields = new Dictionary<string, string>
            {
                { "Nome", "Nome" },
                { "Name", "Nome" },
                { "Descricao", "Descrição" },
                { "IsActive", "Status" },
                { "Ativo", "Status" },
                { "DataHoraRemocao", "Remoção" },
                { "UsuarioRemocao", "Usuário de Remoção" }
            };

            foreach (var change in changes)
            {
                if (change.Key == "_operation")
                    continue;

                var propertyName = change.Key;
                var oldValue = change.Value.ContainsKey("oldValue") ? change.Value["oldValue"]?.ToString() : null;
                var newValue = change.Value.ContainsKey("newValue") ? change.Value["newValue"]?.ToString() : null;

                // 🔥 FIX: Ignorar campos onde oldValue e newValue são iguais (após normalização)
                // Isso evita registrar campos que não mudaram realmente
                if (AreEqualStringValues(oldValue, newValue))
                    continue;

                // Verificar se é internalChanges (mudanças em entidade relacionada)
                if (change.Value.ContainsKey("internalChanges") && change.Value["internalChanges"] is Dictionary<string, Dictionary<string, object?>> internalChanges)
                {
                    // Processar apenas os campos internos que realmente mudaram
                    foreach (var internalChange in internalChanges)
                    {
                        var internalOldValue = internalChange.Value.ContainsKey("oldValue") ? internalChange.Value["oldValue"]?.ToString() : null;
                        var internalNewValue = internalChange.Value.ContainsKey("newValue") ? internalChange.Value["newValue"]?.ToString() : null;

                        // Ignorar se os valores são iguais
                        if (AreEqualStringValues(internalOldValue, internalNewValue))
                            continue;

                        var internalFieldName = internalChange.Key;
                        if (importantFields.ContainsKey(internalFieldName))
                        {
                            var fieldDisplayName = importantFields[internalFieldName];
                            changedFields.Add($"{fieldDisplayName} alterado");
                        }
                        else
                        {
                            changedFields.Add($"{internalFieldName} alterado");
                        }
                    }
                    continue;
                }

                if (importantFields.ContainsKey(propertyName))
                {
                    var fieldDisplayName = importantFields[propertyName];

                    if (propertyName == "IsActive" || propertyName == "Ativo")
                    {
                        var status = newValue == "True" || newValue == "true" ? "ativado" : "desativado";
                        changedFields.Add($"{fieldDisplayName} {status}");
                    }
                    else if (propertyName == "DataHoraRemocao")
                    {
                        if (newValue != null && oldValue == null)
                        {
                            changedFields.Add("marcado para remoção");
                        }
                    }
                    else if (!string.IsNullOrEmpty(newValue))
                    {
                        changedFields.Add($"{fieldDisplayName} alterado para \"{TruncateValue(newValue, 50)}\"");
                    }
                }
                else
                {
                    // Campo genérico
                    changedFields.Add($"{propertyName} alterado");
                }
            }

            if (changedFields.Count > 0)
            {
                var changesText = string.Join(", ", changedFields);

                // Incluir informação do pai se disponível
                string parentContext = "";
                if (parentInfo.HasValue)
                {
                    parentContext = !string.IsNullOrEmpty(parentInfo.Value.ParentName)
                        ? $" vinculado ao {parentInfo.Value.ParentType} '{parentInfo.Value.ParentName}'"
                        : $" vinculado ao {parentInfo.Value.ParentType} (ID: {parentInfo.Value.ParentId})";
                }

                if (!string.IsNullOrEmpty(entityDisplayName))
                {
                    return $"Atualizado {entityTypeName} '{entityDisplayName}'{parentContext} - {changesText}";
                }
                return $"Atualizado {entityTypeName} (ID: {entity.Id}){parentContext} - {changesText}";
            }

            return null;
        }

        private string? GetEntityDisplayName<T>(T entity) where T : EntityBaseCore
        {
            if (entity == null)
                return null;

            // Tentar encontrar propriedades comuns de nome/descrição
            var nameProperties = new[] { "Nome", "Name", "Descricao", "Description", "Titulo", "Title" };

            foreach (var propName in nameProperties)
            {
                var prop = entity.GetType().GetProperty(propName);
                if (prop != null)
                {
                    var value = prop.GetValue(entity)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        private string TruncateValue(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value ?? "";

            return value.Substring(0, maxLength - 3) + "...";
        }

        private (string ParentType, int ParentId, string? ParentName)? DetectParentEntity<T>(T entity) where T : EntityBaseCore
        {
            try
            {
                if (entity == null)
                    return null;

                var entityType = entity.GetType();
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Propriedades que não devem ser consideradas como relacionamentos pai
                var excludedPropertyNames = new HashSet<string>
                {
                    "Id", "UsuarioCriacao", "UsuarioAlteracao", "DataHoraCriacao",
                    "DataHoraAlteracao", "ObjectGuid", "DataHoraRemocao", "UsuarioRemocao"
                };

                // Propriedades que indicam entidades intermediárias (não são o pai principal)
                var intermediateEntityNames = new HashSet<string>
                {
                    "Tags", "Pessoa", "Empresa", "Usuario"
                };

                EntityBaseCore? parentEntity = null;
                string? parentPropertyName = null;
                int maxParentNameLength = 0;

                foreach (var prop in properties)
                {
                    // Ignorar propriedades excluídas
                    if (excludedPropertyNames.Contains(prop.Name))
                        continue;

                    // Verificar se a propriedade é do tipo EntityBaseCore
                    if (typeof(EntityBaseCore).IsAssignableFrom(prop.PropertyType) &&
                        prop.PropertyType != typeof(EntityBaseCore))
                    {
                        var propValue = prop.GetValue(entity);
                        if (propValue is EntityBaseCore relatedEntity && relatedEntity.Id > 0)
                        {
                            // 🔥 FIX: Obter tipo real removendo sufixo "Proxy"
                            var relatedEntityType = GetRealEntityType(relatedEntity.GetType());

                            // Se for uma entidade intermediária (Tags, Pessoa, etc.), pular
                            if (intermediateEntityNames.Contains(relatedEntityType.Name))
                                continue;

                            // Preferir a entidade pai com nome mais longo (geralmente é a principal)
                            var parentName = GetEntityName(relatedEntity, "Nome");
                            var nameLength = parentName?.Length ?? 0;

                            if (nameLength > maxParentNameLength || parentEntity == null)
                            {
                                parentEntity = relatedEntity;
                                parentPropertyName = prop.Name;
                                maxParentNameLength = nameLength;
                            }
                        }
                    }
                }

                if (parentEntity != null)
                {
                    // 🔥 FIX: Obter tipo real removendo sufixo "Proxy"
                    var parentEntityType = GetRealEntityType(parentEntity.GetType());
                    var parentName = GetEntityName(parentEntity, "Nome");
                    return (parentEntityType.Name, parentEntity.Id, parentName);
                }

                return null;
            }
            catch
            {
                // Ignorar erros ao detectar relacionamento
                return null;
            }
        }

        private string? GetEntityName(object? entity, string propertyName)
        {
            if (entity == null)
                return null;

            try
            {
                var prop = entity.GetType().GetProperty(propertyName);
                if (prop != null)
                {
                    var value = prop.GetValue(entity);
                    return value?.ToString();
                }

                // 🔥 MELHORIA: Para Usuario, tentar propriedades alternativas se "Nome" não existir
                if (entity.GetType().Name == "Usuario")
                {
                    // Tentar propriedades comuns de nome para Usuario
                    var alternativeProps = new[] { "Login", "Email", "NomeCompleto", "Name" };
                    foreach (var altPropName in alternativeProps)
                    {
                        var altProp = entity.GetType().GetProperty(altPropName);
                        if (altProp != null)
                        {
                            var altValue = altProp.GetValue(entity);
                            if (altValue != null && !string.IsNullOrWhiteSpace(altValue.ToString()))
                            {
                                return altValue.ToString();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignorar erros
            }

            return null;
        }

        public async Task LogUpdateAsync<T>(T oldEntity, T newEntity) where T : EntityBaseCore
        {
            try
            {
                if (oldEntity == null || newEntity == null)
                    return;

                var changes = CompareEntities(oldEntity, newEntity);
                if (changes.Count == 0)
                    return; // Nenhuma mudança relevante

                // 🔥 MELHORIA: Garantir que entidades relacionadas (especialmente Tags) estejam carregadas
                // Isso é importante para capturar nomes de tags em operações de adição/remoção
                EnsureRelatedEntitiesLoaded(newEntity);

                // Gerar mensagem amigável para atualização
                var friendlyMessage = GenerateFriendlyMessageForOperation(newEntity, EnumAuditAction.Update, changes);

                // Adicionar mensagem amigável geral se disponível
                if (!string.IsNullOrEmpty(friendlyMessage))
                {
                    if (!changes.ContainsKey("_operation"))
                    {
                        changes["_operation"] = new Dictionary<string, object?>
                        {
                            { "friendlyMessage", friendlyMessage }
                        };
                    }
                }

                // 🔥 FIX: Obter tipo real removendo sufixo "Proxy"
                var entityType = GetRealEntityType(newEntity.GetType());

                var context = GetAuditContext();
                var message = new AuditLogMessageEvent
                {
                    EntityType = entityType.Name,
                    EntityId = newEntity.Id,
                    Action = (int)EnumAuditAction.Update,
                    UserId = newEntity.UsuarioAlteracao,
                    Timestamp = newEntity.DataHoraAlteracao ?? DateTime.Now,
                    IpAddress = context.IpAddress,
                    UserAgent = context.UserAgent,
                    ChangesJson = JsonSerializer.Serialize(changes, _jsonOptions),
                    ObjectGuid = newEntity.ObjectGuid
                };

                await _auditQueueProducer.EnqueueAuditLogAsync(message);
            }
            catch (Exception)
            {
                // Não lançar exceção para não quebrar a operação principal
            }
        }

        public async Task LogDeleteAsync<T>(T entity) where T : EntityBaseCore
        {
            try
            {
                var context = GetAuditContext();

                // 🔥 FIX: Tentar capturar UserId da entidade primeiro (UsuarioRemocao > UsuarioAlteracao > UsuarioCriacao > HTTP Context)
                int? userId = null;

                // 1. Verificar se a entidade tem UsuarioRemocao (soft delete)
                var usuarioRemocaoProperty = entity.GetType().GetProperty("UsuarioRemocao");
                if (usuarioRemocaoProperty != null)
                {
                    var usuarioRemocao = usuarioRemocaoProperty.GetValue(entity) as int?;
                    if (usuarioRemocao.HasValue && usuarioRemocao.Value > 0)
                    {
                        userId = usuarioRemocao;
                    }
                }

                // 2. Verificar se a entidade tem UsuarioRemocao (soft delete)
                usuarioRemocaoProperty = entity.GetType().GetProperty("UsuarioRemocaoId");
                if (usuarioRemocaoProperty != null)
                {
                    var usuarioRemocao = usuarioRemocaoProperty.GetValue(entity) as int?;
                    if (usuarioRemocao.HasValue && usuarioRemocao.Value > 0)
                    {
                        userId = usuarioRemocao;
                    }
                }


                // 3. Por último, usar o contexto HTTP
                if (!userId.HasValue || userId.Value == 0)
                {
                    userId = context.UserId;
                }

                // Gerar mensagem amigável para exclusão
                var friendlyMessage = GenerateFriendlyMessageForOperation(entity, EnumAuditAction.Delete, null);

                // 🔥 MELHORIA: Para entidades Tags, capturar informações detalhadas para o ChangesJson
                var changes = new Dictionary<string, Dictionary<string, object?>>();

                if (!string.IsNullOrEmpty(friendlyMessage))
                {
                    changes["_operation"] = new Dictionary<string, object?>
                    {
                        { "friendlyMessage", friendlyMessage }
                    };
                }

                // 🔥 FIX: Obter tipo real removendo sufixo "Proxy"
                var entityType = GetRealEntityType(entity.GetType());

                // Para entidades de Tags, capturar informações detalhadas
                if (entityType.Name.EndsWith("Tags"))
                {
                    // Para DELETE, estruturar dados como informações da operação, não como mudanças
                    var operationData = new Dictionary<string, object?>();

                    // Adicionar dados da entidade removida
                    var entityData = SerializeEntityDataForChanges(entity);
                    if (entityData != null && entityData.Count > 0)
                    {
                        operationData["DadosRemovidos"] = entityData;
                    }

                    // Capturar informações adicionais (hierárquicas)
                    var detailedInfo = CaptureTagDeletionDetails(entity);
                    if (detailedInfo != null && detailedInfo.Count > 0)
                    {
                        foreach (var item in detailedInfo)
                        {
                            operationData[item.Key] = item.Value;
                        }
                    }

                    // Estruturar como dados da operação, não como mudanças
                    if (operationData.Count > 0)
                    {
                        if (!changes.ContainsKey("_operation"))
                        {
                            changes["_operation"] = new Dictionary<string, object?>();
                        }

                        changes["_operation"]["operationDetails"] = operationData;
                    }
                }

                var changesJson = changes.Count > 0
                    ? JsonSerializer.Serialize(changes, _jsonOptions)
                    : "{}";

                var message = new AuditLogMessageEvent
                {
                    EntityType = entityType.Name,
                    EntityId = entity.Id,
                    Action = (int)EnumAuditAction.Delete,
                    UserId = userId,
                    Timestamp = DateTime.Now,
                    IpAddress = context.IpAddress,
                    UserAgent = context.UserAgent,
                    ChangesJson = changesJson,
                    EntityDataJson = SerializeEntity(entity),
                    ObjectGuid = entity.ObjectGuid
                };

                await _auditQueueProducer.EnqueueAuditLogAsync(message);
            }
            catch (Exception)
            {
                // Não lançar exceção para não quebrar a operação principal
            }
        }

        private Dictionary<string, Dictionary<string, object?>> CompareEntities<T>(T oldEntity, T newEntity) where T : EntityBaseCore
        {
            var changes = new Dictionary<string, Dictionary<string, object?>>();

            // 🔥 FIX: Use GetType() para obter o tipo REAL da instância, não o tipo genérico T
            var actualType = newEntity.GetType();
            var properties = GetProperties(actualType);

            foreach (var prop in properties)
            {
                if (_excludedProperties.Contains(prop.Name))
                    continue;

                try
                {
                    var oldValue = prop.GetValue(oldEntity);
                    var newValue = prop.GetValue(newEntity);

                    // Verificar se é uma coleção
                    if (IsCollection(prop.PropertyType))
                    {
                        var collectionChanges = CompareCollections(oldValue, newValue, prop.Name);
                        if (collectionChanges != null)
                        {
                            changes[prop.Name] = collectionChanges;
                        }
                    }
                    else
                    {
                        // Verificar se é uma entidade relacionada (ManyToOne)
                        if (oldValue is EntityBaseCore oldRelatedEntity && newValue is EntityBaseCore newRelatedEntity)
                        {
                            // 🔥 FIX: Obter tipos reais removendo sufixo "Proxy"
                            var oldEntityType = GetRealEntityType(oldRelatedEntity.GetType());
                            var newEntityType = GetRealEntityType(newRelatedEntity.GetType());

                            // Se os IDs são diferentes ou tipos diferentes, é uma mudança na referência
                            if (oldRelatedEntity.Id != newRelatedEntity.Id || oldEntityType != newEntityType)
                            {
                                var changeDict = new Dictionary<string, object?>
                                {
                                    { "oldValue", FormatValue(oldValue) },
                                    { "newValue", FormatValue(newValue) },
                                    { "oldEntityType", oldEntityType.Name },
                                    { "oldEntityId", oldRelatedEntity.Id },
                                    { "newEntityType", newEntityType.Name },
                                    { "newEntityId", newRelatedEntity.Id }
                                };

                                // Tentar obter nome da entidade antiga
                                var oldEntityName = GetEntityName(oldRelatedEntity, "Nome");
                                if (!string.IsNullOrEmpty(oldEntityName))
                                    changeDict["oldEntityName"] = oldEntityName;

                                // Tentar obter nome da entidade nova
                                var newEntityName = GetEntityName(newRelatedEntity, "Nome");
                                if (!string.IsNullOrEmpty(newEntityName))
                                    changeDict["newEntityName"] = newEntityName;

                                // 🔥 MELHORIA: Se for uma propriedade de Tag, adicionar informações detalhadas
                                var propNameLower = prop.Name.ToLower();
                                if (propNameLower.Contains("tag") || oldEntityType.Name.ToLower().Contains("tag") || newEntityType.Name.ToLower().Contains("tag"))
                                {
                                    // Adicionar informações específicas da tag
                                    if (!string.IsNullOrEmpty(oldEntityName))
                                        changeDict["oldTagNome"] = oldEntityName;
                                    if (!string.IsNullOrEmpty(newEntityName))
                                        changeDict["newTagNome"] = newEntityName;
                                }

                                changes[prop.Name] = changeDict;
                            }
                            // 🔥 FIX: Se os IDs são iguais, NÃO fazer comparação profunda
                            // Isso evita falsos positivos causados por lazy loading do NHibernate
                            // Se o ID é o mesmo, significa que é a mesma entidade, então não houve mudança na referência
                            // Mudanças internas em entidades relacionadas devem ser capturadas salvando a entidade relacionada diretamente
                            // else if (oldRelatedEntity.GetType() == newRelatedEntity.GetType())
                            // {
                            //     // REMOVIDO: Comparação profunda de propriedades quando IDs são iguais
                            //     // Isso causava falsos positivos porque campos podem não estar carregados (lazy loading)
                            // }
                        }
                        // Para outros tipos, usar comparação normal
                        else if (!AreEqual(oldValue, newValue))
                        {
                            var formattedOldValue = FormatValue(oldValue);
                            var formattedNewValue = FormatValue(newValue);

                            // 🔥 FIX: Para strings, distinguir entre null e string vazia
                            // null e "" devem ser tratados como diferentes (mudança real)
                            bool isStringChange = oldValue is string || newValue is string;
                            bool shouldLogChange = false;

                            if (isStringChange)
                            {
                                // Para strings, comparar diretamente (null != "")
                                var oldStr = oldValue?.ToString();
                                var newStr = newValue?.ToString();

                                // null e string vazia são diferentes
                                if (oldStr == null && newStr == "")
                                    shouldLogChange = true;
                                else if (oldStr == "" && newStr == null)
                                    shouldLogChange = true;
                                // Para outros casos, usar comparação normalizada (trim)
                                else if (!AreEqualStringValues(formattedOldValue, formattedNewValue))
                                    shouldLogChange = true;
                            }
                            else
                            {
                                // Para não-strings, usar comparação normalizada
                                shouldLogChange = !AreEqualStringValues(formattedOldValue, formattedNewValue);
                            }

                            if (shouldLogChange)
                            {
                                var changeDict = new Dictionary<string, object?>
                                {
                                    { "oldValue", formattedOldValue },
                                    { "newValue", formattedNewValue }
                                };

                                changes[prop.Name] = changeDict;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignorar erros ao comparar propriedades individuais
                    // Continuar com as outras propriedades
                }
            }

            return changes;
        }

        /// <summary>
        /// Compara as propriedades de uma entidade relacionada (não apenas EntityBaseCore)
        /// Retorna um dicionário com as propriedades que mudaram
        /// </summary>
        private Dictionary<string, Dictionary<string, object?>> CompareRelatedEntityProperties(EntityBaseCore oldEntity, EntityBaseCore newEntity)
        {
            var changes = new Dictionary<string, Dictionary<string, object?>>();

            if (oldEntity.GetType() != newEntity.GetType())
                return changes;

            // Obter TODAS as propriedades do tipo específico (incluindo herdadas, não apenas EntityBaseCore)
            // Usar FlattenHierarchy para incluir propriedades da classe base
            var entityType = oldEntity.GetType();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(p => p.CanRead)
                .ToArray();

            foreach (var prop in properties)
            {
                // Ignorar propriedades excluídas (campos de auditoria da EntityBaseCore)
                if (_excludedProperties.Contains(prop.Name))
                    continue;

                // Ignorar propriedades que são outras entidades relacionadas (ManyToOne)
                // Isso evita recursão infinita e compara apenas propriedades primitivas/strings da entidade relacionada
                if (typeof(EntityBaseCore).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(EntityBaseCore))
                    continue;

                // Ignorar coleções (OneToMany)
                if (IsCollection(prop.PropertyType))
                    continue;

                try
                {
                    var oldValue = prop.GetValue(oldEntity);
                    var newValue = prop.GetValue(newEntity);

                    // Verificar se realmente há mudança
                    if (!AreEqual(oldValue, newValue))
                    {
                        var formattedOldValue = FormatValue(oldValue);
                        var formattedNewValue = FormatValue(newValue);

                        // 🔥 FIX: Para strings, distinguir entre null e string vazia
                        // null e "" devem ser tratados como diferentes (mudança real)
                        bool isStringChange = oldValue is string || newValue is string;
                        bool shouldLogChange = false;

                        if (isStringChange)
                        {
                            // Para strings, comparar diretamente (null != "")
                            var oldStr = oldValue?.ToString();
                            var newStr = newValue?.ToString();

                            // null e string vazia são diferentes
                            if (oldStr == null && newStr == "")
                                shouldLogChange = true;
                            else if (oldStr == "" && newStr == null)
                                shouldLogChange = true;
                            // Para outros casos, usar comparação normalizada (trim)
                            else if (!AreEqualStringValues(formattedOldValue, formattedNewValue))
                                shouldLogChange = true;
                        }
                        else
                        {
                            // Para não-strings, usar comparação normalizada
                            shouldLogChange = !AreEqualStringValues(formattedOldValue, formattedNewValue);
                        }

                        if (shouldLogChange)
                        {
                            changes[prop.Name] = new Dictionary<string, object?>
                            {
                                { "oldValue", formattedOldValue },
                                { "newValue", formattedNewValue }
                            };
                        }
                    }
                }
                catch
                {
                    // Ignorar erros ao comparar propriedades individuais
                }
            }

            return changes;
        }

        private bool IsCollection(Type type)
        {
            if (type == typeof(string))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(type) &&
                   type != typeof(byte[]);
        }

        private Dictionary<string, object?>? CompareCollections(object? oldCollection, object? newCollection, string propertyName)
        {
            var oldList = oldCollection as IEnumerable;
            var newList = newCollection as IEnumerable;

            // Se ambos são null, não há mudança
            if (oldList == null && newList == null)
                return null;

            // Se um é null e outro não, há mudança
            if (oldList == null || newList == null)
            {
                return new Dictionary<string, object?>
                {
                    { "oldValue", oldList == null ? "[]" : FormatCollection(oldList) },
                    { "newValue", newList == null ? "[]" : FormatCollection(newList) }
                };
            }

            // Comparar coleções
            var oldItems = GetCollectionItems(oldList);
            var newItems = GetCollectionItems(newList);

            // Se os IDs são diferentes, há mudança
            if (!oldItems.SequenceEqual(newItems))
            {
                return new Dictionary<string, object?>
                {
                    { "oldValue", FormatCollection(oldList) },
                    { "newValue", FormatCollection(newList) }
                };
            }

            return null;
        }

        private List<string> GetCollectionItems(IEnumerable collection)
        {
            var items = new List<string>();
            foreach (var item in collection)
            {
                if (item == null)
                    continue;

                // Se o item é uma entidade com ID, usar o ID
                if (item is EntityBaseCore entity)
                {
                    // 🔥 FIX: Obter o tipo real da entidade, removendo sufixo "Proxy" do NHibernate
                    var entityType = GetRealEntityType(item.GetType());
                    items.Add($"{entityType.Name}[Id:{entity.Id}]");
                }
                else
                {
                    items.Add(item.ToString() ?? "null");
                }
            }
            return items.OrderBy(x => x).ToList();
        }

        private string FormatCollection(IEnumerable collection)
        {
            var items = GetCollectionItems(collection);
            return $"[{string.Join(", ", items)}]";
        }

        private string? FormatValue(object? value)
        {
            if (value == null)
                return null;

            if (value is EntityBaseCore entity)
            {
                // 🔥 FIX: Obter o tipo real da entidade, removendo sufixo "Proxy" do NHibernate
                var entityType = GetRealEntityType(value.GetType());

                // 🔥 FIX: Tentar obter nome da entidade para exibir junto com ID
                var entityName = GetEntityDisplayName(entity);
                if (!string.IsNullOrEmpty(entityName))
                {
                    return $"{entityType.Name}[Id:{entity.Id}]";
                }

                return $"{entityType.Name}[Id:{entity.Id}]";
            }

            // 🔥 FIX: Formatar datas sempre no padrão dd/MM/yyyy sem horário e sem UTC
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd/MM/yyyy");
            }

            // Para DateTime nullable
            var nullableUnderlyingType = Nullable.GetUnderlyingType(value.GetType());
            if (nullableUnderlyingType == typeof(DateTime))
            {
                var nullableDateTime = value as DateTime?;
                if (nullableDateTime.HasValue)
                {
                    return nullableDateTime.Value.ToString("dd/MM/yyyy");
                }
            }

            return value.ToString();
        }

        /// <summary>
        /// Obtém o tipo real da entidade, removendo o sufixo "Proxy" do NHibernate
        /// </summary>
        private Type GetRealEntityType(Type type)
        {
            // Se o nome do tipo termina com "Proxy", obter o tipo base real
            if (type.Name.EndsWith("Proxy", StringComparison.OrdinalIgnoreCase))
            {
                // Obter a cadeia de tipos base até encontrar um que não seja Proxy
                var baseType = type.BaseType;
                while (baseType != null)
                {
                    // Parar se encontrar Object ou EntityBaseCore como base direta
                    if (baseType == typeof(object) || baseType == typeof(EntityBaseCore))
                    {
                        // Neste caso, o tipo base é muito genérico
                        // Retornar o último tipo concreto antes de EntityBaseCore
                        var currentType = type;
                        while (currentType.BaseType != null &&
                               currentType.BaseType != typeof(EntityBaseCore) &&
                               currentType.BaseType != typeof(object))
                        {
                            currentType = currentType.BaseType;
                        }

                        // Se encontrou um tipo concreto, usar ele
                        if (currentType != typeof(EntityBaseCore) && currentType != typeof(object))
                        {
                            return currentType;
                        }

                        break;
                    }

                    // Se encontrou um tipo base que não termina com "Proxy" e não é Object/EntityBaseCore
                    if (!baseType.Name.EndsWith("Proxy", StringComparison.OrdinalIgnoreCase) &&
                        typeof(EntityBaseCore).IsAssignableFrom(baseType) &&
                        baseType != typeof(EntityBaseCore))
                    {
                        return baseType;
                    }

                    baseType = baseType.BaseType;
                }
            }

            return type;
        }

        /// <summary>
        /// Compara dois valores de string após normalização (trim e tratamento de null/vazio)
        /// </summary>
        private bool AreEqualStringValues(string? value1, string? value2)
        {
            // Normalizar valores null e string vazia como equivalentes
            var normalized1 = string.IsNullOrWhiteSpace(value1) ? null : value1.Trim();
            var normalized2 = string.IsNullOrWhiteSpace(value2) ? null : value2.Trim();

            return normalized1 == normalized2;
        }

        private PropertyInfo[] GetProperties(Type type)
        {
            return _propertyCache.GetOrAdd(type, t =>
            {
                // Incluir propriedades declaradas e herdadas (incluindo da classe base)
                return t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(p => p.CanRead && IsPropertyTypeSupported(p.PropertyType))
                    .ToArray();
            });
        }

        /// <summary>
        /// Verifica se um tipo de propriedade é suportado para auditoria.
        /// Suporta tipos primitivos, strings, tipos de valor, enums, nullable, coleções e entidades.
        /// 🔥 MELHORIA: Versão genérica que captura todas as propriedades relevantes automaticamente.
        /// </summary>
        private bool IsPropertyTypeSupported(Type propertyType)
        {
            // 🔥 MELHORIA: Tratar tipos nullable primeiro para simplificar a lógica
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            if (underlyingType != null)
            {
                // Se é nullable, verificar o tipo subjacente
                return IsPropertyTypeSupported(underlyingType);
            }

            // Tipos primitivos (int, long, short, byte, char, bool, double, float, etc.)
            if (propertyType.IsPrimitive)
                return true;

            // String
            if (propertyType == typeof(string))
                return true;

            // 🔥 MELHORIA: Aceitar qualquer tipo de valor (struct) de forma genérica
            // Isso inclui: DateTime, Guid, TimeSpan, DateOnly, TimeOnly, Decimal, e qualquer outro struct
            if (propertyType.IsValueType)
            {
                // Excluir apenas tipos que não queremos (como ponteiros)
                if (!propertyType.IsPointer && !propertyType.IsByRef)
                    return true;
            }

            // 🔥 MELHORIA: Aceitar qualquer enum de forma genérica (não apenas os específicos)
            if (propertyType.IsEnum)
                return true;

            // Coleções (IEnumerable, List, Array, etc.) - exceto string e byte[]
            if (IsCollection(propertyType))
                return true;

            // Entidades (EntityBaseCore e suas subclasses)
            if (typeof(EntityBaseCore).IsAssignableFrom(propertyType))
                return true;

            return false;
        }


        private bool AreEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null)
                return true;
            if (value1 == null || value2 == null)
                return false;

            // Para entidades relacionadas (ManyToOne), comparar por ID
            if (value1 is EntityBaseCore entity1 && value2 is EntityBaseCore entity2)
            {
                return entity1.Id == entity2.Id && entity1.GetType() == entity2.GetType();
            }

            // Para strings, fazer comparação normalizada (trim e case-sensitive)
            if (value1 is string str1 && value2 is string str2)
            {
                // Comparar strings normalizadas (trim para remover espaços extras)
                return string.Equals(str1?.Trim(), str2?.Trim(), StringComparison.Ordinal);
            }

            // 🔥 FIX: Para DateTime, comparar apenas a data (ignorar horário e UTC)
            if (value1 is DateTime dt1 && value2 is DateTime dt2)
            {
                return dt1.Date == dt2.Date;
            }

            // Para DateTime nullable
            var underlyingType1 = Nullable.GetUnderlyingType(value1.GetType());
            var underlyingType2 = Nullable.GetUnderlyingType(value2.GetType());

            if (underlyingType1 == typeof(DateTime) && underlyingType2 == typeof(DateTime))
            {
                var dtNullable1 = value1 as DateTime?;
                var dtNullable2 = value2 as DateTime?;

                if (!dtNullable1.HasValue && !dtNullable2.HasValue)
                    return true;
                if (!dtNullable1.HasValue || !dtNullable2.HasValue)
                    return false;
                return dtNullable1.Value.Date == dtNullable2.Value.Date;
            }

            // Para tipos primitivos, usar Equals
            if (value1.GetType().IsPrimitive)
            {
                return value1.Equals(value2);
            }

            // Para tipos nullable, comparar valores subjacentes
            if (value1.GetType().IsGenericType && value1.GetType().GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(value1.GetType());
                if (underlyingType != null)
                {
                    // Se for string nullable, comparar como string
                    if (underlyingType == typeof(string))
                    {
                        var str1Nullable = value1 as string;
                        var str2Nullable = value2 as string;
                        return string.Equals(str1Nullable?.Trim(), str2Nullable?.Trim(), StringComparison.Ordinal);
                    }

                    // Para outros tipos nullable primitivos, comparar valores
                    if (underlyingType.IsPrimitive || underlyingType == typeof(decimal))
                    {
                        return value1.Equals(value2);
                    }
                }
            }

            // Fallback para Equals padrão
            return value1.Equals(value2);
        }

        private string SerializeEntity<T>(T entity) where T : EntityBaseCore
        {
            try
            {
                var properties = GetProperties(typeof(T));
                var data = new Dictionary<string, object?>();

                foreach (var prop in properties)
                {
                    if (_excludedProperties.Contains(prop.Name))
                        continue;

                    data[prop.Name] = prop.GetValue(entity);
                }

                return JsonSerializer.Serialize(data, _jsonOptions);
            }
            catch
            {
                return "{}";
            }
        }

        /// <summary>
        /// Serializa os dados da entidade para incluir no ChangesJson durante criação
        /// Inclui apenas propriedades relevantes (não relacionamentos complexos)
        /// </summary>
        private Dictionary<string, object?>? SerializeEntityDataForChanges<T>(T entity) where T : EntityBaseCore
        {
            try
            {
                var entityType = entity.GetType();
                var properties = GetProperties(entityType);
                var data = new Dictionary<string, object?>();

                foreach (var prop in properties)
                {
                    if (_excludedProperties.Contains(prop.Name))
                        continue;

                    try
                    {
                        var value = prop.GetValue(entity);

                        // Para entidades relacionadas (ManyToOne), incluir apenas ID e nome se disponível
                        if (value is EntityBaseCore relatedEntity)
                        {
                            // 🔥 FIX: Obter tipo real removendo sufixo "Proxy"
                            var relatedEntityType = GetRealEntityType(relatedEntity.GetType());

                            var relatedData = new Dictionary<string, object?>
                            {
                                { "Id", relatedEntity.Id },
                                { "Type", relatedEntityType.Name }
                            };

                            // Tentar obter nome da entidade relacionada
                            var nome = GetEntityName(relatedEntity, "Nome");
                            if (!string.IsNullOrEmpty(nome))
                            {
                                relatedData["Nome"] = nome;
                            }
                            else
                            {
                                // Para Usuario, tentar Login
                                if (relatedEntityType.Name == "Usuario")
                                {
                                    var login = GetEntityName(relatedEntity, "Login");
                                    if (!string.IsNullOrEmpty(login))
                                        relatedData["Login"] = login;
                                }
                            }

                            data[prop.Name] = relatedData;
                        }
                        // Para tipos primitivos, strings, enums, incluir diretamente
                        else if (value != null && (
                            prop.PropertyType.IsPrimitive ||
                            prop.PropertyType == typeof(string) ||
                            prop.PropertyType == typeof(DateTime) ||
                            prop.PropertyType == typeof(DateTime?) ||
                            prop.PropertyType == typeof(decimal) ||
                            prop.PropertyType == typeof(decimal?) ||
                            prop.PropertyType == typeof(int) ||
                            prop.PropertyType == typeof(int?) ||
                            prop.PropertyType == typeof(bool) ||
                            prop.PropertyType == typeof(bool?) ||
                            prop.PropertyType.IsEnum ||
                            (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum == true)))
                        {
                            data[prop.Name] = FormatValue(value);
                        }
                        // Ignorar coleções e outros tipos complexos
                    }
                    catch
                    {
                        // Ignorar erros ao serializar propriedades individuais
                    }
                }

                return data.Count > 0 ? data : null;
            }
            catch
            {
                return null;
            }
        }

        private Dictionary<string, object?>? CaptureTagDeletionDetails<T>(T entity) where T : EntityBaseCore
        {
            try
            {
                var entityType = entity.GetType();
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var details = new Dictionary<string, object?>();

                // Detectar propriedades de relacionamento usando reflection
                PropertyInfo? parentProperty = null;

                foreach (var prop in properties)
                {
                    var propName = prop.Name.ToLower();

                    // Detectar propriedade pai principal (ImagemGrupoImagem, GrupoImagem, etc.)
                    // Ignorar Tags pois já está nos DadosRemovidos
                    if ((propName.Contains("imagem") || propName.Contains("grupo")) &&
                        !propName.Contains("tag") &&
                        typeof(EntityBaseCore).IsAssignableFrom(prop.PropertyType))
                    {
                        parentProperty = prop;
                        break; // Pegar apenas o primeiro (principal)
                    }
                }

                // Capturar apenas informações hierárquicas relevantes
                if (parentProperty != null)
                {
                    var parent = parentProperty.GetValue(entity);
                    if (parent is EntityBaseCore parentEntity)
                    {
                        var hasAdditionalInfo = false;

                        // Para imagens, capturar informações do grupo (se disponível e diferente)
                        if (parentProperty.PropertyType.Name.Contains("Imagem"))
                        {
                            var grupoProperty = parentEntity.GetType().GetProperties()
                                .FirstOrDefault(p => p.Name.Contains("Grupo") &&
                                               typeof(EntityBaseCore).IsAssignableFrom(p.PropertyType));

                            if (grupoProperty != null)
                            {
                                var grupo = grupoProperty.GetValue(parentEntity);
                                if (grupo is EntityBaseCore grupoEntity)
                                {
                                    var grupoNome = GetEntityName(grupoEntity, "Nome");
                                    if (!string.IsNullOrEmpty(grupoNome))
                                    {
                                        details["GrupoContexto"] = $"{grupoNome} (ID: {grupoEntity.Id})";
                                        hasAdditionalInfo = true;
                                    }

                                    // Tentar capturar empresa do grupo
                                    var empresaPropGrupo = grupoEntity.GetType().GetProperties()
                                        .FirstOrDefault(p => p.Name.Contains("Empresa") &&
                                                       typeof(EntityBaseCore).IsAssignableFrom(p.PropertyType));

                                    if (empresaPropGrupo != null)
                                    {
                                        var empresa = empresaPropGrupo.GetValue(grupoEntity);
                                        if (empresa is EntityBaseCore empresaEntity)
                                        {
                                            var empresaNome = GetEntityName(empresaEntity, "Nome");
                                            if (!string.IsNullOrEmpty(empresaNome))
                                            {
                                                details["EmpresaContexto"] = $"{empresaNome} (ID: {empresaEntity.Id})";
                                                hasAdditionalInfo = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // Para grupos, capturar empresa diretamente (se disponível)
                        else if (parentProperty.PropertyType.Name.Contains("Grupo"))
                        {
                            var empresaPropGrupo = parentEntity.GetType().GetProperties()
                                .FirstOrDefault(p => p.Name.Contains("Empresa") &&
                                               typeof(EntityBaseCore).IsAssignableFrom(p.PropertyType));

                            if (empresaPropGrupo != null)
                            {
                                var empresa = empresaPropGrupo.GetValue(parentEntity);
                                if (empresa is EntityBaseCore empresaEntity)
                                {
                                    var empresaNome = GetEntityName(empresaEntity, "Nome");
                                    if (!string.IsNullOrEmpty(empresaNome))
                                    {
                                        details["EmpresaContexto"] = $"{empresaNome} (ID: {empresaEntity.Id})";
                                        hasAdditionalInfo = true;
                                    }
                                }
                            }
                        }

                        // Só adicionar contexto se temos informações realmente úteis
                        if (hasAdditionalInfo)
                        {
                            details["OperacaoInfo"] = $"Vínculo removido em {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                        }
                    }
                }

                // Retornar apenas se há informações contextuais úteis
                return details.Count > 0 ? details : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private (string? IpAddress, string? UserAgent, int? UserId) GetAuditContext()
        {
            string? ipAddress = null;
            string? userAgent = null;
            int? userId = null;

            if (_httpContextAccessor?.HttpContext != null)
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // IP Address
                ipAddress = httpContext.Items["AuditIpAddress"]?.ToString()
                    ?? httpContext.Connection.RemoteIpAddress?.ToString();

                // User Agent
                userAgent = httpContext.Items["AuditUserAgent"]?.ToString()
                    ?? httpContext.Request.Headers["User-Agent"].FirstOrDefault();

                // User ID (se disponível no contexto)
                if (httpContext.Items.ContainsKey("AuditUserId"))
                {
                    var userIdStr = httpContext.Items["AuditUserId"]?.ToString();
                    if (int.TryParse(userIdStr, out var parsedUserId))
                        userId = parsedUserId;
                }
            }

            return (ipAddress, userAgent, userId);
        }
    }
}

