using Microsoft.Extensions.Logging;
using Moq;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using Xunit;

namespace SW_PortalProprietario.Test.Services.Core
{
    /// <summary>
    /// Testes unitários completos para o SagaOrchestrator
    /// Garante comportamento de Two-Phase Commit (atomicidade distribuída)
    /// </summary>
    public class SagaOrchestratorTests
    {
        private readonly Mock<ILogger<SagaOrchestrator>> _loggerMock;
        private readonly SagaOrchestrator _orchestrator;

        public SagaOrchestratorTests()
        {
            _loggerMock = new Mock<ILogger<SagaOrchestrator>>();
            _orchestrator = new SagaOrchestrator(_loggerMock.Object);
        }

        #region Testes de Sucesso (Happy Path)

        [Fact]
        public async Task ExecuteAsync_TodosStepsSucesso_DeveRetornarSucesso()
        {
            // Arrange
            var step1 = CreateMockStep("Step1", 1, true);
            var step2 = CreateMockStep("Step2", 2, true);
            var step3 = CreateMockStep("Step3", 3, true);
            
            var steps = new List<IDistributedTransactionStep> { step1, step2, step3 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.True(success);
            Assert.Empty(errorMessage);
            
            // Verificar que todos foram executados
            Mock.Get(step1).Verify(s => s.ExecuteAsync(), Times.Once);
            Mock.Get(step2).Verify(s => s.ExecuteAsync(), Times.Once);
            Mock.Get(step3).Verify(s => s.ExecuteAsync(), Times.Once);
            
            // Verificar que nenhum foi compensado
            Mock.Get(step1).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Never);
            Mock.Get(step2).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Never);
            Mock.Get(step3).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_UmUnicoStep_DeveFuncionarCorretamente()
        {
            // Arrange
            var step1 = CreateMockStep("OnlyStep", 1, true);
            var steps = new List<IDistributedTransactionStep> { step1 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.True(success);
            Mock.Get(step1).Verify(s => s.ExecuteAsync(), Times.Once);
            Mock.Get(step1).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_MuitosSteps_DeveExecutarTodosNaOrdem()
        {
            // Arrange - 10 steps
            var steps = new List<IDistributedTransactionStep>();
            var executionOrder = new List<int>();

            for (int i = 1; i <= 10; i++)
            {
                var step = CreateMockStepWithCallback($"Step{i}", i, true, () => executionOrder.Add(i));
                steps.Add(step);
            }

            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.True(success);
            Assert.Equal(10, executionOrder.Count);
            Assert.Equal(Enumerable.Range(1, 10).ToList(), executionOrder);
        }

        #endregion

        #region Testes de Compensação (Rollback)

        [Fact]
        public async Task ExecuteAsync_FalhaNoSegundoStep_DeveCompensarPrimeiro()
        {
            // Arrange
            var step1 = CreateMockStep("Step1", 1, true);
            var step2 = CreateMockStep("Step2", 2, false, "Erro no step 2");
            var step3 = CreateMockStep("Step3", 3, true);
            
            var steps = new List<IDistributedTransactionStep> { step1, step2, step3 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            Assert.Contains("Erro no step 2", errorMessage);
            
            // Verificar que apenas step1 e step2 foram executados
            Mock.Get(step1).Verify(s => s.ExecuteAsync(), Times.Once);
            Mock.Get(step2).Verify(s => s.ExecuteAsync(), Times.Once);
            Mock.Get(step3).Verify(s => s.ExecuteAsync(), Times.Never); // Não deve executar
            
            // Verificar que apenas step1 foi compensado (step2 falhou, então não tem o que compensar)
            Mock.Get(step1).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Once);
            Mock.Get(step2).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_FalhaNoTerceiroStep_DeveCompensarEmOrdemReversa()
        {
            // Arrange
            var compensationOrder = new List<string>();
            
            var step1 = CreateMockStepWithCompensationCallback("Step1", 1, true, () => compensationOrder.Add("Step1"));
            var step2 = CreateMockStepWithCompensationCallback("Step2", 2, true, () => compensationOrder.Add("Step2"));
            var step3 = CreateMockStep("Step3", 3, false, "Erro no step 3");
            
            var steps = new List<IDistributedTransactionStep> { step1, step2, step3 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            Assert.Contains("Erro no step 3", errorMessage);
            
            // Verificar ordem de compensação (reversa: Step2, Step1)
            Assert.Equal(2, compensationOrder.Count);
            Assert.Equal("Step2", compensationOrder[0]);
            Assert.Equal("Step1", compensationOrder[1]);
        }

        [Fact]
        public async Task ExecuteAsync_FalhaNoUltimoStep_DeveCompensarTodosAnteriores()
        {
            // Arrange
            var compensationOrder = new List<string>();
            
            var step1 = CreateMockStepWithCompensationCallback("Step1", 1, true, () => compensationOrder.Add("Step1"));
            var step2 = CreateMockStepWithCompensationCallback("Step2", 2, true, () => compensationOrder.Add("Step2"));
            var step3 = CreateMockStepWithCompensationCallback("Step3", 3, true, () => compensationOrder.Add("Step3"));
            var step4 = CreateMockStep("Step4", 4, false, "Erro no step 4");
            
            var steps = new List<IDistributedTransactionStep> { step1, step2, step3, step4 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            
            // Todos os steps anteriores devem ser compensados em ordem reversa
            Assert.Equal(3, compensationOrder.Count);
            Assert.Equal("Step3", compensationOrder[0]);
            Assert.Equal("Step2", compensationOrder[1]);
            Assert.Equal("Step1", compensationOrder[2]);
        }

        [Fact]
        public async Task ExecuteAsync_CompensacaoFalha_DeveContinuarCompensandoOutros()
        {
            // Arrange
            var compensationOrder = new List<string>();
            
            var step1 = CreateMockStepWithCompensationCallback("Step1", 1, true, 
                () => compensationOrder.Add("Step1"));
            
            var step2Mock = new Mock<IDistributedTransactionStep>();
            step2Mock.Setup(s => s.StepName).Returns("Step2");
            step2Mock.Setup(s => s.Order).Returns(2);
            step2Mock.Setup(s => s.ExecuteAsync()).ReturnsAsync((true, string.Empty, new object()));
            step2Mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => compensationOrder.Add("Step2-FAILED"))
                .ReturnsAsync(false); // Compensação falha
            
            var step3 = CreateMockStep("Step3", 3, false, "Erro no step 3");
            
            var steps = new List<IDistributedTransactionStep> { step1, step2Mock.Object, step3 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            
            // Mesmo com falha na compensação do Step2, deve tentar compensar Step1
            Assert.Equal(2, compensationOrder.Count);
            Assert.Equal("Step2-FAILED", compensationOrder[0]);
            Assert.Equal("Step1", compensationOrder[1]);
        }

        #endregion

        #region Testes de Ordem de Execução

        [Fact]
        public async Task ExecuteAsync_StepsForaDeOrdem_DeveExecutarNaOrdemCorreta()
        {
            // Arrange
            var executionOrder = new List<string>();
            
            var step1 = CreateMockStepWithCallback("Step1", 3, true, () => executionOrder.Add("Step1")); // Order 3
            var step2 = CreateMockStepWithCallback("Step2", 1, true, () => executionOrder.Add("Step2")); // Order 1
            var step3 = CreateMockStepWithCallback("Step3", 2, true, () => executionOrder.Add("Step3")); // Order 2
            
            var steps = new List<IDistributedTransactionStep> { step1, step2, step3 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.Equal(3, executionOrder.Count);
            Assert.Equal("Step2", executionOrder[0]); // Order 1
            Assert.Equal("Step3", executionOrder[1]); // Order 2
            Assert.Equal("Step1", executionOrder[2]); // Order 3
        }

        [Fact]
        public async Task ExecuteAsync_StepsComMesmaOrdem_DeveExecutarEmOrdemDeAdicao()
        {
            // Arrange
            var executionOrder = new List<string>();
            
            var step1 = CreateMockStepWithCallback("StepA", 1, true, () => executionOrder.Add("StepA"));
            var step2 = CreateMockStepWithCallback("StepB", 1, true, () => executionOrder.Add("StepB"));
            var step3 = CreateMockStepWithCallback("StepC", 1, true, () => executionOrder.Add("StepC"));
            
            var steps = new List<IDistributedTransactionStep> { step1, step2, step3 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert - Ordem estável
            Assert.Equal(3, executionOrder.Count);
            Assert.Equal("StepA", executionOrder[0]);
            Assert.Equal("StepB", executionOrder[1]);
            Assert.Equal("StepC", executionOrder[2]);
        }

        #endregion

        #region Testes de Exceções

        [Fact]
        public async Task ExecuteAsync_StepLancaExcecao_DeveCompensarERetornarErro()
        {
            // Arrange
            var step1 = CreateMockStep("Step1", 1, true);
            
            var step2Mock = new Mock<IDistributedTransactionStep>();
            step2Mock.Setup(s => s.StepName).Returns("Step2");
            step2Mock.Setup(s => s.Order).Returns(2);
            step2Mock.Setup(s => s.ExecuteAsync()).ThrowsAsync(new InvalidOperationException("Erro crítico"));
            step2Mock.Setup(s => s.CompensateAsync(It.IsAny<object>())).ReturnsAsync(true);
            
            var steps = new List<IDistributedTransactionStep> { step1, step2Mock.Object };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            Assert.Contains("Erro crítico", errorMessage);
            
            // Step1 deve ser compensado
            Mock.Get(step1).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_CompensacaoLancaExcecao_DeveContinuarCompensando()
        {
            // Arrange
            var compensationOrder = new List<string>();
            
            var step1 = CreateMockStepWithCompensationCallback("Step1", 1, true, 
                () => compensationOrder.Add("Step1"));
            
            var step2Mock = new Mock<IDistributedTransactionStep>();
            step2Mock.Setup(s => s.StepName).Returns("Step2");
            step2Mock.Setup(s => s.Order).Returns(2);
            step2Mock.Setup(s => s.ExecuteAsync()).ReturnsAsync((true, string.Empty, new object()));
            step2Mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => compensationOrder.Add("Step2-EXCEPTION"))
                .ThrowsAsync(new Exception("Erro na compensação"));
            
            var step3 = CreateMockStep("Step3", 3, false, "Erro no step 3");
            
            var steps = new List<IDistributedTransactionStep> { step1, step2Mock.Object, step3 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            
            // Deve tentar compensar ambos, mesmo com exceção no Step2
            Assert.Equal(2, compensationOrder.Count);
            Assert.Contains("Step2-EXCEPTION", compensationOrder);
            Assert.Contains("Step1", compensationOrder);
        }

        #endregion

        #region Testes de Edge Cases

        [Fact]
        public async Task ExecuteAsync_ListaVazia_DeveRetornarSucesso()
        {
            // Arrange
            var steps = new List<IDistributedTransactionStep>();
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.True(success);
            Assert.Empty(errorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_StepRetornaNull_DeveSerTratado()
        {
            // Arrange
            var step1Mock = new Mock<IDistributedTransactionStep>();
            step1Mock.Setup(s => s.StepName).Returns("Step1");
            step1Mock.Setup(s => s.Order).Returns(1);
            step1Mock.Setup(s => s.ExecuteAsync()).ReturnsAsync((true, string.Empty, null)); // Data null
            step1Mock.Setup(s => s.CompensateAsync(null)).ReturnsAsync(true);
            
            var steps = new List<IDistributedTransactionStep> { step1Mock.Object };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task ExecuteAsync_FalhaNoprimeiroStep_NaoDeveCompensarNada()
        {
            // Arrange
            var step1 = CreateMockStep("Step1", 1, false, "Erro no primeiro step");
            var step2 = CreateMockStep("Step2", 2, true);
            
            var steps = new List<IDistributedTransactionStep> { step1, step2 };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            
            // Nenhum step deve ser compensado (falhou no primeiro)
            Mock.Get(step1).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Never);
            Mock.Get(step2).Verify(s => s.CompensateAsync(It.IsAny<object>()), Times.Never);
            
            // Step2 não deve ser executado
            Mock.Get(step2).Verify(s => s.ExecuteAsync(), Times.Never);
        }

        #endregion

        #region Testes de Atomicidade (Two-Phase Commit)

        [Fact]
        public async Task ExecuteAsync_SimulandoFalhaDeBanco_DeveGarantirAtomicidade()
        {
            // Simula cenário real: Oracle OK, PostgreSQL OK, API Falha
            // Deve compensar PostgreSQL e Oracle
            
            // Arrange
            var oracleExecuted = false;
            var oracleCompensated = false;
            var postgresExecuted = false;
            var postgresCompensated = false;
            
            var oracleStep = CreateMockStepWithCallbacks("Oracle", 1, true,
                onExecute: () => oracleExecuted = true,
                onCompensate: () => oracleCompensated = true);
            
            var postgresStep = CreateMockStepWithCallbacks("PostgreSQL", 2, true,
                onExecute: () => postgresExecuted = true,
                onCompensate: () => postgresCompensated = true);
            
            var apiStep = CreateMockStep("API", 3, false, "Timeout na API externa");
            
            var steps = new List<IDistributedTransactionStep> { oracleStep, postgresStep, apiStep };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert - Atomicidade garantida
            Assert.False(success);
            Assert.True(oracleExecuted, "Oracle deveria ter sido executado");
            Assert.True(postgresExecuted, "PostgreSQL deveria ter sido executado");
            Assert.True(oracleCompensated, "Oracle deveria ter sido compensado (rollback)");
            Assert.True(postgresCompensated, "PostgreSQL deveria ter sido compensado (rollback)");
        }

        [Fact]
        public async Task ExecuteAsync_TodosOsBancosComSucesso_NenhumDeveSerRevertido()
        {
            // Arrange
            var oracleExecuted = false;
            var oracleCompensated = false;
            var postgresExecuted = false;
            var postgresCompensated = false;
            var apiExecuted = false;
            var apiCompensated = false;
            
            var oracleStep = CreateMockStepWithCallbacks("Oracle", 1, true,
                onExecute: () => oracleExecuted = true,
                onCompensate: () => oracleCompensated = true);
            
            var postgresStep = CreateMockStepWithCallbacks("PostgreSQL", 2, true,
                onExecute: () => postgresExecuted = true,
                onCompensate: () => postgresCompensated = true);
            
            var apiStep = CreateMockStepWithCallbacks("API", 3, true,
                onExecute: () => apiExecuted = true,
                onCompensate: () => apiCompensated = true);
            
            var steps = new List<IDistributedTransactionStep> { oracleStep, postgresStep, apiStep };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.True(success);
            Assert.True(oracleExecuted);
            Assert.True(postgresExecuted);
            Assert.True(apiExecuted);
            
            // Nenhuma compensação deve ocorrer em caso de sucesso total
            Assert.False(oracleCompensated, "Oracle NÃO deveria ser compensado");
            Assert.False(postgresCompensated, "PostgreSQL NÃO deveria ser compensado");
            Assert.False(apiCompensated, "API NÃO deveria ser compensada");
        }

        [Fact]
        public async Task ExecuteAsync_CompensacaoParcialmenteFalhando_DeveRegistrarMasNaoInterromper()
        {
            // Cenário: Oracle compensa OK, PostgreSQL falha na compensação, mas não deve parar
            
            // Arrange
            var compensationAttempts = new List<string>();
            
            var oracleStep = CreateMockStepWithCompensationCallback("Oracle", 1, true,
                () => compensationAttempts.Add("Oracle-OK"));
            
            var postgresStep = CreateMockStepWithCompensationCallbackAndResult("PostgreSQL", 2, true,
                () => compensationAttempts.Add("PostgreSQL-FAIL"), compensationResult: false);
            
            var apiStep = CreateMockStep("API", 3, false, "Erro na API");
            
            var steps = new List<IDistributedTransactionStep> { oracleStep, postgresStep, apiStep };
            var operationId = Guid.NewGuid().ToString();

            // Act
            var (success, errorMessage) = await _orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            
            // Ambas as compensações devem ter sido tentadas
            Assert.Equal(2, compensationAttempts.Count);
            Assert.Contains("PostgreSQL-FAIL", compensationAttempts);
            Assert.Contains("Oracle-OK", compensationAttempts);
        }

        #endregion

        #region Helper Methods

        private IDistributedTransactionStep CreateMockStep(
            string stepName, 
            int order, 
            bool success, 
            string errorMessage = "")
        {
            var mock = new Mock<IDistributedTransactionStep>();
            
            mock.Setup(s => s.StepName).Returns(stepName);
            mock.Setup(s => s.Order).Returns(order);
            mock.Setup(s => s.ExecuteAsync())
                .ReturnsAsync((success, errorMessage, success ? new object() : null));
            mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .ReturnsAsync(true);
            
            return mock.Object;
        }

        private IDistributedTransactionStep CreateMockStepWithCallback(
            string stepName, 
            int order, 
            bool success,
            Action onExecute)
        {
            var mock = new Mock<IDistributedTransactionStep>();
            
            mock.Setup(s => s.StepName).Returns(stepName);
            mock.Setup(s => s.Order).Returns(order);
            mock.Setup(s => s.ExecuteAsync())
                .Callback(() => onExecute())
                .ReturnsAsync((success, string.Empty, success ? new object() : null));
            mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .ReturnsAsync(true);
            
            return mock.Object;
        }

        private IDistributedTransactionStep CreateMockStepWithCompensationCallback(
            string stepName, 
            int order, 
            bool success,
            Action onCompensate)
        {
            var mock = new Mock<IDistributedTransactionStep>();
            
            mock.Setup(s => s.StepName).Returns(stepName);
            mock.Setup(s => s.Order).Returns(order);
            mock.Setup(s => s.ExecuteAsync())
                .ReturnsAsync((success, string.Empty, success ? new object() : null));
            mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => onCompensate())
                .ReturnsAsync(true);
            
            return mock.Object;
        }

        private IDistributedTransactionStep CreateMockStepWithCompensationCallbackAndResult(
            string stepName, 
            int order, 
            bool success,
            Action onCompensate,
            bool compensationResult)
        {
            var mock = new Mock<IDistributedTransactionStep>();
            
            mock.Setup(s => s.StepName).Returns(stepName);
            mock.Setup(s => s.Order).Returns(order);
            mock.Setup(s => s.ExecuteAsync())
                .ReturnsAsync((success, string.Empty, success ? new object() : null));
            mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => onCompensate())
                .ReturnsAsync(compensationResult);
            
            return mock.Object;
        }

        private IDistributedTransactionStep CreateMockStepWithCallbacks(
            string stepName, 
            int order, 
            bool success,
            Action onExecute,
            Action onCompensate)
        {
            var mock = new Mock<IDistributedTransactionStep>();
            
            mock.Setup(s => s.StepName).Returns(stepName);
            mock.Setup(s => s.Order).Returns(order);
            mock.Setup(s => s.ExecuteAsync())
                .Callback(() => onExecute())
                .ReturnsAsync((success, string.Empty, success ? new object() : null));
            mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => onCompensate())
                .ReturnsAsync(true);
            
            return mock.Object;
        }

        #endregion
    }
}
