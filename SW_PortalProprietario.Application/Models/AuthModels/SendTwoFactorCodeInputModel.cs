namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para envio do código 2FA pelo canal escolhido (sem validação de senha).
    /// Usado quando o login está configurado com 2FA obrigatório.
    /// </summary>
    public class SendTwoFactorCodeInputModel
    {
        public string? Login { get; set; }
        /// <summary> Canal para envio: "email" | "sms" </summary>
        public string? TwoFactorChannel { get; set; }
    }
}
