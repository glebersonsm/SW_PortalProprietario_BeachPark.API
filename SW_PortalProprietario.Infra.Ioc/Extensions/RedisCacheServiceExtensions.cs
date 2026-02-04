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

            // Lê as configurações do Redis a partir das variáveis de ambiente ou appsettings.json
            var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD") 
                ?? configuration.GetValue<string>("Redis:Password");
            
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") 
                ?? configuration.GetValue<string>("Redis:Hosts:0:Host") 
                ?? "localhost";
            
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") 
                ?? configuration.GetValue<string>("Redis:Hosts:0:Port") 
                ?? "6379";
            
            var redisDatabase = Environment.GetEnvironmentVariable("REDIS_DATABASE") 
                ?? configuration.GetValue<string>("Redis:Database") 
                ?? "0";

            var programId = Environment.GetEnvironmentVariable("PROGRAM_ID") 
                ?? configuration.GetValue<string>("ProgramId") 
                ?? "PORTALCLIENTE_BP";

            // Monta a configuração do Redis manualmente
            var redisConfiguration = new RedisConfiguration
            {
                Password = redisPassword,
                AllowAdmin = false,
                Ssl = false,
                ConnectTimeout = 5000,
                SyncTimeout = 5000,
                KeyPrefix = programId,
                Hosts = new[]
                {
                    new RedisHost
                    {
                        Host = redisHost,
                        Port = int.Parse(redisPort)
                    }
                },
                Database = int.Parse(redisDatabase)
            };

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
