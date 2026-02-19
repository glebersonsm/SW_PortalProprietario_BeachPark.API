using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using System.Security.Claims;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Imagens
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ImagemGrupoImagemHomeController : ControllerBase
    {
        private readonly IImagemGrupoImagemHomeService _imagemGrupoImagemHomeService;

        public ImagemGrupoImagemHomeController(IImagemGrupoImagemHomeService imagemGrupoImagemHomeService)
        {
            _imagemGrupoImagemHomeService = imagemGrupoImagemHomeService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagengrupoimagemhome=W")]
        [ProducesResponseType(typeof(ResultModel<ImagemGrupoImagemHomeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<ImagemGrupoImagemHomeModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ImagemGrupoImagemHomeModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveImagem([FromForm] ImagemGrupoImagemHomeInputModel model)
        {
            try
            {
                var result = await _imagemGrupoImagemHomeService.SaveImagem(model);
                return Ok(new ResultModel<ImagemGrupoImagemHomeModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ImagemGrupoImagemHomeModel>(new ImagemGrupoImagemHomeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar a imagem: ({model.Name})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar a imagem: ({model.Name})", err.Message },
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ImagemGrupoImagemHomeModel>(new ImagemGrupoImagemHomeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar a imagem: ({model.Name})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar a imagem: ({model.Name})", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagengrupoimagemhome=D")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteImagem([FromQuery] int id)
        {
            try
            {
                var result = await _imagemGrupoImagemHomeService.DeleteImagem(id);
                return Ok(new DeleteResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Errors = new List<string>()
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, imagengrupoimagemhome=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchImagemGrupoImagemHomeModel model)
        {
            try
            {
                var result = await _imagemGrupoImagemHomeService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                    {
                        Data = new List<ImagemGrupoImagemHomeModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<ImagemGrupoImagemHomeModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                {
                    Data = new List<ImagemGrupoImagemHomeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                {
                    Data = new List<ImagemGrupoImagemHomeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("searchForHome"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchForHome()
        {
            try
            {
                var result = await _imagemGrupoImagemHomeService.SearchForHome();
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                    {
                        Data = new List<ImagemGrupoImagemHomeModel>(),
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else return Ok(new ResultModel<List<ImagemGrupoImagemHomeModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                {
                    Data = new List<ImagemGrupoImagemHomeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                {
                    Data = new List<ImagemGrupoImagemHomeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("searchForHomePublic")]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ImagemGrupoImagemHomeModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchForHomePublic()
        {
            try
            {
                var result = await _imagemGrupoImagemHomeService.SearchForHomePublic();
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                    {
                        Data = new List<ImagemGrupoImagemHomeModel>(),
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else return Ok(new ResultModel<List<ImagemGrupoImagemHomeModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                {
                    Data = new List<ImagemGrupoImagemHomeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<ImagemGrupoImagemHomeModel>>()
                {
                    Data = new List<ImagemGrupoImagemHomeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("reorder"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, imagengrupoimagemhome=W")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReorderImages([FromBody] List<ReorderImageModel> images)
        {
            try
            {
                await _imagemGrupoImagemHomeService.ReorderImages(images);
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

