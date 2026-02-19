namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para envio do cÃ³digo de verificaÃ§Ã£o por SMS ao alterar o telefone celular do usuÃ¡rio.
    /// </summary>
    public class SendPhoneVerificationCodeInputModel
    {
        public string? Phone { get; set; }
        public int? UserId { get; set; }
    }
}
