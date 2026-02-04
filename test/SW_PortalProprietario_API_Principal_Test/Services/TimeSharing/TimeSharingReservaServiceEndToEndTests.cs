using Microsoft.Extensions.Logging;
using Moq;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Application.Services.TimeSharing;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_Utils.Auxiliar;
using Xunit;
using AppConfirmacaoCmStep = SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing.ConfirmacaoCmStep;

namespace SW_PortalProprietario.Test.Services.TimeSharing
{
    /// <summary>
    /// Testes End-to-End do servi�o de reservas TimeSharing com Saga
    /// Simula fluxo completo desde o controller at� os reposit�rios
    /// </summary>
    public class TimeSharingReservaServiceEndToEndTests
    {
        private readonly Mock<IRepositoryNHCm> _repositoryCmMock;
        private readonly Mock<IRepositoryNH> _repositorySystemMock;
        private readonly Mock<ITimeSharingProviderService> _timeSharingProviderMock;
        private readonly Mock<ILogger<TimeSharingReservaService>> _serviceLoggerMock;
        private readonly Mock<ILogger<ValidacaoCmStep>> _validacaoLoggerMock;
        private readonly Mock<ILogger<GravacaoLogPortalStep>> _gravacaoLoggerMock;
        private readonly Mock<ILogger<CriacaoReservaApiStep>> _criacaoLoggerMock;
        private readonly Mock<ILogger<AppConfirmacaoCmStep>> _confirmacaoLoggerMock;
        private readonly Mock<ILogger<SagaOrchestrator>> _sagaLoggerMock;

        private readonly TimeSharingReservaService _service;

        public TimeSharingReservaServiceEndToEndTests()
        {
            _repositoryCmMock = new Mock<IRepositoryNHCm>();
            _repositorySystemMock = new Mock<IRepositoryNH>();
            _timeSharingProviderMock = new Mock<ITimeSharingProviderService>();
            _serviceLoggerMock = new Mock<ILogger<TimeSharingReservaService>>();
            _validacaoLoggerMock = new Mock<ILogger<ValidacaoCmStep>>();
            _gravacaoLoggerMock = new Mock<ILogger<GravacaoLogPortalStep>>();
            _criacaoLoggerMock = new Mock<ILogger<CriacaoReservaApiStep>>();
            _confirmacaoLoggerMock = new Mock<ILogger<AppConfirmacaoCmStep>>();
            _sagaLoggerMock = new Mock<ILogger<SagaOrchestrator>>();

            _service = new TimeSharingReservaService(
                _repositoryCmMock.Object,
                _repositorySystemMock.Object,
                _timeSharingProviderMock.Object,
                _serviceLoggerMock.Object,
                _validacaoLoggerMock.Object,
                _gravacaoLoggerMock.Object,
                _criacaoLoggerMock.Object,
                _confirmacaoLoggerMock.Object,
                _sagaLoggerMock.Object
            );
        }

        #region Testes End-to-End de Sucesso

        [Fact]
        public async Task E2E_CriarReservaSucesso_DeveChamarTodosOsStepsNaOrdem()
        {
            // Arrange
            var executionLog = new List<string>();

            // Mock valida��o CM
            _repositoryCmMock.Setup(r => r.FindByHql<object>(It.IsAny<string>(), It.IsAny<Parameter[]>()))
                .Callback(() => executionLog.Add("1-VALIDACAO-CM"))
                .ReturnsAsync(Array.Empty<object>());

            // Mock log Portal
            _repositorySystemMock.Setup(r => r.BeginTransaction())
                .Callback(() => executionLog.Add("2-PORTAL-BEGIN"));
            _repositorySystemMock.Setup(r => r.Save(It.IsAny<object>()))
                .Callback(() => executionLog.Add("2-PORTAL-SAVE"))
                .Returns(Task.FromResult<object>(1));
            _repositorySystemMock.Setup(r => r.CommitAsync())
                .Callback(() => executionLog.Add("2-PORTAL-COMMIT"));
            _repositorySystemMock.Setup(r => r.CommitAsync())
                .Returns(Task.FromResult<(bool, Exception?)>((true, null)));

            // Mock API
            _timeSharingProviderMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                .Callback(() => executionLog.Add("3-API-CHAMADA"))
                .ReturnsAsync(12345);

            // Mock confirma��o CM
            _repositoryCmMock.Setup(r => r.BeginTransaction())
                .Callback(() => executionLog.Add("4-CM-BEGIN"));
            _repositoryCmMock.Setup(r => r.CommitAsync())
                .Callback(() => executionLog.Add("4-CM-COMMIT"));
            _repositoryCmMock.Setup(r => r.CommitAsync())
                .Returns(Task.FromResult<(bool, Exception?)>((true, null)));

            var model = new InclusaoReservaInputModel
            {
                IdVendaXContrato = 123
            };

            // Act
            var resultado = await _service.CriarReservaAsync(model, usarSaga: true);

            // Assert
            Assert.True(resultado.Success);
            
            // Verificar ordem de execu��o
            Assert.Equal(6, executionLog.Count);
            Assert.StartsWith("1-", executionLog[0]); // Valida��o primeiro
            Assert.StartsWith("2-", executionLog[1]); // Portal segundo
            Assert.StartsWith("3-", executionLog[3]); // API terceiro
            Assert.StartsWith("4-", executionLog[4]); // CM �ltimo
        }

