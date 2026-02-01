using Microsoft.Extensions.DependencyInjection;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Infra.Data.ModelProfiles;
using SW_PortalProprietario.Infra.Ioc.Mapper;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IServiceCollection AddObjectMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(expression =>
            {
                expression.AllowNullCollections = true;
            }, typeof(AutoMapperConfigurationProfiles));

            services.AddScoped<IProjectObjectMapper, ProjectObjectMapper>();

            return services;

        }

    }
}
