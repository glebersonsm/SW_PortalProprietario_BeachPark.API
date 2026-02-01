using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.Application.Hosted
{
    public class AuthenticatedBaseHostedService : IAuthenticatedBaseHostedService
    {
        private readonly ICacheStore _cache;
        private readonly ILogger<AuthenticatedBaseHostedService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private TokenResultModel? _tokenResultModel;
        public AuthenticatedBaseHostedService(ICacheStore cacheStore,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthenticatedBaseHostedService> logger)
        {
            _cache = cacheStore;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<TokenResultModel?> GetLoggedUserAsync(bool throwIfNotLoggedIn = true)
        {
            try
            {
                if (_tokenResultModel != null)
                    return _tokenResultModel;

                var loggedUserClaim = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(a => a.Type == "UserId");
                string userId = "";
                if (loggedUserClaim != null && !string.IsNullOrEmpty(loggedUserClaim.Value))
                {
                    userId = loggedUserClaim.Value;
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    var result = await _cache.GetAsync<TokenResultModel>(userId, default);
                    if (result != null)
                    {
                        _tokenResultModel = result;
                    }
                }
                else if (throwIfNotLoggedIn)
                    throw new Exception("Não foi possível identificar o usuário logado!");

                return _tokenResultModel;

            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao obter o usuário logado");
                throw;
            }
        }

    }
}
