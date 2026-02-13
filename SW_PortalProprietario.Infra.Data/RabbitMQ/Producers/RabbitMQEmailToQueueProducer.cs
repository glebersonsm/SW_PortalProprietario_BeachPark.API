using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email;
using SW_PortalProprietario.Application.Models.GeralModels;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ.Producers
{
    public class RabbitMQEmailToQueueProducer : ISenderEmailToQueueProducer
    {
        private readonly IConfiguration _configuration;
        private readonly IRabbitMQConnectionManager _connectionManager;
        private readonly ILogger<RabbitMQEmailToQueueProducer> _logger;

        public RabbitMQEmailToQueueProducer(
            IConfiguration configuration,
            IRabbitMQConnectionManager connectionManager,
            ILogger<RabbitMQEmailToQueueProducer> logger)
        {
            _configuration = configuration;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task AddEmailMessageToQueue(EmailModel model)
        {
            IChannel? channel = null;
            try
            {
                var connection = await _connectionManager.GetProducerConnectionAsync();
                channel = await _connectionManager.CreateChannelAsync(connection);

                var programId = Environment.GetEnvironmentVariable("PROGRAM_ID") 
                    ?? _configuration.GetValue<string>("ProgramId", "PORTALPROP_");
                
                var filaEmail = Environment.GetEnvironmentVariable("RABBITMQ_FILA_EMAIL") 
                    ?? _configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "emails_mvc");

                var exchangeAndQueueName = $"{programId}{filaEmail}".Replace(" ", "");
                
                await channel.ExchangeDeclareAsync(
                    exchange: exchangeAndQueueName, 
                    type: ExchangeType.Direct, 
                    durable: true);

                var jsonMsg = System.Text.Json.JsonSerializer.Serialize(model);
                var body = Encoding.UTF8.GetBytes(jsonMsg);
                
                await channel.BasicPublishAsync(exchangeAndQueueName, exchangeAndQueueName, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enfileirar email");
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
                        _logger.LogDebug(ex, "Erro ao fechar channel de email");
                    }
                }
            }
        }
    }
}

