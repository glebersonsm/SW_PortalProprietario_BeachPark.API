namespace SW_PortalProprietario.Application.Models.AuthModels
{
    public class ValidateTwoFactorInputModel
    {
        public Guid TwoFactorId { get; set; }
        public string? Code { get; set; }
    }
}
