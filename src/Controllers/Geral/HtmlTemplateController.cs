using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Geral
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class HtmlTemplateController : ControllerBase
    {

        private readonly ILogger<HtmlTemplateController> _logger;
        private readonly IHtmlTemplateService _htmlTemplateService;
        private readonly IScriptService _scriptService;

        public HtmlTemplateController(ILogger<HtmlTemplateController> logger,
            IHtmlTemplateService htmlTemplateService,
            IScriptService scriptService)
        {
            _logger = logger;
            _htmlTemplateService = htmlTemplateService;
            _scriptService = scriptService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, htmltemplate=W")]
        [ProducesResponseType(typeof(ResultModel<HtmlTemplateModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<HtmlTemplateModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<HtmlTemplateModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveHtmlTemplate([FromBody] HtmlTemplateInputModel model)
        {
            try
            {
                var result = await _htmlTemplateService.SaveHtmlTemplate(model);
                return Ok(new ResultModel<HtmlTemplateModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<HtmlTemplateModel>(new HtmlTemplateModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o HtmlTemplate", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o HtmlTemplate", err.Message },
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<HtmlTemplateModel>(new HtmlTemplateModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o HtmlTemplate", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o HtmlTemplate", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, htmltemplate=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<HtmlTemplateModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<HtmlTemplateModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<HtmlTemplateModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchHtmlTemplateModel model)
        {

            try
            {
                var result = await _htmlTemplateService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<HtmlTemplateModel>>()
                    {
                        Data = new List<HtmlTemplateModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<HtmlTemplateModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<HtmlTemplateModel>>()
                {
                    Data = new List<HtmlTemplateModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<HtmlTemplateModel>>()
                {
                    Data = new List<HtmlTemplateModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }

        }

        [HttpPost("execute"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, htmltemplate=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<HtmlTemplateResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<HtmlTemplateResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<HtmlTemplateResultModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateTemplate([FromBody] HtmlTemplateExecuteModel model)
        {
            try
            {
                var htmlTemplateBase = await _scriptService.GenerateHtmlFromTemplate(model);

                return Ok(new ResultModel<HtmlTemplateResultModel>(htmlTemplateBase)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<HtmlTemplateResultModel>(new HtmlTemplateResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados do HtmlTemplate: ({model.TemplateId})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados do HtmlTemplate: ({model.TemplateId})", err.Message },
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<HtmlTemplateModel>(new HtmlTemplateModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados do HtmlTemplate: ( {model.TemplateId})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados do HtmlTemplate: ( {model.TemplateId})", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpGet("GetKeyValueListFromContratoSCP"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, GestorFinanceiro, htmltemplate=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<KeyValueModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<KeyValueModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<KeyValueModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetKeyValueListFromContratoSCP([FromQuery] GetHtmlValuesModel model)
        {

            try
            {
                var result = await _htmlTemplateService.GetKeyValueListFromContratoSCP(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<KeyValueModel>>()
                    {
                        Data = new List<KeyValueModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<KeyValueModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<KeyValueModel>>()
                {
                    Data = new List<KeyValueModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<KeyValueModel>>()
                {
                    Data = new List<KeyValueModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }

        }

    }
}
