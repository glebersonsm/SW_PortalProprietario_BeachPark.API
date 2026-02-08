using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Interfaces
{
    /// <summary>
    /// Fornece configurações SMTP e tipo de envio (prioridade: ParametroSistema, depois appsettings/.env).
    /// </summary>
    public interface ISmtpSettingsProvider
    {
        /// <summary>
        /// Obtém as configurações SMTP. Retorna null se não houver configuração válida no sistema (ParametroSistema).
        /// O consumidor deve usar appsettings como fallback quando null.
        /// </summary>
        Task<SmtpSettingsResult?> GetSmtpSettingsFromParametroSistemaAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtém contexto completo: credenciais (Settings ou null para fallback .env) e TipoEnvioEmail para escolher MailKit ou System.Net.Mail.
        /// </summary>
        Task<SmtpContext> GetSmtpContextAsync(CancellationToken cancellationToken = default);
    }
}
