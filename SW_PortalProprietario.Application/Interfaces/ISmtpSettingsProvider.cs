using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Interfaces
{
    /// <summary>
    /// Fornece configuraÃ§Ãµes SMTP e tipo de envio (prioridade: ParametroSistema, depois appsettings/.env).
    /// </summary>
    public interface ISmtpSettingsProvider
    {
        /// <summary>
        /// ObtÃ©m as configuraÃ§Ãµes SMTP. Retorna null se nÃ£o houver configuraÃ§Ã£o vÃ¡lida no sistema (ParametroSistema).
        /// O consumidor deve usar appsettings como fallback quando null.
        /// </summary>
        Task<SmtpSettingsResult?> GetSmtpSettingsFromParametroSistemaAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// ObtÃ©m contexto completo: credenciais (Settings ou null para fallback .env) e TipoEnvioEmail para escolher MailKit ou System.Net.Mail.
        /// </summary>
        Task<SmtpContext> GetSmtpContextAsync(CancellationToken cancellationToken = default);
    }
}
