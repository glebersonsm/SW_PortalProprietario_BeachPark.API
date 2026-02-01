using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class FrameworkController : ControllerBase
    {
        private readonly IFrameworkService _frameworkService;
        private readonly ILogger<UsuarioController> _logger;
        private readonly IParametroSistemaService _parametroService;
        public FrameworkController(ILogger<UsuarioController> logger,
            IFrameworkService frameworkService,
            IParametroSistemaService parametroSistemaService)
        {
            _logger = logger;
            _frameworkService = frameworkService;
            _parametroService = parametroSistemaService;
        }

        [HttpGet("searchmodules"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, modules=R")]
        [ProducesResponseType(typeof(ResultModel<List<ModuloModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<ModuloModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ModuloModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SearchModules([FromQuery] ModuloSearchModel searchModel)
        {
            try
            {
                var result = await _frameworkService.SearchModules(searchModel);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<ModuloModel>>() { Data = new List<ModuloModel>(), Errors = new List<string>() { "Ops! Nenhum registro encontrado!" }, Status = StatusCodes.Status404NotFound, Success = true });
                else return Ok(new ResultModel<List<ModuloModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ModuloModel>>()
                {
                    Data = new List<ModuloModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<ModuloModel>>()
                {
                    Data = new List<ModuloModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPost("saveParameters"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, parametrosistema=W")]
        [ProducesResponseType(typeof(ResultModel<ParametroSistemaViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<ParametroSistemaViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ParametroSistemaViewModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveParameters([FromForm] ParametroSistemaInputUpdateModel model)
        {
            try
            {
                var result = await _parametroService.SaveParameters(model);
                return Ok(new ResultModel<ParametroSistemaViewModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ParametroSistemaViewModel>(new ParametroSistemaViewModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o parâmetro do sistema", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o parâmetro do sistema", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ParametroSistemaViewModel>(new ParametroSistemaViewModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o parâmetro do sistema", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o parâmetro do sistema", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpGet("getParameters"), Authorize]
        [ProducesResponseType(typeof(ResultModel<ParametroSistemaViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<ParametroSistemaViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ParametroSistemaViewModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetParameters()
        {
            try
            {
                var result = await _parametroService.GetParameters();
                if (result == null)
                    return Ok(new ResultModel<ParametroSistemaViewModel>()
                    {
                        Data = new ParametroSistemaViewModel(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<ParametroSistemaViewModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ParametroSistemaViewModel>()
                {
                    Data = new ParametroSistemaViewModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<ParametroSistemaViewModel>()
                {
                    Data = new ParametroSistemaViewModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("getEmpresasVinculadas"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaVinculadaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaVinculadaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaVinculadaModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmpresasVinculadas()
        {
            try
            {
                var result = await _frameworkService.GetEmpresasVinculadas();
                if (result == null)
                    return Ok(new ResultModel<List<EmpresaVinculadaModel>>()
                    {
                        Data = new(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else return Ok(new ResultModel<List<EmpresaVinculadaModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<EmpresaVinculadaModel>>()
                {
                    Data = new(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<EmpresaVinculadaModel>>()
                {
                    Data = new(),
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
