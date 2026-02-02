using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuditModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Audit
{
    [Authorize]
    [ApiController]
    [Route("api/audit")]
    [Produces("application/json")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly IAuditCacheService _auditCacheService;

        public AuditLogController(
            IAuditService auditService,
            IAuditCacheService auditCacheService)
        {
            _auditService = auditService;
            _auditCacheService = auditCacheService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<AuditLogModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<AuditLogModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterModel filter)
        {
            try
            {
                // Gerar chave de cache baseada nos filtros
                var cacheKey = GenerateCacheKey(filter);
                
                var result = await _auditCacheService.GetCachedAuditLogsAsync(
                    cacheKey,
                    async () => await _auditService.GetAuditLogsAsync(filter));

                return Ok(new ResultWithPaginationModel<List<AuditLogModel>>(result.Data)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true,
                    PageNumber = result.PageNumber,
                    LastPageNumber = result.LastPageNumber
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultWithPaginationModel<List<AuditLogModel>>(new List<AuditLogModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { "Erro ao buscar logs de auditoria", err.Message, err.InnerException.Message } :
                    new List<string>() { "Erro ao buscar logs de auditoria", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false,
                    PageNumber = filter.PageNumber,
                    LastPageNumber = 1
                });
            }
        }

        [HttpGet("entity/{entityType}/{entityId}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultModel<List<AuditLogModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<AuditLogModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditLogByEntity(string entityType, int entityId)
        {
            try
            {
                var result = await _auditCacheService.GetCachedEntityHistoryAsync(
                    entityType,
                    entityId,
                    async () => await _auditService.GetAuditLogByEntityAsync(entityType, entityId));

                return Ok(new ResultModel<List<AuditLogModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<AuditLogModel>>(new List<AuditLogModel>())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Erro ao buscar histórico de auditoria: {entityType}/{entityId}", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Erro ao buscar histórico de auditoria: {entityType}/{entityId}", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultModel<AuditLogModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<AuditLogModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<AuditLogModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditLogById(int id)
        {
            try
            {
                var result = await _auditService.GetAuditLogByIdAsync(id);
                
                if (result == null)
                {
                    return NotFound(new ResultModel<AuditLogModel>(new AuditLogModel())
                    {
                        Errors = new List<string> { $"Log de auditoria com ID {id} não encontrado" },
                        Status = StatusCodes.Status404NotFound,
                        Success = false
                    });
                }

                return Ok(new ResultModel<AuditLogModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<AuditLogModel>(new AuditLogModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Erro ao buscar log de auditoria: {id}", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Erro ao buscar log de auditoria: {id}", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        private string GenerateCacheKey(AuditLogFilterModel filter)
        {
            return $"{filter.DataInicio:yyyyMMddHHmmss}_{filter.DataFim:yyyyMMddHHmmss}_{filter.EntityType}_{filter.EntityId}_{filter.UserId}_{filter.Action}_{filter.IpAddress}_{filter.PageNumber}_{filter.PageSize}";
        }
    }
}

