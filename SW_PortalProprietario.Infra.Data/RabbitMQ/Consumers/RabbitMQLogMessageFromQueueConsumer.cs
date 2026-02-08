using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Application.Services.Core.Interfaces.ProgramacaoParalela;
using SW_Utils.Models;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ.Consumers
{
    public class RabbitMQLogMessageFromQueueConsumer : ILogMessageFromQueueConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogOperationService _logOperationService;
        private readonly ILogger<RabbitMQLogMessageFromQueueConsumer> _logger;

        private static IConnection? _connection;
        private static bool _isRunning = false;
        private static bool _alreadyLoggedRunning = false;
        private static readonly object _lock = new();

        public RabbitMQLogMessageFromQueueConsumer(
            IConfiguration configuration,
            ILogOperationService logOperationService,
            ILogger<RabbitMQLogMessageFromQueueConsumer> logger)
        {
            _configuration = configuration;
            _logOperationService = logOperationService;
            _logger = logger;
        }
        public async Task RegisterConsumerAndSaveLogFromQueue()
        {
            try
            {
                if (_isRunning)
                {
                    if (!_alreadyLoggedRunning)
                    {
                        _logger.LogDebug("Consumer de log já está em execução");
                        _alreadyLoggedRunning = true;
                    }
                    return;
                }

                lock (_lock)
                {
                    if (_isRunning) return;
                    _isRunning = true;
                }

                var factory = new ConnectionFactory
                {
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? _configuration.GetValue<string>("RabbitMqConnectionHost"),
                    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? _configuration.GetValue<string>("RabbitMqConnectionPass"),
                    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? _configuration.GetValue<string>("RabbitMqConnectionUser"),
                    Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port) ? port : _configuration.GetValue<int>("RabbitMqConnectionPort"),
                    ClientProvidedName = Environment.GetEnvironmentVariable("RABBITMQ_LOG_ACESSO_FILA") ?? "FilaLogAcessoPortalClienteBP_",
                    ConsumerDispatchConcurrency = 1
                };

                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                var queueName = factory.ClientProvidedName;
                queueName = queueName.Replace(" ", "");

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
                if (consumerCount == 0)
                {
                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var tag = ea.DeliveryTag;
                        var message = Encoding.UTF8.GetString(body);

                        try
                        {
                            var operationSystemLog = System.Text.Json.JsonSerializer.Deserialize<OperationSystemLogModelEvent>(message);
                            if (operationSystemLog != null)
                                await _logOperationService.SaveLog(operationSystemLog);
                            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);

                        }
                        catch (Exception err)
                        {
                            _logger.LogError(err, err.Message, err.InnerException?.Message, err.StackTrace);
                            await channel.BasicNackAsync(deliveryTag: tag, multiple: false, requeue: false);
                            return;
                        }
                    };
                    await channel.BasicConsumeAsync(queue: queueName,
                                         autoAck: false,
                                         consumer: consumer);
                }

                _isRunning = true;

                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
            }
        }
    }
}
