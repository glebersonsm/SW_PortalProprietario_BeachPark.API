namespace SW_PortalProprietario.Application.Models.AuthModels
{
    /// <summary>
    /// Modelo para validaÃ§Ã£o do cÃ³digo de verificaÃ§Ã£o por SMS ao alterar o telefone celular do usuÃ¡rio.
    /// </summary>
    public class ValidatePhoneVerificationCodeInputModel
    {
        public int UserId { get; set; }
        public string? Phone { get; set; }
        public string? Code { get; set; }
    }
}
