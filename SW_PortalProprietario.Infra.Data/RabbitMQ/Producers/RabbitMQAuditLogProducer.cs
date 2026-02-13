using Microsoft.Extensions.Configuration;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_Utils.Models;
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ.Producers
{
    public class RabbitMQAuditLogProducer : IAuditLogQueueProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheStore _cache;
        private readonly IRabbitMQConnectionManager _connectionManager;
        private readonly ILogger<RabbitMQAuditLogProducer> _logger;

        public RabbitMQAuditLogProducer(
            IConfiguration configuration, 
            ICacheStore cache,
            IRabbitMQConnectionManager connectionManager,
            ILogger<RabbitMQAuditLogProducer> logger)
        {
            _configuration = configuration;
            _cache = cache;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task EnqueueAuditLogAsync(AuditLogMessageEvent message)
        {
            IChannel? channel = null;
            try
            {
                // ? Reutiliza conexão compartilhada ao invés de criar uma nova
                var connection = await _connectionManager.GetProducerConnectionAsync();
                channel = await _connectionManager.CreateChannelAsync(connection);

                var programId = Environment.GetEnvironmentVariable("PROGRAM_ID") 
                    ?? _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
                
                var filaAuditoria = Environment.GetEnvironmentVariable("RABBITMQ_FILA_AUDITORIA") 
                    ?? _configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "auditoria_bp");

                var exchangeAndQueueName = $"{programId}{filaAuditoria}".Replace(" ", "");

                await channel.ExchangeDeclareAsync(
                    exchange: exchangeAndQueueName, 
                    type: ExchangeType.Direct, 
                    durable: true);

                var jsonMsg = System.Text.Json.JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonMsg);

                await channel.BasicPublishAsync(exchangeAndQueueName, exchangeAndQueueName, body: body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enfileirar log de auditoria");
            }
            finally
            {
                // ? Fecha apenas o channel, não a conexão (conexão é reutilizada)
                if (channel != null)
                {
                    try
                    {
                        await channel.CloseAsync();
                        channel.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Erro ao fechar channel de auditoria");
                    }
                }
            }
        }
    }
}


