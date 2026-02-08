using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Email
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class EmailController : ControllerBase
    {

        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, email=W, email=*")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveEmail([FromBody] EmailInputModel model)
        {
            try
            {
                var result = await _emailService.Save(model);
                return Ok(new ResultModel<bool>(true)
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
                    new List<string>() { $"Não foi possível salvar o Email: ({model.Assunto})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Email: ({model.Assunto})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<EmailModel>(new EmailModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Email: ( {model.Assunto})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Email: ( {model.Assunto})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, email=W, email=*")]
        [ProducesResponseType(typeof(ResultModel<EmailModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<EmailModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<EmailModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEmail([FromBody] AlteracaoEmailInputModel model)
        {
            try
            {
                var result = await _emailService.Update(model);
                return Ok(new ResultModel<EmailModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<EmailModel>(new EmailModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Email: (  {model.Assunto})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Email: (  {model.Assunto})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<EmailModel>(new EmailModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Email: (   {model.Assunto})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Email: (   {model.Assunto})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, email=D, email=*")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmail([FromQuery] int id)
        {
            try
            {
                var result = await _emailService.DeleteEmail(id);
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

        [HttpPost("send"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, email=W, email=*")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnviarEmail([FromQuery] int id)
        {
            try
            {
                var result = await _emailService.Send(id);
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
                    new List<string>() { $"Não foi possível enviar o Email: (  {id})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível enviar o Email: (   {id})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível enviar o Email: (   {id})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível enviar o Email: (    {id})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, email=R, email=*")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<EmailModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<EmailModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<EmailModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchEmailModel model)
        {
            try
            {
                var result = await _emailService.Search(model);
                if (result == null || result.Value.emails == null || !result.Value.emails.Any())
                    return Ok(new ResultWithPaginationModel<List<EmailModel>>()
                    {
                        Data = new List<EmailModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<EmailModel>>(result.Value.emails.AsList())
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
                return BadRequest(new ResultWithPaginationModel<List<EmailModel>>()
                {
                    Data = new List<EmailModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<EmailModel>>()
                {
                    Data = new List<EmailModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Endpoint usado pelo pixel de rastreio de abertura de e-mail.
        /// Chamado quando o cliente de e-mail carrega as imagens do corpo do e-mail.
        /// Público (sem autenticação) para o pixel poder ser carregado.
        /// </summary>
        [HttpGet("track/open")]
        [AllowAnonymous]
        [Produces("image/gif")]
        public async Task<IActionResult> TrackOpen([FromQuery] int id)
        {
            if (id <= 0)
                return File(TransparentGif1x1(), "image/gif");
            try
            {
                await _emailService.RecordEmailOpen(id);
            }
            catch
            {
                // Não expor erro ao cliente; apenas retornar a imagem
            }
            return File(TransparentGif1x1(), "image/gif", "pixel.gif");
        }

        private static byte[] TransparentGif1x1()
        {
            return Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        }
    }
}
