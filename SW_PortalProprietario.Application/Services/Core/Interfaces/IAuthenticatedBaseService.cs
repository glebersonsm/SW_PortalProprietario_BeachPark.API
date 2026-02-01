namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IAuthenticatedBaseService
    {
        Task<(string userId, string providerChaveUsuario, string companyId, bool isAdm)> GetLoggedUserAsync(bool throwIfNotLoggedIn = false);
        Task<string> GetToken();
    }
}
