using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ReservasApi;
using SW_PortalProprietario.Application.Services.Providers.Cm;
using SW_PortalProprietario.Application.Services.Providers.Default;
using SW_PortalProprietario.Application.Services.Providers.Esolution;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Application.Services.ReservasApi;
using SW_PortalProprietario.Infra.Data.CommunicationProviders.CM;
using SW_PortalProprietario.Infra.Data.CommunicationProviders.Default;
using SW_PortalProprietario.Infra.Data.CommunicationProviders.Esolution;
using SW_PortalProprietario.Infra.Data.Repositories.Core;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class ProviderServiceExtensions
    {
        public static IServiceCollection RegisterProviders(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            var integradoCom = configuration.GetValue<string>("IntegradoCom");
            if (!string.IsNullOrEmpty(integradoCom))
            {
                switch (integradoCom.ToLower())
                {
                    case "esolution":
                        services.AddNHbernateAccessCenter(configuration);
                        services.AddNHbernatePortalEsol(configuration);
                        services.TryAddScoped<IUnitOfWorkNHAccessCenter, UnitOfWorkNHAccessCenter>();
                        services.TryAddScoped<IRepositoryNHAccessCenter, RepositoryNHAccessCenter>();
                        services.TryAddScoped<IUnitOfWorkNHEsolPortal, UnitOfWorkNHEsolutionPortal>();
                        services.TryAddScoped<IRepositoryNHEsolPortal, RepositoryNHEsolPortal>();
                        services.TryAddScoped<ICommunicationProvider, EsolutionCommunicationProvider>();
                        services.TryAddScoped<IFinanceiroProviderService, FinanceiroEsolutionService>();
                        services.TryAddScoped<IEmpreendimentoProviderService, EmpreendimentoEsolutionService>();
                        services.TryAddScoped<ITimeSharingProviderService, TimeSharingEsolutionService>();
                        break;
                    case "cm":
                        services.AddNHbernateCM(configuration);
                        services.TryAddScoped<IUnitOfWorkNHCm, UnitOfWorkNHCm>();
                        services.TryAddScoped<IRepositoryNHCm, RepositoryNHCm>();
                        services.TryAddScoped<ICommunicationProvider, CmCommunicationProvider>();
                        services.TryAddScoped<IFinanceiroProviderService, FinanceiroCmService>();
                        services.TryAddScoped<IEmpreendimentoProviderService, EmpreendimentoCmService>();
                        services.TryAddScoped<ITimeSharingProviderService, TimeSharingCmService>();
                        break;
                    default:
                        break;
                }

            }
            else
            {
                services.TryAddScoped<ICommunicationProvider, CommunicationProviderDefault>();
                services.TryAddScoped<IFinanceiroProviderService, FinanceiroDefaultService>();
                services.TryAddScoped<IEmpreendimentoProviderService, EmpreendimentoDefaultService>();
                services.TryAddScoped<ITimeSharingProviderService, TimeSharingDefaultService>();
            }

            return services;
        }

    }
}
