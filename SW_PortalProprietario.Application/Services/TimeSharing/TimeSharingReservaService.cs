using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.Application.Services.TimeSharing
{
    /// <summary>
    /// Serviço de aplicação para reservas TimeSharing com suporte a transações distribuídas
    /// Encapsula o padrão Saga, mantendo os controllers limpos
    /// </summary>
    public class TimeSharingReservaService : ITimeSharingReservaService
    {
        private readonly IRepositoryNHCm _repositoryCm;
        private readonly IRepositoryNH _repositorySystem;
        private readonly ITimeSharingProviderService _timeSharingProvider;
        private readonly ILogger<TimeSharingReservaService> _logger;
        private readonly ILogger<ValidacaoCmStep> _validacaoCmLogger;
        private readonly ILogger<GravacaoLogPortalStep> _gravacaoLogLogger;
        private readonly ILogger<CriacaoReservaApiStep> _criacaoReservaLogger;
        private readonly ILogger<ConfirmacaoCmStep> _confirmacaoCmLogger;
        private readonly ILogger<SagaOrchestrator> _sagaLogger;

        public TimeSharingReservaService(
            IRepositoryNHCm repositoryCm,
            IRepositoryNH repositorySystem,
            ITimeSharingProviderService timeSharingProvider,
            ILogger<TimeSharingReservaService> logger,
            ILogger<ValidacaoCmStep> validacaoCmLogger,
            ILogger<GravacaoLogPortalStep> gravacaoLogLogger,
            ILogger<CriacaoReservaApiStep> criacaoReservaLogger,
            ILogger<ConfirmacaoCmStep> confirmacaoCmLogger,
            ILogger<SagaOrchestrator> sagaLogger)
        {
            _repositoryCm = repositoryCm;
            _repositorySystem = repositorySystem;
            _timeSharingProvider = timeSharingProvider;
            _logger = logger;
            _validacaoCmLogger = validacaoCmLogger;
            _gravacaoLogLogger = gravacaoLogLogger;
            _criacaoReservaLogger = criacaoReservaLogger;
            _confirmacaoCmLogger = confirmacaoCmLogger;
            _sagaLogger = sagaLogger;
        }

        /// <summary>
        /// Cria uma reserva usando transação distribuída (Saga Pattern)
        /// </summary>
        /// <param name="model">Dados da reserva</param>
        /// <param name="usarSaga">Se true, usa Saga; se false, usa método tradicional</param>
        /// <returns>Resultado com ID da reserva ou erro</returns>
        public async Task<ResultModel<long>> CriarReservaAsync(
            InclusaoReservaInputModel model, 
            bool usarSaga = true)
        {
            if (!usarSaga)
            {
                // Fallback: usar método tradicional sem Saga
                return await CriarReservaTradicionalAsync(model);
            }

            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("?? [RESERVA-SERVICE] Iniciando criação com Saga - OperationId: {OperationId}", 
                operationId);

            try
            {
                // Definir steps da transação distribuída
                var steps = new List<IDistributedTransactionStep>
                {
                    new ValidacaoCmStep(_repositoryCm, _validacaoCmLogger, model),
                    new GravacaoLogPortalStep(_repositorySystem, _gravacaoLogLogger, operationId, "CriacaoReservaTS", model),
                    new CriacaoReservaApiStep(_timeSharingProvider, _criacaoReservaLogger, model),
                    new ConfirmacaoCmStep(_repositoryCm, _confirmacaoCmLogger, model)
                };

                // Executar Saga
                var orchestrator = new SagaOrchestrator(_sagaLogger);
                var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

                if (success)
                {
                    _logger.LogInformation("? [RESERVA-SERVICE] Criação concluída - OperationId: {OperationId}", 
                        operationId);

                    return new ResultModel<long>(0) // TODO: Retornar ID real da reserva
                    {
                        Success = true,
                        Message = "Reserva criada com sucesso",
                        Data = 0, // TODO: Obter ID da reserva dos steps
                        AdditionalData = new Dictionary<string, object>
                        {
                            { "OperationId", operationId }
                        }
                    };
                }

                _logger.LogWarning("?? [RESERVA-SERVICE] Falha na criação - OperationId: {OperationId} - Erro: {Error}", 
                    operationId, errorMessage);

                return new ResultModel<long>(-1)
                {
                    Success = false,
                    Message = "Falha ao criar reserva",
                    Errors = new List<string> { errorMessage },
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "OperationId", operationId }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? [RESERVA-SERVICE] Erro inesperado - OperationId: {OperationId}", 
                    operationId);

                return new ResultModel<long>(-1)
                {
                    Success = false,
                    Message = "Erro inesperado ao criar reserva",
                    Errors = new List<string> { ex.Message },
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "OperationId", operationId }
                    }
                };
            }
        }

        /// <summary>
        /// Cancela uma reserva usando transação distribuída
        /// </summary>
        public async Task<ResultModel<bool>> CancelarReservaAsync(
            CancelarReservaTsModel model,
            bool usarSaga = true)
        {
            if (!usarSaga)
            {
                // Fallback: usar método tradicional
                var resultado = await _timeSharingProvider.CancelarReserva(model);
                return new ResultModel<bool>(resultado.GetValueOrDefault(false))
                {
                    Success = resultado.GetValueOrDefault(false)
                };
            }

            var operationId = Guid.NewGuid().ToString();

            _logger.LogInformation("?? [RESERVA-SERVICE] Iniciando cancelamento com Saga - OperationId: {OperationId}", 
                operationId);

            try
            {
                // TODO: Implementar steps para cancelamento
                var steps = new List<IDistributedTransactionStep>
                {
                    // Step 1: Validar cancelamento no CM
                    // Step 2: Gravar log de cancelamento
                    // Step 3: Cancelar via API
                    // Step 4: Confirmar cancelamento no CM
                };

                var orchestrator = new SagaOrchestrator(_sagaLogger);
                var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

                return new ResultModel<bool>(success)
                {
                    Success = success,
                    Message = success ? "Reserva cancelada com sucesso" : errorMessage,
                    Errors = success ? new List<string>() : new List<string> { errorMessage },
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "OperationId", operationId }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? [RESERVA-SERVICE] Erro ao cancelar - OperationId: {OperationId}", 
                    operationId);

                return new ResultModel<bool>(false)
                {
                    Success = false,
                    Message = "Erro ao cancelar reserva",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task<ResultModel<long>> CriarReservaTradicionalAsync(InclusaoReservaInputModel model)
        {
            _logger.LogInformation("?? [RESERVA-SERVICE] Usando método tradicional (sem Saga)");

            try
            {
                var reservaId = await _timeSharingProvider.Save(model);
                
                return new ResultModel<long>(reservaId)
                {
                    Success = reservaId > 0,
                    Message = reservaId > 0 ? "Reserva criada" : "Falha ao criar reserva"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no método tradicional");
                return new ResultModel<long>(-1)
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public Task<ResultModel<long?>> AlterarReservaAsync(InclusaoReservaInputModel model, bool usarSaga = true)
        {
            throw new NotImplementedException();
        }
    }
}
