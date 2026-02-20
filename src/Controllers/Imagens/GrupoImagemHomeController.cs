using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Imagens
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class GrupoImagemHomeController : ControllerBase
    {
        private readonly IGrupoImagemHomeService _grupoImagemHomeService;

        public GrupoImagemHomeController(IGrupoImagemHomeService grupoImagemHomeService)
        {
            _grupoImagemHomeService = grupoImagemHomeService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, grupoimagemhome=W")]
        [ProducesResponseType(typeof(ResultModel<GrupoImagemHomeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<GrupoImagemHomeModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<GrupoImagemHomeModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveGrupoImagemHome([FromBody] GrupoImagemHomeInputModel model)
        {
            try
            {
                var result = await _grupoImagemHomeService.SaveGrupoImagemHome(model);
                return Ok(new ResultModel<GrupoImagemHomeModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<GrupoImagemHomeModel>(new GrupoImagemHomeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar o Grupo de Imagem Home: ({model.Name})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar o Grupo de Imagem Home: ({model.Name})", err.Message },
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<GrupoImagemHomeModel>(new GrupoImagemHomeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar o Grupo de Imagem Home: ({model.Name})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar o Grupo de Imagem Home: ({model.Name})", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, grupoimagemhome=D")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteGrupoImagemHome([FromQuery] int id)
        {
            try
            {
                var result = await _grupoImagemHomeService.DeleteGrupoImagemHome(id);
                return Ok(new DeleteResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Errors = new List<string>(),
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new DeleteResultModel()
                {
                    Result = "NÃ£o deletado",
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
                    Result = "NÃ£o deletado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, grupoimagemhome=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<GrupoImagemHomeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<GrupoImagemHomeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<GrupoImagemHomeModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchGrupoImagemHomeModel model)
        {
            try
            {
                var result = await _grupoImagemHomeService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<GrupoImagemHomeModel>>()
                    {
                        Data = new List<GrupoImagemHomeModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<GrupoImagemHomeModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<GrupoImagemHomeModel>>()
                {
                    Data = new List<GrupoImagemHomeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<GrupoImagemHomeModel>>()
                {
                    Data = new List<GrupoImagemHomeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("reorder"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, grupoimagemhome=W")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReorderGroups([FromBody] List<ReorderImageGroupModel> groups)
        {
            try
            {
                await _grupoImagemHomeService.ReorderGroups(groups);
                return Ok();
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<object>()
                {
                    Errors = new List<string>() { "Erro ao reordenar grupos de imagens", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }
    }
}

