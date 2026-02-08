using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Interfaces
{
    /// <summary>
    /// Fornece configurações SMTP para envio de e-mail (prioridade: ParametroSistema, depois appsettings/.env).
    /// </summary>
    public interface ISmtpSettingsProvider
    {
        /// <summary>
        /// Obtém as configurações SMTP. Retorna null se não houver configuração válida no sistema (ParametroSistema).
        /// O consumidor deve usar appsettings como fallback quando null.
        /// </summary>
        Task<SmtpSettingsResult?> GetSmtpSettingsFromParametroSistemaAsync(CancellationToken cancellationToken = default);
    }
}
