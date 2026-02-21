using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ReservasApi;
using SW_PortalProprietario.Application.Interfaces.ReservaCm;
using SW_PortalProprietario.Application.Services.Providers.Cm;
using SW_PortalProprietario.Application.Services.Providers.Hybrid;
using SW_PortalProprietario.Application.Services.Providers.Esolution;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Application.Services.ReservasApi;
using SW_PortalProprietario.Application.Services.ReservaCm;
using SW_PortalProprietario.Infra.Data.Repositories.Core;
using SW_PortalProprietario.Infra.Data.Repositories.ReservaCm;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class ProviderServiceExtensions
    {
        public static IServiceCollection RegisterProviders(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            // AccessCenter & Esol
            services.AddNHbernateAccessCenter(configuration);
            services.AddNHbernatePortalEsol(configuration);
            services.TryAddScoped<IUnitOfWorkNHAccessCenter, UnitOfWorkNHAccessCenter>();
            services.TryAddScoped<IRepositoryNHAccessCenter, RepositoryNHAccessCenter>();
            services.TryAddScoped<IUnitOfWorkNHEsolPortal, UnitOfWorkNHEsolutionPortal>();
            services.TryAddScoped<IRepositoryNHEsolPortal, RepositoryNHEsolPortal>();

            // CM
            services.AddNHbernateCM(configuration);
            services.TryAddScoped<IUnitOfWorkNHCm, UnitOfWorkNHCm>();
            services.TryAddScoped<IRepositoryNHCm, RepositoryNHCm>();

            // Hybrid Provider
            services.TryAddScoped<IHybrid_CM_Esolution_Communication, SW_PortalProprietario.Infra.Data.CommunicationProviders.Hybrid.Hybrid_CM_Esolution_Communication>();
            services.TryAddScoped<ICommunicationProvider>(sp => sp.GetRequiredService<IHybrid_CM_Esolution_Communication>());

            // Services
            services.TryAddScoped<IHybridProviderService, HybridProviderService>();
            services.TryAddScoped<IEmpreendimentoHybridProviderService, EmpreendimentoHybridService>();
            services.TryAddScoped<IFinanceiroHybridProviderService, FinanceiroHybridService>();

            // TimeSharing
            services.TryAddScoped<TimeSharingCmService>();
            services.TryAddScoped<TimeSharingEsolutionService>();
            services.TryAddScoped<ITimeSharingProviderService, TimeSharingEsolutionService>();

            // Reserva CM (migrado do SW_CMApi)
            services.TryAddScoped<IReservaCMRepository, ReservaCMRepository>();
            services.TryAddScoped<IParametroHotelCMRepository, ParametroHotelCMRepository>();
            services.TryAddScoped<IReservaCMService, ReservaCMService>();

            return services;
        }

    }
}
