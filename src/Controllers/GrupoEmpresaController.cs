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
    public class GrupoEmpresaController : ControllerBase
    {

        private readonly ILogger<GrupoEmpresaController> _logger;
        private readonly IFrameworkService _frameworkService;

        public GrupoEmpresaController(ILogger<GrupoEmpresaController> logger,
            IFrameworkService frameworkService)
        {
            _logger = logger;
            _frameworkService = frameworkService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, companygroup=W")]
        [ProducesResponseType(typeof(ResultModel<GrupoEmpresaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<GrupoEmpresaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<GrupoEmpresaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveCompanyGroup([FromBody] RegistroGrupoEmpresaInputModel model)
        {
            try
            {
                var result = await _frameworkService.SaveCompanyGroup(model);
                return Ok(new ResultModel<GrupoEmpresaModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<GrupoEmpresaModel>(new GrupoEmpresaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Grupo de Empresa: ({model.Pessoa.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Grupo de Empresa: ({model.Pessoa.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<GrupoEmpresaModel>(new GrupoEmpresaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Grupo de Empresa: ({model.Pessoa.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Grupo de Empresa: ({model.Pessoa.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, companygroup=W")]
        [ProducesResponseType(typeof(ResultModel<GrupoEmpresaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<GrupoEmpresaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<GrupoEmpresaModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCompanyGroup([FromBody] AlteracaoGrupoEmpresaInputModel model)
        {
            try
            {
                var result = await _frameworkService.UpdateCompanyGroup(model);
                return Ok(new ResultModel<GrupoEmpresaModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<GrupoEmpresaModel>(new GrupoEmpresaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Grupo de Empresa: ({model.Pessoa.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Grupo de Empresa: ({model.Pessoa.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<GrupoEmpresaModel>(new GrupoEmpresaModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Grupo de Empresa: ({model.Pessoa.RazaoSocial})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Grupo de Empresa: ({model.Pessoa.RazaoSocial})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpGet("search"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, companygroup=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<GrupoEmpresaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<GrupoEmpresaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<GrupoEmpresaModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] GrupoEmpresaSearchModel model)
        {

            try
            {
                var result = await _frameworkService.SearchCompanyGroup(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<GrupoEmpresaModel>>()
                    {
                        Data = new List<GrupoEmpresaModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<GrupoEmpresaModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<GrupoEmpresaModel>>()
                {
                    Data = new List<GrupoEmpresaModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<GrupoEmpresaModel>>()
                {
                    Data = new List<GrupoEmpresaModel>(),
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