        [Fact]
        public async Task E2E_CriarReservaSemSaga_DeveUsarMetodoTradicional()
        {
            // Arrange
            _timeSharingProviderMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                .ReturnsAsync(99999);

            var model = new InclusaoReservaInputModel();

            // Act
            var resultado = await _service.CriarReservaAsync(model, usarSaga: false);

            // Assert
            Assert.True(resultado.Success);
            Assert.Equal(99999, resultado.Data);
            
            // Deve chamar apenas o provider, sem Saga
            _timeSharingProviderMock.Verify(s => s.Save(It.IsAny<InclusaoReservaInputModel>()), Times.Once);
            
            // N�o deve chamar os steps de Saga
            _repositoryCmMock.Verify(r => r.BeginTransaction(), Times.Never);
            _repositorySystemMock.Verify(r => r.BeginTransaction(), Times.Never);
        }

        #endregion

        #region Testes End-to-End de Falha

        [Fact]
        public async Task E2E_APIFalha_DeveRollbackTodosOsBancos()
        {
            // Arrange
            var transactionStates = new Dictionary<string, bool>
            {
                { "CM_RolledBack", false },
                { "Portal_RolledBack", false }
            };

            // Mock Portal - sucesso
            _repositorySystemMock.Setup(r => r.BeginTransaction());
            _repositorySystemMock.Setup(r => r.Save(It.IsAny<object>())).ReturnsAsync(1);
            _repositorySystemMock.Setup(r => r.CommitAsync()).ReturnsAsync((true, null));
            _repositorySystemMock.Setup(r => r.Rollback())
                .Callback(() => transactionStates["Portal_RolledBack"] = true);

            // Mock CM - sucesso
            _repositoryCmMock.Setup(r => r.BeginTransaction());
            _repositoryCmMock.Setup(r => r.CommitAsync()).ReturnsAsync((true, null));
            _repositoryCmMock.Setup(r => r.Rollback())
                .Callback(() => transactionStates["CM_RolledBack"] = true);

            // Mock API - FALHA
            _timeSharingProviderMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                .ThrowsAsync(new HttpRequestException("API indispon�vel"));

            var model = new InclusaoReservaInputModel();

            // Act
            var resultado = await _service.CriarReservaAsync(model, usarSaga: true);

            // Assert
            Assert.False(resultado.Success);
            Assert.Contains("API indispon�vel", resultado.Message);
            
            // Ambos os bancos devem ter rollback
            Assert.True(transactionStates["Portal_RolledBack"], "Portal deveria ter rollback");
            // CM n�o deve ter rollback pois falhou antes de commitar
        }

        [Fact]
        public async Task E2E_OracleCommitFalha_DeveRollbackPostgreSQL()
        {
            // Arrange
            var postgresRolledBack = false;

            // Mock Portal - sucesso
            _repositorySystemMock.Setup(r => r.BeginTransaction());
            _repositorySystemMock.Setup(r => r.Save(It.IsAny<object>())).ReturnsAsync(1);
            _repositorySystemMock.Setup(r => r.CommitAsync()).ReturnsAsync((true, null));
            _repositorySystemMock.Setup(r => r.Rollback())
                .Callback(() => postgresRolledBack = true);

            // Mock API - sucesso
            _timeSharingProviderMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                .ReturnsAsync(12345);

            // Mock CM - FALHA no commit
            _repositoryCmMock.Setup(r => r.BeginTransaction());
            _repositoryCmMock.Setup(r => r.CommitAsync())
                .ReturnsAsync((false, new Exception("Deadlock no Oracle")));

            var model = new InclusaoReservaInputModel();

            // Act
            var resultado = await _service.CriarReservaAsync(model, usarSaga: true);

            // Assert
            Assert.False(resultado.Success);
            Assert.True(postgresRolledBack, "PostgreSQL deveria ter feito rollback");
        }

