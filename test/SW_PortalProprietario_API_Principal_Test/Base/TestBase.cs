using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SW_PortalProprietario.API;

namespace SW_PortalProprietario.Test.Base
{
    public class TestBase : WebApplicationFactory<SW_PortalProprietario.API.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Configuração para testes
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "TestKeyForJwtTokenGenerationInUnitTests123456789" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "OrigensPermitidas", "http://localhost:3000" },
                    { "UseBrokerType", "BrokerNaoConfigurado" }
                });
            });

            builder.ConfigureServices(services =>
            {
                // Configurações adicionais para testes podem ser adicionadas aqui
            });
        }
    }
}

