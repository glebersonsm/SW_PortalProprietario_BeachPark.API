namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Resposta do endpoint de opções 2FA por login (sem senha).
    /// </summary>
    public class Login2FAOptionsResultModel
    {
        public bool RequiresTwoFactor { get; set; }
        /// <summary> "Administrador" | "Cliente" </summary>
        public string? UserType { get; set; }
        public List<Login2FAChannelModel> Channels { get; set; } = new List<Login2FAChannelModel>();
    }

    public class Login2FAChannelModel
    {
        /// <summary> "email" | "sms" </summary>
        public string Type { get; set; } = "";
        /// <summary> Exibição pseudoanonimizada (ex.: j***@gm***.com ou (85) *****7890) </summary>
        public string Display { get; set; } = "";
    }
}
