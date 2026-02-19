using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.RabbitMQ
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class RabbitMQQueueController : ControllerBase
    {
        private readonly IRabbitMQQueueService _rabbitMQQueueService;
        private readonly ILogger<RabbitMQQueueController> _logger;

        public RabbitMQQueueController(
            IRabbitMQQueueService rabbitMQQueueService,
            ILogger<RabbitMQQueueController> logger)
        {
            _rabbitMQQueueService = rabbitMQQueueService;
            _logger = logger;
        }

        [HttpPost("save"), Authorize(Roles = "Administrador, rabbitmqqueue=W")]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveQueue([FromBody] RabbitMQQueueInputModel model)
        {
            try
            {
                var result = await _rabbitMQQueueService.SaveQueue(model);
                return Ok(new ResultModel<RabbitMQQueueViewModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<RabbitMQQueueViewModel>(new RabbitMQQueueViewModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar a fila RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar a fila RabbitMQ", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RabbitMQQueueViewModel>(new RabbitMQQueueViewModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar a fila RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel salvar a fila RabbitMQ", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("getAll"), Authorize(Roles = "Administrador, rabbitmqqueue=R")]
        [ProducesResponseType(typeof(ResultModel<List<RabbitMQQueueViewModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<RabbitMQQueueViewModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<RabbitMQQueueViewModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllQueues()
        {
            try
            {
                var result = await _rabbitMQQueueService.GetAllQueues();
                return Ok(new ResultModel<List<RabbitMQQueueViewModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<RabbitMQQueueViewModel>>(new List<RabbitMQQueueViewModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel buscar as filas RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel buscar as filas RabbitMQ", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<RabbitMQQueueViewModel>>(new List<RabbitMQQueueViewModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel buscar as filas RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel buscar as filas RabbitMQ", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("getById"), Authorize(Roles = "Administrador, rabbitmqqueue=R")]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQueueById([FromQuery] int id)
        {
            try
            {
                var result = await _rabbitMQQueueService.GetQueueById(id);
                if (result == null)
                    return Ok(new ResultModel<RabbitMQQueueViewModel>(new RabbitMQQueueViewModel())
                    {
                        Errors = new List<string>() { "Fila nÃ£o encontrada" },
                        Status = StatusCodes.Status404NotFound,
                        Success = false
                    });

                return Ok(new ResultModel<RabbitMQQueueViewModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<RabbitMQQueueViewModel>(new RabbitMQQueueViewModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel buscar a fila RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel buscar a fila RabbitMQ", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RabbitMQQueueViewModel>(new RabbitMQQueueViewModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel buscar a fila RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel buscar a fila RabbitMQ", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("delete"), Authorize(Roles = "Administrador, rabbitmqqueue=D")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteQueue([FromQuery] int id)
        {
            try
            {
                var result = await _rabbitMQQueueService.DeleteQueue(id);
                return Ok(new DeleteResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Errors = new List<string>()
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new DeleteResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel excluir a fila RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel excluir a fila RabbitMQ", err.Message }
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DeleteResultModel
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel excluir a fila RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel excluir a fila RabbitMQ", err.Message }
                });
            }
        }

        [HttpPost("toggleStatus"), Authorize(Roles = "Administrador, rabbitmqqueue=W")]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<RabbitMQQueueViewModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleQueueStatus([FromQuery] int id)
        {
            try
            {
                var result = await _rabbitMQQueueService.ToggleQueueStatus(id);
                return Ok(new ResultModel<RabbitMQQueueViewModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<RabbitMQQueueViewModel>(new RabbitMQQueueViewModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel alterar o status da fila RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel alterar o status da fila RabbitMQ", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<RabbitMQQueueViewModel>(new RabbitMQQueueViewModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"NÃ£o foi possÃ­vel alterar o status da fila RabbitMQ", err.Message, err.InnerException.Message } :
                    new List<string>() { $"NÃ£o foi possÃ­vel alterar o status da fila RabbitMQ", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }
    }
}
