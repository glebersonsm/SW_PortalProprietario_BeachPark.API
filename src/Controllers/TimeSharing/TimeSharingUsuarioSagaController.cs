using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.TimeSharing
{
    /// <summary>
    /// Controller de exemplo mostrando uso do serviço com Saga
    /// SUBSTITUIR o TimeSharingUsuarioController por este quando estiver pronto
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class TimeSharingUsuarioSagaController : ControllerBase
    {
        private readonly ITimeSharingReservaService _reservaService;
        private readonly ILogger<TimeSharingUsuarioSagaController> _logger;

        public TimeSharingUsuarioSagaController(
            ITimeSharingReservaService reservaService,
            ILogger<TimeSharingUsuarioSagaController> logger)
        {
            _reservaService = reservaService;
            _logger = logger;
        }

        /// <summary>
        /// Criar reserva - USA SAGA POR PADRÃO
        /// O controller não precisa saber sobre transações distribuídas!
        /// </summary>
        [HttpPost("salvarReserva")]
        [Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<long>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<long>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SalvarReserva([FromBody] InclusaoReservaInputModel model)
        {
            try
            {
                // ?? SIMPLES! O serviço cuida de tudo
                // Por padrão usa Saga (usarSaga = true)
                var resultado = await _reservaService.CriarReservaAsync(model);

                if (resultado.Success)
                {
                    return Ok(resultado);
                }

                return BadRequest(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResultModel<long>(-1)
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar reserva");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<long>(-1)
                {
                    Success = false,
                    Message = "Erro ao criar reserva",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Criar reserva SEM SAGA (método tradicional)
        /// Útil para debug ou situações específicas
        /// </summary>
        [HttpPost("salvarReservaSemSaga")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultModel<long>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SalvarReservaSemSaga([FromBody] InclusaoReservaInputModel model)
        {
            try
            {
                // Explicitamente desabilitar Saga
                var resultado = await _reservaService.CriarReservaAsync(model, usarSaga: false);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar reserva (sem Saga)");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<long>(-1)
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancelar reserva - USA SAGA
        /// </summary>
        [HttpPost("cancelarReserva")]
        [Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelarReserva(long numReserva)
        {
            try
            {
                var cancelarModel = new CancelarReservaTsModel
                {
                    MotivoCancelamento = "1",
                    ObservacaoCancelamento = "Cancelada via Portal",
                    ReservaId = numReserva
                };

                // ?? SIMPLES! Saga é transparente
                var resultado = await _reservaService.CancelarReservaAsync(cancelarModel);

                if (resultado.Success)
                {
                    return Ok(resultado);
                }

                return BadRequest(resultado);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new ResultModel<bool>(false)
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResultModel<bool>(false)
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar reserva");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Success = false,
                    Message = "Erro ao cancelar reserva",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Alterar reserva - USA SAGA
        /// </summary>
        [HttpPost("alterarReserva")]
        [Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<long?>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AlterarReserva([FromBody] InclusaoReservaInputModel model)
        {
            try
            {
                var resultado = await _reservaService.AlterarReservaAsync(model);

                if (resultado.Success)
                {
                    return Ok(resultado);
                }

                return BadRequest(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResultModel<long?>(null)
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar reserva");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<long?>(null)
                {
                    Success = false,
                    Message = "Erro ao alterar reserva"
                });
            }
        }
    }
}
