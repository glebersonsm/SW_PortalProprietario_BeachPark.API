using Microsoft.AspNetCore.Http;
using System.Net;

namespace SW_PortalProprietario.Infra.Data.Middleware
{
    public class AuditContextMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Capturar IP Address (considerando proxies)
            var ipAddress = GetClientIpAddress(context);
            if (!string.IsNullOrEmpty(ipAddress))
            {
                context.Items["AuditIpAddress"] = ipAddress;
            }

            // Capturar User Agent
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
            if (!string.IsNullOrEmpty(userAgent))
            {
                context.Items["AuditUserAgent"] = userAgent;
            }

            // Capturar User ID se disponível
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.Claims.FirstOrDefault(c => 
                    c.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase));
                if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
                {
                    context.Items["AuditUserId"] = userIdClaim.Value;
                }
            }

            await _next(context);
        }

        private string? GetClientIpAddress(HttpContext context)
        {
            // Verificar se há IP no header (proxies/load balancers)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',');
                if (ips.Length > 0)
                {
                    var ip = ips[0].Trim();
                    if (IPAddress.TryParse(ip, out _))
                        return ip;
                }
            }

            // Verificar X-Real-IP
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp) && IPAddress.TryParse(realIp, out _))
            {
                return realIp;
            }

            // Usar IP da conexão direta
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}

