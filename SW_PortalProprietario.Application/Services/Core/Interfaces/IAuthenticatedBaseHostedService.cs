using SW_PortalProprietario.Application.Models.AuthModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IAuthenticatedBaseHostedService
    {
        Task<TokenResultModel?> GetLoggedUserAsync(bool throwIfNotLoggedIn = true);
    }
}
