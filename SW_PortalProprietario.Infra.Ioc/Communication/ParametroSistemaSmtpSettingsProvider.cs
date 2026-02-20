using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Ioc.Communication
{
    /// <summary>
    /// ObtÃ©m configuraÃ§Ãµes SMTP a partir de ParametroSistema (banco).
    /// Usa IRepositoryNH (mesmo repositÃ³rio que grava os parÃ¢metros na tela ConfiguraÃ§Ãµes) para garantir
    /// que o envio use os dados salvos pelo usuÃ¡rio; fallback para IRepositoryHosted se NH nÃ£o estiver disponÃ­vel no escopo.
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

        /// <summary>LÃª senha SMTP do .env/appsettings (IConfiguration); aceita "SmtpPass" ou o typo "SmptPass".</summary>
        private string? GetSmtpPassFromConfig()
        {
            var pass = _configuration.GetValue<string>("SmtpPass");
            if (!string.IsNullOrWhiteSpace(pass)) return pass.Trim();
            pass = _configuration.GetValue<string>("SmptPass");
            return string.IsNullOrWhiteSpace(pass) ? null : pass.Trim();
        }

        /// <summary>Host e porta padrÃ£o para AWS SES SMTP (regiÃ£o sa-east-1).</summary>
        private const string AwsSesSmtpHostDefault = "email-smtp.sa-east-1.amazonaws.com";
        private const int AwsSesSmtpPortDefault = 587;

        public async Task<SmtpSettingsResult?> GetSmtpSettingsFromParametroSistemaAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var param = await GetParametroSistemaFromRepositoryAsync(scope.ServiceProvider).ConfigureAwait(false);
                if (param == null)
                    return null;
                var isAws = param.TipoEnvioEmail == EnumTipoEnvioEmail.AwsSes;
                var host = !string.IsNullOrWhiteSpace(param.SmtpHost) ? param.SmtpHost.Trim() : (isAws ? AwsSesSmtpHostDefault : null);
                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(param.SmtpUser))
                    return null;
                var pass = !string.IsNullOrWhiteSpace(param.SmtpPass) ? param.SmtpPass.Trim() : GetSmtpPassFromConfig();
                if (string.IsNullOrWhiteSpace(pass))
                    return null;
                var port = param.SmtpPort ?? 0;
                if (port <= 0)
                    port = isAws ? AwsSesSmtpPortDefault : 0;
                if (port <= 0)
                    return null;

                return new SmtpSettingsResult
                {
                    Host = host,
                    Port = port,
                    User = isAws ? param.SmtpIamUser! : param.SmtpUser.Trim(),
                    Pass = pass,
                    UseSsl = isAws || param.SmtpUseSsl.GetValueOrDefault(EnumSimNao.Nao) == EnumSimNao.Sim,
                    FromName = isAws ? param.SmtpIamUser : string.IsNullOrWhiteSpace(param.SmtpFromName) ? null : param.SmtpFromName.Trim()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NÃ£o foi possÃ­vel obter configuraÃ§Ãµes SMTP de ParametroSistema. SerÃ¡ usado fallback (.env / appsettings).");
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
                ctx.EmailTrackingBaseUrl = !string.IsNullOrWhiteSpace(param?.EmailTrackingBaseUrl) ? param.EmailTrackingBaseUrl!.Trim() : null;

                if (param == null)
                    return ctx;
                var isAws = param.TipoEnvioEmail == EnumTipoEnvioEmail.AwsSes;
                var host = !string.IsNullOrWhiteSpace(param.SmtpHost) ? param.SmtpHost.Trim() : (isAws ? AwsSesSmtpHostDefault : null);
                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(param.SmtpUser))
                    return ctx;
                var pass = !string.IsNullOrWhiteSpace(param.SmtpPass) ? param.SmtpPass.Trim() : GetSmtpPassFromConfig();
                if (string.IsNullOrWhiteSpace(pass))
                    return ctx;
                var port = isAws ? AwsSesSmtpPortDefault : param.SmtpPort ?? 0;
                if (port <= 0)
                    port = isAws ? AwsSesSmtpPortDefault : 0;
                if (port <= 0)
                    return ctx;

                ctx.Settings = new SmtpSettingsResult
                {
                    Host = host,
                    Port = port,
                    User = param.SmtpUser.Trim(),
                    Pass = pass,
                    UseSsl = isAws || param.SmtpUseSsl.GetValueOrDefault(EnumSimNao.Nao) == EnumSimNao.Sim,
                    FromName = isAws ? param.SmtpIamUser : string.IsNullOrWhiteSpace(param.SmtpFromName) ? null : param.SmtpFromName.Trim()
                };
                return ctx;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NÃ£o foi possÃ­vel obter contexto SMTP de ParametroSistema. SerÃ¡ usado fallback (.env / appsettings) e ClienteEmailDireto.");
                return ctx;
            }
        }

        /// <summary>
        /// ObtÃ©m ParametroSistema: tenta IRepositoryNH primeiro (mesma fonte que a tela ConfiguraÃ§Ãµes),
        /// depois IRepositoryHosted para cenÃ¡rios de hosted service.
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
