using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Configuracoes
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class RegraIntercambioController : ControllerBase
    {
        private readonly IRegraIntercambioService _service;
        private readonly ILogger<RegraIntercambioController> _logger;

        public RegraIntercambioController(
            IRegraIntercambioService service,
            ILogger<RegraIntercambioController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("opcoes")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioOpcoesModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioOpcoesModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpcoes()
        {
            try
            {
                var result = await _service.GetOpcoesAsync();
                return Ok(new ResultModel<RegraIntercambioOpcoesModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao buscar opções de regras de intercâmbio");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RegraIntercambioOpcoesModel>(new RegraIntercambioOpcoesModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("Todos")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<List<RegraIntercambioModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<RegraIntercambioModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new ResultModel<List<RegraIntercambioModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao listar regras de intercâmbio");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<RegraIntercambioModel>>(new List<RegraIntercambioModel>())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("Edit")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery]int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new ResultModel<RegraIntercambioModel>(new RegraIntercambioModel())
                    {
                        Errors = new List<string> { $"Regra com ID {id} não encontrada" },
                        Status = StatusCodes.Status404NotFound,
                        Success = false
                    });
                return Ok(new ResultModel<RegraIntercambioModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao buscar regra de intercâmbio {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RegraIntercambioModel>(new RegraIntercambioModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("Salvar")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] RegraIntercambioInputModel model)
        {
            try
            {
                var result = await _service.CreateAsync(model);
                return Ok(new ResultModel<RegraIntercambioModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<RegraIntercambioModel>(new RegraIntercambioModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao criar regra de intercâmbio");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RegraIntercambioModel>(new RegraIntercambioModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("Alterar")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<RegraIntercambioModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] RegraIntercambioInputModel model)
        {
            try
            {
                var result = await _service.UpdateAsync(model);
                return Ok(new ResultModel<RegraIntercambioModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<RegraIntercambioModel>(new RegraIntercambioModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao atualizar regra de intercâmbio {Id}", model.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RegraIntercambioModel>(new RegraIntercambioModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete{id:int}")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                return Ok(new ResultModel<bool>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>(false)
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao excluir regra de intercâmbio {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }
    }
}
