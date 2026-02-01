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
    public class PessoaController : ControllerBase
    {

        private readonly IPessoaService _pessoafisicaService;

        public PessoaController(IPessoaService pessoafisicaService)
        {
            _pessoafisicaService = pessoafisicaService;
        }

        [HttpPost("savePessoaFisica"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoa=W, pessoa=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<PessoaCompletaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<PessoaCompletaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<PessoaCompletaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SavePessoaFisica([FromBody] PessoaFisicaInputModel model)
        {
            try
            {
                var result = await _pessoafisicaService.SalvarPessoaFisica(model);
                return Ok(new ResultModel<PessoaCompletaModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<PessoaCompletaModel>(new PessoaCompletaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Pessoa Física: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Pessoa Física: ( {model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<PessoaCompletaModel>(new PessoaCompletaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Pessoa Física: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Pessoa Física: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("savePessoaJuridica"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoa=W, pessoajuridica=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<PessoaCompletaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<PessoaCompletaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<PessoaCompletaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SavePessoaJuridica([FromBody] PessoaJuridicaInputModel model)
        {
            try
            {
                var result = await _pessoafisicaService.SalvarPessoaJuridica(model);
                return Ok(new ResultModel<PessoaCompletaModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<PessoaCompletaModel>(new PessoaCompletaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Pessoa Jurídica: ({model.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Pessoa Jurídica: ( {model.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<PessoaCompletaModel>(new PessoaCompletaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Pessoa Jurídica: ({model.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Pessoa Jurídica: ({model.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoa=D, pessoa=*, Usuario")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePessoaFisica([FromQuery] int id)
        {
            try
            {
                var result = await _pessoafisicaService.Remover(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, pessoa=R, pessoa=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<PessoaCompletaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaCompletaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<PessoaCompletaModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] PessoaSearchModel model)
        {
            try
            {
                var result = await _pessoafisicaService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<PessoaCompletaModel>>() { Data = new List<PessoaCompletaModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<PessoaCompletaModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<PessoaCompletaModel>>()
                {
                    Data = new List<PessoaCompletaModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<PessoaCompletaModel>>()
                {
                    Data = new List<PessoaCompletaModel>(),
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
