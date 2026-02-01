using Dapper;
using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Interfaces.ReservasApi;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using SW_PortalProprietario.Domain.Enumns;
using System.Collections.Generic;
using System;

namespace SW_PortalProprietario.API.src.Controllers.Multipropriedade
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class MultiPropriedadeUsuarioController : ControllerBase
    {

        private readonly IEmpreendimentoProviderService _empreendimentoService;
        private readonly IReservaAgendamentoService _reservaService;
        private readonly IFinanceiroProviderService _financeiroProviderService;
        private readonly IVoucherReservaService _voucherReservaService;
        private readonly IDocumentTemplateService _documentTemplateService;
        private readonly ILogger<MultiPropriedadeUsuarioController> _logger;

        public MultiPropriedadeUsuarioController(
            IEmpreendimentoProviderService empreendimentoService,
            IReservaAgendamentoService reservaService,
            IFinanceiroProviderService financeiroProviderService,
            IVoucherReservaService voucherReservaService,
            IDocumentTemplateService documentTemplateService,
            ILogger<MultiPropriedadeUsuarioController> logger)
        {
            _empreendimentoService = empreendimentoService;
            _reservaService = reservaService;
            _financeiroProviderService = financeiroProviderService;
            _voucherReservaService = voucherReservaService;
            _documentTemplateService = documentTemplateService;
            _logger = logger;
        }

        [HttpGet("meusContratos"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MeusContratos([FromQuery] SearchMyContractsModel searchModel)
        {
            try
            {
                var result = await _empreendimentoService.GetMyContracts(searchModel);
                if (result == null || !result.Value.contratos.Any())
                    return Ok(new ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>()
                    {
                        Data = new List<ProprietarioSimplificadoModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>(result.Value.contratos.AsList())
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
                return BadRequest(new ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>()
                {
                    Data = new List<ProprietarioSimplificadoModel>(),
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
                return StatusCode(500, new ResultWithPaginationModel<List<ProprietarioSimplificadoModel>>()
                {
                    Data = new List<ProprietarioSimplificadoModel>(),
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


        [HttpGet("meusAgendamentos"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<SemanaModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MinhasReservasAgendamentos([FromQuery] PeriodoCotaDisponibilidadeUsuarioSearchModel model)
        {

            try
            {
                var result = await _reservaService.ConsultarMeusAgendamentos(model);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultWithPaginationModel<List<SemanaModel>>(result.Data)
                    {
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        LastPageNumber = result.LastPageNumber,
                        PageNumber = result.PageNumber,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Errors = new List<string>() { "Nenhuma reserva foi encontrada" },
                    Status = StatusCodes.Status404NotFound,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultWithPaginationModel<List<SemanaModel>>(new List<SemanaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("consultarMinhasReservasAgendamento"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarMinhasReservasByAgendamentoId([FromQuery] string agendamento)
        {

            try
            {
                var result = await _reservaService.ConsultarMinhasReservaByAgendamentoId(agendamento);
                if (result != null && result.Data != null && result.Data.Any())
                    return Ok(new ResultModel<List<ReservaModel>>(result.Data)
                    {
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else return Ok(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Errors = new List<string>() { "Nenhuma reserva foi encontrada" },
                    Status = StatusCodes.Status404NotFound,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<List<ReservaModel>>(new List<ReservaModel>())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("cancelarMinhaReservaAgendamento"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model)
        {
            try
            {
                var result = await _reservaService.CancelarMinhaReservaAgendamento(model);
                if (result != null)
                {
                    if (result.Success)
                    {
                        result.Status = (int)HttpStatusCode.OK;
                        result.Success = true;
                        return Ok(result);
                    }
                    else
                    {
                        result.Errors = new List<string>() { result.Message };
                        result.Success = false;
                        return BadRequest(result);
                    }
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = result.Message != null && !string.IsNullOrEmpty(result.Message) ? new List<string>() { result?.Message } : new List<string>() { "Operação não realizada" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<bool>(false)
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("editarMinhaReserva"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<ReservaForEditModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaForEditModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<ReservaForEditModel>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditarMinhaReserva(int reservaId)
        {

            try
            {
                var result = await _reservaService.EditarMinhaReserva(reservaId);
                if (result != null && result.Success)
                    return Ok(result);
                else return Ok(new ResultModel<ReservaForEditModel>(new ReservaForEditModel())
                {
                    Errors = new List<string>() { "Nenhuma reserva foi encontrada" },
                    Status = StatusCodes.Status404NotFound,
                    Success = true
                });

            }
            catch (FileNotFoundException err)
            {
                return NotFound(new ResultModel<ReservaForEditModel>(new ReservaForEditModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status404NotFound,
                    Success = false
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<ReservaForEditModel>(new ReservaForEditModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<ReservaForEditModel>(new ReservaForEditModel())
                {
                    Errors = new List<string>() { $"{err.Message} - Inner: {err.InnerException?.Message}" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("efetuarOuAlterarReservaAgendamento"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<ReservaModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<ReservaModel>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EfetuarOuAlterarReservaAgendamento(CriacaoReservaAgendamentoInputModel model)
        {
            try
            {
                var result = await _reservaService.SalvarReservaEmAgendamento(model);
                if (result != null && (result.Status == 201 || result.Status == 200))
                {
                    if (result.Success)
                    {
                        return Ok(result);
                    }
                }
                else if (result != null && result.Errors != null && result.Errors.Any())
                {
                    return BadRequest(new ResultModel<int>(-1)
                    {
                        Errors = result.Errors,
                        Status = StatusCodes.Status500InternalServerError,
                        Success = false,
                        Message = result.Errors.First(),
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new ResultModel<int>(-1)
                {
                    Errors = result.Errors,
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Message = result.Message,
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

        [HttpPost("v1/liberarMinhaSemanaParaPool"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> LiberarMinhaSemanaParaPool([FromBody] LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
        {
            try
            {
                var result = await _reservaService.LiberarMinhaSemanaPool(modelAgendamentoPool);
                if (result != null && result.Success)
                    return Ok(result);
                else throw new ArgumentException(result != null ? result.Message : "Não foi possível realizar a liberação para da semana para o POOL");
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<bool>()
                {
                    Data = false,
                    Errors = new List<string>() { err.Message, err.InnerException?.Message },
                    Message = err.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<bool>()
                {
                    Data = false,
                    Errors = new List<string>() { err.Message, err.InnerException?.Message },
                    Message = err.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("salvarMinhaContaBancaria"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(Application.Models.ResultModel<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Application.Models.ResultModel<ClienteContaBancariaViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Application.Models.ResultModel<ClienteContaBancariaViewModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SalvarContaBancaria([FromBody] ClienteContaBancariaInputModel request)
        {
            try
            {
                var result = await _financeiroProviderService.SalvarMinhaContaBancaria(request);
                if (result > 0)
                    return Ok(new Application.Models.ResultModel<int>(result)
                    {
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                        Success = true
                    });
                else return StatusCode(500, new Application.Models.ResultModel<ClienteContaBancariaViewModel>()
                {
                    Data = new ClienteContaBancariaViewModel(),
                    Errors = new List<string>() { $"Não foi possível salvar a conta bancária" },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new Application.Models.ResultModel<ClienteContaBancariaViewModel>()
                {
                    Data = new ClienteContaBancariaViewModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new Application.Models.ResultModel<ClienteContaBancariaViewModel>()
                {
                    Data = new ClienteContaBancariaViewModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("minhasContasBancarias"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(IEnumerable<ClienteContaBancariaViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> GetMinhasContasBancarias()
        {
            try
            {
                var result = await _financeiroProviderService.GetMinhasContasBancarias();
                if (result == null || !result.Any())
                    return Ok(new Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>(result)
                    {
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                return Ok(new Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>()
                {
                    Data = new List<ClienteContaBancariaViewModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>()
                {
                    Data = new List<ClienteContaBancariaViewModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("enviarCodigoVerificacao"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(Application.Models.ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Application.Models.ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Application.Models.ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> EnviarCodigoVerificacao([FromBody] ConfirmacaoLiberacaoCotaPoolInputModel model)
        {
            try
            {
                var result = await _empreendimentoService.GerarCodigoVerificacaoLiberacaoPool(model.AgendamentoId.GetValueOrDefault());
                if (result)
                    return Ok(new Application.Models.ResultModel<bool>(true)
                    {
                        Success = true,
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                    });
                else return StatusCode(500, new Application.Models.ResultModel<bool>(false)
                {
                    Success = false,
                    Errors = new List<string>() { $"Não foi possível gerar o código de verificação" },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new Application.Models.ResultModel<bool>(false)
                {
                    Success = false,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new Application.Models.ResultModel<bool>(false)
                {
                    Success = false,
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("v1/disponibilidadeparatroca"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<SemanaDisponibilidadeModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> ConsultarDisponibilidade([FromQuery] DispobilidadeSearchModel searchModel)
        {
            try
            {
                var result = await _empreendimentoService.ConsultarDisponibilidadeCompativel(searchModel);
                if (result != null)
                    return Ok(result);
                else return NotFound(new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Status = StatusCodes.Status404NotFound,
                    Success = false,
                    Message = "Ops! não foi encontrado nenhum registro.",
                    Errors = new List<string>() { "Ops! não foi encontrado nenhum registro." }
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = err.InnerException != null ?
                        $"Não foi possível consultar a disponibilidade: {err.Message} {err.InnerException.Message}" :
                        $"Não foi possível consultar a disponibilidade: " +
                        $" {err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = err.InnerException != null ?
                    $"Não foi possível consultar a disponibilidade: {err.Message} {err.InnerException.Message}" :
                    $"Não foi possível consultar a disponibilidade: {err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPost("v1/trocarminhasemana"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> TrocarSemana([FromBody] TrocaSemanaInputModel model)
        {
            try
            {
                var result = await _empreendimentoService.TrocarSemana(model);
                if (result != null && result.Success)
                    return Ok(result);
                else return BadRequest(result);
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<int?>()
                {
                    Data = -1,
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/trocartipouso"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> TrocarTipoUso([FromBody] TrocaSemanaInputModel model)
        {
            try
            {
                model.TrocaDeTipoDeUso = true;
                var result = await _empreendimentoService.TrocarTipoUso(model);
                if (result != null && result.Success)
                    return Ok(result);
                else return BadRequest(result);
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<int?>()
                {
                    Data = -1,
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("v1/incluirsemana"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<int?>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> IncluirSemana([FromBody] IncluirSemanaInputModel model)
        {
            try
            {
                var result = await _empreendimentoService.IncluirSemana(model);
                if (result != null && result.Success)
                    return Ok(result);
                else return BadRequest(result);
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<int?>()
                {
                    Data = -1,
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<SemanaDisponibilidadeModel>>()
                {
                    Data = new List<SemanaDisponibilidadeModel>(),
                    Message = $"{err.Message}",
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpGet("downloadContratoSCP"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DownloadResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DownloadResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadContratoSCP([FromQuery] int cotaId)
        {
            try
            {
                var result = await _empreendimentoService.DownloadContratoSCP(cotaId);
                if (result != null && !string.IsNullOrEmpty(result.Path))
                {
                    var ext = Application.Functions.FileUtils.ObterTipoMIMEPorExtensao(string.Concat(".", result.Path.Split("\\").Last().Split(".").Last()));
                    if (string.IsNullOrEmpty(ext))
                        throw new Exception($"Tipo de arquivo: ({result.Path.Split("\\").Last().Split(".").Last()}) não suportado.");

                    var memory = new MemoryStream();
                    using var stream = new FileStream(result.Path, FileMode.Open);
                    await stream.CopyToAsync(memory);

                    memory.Position = 0;
                    return File(memory, ext, Path.GetFileName(result.Path));
                }
                else
                {
                    return NotFound(new DownloadResultModel()
                    {
                        Result = "Não baixado",
                        Errors = new List<string>() { "Contrato não encontrado" },
                        Status = StatusCodes.Status404NotFound,
                    });

                }

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

        //[HttpGet("getDadosVoucher"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        //[ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ResultModel<DadosImpressaoVoucherResultModel>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ResultModel<DadosImpressaoVoucherResultModel>), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetDadosVoucher([FromQuery] string agendamentoId)
        //{
        //    try
        //    {
        //        var result = await _empreendimentoService.GetDadosImpressaoVoucher(agendamentoId);
        //        if (result != null)
        //            return Ok(result);
        //        else return NotFound(new ResultModel<DadosImpressaoVoucherResultModel>()
        //        {
        //            Status = StatusCodes.Status404NotFound,
        //            Success = false,
        //            Message = "Ops! não foi encontrado nenhum registro.",
        //            Errors = new List<string>() { "Ops! não foi encontrado nenhum registro." }
        //        });
        //    }
        //    catch (ArgumentException err)
        //    {
        //        return BadRequest(new ResultModel<DadosImpressaoVoucherResultModel>()
        //        {
        //            Data = new DadosImpressaoVoucherResultModel(),
        //            Message = err.InnerException != null ?
        //                $"Não foi possível retornar os dados: {err.Message} {err.InnerException.Message}" :
        //                $"Não foi possível retornar os dados: " +
        //                $" {err.Message}",
        //            Status = StatusCodes.Status400BadRequest,
        //            Success = false
        //        });
        //    }
        //    catch (Exception err)
        //    {
        //        return StatusCode(500, new ResultModel<DadosImpressaoVoucherResultModel>()
        //        {
        //            Data = new DadosImpressaoVoucherResultModel(),
        //            Message = err.InnerException != null ?
        //            $"Não foi possível retornar os dados: {err.Message} {err.InnerException.Message}" :
        //            $"Não foi possível retornar os dados: {err.Message}",
        //            Status = StatusCodes.Status500InternalServerError,
        //            Success = false
        //        });
        //    }
        //}

        //[HttpGet("getVoucherWithTemplate"), Authorize(Roles = "Administrador, GestorReservasAgendamentos, portalproprietariosw")]
        //[ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetVoucherWithTemplate([FromQuery] string agendamentoId, [FromQuery] int? templateId = null)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(agendamentoId))
        //        {
        //            return BadRequest(new ResultModel<object>
        //            {
        //                Status = StatusCodes.Status400BadRequest,
        //                Success = false,
        //                Message = "O ID do agendamento é obrigatório.",
        //                Errors = new List<string> { "O ID do agendamento é obrigatório." }
        //            });
        //        }

        //        // Buscar dados do voucher
        //        var dadosVoucherResult = await _empreendimentoService.GetDadosImpressaoVoucher(agendamentoId);
        //        if (dadosVoucherResult == null || dadosVoucherResult.Data == null)
        //        {
        //            return NotFound(new ResultModel<object>
        //            {
        //                Status = StatusCodes.Status404NotFound,
        //                Success = false,
        //                Message = "Ops! não foi encontrado nenhum registro de voucher para o agendamento informado.",
        //                Errors = new List<string> { "Ops! não foi encontrado nenhum registro de voucher para o agendamento informado." }
        //            });
        //        }

        //        var dadosVoucher = dadosVoucherResult.Data;

        //        // Buscar template configurado
        //        var templateHtml = await _documentTemplateService.GetTemplateContentHtmlAsync(
        //            EnumDocumentTemplateType.VoucherAgendamentoMultiownership, 
        //            templateId);

        //        if (string.IsNullOrWhiteSpace(templateHtml))
        //        {
        //            return NotFound(new ResultModel<object>
        //            {
        //                Status = StatusCodes.Status404NotFound,
        //                Success = false,
        //                Message = "Nenhum template ativo encontrado para voucher de agendamento multiownership.",
        //                Errors = new List<string> { "Nenhum template ativo encontrado para voucher de agendamento multiownership." }
        //            });
        //        }

        //        // Processar template substituindo placeholders
        //        var processedHtml = ProcessTemplateHtml(templateHtml, dadosVoucher);

        //        return Ok(new ResultModel<object>
        //        {
        //            Status = StatusCodes.Status200OK,
        //            Success = true,
        //            Data = new { html = processedHtml }
        //        });
        //    }
        //    catch (ArgumentException err)
        //    {
        //        return BadRequest(new ResultModel<object>
        //        {
        //            Message = err.InnerException != null ?
        //                $"Não foi possível processar o voucher: {err.Message} {err.InnerException.Message}" :
        //                $"Não foi possível processar o voucher: {err.Message}",
        //            Status = StatusCodes.Status400BadRequest,
        //            Success = false
        //        });
        //    }
        //    catch (Exception err)
        //    {
        //        _logger.LogError(err, "Erro ao processar voucher com template para agendamento {AgendamentoId}", agendamentoId);
        //        return StatusCode(500, new ResultModel<object>
        //        {
        //            Message = err.InnerException != null ?
        //            $"Não foi possível processar o voucher: {err.Message} {err.InnerException.Message}" :
        //            $"Não foi possível processar o voucher: {err.Message}",
        //            Status = StatusCodes.Status500InternalServerError,
        //            Success = false
        //        });
        //    }
        //}

        //private string ProcessTemplateHtml(string templateHtml, DadosImpressaoVoucherResultModel dados)
        //{
        //    if (string.IsNullOrWhiteSpace(templateHtml) || dados == null)
        //        return templateHtml ?? string.Empty;

        //    // Mapear dados do voucher para placeholders
        //    var placeholders = new Dictionary<string, string?>(StringComparer.InvariantCultureIgnoreCase)
        //    {
        //        { "numeroReserva", dados.NumeroReserva ?? "----" },
        //        { "cliente", dados.Cliente ?? "---" },
        //        { "hospedePrincipal", dados.HospedePrincipal ?? dados.HospedePrincipalNome ?? "---" },
        //        { "tipoUso", dados.TipoUso ?? dados.TipoUtilizacao ?? dados.TipoDisponibilizacao ?? "---" },
        //        { "contrato", dados.Contrato ?? "---" },
        //        { "nomeHotel", dados.NomeHotel ?? "---" },
        //        { "observacao", dados.Observacao ?? dados.Observacoes ?? "---" },
        //        { "tipoCliente", dados.TipoUso ?? "---" },
        //        { "dataChegada", dados.DataChegada ?? "--/--/----" },
        //        { "horaChegada", dados.HoraChegada ?? "--:--" },
        //        { "dataPartida", dados.DataPartida ?? "--/--/----" },
        //        { "horaPartida", dados.HoraPartida ?? "--:--" },
        //        { "acomodacao", dados.Acomodacao ?? dados.TipoApartamento ?? "---" },
        //        { "quantidadePax", dados.QuantidadePax ?? dados.OcupacaoMaxima?.ToString() ?? "---" }
        //    };

        //    // Substituir placeholders no formato {{chave}}
        //    return Regex.Replace(
        //        templateHtml,
        //        @"\{\{\s*([^\}]+?)\s*\}\}",
        //        match =>
        //        {
        //            var key = match.Groups[1].Value.Trim();
        //            return placeholders.TryGetValue(key, out var value)
        //                ? value ?? string.Empty
        //                : match.Value;
        //        },
        //        RegexOptions.IgnoreCase);
        //}

        [HttpGet("reservas/{agendamentoId}/voucher"), Authorize(Roles = "Administrador, OperadorSistema, GestorReservasAgendamentos, portalproprietariosw")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GerarVoucherReserva(int agendamentoId)
        {
            try
            {
                var voucher = await _voucherReservaService.GerarVoucherAsync(agendamentoId,false);
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
                _logger.LogError(ex, "Erro ao gerar voucher de reserva para o agendamento {AgendamentoId}", agendamentoId);
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
