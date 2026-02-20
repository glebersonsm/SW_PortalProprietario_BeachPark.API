namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para envio do cÃ³digo 2FA pelo canal escolhido (sem validaÃ§Ã£o de senha).
    /// Usado quando o login estÃ¡ configurado com 2FA obrigatÃ³rio.
    /// </summary>
    public class SendTwoFactorCodeInputModel
    {
        public string? Login { get; set; }
        /// <summary> Canal para envio: "email" | "sms" </summary>
        public string? TwoFactorChannel { get; set; }
    }
}
