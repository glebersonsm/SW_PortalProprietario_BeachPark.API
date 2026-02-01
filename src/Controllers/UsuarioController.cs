using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsuarioController> _logger;
        public UsuarioController(IUserService userService,
            ILogger<UsuarioController> logger)
        {
            _userService = userService;
            _logger = logger;
        }


        [HttpPost, Authorize(Roles = "Administrador, user=W")]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SaveUser(RegistroUsuarioFullInputModel model)
        {
            try
            {

                var result = await _userService.SaveUser(model);
                return Ok(new ResultModel<UserRegisterResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<UserRegisterResultModel>(new UserRegisterResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.Login})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.Login})", err.Message },
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<UserRegisterResultModel>(new UserRegisterResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.Login})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.Login})", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, user=W, Usuario")]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<UserRegisterResultModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> AlterarUsuario(RegistroUsuarioFullInputModel model)
        {
            try
            {

                var result = await _userService.SaveUser(model);
                return Ok(new ResultModel<UserRegisterResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<UserRegisterResultModel>(new UserRegisterResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.Login})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.Login})", err.Message },
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<UserRegisterResultModel>(new UserRegisterResultModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.Login})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível registrar o usuário: ({model.Login})", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPatch("changepassword"), Authorize]
        [ProducesResponseType(typeof(ResultModel<ChangePasswordResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<ChangePasswordResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ChangePasswordResultModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ChangePassword(ChangePasswordInputModel model)
        {

            try
            {
                var result = await _userService.ChangePassword(model);
                return Ok(new ResultModel<ChangePasswordResultModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ChangePasswordResultModel>()
                {
                    Data = new ChangePasswordResultModel(),
                    Status = StatusCodes.Status400BadRequest,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível alterar a senha", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível alterar a senha", err.Message }
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ChangePasswordResultModel>()
                {
                    Data = new ChangePasswordResultModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível alterar a senha", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível alterar a senha", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPatch("resetpassword")]
        [ProducesResponseType(typeof(ResultModel<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<string>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ResetPassword(ResetPasswordoUserModel model)
        {

            try
            {
                var result = await _userService.ResetPassword(model);
                return Ok(new ResultModel<string>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<string>()
                {
                    Data = err.Message,
                    Status = StatusCodes.Status404NotFound,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível resetar a senha", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível resetar a senha", err.Message },
                    Success = false
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<string>()
                {
                    Data = err.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível resetar a senha", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível resetar a senha", err.Message },
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<string>()
                {
                    Data = err.Message,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível resetar a senha", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível resetar a senha", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("search"), Authorize(Roles = "Administrador, user=R, Usuario")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<UsuarioModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<UsuarioModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<UsuarioModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SearchUser([FromQuery] UsuarioSearchPaginatedModel searchModel)
        {

            try
            {
                var result = await _userService.Search(searchModel);
                if (result == null || !result.Value.usuarios.Any())
                    return Ok(new ResultWithPaginationModel<List<UsuarioModel>>()
                    {
                        Data = new List<UsuarioModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<UsuarioModel>>(result.Value.usuarios.AsList())
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
                return BadRequest(new ResultWithPaginationModel<List<UsuarioModel>>()
                {
                    Data = new List<UsuarioModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<UsuarioModel>>()
                {
                    Data = new List<UsuarioModel>(),
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
