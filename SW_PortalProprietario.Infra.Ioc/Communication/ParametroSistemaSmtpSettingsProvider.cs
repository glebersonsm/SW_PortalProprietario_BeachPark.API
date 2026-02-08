using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Ioc.Communication
{
    /// <summary>
    /// Obtém configurações SMTP a partir de ParametroSistema (banco). Usa escopo para resolver IRepositoryHosted.
    /// </summary>
    public class ParametroSistemaSmtpSettingsProvider : ISmtpSettingsProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ParametroSistemaSmtpSettingsProvider> _logger;

        public ParametroSistemaSmtpSettingsProvider(
            IServiceProvider serviceProvider,
            ILogger<ParametroSistemaSmtpSettingsProvider> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<SmtpSettingsResult?> GetSmtpSettingsFromParametroSistemaAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRepositoryHosted>();
                var param = await repository.GetParametroSistemaViewModel();
                if (param == null)
                    return null;
                if (string.IsNullOrWhiteSpace(param.SmtpHost) || string.IsNullOrWhiteSpace(param.SmtpUser) || string.IsNullOrWhiteSpace(param.SmtpPass))
                    return null;
                var port = param.SmtpPort ?? 0;
                if (port <= 0)
                    return null;

                return new SmtpSettingsResult
                {
                    Host = param.SmtpHost.Trim(),
                    Port = port,
                    User = param.SmtpUser.Trim(),
                    Pass = param.SmtpPass.Trim(),
                    UseSsl = param.SmtpUseSsl.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim,
                    FromName = string.IsNullOrWhiteSpace(param.SmtpFromName) ? null : param.SmtpFromName.Trim()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível obter configurações SMTP de ParametroSistema. Será usado appsettings.");
                return null;
            }
        }
    }
}
