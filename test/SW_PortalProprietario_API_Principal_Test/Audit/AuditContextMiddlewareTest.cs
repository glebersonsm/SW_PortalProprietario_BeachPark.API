using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SW_PortalProprietario.Infra.Data.Middleware;
using System.Net;
using Xunit;

namespace SW_PortalProprietario.Test.Audit
{
    public class AuditContextMiddlewareTest
    {
        [Fact(DisplayName = "InvokeAsync - Deve capturar IP Address do HttpContext")]
        public async Task InvokeAsync_DeveCapturarIPAddress_DoHttpContext()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            
            var middleware = new AuditContextMiddleware((HttpContext ctx) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.Should().ContainKey("AuditIpAddress");
            context.Items["AuditIpAddress"].Should().Be("192.168.1.100");
        }

        [Fact(DisplayName = "InvokeAsync - Deve capturar IP de X-Forwarded-For quando disponÃ­vel")]
        public async Task InvokeAsync_DeveCapturarIP_DeXForwardedFor()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "10.0.0.1, 192.168.1.1";
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            
            var middleware = new AuditContextMiddleware((HttpContext ctx) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.Should().ContainKey("AuditIpAddress");
            context.Items["AuditIpAddress"].Should().Be("10.0.0.1");
        }

        [Fact(DisplayName = "InvokeAsync - Deve capturar IP de X-Real-IP quando disponÃ­vel")]
        public async Task InvokeAsync_DeveCapturarIP_DeXRealIP()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Real-IP"] = "172.16.0.1";
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            
            var middleware = new AuditContextMiddleware((HttpContext ctx) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.Should().ContainKey("AuditIpAddress");
            context.Items["AuditIpAddress"].Should().Be("172.16.0.1");
        }

        [Fact(DisplayName = "InvokeAsync - Deve capturar User Agent")]
        public async Task InvokeAsync_DeveCapturarUserAgent()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
            
            var middleware = new AuditContextMiddleware((HttpContext ctx) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.Should().ContainKey("AuditUserAgent");
            context.Items["AuditUserAgent"].Should().Be("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        [Fact(DisplayName = "InvokeAsync - Deve capturar User ID quando autenticado")]
        public async Task InvokeAsync_DeveCapturarUserId_QuandoAutenticado()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var identity = new System.Security.Claims.ClaimsIdentity("TestAuth", "UserId", "Role");
            identity.AddClaim(new System.Security.Claims.Claim("UserId", "123"));
            context.User = new System.Security.Claims.ClaimsPrincipal(identity);
            
            var middleware = new AuditContextMiddleware((HttpContext ctx) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.Should().ContainKey("AuditUserId");
            context.Items["AuditUserId"].Should().Be("123");
        }

        [Fact(DisplayName = "InvokeAsync - NÃ£o deve capturar User ID quando nÃ£o autenticado")]
        public async Task InvokeAsync_NaoDeveCapturarUserId_QuandoNaoAutenticado()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal();
            
            var middleware = new AuditContextMiddleware((HttpContext ctx) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.Should().NotContainKey("AuditUserId");
        }

        [Fact(DisplayName = "InvokeAsync - Deve chamar prÃ³ximo middleware")]
        public async Task InvokeAsync_DeveChamarProximoMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var nextCalled = false;
            
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };
            
            var middleware = new AuditContextMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
        }

        [Fact(DisplayName = "InvokeAsync - Deve processar mÃºltiplos IPs em X-Forwarded-For")]
        public async Task InvokeAsync_DeveProcessarMultiplosIPs_EmXForwardedFor()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "203.0.113.1, 198.51.100.1, 192.168.1.1";
            
            var middleware = new AuditContextMiddleware((HttpContext ctx) => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.Should().ContainKey("AuditIpAddress");
            context.Items["AuditIpAddress"].Should().Be("203.0.113.1"); // Primeiro IP da lista
        }
    }
}

