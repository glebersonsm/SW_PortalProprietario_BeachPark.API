using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_Utils.Models;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ.Producers
{
    public class RabbitMQRegisterLogMessageToQueueProducer : ILogMessageToQueueProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheStore _cache;
        public RabbitMQRegisterLogMessageToQueueProducer(IConfiguration configuration, ICacheStore cache)
        {
            _configuration = configuration;
            _cache = cache;

        }

        public async Task AddLogMessage()
        {
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? _configuration.GetValue<string>("RabbitMqConnectionHost"),
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? _configuration.GetValue<string>("RabbitMqConnectionPass"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? _configuration.GetValue<string>("RabbitMqConnectionUser"),
                Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port) ? port : _configuration.GetValue<int>("RabbitMqConnectionPort"),
                ClientProvidedName = _configuration.GetValue<string>("RabbitMqFilaDeLogNome", "ProcessamentoFilaLog"),
                ConsumerDispatchConcurrency = 1
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var exchangeAndQueueName = $"{_configuration.GetValue<string>("ProgramId", "PORTALPROP_")}{_configuration.GetValue<string>("RabbitMqFilaDeLogNome", "ProcessamentoFilaLog")}";
            exchangeAndQueueName = exchangeAndQueueName.Replace(" ", "");
            await channel.ExchangeDeclareAsync(exchange: exchangeAndQueueName, type: ExchangeType.Direct, durable: true);

            var messagesToPublish = new List<OperationSystemLogModelEvent>();
            foreach (var message in messagesToPublish)
            {
                var jsonMsg = System.Text.Json.JsonSerializer.Serialize(message);

                var body = Encoding.UTF8.GetBytes(jsonMsg);
                await channel.BasicPublishAsync(exchangeAndQueueName, exchangeAndQueueName, body);
            }

            channel.Dispose();
            connection.Dispose();

            await Task.CompletedTask;

        }

        public async Task AddLogMessage(OperationSystemLogModelEvent message)
        {
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? _configuration.GetValue<string>("RabbitMqConnectionHost"),
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? _configuration.GetValue<string>("RabbitMqConnectionPass"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? _configuration.GetValue<string>("RabbitMqConnectionUser"),
                Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port) ? port : _configuration.GetValue<int>("RabbitMqConnectionPort"),
                ClientProvidedName = _configuration.GetValue<string>("RabbitMqFilaDeLogNome", "ProcessamentoFilaLog"),
                ConsumerDispatchConcurrency = 1
            };


            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var exchangeAndQueueName = $"{_configuration.GetValue<string>("ProgramId", "PORTALPROP_")}{_configuration.GetValue<string>("RabbitMqFilaDeLogNome", "ProcessamentoFilaLog")}";
            exchangeAndQueueName = exchangeAndQueueName.Replace(" ", "");
            await channel.ExchangeDeclareAsync(exchange: exchangeAndQueueName, type: ExchangeType.Direct, durable: true);

            var jsonMsg = System.Text.Json.JsonSerializer.Serialize(message);



            var body = Encoding.UTF8.GetBytes(jsonMsg);
            await channel.BasicPublishAsync(exchangeAndQueueName, exchangeAndQueueName, body);


            channel.Dispose();
            connection.Dispose();

            await Task.CompletedTask;
        }

    }
}
