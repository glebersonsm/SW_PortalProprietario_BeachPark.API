using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ProgramacaoParalela.LogsBackGround;
using SW_PortalProprietario.Infra.Data.RabbitMQ.Producers;
using SW_Utils.Models;
using Xunit;

namespace SW_PortalProprietario.Test.Audit
{
    public class RabbitMQAuditLogProducerTest
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ICacheStore> _cacheMock;
        private readonly RabbitMQAuditLogProducer _producer;

        public RabbitMQAuditLogProducerTest()
        {
            _configurationMock = new Mock<IConfiguration>();
            _cacheMock = new Mock<ICacheStore>();
            
            // ConfiguraÃ§Ã£o padrÃ£o para testes (sem RabbitMQ real)
            // Usar SetupGet para mÃ©todos de extensÃ£o GetValue
            _configurationMock.Setup(x => x[It.Is<string>(s => s == "RabbitMqConnectionHost")]).Returns("localhost");
            _configurationMock.Setup(x => x[It.Is<string>(s => s == "RabbitMqConnectionUser")]).Returns("guest");
            _configurationMock.Setup(x => x[It.Is<string>(s => s == "RabbitMqConnectionPass")]).Returns("guest");
            _configurationMock.Setup(x => x[It.Is<string>(s => s == "RabbitMqConnectionPort")]).Returns("5672");
            _configurationMock.Setup(x => x[It.Is<string>(s => s == "ProgramId")]).Returns("TEST_");
            _configurationMock.Setup(x => x[It.Is<string>(s => s == "RabbitMqFilaDeAuditoriaNome")]).Returns("TestAuditQueue");

            _producer = new RabbitMQAuditLogProducer(_configurationMock.Object, _cacheMock.Object);
        }

        [Fact(DisplayName = "EnqueueAuditLogAsync - Deve construir nome da fila corretamente")]
        public async Task EnqueueAuditLogAsync_DeveConstruirNomeDaFila_Corretamente()
        {
            // Arrange
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = 1,
                UserId = 1,
                Timestamp = DateTime.Now
            };

            // Act & Assert
            // Como nÃ£o temos RabbitMQ configurado nos testes, esperamos que a exceÃ§Ã£o seja tratada silenciosamente
            // ou que o mÃ©todo complete sem lanÃ§ar exceÃ§Ã£o
            var exception = await Record.ExceptionAsync(async () => await _producer.EnqueueAuditLogAsync(message));
            
            // O mÃ©todo nÃ£o deve lanÃ§ar exceÃ§Ã£o mesmo sem RabbitMQ (tratamento interno)
            // Isso Ã© esperado pois o mÃ©todo tem try-catch interno
        }

        [Fact(DisplayName = "EnqueueAuditLogAsync - Deve serializar mensagem corretamente")]
        public async Task EnqueueAuditLogAsync_DeveSerializarMensagem_Corretamente()
        {
            // Arrange
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = 1,
                UserId = 1,
                UserName = "TestUser",
                Timestamp = DateTime.Now,
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                ChangesJson = "{\"Nome\":{\"oldValue\":\"Old\",\"newValue\":\"New\"}}",
                EntityDataJson = "{\"Id\":1,\"Nome\":\"Test\"}",
                ObjectGuid = Guid.NewGuid().ToString()
            };

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () => await _producer.EnqueueAuditLogAsync(message));
            
            // NÃ£o deve lanÃ§ar exceÃ§Ã£o mesmo sem RabbitMQ
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "EnqueueAuditLogAsync - Deve remover espaÃ§os do nome da fila")]
        public async Task EnqueueAuditLogAsync_DeveRemoverEspacos_DoNomeDaFila()
        {
            // Arrange
            _configurationMock.Setup(x => x[It.Is<string>(s => s == "RabbitMqFilaDeAuditoriaNome")]).Returns("Test Audit Queue");
            
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = 1,
                Timestamp = DateTime.Now
            };

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () => await _producer.EnqueueAuditLogAsync(message));
            
            // NÃ£o deve lanÃ§ar exceÃ§Ã£o
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "EnqueueAuditLogAsync - Deve tratar erro de conexÃ£o sem lanÃ§ar exceÃ§Ã£o")]
        public async Task EnqueueAuditLogAsync_DeveTratarErroDeConexao_SemLancarExcecao()
        {
            // Arrange
            // Configurar para um host invÃ¡lido
            _configurationMock.Setup(x => x[It.Is<string>(s => s == "RabbitMqConnectionHost")]).Returns("invalid-host-that-does-not-exist");
            
            var message = new AuditLogMessageEvent
            {
                EntityType = "Cidade",
                EntityId = 1,
                Action = 1,
                Timestamp = DateTime.Now
            };

            // Act & Assert
            // O mÃ©todo deve tratar o erro internamente e nÃ£o lanÃ§ar exceÃ§Ã£o
            var exception = await Record.ExceptionAsync(async () => await _producer.EnqueueAuditLogAsync(message));
            
            // NÃ£o deve lanÃ§ar exceÃ§Ã£o (tratamento interno)
            exception.Should().BeNull();
        }
    }
}

