using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class EstadoController : ControllerBase
    {

        private readonly IStateService _stateService;

        public EstadoController(IStateService stateService)
        {
            _stateService = stateService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, state=W, Usuario")]
        [ProducesResponseType(typeof(ResultModel<EstadoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<EstadoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<EstadoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveState([FromBody] RegistroEstadoInputModel model)
        {
            try
            {
                var result = await _stateService.SaveState(model);
                return Ok(new ResultModel<EstadoModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<EstadoModel>(new EstadoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Estado: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Estado: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<EstadoModel>(new EstadoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Estado: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Estado: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, state=W, Usuario")]
        [ProducesResponseType(typeof(ResultModel<EstadoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<EstadoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<EstadoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateState([FromBody] AlteracaoEstadoInputModel model)
        {
            try
            {
                var result = await _stateService.UpdateState(model);
                return Ok(new ResultModel<EstadoModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<EstadoModel>(new EstadoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Estado: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Estado: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<EstadoModel>(new EstadoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Estado: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Estado: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, state=D")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteState([FromQuery] int id)
        {

            try
            {
                var result = await _stateService.DeleteState(id);
                return Ok(new DeleteResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Errors = new List<string>(),
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new DeleteResultModel()
                {
                    Result = "Não deletado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status404NotFound,
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new DeleteResultModel()
                {
                    Result = "Não deletado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DeleteResultModel()
                {
                    Result = "Não deletado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, state=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<EstadoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<EstadoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<EstadoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] EstadoSearchModel model)
        {
            try
            {
                var result = await _stateService.SearchState(model);
                if (result == null || !result.Any())
                    return NotFound(new ResultModel<List<EstadoSearchModel>>() { Data = new List<EstadoSearchModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<EstadoModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<EstadoModel>>()
                {
                    Data = new List<EstadoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<EstadoModel>>()
                {
                    Data = new List<EstadoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }
    }
}
