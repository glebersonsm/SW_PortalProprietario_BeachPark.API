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
    public class TipoEnderecoController : ControllerBase
    {

        private readonly ITipoEnderecoService _enderecoService;

        public TipoEnderecoController(ITipoEnderecoService enderecoService)
        {
            _enderecoService = enderecoService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipoendereco=W, tipoendereco=*")]
        [ProducesResponseType(typeof(ResultModel<TipoEnderecoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TipoEnderecoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TipoEnderecoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveTipoEndereco([FromBody] TipoEnderecoInputModel model)
        {
            try
            {
                var result = await _enderecoService.Salvar(model);
                return Ok(new ResultModel<TipoEnderecoModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TipoEnderecoModel>(new TipoEnderecoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Tipo Endereço: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Tipo Endereço: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TipoEnderecoModel>(new TipoEnderecoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Tipo Endereço: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Tipo Endereço: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipoendereco=W, tipoendereco=*")]
        [ProducesResponseType(typeof(ResultModel<TipoEnderecoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TipoEnderecoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TipoEnderecoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTipoEndereco([FromBody] TipoEnderecoInputModel model)
        {
            try
            {
                var result = await _enderecoService.Update(model);
                return Ok(new ResultModel<TipoEnderecoModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TipoEnderecoModel>(new TipoEnderecoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Tipo Endereço: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Tipo Endereço: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TipoEnderecoModel>(new TipoEnderecoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Tipo Endereço: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Tipo Endereço: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipoendereco=D, tipoendereco=*")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTipoEndereco([FromQuery] int id)
        {
            try
            {
                var result = await _enderecoService.Remover(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipoendereco=R, tipoendereco=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<TipoEnderecoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<TipoEnderecoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<TipoEnderecoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchPadraoModel model)
        {
            try
            {
                var result = await _enderecoService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<TipoEnderecoModel>>() { Data = new List<TipoEnderecoModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<TipoEnderecoModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<TipoEnderecoModel>>()
                {
                    Data = new List<TipoEnderecoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<TipoEnderecoModel>>()
                {
                    Data = new List<TipoEnderecoModel>(),
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
