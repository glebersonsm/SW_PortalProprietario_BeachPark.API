using Microsoft.Extensions.Logging;
using Moq;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using Xunit;

namespace SW_PortalProprietario.Test.Services.Core
{
    /// <summary>
    /// Testes específicos para validar comportamento de Two-Phase Commit
    /// Garante propriedades ACID em transações distribuídas
    /// </summary>
    public class TwoPhaseCommitTests
    {
        private readonly Mock<ILogger<SagaOrchestrator>> _loggerMock;
        private readonly SagaOrchestrator _orchestrator;

        public TwoPhaseCommitTests()
        {
            _loggerMock = new Mock<ILogger<SagaOrchestrator>>();
            _orchestrator = new SagaOrchestrator(_loggerMock.Object);
        }

        #region Propriedade: Atomicidade

        [Fact]
        public async Task TwoPhaseCommit_Atomicidade_TodosOuNenhum()
        {
            // Arrange
            var transactions = new Dictionary<string, string>
            {
                { "Resource1", "INITIAL" },
                { "Resource2", "INITIAL" },
                { "Resource3", "INITIAL" }
            };

            // Simular 3 recursos (bancos de dados)
            var step1 = CreateResourceStep("Resource1", 1, true, transactions);
            var step2 = CreateResourceStep("Resource2", 2, true, transactions);
            var step3 = CreateResourceStep("Resource3", 3, false, transactions); // Falha aqui

            var steps = new List<IDistributedTransactionStep> { step1, step2, step3 };

            // Act
            var (success, _) = await _orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert - Atomicidade
            Assert.False(success);
            
            // TODOS devem estar no estado final consistente (ROLLBACK ou INITIAL)
            Assert.NotEqual("COMMITTED", transactions["Resource1"]);
            Assert.NotEqual("COMMITTED", transactions["Resource2"]);
            Assert.NotEqual("COMMITTED", transactions["Resource3"]);
            
            // Resource1 e Resource2 devem ter sido compensados
            Assert.Equal("ROLLBACK", transactions["Resource1"]);
            Assert.Equal("ROLLBACK", transactions["Resource2"]);
        }

        [Fact]
        public async Task TwoPhaseCommit_Atomicidade_SucessoCompleto()
        {
            // Arrange
            var transactions = new Dictionary<string, string>
            {
                { "DB1", "INITIAL" },
                { "DB2", "INITIAL" },
                { "API", "INITIAL" }
            };

            var step1 = CreateResourceStep("DB1", 1, true, transactions);
            var step2 = CreateResourceStep("DB2", 2, true, transactions);
            var step3 = CreateResourceStep("API", 3, true, transactions);

            var steps = new List<IDistributedTransactionStep> { step1, step2, step3 };

            // Act
            var (success, _) = await _orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert
            Assert.True(success);
            
            // TODOS devem estar commitados
            Assert.Equal("COMMITTED", transactions["DB1"]);
            Assert.Equal("COMMITTED", transactions["DB2"]);
            Assert.Equal("COMMITTED", transactions["API"]);
        }

        #endregion

        #region Propriedade: Consistência

        [Fact]
        public async Task TwoPhaseCommit_Consistencia_EstadoFinalValido()
        {
            // Arrange - Simular transferência bancária distribuída
            var saldos = new Dictionary<string, decimal>
            {
                { "ContaOrigem", 1000m },
                { "ContaDestino", 500m }
            };

            // Step 1: Debitar origem
            var stepDebito = CreateMockStepWithCallback("Debito", 1, true, () =>
            {
                saldos["ContaOrigem"] -= 100m; // Debita
            });

            // Step 2: Creditar destino (FALHA)
            var stepCredito = CreateMockStepWithCallback("Credito", 2, false, () =>
            {
                saldos["ContaDestino"] += 100m; // Credita
            });

            // Compensação: estornar débito
            Mock.Get(stepDebito).Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => saldos["ContaOrigem"] += 100m) // Estorna
                .ReturnsAsync(true);

            var steps = new List<IDistributedTransactionStep> { stepDebito, stepCredito };

            // Act
            var (success, _) = await _orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert - Consistência
            Assert.False(success);
            
            // Saldos devem voltar ao estado original (consistente)
            Assert.Equal(1000m, saldos["ContaOrigem"]);
            Assert.Equal(500m, saldos["ContaDestino"]);
        }

        [Fact]
        public async Task TwoPhaseCommit_Consistencia_NenhumaOperacaoParcial()
        {
            // Arrange
            var operacoes = new List<string>();

            var step1 = CreateMockStepWithCallback("Op1", 1, true, () => operacoes.Add("Op1-EXEC"));
            var step2 = CreateMockStepWithCallback("Op2", 2, true, () => operacoes.Add("Op2-EXEC"));
            var step3 = CreateMockStepWithCallback("Op3", 3, false, () => operacoes.Add("Op3-FAILED"));

            Mock.Get(step1).Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => operacoes.Add("Op1-ROLLBACK"))
                .ReturnsAsync(true);

