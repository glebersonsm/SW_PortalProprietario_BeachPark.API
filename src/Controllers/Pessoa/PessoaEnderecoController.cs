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
    public class PessoaEnderecoController : ControllerBase
    {

        private readonly IPessoaEnderecoService _pessoaenderecoService;

        public PessoaEnderecoController(IPessoaEnderecoService pessoaenderecoService)
        {
            _pessoaenderecoService = pessoaenderecoService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoaendereco=W, pessoaendereco=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<PessoaEnderecoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<PessoaEnderecoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<PessoaEnderecoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SavePessoaEndereco([FromBody] List<PessoaEnderecoInputModel> model)
        {
            try
            {
                var result = await _pessoaenderecoService.SalvarLista(model);
                return Ok(new ResultModel<List<PessoaEnderecoModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<PessoaEnderecoModel>(new PessoaEnderecoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o(s) Endereço(s): ({string.Join(",", model.Select(a => a.Logradouro).AsList())})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o(s) Endereço(s): ({string.Join(",", model.Select(a => a.Logradouro).AsList())})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<PessoaEnderecoModel>(new PessoaEnderecoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o(s) Endereço(s): ({string.Join(",", model.Select(a => a.Logradouro).AsList())})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o(s) Endereço(s): ({string.Join(",", model.Select(a => a.Logradouro).AsList())})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoaendereco=W, pessoaendereco=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<PessoaEnderecoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<PessoaEnderecoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<PessoaEnderecoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePessoaEndereco([FromBody] PessoaEnderecoInputModel model)
        {
            try
            {
                var result = await _pessoaenderecoService.Update(model);
                return Ok(new ResultModel<PessoaEnderecoModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<PessoaEnderecoModel>(new PessoaEnderecoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Endereço: ({model.Id} - {model.Logradouro})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Endereço: ({model.Id} - {model.Logradouro})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<PessoaEnderecoModel>(new PessoaEnderecoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Endereço: ({model.Id} - {model.Logradouro})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Endereço: ({model.Id} - {model.Logradouro})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoaendereco=D, pessoaendereco=*, Usuario")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePessoaEndereco([FromQuery] int id)
        {
            try
            {
                var result = await _pessoaenderecoService.Remover(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoaendereco=R, pessoaendereco=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<PessoaEnderecoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaEnderecoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaEnderecoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchPadraoComListaIdsModel model)
        {
            try
            {
                var result = await _pessoaenderecoService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<PessoaEnderecoModel>>() { Data = new List<PessoaEnderecoModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<PessoaEnderecoModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<PessoaEnderecoModel>>()
                {
                    Data = new List<PessoaEnderecoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<PessoaEnderecoModel>>()
                {
                    Data = new List<PessoaEnderecoModel>(),
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
