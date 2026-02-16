namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para envio do código de verificação de e-mail ao alterar o e-mail do usuário.
    /// </summary>
    public class SendEmailVerificationCodeInputModel
    {
        public string? Email { get; set; }
        public int? UserId { get; set; }
    }
}
