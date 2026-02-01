using Microsoft.Extensions.DependencyInjection;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Empreendimento;
using SW_PortalProprietario.Application.Services.TimeSharing;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    /// <summary>
    /// Extensões para configurar transações distribuídas (Saga Pattern)
    /// </summary>
    public static class DistributedTransactionExtensions
    {
        /// <summary>
        /// Registra os serviços necessários para transações distribuídas
        /// </summary>
        public static IServiceCollection AddDistributedTransactions(this IServiceCollection services)
        {
            // Registrar o orquestrador Saga como Scoped
            // Cada requisição terá sua própria instância
            services.AddScoped<SagaOrchestrator>();

            // Registrar Application Services que usam Saga
            // TimeSharing
            services.AddScoped<ITimeSharingReservaService, TimeSharingReservaService>();
            
            // Multipropriedade
            services.AddScoped<IMultipropriedadeService, MultipropriedadeService>();

            return services;
        }
    }
}
