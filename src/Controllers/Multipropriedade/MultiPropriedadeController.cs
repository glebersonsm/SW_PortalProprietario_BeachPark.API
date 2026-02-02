using Dapper;
using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Interfaces.ReservasApi;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Multipropriedade
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class MultiPropriedadeController : ControllerBase
    {

        private readonly IEmpreendimentoProviderService _empreendimentoService;
        private readonly IReservaAgendamentoService _reservaService;

        public MultiPropriedadeController(IEmpreendimentoProviderService empreendimentoService, IReservaAgendamentoService reservaService)
        {
            _empreendimentoService = empreendimentoService;
            _reservaService = reservaService;
        }

        [HttpGet("searchImovel"), Authorize(Roles = "Administrador, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ImovelSimplificadoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ImovelSimplificadoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ImovelSimplificadoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchImovel([FromQuery] SearchImovelModel searchModel)
        {
            try
            {
                var result = await _empreendimentoService.GetImoveis(searchModel);
                if (result == null || !result.Value.imoveis.Any())
                    return Ok(new ResultWithPaginationModel<List<ImovelSimplificadoModel>>()
                    {
                        Data = new List<ImovelSimplificadoModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<ImovelSimplificadoModel>>(result.Value.imoveis.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<ImovelSimplificadoModel>>()
                {
                    Data = new List<ImovelSimplificadoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ImovelSimplificadoModel>>()
                {
                    Data = new List<ImovelSimplificadoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("searchProprietario"), Authorize(Roles = "Administrador, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchProprietario([FromQuery] SearchProprietarioModel searchModel)
        {
            try
            {
                var result = await _empreendimentoService.GetProprietarios(searchModel);
                if (result == null || !result.Value.proprietarios.Any())
                    return Ok(new ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>()
                    {
                        Data = new List<ProprietarioSimplificadoModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>(result.Value.proprietarios.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>()
                {
                    Data = new List<ProprietarioSimplificadoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>()
                {
                    Data = new List<ProprietarioSimplificadoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("consultarAgendamentosGerais"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarAgendamentosGerais([FromQuery] ReservasMultiPropriedadeSearchModel model)
        {

            try
            {
                var result = await _reservaService.ConsultarAgendamentosGerais(model);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultWithPaginationModel<List<SemanaModel>>(result.Data)
                    {
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        LastPageNumber = result.LastPageNumber,
                        PageNumber = result.PageNumber,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Errors = new List<string>() { "Nenhuma reserva foi encontrada" },
                    Status = StatusCodes.Status404NotFound,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("consultarReservasAgendamento"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarReservasByAgendamentoId(string agendamento)
        {

            try
            {
                var result = await _reservaService.ConsultarReservaByAgendamentoId(agendamento);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultModel<List<ReservaModel>>(result.Data)
                    {
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else return Ok(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Errors = new List<string>() { "Nenhuma reserva foi encontrada" },
                    Status = StatusCodes.Status404NotFound,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("agendamento/history/{agendamentoId}"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<AgendamentoHistoryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<AgendamentoHistoryModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<AgendamentoHistoryModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistorico(int agendamentoId)
        {

            try
            {
                var result = await _reservaService.ConsultarHistoricos(agendamentoId);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultModel<List<AgendamentoHistoryModel>>(result.Data)
                    {
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else return Ok(new ResultModel<List<AgendamentoHistoryModel>>(new List<AgendamentoHistoryModel>())
                {
                    Errors = new List<string>() { "Nenhum histórico encontrado" },
                    Status = StatusCodes.Status404NotFound,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<List<AgendamentoHistoryModel>>(new List<AgendamentoHistoryModel>())
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<AgendamentoHistoryModel>>(new List<AgendamentoHistoryModel>())
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<AgendamentoHistoryModel>>(new List<AgendamentoHistoryModel>())
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("editarReserva"), Authorize(Roles = "Administrador, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<List<ReservaForEditModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaForEditModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaForEditModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditarReserva(int reservaId)
        {

            try
            {
                var result = await _reservaService.EditarReserva(reservaId);
                if (result != null && result.Success)
                    return Ok(result);
                else return Ok(new ResultModel<ReservaForEditModel>(new ReservaForEditModel())
                {
                    Errors = new List<string>() { "Nenhuma reserva foi encontrada" },
                    Status = StatusCodes.Status404NotFound,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<ReservaForEditModel>(new ReservaForEditModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ReservaForEditModel>(new ReservaForEditModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ReservaForEditModel>(new ReservaForEditModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("cancelarReservaAgendamento"), Authorize(Roles = "Administrador, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelarReservaAgendamento([FromBody] CancelamentoReservaAgendamentoModel model)
        {

            try
            {
                var result = await _reservaService.CancelarReservaAgendamento(model);
                if (result != null)
                {
                    if (result.Success)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        result.Errors = new List<string>() { result.Message };
                        return BadRequest(result);
                    }
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = result.Message != null && !string.IsNullOrEmpty(result.Message) ? new List<string>() { result?.Message } : new List<string>() { "Operação não realizada" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/agendamento/liberarSemanaPool"), Authorize(Roles = "Administrador, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> LiberarSemanaPool([FromBody] LiberacaoAgendamentoInputModel modelAgendamentoPool)
        {
            try
            {
                var result = await _reservaService.LiberarSemanaPool(modelAgendamentoPool);
                if (result.Success)
                    return Ok(result);
                else throw new ArgumentException(result.Message);
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>()
                {
                    Data = false,
                    Errors = new List<string>() { err.Message, err.InnerException?.Message },
                    Message = err.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<bool>()
                {
                    Data = false,
                    Errors = new List<string>() { err.Message, err.InnerException?.Message },
                    Message = err.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/agendamento/retirarSemanaPool"), Authorize(Roles = "Administrador, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]

        public async Task<IActionResult> RetirarSemanaPool([FromBody] AgendamentoInventarioModel modelAgendamentoPool)
        {
            try
            {
                var result = await _reservaService.RetirarSemanaPool(modelAgendamentoPool);
                if (result.Success)
                    return Ok(result);
                else throw new ArgumentException(result.Message);
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>()
                {
                    Data = false,
                    Errors = new List<string>() { err.Message, err.InnerException?.Message },
                    Message = err.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<bool>()
                {
                    Data = false,
                    Errors = new List<string>() { err.Message, err.InnerException?.Message },
                    Message = err.Message,
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
                var result = await _reservaService.ConsultarInventarios(searchModel);
                return Ok(result);
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<InventarioModel>>()
                {
                    Data = new List<InventarioModel>(),
                    Message = err.InnerException != null ?
                        $"Não foi possível consultar os inventários {err.Message} {err.InnerException.Message}" :
                        $"Não foi possível consultar os inventários" +
                        $" {err.Message}",
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
                    $"Não foi possível consultar os inventários {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível consultar os inventários {err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("v1/disponibilidadeparatroca"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ConsultarDisponibilidade([FromQuery] DispobilidadeSearchModel searchModel)
        {
            try
            {
                var result = await _empreendimentoService.ConsultarDisponibilidadeCompativel(searchModel);
                if (result != null)
                    return Ok(result);
                else return NotFound(new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Status = StatusCodes.Status404NotFound,
                    Success = false,
                    Message = "Ops! não foi encontrado nenhum registro.",
                    Errors = new List<string>() { "Ops! não foi encontrado nenhum registro." }
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = err.InnerException != null ?
                        $"Não foi possível consultar a disponibilidade: {err.Message} {err.InnerException.Message}" :
                        $"Não foi possível consultar a disponibilidade: " +
                        $" {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = err.InnerException != null ?
                    $"Não foi possível consultar a disponibilidade: {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível consultar a disponibilidade: {err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPost("v1/trocarminhasemana"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> TrocarSemana([FromBody] TrocaSemanaInputModel model)
        {
            try
            {
                var result = await _empreendimentoService.TrocarSemana(model);
                if (result != null && result.Success)
                    return Ok(result);
                else return BadRequest(result);
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<int?>()
                {
                    Data = -1,
                    Message = err.InnerException != null ?
                        $"Não foi possível trocar a semana: {err.Message} {err.InnerException.Message}" :
                        $"Não foi possível trocar a semana: " +
                        $" {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = err.InnerException != null ?
                    $"Não foi possível trocar a semana: {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível trocar a semana: {err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/incluirsemana"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> IncluirSemana([FromBody] IncluirSemanaInputModel model)
        {
            try
            {
                var result = await _empreendimentoService.IncluirSemana(model);
                if (result != null && result.Success)
                    return Ok(result);
                else return BadRequest(result);
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<int?>()
                {
                    Data = -1,
                    Message = err.InnerException != null ?
                        $"Não foi possível incluir a semana: {err.Message} {err.InnerException.Message}" :
                        $"Não foi possível incluir a semana: " +
                        $" {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = err.InnerException != null ?
                    $"Não foi possível incluir a semana: {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível incluir a semana: {err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpGet("downloadContratoSCP"), Authorize(Roles = "Administrador, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DownloadResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DownloadResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadContratoSCP([FromQuery] int cotaId)
        {
            try
            {
                var result = await _empreendimentoService.DownloadContratoSCP(cotaId);
                if (result != null && !string.IsNullOrEmpty(result.Path))
                {
                    var ext = SW_PortalProprietario.Application.Functions.FileUtils.ObterTipoMIMEPorExtensao(string.Concat(".", result.Path.Split("\\").Last().Split(".").Last()));
                    if (string.IsNullOrEmpty(ext))
                        throw new Exception($"Tipo de arquivo: ({result.Path.Split("\\").Last().Split(".").Last()}) não suportado.");

                    var memory = new MemoryStream();
                    using var stream = new FileStream(result.Path, FileMode.Open);
                    await stream.CopyToAsync(memory);

                    memory.Position = 0;
                    return File(memory, ext, Path.GetFileName(result.Path));
                }
                else
                {
                    return NotFound(new DownloadResultModel()
                    {
                        Result = "Não baixado",
                        Errors = new List<string>() { "Contrato não encontrado" },
                        Status = StatusCodes.Status404NotFound,
                    });

                }

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new DownloadResultModel()
                {
                    Result = "Não baixado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status404NotFound,
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new DownloadResultModel()
                {
                    Result = "Não baixado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DownloadResultModel()
                {
                    Result = "Não baixado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("statusCrc"), Authorize(Roles = "Administrador, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<SW_PortalProprietario.Application.Models.Empreendimento.StatusCrcModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<SW_PortalProprietario.Application.Models.Empreendimento.StatusCrcModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StatusCrc()
        {
            try
            {
                var result = await _empreendimentoService.ConsultarStatusCrc();
                return base.Ok(new SW_PortalProprietario.Application.Models.ResultModel<List<SW_PortalProprietario.Application.Models.Empreendimento.StatusCrcModel>>((List<SW_PortalProprietario.Application.Models.Empreendimento.StatusCrcModel>?)result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return base.BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<List<SW_PortalProprietario.Application.Models.Empreendimento.StatusCrcModel>>()
                {
                    Data = new List<SW_PortalProprietario.Application.Models.Empreendimento.StatusCrcModel>(),
                    Message = err.InnerException != null ?
                        $"Não foi possível consultar os status crc: {err.Message} {err.InnerException.Message}" :
                        $"Não foi possível consultar os status crc: " +
                        $" {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return base.StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<List<SW_PortalProprietario.Application.Models.Empreendimento.StatusCrcModel>>()
                {
                    Data = new List<SW_PortalProprietario.Application.Models.Empreendimento.StatusCrcModel>(),
                    Message = err.InnerException != null ?
                    $"Não foi possível consultar os status crc: {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível consultar os status crc: {err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

    }
}
