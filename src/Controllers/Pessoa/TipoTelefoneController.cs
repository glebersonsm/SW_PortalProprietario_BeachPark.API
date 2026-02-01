using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers.Pessoa
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class TipoTelefoneController : ControllerBase
    {

        private readonly ITipoTelefoneService _telefoneService;

        public TipoTelefoneController(ITipoTelefoneService telefoneService)
        {
            _telefoneService = telefoneService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipotelefone=W, tipotelefone=*")]
        [ProducesResponseType(typeof(ResultModel<TipoTelefoneModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TipoTelefoneModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TipoTelefoneModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveTipoTelefone([FromBody] TipoTelefoneInputModel model)
        {
            try
            {
                var result = await _telefoneService.Salvar(model);
                return Ok(new ResultModel<TipoTelefoneModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TipoTelefoneModel>(new TipoTelefoneModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Ação: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Ação: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TipoTelefoneModel>(new TipoTelefoneModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Ação: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Ação: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipotelefone=W, tipotelefone=*")]
        [ProducesResponseType(typeof(ResultModel<TipoTelefoneModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TipoTelefoneModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TipoTelefoneModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTipoTelefone([FromBody] TipoTelefoneInputModel model)
        {
            try
            {
                var result = await _telefoneService.Update(model);
                return Ok(new ResultModel<TipoTelefoneModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TipoTelefoneModel>(new TipoTelefoneModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Ação: ({model.Id} - {model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Ação: ({model.Id} - {model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TipoTelefoneModel>(new TipoTelefoneModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Ação: ({model.Id} - {model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Ação: ({model.Id} - {model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipotelefone=D, tipotelefone=*")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTipoTelefone([FromQuery] int id)
        {
            try
            {
                var result = await _telefoneService.Remover(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipotelefone=R, tipotelefone=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<TipoTelefoneModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<TipoTelefoneModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<TipoTelefoneModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchPadraoModel model)
        {
            try
            {
                var result = await _telefoneService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<TipoTelefoneModel>>() { Data = new List<TipoTelefoneModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<TipoTelefoneModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<TipoTelefoneModel>>()
                {
                    Data = new List<TipoTelefoneModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<TipoTelefoneModel>>()
                {
                    Data = new List<TipoTelefoneModel>(),
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
