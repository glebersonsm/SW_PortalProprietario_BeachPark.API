namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para envio do cÃ³digo de verificaÃ§Ã£o de e-mail ao alterar o e-mail do usuÃ¡rio.
    /// </summary>
    public class SendEmailVerificationCodeInputModel
    {
        public string? Email { get; set; }
        public int? UserId { get; set; }
    }
}
