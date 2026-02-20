using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Financeiro
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class FinanceiroController : ControllerBase
    {

        private readonly IFinanceiroHybridProviderService _financeiroProviderService;
        private readonly IFinanceiroTransacaoService _financeiroTransacaoService;

        public FinanceiroController(IFinanceiroHybridProviderService financeiroUsuarioService,
            IFinanceiroTransacaoService financeiroTransacaoService)
        {
            _financeiroProviderService = financeiroUsuarioService;
            _financeiroTransacaoService = financeiroTransacaoService;
        }

        [HttpGet("searchContasPendentes"), Authorize(Roles = "Administrador, GestorFinanceiro")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContaPendenteModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContaPendenteModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContaPendenteModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchFinanceiroGeral([FromQuery] SearchContasPendentesGeral searchModel)
        {
            try
            {
                var result = await _financeiroProviderService.GetContaPendenteGeral(searchModel);
                if (result == null || !result.Value.contasPendentes.Any())
                    return Ok(new ResultWithPaginationModel<List<ContaPendenteModel>>()
                    {
                        Data = new List<ContaPendenteModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<ContaPendenteModel>>(result.Value.contasPendentes.AsList())
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
                return BadRequest(new ResultWithPaginationModel<List<ContaPendenteModel>>()
                {
                    Data = new List<ContaPendenteModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<ContaPendenteModel>>()
                {
                    Data = new List<ContaPendenteModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("downloadBoleto"), Authorize(Roles = "Administrador, GestorFinanceiro")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DownloadResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DownloadResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadBoleto([FromQuery] DownloadBoleto model)
        {
            try
            {
                var result = await _financeiroProviderService.DownloadBoleto(model);
                if (result != null && !string.IsNullOrEmpty(result.Path))
                {
                    var ext = SW_PortalProprietario.Application.Functions.FileUtils.ObterTipoMIMEPorExtensao(string.Concat(".", result.Path.Split("\\").Last().Split(".").Last()));
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
                        Errors = new List<string>() { "Boleto não encontrado" },
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

        [HttpPost("tokenize"), Authorize(Roles = "Administrador, GestorFinanceiro")]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<CardTokenizedModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<CardTokenizedModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<CardTokenizedModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Tokenize([FromBody] CardTokenizeRequestModel cardModel)
        {
            try
            {
                var result = await _financeiroTransacaoService.Tokenize(cardModel);
                if (result == null)
                    return Ok(new SW_PortalProprietario.Application.Models.ResultModel<CardTokenizedModel>()
                    {
                        Data = new CardTokenizedModel(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else
                {
                    if (result.errors == null || !result.errors.Any())
                    {
                        return Ok(new SW_PortalProprietario.Application.Models.ResultModel<CardTokenizedModel>(result)
                        {
                            Errors = new List<string>(),
                            Status = StatusCodes.Status200OK,
                            Success = true
                        });
                    }
                    else
                    {
                        return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<CardTokenizedModel>()
                        {
                            Data = new CardTokenizedModel(),
                            Errors = result.errors.AsList(),
                            Status = StatusCodes.Status400BadRequest,
                        });

                    }
                }

            }
            catch (ArgumentException err)
            {
                return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<CardTokenizedModel>()
                {
                    Data = new CardTokenizedModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<CardTokenizedModel>()
                {
                    Data = new CardTokenizedModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("gettokenizedcards"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<CardTokenizedModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<CardTokenizedModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<CardTokenizedModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTokenizedCard([FromQuery] SearchTokenizedCardFromUserModel searchTokenizedCardModel)
        {
            try
            {
                var result = await _financeiroTransacaoService.GetAllTokenizedCardFromUser(searchTokenizedCardModel);
                if (result == null || !result.Any())
                    return Ok(new SW_PortalProprietario.Application.Models.ResultModel<List<CardTokenizedModel>>()
                    {
                        Data = new List<CardTokenizedModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new SW_PortalProprietario.Application.Models.ResultModel<List<CardTokenizedModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<List<CardTokenizedModel>>()
                {
                    Data = new List<CardTokenizedModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<List<CardTokenizedModel>>()
                {
                    Data = new List<CardTokenizedModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPost("transacionarcomcartao"), Authorize(Roles = "Administrador, GestorFinanceiro")]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<TransactionCardResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<TransactionCardResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<TransactionCardResultModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DoCardTransaction([FromBody] DoTransactionCardInputModel doTransactionInputModel)
        {
            try
            {
                var result = await _financeiroTransacaoService.DoCardTransaction(doTransactionInputModel);
                if (result == null)
                    return Ok(new SW_PortalProprietario.Application.Models.ResultModel<TransactionCardResultModel>()
                    {
                        Data = new TransactionCardResultModel(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else
                {
                    if (result.errors == null || !result.errors.Any())
                    {
                        return Ok(new SW_PortalProprietario.Application.Models.ResultModel<TransactionCardResultModel>(result)
                        {
                            Errors = new List<string>(),
                            Status = StatusCodes.Status200OK,
                            Success = true
                        });
                    }
                    else
                    {
                        return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<TransactionCardResultModel>()
                        {
                            Data = new TransactionCardResultModel(),
                            Errors = result.errors.AsList(),
                            Status = StatusCodes.Status400BadRequest,
                            Success = false
                        });
                    }
                }

            }
            catch (ArgumentException err)
            {
                return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<TransactionCardResultModel>()
                {
                    Data = new TransactionCardResultModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<TransactionCardResultModel>()
                {
                    Data = new TransactionCardResultModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("searchtransacoes"), Authorize(Roles = "Administrador, GestorFinanceiro")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<TransactionSimplifiedResultModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<TransactionSimplifiedResultModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<TransactionSimplifiedResultModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchTransactions([FromQuery] SearchTransacoesModel searchTransacoesModel)
        {
            try
            {
                var result = await _financeiroTransacaoService.SearchTransacoes(searchTransacoesModel);
                if (result == null)
                    return Ok(new ResultWithPaginationModel<List<TransactionCardResultModel>>()
                    {
                        Data = new List<TransactionCardResultModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        LastPageNumber = -1,
                        PageNumber = -1,
                        Success = true
                    });
                else return Ok(new ResultWithPaginationModel<List<TransactionSimplifiedResultModel>>(result.Value.transactionResult)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    LastPageNumber = result.Value.lastPageNumber,
                    PageNumber = result.Value.pageNumber
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultWithPaginationModel<List<TransactionCardResultModel>>()
                {
                    Data = new List<TransactionCardResultModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultWithPaginationModel<List<TransactionCardResultModel>>()
                {
                    Data = new List<TransactionCardResultModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("cancelartransacao"), Authorize(Roles = "Administrador, GestorFinanceiro")]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelarTransacaoCartao([FromQuery] string paymentId)
        {
            try
            {
                var result = await _financeiroTransacaoService.CancelCardTransaction(paymentId);
                if (result == null)
                    return Ok(new SW_PortalProprietario.Application.Models.ResultModel<bool>()
                    {
                        Data = false,
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else
                {
                    if (result.GetValueOrDefault(false))
                    {
                        return Ok(new SW_PortalProprietario.Application.Models.ResultModel<bool>(true)
                        {
                            Errors = new List<string>(),
                            Status = StatusCodes.Status200OK,
                            Success = true
                        });
                    }
                    else
                    {
                        return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<bool>(false)
                        {
                            Errors = new List<string>() { "Não foi possível cancelar o pagamento informado" },
                            Status = StatusCodes.Status400BadRequest,
                            Success = false
                        });
                    }
                }

            }
            catch (ArgumentException err)
            {
                return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<List<TransactionCardResultModel>>()
                {
                    Data = new List<TransactionCardResultModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                    Success = false
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<List<TransactionCardResultModel>>()
                {
                    Data = new List<TransactionCardResultModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }

        [HttpPost("gerarQrCodePagamentoComPix"), Authorize(Roles = "Administrador, GestorFinanceiro")]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<TransactionPixResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<TransactionPixResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<TransactionPixResultModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GerarLinkPagamentoPix([FromBody] DoTransactionPixInputModel doTransactionInputModel)
        {
            try
            {
                var result = await _financeiroTransacaoService.GeneratePixTransaction(doTransactionInputModel);
                if (result == null)
                    return Ok(new SW_PortalProprietario.Application.Models.ResultModel<TransactionPixResultModel>()
                    {
                        Data = new TransactionPixResultModel(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else
                {
                    if (result.errors == null || !result.errors.Any())
                    {
                        return Ok(new SW_PortalProprietario.Application.Models.ResultModel<TransactionPixResultModel>(result)
                        {
                            Errors = new List<string>(),
                            Status = StatusCodes.Status200OK,
                            Success = true
                        });
                    }
                    else
                    {
                        return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<TransactionPixResultModel>()
                        {
                            Data = new TransactionPixResultModel(),
                            Errors = result.errors.AsList(),
                            Status = StatusCodes.Status400BadRequest,
                        });
                    }
                }

            }
            catch (ArgumentException err)
            {
                return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<TransactionPixResultModel>()
                {
                    Data = new TransactionPixResultModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<TransactionPixResultModel>()
                {
                    Data = new TransactionPixResultModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("salvarcontabancaria")]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SalvarContaBancaria([FromBody] ClienteContaBancariaInputModel request)
        {
            try
            {
                var result = await _financeiroProviderService.SalvarContaBancaria(request);
                if (result > 0)
                    return Ok(new SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>()
                    {
                        Data = new ClienteContaBancariaViewModel(),
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK,
                    });
                else return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>()
                {
                    Data = new ClienteContaBancariaViewModel(),
                    Errors = new List<string>() { $"Não foi possível salvar a conta bancária" },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>()
                {
                    Data = new ClienteContaBancariaViewModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>()
                {
                    Data = new ClienteContaBancariaViewModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("contasbancarias")]
        [ProducesResponseType(typeof(IEnumerable<ClienteContaBancariaViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SearchContaBancariaFornecedor([FromQuery] int pessoaId)
        {
            try
            {
                var result = await _financeiroProviderService.GetContasBancarias(pessoaId);
                if (result == null || !result.Any())
                    return Ok(new SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>()
                    {
                        Data = new List<ClienteContaBancariaViewModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound
                    });
                return Ok(new SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>()
                {
                    Data = new List<ClienteContaBancariaViewModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>()
                {
                    Data = new List<ClienteContaBancariaViewModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
