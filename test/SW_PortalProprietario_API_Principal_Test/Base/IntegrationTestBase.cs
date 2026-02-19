using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SW_PortalProprietario.Test.Base
{
    /// <summary>
    /// Base class para testes de integraÃ§Ã£o com banco de dados em memÃ³ria
    /// </summary>
    public class IntegrationTestBase : TestBase, IDisposable
    {
        private bool _disposed = false;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // ConfiguraÃ§Ã£o para testes
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "TestKeyForJwtTokenGenerationInUnitTests123456789012345678901234567890" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "OrigensPermitidas", "http://localhost:3000" },
                    { "UseBrokerType", "BrokerNaoConfigurado" },
                    { "UpdateDataBase", "false" },
                    { "ByPass", "true" },
                    // Connection string para SQLite in-memory
                    { "ConnectionStrings:DefaultConnection", "Data Source=:memory:;Version=3;New=True;" },
                    { "ConnectionStrings:eSolutionPortal", "Data Source=:memory:;Version=3;New=True;" },
                    { "ConnectionStrings:eSolutionAccessCenter", "Data Source=:memory:;Version=3;New=True;" }
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove serviÃ§os HostedService que causam problemas de injeÃ§Ã£o de dependÃªncia nos testes
                var hostedServices = services.Where(s => s.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService)).ToList();
                foreach (var service in hostedServices)
                {
                    services.Remove(service);
                }
                
                // Remove serviÃ§os problemÃ¡ticos de email queue que tentam consumir serviÃ§os Scoped
                var emailQueueServices = services.Where(s => 
                    s.ServiceType.FullName?.Contains("IEmailSenderFromQueueConsumer") == true ||
                    s.ServiceType.FullName?.Contains("IBackGroundSenderEmailFromProcessingQueue") == true ||
                    s.ServiceType.FullName?.Contains("EmailSenderFromProcessingQueueConsumer") == true).ToList();
                foreach (var service in emailQueueServices)
                {
                    services.Remove(service);
                }
            });
        }

        /// <summary>
        /// Cria um token JWT para autenticaÃ§Ã£o nos testes
        /// </summary>
        protected string GenerateJwtToken(string userId = "1", string? role = null, string? companyId = null, bool isAdm = false)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("TestKeyForJwtTokenGenerationInUnitTests123456789012345678901234567890"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("UserId", userId),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            if (!string.IsNullOrEmpty(companyId))
            {
                claims.Add(new Claim("CompanyId", companyId));
            }

            if (isAdm)
            {
                claims.Add(new Claim("IsAdm", "true"));
            }

            var token = new JwtSecurityToken(
                issuer: "TestIssuer",
                audience: "TestAudience",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Cria um HttpClient autenticado para os testes
        /// </summary>
        protected HttpClient CreateAuthenticatedClient(string userId = "1", string? role = null, string? companyId = null, bool isAdm = false)
        {
            var client = CreateClient();
            var token = GenerateJwtToken(userId, role, companyId, isAdm);
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cleanup se necessÃ¡rio
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}

