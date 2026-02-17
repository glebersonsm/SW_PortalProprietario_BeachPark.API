namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para validação do código de verificação por e-mail ao alterar o e-mail do usuário.
    /// </summary>
    public class ValidateEmailVerificationCodeInputModel
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
    }
}
