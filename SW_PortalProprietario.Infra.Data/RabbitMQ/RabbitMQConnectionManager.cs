using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace SW_PortalProprietario.Infra.Data.RabbitMQ
{
    /// <summary>
    /// Gerenciador de conexões RabbitMQ compartilhadas.
    /// Mantém uma conexão singleton por tipo (Producer/Consumer) para evitar leak de conexões.
    /// </summary>
    public interface IRabbitMQConnectionManager : IDisposable
    {
        Task<IConnection> GetProducerConnectionAsync();
        Task<IConnection> GetConsumerConnectionAsync(string consumerName);
        Task<IChannel> CreateChannelAsync(IConnection connection);
    }

    public class RabbitMQConnectionManager : IRabbitMQConnectionManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQConnectionManager> _logger;
        
        private IConnection? _producerConnection;
        private readonly Dictionary<string, IConnection> _consumerConnections = new();
        private readonly SemaphoreSlim _producerLock = new(1, 1);
        private readonly SemaphoreSlim _consumerLock = new(1, 1);
        private bool _disposed = false;

        public RabbitMQConnectionManager(
            IConfiguration configuration,
            ILogger<RabbitMQConnectionManager> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IConnection> GetProducerConnectionAsync()
        {
            if (_producerConnection != null && _producerConnection.IsOpen)
                return _producerConnection;

            await _producerLock.WaitAsync();
            try
            {
                if (_producerConnection != null && _producerConnection.IsOpen)
                    return _producerConnection;

                var factory = CreateConnectionFactory("ProducerConnection");
                _producerConnection = await factory.CreateConnectionAsync();
                
                _logger.LogInformation("Conexão RabbitMQ Producer criada e compartilhada");
                
                return _producerConnection;
            }
            finally
            {
                _producerLock.Release();
            }
        }

        public async Task<IConnection> GetConsumerConnectionAsync(string consumerName)
        {
            if (_consumerConnections.TryGetValue(consumerName, out var existingConnection) 
                && existingConnection.IsOpen)
            {
                return existingConnection;
            }

            await _consumerLock.WaitAsync();
            try
            {
                if (_consumerConnections.TryGetValue(consumerName, out existingConnection) 
                    && existingConnection.IsOpen)
                {
                    return existingConnection;
                }

                var factory = CreateConnectionFactory($"Consumer_{consumerName}");
                var connection = await factory.CreateConnectionAsync();
                
                _consumerConnections[consumerName] = connection;
                _logger.LogInformation($"Conexão RabbitMQ Consumer '{consumerName}' criada e compartilhada");
                
                return connection;
            }
            finally
            {
                _consumerLock.Release();
            }
        }

        public async Task<IChannel> CreateChannelAsync(IConnection connection)
        {
            if (!connection.IsOpen)
                throw new InvalidOperationException("A conexão RabbitMQ não está aberta");

            return await connection.CreateChannelAsync();
        }

        private ConnectionFactory CreateConnectionFactory(string clientName)
        {
            var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
                ?? _configuration.GetValue<string>("RabbitMqConnectionHost") 
                ?? "localhost";
            
            var port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int envPort) 
                ? envPort 
                : _configuration.GetValue<int>("RabbitMqConnectionPort", 5672);
            
            var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") 
                ?? _configuration.GetValue<string>("RabbitMqConnectionUser") 
                ?? "guest";
            
            var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") 
                ?? _configuration.GetValue<string>("RabbitMqConnectionPass") 
                ?? "guest";

            return new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = user,
                Password = pass,
                ClientProvidedName = clientName,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                // Channel pooling settings
                ConsumerDispatchConcurrency = 1
            };
        }

        public void Dispose()
        {
            if (_disposed) return;

            _logger.LogInformation("Encerrando RabbitMQConnectionManager...");

            try
            {
                if (_producerConnection != null && _producerConnection.IsOpen)
                {
                    _producerConnection.CloseAsync().GetAwaiter().GetResult();
                    _producerConnection.Dispose();
                    _logger.LogInformation("Conexão Producer fechada");
                }

                foreach (var kvp in _consumerConnections)
                {
                    if (kvp.Value != null && kvp.Value.IsOpen)
                    {
                        kvp.Value.CloseAsync().GetAwaiter().GetResult();
                        kvp.Value.Dispose();
                        _logger.LogInformation($"Conexão Consumer '{kvp.Key}' fechada");
                    }
                }

                _consumerConnections.Clear();
                _producerLock.Dispose();
                _consumerLock.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao encerrar conexões RabbitMQ");
            }

            _disposed = true;
        }
    }
}
