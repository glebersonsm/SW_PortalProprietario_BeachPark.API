using System.Net.Http.Headers;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Auxiliar;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class RabbitMQQueueService : IRabbitMQQueueService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<RabbitMQQueueService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IConfiguration _configuration;

        public RabbitMQQueueService(
            IRepositoryNH repository,
            ILogger<RabbitMQQueueService> logger,
            IProjectObjectMapper mapper,
            IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<RabbitMQQueueViewModel?> SaveQueue(RabbitMQQueueInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                RabbitMQQueue? queueBd = null;
                if (model.Id.HasValue && model.Id.Value > 0)
                {
                    queueBd = (await _repository.FindByHql<RabbitMQQueue>($"From RabbitMQQueue q Where q.Id = {model.Id.Value}")).FirstOrDefault();
                }

                RabbitMQQueue? queue = queueBd != null 
                    ? _mapper.Map(model, queueBd) 
                    : _mapper.Map<RabbitMQQueue>(model);

                await queue.SaveValidate();
                await _repository.Save(queue);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Fila RabbitMQ: ({queue.Id} - {queue.Nome}) salva com sucesso!");
                    return _mapper.Map<RabbitMQQueueViewModel>(queue);
                }
                throw exception ?? new Exception($"Não foi possível salvar a fila");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a fila RabbitMQ");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<List<RabbitMQQueueViewModel>> GetAllQueues()
        {
            try
            {
                // Sincronizar filas do RabbitMQ com o banco
                await SyncQueuesFromRabbitMQ();

                // Buscar filas do banco e filtrar apenas as que iniciam com PROGRAM_ID
                var programId = Environment.GetEnvironmentVariable("PROGRAM_ID") ?? "PortalClienteBP_";
                var queues = (await _repository.FindByHql<RabbitMQQueue>("From RabbitMQQueue q Order By q.Nome")).AsList();
                var queueViewModels = queues
                    .Where(q => string.IsNullOrWhiteSpace(programId) || q.Nome.StartsWith(programId, StringComparison.OrdinalIgnoreCase))
                    .Select(q => _mapper.Map<RabbitMQQueueViewModel>(q))
                    .ToList();

                // Buscar estatísticas das filas do RabbitMQ
                await EnrichQueuesWithStatistics(queueViewModels);

                return queueViewModels;
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Não foi possível buscar as filas de processamento assíncrono");
                throw;
            }
        }

        private async Task SyncQueuesFromRabbitMQ()
        {
            try
            {
                var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
                var port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int envPort)
                    ? envPort : 5672;
                var user = Environment.GetEnvironmentVariable("RABBITMQ_USER");
                var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS");

                if (string.IsNullOrWhiteSpace(host) || port == 0)
                {
                    _logger.LogWarning("Configurações do RabbitMQ não encontradas, sincronizando apenas filas configuradas");
                    await SyncConfiguredQueues();
                    return;
                }

                _repository.BeginTransaction();

                var programId = Environment.GetEnvironmentVariable("PROGRAM_ID") ?? "PortalClienteBP_";
                _logger.LogInformation($"ProgramId configurado: '{programId}' (filtrando apenas filas que iniciam com este prefixo)");
                var programIdLower = programId!.ToLower();

                var consumerConcurrency = Environment.GetEnvironmentVariable("RABBITMQ_CONSUMER_CONCURRENCY") ?? "5";
                var retryAttempts = Environment.GetEnvironmentVariable("RABBITMQ_RETRY_ATTEMPTS") ?? "3";
                var retryDelaySeconds = Environment.GetEnvironmentVariable("RABBITMQ_RETRY_DELAY_SECONDS") ?? "30";

                // Lista de filas conhecidas com suas configurações
                var knownQueues = new Dictionary<string, (string tipo, string descricao, int? consumerConcurrency, int? retryAttempts, int? retryDelaySeconds)>
                {
                    {
                        $"{programId}{Environment.GetEnvironmentVariable("RABBITMQ_LOG_AUDIT_FILA")}".Replace(" ", "").ToLower(),
                        ("Auditoria", "Fila de processamento de logs de auditoria", 
                         int.Parse(consumerConcurrency),
                         int.Parse(retryAttempts),
                         int.Parse(retryDelaySeconds))
                    },
                    {
                        $"{programId}{Environment.GetEnvironmentVariable("RABBITMQ_LOG_ACESSO_FILA")}".Replace(" ", "").ToLower(),
                        ("Log", "Fila de processamento de logs de acesso", int.Parse(consumerConcurrency),
                         int.Parse(retryAttempts),
                         int.Parse(retryDelaySeconds))
                    },
                    {
                        $"{programId}{Environment.GetEnvironmentVariable("RABBITMQ_EMAIL_FILA")}".Replace(" ", "").ToLower(),
                        ("Email", "Fila de processamento de envio de e-mails", int.Parse(consumerConcurrency) , int.Parse(retryAttempts) , int.Parse(retryDelaySeconds))
                    }
                };

                // Tentar obter TODAS as filas via Management API (lista todas as filas do broker)
                var allDiscoveredQueues = await GetQueuesFromManagementApi(host, user, pass);
                // Filtrar apenas filas que iniciam com PROGRAM_ID
                var discoveredQueues = allDiscoveredQueues
                    .Where(q => !string.IsNullOrWhiteSpace(programId) && q.StartsWith(programId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (allDiscoveredQueues.Count > 0)
                    _logger.LogInformation($"Filtrando filas por PROGRAM_ID '{programId}': {discoveredQueues.Count} de {allDiscoveredQueues.Count} filas");

                // Se Management API não retornou filas, fallback: tentar filas conhecidas via AMQP
                if (discoveredQueues.Count == 0)
                {
                    _logger.LogInformation("Management API não disponível ou sem filas, tentando descobrir filas conhecidas via AMQP");
                    try
                    {
                        var factory = new ConnectionFactory
                        {
                            HostName = host,
                            Port = port,
                            UserName = user ?? "guest",
                            Password = pass ?? "guest25"
                        };

                        using var connection = await factory.CreateConnectionAsync();
                        using var channel = await connection.CreateChannelAsync();

                        var queueAuditoriaNomeRaw =  Environment.GetEnvironmentVariable("RABBITMQ_FILA_AUDITORIA") ??  "Auditoria_BP_";
                        var queueLogNomeRaw = Environment.GetEnvironmentVariable("RABBITMQ_LOG_AUDIT_FILA") ?? "FilaLogAuditPortalClienteBP_";
                        var queueEmailNomeRaw = Environment.GetEnvironmentVariable("RABBITMQ_EMAIL_FILA") ?? "FilaLogAcessoPortalClienteBP_";

                        var queueAuditoriaNomeExact = $"{programId}{queueAuditoriaNomeRaw}".Replace(" ", "");
                        var queueLogNomeExact = $"{programId}{queueLogNomeRaw}".Replace(" ", "");
                        var queueEmailNomeExact = $"{programId}{queueEmailNomeRaw}".Replace(" ", "");

                        var queuesToTry = new[] { queueAuditoriaNomeExact, queueLogNomeExact, queueEmailNomeExact };

                        foreach (var queueNameExact in queuesToTry)
                        {
                            try
                            {
                                var queueInfo = await channel.QueueDeclarePassiveAsync(queueNameExact);
                                discoveredQueues.Add(queueNameExact);
                                _logger.LogInformation($"Fila encontrada no RabbitMQ: {queueNameExact} (mensagens pendentes: {queueInfo.MessageCount})");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug(ex, $"Fila não encontrada no RabbitMQ: {queueNameExact}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ para listar filas, usando apenas configurações");
                        await SyncConfiguredQueues();
                        _repository.Rollback();
                        return;
                    }
                }
                else
                {
                    _logger.LogInformation($"Management API retornou {discoveredQueues.Count} filas do RabbitMQ");
                }

                if (discoveredQueues.Count > 0)
                {

                    // Buscar todas as filas do banco
                    var existingQueuesInDb = (await _repository.FindByHql<RabbitMQQueue>("From RabbitMQQueue q")).ToList();
                    _logger.LogInformation($"Encontradas {existingQueuesInDb.Count} filas existentes no banco de dados");
                    
                    var existingQueuesDict = existingQueuesInDb.GroupBy(q => q.Nome.ToLower())
                        .ToDictionary(g => g.Key, g => g.First());

                    _logger.LogInformation($"Iniciando sincronização de {discoveredQueues.Count} filas descobertas no RabbitMQ");

                    // Sincronizar filas descobertas
                    var syncCount = 0;
                    foreach (var queueName in discoveredQueues)
                    {
                        var queueNameLower = queueName.ToLower();
                        var queueInfo = knownQueues.ContainsKey(queueNameLower) 
                            ? knownQueues[queueNameLower] 
                            : ("Outros", $"Fila de processamento: {queueName}", null, null, null);

                        _logger.LogDebug($"Processando fila: {queueName} (tipo: {queueInfo.tipo})");

                        if (existingQueuesDict.TryGetValue(queueNameLower, out var existingQueue))
                        {
                            // Atualizar se necessário
                            var needsUpdate = false;
                            if (existingQueue.TipoFila != queueInfo.tipo)
                            {
                                existingQueue.TipoFila = queueInfo.tipo;
                                needsUpdate = true;
                            }
                            if (existingQueue.Descricao != queueInfo.descricao)
                            {
                                existingQueue.Descricao = queueInfo.descricao;
                                needsUpdate = true;
                            }
                            if (queueInfo.consumerConcurrency.HasValue && existingQueue.ConsumerConcurrency != queueInfo.consumerConcurrency)
                            {
                                existingQueue.ConsumerConcurrency = queueInfo.consumerConcurrency;
                                needsUpdate = true;
                            }
                            if (queueInfo.retryAttempts.HasValue && existingQueue.RetryAttempts != queueInfo.retryAttempts)
                            {
                                existingQueue.RetryAttempts = queueInfo.retryAttempts;
                                needsUpdate = true;
                            }
                            if (queueInfo.retryDelaySeconds.HasValue && existingQueue.RetryDelaySeconds != queueInfo.retryDelaySeconds)
                            {
                                existingQueue.RetryDelaySeconds = queueInfo.retryDelaySeconds;
                                needsUpdate = true;
                            }

                            if (needsUpdate)
                            {
                                await _repository.Save(existingQueue);
                                _logger.LogInformation($"Fila atualizada: {queueName}");
                            }
                            else
                            {
                                _logger.LogDebug($"Fila {queueName} já está atualizada");
                            }
                        }
                        else
                        {
                            // Criar nova fila
                            var newQueue = new RabbitMQQueue
                            {
                                Nome = queueName,
                                TipoFila = queueInfo.tipo,
                                Descricao = queueInfo.descricao,
                                Ativo = EnumSimNao.Sim,
                                ConsumerConcurrency = queueInfo.consumerConcurrency,
                                RetryAttempts = queueInfo.retryAttempts,
                                RetryDelaySeconds = queueInfo.retryDelaySeconds
                            };

                            await _repository.Save(newQueue);
                            _logger.LogInformation($"Fila criada do RabbitMQ: {queueName} (tipo: {queueInfo.tipo})");
                        }
                        
                        syncCount++;
                    }

                    _logger.LogInformation($"Total de filas sincronizadas do RabbitMQ: {syncCount}");
                }
                else
                {
                    _logger.LogInformation("Nenhuma fila encontrada, executando sincronização de filas configuradas");
                    _repository.Rollback();
                    await SyncConfiguredQueues();
                    return;
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation("Sincronização de filas concluída com sucesso");
                }
                else if (exception != null)
                {
                    _logger.LogWarning(exception, "Erro ao sincronizar filas, mas continuando...");
                    _repository.Rollback();
                }
            }
            catch (Exception err)
            {
                _logger.LogWarning(err, "Erro ao sincronizar filas do RabbitMQ, tentando apenas configurações...");
                _repository.Rollback();
                // Fallback: tentar sincronizar apenas filas configuradas
                try
                {
                    await SyncConfiguredQueues();
                }
                catch (Exception fallbackErr)
                {
                    _logger.LogError(fallbackErr, "Erro também no fallback de sincronização");
                }
            }
        }

        /// <summary>
        /// Obtém todas as filas do RabbitMQ via Management API (HTTP).
        /// Requer o plugin rabbitmq_management habilitado no broker (porta 15672).
        /// Retorna lista vazia se Management API não estiver disponível.
        /// </summary>
        private async Task<List<string>> GetQueuesFromManagementApi(string host, string? user, string? pass)
        {
            var managementPort = Environment.GetEnvironmentVariable("RABBITMQ_MANAGEMENT_PORT") ?? "1572";
            var port = int.TryParse(managementPort, out int p);
            var useHttps = Environment.GetEnvironmentVariable("RABBITMQ_MANAGEMENT_USE_HTTPS") ?? "S";
            var scheme = useHttps == "S" ? "https" : "http";
            var vhost = Environment.GetEnvironmentVariable("RABBITMQ_MANAGEMENT_MQ_VHOST") ?? "/";
            var vhostEncoded = Uri.EscapeDataString(vhost);
            var url = $"{scheme}://{host}:{port}/api/queues/{vhostEncoded}?columns=name";

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                var authValue = Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{user ?? "guest"}:{pass ?? "guest"}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", authValue);

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Management API retornou {StatusCode} para {Url}", response.StatusCode, url);
                    return new List<string>();
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var queues = new List<string>();
                foreach (var elem in doc.RootElement.EnumerateArray())
                {
                    if (elem.TryGetProperty("name", out var nameProp))
                    {
                        var name = nameProp.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                            queues.Add(name);
                    }
                }
                return queues;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Não foi possível obter filas via Management API em {Url}", url);
                return new List<string>();
            }
        }

        private async Task SyncConfiguredQueues()
        {
            try
            {
                _repository.BeginTransaction();

                var programId = Environment.GetEnvironmentVariable("PROGRAM_ID") ?? "PortalClienteBP_";
                _logger.LogInformation($"SyncConfiguredQueues - ProgramId configurado: '{programId}'");

                // Definir filas configuradas no sistema
                var queueAuditoriaNomeRaw = Environment.GetEnvironmentVariable("RABBITMQ_FILA_AUDITORIA") ?? "Auditoria_BP_";
                var queueLogNomeRaw = Environment.GetEnvironmentVariable("RABBITMQ_LOG_AUDIT_FILA") ?? "FilaLogAuditPortalClienteBP_";
                var queueEmailNomeRaw = Environment.GetEnvironmentVariable("RABBITMQ_EMAIL_FILA") ?? "FilaLogAcessoPortalClienteBP_";

                var consumerConcurrency = Environment.GetEnvironmentVariable("RABBITMQ_CONSUMER_CONCURRENCY") ?? "5";
                var retryAttempts = Environment.GetEnvironmentVariable("RABBITMQ_RETRY_ATTEMPTS") ?? "3";
                var retryDelaySeconds = Environment.GetEnvironmentVariable("RABBITMQ_RETRY_DELAY_SECONDS") ?? "30";

                _logger.LogInformation($"SyncConfiguredQueues - Nomes brutos - Auditoria: '{queueAuditoriaNomeRaw}', Log: '{queueLogNomeRaw}', Email: '{queueEmailNomeRaw}'");
                
                var configuredQueues = new List<(string nome, string tipo, string descricao, int? consumerConcurrency, int? retryAttempts, int? retryDelaySeconds)>
                {
                    (
                        queueAuditoriaNomeRaw,
                        "Auditoria",
                        "Fila de processamento de logs de auditoria",
                        int.Parse(consumerConcurrency),
                        int.Parse(retryAttempts),
                        int.Parse(retryDelaySeconds)
                    ),
                    (
                        queueLogNomeRaw,
                        "Log",
                        "Fila de processamento de logs de acesso",
                        int.Parse(consumerConcurrency),
                        int.Parse(retryAttempts),
                        int.Parse(retryDelaySeconds)
                    ),
                    (
                        queueEmailNomeRaw,
                        "Email",
                        "Fila de processamento de envio de e-mails",
                        int.Parse(consumerConcurrency),
                        int.Parse(retryAttempts),
                        int.Parse(retryDelaySeconds)
                    )
                };

                _logger.LogInformation($"Iniciando sincronização de {configuredQueues.Count} filas configuradas");

                var existingQueuesInDb = (await _repository.FindByHql<RabbitMQQueue>("From RabbitMQQueue q")).ToList();
                _logger.LogInformation($"Encontradas {existingQueuesInDb.Count} filas existentes no banco de dados");

                var existingQueuesDict = existingQueuesInDb.GroupBy(q => q.Nome.ToLower())
                    .ToDictionary(g => g.Key, g => g.First());

                var processedCount = 0;
                foreach (var configQueue in configuredQueues)
                {
                    if (string.IsNullOrWhiteSpace(configQueue.nome))
                    {
                        _logger.LogWarning($"Nome de fila vazio encontrado no tipo '{configQueue.tipo}', ignorando");
                        continue;
                    }

                    var fullQueueName = $"{programId}{configQueue.nome}".Replace(" ", "");
                    var fullQueueNameLower = fullQueueName.ToLower();

                    _logger.LogInformation($"Processando fila: '{fullQueueName}' (tipo: {configQueue.tipo})");
                    _logger.LogDebug($"  - ProgramId: '{programId}', Nome bruto: '{configQueue.nome}', Nome completo: '{fullQueueName}'");

                    if (existingQueuesDict.TryGetValue(fullQueueNameLower, out var existingQueue))
                    {
                        // Atualizar configurações se necessário (sem alterar o status manual)
                        var needsUpdate = false;
                        if (existingQueue.TipoFila != configQueue.tipo)
                        {
                            existingQueue.TipoFila = configQueue.tipo;
                            needsUpdate = true;
                        }
                        if (existingQueue.Descricao != configQueue.descricao)
                        {
                            existingQueue.Descricao = configQueue.descricao;
                            needsUpdate = true;
                        }
                        if (configQueue.consumerConcurrency.HasValue && existingQueue.ConsumerConcurrency != configQueue.consumerConcurrency)
                        {
                            existingQueue.ConsumerConcurrency = configQueue.consumerConcurrency;
                            needsUpdate = true;
                        }
                        if (configQueue.retryAttempts.HasValue && existingQueue.RetryAttempts != configQueue.retryAttempts)
                        {
                            existingQueue.RetryAttempts = configQueue.retryAttempts;
                            needsUpdate = true;
                        }
                        if (configQueue.retryDelaySeconds.HasValue && existingQueue.RetryDelaySeconds != configQueue.retryDelaySeconds)
                        {
                            existingQueue.RetryDelaySeconds = configQueue.retryDelaySeconds;
                            needsUpdate = true;
                        }

                        if (needsUpdate)
                        {
                            await _repository.Save(existingQueue);
                            _logger.LogInformation($"Fila de processamento assíncrono atualizada: {fullQueueName}");
                        }
                        else
                        {
                            _logger.LogDebug($"Fila {fullQueueName} já está atualizada, nenhuma alteração necessária");
                        }
                    }
                    else
                    {
                        // Criar fila automaticamente
                        var newQueue = new RabbitMQQueue
                        {
                            Nome = fullQueueName,
                            TipoFila = configQueue.tipo,
                            Descricao = configQueue.descricao,
                            Ativo = EnumSimNao.Sim,
                            ConsumerConcurrency = configQueue.consumerConcurrency,
                            RetryAttempts = configQueue.retryAttempts,
                            RetryDelaySeconds = configQueue.retryDelaySeconds
                        };

                        await _repository.Save(newQueue);
                        _logger.LogInformation($"Fila de processamento assíncrono criada: {fullQueueName} (tipo: {configQueue.tipo})");
                    }
                    
                    processedCount++;
                }

                _logger.LogInformation($"Total de filas processadas: {processedCount}");

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Sincronização de filas configuradas concluída com sucesso. {processedCount} filas processadas.");
                }
                else if (exception != null)
                {
                    _logger.LogWarning(exception, "Erro ao sincronizar filas configuradas");
                    _repository.Rollback();
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao sincronizar filas configuradas");
                _repository.Rollback();
            }
        }

        private async Task EnrichQueuesWithStatistics(List<RabbitMQQueueViewModel> queues)
        {
            try
            {
                var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
                var port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int envPort)
                    ? envPort : 5672;
                var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "swsolucoes";
                var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "SW@dba#2024!";

                if (string.IsNullOrWhiteSpace(host) || port == 0)
                {
                    _logger.LogWarning("Configurações do RabbitMQ não encontradas, não é possível buscar estatísticas");
                    return;
                }

                // Tentar buscar estatísticas via Management API (se disponível)
                // Por enquanto, vamos usar a conexão direta para obter informações básicas
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = host,
                        Port = port,
                        UserName = user ?? "guest",
                        Password = pass ?? "guest"
                    };

                    using var connection = await factory.CreateConnectionAsync();
                    using var channel = await connection.CreateChannelAsync();

                    foreach (var queue in queues)
                    {
                        try
                        {
                            var queueDeclareResult = await channel.QueueDeclarePassiveAsync(queue.Nome);
                            // Itens pendentes = mensagens não consumidas
                            queue.ItensPendentes = (int)queueDeclareResult.MessageCount;
                            // Itens processados não está disponível diretamente via API básica
                            // Seria necessário usar Management API ou manter contadores próprios
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, $"Não foi possível obter estatísticas da fila {queue.Nome}");
                            // Fila pode não existir ainda ou estar inativa
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ para buscar estatísticas");
                }
            }
            catch (Exception err)
            {
                _logger.LogWarning(err, "Erro ao enriquecer filas com estatísticas");
            }
        }

        public async Task<RabbitMQQueueViewModel?> GetQueueById(int id)
        {
            try
            {
                var queue = (await _repository.FindByHql<RabbitMQQueue>($"From RabbitMQQueue q Where q.Id = {id}")).FirstOrDefault();
                return queue != null ? _mapper.Map<RabbitMQQueueViewModel>(queue) : null;
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível buscar a fila RabbitMQ com id {id}");
                throw;
            }
        }

        /// <summary>
        /// Busca uma fila pelo nome (apenas leitura no banco, sem sincronizar com RabbitMQ).
        /// Usado pelos consumers para verificar se a fila está ativa antes de iniciar.
        /// </summary>
        public async Task<RabbitMQQueueViewModel?> GetQueueByNome(string nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                    return null;
                var list = await _repository.FindByHql<RabbitMQQueue>(
                    "From RabbitMQQueue q Where Lower(q.Nome) = Lower(:nome)",
                    null,
                    new Parameter("nome", nome));
                var queue = list.FirstOrDefault();
                return queue != null ? _mapper.Map<RabbitMQQueueViewModel>(queue) : null;
            }
            catch (Exception err)
            {
                _logger.LogDebug(err, "GetQueueByNome: {Nome}", nome);
                return null;
            }
        }

        /// <summary>
        /// Indica se a fila está ativa no banco (Ativo = Sim). Usado pelos HostedServices para iniciar/parar consumers.
        /// </summary>
        public async Task<bool> IsQueueActiveByNome(string nome)
        {
            var vm = await GetQueueByNome(nome);
            return vm != null && vm.Ativo == EnumSimNao.Sim;
        }

        public async Task<bool> DeleteQueue(int id)
        {
            try
            {
                _repository.BeginTransaction();

                var queue = (await _repository.FindByHql<RabbitMQQueue>($"From RabbitMQQueue q Where q.Id = {id}")).FirstOrDefault();
                if (queue == null)
                    throw new ArgumentException($"Fila com id {id} não encontrada");

                await _repository.Remove(queue);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Fila RabbitMQ: ({queue.Id} - {queue.Nome}) excluída com sucesso!");
                    return true;
                }
                throw exception ?? new Exception($"Não foi possível excluir a fila");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível excluir a fila RabbitMQ com id {id}");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<RabbitMQQueueViewModel?> ToggleQueueStatus(int id)
        {
            try
            {
                _repository.BeginTransaction();

                var queue = (await _repository.FindByHql<RabbitMQQueue>($"From RabbitMQQueue q Where q.Id = {id}")).FirstOrDefault();
                if (queue == null)
                    throw new ArgumentException($"Fila com id {id} não encontrada");

                queue.Ativo = queue.Ativo == EnumSimNao.Sim ? EnumSimNao.Não : EnumSimNao.Sim;
                await _repository.Save(queue);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Status da fila RabbitMQ: ({queue.Id} - {queue.Nome}) alterado para {(queue.Ativo == EnumSimNao.Sim ? "Ativo" : "Inativo")}");
                    return _mapper.Map<RabbitMQQueueViewModel>(queue);
                }
                throw exception ?? new Exception($"Não foi possível alterar o status da fila");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível alterar o status da fila RabbitMQ com id {id}");
                _repository.Rollback();
                throw;
            }
        }
    }
}
