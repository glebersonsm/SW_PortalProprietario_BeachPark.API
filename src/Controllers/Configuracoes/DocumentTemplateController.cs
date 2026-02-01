using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using System.Linq;

namespace SW_PortalProprietario.API.src.Controllers.Configuracoes
{
    [ApiController]
    [Route("api/configuracoes/document-templates")]
    [Authorize]
    public class DocumentTemplateController : ControllerBase
    {
        private readonly IDocumentTemplateService _documentTemplateService;
        private readonly ILogger<DocumentTemplateController> _logger;

        public DocumentTemplateController(
            IDocumentTemplateService documentTemplateService,
            ILogger<DocumentTemplateController> logger)
        {
            _documentTemplateService = documentTemplateService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<DocumentTemplateSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] string? templateType = null)
        {
            EnumDocumentTemplateType? enumTemplateType = null;

            if (!string.IsNullOrWhiteSpace(templateType))
            {
                // Tentar parsear como string primeiro
                if (TryParseTemplateType(templateType, out var parsedType))
                {
                    enumTemplateType = parsedType;
                }
                // Se não conseguir parsear como string, tentar como número
                else if (int.TryParse(templateType, out var templateTypeInt))
                {
                    if (Enum.IsDefined(typeof(EnumDocumentTemplateType), templateTypeInt))
                    {
                        enumTemplateType = (EnumDocumentTemplateType)templateTypeInt;
                    }
                }
            }

            var templates = await _documentTemplateService.ListAsync(enumTemplateType);
            return Ok(templates);
        }

