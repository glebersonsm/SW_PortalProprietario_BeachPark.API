using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;

namespace SW_PortalProprietario.Application.Hosted
{
    public class AuditLogConsumerHostedService : BackgroundService
    {
        private readonly IAuditLogQueueConsumer _auditLogConsumer;
        private readonly ILogger<AuditLogConsumerHostedService> _logger;
        private readonly IConfiguration _configuration;

        public AuditLogConsumerHostedService(
            IAuditLogQueueConsumer auditLogConsumer,
            ILogger<AuditLogConsumerHostedService> logger,
            IConfiguration configuration)
        {
            _auditLogConsumer = auditLogConsumer;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Aguardar um pouco antes de iniciar para garantir que o RabbitMQ está pronto
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            _logger.LogInformation("Iniciando consumer de logs de auditoria...");

            try
            {
                await _auditLogConsumer.RegisterConsumerAndSaveAuditLogFromQueue();
                _logger.LogInformation("Consumer de logs de auditoria iniciado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar consumer de logs de auditoria");
            }

            // Manter o serviço rodando
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

