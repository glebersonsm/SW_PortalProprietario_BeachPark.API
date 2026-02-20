using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using System.IO.Compression;

namespace SW_PortalCliente_BeachPark.API.src.Controllers.Financeiro
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class FinanceiroUsuarioController : ControllerBase
    {

        private readonly IFinanceiroHybridProviderService _financeiroProviderService;
        private readonly IFinanceiroTransacaoUsuarioService _financeiroTransacaoUsuarioService;
        private readonly ICertidaoFinanceiraService _certidaoFinanceiraService;

        public FinanceiroUsuarioController(IFinanceiroTransacaoUsuarioService financeiroTransacaoUsuarioService,
            ICertidaoFinanceiraService certidaoFinanceiraService,
            IFinanceiroHybridProviderService financeiroProviderService)
        {
            _financeiroTransacaoUsuarioService = financeiroTransacaoUsuarioService;
            _certidaoFinanceiraService = certidaoFinanceiraService;
            _financeiroProviderService = financeiroProviderService;
        }

        [HttpGet("searchContasPedentesDoUsuarioLogado"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContaPendenteModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContaPendenteModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultWithPaginationModel<List<ContaPendenteModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchContaPendenteDoUsuarioLogado([FromQuery] SearchContasPendentesUsuarioLogado searchModel)
        {
            try
            {
                var result = await _financeiroProviderService.GetContaPendenteDoUsuario(searchModel);
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

        [HttpGet("getmytokenizedcards"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(ResultModel<List<CardTokenizedModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<List<CardTokenizedModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<List<CardTokenizedModel>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyTokenizedCard()
        {
            try
            {
                var result = await _financeiroTransacaoUsuarioService.GetMyTokenizedCards();
                if (result == null || !result.Any())
                    return Ok(new ResultModel<List<CardTokenizedModel>>()
                    {
                        Data = new List<CardTokenizedModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<List<CardTokenizedModel>>(result.AsList())
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<List<CardTokenizedModel>>()
                {
                    Data = new List<CardTokenizedModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<List<CardTokenizedModel>>()
                {
                    Data = new List<CardTokenizedModel>(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("tokenizemycard"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(ResultModel<CardTokenizedModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<CardTokenizedModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<CardTokenizedModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TokenizeMyCard([FromBody] TokenizeMyCardInputModel cardModel)
        {
            try
            {
                var result = await _financeiroTransacaoUsuarioService.TokenizeMyCard(cardModel);
                if (result == null)
                    return Ok(new ResultModel<CardTokenizedModel>()
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
                        return Ok(new ResultModel<CardTokenizedModel>(result)
                        {
                            Errors = new List<string>(),
                            Status = StatusCodes.Status200OK
                        });
                    }
                    else
                    {
                        return BadRequest(new ResultModel<CardTokenizedModel>()
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
                return BadRequest(new ResultModel<CardTokenizedModel>()
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
                return StatusCode(500, new ResultModel<CardTokenizedModel>()
                {
                    Data = new CardTokenizedModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("deletemycard"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTokenizedCard([FromBody] DeleteTokenizedCardModel model)
        {
            try
            {
                var result = await _financeiroTransacaoUsuarioService.RemoveMyCardTokenized(model.Tokenizedcard.GetValueOrDefault(0));
                if (result)
                    return Ok(new ResultModel<bool>()
                    {
                        Data = result,
                        Errors = new List<string>(),
                        Status = StatusCodes.Status200OK
                    });
                else return BadRequest(new ResultModel<CardTokenizedModel>()
                {
                    Data = new CardTokenizedModel(),
                    Errors = new List<string>() { $"Não foi possível remover o cartão de id: {model.Tokenizedcard}" },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<CardTokenizedModel>()
                {
                    Data = new CardTokenizedModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<CardTokenizedModel>()
                {
                    Data = new CardTokenizedModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpPost("transacionarcomcartao"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(ResultModel<TransactionCardResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<TransactionCardResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TransactionCardResultModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DoCardTransaction([FromBody] DoTransactionCardInputModel doTransactionInputModel)
        {
            try
            {
                var result = await _financeiroTransacaoUsuarioService.DoCardTransaction(doTransactionInputModel);
                if (result == null)
                    return Ok(new ResultModel<TransactionCardResultModel>()
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
                        return Ok(new ResultModel<TransactionCardResultModel>(result)
                        {
                            Errors = new List<string>(),
                            Status = StatusCodes.Status200OK,
                            Success = true
                        });
                    }
                    else
                    {
                        return BadRequest(new ResultModel<TransactionCardResultModel>()
                        {
                            Data = new TransactionCardResultModel(),
                            Success = false,
                            Errors = result.errors.AsList(),
                            Message = result.errors.Any() ? result.errors.First() : "Erro no processamento",
                            Status = StatusCodes.Status400BadRequest,
                        });
                    }
                }

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<TransactionCardResultModel>()
                {
                    Data = new TransactionCardResultModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Success = false,
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<TransactionCardResultModel>()
                {
                    Data = new TransactionCardResultModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError,
                    Success = false
                });
            }
        }


        [HttpPost("gerarQrCodePagamentoComPix"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(ResultModel<TransactionPixResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<TransactionPixResultModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<TransactionPixResultModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GerarLinkPagamentoPix([FromBody] DoTransactionPixInputModel doTransactionInputModel)
        {
            try
            {
                var result = await _financeiroTransacaoUsuarioService.GeneratePixTransaction(doTransactionInputModel);
                if (result == null)
                    return Ok(new ResultModel<TransactionPixResultModel>()
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
                        return Ok(new ResultModel<TransactionPixResultModel>(result)
                        {
                            Errors = new List<string>(),
                            Status = StatusCodes.Status200OK
                        });
                    }
                    else
                    {
                        return BadRequest(new ResultModel<TransactionPixResultModel>()
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
                return BadRequest(new ResultModel<TransactionPixResultModel>()
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
                return StatusCode(500, new ResultModel<TransactionPixResultModel>()
                {
                    Data = new TransactionPixResultModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("certidaofinanceira"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GerarCertidaoFinanceira(GeracaoCertidaoInputModel geracaoCertidaoInputModel)
        {
            try
            {
                //Ajustar datas
                var result = await _certidaoFinanceiraService.GerarCertidaoNegativaPositivaDeDebitosFinanceiros(geracaoCertidaoInputModel);
                if (result == null || !result.Any())
                    return NotFound(new DeleteResultModel()
                    {
                        Result = "Não foi possível gerar certidão de situação financeira",
                        Errors = new List<string>() { "Nenhuma certidão foi gerada" },
                        Status = StatusCodes.Status404NotFound,
                    });

                var memoryStream = new MemoryStream();
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in result)
                    {
                        if (System.IO.File.Exists(file.Path))
                        {
                            var entry = zipArchive.CreateEntry(file.FileName);
                            using (var entryStream = entry.Open())
                            using (var fileStream = System.IO.File.OpenRead(file.Path))
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        else return NotFound(new DeleteResultModel()
                        {
                            Result = "Não foi possível gerar certidão de situação financeira",
                            Errors = new List<string>() { "Nenhuma certidão foi gerada" },
                            Status = StatusCodes.Status404NotFound,
                        });
                    }
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream, "application/zip", "certidoes.zip");


            }
            catch (FileNotFoundException err)
            {
                return NotFound(new DeleteResultModel()
                {
                    Result = "Não gerada",
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
                    Result = "Não gerada",
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
                    Result = "Não gerada",
                    Errors = err.InnerException != null ?
                    new List<string>() { err.Message, err.InnerException.Message } :
                    new List<string>() { err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("validarprotocolo/{protocolo}")]
        [ProducesResponseType(typeof(ResultModel<CertidaoViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultModel<CertidaoViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultModel<CertidaoViewModel>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidarProtocolo(string protocolo)
        {
            try
            {
                var result = await _certidaoFinanceiraService.ValidarCertidao(protocolo);
                if (result == null)
                    return Ok(new ResultModel<CertidaoViewModel>()
                    {
                        Data = new CertidaoViewModel(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
                    });
                else return Ok(new ResultModel<CertidaoViewModel>(result)
                {
                    Errors = new List<string>(),
                    Status = StatusCodes.Status200OK,
                    Success = true
                });

            }
            catch (ArgumentException err)
            {
                return BadRequest(new ResultModel<CertidaoViewModel>()
                {
                    Data = new CertidaoViewModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            catch (Exception err)
            {
                return StatusCode(500, new ResultModel<CertidaoViewModel>()
                {
                    Data = new CertidaoViewModel(),
                    Errors = err.InnerException != null ?
                    new List<string>() { $"Não foi possível retornar os dados", err.Message, err.InnerException.Message } :
                    new List<string>() { $"Não foi possível retornar os dados", err.Message },
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("downloadBoleto"), Authorize(Roles = "Administrador, GestorFinanceiro, Usuario")]
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

        [HttpPost("salvarMinhaContaBancaria")]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<ClienteContaBancariaViewModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SalvarContaBancaria([FromBody] ClienteContaBancariaInputModel request)
        {
            try
            {
                var result = await _financeiroProviderService.SalvarMinhaContaBancaria(request);
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

        [HttpGet("minhasContasBancarias")]
        [ProducesResponseType(typeof(IEnumerable<ClienteContaBancariaViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> SearchContaBancariaFornecedor()
        {
            try
            {
                var result = await _financeiroProviderService.GetMinhasContasBancarias();
                if (result == null || !result.Any())
                    return Ok(new SW_PortalProprietario.Application.Models.ResultModel<List<ClienteContaBancariaViewModel>>(result)
                    {
                        Data = new List<ClienteContaBancariaViewModel>(),
                        Errors = new List<string>() { "Ops! Nenhum registro encontrado!" },
                        Status = StatusCodes.Status404NotFound,
                        Success = true
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
