using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Infra.Data.Caching;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class RedisCacheServicesExtension
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            var redisConfiguration = configuration.GetSection("Redis").Get<RedisConfiguration>();
            ArgumentNullException.ThrowIfNull(redisConfiguration, nameof(redisConfiguration));

            redisConfiguration.KeyPrefix = configuration.GetValue<string>("ProgramId");
            redisConfiguration.ConnectTimeout = 5000;
            redisConfiguration.SyncTimeout = 5000;

            ThreadPool.SetMinThreads(200, 200);
            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(new List<RedisConfiguration>
            {
                redisConfiguration
            });

            services.AddSingleton<ICacheStore, CacheStore>();

            return services;
        }

    }
}
