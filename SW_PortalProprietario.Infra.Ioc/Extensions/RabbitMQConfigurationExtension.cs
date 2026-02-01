using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Infra.Data.RabbitMQ.Consumers;
using SW_PortalProprietario.Infra.Data.RabbitMQ.Producers;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class RabbitMQConfigurationExtensions
    {
        public static IServiceCollection ConfigureRabbitMQ(this IServiceCollection services)
        {
            //Producer de logs de acesso e de alterações do sistema no RabbitMQ
            services.TryAddSingleton<ILogMessageToQueueProducer, RabbitMQRegisterLogMessageToQueueProducer>();
            //Consummer da fila de logs de acesso e de alterações do sistema do RabbitMQ para o banco de dados
            services.TryAddSingleton<ILogMessageFromQueueConsumer, RabbitMQLogMessageFromQueueConsumer>();

            //Producer de emails para o RabbitMQ
            services.TryAddSingleton<ISenderEmailToQueueProducer, RabbitMQEmailToQueueProducer>();
            //Consumer da fila de emails do RabbitMQ
            services.TryAddSingleton<IEmailSenderFromQueueConsumer, RabbitMQEmailSenderFromQueueConsumer>();

            //Producer de logs de auditoria para o RabbitMQ
            services.TryAddSingleton<IAuditLogQueueProducer, RabbitMQAuditLogProducer>();
            //Consumer da fila de logs de auditoria do RabbitMQ
            services.TryAddSingleton<IAuditLogQueueConsumer, RabbitMQAuditLogConsumer>();

            return services;

        }

    }
}
