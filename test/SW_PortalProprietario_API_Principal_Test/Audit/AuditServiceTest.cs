using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Models;
using Xunit;

namespace SW_PortalProprietario.Test.Audit
{
    public class AuditServiceTest
    {
        private readonly Mock<IRepositoryNH> _repositoryMock;
        private readonly Mock<ILogger<AuditService>> _loggerMock;
        private readonly Mock<ISwSessionFactoryDefault> _sessionFactoryMock;
        private readonly AuditService _auditService;

        public AuditServiceTest()
        {
            _repositoryMock = new Mock<IRepositoryNH>();
            _loggerMock = new Mock<ILogger<AuditService>>();
            _sessionFactoryMock = new Mock<ISwSessionFactoryDefault>();
            
            _auditService = new AuditService(
                _repositoryMock.Object,
                _loggerMock.Object,
                _sessionFactoryMock.Object);
        }

        [Fact(DisplayName = "SaveAuditLogAsync - Deve criar sessÃ£o isolada para persistÃªncia")]
        public async Task SaveAuditLogAsync_DeveCriarSessaoIsolada_ParaPersistencia()
        {
            // Arrange
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = (int)EnumAuditAction.Create,
                UserId = 1,
                UserName = "TestUser",
                Timestamp = DateTime.Now,
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                ChangesJson = "{}",
                EntityDataJson = "{\"Id\":1}",
                ObjectGuid = Guid.NewGuid().ToString()
            };

            var statelessSessionMock = new Mock<IStatelessSession>();
            var transactionMock = new Mock<ITransaction>();

            _sessionFactoryMock
                .Setup(x => x.OpenStatelessSession())
                .Returns(statelessSessionMock.Object);

            statelessSessionMock
                .Setup(x => x.BeginTransaction())
                .Returns(transactionMock.Object);

            transactionMock.Setup(x => x.IsActive).Returns(true);

            statelessSessionMock
                .Setup(x => x.InsertAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((object)null!);

            // Act
            await _auditService.SaveAuditLogAsync(message);

            // Assert
            _sessionFactoryMock.Verify(x => x.OpenStatelessSession(), Times.Once);
            statelessSessionMock.Verify(x => x.BeginTransaction(), Times.Once);
            statelessSessionMock.Verify(x => x.InsertAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "SaveAuditLogAsync - Deve fazer rollback em caso de erro")]
        public async Task SaveAuditLogAsync_DeveFazerRollback_EmCasoDeErro()
        {
            // Arrange
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = (int)EnumAuditAction.Create,
                Timestamp = DateTime.Now
            };

            var statelessSessionMock = new Mock<IStatelessSession>();
            var transactionMock = new Mock<ITransaction>();

            _sessionFactoryMock
                .Setup(x => x.OpenStatelessSession())
                .Returns(statelessSessionMock.Object);

            statelessSessionMock
                .Setup(x => x.BeginTransaction())
                .Returns(transactionMock.Object);

            transactionMock.Setup(x => x.IsActive).Returns(true);

            statelessSessionMock
                .Setup(x => x.InsertAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _auditService.SaveAuditLogAsync(message);

            // Assert
            transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }

        [Fact(DisplayName = "SaveAuditLogAsync - Deve liberar recursos mesmo em caso de erro")]
        public async Task SaveAuditLogAsync_DeveLiberarRecursos_MesmoEmCasoDeErro()
        {
            // Arrange
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = (int)EnumAuditAction.Create,
                Timestamp = DateTime.Now
            };

            var statelessSessionMock = new Mock<IStatelessSession>();
            var transactionMock = new Mock<ITransaction>();

            _sessionFactoryMock
                .Setup(x => x.OpenStatelessSession())
                .Returns(statelessSessionMock.Object);

            statelessSessionMock
                .Setup(x => x.BeginTransaction())
                .Returns(transactionMock.Object);

            transactionMock.Setup(x => x.IsActive).Returns(true);

            statelessSessionMock
                .Setup(x => x.InsertAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _auditService.SaveAuditLogAsync(message);

            // Assert
            transactionMock.Verify(x => x.Dispose(), Times.Once);
            statelessSessionMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact(DisplayName = "SaveAuditLogAsync - Deve mapear corretamente os dados da mensagem")]
        public async Task SaveAuditLogAsync_DeveMapearCorretamente_OsDadosDaMensagem()
        {
            // Arrange
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = (int)EnumAuditAction.Update,
                UserId = 123,
                UserName = "John Doe",
                Timestamp = DateTime.Now.AddHours(-1),
                IpAddress = "10.0.0.1",
                UserAgent = "Chrome/120.0",
                ChangesJson = "{\"Nome\":{\"oldValue\":\"Old\",\"newValue\":\"New\"}}",
                EntityDataJson = "{\"Id\":1,\"Nome\":\"New\"}",
                ObjectGuid = "test-guid-123"
            };

            var statelessSessionMock = new Mock<IStatelessSession>();
            var transactionMock = new Mock<ITransaction>();

            _sessionFactoryMock
                .Setup(x => x.OpenStatelessSession())
                .Returns(statelessSessionMock.Object);

            statelessSessionMock
                .Setup(x => x.BeginTransaction())
                .Returns(transactionMock.Object);

            transactionMock.Setup(x => x.IsActive).Returns(true);

            Domain.Entities.Core.Auditoria.AuditLog? capturedAuditLog = null;
            statelessSessionMock
                .Setup(x => x.InsertAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((obj, ct) => 
                {
                    capturedAuditLog = obj as Domain.Entities.Core.Auditoria.AuditLog;
                })
                .ReturnsAsync((object)null!);

            // Act
            await _auditService.SaveAuditLogAsync(message);

            // Assert
            capturedAuditLog.Should().NotBeNull();
            capturedAuditLog!.EntityType.Should().Be("Cidade");
            capturedAuditLog.EntityId.Should().Be(1);
            capturedAuditLog.Action.Should().Be(EnumAuditAction.Update);
            capturedAuditLog.UserId.Should().Be(123);
            capturedAuditLog.UserName.Should().Be("John Doe");
            capturedAuditLog.IpAddress.Should().Be("10.0.0.1");
            capturedAuditLog.UserAgent.Should().Be("Chrome/120.0");
            capturedAuditLog.ChangesJson.Should().Be("{\"Nome\":{\"oldValue\":\"Old\",\"newValue\":\"New\"}}");
            capturedAuditLog.EntityDataJson.Should().Be("{\"Id\":1,\"Nome\":\"New\"}");
            capturedAuditLog.ObjectGuid.Should().Be("test-guid-123");
            capturedAuditLog.UsuarioCriacao.Should().Be(123);
            capturedAuditLog.DataHoraCriacao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact(DisplayName = "SaveAuditLogAsync - NÃ£o deve lanÃ§ar exceÃ§Ã£o mesmo em caso de erro")]
        public async Task SaveAuditLogAsync_NaoDeveLancarExcecao_MesmoEmCasoDeErro()
        {
            // Arrange
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = (int)EnumAuditAction.Create,
                Timestamp = DateTime.Now
            };

            _sessionFactoryMock
                .Setup(x => x.OpenStatelessSession())
                .Throws(new Exception("Session factory error"));

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () => await _auditService.SaveAuditLogAsync(message));
            
            // NÃ£o deve lanÃ§ar exceÃ§Ã£o (tratamento interno)
            exception.Should().BeNull();
        }
    }
}

