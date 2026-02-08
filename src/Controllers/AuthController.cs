using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers
{
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> Register(UserRegisterInputModel model)
        {
            try
            {
                var result = await _authService.Register(model);
                return Ok(new ResultModel<UserRegisterResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<UserRegisterResultModel>(new UserRegisterResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.FullName})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.FullName})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<UserRegisterResultModel>(new UserRegisterResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.FullName})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.FullName})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("login2FAOptions")]
        [ProducesResponseType(typeof(ResultModel<Login2FAOptionsResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<Login2FAOptionsResultModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> GetLogin2FAOptions([FromQuery] string? login)
        {
            try
            {
                var result = await _authService.GetLogin2FAOptionsAsync(login ?? "");
                return Ok(new ResultModel<Login2FAOptionsResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<Login2FAOptionsResultModel>(new Login2FAOptionsResultModel())
                {
                    Errors = new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("login2FAOptions")]
        [ProducesResponseType(typeof(ResultModel<Login2FAOptionsResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<Login2FAOptionsResultModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> GetLogin2FAOptionsPost([FromBody] Login2FAOptionsRequestModel model)
        {
            try
            {
                var result = await _authService.GetLogin2FAOptionsAsync(model?.Login ?? "");
                return Ok(new ResultModel<Login2FAOptionsResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<Login2FAOptionsResultModel>(new Login2FAOptionsResultModel())
                {
                    Errors = new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status500InternalServerError)]

        [Produces("application/json")]
        public async Task<IActionResult> Login(LoginInputModel user)
        {
            try
            {
                var result = await _authService.Login(user);
                user.Senha = "";
                return Ok(new ResultModel<TokenResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("sendTwoFactorCode")]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SendTwoFactorCode([FromBody] SendTwoFactorCodeInputModel model)
        {
            try
            {
                var result = await _authService.SendTwoFactorCodeAsync(model);
                if (result == null)
                    return BadRequest(new ResultModel<TokenResultModel>(new TokenResultModel())
                    {
                        Errors = new List<string>() { "Não foi possível enviar o código." },
                        Status = StatusCodes.Status400BadRequest,
                        Success = false
                    });
                return Ok(new ResultModel<TokenResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = new List<string>() { "Usuário não encontrado." },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("validateTwoFactor")]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ValidateTwoFactor([FromBody] ValidateTwoFactorInputModel model)
        {
            try
            {
                var result = await _authService.ValidateTwoFactorAsync(model);
                if (result == null)
                    return Unauthorized(new ResultModel<TokenResultModel>(new TokenResultModel())
                    {
                        Errors = new List<string>() { "Código inválido ou expirado." },
                        Status = StatusCodes.Status401Unauthorized,
                        Success = false
                    });
                return Ok(new ResultModel<TokenResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("changecompany"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<TokenResultModel>), StatusCodes.Status500InternalServerError)]

        [Produces("application/json")]
        public async Task<IActionResult> ChangeActualCompanyId(SetCompanyModel model)
        {

            try
            {
                var result = await _authService.ChangeActualCompanyId(model);
                return Ok(new ResultModel<TokenResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TokenResultModel>(new TokenResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("comunicacoesTokenEnviadas"), Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ComunicacaoTokenEnviadaViewModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<IActionResult> SearchComunicacoesTokenEnviadas([FromQuery] DateTime? dataHoraEnvioInicial, [FromQuery] DateTime? dataHoraEnvioFinal, [FromQuery] string? canal, [FromQuery] string? login, [FromQuery] int? numeroDaPagina, [FromQuery] int? quantidadeRegistrosRetornar)
        {
            try
            {
                var search = new SearchComunicacaoTokenEnviadaModel
                {
                    DataHoraEnvioInicial = dataHoraEnvioInicial,
                    DataHoraEnvioFinal = dataHoraEnvioFinal,
                    Canal = canal,
                    Login = login,
                    NumeroDaPagina = numeroDaPagina,
                    QuantidadeRegistrosRetornar = quantidadeRegistrosRetornar
                };
                var result = await _authService.SearchComunicacoesTokenEnviadasAsync(search);
                if (result == null)
                    return Ok(new ResultWithPaginationModel<List<ComunicacaoTokenEnviadaViewModel>>(new List<ComunicacaoTokenEnviadaViewModel>()) { PageNumber = 1, LastPageNumber = 1, Success = true, Errors = new List<string>() });
                var (pageNumber, lastPageNumber, list) = result.Value;
                return Ok(new ResultWithPaginationModel<List<ComunicacaoTokenEnviadaViewModel>>(list) { PageNumber = pageNumber, LastPageNumber = lastPageNumber, Success = true, Errors = new List<string>() });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<ComunicacaoTokenEnviadaViewModel>>(new List<ComunicacaoTokenEnviadaViewModel>())
                {
                    Errors = new List<string>() { err.Message },
                    Success = false
                });
            }
        }

        [HttpGet("resumoVolumeComunicacoesToken"), Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultModel<List<ResumoVolumeComunicacaoTokenModel>>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetResumoVolumeComunicacoesToken([FromQuery] DateTime? dataInicial, [FromQuery] DateTime? dataFinal)
        {
            try
            {
                var list = await _authService.GetResumoVolumeComunicacoesTokenAsync(dataInicial, dataFinal);
                return Ok(new ResultModel<List<ResumoVolumeComunicacaoTokenModel>>(list) { Success = true, Errors = new List<string>() });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<ResumoVolumeComunicacaoTokenModel>>(new List<ResumoVolumeComunicacaoTokenModel>())
                {
                    Errors = new List<string>() { err.Message },
                    Success = false
                });
            }
        }
    }
}
