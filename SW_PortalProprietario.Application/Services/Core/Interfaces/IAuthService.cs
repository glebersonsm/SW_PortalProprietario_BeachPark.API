using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IAuthService
    {
        Task<UserRegisterResultModel> Register(UserRegisterInputModel userInputModel);
        Task<TokenResultModel?> Login(LoginInputModel userLoginInputModel);
        Task<TokenResultModel> ChangeActualCompanyId(SetCompanyModel model);
    }
}
