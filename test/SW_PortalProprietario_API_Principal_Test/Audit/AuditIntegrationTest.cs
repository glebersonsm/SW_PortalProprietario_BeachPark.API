using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Infra.Data.Audit;
using SW_PortalProprietario.Infra.Data.Middleware;
using SW_Utils.Models;
using System.Text.Json;
using Xunit;

namespace SW_PortalProprietario.Test.Audit
{
    /// <summary>
    /// Testes de integraÃ§Ã£o que validam o fluxo completo de auditoria
    /// </summary>
    public class AuditIntegrationTest
    {
        [Fact(DisplayName = "Fluxo Completo - Deve capturar contexto HTTP e gerar log de criaÃ§Ã£o")]
        public async Task FluxoCompleto_DeveCapturarContextoHTTP_EGerarLogDeCriacao()
        {
            // Arrange
            var auditQueueProducerMock = new Mock<IAuditLogQueueProducer>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            
            var httpContext = new DefaultHttpContext();
            httpContext.Items["AuditIpAddress"] = "192.168.1.100";
            httpContext.Items["AuditUserAgent"] = "Mozilla/5.0";
            httpContext.Items["AuditUserId"] = "42";
            
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            var auditHelper = new AuditHelper(auditQueueProducerMock.Object, httpContextAccessorMock.Object);
            
            var cidade = new Cidade
            {
                Id = 1,
                Nome = "Rio de Janeiro",
                CodigoIbge = "3304557",
                UsuarioCriacao = 42,
                DataHoraCriacao = DateTime.Now,
                ObjectGuid = Guid.NewGuid().ToString()
            };

            AuditLogMessageEvent? capturedMessage = null;
            auditQueueProducerMock
                .Setup(x => x.EnqueueAuditLogAsync(It.IsAny<AuditLogMessageEvent>()))
                .Callback<AuditLogMessageEvent>(msg => capturedMessage = msg)
                .Returns(Task.CompletedTask);

            // Act
            await auditHelper.LogCreateAsync(cidade);

            // Assert
            capturedMessage.Should().NotBeNull();
            capturedMessage!.EntityType.Should().Be("Cidade");
            capturedMessage.EntityId.Should().Be(1);
            capturedMessage.Action.Should().Be((int)EnumAuditAction.Create);
            capturedMessage.UserId.Should().Be(42);
            capturedMessage.IpAddress.Should().Be("192.168.1.100");
            capturedMessage.UserAgent.Should().Be("Mozilla/5.0");
            capturedMessage.ObjectGuid.Should().Be(cidade.ObjectGuid);
        }

        [Fact(DisplayName = "Fluxo Completo - Deve detectar mudanÃ§as e gerar log de atualizaÃ§Ã£o")]
        public async Task FluxoCompleto_DeveDetectarMudancas_EGerarLogDeAtualizacao()
        {
            // Arrange
            var auditQueueProducerMock = new Mock<IAuditLogQueueProducer>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            
            var httpContext = new DefaultHttpContext();
            httpContext.Items["AuditIpAddress"] = "10.0.0.1";
            httpContext.Items["AuditUserAgent"] = "Chrome/120.0";
            
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            var auditHelper = new AuditHelper(auditQueueProducerMock.Object, httpContextAccessorMock.Object);
            
            var oldCidade = new Cidade
            {
                Id = 1,
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308"
            };

            var newCidade = new Cidade
            {
                Id = 1,
                Nome = "SÃ£o Paulo - Capital",
                CodigoIbge = "3550308",
                UsuarioAlteracao = 1,
                DataHoraAlteracao = DateTime.Now
            };

            AuditLogMessageEvent? capturedMessage = null;
            auditQueueProducerMock
                .Setup(x => x.EnqueueAuditLogAsync(It.IsAny<AuditLogMessageEvent>()))
                .Callback<AuditLogMessageEvent>(msg => capturedMessage = msg)
                .Returns(Task.CompletedTask);

            // Act
            await auditHelper.LogUpdateAsync(oldCidade, newCidade);

            // Assert
            capturedMessage.Should().NotBeNull();
            capturedMessage!.Action.Should().Be((int)EnumAuditAction.Update);
            
            var changes = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(capturedMessage.ChangesJson);
            changes.Should().NotBeNull();
            changes!.Should().ContainKey("Nome");
            changes["Nome"]["oldValue"].ToString().Should().Be("SÃ£o Paulo");
            changes["Nome"]["newValue"].ToString().Should().Be("SÃ£o Paulo - Capital");
        }

