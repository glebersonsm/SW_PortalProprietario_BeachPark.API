using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Infra.Data;
using SW_PortalProprietario.Infra.Data.SwNHibernate;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class NHibernateExtensions
    {
        public static IServiceCollection AddNHbernate(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            var connectionString = System.Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Não foi configurada a string de conexão DEFAULT_CONNECTION.");
            }

            if (connectionString.Contains(".db", StringComparison.InvariantCultureIgnoreCase))
            {
                CreateSqLiteConnection(services, configuration, connectionString);
            }
            else if (connectionString.Contains("Initial Catalog", StringComparison.InvariantCultureIgnoreCase))
            {
                CreateSqlServerConnection(services, configuration, connectionString);
            }
            else if (connectionString.Contains("Host=", StringComparison.InvariantCultureIgnoreCase) && connectionString.Contains("PORT=", StringComparison.CurrentCultureIgnoreCase))
            {
                CreatePostgreSqlConnection(services, configuration, connectionString);
            }
            else if (connectionString.Contains("CONNECT_DATA", StringComparison.InvariantCultureIgnoreCase) && connectionString.Contains("DESCRIPTION", StringComparison.CurrentCultureIgnoreCase))
            {
                ConfigureOracleConnection(services, configuration, connectionString);
            }

            return services;
        }

        private static void ConfigureOracleConnection(IServiceCollection services, IConfiguration configuration, string? connectionString)
        {
            var _sessionFactory = Fluently.Configure()
                                .Database(OracleManagedDataClientConfiguration.Oracle10.ConnectionString(connectionString)
                                .ShowSql()
                                .AdoNetBatchSize(50)
                                .Raw("throw_on_error", "true"))
                                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AutomappingConfigurationDefault>());

            if (configuration.GetValue<bool>("UpdateDataBase"))
            {
                _sessionFactory.ExposeConfiguration(BuildSchema);
            }

            var sessionFactory = _sessionFactory.BuildSessionFactory();
            var sf = new SwSessionFactoryDefault() { SessionFactory = sessionFactory };

            services.TryAddSingleton<ISwSessionFactoryDefault>(sf);
            services.TryAddScoped(factory => sf.OpenStatelessSession());
        }

        private static void CreatePostgreSqlConnection(IServiceCollection services, IConfiguration configuration, string? connectionString)
        {
            var _sessionFactory = Fluently.Configure()
            .Database(PostgreSQLConfiguration.Standard.ConnectionString(connectionString)
            .Raw("hibernate.connection.provider", "NHibernate.Connection.C3P0ConnectionProvider") // Configuração do provedor de conexão
            .Raw("hibernate.c3p0.min_size", "5")  // Tamanho mínimo do pool de conexões
            .Raw("hibernate.c3p0.max_size", "300") // Tamanho máximo do pool de conexões
            .Raw("hibernate.c3p0.timeout", "300") // Tempo limite de conexão em segundos
            .Raw("default_schema", "portalohana")) // Schema padrão para PostgreSQL
            .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AutomappingConfigurationDefault>());

            if (configuration.GetValue<bool>("UpdateDataBase"))
            {
                _sessionFactory.ExposeConfiguration(BuildSchema);
            }


            var sessionFactory = _sessionFactory.BuildSessionFactory();
            var sf = new SwSessionFactoryDefault() { SessionFactory = sessionFactory };

            services.TryAddSingleton<ISwSessionFactoryDefault>(sf);
            services.TryAddScoped(factory => sf.OpenStatelessSession());
        }

        private static void CreateSqlServerConnection(IServiceCollection services, IConfiguration configuration, string? connectionString)
        {
            var _sessionFactory = Fluently.Configure()
                                .Database(MsSqlConfiguration.MsSql2012.ConnectionString(connectionString)
                                .ShowSql()
                                .AdoNetBatchSize(50)
                                .Raw("throw_on_error", "true"))
                                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AutomappingConfigurationDefault>());
            if (configuration.GetValue<bool>("UpdateDataBase"))
            {
                _sessionFactory.ExposeConfiguration(BuildSchema);
            }

            var sessionFactory = _sessionFactory.BuildSessionFactory();
            var sf = new SwSessionFactoryDefault() { SessionFactory = sessionFactory };

            services.TryAddSingleton<ISwSessionFactoryDefault>(sf);
            services.TryAddScoped(factory => sf.OpenStatelessSession());
        }

        private static void CreateSqLiteConnection(IServiceCollection services, IConfiguration configuration, string? connectionString)
        {
            var _sessionFactory = Fluently.Configure()
                                .Database(SQLiteConfiguration.Standard.ConnectionString(connectionString)
                                .ShowSql()
                                .AdoNetBatchSize(50)
                                .Raw("throw_on_error", "true"))
                                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AutomappingConfigurationDefault>());
            if (configuration.GetValue<bool>("UpdateDataBase"))
            {
                _sessionFactory.ExposeConfiguration(BuildSchema);
            }

            var sessionFactory = _sessionFactory.BuildSessionFactory();
            var sf = new SwSessionFactoryDefault() { SessionFactory = sessionFactory };

            services.TryAddSingleton<ISwSessionFactoryDefault>(sf);
            services.TryAddScoped(factory => sf.OpenStatelessSession());
        }

        private static void BuildSchema(Configuration configuration)
        {
            new SchemaUpdate(configuration)
                .Execute(useStdOut: false,
                doUpdate: true);
        }
    }
}
