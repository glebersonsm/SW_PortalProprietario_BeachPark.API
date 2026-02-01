using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using System.Threading.Tasks;

namespace SW_PortalProprietario.API.src.Controllers.Configuracoes
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AutomaticCommunicationConfigController : ControllerBase
    {
        private readonly IAutomaticCommunicationConfigService _service;

        public AutomaticCommunicationConfigController(IAutomaticCommunicationConfigService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<List<AutomaticCommunicationConfigModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<AutomaticCommunicationConfigModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] EnumProjetoType? projetoType = null)
        {
            try
            {
                var result = await _service.GetAllAsync(projetoType);
                
                return Ok(new ResultModel<List<AutomaticCommunicationConfigModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<AutomaticCommunicationConfigModel>>(new List<AutomaticCommunicationConfigModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { "Erro ao buscar todas as configurações de comunicação automática", err.Message, err.InnerException.Message } :
                    new List<string>() { "Erro ao buscar todas as configurações de comunicação automática", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("{communicationType}")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCommunicationType(EnumDocumentTemplateType communicationType, [FromQuery] EnumProjetoType? projetoType = null)
        {
            try
            {
                var result = await _service.GetByCommunicationTypeAsync(communicationType, projetoType);
                
                if (result == null)
                {
                    return NotFound(new ResultModel<AutomaticCommunicationConfigModel>(new AutomaticCommunicationConfigModel())
                    {
                        Errors = new List<string> { $"Configuração para o tipo '{communicationType}' não encontrada" },
                        Status = StatusCodes.Status404NotFound,
                        Success = false
                    });
                }

                return Ok(new ResultModel<AutomaticCommunicationConfigModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<AutomaticCommunicationConfigModel>(new AutomaticCommunicationConfigModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Erro ao buscar configuração: {communicationType}", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Erro ao buscar configuração: {communicationType}", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Save([FromBody] AutomaticCommunicationConfigInputModel model)
        {
            try
            {
                var result = await _service.SaveAsync(model);
                
                return Ok(new ResultModel<AutomaticCommunicationConfigModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<AutomaticCommunicationConfigModel>(new AutomaticCommunicationConfigModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a configuração: ({model.CommunicationType})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a configuração: ({model.CommunicationType})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<AutomaticCommunicationConfigModel>(new AutomaticCommunicationConfigModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar a configuração: ({model.CommunicationType})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar a configuração: ({model.CommunicationType})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("{id}")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<AutomaticCommunicationConfigModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AutomaticCommunicationConfigInputModel model)
        {
            try
            {
                var result = await _service.UpdateAsync(id, model);
                
               
                return Ok(new ResultModel<AutomaticCommunicationConfigModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<AutomaticCommunicationConfigModel>(new AutomaticCommunicationConfigModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível atualizar a configuração: ({id})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível atualizar a configuração: ({id})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<AutomaticCommunicationConfigModel>(new AutomaticCommunicationConfigModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível atualizar a configuração: ({id})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível atualizar a configuração: ({id})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
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
                    new List<string>() { $"Não foi possível deletar a configuração: ({id})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível deletar a configuração: ({id})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível deletar a configuração: ({id})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível deletar a configuração: ({id})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("{id}/simulate")]
        [Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Simulate(int id, [FromBody] SimulateEmailRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    return BadRequest(new ResultModel<bool>(false)
                    {
                        Errors = new List<string>() { "Email do usuário não informado" },
                        Status = StatusCodes.Status400BadRequest,
                        Success = false
                    });
                }

                var result = await _service.SimulateEmailAsync(id, model.Email);
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
                    new List<string>() { $"Não foi possível simular o envio: ({id})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível simular o envio: ({id})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível simular o envio: ({id})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível simular o envio: ({id})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

    }

    public class SimulateEmailRequestModel
    {
        public string Email { get; set; } = string.Empty;
    }
}

