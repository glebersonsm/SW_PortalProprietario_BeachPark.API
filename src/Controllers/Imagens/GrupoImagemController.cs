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
    public class GrupoImagemController : ControllerBase
    {

        private readonly IImageGroupService _imageGroupService;

        public GrupoImagemController(
            IImageGroupService imageGroupService)
        {
            _imageGroupService = imageGroupService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagegroup=W")]
        [ProducesResponseType(typeof(ResultModel<ImageGroupModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<ImageGroupModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ImageGroupModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveImageGroup([FromBody] ImageGroupInputModel model)
        {
            try
            {
                var result = await _imageGroupService.SaveImageGroup(model);
                return Ok(new ResultModel<ImageGroupModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ImageGroupModel>(new ImageGroupModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Grupo de Imagem: ({model.Name})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Grupo de Imagem: ({model.Name})", err.Message },
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ImageGroupModel>(new ImageGroupModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Grupo de Imagem: ({model.Name})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Grupo de Imagem: ({model.Name})", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagegroup=D")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteImageGroup([FromQuery] int id)
        {
            try
            {
                var result = await _imageGroupService.DeleteImageGroup(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagegroup=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<ImageGroupModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<ImageGroupModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ImageGroupModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchImageGroupModel model)
        {
            try
            {

                var result = await _imageGroupService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<ImageGroupModel>>()
                    {
                        Data = new List<ImageGroupModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<ImageGroupModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ImageGroupModel>>()
                {
                    Data = new List<ImageGroupModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<ImageGroupModel>>()
                {
                    Data = new List<ImageGroupModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("reorder"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, imagegroup=W")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReorderGroups([FromBody] List<ReorderImageGroupModel> groups)
        {
            try
            {
                await _imageGroupService.ReorderGroups(groups);
                return Ok();
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<object>()
                {
                    Errors = new List<string>() { "Erro ao reordenar grupos de imagem", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }
    }
}
