using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_Utils.Models;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ.Consumers
{
    public class RabbitMQAuditLogConsumer : IAuditLogQueueConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RabbitMQAuditLogConsumer> _logger;

        private IConnection? _connectionInstance;
        private IChannel? _channelInstance;
        private string? _consumerTag;
        private static bool _isRunning = false;
        private static bool _alreadyLoggedRunning = false;
        private static readonly object _lock = new();

        public bool IsRunning => _isRunning;

        public RabbitMQAuditLogConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RabbitMQAuditLogConsumer> logger)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task RegisterConsumerAndSaveAuditLogFromQueue()
        {
            var queueName = $"{_configuration.GetValue<string>("ProgramId", "PORTALPROP_")}{_configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "auditoria_bp")}";
            queueName = queueName.Replace(" ", "");

            try
            {
                if (_isRunning)
                {
                    if (!_alreadyLoggedRunning)
                    {
                        _logger.LogDebug("Consumer de auditoria j? est? em execu??o");
                        _alreadyLoggedRunning = true;
                    }
                    return;
                }

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var queueService = scope.ServiceProvider.GetRequiredService<IRabbitMQQueueService>();
                    var isActive = await queueService.IsQueueActiveByNome(queueName);
                    if (!isActive)
                    {
                        _logger.LogInformation("Fila de auditoria {QueueName} est? inativa no painel; consumer n?o iniciado.", queueName);
                        return;
                    }
                }

                lock (_lock)
                {
                    if (_isRunning) return;
                }

                ushort maximaConcorrencia = (ushort)_configuration.GetValue<Int16>("AuditLog:ConsumerConcurrency", 5);

                var factory = new ConnectionFactory
                {
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? _configuration.GetValue<string>("RabbitMqConnectionHost"),
                    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? _configuration.GetValue<string>("RabbitMqConnectionPass"),
                    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? _configuration.GetValue<string>("RabbitMqConnectionUser"),
                    Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port) ? port : _configuration.GetValue<int>("RabbitMqConnectionPort"),
                    ClientProvidedName = _configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "ProcessamentoFilaAuditoria"),
                    ConsumerDispatchConcurrency = maximaConcorrencia
                };

                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();
                _connectionInstance = connection;
                _channelInstance = channel;

                _logger.LogInformation("Configurando fila de auditoria: {QueueName}", queueName);

                await channel.ExchangeDeclareAsync(
                    exchange: queueName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                await channel.QueueBindAsync(queue: queueName, exchange: queueName, routingKey: queueName);

                var consumerCount = await channel.ConsumerCountAsync(queueName);
                _logger.LogInformation("Quantidade de consumers na fila {QueueName}: {ConsumerCount}", queueName, consumerCount);

                if (consumerCount == 0)
                {
                    _logger.LogInformation("Registrando consumer na fila {QueueName}", queueName);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var tag = ea.DeliveryTag;
                        var message = Encoding.UTF8.GetString(body);

                        try
                        {
                            bool queueStillActive;
                            using (var scopeCheck = _serviceScopeFactory.CreateScope())
                            {
                                var queueService = scopeCheck.ServiceProvider.GetRequiredService<IRabbitMQQueueService>();
                                queueStillActive = await queueService.IsQueueActiveByNome(queueName);
                            }
                            if (!queueStillActive)
                            {
                                // N?o processar enquanto a fila estiver inativa; reenfileirar para n?o perder a mensagem
                                _logger.LogDebug("Fila {QueueName} est? inativa; mensagem reenfileirada sem gravar (ser? processada ao reativar).", queueName);
                                await channel.BasicNackAsync(deliveryTag: tag, multiple: false, requeue: true);
                                return;
                            }

                            _logger.LogDebug("Recebido log de auditoria da fila: {Message}", message);

                            var auditLogMessage = System.Text.Json.JsonSerializer.Deserialize<AuditLogMessageEvent>(message);
                            if (auditLogMessage != null)
                            {
                                using (var scope = _serviceScopeFactory.CreateScope())
                                {
                                    var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
                                    await auditService.SaveAuditLogAsync(auditLogMessage);
                                }

                                _logger.LogDebug("Log de auditoria salvo com sucesso: EntityType={EntityType}, EntityId={EntityId}",
                                    auditLogMessage.EntityType, auditLogMessage.EntityId);
                            }

                            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        catch (Exception err)
                        {
                            // Erro no processamento: n?o reenfileirar para evitar loop com mensagem inv?lida
                            _logger.LogError(err, "Erro ao processar log de auditoria: {Message}", err.Message);
                            await channel.BasicNackAsync(deliveryTag: tag, multiple: false, requeue: false);
                            return;
                        }
                    };

                    _consumerTag = await channel.BasicConsumeAsync(queue: queueName,
                                         autoAck: false,
                                         consumer: consumer);

                    _logger.LogInformation("Consumer registrado com sucesso na fila {QueueName}", queueName);
                }
                else
                {
                    _logger.LogWarning("Fila {QueueName} j? possui {ConsumerCount} consumer(s) ativo(s)", queueName, consumerCount);
                }

                lock (_lock)
                {
                    _isRunning = true;
                    _alreadyLoggedRunning = false;
                }

                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao registrar consumer de auditoria: {Message}", err.Message);
                _connectionInstance = null;
                _channelInstance = null;
                lock (_lock) { _isRunning = false; }
            }
        }

        public async Task StopConsumerAsync()
        {
            lock (_lock)
            {
                if (!_isRunning) return;
            }

            try
            {
                if (_channelInstance != null && !string.IsNullOrEmpty(_consumerTag))
                {
                    try
                    {
                        await _channelInstance.BasicCancelAsync(_consumerTag);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "BasicCancelAsync j? pode ter sido aplicado");
                    }
                    _consumerTag = null;
                }
                if (_channelInstance != null)
                {
                    await _channelInstance.CloseAsync();
                    _channelInstance.Dispose();
                    _channelInstance = null;
                }
                if (_connectionInstance != null)
                {
                    await _connectionInstance.CloseAsync();
                    _connectionInstance.Dispose();
                    _connectionInstance = null;
                }
                lock (_lock)
                {
                    _isRunning = false;
                    _alreadyLoggedRunning = false;
                }
                _logger.LogInformation("Consumer de auditoria parado; fila não será processada até reativação no painel.");
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao parar consumer de auditoria");
                lock (_lock) { _isRunning = false; _alreadyLoggedRunning = false; }
            }

            await Task.CompletedTask;
        }
    }
}

