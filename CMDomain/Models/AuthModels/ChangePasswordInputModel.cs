
namespace CMDomain.Models.AuthModels
{
    public class ChangePasswordInputModel
    {
        public string? CMUserName { get; set; }
        public string? ActualPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }

    }
}