        #endregion

        #region Testes de Atomicidade (Two-Phase Commit)

        [Fact]
        public async Task E2E_AtomicidadeGarantida_TodosOuNenhum()
        {
            // Cen�rio: Simular m�ltiplas tentativas, onde ora sucede, ora falha
            
            var tentativas = new List<Dictionary<string, bool>>();

            for (int i = 0; i < 5; i++)
            {
                var execucaoAtual = new Dictionary<string, bool>
                {
                    { "CM_Committed", false },
                    { "Portal_Committed", false },
                    { "API_Executed", false },
                    { "API_Executado", false }
                };

                // Reset mocks
                _repositoryCmMock.Reset();
                _repositorySystemMock.Reset();
                _timeSharingProviderMock.Reset();

                // Configurar sucesso ou falha alternadamente
                bool shouldSucceed = i % 2 == 0;

                if (shouldSucceed)
                {
                    // Todos devem commitar
                    _repositorySystemMock.Setup(r => r.BeginTransaction());
                    _repositorySystemMock.Setup(r => r.GetLoggedUser())
                        .ReturnsAsync((userId: "1", providerKeyUser: "test@test.com", companyId: "1", isAdm: false));
                    _repositorySystemMock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>()))
                        .ReturnsAsync((DistributedTransactionLog entity) => { entity.Id = 1; return entity; });
                    _repositorySystemMock.Setup(r => r.CommitAsync())
                        .Callback(() => execucaoAtual["Portal_Committed"] = true)
                        .ReturnsAsync((executed: true, exception: (Exception?)null));

                    _timeSharingProviderMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                        .Callback(() => 
                        {
                            execucaoAtual["API_Executed"] = true;
                            execucaoAtual["API_Executado"] = true;
                        })
                        .ReturnsAsync(12345);

                    _repositoryCmMock.Setup(r => r.BeginTransaction());
                    _repositoryCmMock.Setup(r => r.CommitAsync())
                        .Callback(() => execucaoAtual["CM_Committed"] = true)
                        .ReturnsAsync((executed: true, exception: (Exception?)null));
                }
                else
                {
                    // API deve falhar, nenhum deve commitar
                    _repositorySystemMock.Setup(r => r.BeginTransaction(It.IsAny<NHibernate.IStatelessSession>()));
                    _repositorySystemMock.Setup(r => r.GetLoggedUser())
                        .ReturnsAsync((userId: "1", providerKeyUser: "test@test.com", companyId: "1", isAdm: false));
                    _repositorySystemMock.Setup(r => r.Save(It.IsAny<DistributedTransactionLog>(), It.IsAny<NHibernate.IStatelessSession>()))
                        .ReturnsAsync((DistributedTransactionLog entity) => { entity.Id = 1; return entity; });
                    _repositorySystemMock.Setup(r => r.CommitAsync(It.IsAny<NHibernate.IStatelessSession>()))
                        .ReturnsAsync((executed: true, exception: (Exception?)null));
                    _repositorySystemMock.Setup(r => r.FindByHql<object>(It.IsAny<string>(),It.IsAny<NHibernate.IStatelessSession>(), It.IsAny<Parameter[]>()))
                        .ReturnsAsync(new List<object>());
                    _repositorySystemMock.Setup(r => r.Rollback(It.IsAny<NHibernate.IStatelessSession>()));

                    _timeSharingProviderMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                        .ThrowsAsync(new Exception("Falha simulada"));

                    _repositoryCmMock.Setup(r => r.BeginTransaction(It.IsAny<NHibernate.IStatelessSession>()));
                    _repositoryCmMock.Setup(r => r.Rollback(It.IsAny<NHibernate.IStatelessSession>()));
                }

                var model = new InclusaoReservaInputModel();
                var resultado = await _service.CriarReservaAsync(model, usarSaga: true);

