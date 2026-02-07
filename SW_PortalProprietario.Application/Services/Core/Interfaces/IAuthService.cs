using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IAuthService
    {
        Task<UserRegisterResultModel> Register(UserRegisterInputModel userInputModel);
        Task<TokenResultModel?> Login(LoginInputModel userLoginInputModel);
        Task<TokenResultModel> ChangeActualCompanyId(SetCompanyModel model);
        Task<Login2FAOptionsResultModel> GetLogin2FAOptionsAsync(string login);
        Task<TokenResultModel?> ValidateTwoFactorAsync(ValidateTwoFactorInputModel model);
    }
}
