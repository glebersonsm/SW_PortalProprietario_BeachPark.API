using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Imagens
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ImagemGrupoImagemController : ControllerBase
    {

        private readonly IImageGroupImageService _imageGroupImageService;

        public ImagemGrupoImagemController(
            IImageGroupImageService imageGroupImageService)
        {
            _imageGroupImageService = imageGroupImageService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagegroupimage=W")]
        [ProducesResponseType(typeof(ResultModel<ImageGroupImageModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<ImageGroupImageModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ImageGroupImageModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveImage([FromForm] ImageGroupImageInputModel model)
        {
            try
            {
                var result = await _imageGroupImageService.SaveImage(model);
                return Ok(new ResultModel<ImageGroupImageModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ImageGroupImageModel>(new ImageGroupImageModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a imagem: ({model.Name})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a imagem: ({model.Name})", err.Message },
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ImageGroupImageModel>(new ImageGroupImageModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a imagem: ({model.Name})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a imagem: ({model.Name})", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagegroupimage=D")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteImage([FromQuery] int id)
        {
            try
            {
                var result = await _imageGroupImageService.DeleteImage(id);
                return Ok(new DeleteResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Errors = new List<string>()
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


        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagegroupimage=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<ImageGroupImageModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<ImageGroupImageModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ImageGroupImageModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchImageGroupImageModel model)
        {
            try
            {
                var result = await _imageGroupImageService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<ImageGroupImageModel>>()
                    {
                        Data = new List<ImageGroupImageModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<ImageGroupImageModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ImageGroupImageModel>>()
                {
                    Data = new List<ImageGroupImageModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<ImageGroupImageModel>>()
                {
                    Data = new List<ImageGroupImageModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("reorder"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, imagegroupimage=W")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReorderImages([FromBody] List<ReorderImageModel> images)
        {
            try
            {
                await _imageGroupImageService.ReorderImages(images);
                return Ok();
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<object>()
                {
                    Errors = new List<string>() { "Erro ao reordenar imagens", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

    }
}