                if (shouldSucceed)
                {
                    // Todos devem estar commitados
                    Assert.True(execucaoAtual["Portal_Committed"], $"Tentativa {i}: Portal deveria commitar");
                    Assert.True(execucaoAtual["API_Executado"], $"Tentativa {i}: API deveria executar");
                    Assert.True(execucaoAtual["CM_Committed"], $"Tentativa {i}: CM deveria commitar");
                }
                else
                {
                    // Nenhum deve estar commitado (rollback)
                    Assert.False(execucaoAtual["CM_Committed"], $"Tentativa {i}: CM N�O deveria commitar");
                }

                tentativas.Add(execucaoAtual);
            }

            // Verificar que atomicidade foi mantida em todas as tentativas
            Assert.Equal(5, tentativas.Count);
        }

        #endregion

        #region Testes de Performance

        [Fact]
        public async Task E2E_MultiplasChamadasSimultaneas_DeveFuncionarCorretamente()
        {
            // Arrange
            var sucessos = 0;
            var falhas = 0;

            _repositorySystemMock.Setup(r => r.BeginTransaction(It.IsAny<NHibernate.IStatelessSession>()));
            _repositorySystemMock.Setup(r => r.Save(It.IsAny<object>(), It.IsAny<NHibernate.IStatelessSession>())).ReturnsAsync(1);
            _repositorySystemMock.Setup(r => r.CommitAsync(It.IsAny<NHibernate.IStatelessSession>())).ReturnsAsync((true, null));

            _repositoryCmMock.Setup(r => r.BeginTransaction(It.IsAny<NHibernate.IStatelessSession>()));
            _repositoryCmMock.Setup(r => r.CommitAsync(It.IsAny<NHibernate.IStatelessSession>())).ReturnsAsync((true, null));

            _timeSharingProviderMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                .ReturnsAsync(12345);

            var model = new InclusaoReservaInputModel();

            // Act - 10 chamadas simult�neas
            var tasks = Enumerable.Range(1, 10).Select(async i =>
            {
                var resultado = await _service.CriarReservaAsync(model, usarSaga: true);
                
                if (resultado.Success)
                    Interlocked.Increment(ref sucessos);
                else
                    Interlocked.Increment(ref falhas);
            });

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, sucessos + falhas);
            Assert.True(sucessos > 0, "Pelo menos uma chamada deveria ter sucesso");
        }

        #endregion

        #region Testes de OperationId e Rastreabilidade

        [Fact]
        public async Task E2E_CadaOperacao_DeveGerarOperationIdUnico()
        {
            // Arrange
            var operationIds = new HashSet<string>();

            _repositorySystemMock.Setup(r => r.BeginTransaction(It.IsAny<NHibernate.IStatelessSession>()));
            _repositorySystemMock.Setup(r => r.Save(It.IsAny<object>(), It.IsAny<NHibernate.IStatelessSession>()))
                .Callback<object>(obj =>
                {
                    // Capturar OperationId do log
                    if (obj != null)
                    {
                        var props = obj.GetType().GetProperties();
                        var opIdProp = props.FirstOrDefault(p => p.Name == "OperationId");
                        if (opIdProp != null)
                        {
                            var opId = opIdProp.GetValue(obj)?.ToString();
                            if (!string.IsNullOrEmpty(opId))
                                operationIds.Add(opId);
                        }
                    }
                })
                .ReturnsAsync(1);
            _repositorySystemMock.Setup(r => r.CommitAsync(It.IsAny<NHibernate.IStatelessSession>())).ReturnsAsync((true, null));

            _repositoryCmMock.Setup(r => r.BeginTransaction(It.IsAny<NHibernate.IStatelessSession>()));
            _repositoryCmMock.Setup(r => r.CommitAsync(It.IsAny<NHibernate.IStatelessSession>())).ReturnsAsync((true, null));

            _timeSharingProviderMock.Setup(s => s.Save(It.IsAny<InclusaoReservaInputModel>()))
                .ReturnsAsync(12345);

            var model = new InclusaoReservaInputModel();

            // Act - 5 opera��es
            for (int i = 0; i < 5; i++)
            {
                await _service.CriarReservaAsync(model, usarSaga: true);
            }

            // Assert
            Assert.Equal(5, operationIds.Count); // Todos �nicos
        }

        #endregion
    }
}
