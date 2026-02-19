using AccessCenterDomain.AccessCenter;
using CMDomain.Models.Financeiro;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace SW_PortalProprietario.Infra.Ioc.Broker
{
    public class NovaXSPaymentBroker : IBroker
    {
        private const string CARD_TRANSACTION_CACHE_KEY = "CardTransactionResultFinalizacaoPendente_";
        private const int CARD_CACHE_DB = 1;
        private const string PIX_TRANSACTION_CACHE_KEY = "PixTransactionResultFinalizacaoPendente_";
        private const int PIX_CACHE_DB = 1;


        private readonly IConfiguration _configuration;
        private readonly ILogger<NovaXSPaymentBroker> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IRepositoryHosted _repository;
        private readonly IFinanceiroHybridProviderService _financeiroProviderService;
        private readonly BrokerModel? _brokerModel;
        private readonly ICacheStore _cacheStore;


        public NovaXSPaymentBroker(IConfiguration configuration,
            ILogger<NovaXSPaymentBroker> logger,
            IHttpClientFactory clientFactory,
            IRepositoryHosted repository,
            IFinanceiroHybridProviderService financeiroProviderService,
            IOptions<BrokerModel> brokerConfig,
            ICacheStore cacheStore)
        {
            _configuration = configuration;
            _logger = logger;
            _clientFactory = clientFactory;
            _repository = repository;
            _financeiroProviderService = financeiroProviderService;
            _brokerModel = brokerConfig.Value;
            _cacheStore = cacheStore;
        }

        public async Task<TransactionCancelResultModel?> CancelCardTransaction(TransactionCancelModel transactionCancelModel, int empresaLegadoId)
        {
            return await DoCancelPaymentExecute(transactionCancelModel, empresaLegadoId);
        }

        public async Task<TransactionCardResultModel?> DoCardTransaction(TransactionCardModel transactionCardModel, int empresaLegadoId)
        {
            return await DoCardPaymentExecute(transactionCardModel,empresaLegadoId);
        }

        public async Task<TransactionPixResultModel?> GeneratePixTransaction(TransactionPixModel transactionPixModel, int empresaLegadoId)
        {
            return await GeneratePixExecute(transactionPixModel, empresaLegadoId);
        }

        public async Task<TransactionCardResultModel?> GetTransactionResult(string paymentId, int empresaLegadoId)
        {
            return await GetTransactonPixResultExecute(paymentId, empresaLegadoId);
        }

        public async Task<CardTokenizedModel?> Tokenize(CardTokenizeInputModel cardModel, int empresaLegadoId)
        {
            return await TokenizeCardPaymentExecute(cardModel, empresaLegadoId);
        }

        private async Task<CardTokenizedModel?> TokenizeCardPaymentExecute(CardTokenizeInputModel cardModel, int empresaLegadoId)
        {

            CardTokenizedModel? result = null;
            try
            {

                if (_brokerModel == null || string.IsNullOrEmpty(_brokerModel.CardCompanyId) || 
                    string.IsNullOrEmpty(_brokerModel.CardCompanyToken) || 
                    string.IsNullOrEmpty(_brokerModel.ApiCardTokenizeUrl))
                    throw new ArgumentException("Broker para pagamento não configurado corretamente.");

                var strJson = System.Text.Json.JsonSerializer.Serialize(cardModel);

                var content = new StringContent(strJson, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_brokerModel.ApiCardTokenizeUrl!));
                request.Content = content;

                var companyId = _brokerModel.GetCardCompanyId(empresaLegadoId);
                var companyToken = _brokerModel.GetCardCardCompanyToken(empresaLegadoId);


                HttpResponseMessage resp = null;
                using (var client = _clientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri($"{_brokerModel.ApiCardTokenizeUrl!}");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("CompanyId", $"{companyId}");
                    client.DefaultRequestHeaders.Add("CompanyToken", $"{companyToken}");

                    resp = await client.SendAsync(request);
                    if (resp.IsSuccessStatusCode)
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        result = System.Text.Json.JsonSerializer.Deserialize<CardTokenizedModel>(jsonString);
                        if (result != null && !string.IsNullOrEmpty(jsonString) && jsonString.Contains("error", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.errors = new List<string>()
                            {
                               jsonString
                            };
                        }
                    }
                    else
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        _logger.LogError($"Erro ao criar token de cartão do cliente: {cardModel.card?.card_holder} retorno: {resp.StatusCode.ToString()} mensagem: {jsonString} request body: {resp?.RequestMessage?.Content?.ReadAsStringAsync().Result}");
                        result = new CardTokenizedModel()
                        {
                            errors = new List<string>()
                            {
                                jsonString
                            }
                        };
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError($"Erro ao tokeninzar o cartão do cliente: {cardModel.card?.card_holder} retorno: {err.Message}");
            }

            return result;

        }

        private async Task<TransactionCardResultModel?> DoCardPaymentExecute(TransactionCardModel transactionCardModel, int empresaLegadoId)
        {

            var jsonContent = System.Text.Json.JsonSerializer.Serialize(transactionCardModel);

            TransactionCardResultModel? result = null;
            try
            {

                if (_brokerModel == null || string.IsNullOrEmpty(_brokerModel.CardCompanyId) ||
                    string.IsNullOrEmpty(_brokerModel.CardCompanyToken) ||
                    string.IsNullOrEmpty(_brokerModel.ApiPaymentUrl))
                    throw new ArgumentException("Broker para pagamento não configurado corretamente.");


                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_brokerModel.ApiPaymentUrl!));
                request.Content = content;

                var companyId = _brokerModel.GetCardCompanyId(empresaLegadoId);
                var companyToken = _brokerModel.GetCardCardCompanyToken(empresaLegadoId);


                HttpResponseMessage resp = null;
                using (var client = _clientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri($"{_brokerModel.ApiPaymentCancelUrl!}");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("CompanyId", $"{companyId}");
                    client.DefaultRequestHeaders.Add("CompanyToken", $"{companyToken}");

                    resp = await client.SendAsync(request);
                    if (resp.IsSuccessStatusCode)
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        result = System.Text.Json.JsonSerializer.Deserialize<TransactionCardResultModel>(jsonString);
                        if (result != null && !string.IsNullOrEmpty(jsonString) && jsonString.Contains("error", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.errors = new List<string>()
                            {
                               jsonString
                            };
                        }
                    }
                    else
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        _logger.LogError($"Erro ao transacionar cartão do cliente: {transactionCardModel.customer?.name} retorno: {resp.StatusCode.ToString()} mensagem: {jsonString} request body: {resp?.RequestMessage?.Content?.ReadAsStringAsync().Result}");
                        result = new TransactionCardResultModel()
                        {
                            errors = new List<string>()
                            {
                                jsonString
                            }
                        };
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError($"Erro ao transacionar cartão do cliente: {transactionCardModel.customer?.name} retorno: {err.Message} -  body: {jsonContent}");
            }

            return result;

        }

        private async Task<TransactionCancelResultModel?> DoCancelPaymentExecute(TransactionCancelModel transactionCancelCardModel, int empresaLegadoId)
        {

            if (_brokerModel == null || string.IsNullOrEmpty(_brokerModel.CardCompanyId) ||
             string.IsNullOrEmpty(_brokerModel.CardCompanyToken) ||
             string.IsNullOrEmpty(_brokerModel.ApiPaymentUrl))
                throw new ArgumentException("Broker para pagamento não configurado corretamente.");

            TransactionCancelResultModel? result = null;
            try
            {

                var url = $"{_brokerModel.ApiPaymentCancelUrl}";
                
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(transactionCancelCardModel), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{url}"));
                request.Content = content;

                var companyId = _brokerModel.GetCardCompanyId(empresaLegadoId);
                var companyToken = _brokerModel.GetCardCardCompanyToken(empresaLegadoId);

                HttpResponseMessage resp = null;
                using (var client = _clientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri($"{url}");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("CompanyId", $"{companyId}");
                    client.DefaultRequestHeaders.Add("CompanyToken", $"{companyToken}");

                    resp = await client.SendAsync(request);
                    if (resp.IsSuccessStatusCode)
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        result = System.Text.Json.JsonSerializer.Deserialize<TransactionCancelResultModel>(jsonString);
                        if (result != null && !string.IsNullOrEmpty(jsonString) && jsonString.Contains("error", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.errors = new List<string>()
                            {
                               jsonString
                            };

                            if (jsonString.Contains("cancelled", StringComparison.CurrentCultureIgnoreCase))
                                result.status = "cancelled";
                        }
                    }
                    else
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        _logger.LogError($"Erro ao cancelar o pagamento de id: {transactionCancelCardModel.payment_id} retorno: {resp.StatusCode.ToString()} mensagem: {jsonString} request body: {resp?.RequestMessage?.Content?.ReadAsStringAsync().Result}");
                        result = new TransactionCancelResultModel()
                        {
                            errors = new List<string>()
                            {
                                jsonString
                            }
                        };
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError($"Erro ao transacionar cartão do cliente: {transactionCancelCardModel.payment_id} retorno: {err.Message}");
            }

            return result;

        }

        private async Task<TransactionPixResultModel> GeneratePixExecute(TransactionPixModel transactionPixModel, int empresaLegadoId)
        {

            TransactionPixResultModel? result = null;
            try
            {

                var expirationTime = $"{_configuration.GetValue("NovaXSBroker:ExpirationLinkInMinutes", 30)}";
                transactionPixModel.expiration = Convert.ToInt32(expirationTime);


                if (_brokerModel == null || string.IsNullOrEmpty(_brokerModel.PixCompanyId) ||
                   string.IsNullOrEmpty(_brokerModel.PixCompanyToken) ||
                   string.IsNullOrEmpty(_brokerModel.ApiPaymentUrl))
                    throw new ArgumentException("Broker para pagamento não configurado corretamente.");


                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(transactionPixModel), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_brokerModel.ApiPaymentUrl!));
                request.Content = content;

                var companyId = _brokerModel.GetPixCompanyId(empresaLegadoId);
                var companyToken = _brokerModel.GetPixCompanyToken(empresaLegadoId);



                HttpResponseMessage resp = null;
                using (var client = _clientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri($"{_brokerModel.ApiPaymentUrl}");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("CompanyId", $"{companyId}");
                    client.DefaultRequestHeaders.Add("CompanyToken", $"{companyToken}");

                    resp = await client.SendAsync(request);
                    if (resp.IsSuccessStatusCode)
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        result = System.Text.Json.JsonSerializer.Deserialize<TransactionPixResultModel>(jsonString);

                        if (result != null && !string.IsNullOrEmpty(jsonString) && jsonString.Contains("error", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.errors = new List<string>()
                            {
                               jsonString
                            };
                        }
                        else if (result != null)
                            result.expiration_date = DateTime.Now.AddMinutes(transactionPixModel.expiration.GetValueOrDefault(30) - 1);
                    }
                    else
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        _logger.LogError($"Erro ao gerar PIX do cliente: {transactionPixModel.customer?.name} retorno: {resp.StatusCode.ToString()} mensagem: {jsonString} request body: {resp?.RequestMessage?.Content?.ReadAsStringAsync().Result}");
                        result = new TransactionPixResultModel()
                        {
                            errors = new List<string>()
                            {
                                jsonString
                            }
                        };
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError($"Erro ao gerar pix para o cliente: {transactionPixModel.customer?.name} retorno: {err.Message}");
            }

            return result;

        }

        private async Task<TransactionCardResultModel> GetTransactonPixResultExecute(string paymentId, int empresaLegadoId)
        {

            TransactionCardResultModel? result = null;
            try
            {

                if (_brokerModel == null || string.IsNullOrEmpty(_brokerModel.PixCompanyId) ||
                   string.IsNullOrEmpty(_brokerModel.PixCompanyToken) ||
                   string.IsNullOrEmpty(_brokerModel.ApiPaymentUrl))
                    throw new ArgumentException("Broker para pagamento não configurado corretamente.");

            
                var companyId = _brokerModel.GetPixCompanyId(empresaLegadoId);
                var companyToken = _brokerModel.GetPixCompanyToken(empresaLegadoId);


                var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_brokerModel.ApiPaymentConsultTransactionUrl}"));

                HttpResponseMessage resp = null;
                using (var client = _clientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri($"{_brokerModel.ApiPaymentConsultTransactionUrl}");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("CompanyId", $"{companyId}");
                    client.DefaultRequestHeaders.Add("CompanyToken", $"{companyToken}");

                    resp = await client.SendAsync(request);
                    if (resp.IsSuccessStatusCode)
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        result = System.Text.Json.JsonSerializer.Deserialize<TransactionCardResultModel>(jsonString);
                        if (result != null && !string.IsNullOrEmpty(jsonString) && jsonString.Contains("error", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.errors = new List<string>()
                            {
                               jsonString,
                            };
                        }

                        if (result != null)
                            result.statusCode = resp != null ? $"{resp.StatusCode}" : null;
                    }
                    else
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        _logger.LogError($"Erro ao consultar o pagamento id: {paymentId} retorno: {resp.StatusCode.ToString()} mensagem: {jsonString} request body: {resp?.RequestMessage?.Content?.ReadAsStringAsync().Result}");
                        result = new TransactionCardResultModel()
                        {
                            errors = new List<string>()
                            {
                                jsonString,
                            },
                            statusCode = resp != null ? $"{resp.StatusCode}" : null
                        };
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError($"Erro ao consultar o pagamento id: {paymentId} retorno: {err.Message}");
            }

            return result;

        }

        private async Task<TransactionCardResultModel> GetTransactonCardResultExecute(string paymentId, int empresaLegadoId)
        {

            TransactionCardResultModel? result = null;
            try
            {

                if (_brokerModel == null || string.IsNullOrEmpty(_brokerModel.CardCompanyId) ||
                   string.IsNullOrEmpty(_brokerModel.CardCompanyToken) ||
                   string.IsNullOrEmpty(_brokerModel.ApiPaymentUrl))
                    throw new ArgumentException("Broker para pagamento não configurado corretamente.");


                var companyId = _brokerModel.GetCardCompanyId(empresaLegadoId);
                var companyToken = _brokerModel.GetCardCardCompanyToken(empresaLegadoId);


                var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_brokerModel.ApiPaymentConsultTransactionUrl}"));

                HttpResponseMessage resp = null;
                using (var client = _clientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri($"{_brokerModel.ApiPaymentConsultTransactionUrl}");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("CompanyId", $"{companyId}");
                    client.DefaultRequestHeaders.Add("CompanyToken", $"{companyToken}");

                    resp = await client.SendAsync(request);
                    if (resp.IsSuccessStatusCode)
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        result = System.Text.Json.JsonSerializer.Deserialize<TransactionCardResultModel>(jsonString);
                        if (result != null && !string.IsNullOrEmpty(jsonString) && jsonString.Contains("error", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result.errors = new List<string>()
                            {
                               jsonString,
                            };
                        }

                        if (result != null)
                            result.statusCode = resp != null ? $"{resp.StatusCode}" : null;
                    }
                    else
                    {
                        string jsonString = resp.Content.ReadAsStringAsync().Result;
                        _logger.LogError($"Erro ao consultar o pagamento id: {paymentId} retorno: {resp.StatusCode.ToString()} mensagem: {jsonString} request body: {resp?.RequestMessage?.Content?.ReadAsStringAsync().Result}");
                        result = new TransactionCardResultModel()
                        {
                            errors = new List<string>()
                            {
                                jsonString,
                            },
                            statusCode = resp != null ? $"{resp.StatusCode}" : null
                        };
                    }
                }

            }
            catch (Exception err)
            {
                _logger.LogError($"Erro ao consultar o pagamento id: {paymentId} retorno: {err.Message}");
            }

            return result;

        }

        public async Task GetTransactionPixResult(PaymentPix item, NHibernate.IStatelessSession? session)
        {
            try
            {
                _repository.BeginTransaction(session);

                bool considerarComoPago = _configuration.GetValue<bool>("NovaXSBroker:Production", true) == false;

                if (!string.IsNullOrEmpty(item.PaymentId))
                {
                    var result = await GetTransactonPixResultExecute(item.PaymentId, item.EmpresaLegadoId.GetValueOrDefault());
                    if (result != null)
                    {
                        if (!string.IsNullOrEmpty(result.statusCode) && result.statusCode.Contains("notfound", StringComparison.CurrentCultureIgnoreCase) && !considerarComoPago)
                        {
                            item.Status = "cancelled";
                            await _repository.ForcedSave(item, session);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(result.status) || considerarComoPago)
                            {
                                if (considerarComoPago || (!string.IsNullOrEmpty(result.status) && result.status.Contains("paid", StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    //Efetuar baixa no Provider
                                    var baixa = await _financeiroProviderService.BaixarValoresPagosEmPix(item, session);
                                    if (baixa != null)
                                    {
                                        if (baixa.Erros == null || !baixa.Erros.Any())
                                        {
                                            item.Status = "captured";
                                            item.AgrupamentoBaixaLegadoId = baixa.Id;
                                            await _repository.ForcedSave(item, session);
                                            await _cacheStore.DeleteByKey($"{PIX_TRANSACTION_CACHE_KEY}{item.PaymentId}");
                                        }
                                        else
                                        {
                                            throw new ArgumentException(baixa.Erros.First());
                                        }
                                    }
                                }
                                else if (item.ValidoAte <= DateTime.Now)
                                {
                                    item.Status = "cancelled";
                                    await _repository.ForcedSave(item, session);
                                    await _cacheStore.DeleteByKey($"{PIX_TRANSACTION_CACHE_KEY}{item.PaymentId}",PIX_CACHE_DB);
                                }
                            }
                        }
                    }
                }
                else
                {
                    item.Status = "cancelled";
                    await _repository.ForcedSave(item, session);
                    await _cacheStore.DeleteByKey($"{PIX_TRANSACTION_CACHE_KEY}{item.PaymentId}",PIX_CACHE_DB);
                }

                var commitResult = await _repository.CommitAsync(session);
                if (commitResult.exception != null)
                    throw commitResult.exception;
            }
            catch (Exception err)
            {
                _repository.Rollback(session);
                _logger.LogError(err, err.Message);
            }
        }

        public async Task GetTransactionCardResult(PaymentCardTokenized item, IStatelessSession? session)
        {
            try
            {


                if (!string.IsNullOrEmpty(item.PaymentId))
                {
                    var result = item;
                    if (result != null)
                    {
                        if (!string.IsNullOrEmpty(result.Status) && !result.Status.Contains("captured", StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.Status = "cancelled";
                            await _repository.ForcedSave(item, session);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(result.Status))
                            {
                                if (!string.IsNullOrEmpty(result.Status) && result.Status.Contains("captured", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    //Efetuar ajustes
                                    try
                                    {
                                        var resultAjustesCartao = await _financeiroProviderService.AlterarTipoContaReceberPagasEmCartao(item, session);
                                        if (resultAjustesCartao != null)
                                        {
                                            if (resultAjustesCartao.Erros == null || !resultAjustesCartao.Erros.Any())
                                            {
                                                item.ParcelasSincronizadas = 1;
                                                await _repository.ForcedSave(item, session);
                                                await _cacheStore.DeleteByKey($"{CARD_TRANSACTION_CACHE_KEY}{item.PaymentId}",CARD_CACHE_DB);
                                            }
                                            else
                                            {
                                                throw new ArgumentException(resultAjustesCartao.Erros.First());
                                            }
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        item.ResultadoSincronizacaoParcelas = err.Message;
                                        item.ParcelasSincronizadas = 0;
                                    }
                                }
                                else if (result!.Status!.Contains("cancelled"))
                                {
                                    item.ParcelasSincronizadas = 0;
                                    item.Status = result.Status;
                                    await _repository.ForcedSave(item, session);
                                    await _cacheStore.DeleteByKey($"{CARD_TRANSACTION_CACHE_KEY}{item.PaymentId}",CARD_CACHE_DB);
                                }
                            }
                        }
                    }
                }
                else
                {
                    item.Status = "cancelled";
                    await _repository.ForcedSave(item, session);
                    await _cacheStore.DeleteByKey($"{CARD_TRANSACTION_CACHE_KEY}{item.PaymentId}", CARD_CACHE_DB);
                }

            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                throw err;
            }
        }
    }
}
