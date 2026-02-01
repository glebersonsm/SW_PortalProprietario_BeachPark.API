using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class EmpresaController : ControllerBase
    {

        private readonly ILogger<EmpresaController> _logger;
        private readonly IFrameworkService _frameworkService;

        public EmpresaController(ILogger<EmpresaController> logger,
            IFrameworkService frameworkService)
        {
            _logger = logger;
            _frameworkService = frameworkService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, company=W")]
        [ProducesResponseType(typeof(ResultModel<EmpresaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<EmpresaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<EmpresaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveCompany([FromBody] RegistroEmpresaInputModel model)
        {
            try
            {
                var result = await _frameworkService.SaveCompany(model);
                return Ok(new ResultModel<EmpresaModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<EmpresaModel>(new EmpresaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Empresa: ({model.Pessoa.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Empresa: ({model.Pessoa.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<EmpresaModel>(new EmpresaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Empresa: ({model.Pessoa.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Empresa: ({model.Pessoa.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, company=W")]
        [ProducesResponseType(typeof(ResultModel<EmpresaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<EmpresaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<EmpresaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCompany([FromBody] AlteracaoEmpresaInputModel model)
        {
            try
            {
                var result = await _frameworkService.UpdateCompany(model);
                return Ok(new ResultModel<EmpresaModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<EmpresaModel>(new EmpresaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Empresa: ({model.Pessoa.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Empresa: ({model.Pessoa.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<EmpresaModel>(new EmpresaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Empresa: ({model.Pessoa.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Empresa: ({model.Pessoa.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpGet("search"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, company=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] EmpresaSearchModel model)
        {
            try
            {
                var result = await _frameworkService.SearchCompany(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<EmpresaModel>>()
                    {
                        Data = new List<EmpresaModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<EmpresaModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<EmpresaModel>>()
                {
                    Data = new List<EmpresaModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<EmpresaModel>>()
                {
                    Data = new List<EmpresaModel>(),
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