        [HttpGet("{templateId:int}")]
        [ProducesResponseType(typeof(DocumentTemplateModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int templateId)
        {
            if (templateId <= 0)
                return BadRequest(new { message = "ID do template inválido." });

            var result = await _documentTemplateService.GetActiveTemplateAsync(null, templateId);
            if (result == null)
                return NotFound(new { message = $"Template com ID {templateId} não encontrado." });

            // Garantir que sempre retornamos contentHtml, mesmo que seja string vazia
            if (result.ContentHtml == null)
                result.ContentHtml = string.Empty;

            return Ok(result);
        }

        [HttpGet("{templateType}/active")]
        [ProducesResponseType(typeof(DocumentTemplateModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActiveTemplate(string templateType, [FromQuery] int? templateId = null)
        {
            if (!TryParseTemplateType(templateType, out var enumTemplateType))
                return BadRequest(new { message = $"Tipo de template inválido: {templateType}" });

            var result = await _documentTemplateService.GetActiveTemplateAsync(enumTemplateType, templateId);
            if (result == null)
                return NotFound(new { message = "Nenhum template ativo encontrado." });

            // Garantir que sempre retornamos contentHtml, mesmo que seja string vazia
            if (result.ContentHtml == null)
                result.ContentHtml = string.Empty;

            return Ok(result);
        }

        [HttpGet("{templateType}/active/content")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActiveTemplateContent(string templateType, [FromQuery] int? templateId = null)
        {
            if (!TryParseTemplateType(templateType, out var enumTemplateType))
                return BadRequest(new { message = $"Tipo de template inválido: {templateType}" });

            var contentHtml = await _documentTemplateService.GetTemplateContentHtmlAsync(enumTemplateType, templateId);
            if (string.IsNullOrWhiteSpace(contentHtml))
                return NotFound(new { message = "Template ativo não encontrado." });

            return Ok(new { contentHtml });
        }

        [HttpGet("{templateType}/placeholders")]
        [ProducesResponseType(typeof(IEnumerable<dynamic>), StatusCodes.Status200OK)]
        public IActionResult GetPlaceholders(string templateType)
        {
            if (!TryParseTemplateType(templateType, out var enumTemplateType))
                return BadRequest(new { message = $"Tipo de template inválido: {templateType}" });

            return Ok(GetPlaceholdersByType(enumTemplateType));
        }

        [HttpPost("Salvar")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DocumentTemplateModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Salvar([FromBody] DocumentTemplateUploadInputModel model)
        {
            if (model == null)
                return BadRequest(new { message = "Dados do template não informados." });

            // Validar que não está tentando criar com TemplateId
            if (model.TemplateId.HasValue && model.TemplateId.Value > 0)
                return BadRequest(new { message = "TemplateId não deve ser informado ao criar um novo template." });

            var usuarioId = ObterUsuarioId();
            _logger.LogInformation("Criando novo template {TemplateType} pelo usuário {UsuarioId}", model.TemplateType, usuarioId);

            var template = await _documentTemplateService.CreateAsync(model, usuarioId);
            return Ok(template);
        }

        [HttpPost("Alterar")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DocumentTemplateModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Alterar([FromBody] DocumentTemplateUploadInputModel model)
        {
            if (model == null)
                return BadRequest(new { message = "Dados do template não informados." });

            if (model.TemplateId <= 0)
                return BadRequest(new { message = "ID do template inválido." });


            var usuarioId = ObterUsuarioId();
            _logger.LogInformation("Atualizando template {TemplateType} id {TemplateId} pelo usuário {UsuarioId}", model.TemplateType, model.TemplateId, usuarioId);

            try
            {
                var template = await _documentTemplateService.UpdateAsync(model, usuarioId);
                return Ok(template);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro ao atualizar template {TemplateId}: {Message}", model.TemplateId, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar template {TemplateId}", model.TemplateId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao atualizar o template." });
            }
        }

        //[HttpPost("upload")]
        //[Consumes("application/json")]
        //[ProducesResponseType(typeof(DocumentTemplateModel), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[Obsolete("Use POST /api/configuracoes/document-templates para criar ou POST /api/configuracoes/document-templates/{id} para atualizar")]
        //public async Task<IActionResult> Upload([FromBody] DocumentTemplateUploadInputModel model)
        //{
        //    if (model == null)
        //        return BadRequest(new { message = "Dados do template não informados." });

        //    var usuarioId = ObterUsuarioId();
        //    _logger.LogInformation("Recebendo atualização de template {TemplateType} id {TemplateId} pelo usuário {UsuarioId}", model.TemplateType, model.TemplateId, usuarioId);

        //    var template = await _documentTemplateService.UploadAsync(model, usuarioId);
        //    return Ok(template);
        //}

        [HttpDelete("{templateId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int templateId)
        {
            if (templateId <= 0)
                return BadRequest(new { message = "ID do template inválido." });

            try
            {
                var usuarioId = ObterUsuarioId();
                _logger.LogInformation("Recebendo solicitação de exclusão de template {TemplateId} pelo usuário {UsuarioId}", templateId, usuarioId);

                var result = await _documentTemplateService.DeleteAsync(templateId, usuarioId);
                if (result)
                {
                    return Ok(new { message = "Template desativado com sucesso." });
                }

                return BadRequest(new { message = "Não foi possível desativar o template." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro ao desativar template {TemplateId}: {Message}", templateId, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desativar template {TemplateId}", templateId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao desativar o template." });
            }
        }

        private int ObterUsuarioId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private static bool TryParseTemplateType(string value, out EnumDocumentTemplateType templateType)
        {
            templateType = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Replace("-", "", StringComparison.InvariantCultureIgnoreCase)
                                  .Replace("_", "", StringComparison.InvariantCultureIgnoreCase);
            return Enum.TryParse(normalized, ignoreCase: true, out templateType);
        }

        private static IEnumerable<dynamic> GetPlaceholdersByType(EnumDocumentTemplateType templateType)
        {
            return templateType switch
            {
                EnumDocumentTemplateType.VoucherReserva => VoucherReservaPlaceholder.All,
                EnumDocumentTemplateType.AvisoReservaCheckinProximo => VoucherReservaPlaceholder.All,
                EnumDocumentTemplateType.ComunicacaoCancelamentoReservaRci => ComunicacaoRelacionadaAoContrato.All,
                EnumDocumentTemplateType.IncentivoParaAgendamento => IncentivoParaAgendamentoPlaceholder.All,

                _ => Enumerable.Empty<dynamic>()
            };
        }
    }
}

