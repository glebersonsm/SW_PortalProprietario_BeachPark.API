using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Interfaces.Esol;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Esol;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Esol
{
    /// <summary>
    /// Controller de consultas gerais - migrado do SwReservaApiMain (Access Center).
    /// Sufixo Esol para evitar conflitos.
    /// </summary>
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class GeralAccessCenterEsolController : ControllerBase
    {
        private readonly IGeralAccessCenterEsolService _geralService;
        private readonly ILogger<GeralAccessCenterEsolController> _logger;

        public GeralAccessCenterEsolController(IGeralAccessCenterEsolService geralService, ILogger<GeralAccessCenterEsolController> logger)
        {
            _geralService = geralService;
            _logger = logger;
        }

        [HttpGet("consultarEmpresa"), Authorize(Roles = "*, Administrador, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaEsolModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaEsolModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaEsolModel>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<EmpresaEsolModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ConsultarEmpresa([FromQuery] ConsultaEmpresaEsolModel model)
        {
            try
            {
                var result = await _geralService.ConsultarEmpresa(model);
                if (result != null && result.Any())
                    return Ok(new ResultModel<List<EmpresaEsolModel>>(result)
                    {
                        Message = "",
                        Success = true,
                        Status = StatusCodes.Status200OK
                    });
                return NotFound(new ResultModel<List<EmpresaEsolModel>>(new List<EmpresaEsolModel>())
                {
                    Message = "Nenhuma empresa foi encontrada",
                    Success = false,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<List<EmpresaEsolModel>>(new List<EmpresaEsolModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<EmpresaEsolModel>>(new List<EmpresaEsolModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<EmpresaEsolModel>>(new List<EmpresaEsolModel>())
                {
                    Message = $"{err.Message} - Inner: {err.InnerException?.Message}",
                    Success = false,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
