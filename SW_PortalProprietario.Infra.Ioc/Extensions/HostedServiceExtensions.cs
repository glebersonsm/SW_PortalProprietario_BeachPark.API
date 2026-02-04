using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SW_PortalProprietario.Application.Hosted;
using SW_PortalProprietario.Application.Hosted.ProgramacaoParalela.Communication.Email;
using SW_PortalProprietario.Application.Hosted.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.Communication.Email;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces.Communication;
using SW_PortalProprietario.Application.Services.Core.Interfaces.ProgramacaoParalela;
using SW_PortalProprietario.Application.Services.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Infra.Data.Repositories.Core;
using SW_PortalProprietario.Infra.Ioc.Communication;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class HostedServiceExtensions
    {
        public static IServiceCollection RegisterHostedServices(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });


            services.AddNHbernateHosted(configuration);
            services.TryAddSingleton<ILogOperationService, LogOperationService>();

            services.TryAddScoped<IUnitOfWorkHosted, UnitOfWorkHosted>();
            services.TryAddScoped<IAuthenticatedBaseHostedService, AuthenticatedBaseHostedService>();
            services.TryAddScoped<IRepositoryHosted, RepositoryHosted>();
            services.TryAddSingleton<IEmailSenderHostedService, EmailSenderHostedService>();


            if (configuration.GetValue<bool>("UpdateFramework"))
            {

                services.TryAddSingleton<IFrameworkInitialService, FrameworkInitialService>();
                services.TryAddSingleton<IBackGroundProcessUpdateFramework, UpdateFrameworkHostedService>();
                services.AddHostedService<UpdateFrameworkHostedService>();
            }

            if (configuration.GetValue<bool>("ConsultarStatusPix"))
            {
                services.TryAddSingleton<IBackGroundProcessUpdateTransactionStatus, SearchTransactionStatusHostedService>();
                services.AddHostedService<SearchTransactionStatusHostedService>();
            }

            if (configuration.GetValue<bool>("SendOperationsToProcessingLogQueue"))
            {
                services.TryAddSingleton<IBackGroundSenderLogToProcessingQueue, SenderLogOperationsSystemToProcessingQueue>();
                services.AddHostedService<SenderLogOperationsSystemToProcessingQueue>();
            }

            if (configuration.GetValue<bool>("SaveLogOperationsFromProcessingLogQueue"))
            {
                services.TryAddSingleton<IBackGroundSaveLogFromProcessingQueue, SaveLogOperationsSystemFromProcessingQueueConsumer>();
                services.AddHostedService<SaveLogOperationsSystemFromProcessingQueueConsumer>();
            }

            if (configuration.GetValue<bool>("SendEmailFromProcessingQueue"))
            {
                services.TryAddSingleton<IBackGroundSenderEmailFromProcessingQueue, EmailSenderFromProcessingQueueConsumer>();
                services.AddHostedService<EmailSenderFromProcessingQueueConsumer>();
            }

            if (configuration.GetValue<bool>("AutomaticCommunicationEmailEnabled", false))
            {
                services.AddHostedService<AutomaticCommunicationEmailHostedService>();
            }

            // Consumer de logs de auditoria
            if (configuration.GetValue<bool>("AuditLog:Enabled", true))
            {
                services.AddHostedService<AuditLogConsumerHostedService>();
            }

            return services;
        }

    }
}
