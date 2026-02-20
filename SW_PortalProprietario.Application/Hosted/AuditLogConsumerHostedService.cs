using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.Application.Hosted
{
    public class AuditLogConsumerHostedService : BackgroundService
    {
        private readonly IAuditLogQueueConsumer _auditLogConsumer;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AuditLogConsumerHostedService> _logger;
        private readonly IConfiguration _configuration;

        public AuditLogConsumerHostedService(
            IAuditLogQueueConsumer auditLogConsumer,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AuditLogConsumerHostedService> logger,
            IConfiguration configuration)
        {
            _auditLogConsumer = auditLogConsumer;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            var queueName = $"{_configuration.GetValue<string>("ProgramId", "PORTALPROP_")}{_configuration.GetValue<string>("RabbitMqFilaDeAuditoriaNome", "auditoria_bp")}";
            queueName = queueName.Replace(" ", "");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool queueActive;
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var queueService = scope.ServiceProvider.GetRequiredService<IRabbitMQQueueService>();
                        queueActive = await queueService.IsQueueActiveByNome(queueName);
                    }

                    if (queueActive && !_auditLogConsumer.IsRunning)
                    {
                        _logger.LogInformation("Fila de auditoria ativa no painel; iniciando consumer...");
                        await _auditLogConsumer.RegisterConsumerAndSaveAuditLogFromQueue();
                    }
                    else if (!queueActive && _auditLogConsumer.IsRunning)
                    {
                        _logger.LogInformation("Fila de auditoria inativa no painel; parando consumer...");
                        await _auditLogConsumer.StopConsumerAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao verificar/iniciar/parar consumer de logs de auditoria");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

