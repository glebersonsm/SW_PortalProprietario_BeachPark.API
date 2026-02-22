using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Interfaces.Esol;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Esol
{
    /// <summary>
    /// Controller de reservas - migrado do SwReservaApiMain.
    /// Sufixo Esol para evitar conflitos.
    /// </summary>
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ReservaEsolController : ControllerBase
    {
        private readonly IReservaEsolService _reservaService;
        private readonly ILogger<ReservaEsolController> _logger;

        public ReservaEsolController(IReservaEsolService reservaService, ILogger<ReservaEsolController> logger)
        {
            _reservaService = reservaService;
            _logger = logger;
        }

        [HttpGet("v1/consultarReservasAgendamento"), Authorize(Roles = "*, Administrador")]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarReservasAgendamento([FromQuery] string agendamento = "")
        {
            try
            {
                var result = await _reservaService.ConsultarReservaByAgendamentoId(agendamento);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultModel<List<ReservaModel>>(result.Data)
                    {
                        Message = "",
                        Success = true,
                        Status = StatusCodes.Status200OK
                    });
                return NotFound(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = "Nenhuma reserva foi encontrada",
                    Success = false,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("v1/consultarMinhasReservasAgendamento"), Authorize(Roles = "*, Administrador, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarMinhasReservasAgendamento([FromQuery] string agendamento = "")
        {
            try
            {
                var result = await _reservaService.ConsultarMinhasReservaByAgendamentoId(agendamento);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultModel<List<ReservaModel>>(result.Data)
                    {
                        Message = "",
                        Success = true,
                        Status = StatusCodes.Status200OK
                    });
                return NotFound(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = "Nenhuma reserva foi encontrada",
                    Success = false,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("v1/consultarReservasGeral"), Authorize(Roles = "*, Administrador, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarReservasGeralAgendamento([FromQuery] ReservasMultiPropriedadeSearchModel model)
        {
            try
            {
                var result = await _reservaService.ConsultarGeralReserva(model);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultWithPaginationModel<List<ReservaModel>>(result.Data)
                    {
                        Message = "",
                        Success = true,
                        Status = StatusCodes.Status200OK,
                        NumberRecords = result.NumberRecords,
                        LastPageNumber = result.LastPageNumber,
                        PageNumber = result.PageNumber
                    });
                return NotFound(new ResultWithPaginationModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = "Nenhuma reserva foi encontrada",
                    Success = false,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultWithPaginationModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultWithPaginationModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("v1/efetuarReservasAgendamento"), Authorize(Roles = "*, Administrador, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultModel<ReservaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ReservaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EfetuarReservasAgendamento([FromBody] CriacaoReservaAgendamentoInputModel model)
        {
            try
            {
                var result = await _reservaService.SalvarReservaEmAgendamento(model);
                if (result != null && result.Success && result.Data > 0)
                {
                    return Ok(new ResultModel<int>(result.Data)
                    {
                        Success = true,
                        Status = StatusCodes.Status200OK,
                    });
                }
                throw new ArgumentException(result?.Message ?? "Não foi possível salvar a reserva");
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ReservaModel>(new ReservaModel())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível salvar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível salvar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ReservaModel>(new ReservaModel())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível salvar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível salvar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("v1/editarReserva"), Authorize(Roles = "*, Administrador")]
        [ProducesResponseType(typeof(ResultModel<ReservaForEditModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<ReservaForEditModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ReservaForEditModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditReservaAgendamento([FromQuery] int id)
        {
            try
            {
                var result = await _reservaService.EditarReserva(id);
                if (result != null && result.Success)
                    return Ok(new ResultModel<ReservaForEditModel>(result.Data!) { Status = StatusCodes.Status200OK, Success = true });
                throw new ArgumentException(result?.Message ?? "Não foi possível localizar a reserva");
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ReservaModel>(new ReservaModel())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível localizar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível localizar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ReservaModel>(new ReservaModel())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível localizar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível localizar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("v1/editarMinhaReserva"), Authorize(Roles = "*, Administrador, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<ReservaForEditModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<ReservaForEditModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ReservaForEditModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditMinhaReservaAgendamento([FromQuery] int id)
        {
            try
            {
                var result = await _reservaService.EditarMinhaReserva(id);
                if (result != null && result.Success)
                    return Ok(new ResultModel<ReservaForEditModel>(result.Data!) { Status = StatusCodes.Status200OK, Success = true });
                throw new ArgumentException(result?.Message ?? "Não foi possível localizar a reserva");
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ReservaModel>(new ReservaModel())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível localizar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível localizar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ReservaModel>(new ReservaModel())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível localizar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível localizar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("v1/getDadosImpressaoVoucher"), Authorize(Roles = "*, Administrador, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<DadosImpressaoVoucherResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<DadosImpressaoVoucherResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<DadosImpressaoVoucherResultModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDadosImpresaoVoucher([FromQuery] string agendamentoId)
        {
            try
            {
                var result = await _reservaService.GetDadosImpressaoVoucher(agendamentoId);
                if (result != null)
                    return Ok(new ResultModel<DadosImpressaoVoucherResultModel>(result) { Status = StatusCodes.Status200OK, Success = true });
                throw new ArgumentException("Não foi possível retornar os dados");
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<DadosImpressaoVoucherResultModel>(new DadosImpressaoVoucherResultModel())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível retornar os dados - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível retornar os dados - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<DadosImpressaoVoucherResultModel>(new DadosImpressaoVoucherResultModel())
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível retornar os dados - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível retornar os dados - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("v1/cancelarReservaAgendamento"), Authorize(Roles = "*, Administrador")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelarReservaAgendamento([FromBody] CancelamentoReservaAgendamentoModel model)
        {
            try
            {
                var result = await _reservaService.CancelarReservaAgendamento(model);
                if (result == null || !result.Success)
                    return NotFound("Não foi encontrado reserva com o Id " + model.ReservaId);
                return Ok(new ResultModel<bool>(true) { Status = StatusCodes.Status200OK, Success = true });
            }
            catch (ArgumentException err)
            {
                var result = new ResultModel<bool>(false)
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível cancelar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível cancelar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                };
                return BadRequest(result);
            }
            catch (Exception err)
            {
                var result = new ResultModel<bool>(false)
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível cancelar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível cancelar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                };
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }

        [HttpPost("v1/cancelarMinhaReservaAgendamento"), Authorize(Roles = "*, Administrador, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelarMinhaReservaAgendamento([FromBody] CancelamentoReservaAgendamentoModel model)
        {
            try
            {
                var result = await _reservaService.CancelarMinhaReservaAgendamento(model);
                if (result == null || !result.Success)
                    return NotFound("Não foi encontrado reserva com o Id " + model.ReservaId);
                return Ok(new ResultModel<bool>(true) { Status = StatusCodes.Status200OK, Success = true });
            }
            catch (ArgumentException err)
            {
                var result = new ResultModel<bool>(false)
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível cancelar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível cancelar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                };
                return BadRequest(result);
            }
            catch (Exception err)
            {
                var result = new ResultModel<bool>(false)
                {
                    Message = err.InnerException != null ?
                    $"Não foi possível cancelar a reserva - {err.Message} - {err.InnerException?.Message}" :
                    $"Não foi possível cancelar a reserva - {err.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                };
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }
    }
}
