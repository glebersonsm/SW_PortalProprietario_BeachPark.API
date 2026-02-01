using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using System.Security.Claims;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class AuthenticatedBaseService : IAuthenticatedBaseService
    {
        private readonly ICacheStore _cache;
        private readonly ILogger<AuthenticatedBaseService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private TokenResultModel? _tokenResultModel;
        public AuthenticatedBaseService(ICacheStore cacheStore,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthenticatedBaseService> logger)
        {
            _cache = cacheStore;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(string userId, string providerChaveUsuario, string companyId, bool isAdm)> GetLoggedUserAsync(bool throwIfNotLoggedIn = false)
        {
            try
            {

                var loggedUserClaim = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(a => a.Type == "UserId");
                string userId = "";
                if (loggedUserClaim != null && !string.IsNullOrEmpty(loggedUserClaim.Value))
                {
                    userId = loggedUserClaim.Value;
                }
                var providerKeyUserClaim = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(a => a.Type == "ProviderKeyUser");
                string _providerKeyUser = "";
                if (providerKeyUserClaim != null && !string.IsNullOrEmpty(providerKeyUserClaim.Value))
                    _providerKeyUser = providerKeyUserClaim.Value;

                var companyIdClaim = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(a => a.Type == "CompanyId");
                string _companyId = "";
                if (companyIdClaim != null && !string.IsNullOrEmpty(companyIdClaim.Value))
                    _companyId = companyIdClaim.Value;

                var role = _httpContextAccessor?.HttpContext?.User.FindAll(ClaimTypes.Role).Select(b => b.Value);

                var hasRoleAdmin = false;
                if (role != null && role.Any())
                {
                    hasRoleAdmin = role.Any(b => b.Equals("administrador", StringComparison.CurrentCultureIgnoreCase)) || role.Any(b => b.Equals("*", StringComparison.CurrentCultureIgnoreCase));
                }


                if (!string.IsNullOrEmpty(userId))
                {
                    if (_tokenResultModel == null)
                    {
                        var result = await _cache.GetAsync<TokenResultModel>(userId, default);
                        if (result != null)
                        {
                            _tokenResultModel = result;
                        }
                    }
                }

                if (_tokenResultModel == null && throwIfNotLoggedIn)
                    throw new Exception("Usuário não logado");

                return (userId, _providerKeyUser, _companyId, hasRoleAdmin);

            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao obter o usuário logado");
                throw;
            }
        }

        public async Task<string> GetToken()
        {
            // Obtém o token JWT do cabeçalho da requisição
            var bearerToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            return await Task.FromResult(bearerToken ?? string.Empty);
        }
    }
}