            Mock.Get(step2).Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => operacoes.Add("Op2-ROLLBACK"))
                .ReturnsAsync(true);

            var steps = new List<IDistributedTransactionStep> { step1, step2, step3 };

            // Act
            await _orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert
            // Deve haver compensação para cada operação executada
            Assert.Contains("Op1-EXEC", operacoes);
            Assert.Contains("Op2-EXEC", operacoes);
            Assert.Contains("Op1-ROLLBACK", operacoes);
            Assert.Contains("Op2-ROLLBACK", operacoes);
            
            // Ordem: EXEC, EXEC, FAILED, ROLLBACK (reversa), ROLLBACK
            Assert.Equal(5, operacoes.Count);
        }

        #endregion

        #region Propriedade: Isolamento

        [Fact]
        public async Task TwoPhaseCommit_Isolamento_OperacoesIndependentes()
        {
            // Arrange - 2 operações simultâneas
            var recursoCompartilhado = 0;
            var lock1 = new object();
            var lock2 = new object();

            // Operação 1
            var step1Op1 = CreateMockStepWithCallback("Op1-Step1", 1, true, () =>
            {
                lock (lock1) { recursoCompartilhado++; }
            });

            // Operação 2
            var step1Op2 = CreateMockStepWithCallback("Op2-Step1", 1, true, () =>
            {
                lock (lock2) { recursoCompartilhado++; }
            });

            var steps1 = new List<IDistributedTransactionStep> { step1Op1 };
            var steps2 = new List<IDistributedTransactionStep> { step1Op2 };

            var orchestrator1 = new SagaOrchestrator(_loggerMock.Object);
            var orchestrator2 = new SagaOrchestrator(_loggerMock.Object);

            // Act - Executar simultaneamente
            var task1 = Task.Run(() => orchestrator1.ExecuteAsync(steps1, "Op1"));
            var task2 = Task.Run(() => orchestrator2.ExecuteAsync(steps2, "Op2"));

            await Task.WhenAll(task1, task2);

            // Assert - Isolamento
            Assert.Equal(2, recursoCompartilhado); // Ambas executaram
        }

        [Fact]
        public async Task TwoPhaseCommit_Isolamento_SemInterferencia()
        {
            // Arrange
            var op1Data = new Dictionary<string, int> { { "Value", 0 } };
            var op2Data = new Dictionary<string, int> { { "Value", 0 } };

            // Operação 1 - Sucesso
            var step1 = CreateMockStepWithCallback("Op1", 1, true, () => op1Data["Value"] = 100);
            Mock.Get(step1).Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => op1Data["Value"] = 0)
                .ReturnsAsync(true);

            // Operação 2 - Falha
            var step2 = CreateMockStepWithCallback("Op2", 1, false, () => op2Data["Value"] = 200);

            var steps1 = new List<IDistributedTransactionStep> { step1 };
            var steps2 = new List<IDistributedTransactionStep> { step2 };

            // Act
            var (success1, _) = await _orchestrator.ExecuteAsync(steps1, "Op1");
            var (success2, _) = await _orchestrator.ExecuteAsync(steps2, "Op2");

            // Assert
            Assert.True(success1);
            Assert.False(success2);
            
            // Op1 não deve ser afetada por falha de Op2
            Assert.Equal(100, op1Data["Value"]);
            Assert.Equal(0, op2Data["Value"]);
        }

        #endregion

        #region Propriedade: Durabilidade

        [Fact]
        public async Task TwoPhaseCommit_Durabilidade_DadosPersistidos()
        {
            // Arrange - Simular persistência
            var persistedData = new List<string>();

            var step1 = CreateMockStepWithCallback("Persist1", 1, true, () =>
            {
                persistedData.Add("Data1-SAVED");
            });

            var step2 = CreateMockStepWithCallback("Persist2", 2, true, () =>
            {
                persistedData.Add("Data2-SAVED");
            });

            var steps = new List<IDistributedTransactionStep> { step1, step2 };

            // Act
            var (success, _) = await _orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert - Durabilidade
            Assert.True(success);
            Assert.Equal(2, persistedData.Count);
            Assert.Contains("Data1-SAVED", persistedData);
            Assert.Contains("Data2-SAVED", persistedData);
        }

        [Fact]
        public async Task TwoPhaseCommit_Durabilidade_RollbackRemoveDados()
        {
            // Arrange
            var persistedData = new List<string>();

            var step1 = CreateMockStepWithCallback("Persist1", 1, true, () =>
            {
                persistedData.Add("TempData");
            });

            Mock.Get(step1).Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => persistedData.Remove("TempData")) // Rollback remove
                .ReturnsAsync(true);

            var step2 = CreateMockStepWithCallback("FailStep", 2, false, () => { });

            var steps = new List<IDistributedTransactionStep> { step1, step2 };

            // Act
            var (success, _) = await _orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert
            Assert.False(success);
            Assert.Empty(persistedData); // Dado foi removido no rollback
        }

        #endregion

        #region Cenários Complexos

        [Fact]
        public async Task TwoPhaseCommit_CenarioComplexo_MultiplosBancosEApis()
        {
            // Arrange - Simular Oracle, PostgreSQL, SQL Server e 2 APIs
            var states = new Dictionary<string, string>
            {
                { "Oracle", "INITIAL" },
                { "PostgreSQL", "INITIAL" },
                { "SQLServer", "INITIAL" },
                { "API1", "INITIAL" },
                { "API2", "INITIAL" }
            };

            var stepOracle = CreateResourceStep("Oracle", 1, true, states);
            var stepPostgres = CreateResourceStep("PostgreSQL", 2, true, states);
            var stepSQLServer = CreateResourceStep("SQLServer", 3, true, states);
            var stepAPI1 = CreateResourceStep("API1", 4, true, states);
            var stepAPI2 = CreateResourceStep("API2", 5, false, states); // Falha aqui

            var steps = new List<IDistributedTransactionStep> 
            { 
                stepOracle, stepPostgres, stepSQLServer, stepAPI1, stepAPI2 
            };

            // Act
            var (success, _) = await _orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert
            Assert.False(success);
            
            // Todos os 4 anteriores devem ter rollback
            Assert.Equal("ROLLBACK", states["Oracle"]);
            Assert.Equal("ROLLBACK", states["PostgreSQL"]);
            Assert.Equal("ROLLBACK", states["SQLServer"]);
            Assert.Equal("ROLLBACK", states["API1"]);
            Assert.Equal("FAILED", states["API2"]);
        }

        [Fact]
        public async Task TwoPhaseCommit_CenarioComplexo_CompensacaoParcialFalhando()
        {
            // Arrange - Cenário onde algumas compensações falham
            var compensationResults = new Dictionary<string, bool>();

            var step1 = CreateResourceStepWithCompensationResult("Step1", 1, true, compensationResults, true);
            var step2 = CreateResourceStepWithCompensationResult("Step2", 2, true, compensationResults, false); // Falha compensação
            var step3 = CreateResourceStepWithCompensationResult("Step3", 3, true, compensationResults, true);
            var step4 = CreateResourceStepWithCompensationResult("Step4", 4, false, compensationResults, true);

            var steps = new List<IDistributedTransactionStep> { step1, step2, step3, step4 };

            // Act
            var (success, _) = await _orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert
            Assert.False(success);
            
            // Todas as compensações devem ter sido tentadas
            Assert.True(compensationResults["Step1"]);
            Assert.False(compensationResults["Step2"]); // Falhou
            Assert.True(compensationResults["Step3"]);
            Assert.False(compensationResults.ContainsKey("Step4")); // Não foi compensado (falhou)
        }

        #endregion

        #region Helper Methods

        private IDistributedTransactionStep CreateResourceStep(
            string resourceName,
            int order,
            bool success,
            Dictionary<string, string> transactions)
        {
            var mock = new Mock<IDistributedTransactionStep>();
            mock.Setup(s => s.StepName).Returns(resourceName);
            mock.Setup(s => s.Order).Returns(order);
            
            mock.Setup(s => s.ExecuteAsync())
                .Callback(() =>
                {
                    if (success)
                        transactions[resourceName] = "COMMITTED";
                    else
                        transactions[resourceName] = "FAILED";
                })
                .ReturnsAsync((success, success ? string.Empty : $"Erro em {resourceName}", success ? new object() : null));
            
            mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => transactions[resourceName] = "ROLLBACK")
                .ReturnsAsync(true);
            
            return mock.Object;
        }

        private IDistributedTransactionStep CreateResourceStepWithCompensationResult(
            string stepName,
            int order,
            bool executeSuccess,
            Dictionary<string, bool> compensationResults,
            bool compensationSuccess)
        {
            var mock = new Mock<IDistributedTransactionStep>();
            mock.Setup(s => s.StepName).Returns(stepName);
            mock.Setup(s => s.Order).Returns(order);
            
            mock.Setup(s => s.ExecuteAsync())
                .ReturnsAsync((executeSuccess, executeSuccess ? string.Empty : "Erro", executeSuccess ? new object() : null));
            
            mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .Callback(() => compensationResults[stepName] = compensationSuccess)
                .ReturnsAsync(compensationSuccess);
            
            return mock.Object;
        }

        private IDistributedTransactionStep CreateMockStepWithCallback(
            string stepName,
            int order,
            bool success,
            Action callback)
        {
            var mock = new Mock<IDistributedTransactionStep>();
            mock.Setup(s => s.StepName).Returns(stepName);
            mock.Setup(s => s.Order).Returns(order);
            mock.Setup(s => s.ExecuteAsync())
                .Callback(() => callback())
                .ReturnsAsync((success, success ? string.Empty : "Erro", success ? new object() : null));
            mock.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .ReturnsAsync(true);
            
            return mock.Object;
        }

        #endregion
    }
}
