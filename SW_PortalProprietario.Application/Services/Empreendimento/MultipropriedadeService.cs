using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.Application.Services.Empreendimento
{
    /// <summary>
    /// Serviço de aplicação para operações de Multipropriedade com suporte a transações distribuídas
    /// </summary>
    public class MultipropriedadeService : IMultipropriedadeService
    {
        private readonly IRepositoryNHAccessCenter _repositoryAccessCenter;
        private readonly IRepositoryNHEsolPortal _repositoryPortal;
        private readonly IRepositoryNH _repositorySystem;
        private readonly IEmpreendimentoHybridProviderService _empreendimentoProvider;
        private readonly ILogger<MultipropriedadeService> _logger;
        private readonly ILogger<SagaOrchestrator> _sagaLogger;

        public MultipropriedadeService(
            IRepositoryNHAccessCenter repositoryAccessCenter,
            IRepositoryNHEsolPortal repositoryPortal,
            IRepositoryNH repositorySystem,
            IEmpreendimentoHybridProviderService empreendimentoProvider,
            ILogger<MultipropriedadeService> logger,
            ILogger<SagaOrchestrator> sagaLogger)
        {
            _repositoryAccessCenter = repositoryAccessCenter;
            _repositoryPortal = repositoryPortal;
            _repositorySystem = repositorySystem;
            _empreendimentoProvider = empreendimentoProvider;
            _logger = logger;
            _sagaLogger = sagaLogger;
        }

        /// <summary>
        /// Cria uma reserva de agendamento usando Saga
        /// </summary>
        public async Task<ResultModel<int>> SalvarReservaEmAgendamentoAsync(
            CriacaoReservaAgendamentoInputModel model,
            bool usarSaga = true)
        {
            if (!usarSaga)
            {
                // Fallback: método tradicional
                return await _empreendimentoProvider.SalvarReservaEmAgendamento(model);
            }

            var operationId = Guid.NewGuid().ToString();

            _logger.LogInformation("?? [MULTIPROP-SERVICE] Criando reserva com Saga - OperationId: {OperationId}", 
                operationId);

            try
            {
                // TODO: Implementar steps específicos para Multipropriedade
                var steps = new List<IDistributedTransactionStep>
                {
                    // Step 1: Validar no AccessCenter
                    // Step 2: Gravar log no Portal
                    // Step 3: Criar reserva via API eSolution
                    // Step 4: Confirmar no AccessCenter
                };

                var orchestrator = new SagaOrchestrator(_sagaLogger);
                var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

                if (success)
                {
                    return new ResultModel<int>(0)
                    {
                        Success = true,
                        Message = "Reserva criada com sucesso",
                        AdditionalData = new Dictionary<string, object>
                        {
                            { "OperationId", operationId }
                        }
                    };
                }

                return new ResultModel<int>(-1)
                {
                    Success = false,
                    Message = errorMessage,
                    Errors = new List<string> { errorMessage }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? [MULTIPROP-SERVICE] Erro - OperationId: {OperationId}", operationId);
                return new ResultModel<int>(-1)
                {
                    Success = false,
                    Message = "Erro ao criar reserva",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Libera semana para pool usando Saga
        /// </summary>
        public async Task<ResultModel<bool>> LiberarMinhaSemanaPoolAsync(
            LiberacaoMeuAgendamentoInputModel model,
            bool usarSaga = true)
        {
            if (!usarSaga)
            {
                // Fallback: método tradicional
                return await _empreendimentoProvider.LiberarMinhaSemanaPool(model);
            }

            var operationId = Guid.NewGuid().ToString();

            _logger.LogInformation("?? [MULTIPROP-SERVICE] Liberando para pool com Saga - OperationId: {OperationId}", 
                operationId);

            try
            {
                // TODO: Implementar steps
                var steps = new List<IDistributedTransactionStep>
                {
                    // Steps específicos para liberação de pool
                };

                var orchestrator = new SagaOrchestrator(_sagaLogger);
                var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

                return new ResultModel<bool>(success)
                {
                    Success = success,
                    Message = success ? "Liberado para pool" : errorMessage,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "OperationId", operationId }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? [MULTIPROP-SERVICE] Erro ao liberar - OperationId: {OperationId}", 
                    operationId);

                return new ResultModel<bool>(false)
                {
                    Success = false,
                    Message = "Erro ao liberar para pool",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Troca semana usando Saga
        /// </summary>
        public async Task<ResultModel<int>> TrocarSemanaAsync(
            TrocaSemanaInputModel model,
            bool usarSaga = true)
        {
            if (!usarSaga)
            {
                // Fallback
                return await _empreendimentoProvider.TrocarSemana(model);
            }

            var operationId = Guid.NewGuid().ToString();

            _logger.LogInformation("?? [MULTIPROP-SERVICE] Trocando semana com Saga - OperationId: {OperationId}", 
                operationId);

            try
            {
                // TODO: Implementar steps para troca
                var steps = new List<IDistributedTransactionStep>
                {
                    // Steps para troca de semana
                };

                var orchestrator = new SagaOrchestrator(_sagaLogger);
                var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

                return new ResultModel<int>(success ? 1 : -1)
                {
                    Success = success,
                    Message = success ? "Semana trocada" : errorMessage,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "OperationId", operationId }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? [MULTIPROP-SERVICE] Erro ao trocar - OperationId: {OperationId}", 
                    operationId);

                return new ResultModel<int>(-1)
                {
                    Success = false,
                    Message = "Erro ao trocar semana",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
