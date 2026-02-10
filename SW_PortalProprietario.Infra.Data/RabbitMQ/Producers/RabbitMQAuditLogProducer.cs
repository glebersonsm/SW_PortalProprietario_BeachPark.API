using Microsoft.Extensions.Configuration;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_Utils.Models;
using RabbitMQ.Client;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ.Producers
{
    public class RabbitMQAuditLogProducer : IAuditLogQueueProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheStore _cache;

        public RabbitMQAuditLogProducer(IConfiguration configuration, ICacheStore cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public async Task EnqueueAuditLogAsync(AuditLogMessageEvent message)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? _configuration.GetValue<string>("RabbitMqConnectionHost"),
                    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? _configuration.GetValue<string>("RabbitMqConnectionPass"),
                    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? _configuration.GetValue<string>("RabbitMqConnectionUser"),
                    Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port) ? port : _configuration.GetValue<int>("RabbitMqConnectionPort"),
                    ClientProvidedName = _configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "ProcessamentoFilaAuditoria"),
                    ConsumerDispatchConcurrency = 1
                };

                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                var exchangeAndQueueName = $"{_configuration.GetValue<string>("ProgramId", "PORTALPROP_")}{_configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "auditoria_bp")}";
                exchangeAndQueueName = exchangeAndQueueName.Replace(" ", "");

                await channel.ExchangeDeclareAsync(exchange: exchangeAndQueueName, type: ExchangeType.Direct, durable: true);

                var jsonMsg = System.Text.Json.JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonMsg);

                await channel.BasicPublishAsync(exchangeAndQueueName, exchangeAndQueueName, body: body);

                channel.Dispose();
                connection.Dispose();
            }
            catch (Exception ex)
            {
                // Log error but don't throw - we don't want to break the main operation
                // Consider logging to a fallback mechanism
                System.Diagnostics.Debug.WriteLine($"Erro ao enfileirar log de auditoria: {ex.Message}");
            }

            await Task.CompletedTask;
        }
    }
}