        [Fact(DisplayName = "Fluxo Completo - Deve gerar mensagem amigÃ¡vel para operaÃ§Ã£o de tag")]
        public async Task FluxoCompleto_DeveGerarMensagemAmigavel_ParaOperacaoDeTag()
        {
            // Arrange
            var auditQueueProducerMock = new Mock<IAuditLogQueueProducer>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            
            var httpContext = new DefaultHttpContext();
            httpContext.Items["AuditIpAddress"] = "192.168.1.1";
            httpContext.Items["AuditUserAgent"] = "TestAgent";
            
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            var auditHelper = new AuditHelper(auditQueueProducerMock.Object, httpContextAccessorMock.Object);
            
            var tag = new Tags { Id = 1, Nome = "Tag Teste" };
            var grupo = new GrupoImagemHome { Id = 1, Nome = "Grupo Teste" };
            var grupoTag = new GrupoImagemHomeTags
            {
                Id = 1,
                Tags = tag,
                GrupoImagemHome = grupo,
                UsuarioCriacao = 1,
                DataHoraCriacao = DateTime.Now,
                ObjectGuid = Guid.NewGuid().ToString()
            };

            AuditLogMessageEvent? capturedMessage = null;
            auditQueueProducerMock
                .Setup(x => x.EnqueueAuditLogAsync(It.IsAny<AuditLogMessageEvent>()))
                .Callback<AuditLogMessageEvent>(msg => capturedMessage = msg)
                .Returns(Task.CompletedTask);

            // Act
            await auditHelper.LogCreateAsync(grupoTag);

            // Assert
            capturedMessage.Should().NotBeNull();
            capturedMessage!.ChangesJson.Should().NotBeNullOrEmpty();
            
            var changes = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(capturedMessage.ChangesJson);
            changes.Should().NotBeNull();
            changes!.Should().ContainKey("_operation");
            changes["_operation"].Should().ContainKey("friendlyMessage");
            
            var friendlyMessage = changes["_operation"]["friendlyMessage"].ToString();
            friendlyMessage.Should().Contain("Vinculada a tag");
            friendlyMessage.Should().Contain("Tag Teste");
            friendlyMessage.Should().Contain("Grupo Teste");
        }

        [Fact(DisplayName = "Fluxo Completo - Middleware deve capturar contexto antes do AuditHelper")]
        public async Task FluxoCompleto_MiddlewareDeveCapturarContexto_AntesDoAuditHelper()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.50");
            context.Request.Headers["User-Agent"] = "IntegrationTest/1.0";
            
            var identity = new System.Security.Claims.ClaimsIdentity("TestAuth", "UserId", "Role");
            identity.AddClaim(new System.Security.Claims.Claim("UserId", "99"));
            context.User = new System.Security.Claims.ClaimsPrincipal(identity);
            
            RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
            var middleware = new AuditContextMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Items.Should().ContainKey("AuditIpAddress");
            context.Items.Should().ContainKey("AuditUserAgent");
            context.Items.Should().ContainKey("AuditUserId");
            
            context.Items["AuditIpAddress"].Should().Be("192.168.1.50");
            context.Items["AuditUserAgent"].Should().Be("IntegrationTest/1.0");
            context.Items["AuditUserId"].Should().Be("99");
        }

        [Fact(DisplayName = "Fluxo Completo - Deve processar mÃºltiplas operaÃ§Ãµes sem interferÃªncia")]
        public async Task FluxoCompleto_DeveProcessarMultiplasOperacoes_SemInterferencia()
        {
            // Arrange
            var auditQueueProducerMock = new Mock<IAuditLogQueueProducer>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            
            var httpContext = new DefaultHttpContext();
            httpContext.Items["AuditIpAddress"] = "192.168.1.1";
            httpContext.Items["AuditUserAgent"] = "TestAgent";
            
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            var auditHelper = new AuditHelper(auditQueueProducerMock.Object, httpContextAccessorMock.Object);
            
            var messages = new List<AuditLogMessageEvent>();
            auditQueueProducerMock
                .Setup(x => x.EnqueueAuditLogAsync(It.IsAny<AuditLogMessageEvent>()))
                .Callback<AuditLogMessageEvent>(msg => messages.Add(msg))
                .Returns(Task.CompletedTask);

            // Act - Criar mÃºltiplas entidades
            var cidade1 = new Cidade { Id = 1, Nome = "Cidade 1", UsuarioCriacao = 1, DataHoraCriacao = DateTime.Now };
            var cidade2 = new Cidade { Id = 2, Nome = "Cidade 2", UsuarioCriacao = 1, DataHoraCriacao = DateTime.Now };
            var cidade3 = new Cidade { Id = 3, Nome = "Cidade 3", UsuarioCriacao = 1, DataHoraCriacao = DateTime.Now };

            await Task.WhenAll(
                auditHelper.LogCreateAsync(cidade1),
                auditHelper.LogCreateAsync(cidade2),
                auditHelper.LogCreateAsync(cidade3)
            );

            // Assert
            messages.Should().HaveCount(3);
            messages.Should().OnlyContain(m => m.EntityType == "Cidade");
            messages.Should().OnlyContain(m => m.Action == (int)EnumAuditAction.Create);
            messages.Select(m => m.EntityId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }
    }
}

