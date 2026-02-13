using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IRabbitMQConnectionManager _connectionManager;
        private readonly ILogger<RabbitMQRegisterLogMessageToQueueProducer> _logger;

        public RabbitMQRegisterLogMessageToQueueProducer(
            IConfiguration configuration, 
            ICacheStore cache,
            IRabbitMQConnectionManager connectionManager,
            ILogger<RabbitMQRegisterLogMessageToQueueProducer> logger)
        {
            _configuration = configuration;
            _cache = cache;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task AddLogMessage()
        {
            IChannel? channel = null;
            try
            {
                var connection = await _connectionManager.GetProducerConnectionAsync();
                channel = await _connectionManager.CreateChannelAsync(connection);

                var programId = Environment.GetEnvironmentVariable("PROGRAM_ID") 
                    ?? _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
                
                var filaLog = Environment.GetEnvironmentVariable("RABBITMQ_FILA_LOG") 
                    ?? _configuration.GetValue<string>("RabbitMqFilaDeLogNome", "gravacaoLogs_mvc");

                var exchangeAndQueueName = $"{programId}{filaLog}".Replace(" ", "");
                
                await channel.ExchangeDeclareAsync(
                    exchange: exchangeAndQueueName, 
                    type: ExchangeType.Direct, 
                    durable: true);

                var messagesToPublish = new List<OperationSystemLogModelEvent>();
                foreach (var message in messagesToPublish)
                {
                    var jsonMsg = System.Text.Json.JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(jsonMsg);
                    await channel.BasicPublishAsync(exchangeAndQueueName, exchangeAndQueueName, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enfileirar logs de acesso");
            }
            finally
            {
                if (channel != null)
                {
                    try
                    {
                        await channel.CloseAsync();
                        channel.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Erro ao fechar channel de log");
                    }
                }
            }
        }

        public async Task AddLogMessage(OperationSystemLogModelEvent message)
        {
            IChannel? channel = null;
            try
            {
                var connection = await _connectionManager.GetProducerConnectionAsync();
                channel = await _connectionManager.CreateChannelAsync(connection);

                var programId = Environment.GetEnvironmentVariable("PROGRAM_ID") 
                    ?? _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
                
                var filaLog = Environment.GetEnvironmentVariable("RABBITMQ_FILA_LOG") 
                    ?? _configuration.GetValue<string>("RabbitMqFilaDeLogNome", "gravacaoLogs_mvc");

                var exchangeAndQueueName = $"{programId}{filaLog}".Replace(" ", "");
                
                await channel.ExchangeDeclareAsync(
                    exchange: exchangeAndQueueName, 
                    type: ExchangeType.Direct, 
                    durable: true);

                var jsonMsg = System.Text.Json.JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonMsg);
                
                await channel.BasicPublishAsync(exchangeAndQueueName, exchangeAndQueueName, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enfileirar log de acesso");
            }
            finally
            {
                if (channel != null)
                {
                    try
                    {
                        await channel.CloseAsync();
                        channel.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Erro ao fechar channel de log");
                    }
                }
            }
        }
    }
}

