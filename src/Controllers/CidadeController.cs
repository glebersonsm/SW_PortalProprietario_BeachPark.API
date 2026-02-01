using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class CidadeController : ControllerBase
    {

        private readonly ICityService _cityService;

        public CidadeController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, city=W, city=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<CidadeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<CidadeModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<CidadeModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveCity([FromBody] RegistroCidadeInputModel model)
        {
            try
            {
                var result = await _cityService.SaveCity(model);
                return Ok(new ResultModel<CidadeModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<CidadeModel>(new CidadeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Cidade: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Cidade: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<CidadeModel>(new CidadeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Cidade: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Cidade: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, city=W, city=*, Usuario")]
        [ProducesResponseType(typeof(ResultModel<CidadeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<CidadeModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<CidadeModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCity([FromBody] AlteracaoCidadeInputModel model)
        {
            try
            {
                var result = await _cityService.UpdateCity(model);
                return Ok(new ResultModel<CidadeModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<CidadeModel>(new CidadeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Cidade: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Cidade: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<CidadeModel>(new CidadeModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a Cidade: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a Cidade: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, city=D, city=*, Usuario")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCity([FromQuery] int id)
        {
            try
            {
                var result = await _cityService.DeleteCity(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, city=R, city=*, Usuario, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<CidadeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<CidadeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<CidadeModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] CidadeSearchModel model)
        {
            try
            {
                var result = await _cityService.SearchCity(model);
                if (result == null || !result.Value.cidades.Any())
                    return Ok(new ResultWithPaginationModel<List<CidadeModel>>()
                    {
                        Data = new List<CidadeModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<CidadeModel>>(result.Value.cidades.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<CidadeModel>>()
                {
                    Data = new List<CidadeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<CidadeModel>>()
                {
                    Data = new List<CidadeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("searchOnProvider"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, city=R, city=*, Usuario, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<CidadeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<CidadeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<CidadeModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchOnProvider([FromQuery] CidadeSearchModel model)
        {
            try
            {
                var result = await _cityService.SearchCityOnProvider(model);
                if (result == null || !result.Value.cidades.Any())
                    return Ok(new ResultWithPaginationModel<List<CidadeModel>>()
                    {
                        Data = new List<CidadeModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<CidadeModel>>(result.Value.cidades.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<CidadeModel>>()
                {
                    Data = new List<CidadeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<CidadeModel>>()
                {
                    Data = new List<CidadeModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("consultarCep"), Authorize(Roles = "Administrador, Usuario, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<CepResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<CepResponseModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<CepResponseModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarCep([FromQuery] string cep)
        {
            try
            {
                var cepData = await _cityService.ConsultarCep(cep);
                return Ok(new ResultModel<CepResponseModel>(cepData)
                {
                    Success = true,
                    Status = StatusCodes.Status200OK
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<CepResponseModel>(null)
                {
                    Errors = new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return BadRequest(new ResultModel<CepResponseModel>(null)
                {
                    Errors = new List<string>() { "Cep não localizado" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
        }
    }
}
