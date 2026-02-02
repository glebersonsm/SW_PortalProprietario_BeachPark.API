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
    public class PessoaDocumentoController : ControllerBase
    {

        private readonly IPessoaDocumentoService _pessoadocumentoService;

        public PessoaDocumentoController(IPessoaDocumentoService pessoadocumentoService)
        {
            _pessoadocumentoService = pessoadocumentoService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoadocumento=W, pessoadocumento=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<PessoaDocumentoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaDocumentoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaDocumentoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SavePessoaDocumento([FromBody] List<PessoaDocumentoInputModel> model)
        {
            try
            {
                var result = await _pessoadocumentoService.SalvarLista(model);
                return Ok(new ResultModel<List<PessoaDocumentoModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<PessoaDocumentoModel>>(new List<PessoaDocumentoModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o(s) Documentos(s): ({string.Join(",", model.Select(a => a.Numero).AsList())})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o(s) Documentos(s): ({string.Join(",", model.Select(a => a.Numero).AsList())})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<PessoaDocumentoModel>>(new List<PessoaDocumentoModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o(s) Documentos(s): ({string.Join(",", model.Select(a => a.Numero).AsList())})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o(s) Documentos(s): ({string.Join(",", model.Select(a => a.Numero).AsList())})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPatch, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoadocumento=W, pessoadocumento=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<PessoaDocumentoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<PessoaDocumentoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<PessoaDocumentoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePessoaDocumento([FromBody] PessoaDocumentoInputModel model)
        {
            try
            {
                var result = await _pessoadocumentoService.Update(model);
                return Ok(new ResultModel<PessoaDocumentoModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<PessoaDocumentoModel>(new PessoaDocumentoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Id} - {model.Numero})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Id} - {model.Numero})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<PessoaDocumentoModel>(new PessoaDocumentoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Id} - {model.Numero})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Id} - {model.Numero})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoadocumento=D, pessoadocumento=*, Usuario")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePessoaDocumento([FromQuery] int id)
        {
            try
            {
                var result = await _pessoadocumentoService.Remover(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoadocumento=R, pessoadocumento=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<PessoaDocumentoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaDocumentoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaDocumentoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchPadraoComListaIdsModel model)
        {
            try
            {
                var result = await _pessoadocumentoService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<PessoaDocumentoModel>>() { Data = new List<PessoaDocumentoModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<PessoaDocumentoModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<PessoaDocumentoModel>>()
                {
                    Data = new List<PessoaDocumentoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<PessoaDocumentoModel>>()
                {
                    Data = new List<PessoaDocumentoModel>(),
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
