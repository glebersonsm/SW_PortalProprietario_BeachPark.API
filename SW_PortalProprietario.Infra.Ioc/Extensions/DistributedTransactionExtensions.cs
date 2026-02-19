using Microsoft.Extensions.DependencyInjection;
using SW_PortalProprietario.Application.Interfaces.Saga;
using SW_PortalProprietario.Application.Services.Core.Saga;
using SW_PortalProprietario.Infra.Data.Repositories.Saga;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class DistributedTransactionExtensions
    {
        public static IServiceCollection AddSagaPattern(this IServiceCollection services)
        {
            services.AddScoped<ISagaRepository, SagaRepository>();
            services.AddScoped<ISagaOrchestrator, SagaOrchestrator>();
            services.AddHttpContextAccessor();
            return services;
        }
    }
}
