namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para validaÃ§Ã£o do cÃ³digo de verificaÃ§Ã£o por e-mail ao alterar o e-mail do usuÃ¡rio.
    /// </summary>
    public class ValidateEmailVerificationCodeInputModel
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
    }
}
