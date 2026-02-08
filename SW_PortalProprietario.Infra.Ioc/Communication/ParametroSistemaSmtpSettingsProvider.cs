using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Ioc.Communication
{
    /// <summary>
    /// Obtém configurações SMTP a partir de ParametroSistema (banco).
    /// Usa IRepositoryNH (mesmo repositório que grava os parâmetros na tela Configurações) para garantir
    /// que o envio use os dados salvos pelo usuário; fallback para IRepositoryHosted se NH não estiver disponível no escopo.
    /// Se a senha estiver vazia em ParametroSistema, usa a senha do .env/appsettings (SmtpPass ou SmptPass).
    /// </summary>
    public class ParametroSistemaSmtpSettingsProvider : ISmtpSettingsProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ParametroSistemaSmtpSettingsProvider> _logger;
        private readonly IConfiguration _configuration;

        public ParametroSistemaSmtpSettingsProvider(
            IServiceProvider serviceProvider,
            ILogger<ParametroSistemaSmtpSettingsProvider> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>Lê senha SMTP do .env/appsettings (IConfiguration); aceita "SmtpPass" ou o typo "SmptPass".</summary>
        private string? GetSmtpPassFromConfig()
        {
            var pass = _configuration.GetValue<string>("SmtpPass");
            if (!string.IsNullOrWhiteSpace(pass)) return pass.Trim();
            pass = _configuration.GetValue<string>("SmptPass");
            return string.IsNullOrWhiteSpace(pass) ? null : pass.Trim();
        }

        public async Task<SmtpSettingsResult?> GetSmtpSettingsFromParametroSistemaAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var param = await GetParametroSistemaFromRepositoryAsync(scope.ServiceProvider).ConfigureAwait(false);
                if (param == null)
                    return null;
                if (string.IsNullOrWhiteSpace(param.SmtpHost) || string.IsNullOrWhiteSpace(param.SmtpUser))
                    return null;
                var pass = !string.IsNullOrWhiteSpace(param.SmtpPass) ? param.SmtpPass.Trim() : GetSmtpPassFromConfig();
                if (string.IsNullOrWhiteSpace(pass))
                    return null;
                var port = param.SmtpPort ?? 0;
                if (port <= 0)
                    return null;

                return new SmtpSettingsResult
                {
                    Host = param.SmtpHost.Trim(),
                    Port = port,
                    User = param.SmtpUser.Trim(),
                    Pass = pass,
                    UseSsl = param.SmtpUseSsl.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim,
                    FromName = string.IsNullOrWhiteSpace(param.SmtpFromName) ? null : param.SmtpFromName.Trim()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível obter configurações SMTP de ParametroSistema. Será usado fallback (.env / appsettings).");
                return null;
            }
        }

        public async Task<SmtpContext> GetSmtpContextAsync(CancellationToken cancellationToken = default)
        {
            var ctx = new SmtpContext();
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var param = await GetParametroSistemaFromRepositoryAsync(scope.ServiceProvider).ConfigureAwait(false);
                ctx.TipoEnvioEmail = param?.TipoEnvioEmail ?? EnumTipoEnvioEmail.ClienteEmailDireto;

                if (param == null)
                    return ctx;
                if (string.IsNullOrWhiteSpace(param.SmtpHost) || string.IsNullOrWhiteSpace(param.SmtpUser))
                    return ctx;
                var pass = !string.IsNullOrWhiteSpace(param.SmtpPass) ? param.SmtpPass.Trim() : GetSmtpPassFromConfig();
                if (string.IsNullOrWhiteSpace(pass))
                    return ctx;
                var port = param.SmtpPort ?? 0;
                if (port <= 0)
                    return ctx;

                ctx.Settings = new SmtpSettingsResult
                {
                    Host = param.SmtpHost.Trim(),
                    Port = port,
                    User = param.SmtpUser.Trim(),
                    Pass = pass,
                    UseSsl = param.SmtpUseSsl.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim,
                    FromName = string.IsNullOrWhiteSpace(param.SmtpFromName) ? null : param.SmtpFromName.Trim()
                };
                return ctx;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível obter contexto SMTP de ParametroSistema. Será usado fallback (.env / appsettings) e ClienteEmailDireto.");
                return ctx;
            }
        }

        /// <summary>
        /// Obtém ParametroSistema: tenta IRepositoryNH primeiro (mesma fonte que a tela Configurações),
        /// depois IRepositoryHosted para cenários de hosted service.
        /// </summary>
        private static async Task<ParametroSistemaViewModel?> GetParametroSistemaFromRepositoryAsync(IServiceProvider serviceProvider)
        {
            var repoNH = serviceProvider.GetService<IRepositoryNH>();
            if (repoNH != null)
            {
                try
                {
                    var param = await repoNH.GetParametroSistemaViewModel().ConfigureAwait(false);
                    if (param != null)
                        return param;
                }
                catch
                {
                    // Ignora e tenta Hosted
                }
            }

            var repoHosted = serviceProvider.GetService<IRepositoryHosted>();
            if (repoHosted != null)
                return await repoHosted.GetParametroSistemaViewModel().ConfigureAwait(false);

            return null;
        }
    }
}
