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

                // Buscar filas do banco
                var queues = (await _repository.FindByHql<RabbitMQQueue>("From RabbitMQQueue q Order By q.Nome")).AsList();
                var queueViewModels = queues.Select(q => _mapper.Map<RabbitMQQueueViewModel>(q)).ToList();

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
                var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? _configuration.GetValue<string>("RabbitMqConnectionHost");
                var port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int envPort) 
                    ? envPort 
                    : _configuration.GetValue<int>("RabbitMqConnectionPort");
                var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? _configuration.GetValue<string>("RabbitMqConnectionUser");
                var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? _configuration.GetValue<string>("RabbitMqConnectionPass");

                if (string.IsNullOrWhiteSpace(host) || port == 0)
                {
                    _logger.LogWarning("Configurações do RabbitMQ não encontradas, sincronizando apenas filas configuradas");
                    await SyncConfiguredQueues();
                    return;
                }

                _repository.BeginTransaction();

                var programId = _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
                var programIdLower = programId.ToLower();

                // Lista de filas conhecidas com suas configurações
                var knownQueues = new Dictionary<string, (string tipo, string descricao, int? consumerConcurrency, int? retryAttempts, int? retryDelaySeconds)>
                {
                    {
                        $"{programId}{_configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "auditoria_bp")}".Replace(" ", "").ToLower(),
                        ("Auditoria", "Fila de processamento de logs de auditoria", 
                         _configuration.GetValue<int?>("AuditLog:ConsumerConcurrency"),
                         _configuration.GetValue<int?>("AuditLog:RetryAttempts"),
                         _configuration.GetValue<int?>("AuditLog:RetryDelaySeconds"))
                    },
                    {
                        $"{programId}{_configuration.GetValue<string>("RabbitMqFilaDeLogNome", "gravacaoLogs_mvc")}".Replace(" ", "").ToLower(),
                        ("Log", "Fila de processamento de logs de acesso", null, null, null)
                    },
                    {
                        $"{programId}{_configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "emails_mvc")}".Replace(" ", "").ToLower(),
                        ("Email", "Fila de processamento de envio de e-mails", null, null, null)
                    }
                };

                // Tentar conectar ao RabbitMQ e listar filas
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

                    // Listar todas as filas do RabbitMQ usando Management API via HTTP
                    // Como alternativa, vamos tentar descobrir filas conhecidas tentando declará-las passivamente
                    var discoveredQueues = new List<string>();

                    // Construir nomes exatos das filas
                    var queueAuditoriaNomeExact = $"{programId}{_configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "auditoria_bp")}".Replace(" ", "");
                    var queueLogNomeExact = $"{programId}{_configuration.GetValue<string>("RabbitMqFilaDeLogNome", "gravacaoLogs_mvc")}".Replace(" ", "");
                    var queueEmailNomeExact = $"{programId}{_configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "emails_mvc")}".Replace(" ", "");

                    // Tentar descobrir filas conhecidas usando nomes exatos
                    var queuesToTry = new[] 
                    { 
                        (queueAuditoriaNomeExact, queueAuditoriaNomeExact.ToLower()),
                        (queueLogNomeExact, queueLogNomeExact.ToLower()),
                        (queueEmailNomeExact, queueEmailNomeExact.ToLower())
                    };

                    foreach (var (queueNameExact, queueNameLower) in queuesToTry)
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
                            // Fila não existe, continuar
                        }
                    }

                    // Buscar todas as filas do banco
                    var existingQueuesInDb = (await _repository.FindByHql<RabbitMQQueue>("From RabbitMQQueue q")).ToList();
                    var existingQueuesDict = existingQueuesInDb.ToDictionary(q => q.Nome.ToLower(), q => q);

                    // Sincronizar filas descobertas
                    foreach (var queueName in discoveredQueues)
                    {
                        var queueNameLower = queueName.ToLower();
                        var queueInfo = knownQueues.ContainsKey(queueNameLower) 
                            ? knownQueues[queueNameLower] 
                            : ("Outros", $"Fila de processamento: {queueName}", null, null, null);

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
                            _logger.LogInformation($"Fila sincronizada do RabbitMQ: {queueName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ para listar filas, usando apenas configurações");
                    // Fallback: sincronizar apenas filas configuradas
                    await SyncConfiguredQueues();
                    _repository.Rollback();
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

        private async Task SyncConfiguredQueues()
        {
            try
            {
                _repository.BeginTransaction();

                var programId = _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
                
                // Definir filas configuradas no sistema
                var configuredQueues = new List<(string nome, string tipo, string descricao, int? consumerConcurrency, int? retryAttempts, int? retryDelaySeconds)>
                {
                    (
                        _configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "auditoria_bp"),
                        "Auditoria",
                        "Fila de processamento de logs de auditoria",
                        _configuration.GetValue<int?>("AuditLog:ConsumerConcurrency"),
                        _configuration.GetValue<int?>("AuditLog:RetryAttempts"),
                        _configuration.GetValue<int?>("AuditLog:RetryDelaySeconds")
                    ),
                    (
                        _configuration.GetValue<string>("RabbitMqFilaDeLogNome", "gravacaoLogs_mvc"),
                        "Log",
                        "Fila de processamento de logs de acesso",
                        null,
                        null,
                        null
                    ),
                    (
                        _configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "emails_mvc"),
                        "Email",
                        "Fila de processamento de envio de e-mails",
                        null,
                        null,
                        null
                    )
                };

                var existingQueuesInDb = (await _repository.FindByHql<RabbitMQQueue>("From RabbitMQQueue q")).ToList();
                var existingQueuesDict = existingQueuesInDb.ToDictionary(q => q.Nome.ToLower(), q => q);

                foreach (var configQueue in configuredQueues)
                {
                    if (string.IsNullOrWhiteSpace(configQueue.nome))
                        continue;

                    var fullQueueName = $"{programId}{configQueue.nome}".Replace(" ", "");
                    var fullQueueNameLower = fullQueueName.ToLower();

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
                        _logger.LogInformation($"Fila de processamento assíncrono sincronizada automaticamente: {fullQueueName}");
                    }
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (!executed && exception != null)
                {
                    _logger.LogWarning(exception, "Erro ao sincronizar filas configuradas, mas continuando...");
                    _repository.Rollback();
                }
            }
            catch (Exception err)
            {
                _logger.LogWarning(err, "Erro ao sincronizar filas configuradas, mas continuando...");
                _repository.Rollback();
            }
        }

        private async Task EnrichQueuesWithStatistics(List<RabbitMQQueueViewModel> queues)
        {
            try
            {
                var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? _configuration.GetValue<string>("RabbitMqConnectionHost");
                var port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int envPort) 
                    ? envPort 
                    : _configuration.GetValue<int>("RabbitMqConnectionPort");
                var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? _configuration.GetValue<string>("RabbitMqConnectionUser");
                var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? _configuration.GetValue<string>("RabbitMqConnectionPass");

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

                _repository.Remove(queue);

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
