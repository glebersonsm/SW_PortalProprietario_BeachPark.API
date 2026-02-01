using Dapper;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Enumns;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace SW_PortalProprietario.API.src.Controllers.TimeSharing
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class TimeSharingUsuarioController : ControllerBase
    {

        private readonly ITimeSharingProviderService _timeSharingProviderService;
        private readonly IVoucherReservaService _voucherReservaService;
        private readonly ILogger<TimeSharingUsuarioController> _logger;
        private readonly IDocumentTemplateService _documentTemplateService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public TimeSharingUsuarioController(
            ITimeSharingProviderService timeSharingService, 
            IVoucherReservaService voucherReservaService, 
            ILogger<TimeSharingUsuarioController> logger,
            IDocumentTemplateService documentTemplateService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _timeSharingProviderService = timeSharingService;
            _voucherReservaService = voucherReservaService;
            _logger = logger;
            _documentTemplateService = documentTemplateService;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet("meusContratos"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContratoTimeSharingModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContratoTimeSharingModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContratoTimeSharingModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProprietariosContratosTimeSharing([FromQuery] SearchMeusContratosTimeSharingModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.GetMeusContratosTimeSharing(searchModel);
                if (result == null || !result.Value.contratos.Any())
                    return Ok(new ResultWithPaginationModel<List<ContratoTimeSharingModel>>()
                    {
                        Data = new List<ContratoTimeSharingModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = false
                    });
                else return Ok(new ResultWithPaginationModel<List<ContratoTimeSharingModel>>(result.Value.contratos.AsList())
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
                return BadRequest(new ResultWithPaginationModel<List<ContratoTimeSharingModel>>()
                {
                    Data = new List<ContratoTimeSharingModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ContratoTimeSharingModel>>()
                {
                    Data = new List<ContratoTimeSharingModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("minhasReservasComPontosBaixados"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaTsModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaTsModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaTsModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchMinhasReservasGeralComConsumoPontos([FromQuery] SearchMinhasReservaTsModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.GetMinhasReservasGeralComConsumoPontos(searchModel);
                if (result == null || !result.Value.reservas.Any())
                    return Ok(new ResultWithPaginationModel<List<ReservaTsModel>>()
                    {
                        Data = new List<ReservaTsModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = false
                    });
                else return Ok(new ResultWithPaginationModel<List<ReservaTsModel>>(result.Value.reservas.AsList())
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
                return BadRequest(new ResultWithPaginationModel<List<ReservaTsModel>>()
                {
                    Data = new List<ReservaTsModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ReservaTsModel>>()
                {
                    Data = new List<ReservaTsModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("minhasReservas"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaGeralTsModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaGeralTsModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ReservaGeralTsModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchReservas([FromQuery] SearchMinhasReservasGeralModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.GetMinhasReservasGeral(searchModel);
                if (result == null || !result.Value.reservas.Any())
                    return Ok(new ResultWithPaginationModel<List<ReservaGeralTsModel>>()
                    {
                        Data = new List<ReservaGeralTsModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = false
                    });
                else return Ok(new ResultWithPaginationModel<List<ReservaGeralTsModel>>(result.Value.reservas.AsList())
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
                return BadRequest(new ResultWithPaginationModel<List<ReservaGeralTsModel>>()
                {
                    Data = new List<ReservaGeralTsModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ReservaGeralTsModel>>()
                {
                    Data = new List<ReservaGeralTsModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    LastPageNumber = -1,
                    PageNumber = -1,
                    Success = false
                });
            }
        }

        [HttpGet("editarReserva"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<ReservaTimeSharingCMModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaTimeSharingCMModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaTimeSharingCMModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Editar(Int64 numReserva)
        {

            try
            {
                var result = await _timeSharingProviderService.Editar(numReserva);
                if (result != null)
                    return Ok(new ResultModel<ReservaTimeSharingCMModel>(result)
                    {
                        Success = true,
                        Status = StatusCodes.Status200OK
                    });
                else return Ok(new ResultModel<ReservaTimeSharingCMModel>(new ReservaTimeSharingCMModel())
                {
                    Errors = new List<string>() { "Nenhuma reserva foi encontrada" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<ReservaTimeSharingCMModel>(new ReservaTimeSharingCMModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ReservaTimeSharingCMModel>(new ReservaTimeSharingCMModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ReservaTimeSharingCMModel>(new ReservaTimeSharingCMModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("disponibilidade"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<PeriodoDisponivelResultModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<PeriodoDisponivelResultModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<PeriodoDisponivelResultModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Disponibilidade([FromQuery] SearchDisponibilidadeModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.Disponibilidade(searchModel);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<PeriodoDisponivelResultModel>>()
                    {
                        Data = new List<PeriodoDisponivelResultModel>(),
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<PeriodoDisponivelResultModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return Ok(new ResultModel<List<PeriodoDisponivelResultModel>>()
                {
                    Data = new List<PeriodoDisponivelResultModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<PeriodoDisponivelResultModel>>()
                {
                    Data = new List<PeriodoDisponivelResultModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("salvarReserva"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<Int64>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<Int64>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<Int64>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SalvarReserva([FromBody] InclusaoReservaInputModel model)
        {
            try
            {
                var result = await _timeSharingProviderService.Save(model);
                if (result > 0 || model.TipoUso == "I")
                    return Ok(new ResultModel<Int64>(result)
                    {
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else throw new ArgumentException("Falha na criação da reserva.");

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<Int64>()
                {
                    Data = -1,
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<Int64>()
                {
                    Data = -1,
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPost("alterarReserva"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<Int64>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<Int64>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<Int64>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AlterarReserva([FromBody] InclusaoReservaInputModel model)
        {
            try
            {
                var result = await _timeSharingProviderService.AlterarReserva(model);
                if (result != null)
                    return Ok(new ResultModel<Int64?>(result.IdReservasFront)
                    {
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else throw new ArgumentException("Falha na alteração da reserva.");

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<Int64>()
                {
                    Data = -1,
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<Int64>()
                {
                    Data = -1,
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPost("cancelarReserva"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<bool?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool?>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelarReserva(Int64 numReserva)
        {
            try
            {
                var cancelarReservaModel = new CancelarReservaTsModel()
                {
                    MotivoCancelamento = "1",
                    ObservacaoCancelamento = "Cancelada via Portal MVC",
                    ReservaId = numReserva
                };

                var result = await _timeSharingProviderService.CancelarReserva(cancelarReservaModel);
                if (result.GetValueOrDefault(false))
                {
                    return Ok(new ResultModel<bool>(true)
                    {
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors =  new List<string>() { "Operação não realizada" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("cancelarReservaRCI"), Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(ResultModel<bool?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool?>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelarReservaRCI(
            int? reservaTimesharingId, 
            string? motivoCancelamentoInfUsu = null,
            bool? notificarClientePorEmail = false)
        {
            try
            {
                var cancelarReservaModel = new CancelarReservaTsModel()
                {
                    MotivoCancelamento = "1",
                    ObservacaoCancelamento = "Cancelada via Portal MVC",
                    ReservaTimesharingId = reservaTimesharingId,
                    MotivoCancelamentoInfUsu = motivoCancelamentoInfUsu,
                    NotificarCliente = notificarClientePorEmail
                };

                var result = await _timeSharingProviderService.CancelarReserva(cancelarReservaModel);
                if (result.GetValueOrDefault(false))
                {
                    // Enviar email se solicitado
                    if (notificarClientePorEmail.GetValueOrDefault(false) && reservaTimesharingId.HasValue)
                    {
                        try
                        {
                            await EnviarEmailCancelamentoReserva(reservaTimesharingId.Value);
                        }
                        catch (Exception emailErr)
                        {
                            _logger.LogError(emailErr, "Erro ao enviar email de cancelamento para reserva {ReservaId}", reservaTimesharingId);
                        }
                    }

                    return Ok(new ResultModel<bool>(true)
                    {
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { "Operação não realizada" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        private async Task EnviarEmailCancelamentoReserva(int reservaTimesharingId)
        {
            var emailApenasParDestinatariosPermitidos = _configuration.GetValue<bool>("EnviarEmailApenasParaDestinatariosPermitidos", true);
            var emailsPermitidos = _configuration.GetValue<string>("DestinatarioEmailPermitido");
            if (emailApenasParDestinatariosPermitidos && (string.IsNullOrEmpty(emailsPermitidos) || !emailsPermitidos.Contains("@")))
                throw new ArgumentException("O ambiente está configurado para não enviar email para qualquer destinatário.");

            // Buscar dados da reserva usando GetReservasRci
            // Buscar todas as reservas para encontrar a específica (pode ser otimizado futuramente)
            var searchModel = new SearchReservasRciModel
            {
                NumeroDaPagina = 1,
                QuantidadeRegistrosRetornar = 1000 // Buscar muitas reservas para encontrar a específica
            };

            var reservaEncontrada = (await _timeSharingProviderService.GetReservasRci(searchModel)).Value.reservas?.FirstOrDefault();

            if (reservaEncontrada == null)
            {
                _logger.LogWarning("Reserva {ReservaId} não encontrada para envio de email de cancelamento", reservaTimesharingId);
                return;
            }

            // Buscar email do cliente - agora disponível diretamente na reserva
            var emailCliente = emailApenasParDestinatariosPermitidos ? emailsPermitidos : reservaEncontrada.EmailCliente;

            if (string.IsNullOrWhiteSpace(emailCliente))
            {
                _logger.LogWarning("Email do cliente não encontrado para reserva {ReservaId}. ClienteReservante: {ClienteId}", 
                    reservaTimesharingId, reservaEncontrada.ClienteReservante);
                return;
            }

            // Buscar template de cancelamento
            var templateHtml = await _documentTemplateService.GetTemplateContentHtmlAsync(
                EnumDocumentTemplateType.ComunicacaoCancelamentoReservaRci, null);
            
            if (string.IsNullOrWhiteSpace(templateHtml))
            {
                _logger.LogWarning("Template de cancelamento não encontrado");
                return;
            }

            // Preencher placeholders
            var dataFormatada = reservaEncontrada.DataCriacao?.ToString("dd/MM/yyyy") 
                ?? reservaEncontrada.DataHoraCriacao?.ToString("dd/MM/yyyy") 
                ?? reservaEncontrada.TrgDtInclusao?.ToString("dd/MM/yyyy") 
                ?? string.Empty;

            var placeholders = new Dictionary<string, string?>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "NomeCliente", reservaEncontrada.NomeCliente ?? string.Empty },
                { "ContratoNumero", reservaEncontrada.NumeroContrato ?? string.Empty },
                { "DataSolicitacaoAgendamento", dataFormatada },
                { "IdRci", reservaEncontrada.IdRCI ?? string.Empty }
            };

            var conteudoEmail = PopulateHtmlTemplate(templateHtml, placeholders);

            // Salvar e enviar email
            var usuarioSistemaId = _configuration.GetValue<int>("UsuarioSistemaId", 1);
            await _emailService.SaveInternal(new EmailInputInternalModel
            {
                UsuarioCriacao = usuarioSistemaId,
                Assunto = "Confirmação de cancelamento de solicitação de reserva RCI",
                Destinatario = emailCliente,
                ConteudoEmail = conteudoEmail,
            });

            _logger.LogInformation("Email de cancelamento enviado para reserva {ReservaId} - Email: {Email}", 
                reservaTimesharingId, emailCliente);
        }


        private static string PopulateHtmlTemplate(string templateHtml, IDictionary<string, string?> placeholders)
        {
            return Regex.Replace(
                templateHtml,
                @"\{\{\s*([^\}]+?)\s*\}\}",
                match =>
                {
                    var key = match.Groups[1].Value.Trim();
                    return placeholders.TryGetValue(key, out var value)
                        ? value ?? string.Empty
                        : match.Value;
                },
                RegexOptions.IgnoreCase);
        }

        [HttpGet("parametrosPontosRci"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<int>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetParametrosPontosRci()
        {
            try
            {
                var parametroSistema = await _timeSharingProviderService.GetParametroSistema();
                var pontosRci = parametroSistema?.PontosRci ?? 5629;
                return Ok(new ResultModel<int>(pontosRci)
                {
                    Success = true,
                    Status = StatusCodes.Status200OK
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<int>(5629)
                {
                    Errors = new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("v1/disponibilidadeparatroca"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<PeriodoDisponivelResultModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<PeriodoDisponivelResultModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<PeriodoDisponivelResultModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DisponibilidadeParaTroca([FromQuery] SearchDisponibilidadeParaTrocaModel searchModel)
        {
            try
            {
                var result = await _timeSharingProviderService.DisponibilidadeParaTroca(searchModel);
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<PeriodoDisponivelResultModel>>()
                    {
                        Data = new List<PeriodoDisponivelResultModel>(),
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<PeriodoDisponivelResultModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return Ok(new ResultModel<List<PeriodoDisponivelResultModel>>()
                {
                    Data = new List<PeriodoDisponivelResultModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<PeriodoDisponivelResultModel>>()
                {
                    Data = new List<PeriodoDisponivelResultModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/trocarperiodo"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<TrocaPeriodoResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TrocaPeriodoResponseModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TrocaPeriodoResponseModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<TrocaPeriodoResponseModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TrocarPeriodo([FromBody] TrocaPeriodoRequestModel model)
        {
            try
            {
                var result = await _timeSharingProviderService.TrocarPeriodo(model);
                return Ok(new ResultModel<TrocaPeriodoResponseModel>(result)
                {
                    Success = true,
                    Status = StatusCodes.Status200OK
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TrocaPeriodoResponseModel>()
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<TrocaPeriodoResponseModel>()
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TrocaPeriodoResponseModel>()
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/trocartipouso"), Authorize(Roles = "Administrador, Usuario")]
        [ProducesResponseType(typeof(ResultModel<TrocaTipoUsoResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<TrocaTipoUsoResponseModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TrocaTipoUsoResponseModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<TrocaTipoUsoResponseModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TrocarTipoUso([FromBody] TrocaTipoUsoRequestModel model)
        {
            try
            {
                var result = await _timeSharingProviderService.TrocarTipoUso(model);
                return Ok(new ResultModel<TrocaTipoUsoResponseModel>(result)
                {
                    Success = true,
                    Status = StatusCodes.Status200OK
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TrocaTipoUsoResponseModel>()
                {
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<TrocaTipoUsoResponseModel>()
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<TrocaTipoUsoResponseModel>()
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("reservas/{reservaId}/voucher"), Authorize(Roles = "Administrador, OperadorSistema, GestorReservasAgendamentos, portalproprietariosw, Usuario")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GerarVoucherReserva(int reservaId)
        {
            try
            {
                var voucher = await _voucherReservaService.GerarVoucherAsync(reservaId, true);
                return File(voucher.FileBytes, voucher.ContentType, voucher.FileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ResultModel<object>()
                {
                    Data = null,
                    Message = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Success = false,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResultModel<object>()
                {
                    Data = null,
                    Message = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Success = false,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar voucher de reserva para o agendamento {AgendamentoId}", reservaId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<object>()
                {
                    Data = null,
                    Message = "Não foi possível gerar o voucher de reserva.",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
