using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email;
using SW_PortalProprietario.Application.Models.GeralModels;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ.Producers
{
    public class RabbitMQEmailToQueueProducer : ISenderEmailToQueueProducer
    {
        private readonly IConfiguration _configuration;
        public RabbitMQEmailToQueueProducer(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        public async Task AddEmailMessageToQueue(EmailModel model)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration.GetValue<string>("RabbitMqConnectionHost"),
                Password = _configuration.GetValue<string>("RabbitMqConnectionPass"),
                UserName = _configuration.GetValue<string>("RabbitMqConnectionUser"),
                Port = _configuration.GetValue<int>("RabbitMqConnectionPort"),
                ClientProvidedName = _configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "ProcessamentoFilaEmail"),
                ConsumerDispatchConcurrency = 1
            };


            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var exchangeAndQueueName = $"{_configuration.GetValue<string>("ProgramId", "PORTALPROP_")}{_configuration.GetValue<string>("RabbitMqFilaDeEmailNome", "ProcessamentoFilaEmail")}";
            exchangeAndQueueName = exchangeAndQueueName.Replace(" ", "");
            await channel.ExchangeDeclareAsync(exchange: exchangeAndQueueName, type: ExchangeType.Direct, durable: true);

            var jsonMsg = System.Text.Json.JsonSerializer.Serialize(model);

            var body = Encoding.UTF8.GetBytes(jsonMsg);
            await channel.BasicPublishAsync(exchangeAndQueueName, exchangeAndQueueName, body);

            channel.Dispose();
            connection.Dispose();

            await Task.CompletedTask;
        }

    }
}
