using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace SW_PortalProprietario.Application.Services.Providers.Esolution
{
    public class JwtTokenService : ITokenBodyService
    {
        public Dictionary<string, object> GetBodyToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var values = jwtSecurityToken.Payload;

            return values;
        }
    }
}
