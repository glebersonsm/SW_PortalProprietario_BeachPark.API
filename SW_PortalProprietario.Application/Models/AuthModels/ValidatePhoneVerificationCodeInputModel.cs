namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para validação do código de verificação por SMS ao alterar o telefone celular do usuário.
    /// </summary>
    public class ValidatePhoneVerificationCodeInputModel
    {
        public int UserId { get; set; }
        public string? Phone { get; set; }
        public string? Code { get; set; }
    }
}
