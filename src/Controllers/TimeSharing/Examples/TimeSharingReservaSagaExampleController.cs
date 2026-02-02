using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.DistributedTransactions.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.TimeSharing.Examples
{
    /// <summary>
    /// EXEMPLO DE USO DO PADRÃO SAGA
    /// Controller demonstrando como utilizar transações distribuídas
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TimeSharingReservaSagaExampleController : ControllerBase
    {
        private readonly IRepositoryNHCm _repositoryCm;
        private readonly IRepositoryNH _repositorySystem;
        private readonly ITimeSharingProviderService _timeSharingService;
        private readonly ILogger<TimeSharingReservaSagaExampleController> _logger;
        private readonly ILogger<ValidacaoCmStep> _validacaoCmLogger;
        private readonly ILogger<GravacaoLogPortalStep> _gravacaoLogPortalLogger;
        private readonly ILogger<CriacaoReservaApiStep> _criacaoReservaApiLogger;
        private readonly ILogger<ConfirmacaoCmStep> _confirmacaoCmLogger;

        public TimeSharingReservaSagaExampleController(
            IRepositoryNHCm repositoryCm,
            IRepositoryNH repositorySystem,
            ITimeSharingProviderService timeSharingService,
            ILogger<TimeSharingReservaSagaExampleController> logger,
            ILogger<ValidacaoCmStep> validacaoCmLogger,
            ILogger<GravacaoLogPortalStep> gravacaoLogPortalLogger,
            ILogger<CriacaoReservaApiStep> criacaoReservaApiLogger,
            ILogger<ConfirmacaoCmStep> confirmacaoCmLogger)
        {
            _repositoryCm = repositoryCm;
            _repositorySystem = repositorySystem;
            _timeSharingService = timeSharingService;
            _logger = logger;
            _validacaoCmLogger = validacaoCmLogger;
            _gravacaoLogPortalLogger = gravacaoLogPortalLogger;
            _criacaoReservaApiLogger = criacaoReservaApiLogger;
            _confirmacaoCmLogger = confirmacaoCmLogger;
        }

        /// <summary>
        /// Exemplo: Criar reserva usando o padrão Saga
        /// Garante atomicidade entre Oracle (CM), PostgreSQL (Portal) e API externa
        /// </summary>
        [HttpPost("criar-reserva-saga")]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarReservaSaga([FromBody] InclusaoReservaInputModel model)
        {
            // 1. Gerar ID único para rastreamento da operação
            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("🔵 Iniciando criação de reserva com Saga - OperationId: {OperationId}", operationId);

            try
            {
                // 2. Definir os steps da transação distribuída na ordem correta
                var steps = new List<IDistributedTransactionStep>
                {
                    // Step 1: Validação no CM (não precisa compensação)
                    new ValidacaoCmStep(
                        _repositoryCm,
                        _validacaoCmLogger,
                        model),

                    // Step 2: Gravação de log no Portal PostgreSQL
                    new GravacaoLogPortalStep(
                        _repositorySystem,
                        _gravacaoLogPortalLogger,
                        operationId,
                        "CriacaoReservaTimeSharing",
                        model),

                    // Step 3: Criação de reserva via API externa
                    new CriacaoReservaApiStep(
                        _timeSharingService,
                        _criacaoReservaApiLogger,
                        model),

                    // Step 4: Confirmação final no CM Oracle
                    new ConfirmacaoCmStep(
                        _repositoryCm,
                        _confirmacaoCmLogger,
                        model)
                };

                // 3. Executar a Saga
                var orchestrator = new SagaOrchestrator(
                    (ILogger<SagaOrchestrator>)HttpContext.RequestServices.GetService(typeof(ILogger<SagaOrchestrator>))
                );
                var (success, errorMessage) = await orchestrator.ExecuteAsync(steps, operationId);

                // 4. Retornar resultado
                if (success)
                {
                    _logger.LogInformation("✅ Reserva criada com sucesso - OperationId: {OperationId}", operationId);
                    
                    return Ok(new ResultModel<object>(new 
                    { 
                        Message = "Reserva criada com sucesso", 
                        OperationId = operationId 
                    })
                    {
                        Success = true,
                        Status = StatusCodes.Status200OK
                    });
                }
                
                _logger.LogWarning("⚠️ Falha na criação de reserva - OperationId: {OperationId} - Erro: {Error}", 
                    operationId, errorMessage);
                
                return BadRequest(new ResultModel<object>(new { OperationId = operationId })
                {
                    Success = false,
                    Message = errorMessage,
                    Status = StatusCodes.Status400BadRequest,
                    Errors = new List<string> { errorMessage }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Erro inesperado na criação de reserva - OperationId: {OperationId}", operationId);
                
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ResultModel<object>(new { OperationId = operationId })
                    {
                        Success = false,
                        Message = "Erro inesperado ao criar reserva",
                        Status = StatusCodes.Status500InternalServerError,
                        Errors = new List<string> { ex.Message }
                    });
            }
        }

        /// <summary>
        /// Exemplo: Consultar log de transação distribuída
        /// Permite rastrear o que aconteceu em cada step
        /// </summary>
        [HttpGet("operacao/{operationId}/log")]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConsultarLogOperacao(string operationId)
        {
            try
            {
                var logs = await _repositorySystem.FindBySql<object>(
                    $@"SELECT 
                        OperationId,
                        OperationType,
                        StepName,
                        StepOrder,
                        Status,
                        ErrorMessage,
                        DataHoraCriacao,
                        DataHoraCompensacao
                    FROM DistributedTransactionLog
                    WHERE OperationId = '{operationId}'
                    ORDER BY StepOrder");

                return Ok(new ResultModel<object>(logs)
                {
                    Success = true,
                    Status = StatusCodes.Status200OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar log da operação {OperationId}", operationId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ResultModel<object>(null)
                    {
                        Success = false,
                        Message = "Erro ao consultar log",
                        Errors = new List<string> { ex.Message }
                    });
            }
        }
    }
}
