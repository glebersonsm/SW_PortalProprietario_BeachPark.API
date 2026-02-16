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
    public class VhfConfigController : ControllerBase
    {
        private readonly IVhfConfigService _vhfConfigService;
        private readonly ILogger<VhfConfigController> _logger;

        public VhfConfigController(
            IVhfConfigService vhfConfigService,
            ILogger<VhfConfigController> logger)
        {
            _vhfConfigService = vhfConfigService;
            _logger = logger;
        }

        /// <summary>
        /// Retorna todas as opções disponíveis para configuração de reservas VHF (PMS).
        /// Inclui: Tipo de utilização, Hotéis (CM), Tipo de Hóspede (CM), Origem (CM), Tarifa Hotel (CM), Código de Pensão.
        /// </summary>
        [HttpGet("opcoes")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<VhfConfigOpcoesModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<VhfConfigOpcoesModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpcoes()
        {
            try
            {
                var result = await _vhfConfigService.GetOpcoesAsync();
                return Ok(new ResultModel<VhfConfigOpcoesModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao buscar opções de configuração VHF");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<VhfConfigOpcoesModel>(new VhfConfigOpcoesModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("Todos")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<List<VhfConfigModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<VhfConfigModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _vhfConfigService.GetAllAsync();
                return Ok(new ResultModel<List<VhfConfigModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao listar configurações VHF");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<VhfConfigModel>>(new List<VhfConfigModel>())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("Edit")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery]int id)
        {
            try
            {
                var result = await _vhfConfigService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new ResultModel<VhfConfigModel>(new VhfConfigModel())
                    {
                        Errors = new List<string> { $"Configuração com ID {id} não encontrada" },
                        Status = StatusCodes.Status404NotFound,
                        Success = false
                    });
                return Ok(new ResultModel<VhfConfigModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao buscar configuração VHF {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<VhfConfigModel>(new VhfConfigModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("Salvar")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] VhfConfigInputModel model)
        {
            try
            {
                var result = await _vhfConfigService.CreateAsync(model);
                return Ok(new ResultModel<VhfConfigModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<VhfConfigModel>(new VhfConfigModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao criar configuração VHF");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<VhfConfigModel>(new VhfConfigModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("Alterar")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<VhfConfigModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] VhfConfigInputModel model)
        {
            try
            {
                var result = await _vhfConfigService.UpdateAsync(model);
                return Ok(new ResultModel<VhfConfigModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<VhfConfigModel>(new VhfConfigModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Erro ao atualizar configuração VHF {Id}", model.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<VhfConfigModel>(new VhfConfigModel())
                {
                    Errors = new List<string> { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("deletar")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            try
            {
                var result = await _vhfConfigService.DeleteAsync(id);
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
                _logger.LogError(err, "Erro ao excluir configuração VHF {Id}", id);
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
