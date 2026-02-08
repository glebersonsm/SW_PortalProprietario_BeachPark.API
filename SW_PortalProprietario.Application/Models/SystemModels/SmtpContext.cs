using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    /// <summary>
    /// Contexto SMTP para envio: credenciais (ParametroSistema ou .env) e tipo de cliente (MailKit ou System.Net.Mail).
    /// </summary>
    public class SmtpContext
    {
        /// <summary> Configurações SMTP quando disponíveis no sistema; null para usar fallback (.env). </summary>
        public SmtpSettingsResult? Settings { get; set; }

        /// <summary> Tipo de envio: ClienteEmailDireto = MailKit, ClienteEmailApp = System.Net.Mail. </summary>
        public EnumTipoEnvioEmail TipoEnvioEmail { get; set; } = EnumTipoEnvioEmail.ClienteEmailDireto;

        /// <summary> URL base para confirmação de leitura (pixel). Se null/vazio, usa IConfiguration["EmailTrackingBaseUrl"]. </summary>
        public string? EmailTrackingBaseUrl { get; set; }
    }
}
