using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Pessoa
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class TipoDocumentoController : ControllerBase
    {

        private readonly ITipoDocumentoPessoaService _tipodocumentoService;

        public TipoDocumentoController(ITipoDocumentoPessoaService tipodocumentoService)
        {
            _tipodocumentoService = tipodocumentoService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipodocumentopessoa=W, tipodocumentopessoa=*")]
        [ProducesResponseType(typeof(ResultModel<TipoDocumentoPessoaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TipoDocumentoPessoaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TipoDocumentoPessoaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveTipoDocumento([FromBody] TipoDocumentoPessoaInputModel model)
        {
            try
            {
                var result = await _tipodocumentoService.Salvar(model);
                return Ok(new ResultModel<TipoDocumentoPessoaModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TipoDocumentoPessoaModel>(new TipoDocumentoPessoaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Tipo de Documento: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Tipo de Documento: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TipoDocumentoPessoaModel>(new TipoDocumentoPessoaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Tipo de Documento: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Tipo de Documento: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipodocumentopessoa=W, tipodocumentopessoa=*")]
        [ProducesResponseType(typeof(ResultModel<TipoDocumentoPessoaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TipoDocumentoPessoaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TipoDocumentoPessoaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTipoDocumento([FromBody] TipoDocumentoPessoaInputModel model)
        {
            try
            {
                var result = await _tipodocumentoService.Update(model);
                return Ok(new ResultModel<TipoDocumentoPessoaModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TipoDocumentoPessoaModel>(new TipoDocumentoPessoaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Tipo de Documento: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Tipo de Documento: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TipoDocumentoPessoaModel>(new TipoDocumentoPessoaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Tipo de Documento: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Tipo de Documento: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipodocumentopessoa=D, tipodocumentopessoa=*")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTipoDocumento([FromQuery] int id)
        {
            try
            {
                var result = await _tipodocumentoService.Remover(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, tipodocumentopessoa=R, tipodocumentopessoa=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<TipoDocumentoPessoaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<TipoDocumentoPessoaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<TipoDocumentoPessoaModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchPadraoComTipoPessoaModel model)
        {
            try
            {
                var result = await _tipodocumentoService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<TipoDocumentoPessoaModel>>() { Data = new List<TipoDocumentoPessoaModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<TipoDocumentoPessoaModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<TipoDocumentoPessoaModel>>()
                {
                    Data = new List<TipoDocumentoPessoaModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<TipoDocumentoPessoaModel>>()
                {
                    Data = new List<TipoDocumentoPessoaModel>(),
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
