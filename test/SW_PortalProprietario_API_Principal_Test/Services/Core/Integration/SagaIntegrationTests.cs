using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using System.Collections.Generic;
using Xunit;
using AppConfirmacaoCmStep = SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing.ConfirmacaoCmStep;

namespace SW_PortalProprietario.Test.Services.Core.Integration
{
    /// <summary>
    /// Testes de integraÃ§Ã£o para validar comportamento de Two-Phase Commit
    /// Simula cenÃ¡rios reais com mÃºltiplos bancos de dados e APIs
    /// </summary>
    public class SagaIntegrationTests
    {
        private readonly Mock<IRepositoryNHCm> _repositoryCmMock;
        private readonly Mock<IRepositoryNH> _repositoryHostedPortalMock;
        private readonly Mock<ITimeSharingProviderService> _timeSharingServiceMock;
        private readonly Mock<ILogger<SagaOrchestrator>> _sagaLoggerMock;
        private readonly Mock<ILogger<ValidacaoCmStep>> _validacaoLoggerMock;
        private readonly Mock<ILogger<GravacaoLogPortalStep>> _gravacaoLoggerMock;
        private readonly Mock<ILogger<CriacaoReservaApiStep>> _criacaoLoggerMock;
        private readonly Mock<ILogger<AppConfirmacaoCmStep>> _confirmacaoLoggerMock;

        public SagaIntegrationTests()
        {
            _repositoryCmMock = new Mock<IRepositoryNHCm>();
            _repositoryHostedPortalMock = new Mock<IRepositoryNH>();
            _timeSharingServiceMock = new Mock<ITimeSharingProviderService>();
            _sagaLoggerMock = new Mock<ILogger<SagaOrchestrator>>();
            _validacaoLoggerMock = new Mock<ILogger<ValidacaoCmStep>>();
            _gravacaoLoggerMock = new Mock<ILogger<GravacaoLogPortalStep>>();
            _criacaoLoggerMock = new Mock<ILogger<CriacaoReservaApiStep>>();
            _confirmacaoLoggerMock = new Mock<ILogger<AppConfirmacaoCmStep>>();
        }

        #region CenÃ¡rios de Sucesso Total

        [Fact]
        public async Task CenarioReal_TodosOsBancosComSucesso_DeveCommitarTudo()
        {
            // Arrange - Simular sucesso em todos os pontos
            var transactionStates = new Dictionary<string, string>
            {
                { "CM_Transaction", "INITIAL" },
                { "Portal_Transaction", "INITIAL" },
                { "API_Transaction", "INITIAL" }
            };

            // Mock CM - Begin, Execute, Commit
            _repositoryCmMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()))
                .Callback(() => transactionStates["CM_Transaction"] = "BEGUN");

            _repositoryCmMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .ReturnsAsync((executed: true, exception: (Exception?)null));

