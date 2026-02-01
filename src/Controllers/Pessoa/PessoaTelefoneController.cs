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
    public class PessoaTelefoneController : ControllerBase
    {

        private readonly IPessoaTelefoneService _pessoatelefoneService;

        public PessoaTelefoneController(IPessoaTelefoneService pessoatelefoneService)
        {
            _pessoatelefoneService = pessoatelefoneService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoatelefone=W, pessoatelefone=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<PessoaTelefoneModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaTelefoneModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaTelefoneModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SavePessoaTelefoneList([FromBody] List<PessoaTelefoneInputModel> model)
        {
            try
            {
                var result = await _pessoatelefoneService.SalvarLista(model);
                return Ok(new ResultModel<List<PessoaTelefoneModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<PessoaTelefoneModel>>(new List<PessoaTelefoneModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o(s) Telefone(s): ({string.Join(",", model.Select(a => a.Numero).AsList())})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o(s) Telefone(s): ({string.Join(",", model.Select(a => a.Numero).AsList())})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<PessoaTelefoneModel>>(new List<PessoaTelefoneModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o(s) Telefone(s): ({string.Join(",", model.Select(a => a.Numero).AsList())})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o(s) Telefone(s): ({string.Join(",", model.Select(a => a.Numero).AsList())})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoatelefone=W, pessoatelefone=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<PessoaTelefoneModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<PessoaTelefoneModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<PessoaTelefoneModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePessoaTelefone([FromBody] PessoaTelefoneInputModel model)
        {
            try
            {
                var result = await _pessoatelefoneService.Update(model);
                return Ok(new ResultModel<PessoaTelefoneModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<PessoaTelefoneModel>(new PessoaTelefoneModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Telefone: ({model.Id} - {model.Numero})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Telefone: ({model.Id} - {model.Numero})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<PessoaTelefoneModel>(new PessoaTelefoneModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Telefone: ({model.Id} - {model.Numero})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Telefone: ({model.Id} - {model.Numero})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoatelefone=D, pessoatelefone=*, Usuario")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePessoaTelefone([FromQuery] int id)
        {
            try
            {
                var result = await _pessoatelefoneService.Remover(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoatelefone=R, pessoatelefone=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<PessoaTelefoneModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaTelefoneModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaTelefoneModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchPadraoComListaIdsModel model)
        {
            try
            {
                var result = await _pessoatelefoneService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<PessoaTelefoneModel>>() { Data = new List<PessoaTelefoneModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<PessoaTelefoneModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<PessoaTelefoneModel>>()
                {
                    Data = new List<PessoaTelefoneModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<PessoaTelefoneModel>>()
                {
                    Data = new List<PessoaTelefoneModel>(),
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
