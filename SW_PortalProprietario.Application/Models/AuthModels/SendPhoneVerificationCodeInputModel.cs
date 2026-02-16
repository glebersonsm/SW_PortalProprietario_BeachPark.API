namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para envio do código de verificação por SMS ao alterar o telefone celular do usuário.
    /// </summary>
    public class SendPhoneVerificationCodeInputModel
    {
        public string? Phone { get; set; }
        public int? UserId { get; set; }
    }
}
