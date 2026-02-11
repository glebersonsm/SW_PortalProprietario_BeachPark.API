using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;

namespace SW_PortalProprietario.API.src.Controllers.Documento
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class DocumentoController : ControllerBase
    {

        private readonly IDocumentService _documentService;

        public DocumentoController(
            IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, document=W")]
        [ProducesResponseType(typeof(ResultModel<DocumentoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<DocumentoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<DocumentoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveDocument([FromForm] DocumentInputModel model)
        {
            try
            {
                var result = await _documentService.SaveDocument(model);
                return Ok(new ResultModel<DocumentoModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<DocumentoModel>(new DocumentoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<DocumentoModel>(new DocumentoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPatch, Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, document=W")]
        [ProducesResponseType(typeof(ResultModel<DocumentoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<DocumentoModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<DocumentoModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AlterarDocument([FromBody] AlteracaoDocumentInputModel model)
        {
            try
            {
                var result = await _documentService.UpdateDocument(model);
                return Ok(new ResultModel<DocumentoModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<DocumentoModel>(new DocumentoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<DocumentoModel>(new DocumentoModel())
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Nome})", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível salvar o Documento: ({model.Nome})", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpPost("delete"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, document=D")]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDocument([FromQuery] int id)
        {
            try
            {
                var result = await _documentService.DeleteDocument(id);
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


        [HttpGet("download/{id}"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, document=Download, Usuario")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DownloadResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DownloadResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownLoadFile(int id)
        {
            try
            {
                var result = await _documentService.DownloadFile(id);
                
                if (result.Arquivo == null || result.Arquivo.Length == 0)
                    throw new Exception("Arquivo não encontrado.");

                // Se o TipoMime vier vazio por algum motivo, utiliza um padrão seguro
                var tipoMime = string.IsNullOrWhiteSpace(result.TipoMime)
                    ? "application/octet-stream"
                    : result.TipoMime;

                var memory = new MemoryStream(result.Arquivo);
                memory.Position = 0;
                
                // Se o NomeArquivo vier vazio, utiliza um nome padrão
                var fileName = string.IsNullOrWhiteSpace(result.NomeArquivo)
                    ? $"documento_{id}.pdf"
                    : result.NomeArquivo;

                return File(memory, tipoMime, fileName);

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new DownloadResultModel()
                {
                    Result = "Não baixado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status404NotFound,
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new DownloadResultModel()
                {
                    Result = "Não baixado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new DownloadResultModel()
                {
                    Result = "Não baixado",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("search"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, document=R, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<DocumentoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<DocumentoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<DocumentoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchPadraoModel model)
        {
            try
            {
                var result = await _documentService.Search(model);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<DocumentoModel>>()
                    {
                        Data = new List<DocumentoModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<DocumentoModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<DocumentoModel>>()
                {
                    Data = new List<DocumentoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<DocumentoModel>>()
                {
                    Data = new List<DocumentoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("history/{id}"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, document=R")]
        [ProducesResponseType(typeof(ResultModel<List<DocumentoHistoricoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<DocumentoHistoricoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<DocumentoHistoricoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> History(int id)
        {
            try
            {
                var result = await _documentService.History(id);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<DocumentoHistoricoModel>>()
                    {
                        Data = new List<DocumentoHistoricoModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<DocumentoHistoricoModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<DocumentoHistoricoModel>>()
                {
                    Data = new List<DocumentoHistoricoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<DocumentoHistoricoModel>>()
                {
                    Data = new List<DocumentoHistoricoModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("reorder"), Authorize(Roles = "Administrador, GestorFinanceiro, GestorReservasAgendamentos, document=W")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReorderDocuments([FromBody] List<ReorderDocumentModel> documents)
        {
            try
            {
                var result = await _documentService.ReorderDocuments(documents);
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
                    new List<string>() { "Não foi possível atualizar a ordem dos documentos", err.Message, err.InnerException.Message } :
                    new List<string>() { "Não foi possível atualizar a ordem dos documentos", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { "Não foi possível atualizar a ordem dos documentos", err.Message, err.InnerException.Message } :
                    new List<string>() { "Não foi possível atualizar a ordem dos documentos", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

    }
}
