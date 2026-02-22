using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Interfaces.Esol;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Esol
{
    /// <summary>
    /// Controller de condomínio - migrado do SwReservaApiMain.
    /// Sufixo Esol para evitar conflitos.
    /// </summary>
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class CondominioEsolController : ControllerBase
    {
        private readonly ICondominioEsolService _condominioService;
        private readonly ILogger<CondominioEsolController> _logger;

        public CondominioEsolController(ICondominioEsolService condominioService, ILogger<CondominioEsolController> logger)
        {
            _condominioService = condominioService;
            _logger = logger;
        }

        [HttpGet("v1/consultarSemanasContrato"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ConsultarSemanasCota([FromQuery] PeriodoCotaDisponibilidadeUsuarioSearchModel model)
        {
            try
            {
                var result = await _condominioService.ConsultarSemanasCota(model);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultWithPaginationModel<List<SemanaModel>>(result.Data)
                    {
                        Message = "",
                        Status = StatusCodes.Status200OK,
                        NumberRecords = result.NumberRecords,
                        LastPageNumber = result.LastPageNumber,
                        PageNumber = result.PageNumber,
                        Success = true
                    });
                return NotFound(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Message = "Ops! Nenhum registro encontrado!",
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível retornar os dados {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível retornar os dados {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível retornar os dados {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível retornar os dados {err.Message}",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("v1/consultarGeralSemanasContrato"), Authorize(Roles = "GestorReservasAgendamentos, Administrador")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ConsultarGeralSemanasContrato([FromBody] PeriodoCotaDisponibilidadeSearchModel searchModel)
        {
            try
            {
                var result = await _condominioService.ConsultarGeralSemanasCota(searchModel);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultWithPaginationModel<List<SemanaModel>>(result.Data)
                    {
                        Message = "",
                        Status = StatusCodes.Status200OK,
                        NumberRecords = result.NumberRecords,
                        LastPageNumber = result.LastPageNumber,
                        PageNumber = result.PageNumber,
                        Success = true
                    });
                return NotFound(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Message = "Ops! Nenhum registro encontrado!",
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível retornar os dados {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível retornar os dados {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível retornar os dados {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível retornar os dados {err.Message}",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("v1/agendamento/liberarSemanaPool"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> LiberarSemanaPool([FromBody] AgendamentoInventarioModel modelAgendamentoPool)
        {
            try
            {
                await _condominioService.LiberarSemanaPool(modelAgendamentoPool);
                return Ok(new ResultModel<bool>(true) { Success = true, Status = StatusCodes.Status200OK });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>()
                {
                    Data = false,
                    Message = err.InnerException != null ?
                        $"Não foi possível liberar semana para o Pool {err.Message} {err.InnerException.Message}" :
                        $"Não foi possível liberar semana para o Pool {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<bool>()
                {
                    Data = false,
                    Message = err.InnerException != null ?
                    $"Não foi possível liberar semana para o Pool {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível liberar semana para o Pool {err.Message}",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("v1/agendamento/retirarSemanaPool"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> RetirarSemanaPool([FromBody] AgendamentoInventarioModel modelAgendamentoPool)
        {
            try
            {
                await _condominioService.RetirarSemanaPool(modelAgendamentoPool);
                return Ok(new ResultModel<bool>(true) { Success = true, Status = StatusCodes.Status200OK });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>()
                {
                    Data = false,
                    Message = err.InnerException != null ?
                        $"Não foi possível retirar semana do Pool {err.Message} {err.InnerException.Message}" :
                        $"Não foi possível retirar semana do Pool {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<bool>()
                {
                    Data = false,
                    Message = err.InnerException != null ?
                    $"Não foi possível retirar semana do Pool {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível retirar semana do Pool {err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("v1/agendamento/inventarios"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<InventarioModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<InventarioModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<InventarioModel>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<InventarioModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ConsultarInventarios([FromQuery] InventarioSearchModel searchModel)
        {
            try
            {
                var result = await _condominioService.ConsultarInventarios(searchModel);
                return Ok(new ResultModel<List<InventarioModel>>(result?.Data ?? new List<InventarioModel>())
                {
                    Message = "",
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<InventarioModel>>()
                {
                    Data = new List<InventarioModel>(),
                    Message = err.InnerException != null ?
                    $"Não foi possível consultar os inventários: {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível consultar os inventários: {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<InventarioModel>>()
                {
                    Data = new List<InventarioModel>(),
                    Message = err.InnerException != null ?
                    $"Não foi possível consultar os inventários: {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível consultar os inventários: {err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("v1/agendamento/disponibilidade"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ConsultarDisponibilidade([FromQuery] DispobilidadeSearchModel searchModel)
        {
            try
            {
                var result = await _condominioService.ConsultarDisponibilidadeCompativel(searchModel);
                return Ok(new ResultModel<List<SemanaDisponibilidadeModel>>(result?.Data ?? new List<SemanaDisponibilidadeModel>())
                {
                    Message = "",
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/agendamento/trocarsemana"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> TrocarSemana([FromBody] TrocaSemanaInputModel model)
        {
            try
            {
                var result = await _condominioService.TrocarSemana(model);
                return Ok(new ResultModel<int?>(result?.Data)
                {
                    Message = "",
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<int?>()
                {
                    Data = -1,
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<int?>()
                {
                    Data = -1,
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/agendamento/incluirsemana"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> IncluirSemana([FromBody] IncluirSemanaInputModel model)
        {
            try
            {
                var result = await _condominioService.IncluirSemana(model);
                return Ok(new ResultModel<int?>(result?.Data)
                {
                    Message = "",
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<int?>()
                {
                    Data = -1,
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<int?>()
                {
                    Data = -1,
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }
    }
}
