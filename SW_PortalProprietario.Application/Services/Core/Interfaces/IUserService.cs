using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserRegisterResultModel> SaveUser(RegistroUsuarioFullInputModel model);
        Task<ChangePasswordResultModel> ChangePassword(ChangePasswordInputModel model);
        Task<(int pageNumber, int lastPageNumber, List<UsuarioModel> usuarios)?> Search(UsuarioSearchPaginatedModel searchModel);
        Task<List<UsuarioModel>?> SearchNotPaginated(UsuarioSearchModel searchModel);
        Task<string> ResetPassword(ResetPasswordoUserModel model);
        Task<Login2FAOptionsResultModel> GetResetPasswordChannelsAsync(string login);
    }
}
