using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers.Faq
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class FaqController : ControllerBase
    {

        private readonly IFaqService _faqService;

        public FaqController(
            IFaqService faqService)
        {
            _faqService = faqService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, faq=W, Usuario")]
        [ProducesResponseType(typeof(ResultModel<FaqModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<FaqModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<FaqModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveFaq([FromBody] FaqInputModel model)
        {
            try
            {
                var result = await _faqService.SaveFaq(model);
                return Ok(new ResultModel<FaqModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<FaqModel>(new FaqModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a FAQ: ({model.Pergunta})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a FAQ: ({model.Pergunta})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<FaqModel>(new FaqModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a FAQ: ({model.Pergunta})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a FAQ: ({model.Pergunta})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, faq=D")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFaq([FromQuery] int id)
        {
            try
            {
                var result = await _faqService.DeleteFaq(id);
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

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, faq=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<FaqModelSimplificado>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<FaqModelSimplificado>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<FaqModelSimplificado>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchFaqModel model)
        {
            try
            {
                var result = await _faqService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<FaqModelSimplificado>>()
                    {
                        Data = new List<FaqModelSimplificado>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<FaqModelSimplificado>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<FaqModelSimplificado>>()
                {
                    Data = new List<FaqModelSimplificado>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<FaqModelSimplificado>>()
                {
                    Data = new List<FaqModelSimplificado>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("reorder"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, faq=W")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReorderFaqs([FromBody] List<ReorderFaqModel> faqs)
        {
            try
            {
                var result = await _faqService.ReorderFaqs(faqs);
                return Ok(new ResultModel<bool>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>(false)
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { "Não foi possível atualizar a ordem das FAQs", err.Message, err.InnerException.Message } :
                    new List<string>() { "Não foi possível atualizar a ordem das FAQs", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { "Não foi possível atualizar a ordem das FAQs", err.Message, err.InnerException.Message } :
                    new List<string>() { "Não foi possível atualizar a ordem das FAQs", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }
    }
}
