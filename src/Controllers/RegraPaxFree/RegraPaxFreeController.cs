using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers.RegraPaxFree
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class RegraPaxFreeController : ControllerBase
    {
        private readonly IRegraPaxFreeService _regraPaxFreeService;

        public RegraPaxFreeController(IRegraPaxFreeService regraPaxFreeService)
        {
            _regraPaxFreeService = regraPaxFreeService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<RegraPaxFreeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RegraPaxFreeModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<RegraPaxFreeModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveRegraPaxFree([FromBody] RegraPaxFreeInputModel model)
        {
            try
            {
                var result = await _regraPaxFreeService.SaveRegraPaxFree(model);
                return Ok(new ResultModel<RegraPaxFreeModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<RegraPaxFreeModel>(new RegraPaxFreeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a RegraPaxFree: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a RegraPaxFree: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RegraPaxFreeModel>(new RegraPaxFreeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a RegraPaxFree: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a RegraPaxFree: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<RegraPaxFreeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RegraPaxFreeModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<RegraPaxFreeModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AlterarRegraPaxFree([FromBody] AlteracaoRegraPaxFreeInputModel model)
        {
            try
            {
                var result = await _regraPaxFreeService.UpdateRegraPaxFree(model);
                return Ok(new ResultModel<RegraPaxFreeModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<RegraPaxFreeModel>(new RegraPaxFreeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a RegraPaxFree: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a RegraPaxFree: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RegraPaxFreeModel>(new RegraPaxFreeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a RegraPaxFree: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a RegraPaxFree: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRegraPaxFree([FromQuery] int id)
        {
            try
            {
                var result = await _regraPaxFreeService.DeleteRegraPaxFree(id);
                return Ok(new DeleteResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Errors = new List<string>(),
                    Result = "Removido com sucesso!"
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<RegraPaxFreeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<RegraPaxFreeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<RegraPaxFreeModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchPadraoModel model)
        {
            try
            {
                var result = await _regraPaxFreeService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<RegraPaxFreeModel>>()
                    {
                        Data = new List<RegraPaxFreeModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<RegraPaxFreeModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<RegraPaxFreeModel>>()
                {
                    Data = new List<RegraPaxFreeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<RegraPaxFreeModel>>()
                {
                    Data = new List<RegraPaxFreeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}