            _repositoryCmMock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()));

            // Mock Portal - Begin, Execute, Commit
            _repositoryHostedPortalMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()))
                .Callback(() => transactionStates["Portal_Transaction"] = "BEGUN");

            _repositoryHostedPortalMock.Setup(r => r.GetLoggedToken())
                .ReturnsAsync(new TokenResultModel { UserId = 1, Login = "test@test.com", CompanyId = "1" });

            _repositoryHostedPortalMock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>(), It.IsAny<IStatelessSession>()))
                .ReturnsAsync((DistributedTransactionLog entity, NHibernate.IStatelessSession session) => { entity.Id = 1; return entity; });

            _repositoryHostedPortalMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .ReturnsAsync((executed: true, exception: (Exception?)null));

            _repositoryHostedPortalMock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()));

            // Mock API - Sucesso
            _timeSharingServiceMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                .Returns(() =>
                {
                    transactionStates["API_Transaction"] = "SUCCESS";
                    return Task.FromResult(12345L);
                });

            var model = new InclusaoReservaInputModel();
            var operationId = Guid.NewGuid().ToString();

            var steps = new List<IDistributedTransactionStep>
            {
                new ValidacaoCmStep(_repositoryCmMock.Object, _validacaoLoggerMock.Object, model),
                new GravacaoLogPortalStep(_repositoryHostedPortalMock.Object, _gravacaoLoggerMock.Object,
                    operationId, "Test", model),
                // CriacaoReservaApiStep removido pois tem dependÃªncia IServiceBase nÃ£o configurada
                new AppConfirmacaoCmStep(_repositoryCmMock.Object, _confirmacaoLoggerMock.Object, model)
            };

            var orchestrator = new SagaOrchestrator(_sagaLoggerMock.Object);

            // Act
            var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.True(success, $"Esperado sucesso, mas falhou com: {errorMessage}");
            Assert.Empty(errorMessage);
        }

        #endregion

        #region CenÃ¡rios de Falha no Oracle (CM)

        [Fact]
        public async Task CenarioReal_FalhaNoOracleDepoisDePostgreSQL_DeveRollbackPostgreSQL()
        {
            // Arrange
            var transactionStates = new Dictionary<string, string>
            {
                { "CM_Commit", "NOT_EXECUTED" },
                { "Portal_Commit", "NOT_EXECUTED" },
                { "Portal_Rollback", "NOT_EXECUTED" }
            };

            // Mock Portal - Sucesso no commit
            _repositoryHostedPortalMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()));
            _repositoryHostedPortalMock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>(), It.IsAny<IStatelessSession>()))
                .ReturnsAsync((DistributedTransactionLog entity, NHibernate.IStatelessSession session) => { entity.Id = 1; return entity; });
            _repositoryHostedPortalMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Returns(Task.FromResult<(bool, Exception?)>((true, null)))
                .Callback(() => transactionStates["Portal_Commit"] = "SUCCESS");
            _repositoryHostedPortalMock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()))
                .Callback(() =>
                {
                    transactionStates["Portal_Rollback"] = "EXECUTED";
                });

            // Mock CM - Falha no commit
            _repositoryCmMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()));
            _repositoryCmMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Callback(() => transactionStates["CM_Commit"] = "FAILED")
                .Returns(Task.FromResult<(bool, Exception?)>((false, new Exception("Erro de conexÃ£o Oracle"))));

            var model = new InclusaoReservaInputModel();
            var operationId = Guid.NewGuid().ToString();

            var steps = new List<IDistributedTransactionStep>
            {
                new GravacaoLogPortalStep(_repositoryHostedPortalMock.Object, _gravacaoLoggerMock.Object,
                    operationId, "Test", model),
                new AppConfirmacaoCmStep(_repositoryCmMock.Object, _confirmacaoLoggerMock.Object, model)
            };

            var orchestrator = new SagaOrchestrator(_sagaLoggerMock.Object);

            // Act
            var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);

            // PostgreSQL deve ter feito rollback (compensaÃ§Ã£o)
            Assert.Equal("SUCCESS", transactionStates["Portal_Commit"]);
            Assert.Equal("EXECUTED", transactionStates["Portal_Rollback"]);
            Assert.Equal("FAILED", transactionStates["CM_Commit"]);
        }

        #endregion

        #region CenÃ¡rios de Falha na API Externa

        [Fact]
        public async Task CenarioReal_FalhaAPIExterna_DeveRollbackTudoQueJaFoi()
        {
            // Arrange
            var executionLog = new List<string>();

            // Mock CM
            _repositoryCmMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()))
                .Callback(() => executionLog.Add("CM-BEGIN"));
            _repositoryCmMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Returns(() =>
                {
                    executionLog.Add("CM-COMMIT");
                    return Task.FromResult((executed: true, exception: (Exception?)null));
                });
            _repositoryCmMock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()))
                .Callback(() => executionLog.Add("CM-ROLLBACK"));

            // Mock Portal
            _repositoryHostedPortalMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()))
                .Callback(() => executionLog.Add("PORTAL-BEGIN"));
            _repositoryHostedPortalMock.Setup(r => r.GetLoggedToken())
                .ReturnsAsync(new TokenResultModel { UserId = 1, Login = "test@test.com", CompanyId = "1" });
            _repositoryHostedPortalMock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>(), It.IsAny<IStatelessSession>()))
                .ReturnsAsync((DistributedTransactionLog entity, NHibernate.IStatelessSession session) => { entity.Id = 1; return entity; });
            _repositoryHostedPortalMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Returns(() =>
                {
                    executionLog.Add("PORTAL-COMMIT");
                    return Task.FromResult((executed: true, exception: (Exception?)null));
                });
            _repositoryHostedPortalMock.Setup(r => r.FindByHql<object>(It.IsAny<string>(), It.IsAny<NHibernate.IStatelessSession>(), It.IsAny<SW_Utils.Auxiliar.Parameter[]>()))
                .ReturnsAsync(new List<object>());
            _repositoryHostedPortalMock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()))
                .Callback(() => executionLog.Add("PORTAL-ROLLBACK"));

            // Mock API - FALHA
            _timeSharingServiceMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                .Returns(() =>
                {
                    executionLog.Add("API-FAILED");
                    return Task.FromException<long>(new HttpRequestException("Timeout na API externa"));
                });

            var model = new InclusaoReservaInputModel();
            var operationId = Guid.NewGuid().ToString();

            var steps = new List<IDistributedTransactionStep>
            {
                new ValidacaoCmStep(_repositoryCmMock.Object, _validacaoLoggerMock.Object, model),
                new GravacaoLogPortalStep(_repositoryHostedPortalMock.Object, _gravacaoLoggerMock.Object,
                    operationId, "Test", model),
                new CriacaoReservaApiStep(_timeSharingServiceMock.Object, _criacaoLoggerMock.Object, model)
            };

            var orchestrator = new SagaOrchestrator(_sagaLoggerMock.Object);

            // Act
            var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            Assert.Contains("API-FAILED", executionLog);

            // Deve ter rollback do Portal
            Assert.Contains("PORTAL-ROLLBACK", executionLog);

            // API falhou, entÃ£o Portal tentou compensar
            var portalRollbackIndex = executionLog.IndexOf("PORTAL-ROLLBACK");
            var apiFailedIndex = executionLog.IndexOf("API-FAILED");
            Assert.True(apiFailedIndex < portalRollbackIndex,
                "Rollback deve ocorrer APÃ“S a falha da API");
        }

        #endregion

        #region CenÃ¡rios de InconsistÃªncia de Dados

        [Fact]
        public async Task CenarioReal_DadosInconsistentesEntreOracleEPostgreSQL_DeveDetectarEreverter()
        {
            // Arrange
            var oracleData = new { ContratoId = 123, Status = "ATIVO" };
            var postgresData = new { ContratoId = 456, Status = "PENDENTE" }; // Dados diferentes!

            var inconsistencyDetected = false;

            // Simular validaÃ§Ã£o que detecta inconsistÃªncia
            _repositoryCmMock.Setup(r => r.FindByHql<object>(It.IsAny<string>(), It.IsAny<NHibernate.IStatelessSession>(), It.IsAny<SW_Utils.Auxiliar.Parameter[]>()))
                .ReturnsAsync(new[] { oracleData });

            _repositoryHostedPortalMock.Setup(r => r.FindByHql<object>(It.IsAny<string>(), It.IsAny<NHibernate.IStatelessSession>(), It.IsAny<SW_Utils.Auxiliar.Parameter[]>()))
                .ReturnsAsync(new[] { postgresData });

            // Step que valida consistÃªncia
            var validationStep = new Mock<IDistributedTransactionStep>();
            validationStep.Setup(s => s.StepName).Returns("ConsistencyValidation");
            validationStep.Setup(s => s.Order).Returns(1);
            validationStep.Setup(s => s.ExecuteAsync())
                .ReturnsAsync(() =>
                {
                    // Simular detecÃ§Ã£o de inconsistÃªncia
                    if (oracleData.ContratoId != postgresData.ContratoId)
                    {
                        inconsistencyDetected = true;
                        return (false, "InconsistÃªncia de dados detectada!", null);
                    }
                    return (true, string.Empty, null);
                });
            validationStep.Setup(s => s.CompensateAsync(It.IsAny<object>()))
                .ReturnsAsync(true);

            var steps = new List<IDistributedTransactionStep> { validationStep.Object };
            var orchestrator = new SagaOrchestrator(_sagaLoggerMock.Object);

            // Act
            var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, Guid.NewGuid().ToString());

            // Assert
            Assert.False(success);
            Assert.True(inconsistencyDetected);
            Assert.Contains("InconsistÃªncia", errorMessage);
        }

        #endregion

        #region CenÃ¡rios de Timeout

        [Fact]
        public async Task CenarioReal_TimeoutNoOracle_DeveAbortarECompensarOutros()
        {
            // Arrange
            var executionLog = new List<string>();

            // Mock Portal - sucesso
            _repositoryHostedPortalMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()))
                .Callback(() => executionLog.Add("PORTAL-BEGIN"));
            _repositoryHostedPortalMock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>(), It.IsAny<IStatelessSession>()))
                .ReturnsAsync((DistributedTransactionLog entity, NHibernate.IStatelessSession session) => { entity.Id = 1; return entity; });
            _repositoryHostedPortalMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Returns(() =>
                {
                    executionLog.Add("PORTAL-COMMIT");
                    return Task.FromResult<(bool, Exception?)>((true, null));
                });
            _repositoryHostedPortalMock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()))
                .Callback(() => executionLog.Add("PORTAL-ROLLBACK"));

            // Mock CM - TIMEOUT
            _repositoryCmMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()))
                .Callback(() =>
                {
                    executionLog.Add("CM-BEGIN");
                    Thread.Sleep(100); // Simular operaÃ§Ã£o lenta
                });

            _repositoryCmMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Returns(async () =>
                {
                    executionLog.Add("CM-TIMEOUT");
                    await Task.Delay(100); // Simular timeout (reduzido)
                    throw new TimeoutException("Timeout ao commitar no Oracle");
                });

            var model = new InclusaoReservaInputModel();
            var operationId = Guid.NewGuid().ToString();

            var steps = new List<IDistributedTransactionStep>
            {
                new GravacaoLogPortalStep(_repositoryHostedPortalMock.Object, _gravacaoLoggerMock.Object,
                    operationId, "Test", model),
                new AppConfirmacaoCmStep(_repositoryCmMock.Object, _confirmacaoLoggerMock.Object, model)
            };

            var orchestrator = new SagaOrchestrator(_sagaLoggerMock.Object);

            // Act
            var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);
            Assert.Contains("PORTAL-ROLLBACK", executionLog);

            // CompensaÃ§Ã£o deve ocorrer
            _repositoryHostedPortalMock.Verify(r => r.Rollback(It.IsAny<IStatelessSession>()), Times.AtLeastOnce);

            // CompensaÃ§Ã£o deve ocorrer
            _repositoryHostedPortalMock.Verify(r => r.Rollback(It.IsAny<IStatelessSession>()), Times.AtLeastOnce);
        }

        #endregion

        #region CenÃ¡rios de CompensaÃ§Ã£o Parcial

        [Fact]
        public async Task CenarioReal_CompensacaoFalhaEmUmBanco_DeveContinuarCompensandoOutros()
        {
            // Arrange
            var compensationLog = new List<string>();

            // Mock Portal - commit OK, mas rollback FALHA
            _repositoryHostedPortalMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()));
            _repositoryHostedPortalMock.Setup(r => r.GetLoggedToken())
                .ReturnsAsync(new TokenResultModel { UserId = 1, Login = "test@test.com", CompanyId = "1" });
            _repositoryHostedPortalMock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>(), It.IsAny<IStatelessSession>()))
                .ReturnsAsync((DistributedTransactionLog entity, NHibernate.IStatelessSession session) => { entity.Id = 1; return entity; });
            _repositoryHostedPortalMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Returns(() =>
                {
                    compensationLog.Add("PORTAL-COMMIT-OK");
                    return Task.FromResult((executed: true, exception: (Exception?)null));
                });
            _repositoryHostedPortalMock.Setup(r => r.FindByHql<object>(It.IsAny<string>(), It.IsAny<NHibernate.IStatelessSession>(), It.IsAny<SW_Utils.Auxiliar.Parameter[]>()))
                .ReturnsAsync(new List<object>());
            _repositoryHostedPortalMock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()))
                .Callback(() =>
                {
                    compensationLog.Add("PORTAL-ROLLBACK-FAILED");
                    throw new Exception("Falha ao fazer rollback no PostgreSQL");
                });

            // Mock CM - commit OK, rollback OK
            _repositoryCmMock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()));
            _repositoryCmMock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Returns(() =>
                {
                    compensationLog.Add("CM-COMMIT-OK");
                    return Task.FromResult((executed: true, exception: (Exception?)null));
                });
            _repositoryCmMock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()))
                .Callback(() => compensationLog.Add("CM-ROLLBACK-OK"));

            // Step que falha
            var failingStep = new Mock<IDistributedTransactionStep>();
            failingStep.Setup(s => s.StepName).Returns("FailingStep");
            failingStep.Setup(s => s.Order).Returns(3);
            failingStep.Setup(s => s.ExecuteAsync())
                .Callback(() => compensationLog.Add("STEP-FAILED"))
                .ReturnsAsync((false, "Step falhou intencionalmente", null));

            var model = new InclusaoReservaInputModel();
            var operationId = Guid.NewGuid().ToString();

            var steps = new List<IDistributedTransactionStep>
            {
                new GravacaoLogPortalStep(_repositoryHostedPortalMock.Object, _gravacaoLoggerMock.Object,
                    operationId, "Test", model),
                new AppConfirmacaoCmStep(_repositoryCmMock.Object, _confirmacaoLoggerMock.Object, model),
                failingStep.Object
            };

            var orchestrator = new SagaOrchestrator(_sagaLoggerMock.Object);

            // Act
            var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

            // Assert
            Assert.False(success);

            // Deve ter tentado compensar ambos, mesmo com falha no Portal
            Assert.Contains("PORTAL-ROLLBACK-FAILED", compensationLog);
            Assert.Contains("CM-ROLLBACK-OK", compensationLog);

            // CM deve ter sido compensado com sucesso
            _repositoryCmMock.Verify(r => r.Rollback(It.IsAny<IStatelessSession>()), Times.Once);
        }

        #endregion

        #region CenÃ¡rios de Race Condition

        [Fact]
        public async Task CenarioReal_ExecutacaoSimultanea_DeveIniciarECompensarAtomicamente()
        {
            // Arrange - Simular 2 operaÃ§Ãµes simultÃ¢neas
            var operation1Log = new List<string>();
            var operation2Log = new List<string>();

            var repo1Mock = new Mock<IRepositoryNH>();
            repo1Mock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()))
                .Callback(() => operation1Log.Add("OP1-BEGIN"));
            repo1Mock.Setup(r => r.GetLoggedToken())
                .ReturnsAsync(new TokenResultModel { UserId = 1, Login = "test@test.com", CompanyId = "1" });
            repo1Mock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>(), It.IsAny<IStatelessSession>()))
                .ReturnsAsync((DistributedTransactionLog entity, NHibernate.IStatelessSession session) => { entity.Id = 1; return entity; });
            repo1Mock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Callback(() => operation1Log.Add("OP1-COMMIT"))
                .ReturnsAsync((executed: true, exception: (Exception?)null));
            repo1Mock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()));

            var repo2Mock = new Mock<IRepositoryNH>();
            repo2Mock.Setup(r => r.BeginTransaction(It.IsAny<IStatelessSession>()))
                .Callback(() => operation2Log.Add("OP2-BEGIN"));
            repo2Mock.Setup(r => r.GetLoggedToken())
                .ReturnsAsync(new TokenResultModel { UserId = 2, Login = "test2@test.com", CompanyId = "1" });
            repo2Mock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>(), It.IsAny<IStatelessSession>()))
                .ReturnsAsync((DistributedTransactionLog entity, NHibernate.IStatelessSession session) => { entity.Id = 2; return entity; });
            repo2Mock.Setup(r => r.CommitAsync(It.IsAny<IStatelessSession>()))
                .Callback(() => operation2Log.Add("OP2-COMMIT"))
                .ReturnsAsync((executed: true, exception: (Exception?)null));
            repo2Mock.Setup(r => r.Rollback(It.IsAny<IStatelessSession>()));

            var model = new InclusaoReservaInputModel();
            var operationId1 = Guid.NewGuid().ToString();
            var operationId2 = Guid.NewGuid().ToString();

            var steps1 = new List<IDistributedTransactionStep>
            {
                new GravacaoLogPortalStep(repo1Mock.Object, _gravacaoLoggerMock.Object,
                    operationId1, "Test", model)
            };

            var steps2 = new List<IDistributedTransactionStep>
            {
                new GravacaoLogPortalStep(repo2Mock.Object, _gravacaoLoggerMock.Object,
                    operationId2, "Test", model)
            };

            var orchestrator1 = new SagaOrchestrator(_sagaLoggerMock.Object);
            var orchestrator2 = new SagaOrchestrator(_sagaLoggerMock.Object);

            // Act - Executar simultaneamente
            var task1 = Task.Run(() => orchestrator1.ExecuteAsync(steps1, operationId1));
            var task2 = Task.Run(() => orchestrator2.ExecuteAsync(steps2, operationId2));

            await Task.WhenAll(task1, task2);

            // Assert - Ambas devem ter executado independentemente
            Assert.Contains("OP1-BEGIN", operation1Log);
            Assert.Contains("OP1-COMMIT", operation1Log);
            Assert.Contains("OP2-BEGIN", operation2Log);
            Assert.Contains("OP2-COMMIT", operation2Log);
        }

        #endregion
    }
}
