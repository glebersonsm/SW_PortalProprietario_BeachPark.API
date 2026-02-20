namespace SW_PortalProprietario.Application.Models.SystemModels
{
    /// <summary>
    /// ConfiguraÃ§Ãµes SMTP para envio de e-mail (podem vir de ParametroSistema ou appsettings).
    /// </summary>
    public class SmtpSettingsResult
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string User { get; set; } = "";
        public string Pass { get; set; } = "";
        public bool UseSsl { get; set; }
        public string? FromName { get; set; }
    }
}
