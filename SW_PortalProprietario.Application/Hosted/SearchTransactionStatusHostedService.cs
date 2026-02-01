using CMDomain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate.Engine;
using PuppeteerSharp;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Transactions;

namespace SW_PortalProprietario.Application.Hosted
{
    public class SearchTransactionStatusHostedService : BackgroundService, IBackGroundProcessUpdateTransactionStatus
    {
        private const string CARD_TRANSACTION_CACHE_KEY = "CardTransactionResultFinalizacaoPendente_";
        private const int CARD_CACHE_DB = 1;
        private const string PIX_TRANSACTION_CACHE_KEY = "PixTransactionResultFinalizacaoPendente_";
        private const int PIX_CACHE_DB = 1;
        static bool _stopped = false;
        private readonly IRepositoryHosted _repository;
        private readonly ILogger<SearchTransactionStatusHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBroker _broker;
        private readonly ICacheStore _cacheStore;
        private static bool isRunning = false;

        public SearchTransactionStatusHostedService(IRepositoryHosted repository,
            ILogger<SearchTransactionStatusHostedService> logger,
            IConfiguration configuration,
            IServiceBase serviceBase,
            IBroker broker,
            ICacheStore cacheStore)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
            _broker = broker;
            _cacheStore = cacheStore;
        }

        public bool Stopped
        {
            get
            {
                return _stopped;
            }
            set { _stopped = value; }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!Stopped && !isRunning)
            {
                isRunning = true;
                await Execute();
            }
        }

        private async Task Execute()
        {
            await Task.Delay(_configuration.GetValue("TimeWaitInSecondsSearchPixResult", 10) * 1000);
            await SearchPixAndCardResult();
        }

        private async Task SearchPixAndCardResult()
        {
            using (var session = _repository.CreateSession())
            {
                List<string> paymentCardCache = new List<string>();
                try
                {
                    var paymentPixCache = await _cacheStore.GetKeysAsync<string>($"{PIX_TRANSACTION_CACHE_KEY}", PIX_CACHE_DB, _repository.CancellationToken);

                    var itensToUpdate = (await _repository.FindByHql<PaymentPix>(@"From 
                                                                             PaymentPix pp 
                                                                            Where 
                                                                             (Lower(pp.Status) = 'pending' or (Lower(pp.Status) = 'captured' and Coalesce(pp.AgrupamentoBaixaLegadoId,'0') = '0')) and 
                                                                             pp.PaymentId is not null", session)).AsList();


                    foreach (var item in itensToUpdate)
                    {
                        try
                        {
                            await _broker.GetTransactionPixResult(item, session);
                        }
                        catch (Exception err)
                        {
                            _logger.LogError($"Erro ao consultar transação PIX: {item.Payment_Id}");
                        }
                    }

                    paymentCardCache = await _cacheStore.GetKeysAsync<string>($"{CARD_TRANSACTION_CACHE_KEY}", CARD_CACHE_DB, _repository.CancellationToken) ?? new List<string>();

                    var pagamentosEmCartaoPendentesProcessamento = (await _repository.FindByHql<PaymentCardTokenized>(@"From 
                                                                             PaymentCardTokenized pc 
                                                                            Where 
                                                                             (Lower(pc.Status) = 'captured' and Coalesce(pc.ParcelasSincronizadas,0) = 0 )
                                                                             and pc.PaymentId is not null", session)).AsList();

                    try
                    {
                        _repository.BeginTransaction(session);
                        await GravarTransacaoCartaoSeNaoExistirNaBaseDados(session, paymentCardCache, pagamentosEmCartaoPendentesProcessamento);
                        var commitResult = await _repository.CommitAsync(session);
                        if (!commitResult.executed)
                            throw commitResult.exception ?? new ArgumentException("Falha na finalização da transação.");

                        foreach (var item in paymentCardCache)
                        {
                            await _cacheStore.DeleteByKey($"{CARD_TRANSACTION_CACHE_KEY}{item}", CARD_CACHE_DB);
                        }
                       
                    }
                    catch (Exception err)
                    {
                        _logger.LogError($"Erro ao consultar transação.");
                        _repository.Rollback(session);
                    }

                    pagamentosEmCartaoPendentesProcessamento = (await _repository.FindByHql<PaymentCardTokenized>(@"From 
                                                                             PaymentCardTokenized pc 
                                                                            Where 
                                                                             (Lower(pc.Status) = 'captured' and Coalesce(pc.ParcelasSincronizadas,0) = 0 )
                                                                             and pc.PaymentId is not null", session)).AsList();

                    foreach (var item in pagamentosEmCartaoPendentesProcessamento)
                    {
                        try
                        {
                            _repository.BeginTransaction(session);
                            await _broker.GetTransactionCardResult(item, session);
                            var commitResult = await _repository.CommitAsync(session);
                            if (!commitResult.executed)
                                throw commitResult.exception ?? new ArgumentException("Falha na finalização da transação.");

                            await _cacheStore.DeleteByKey($"{CARD_TRANSACTION_CACHE_KEY}{item}", CARD_CACHE_DB);
                        }
                        catch (Exception err)
                        {
                            _logger.LogError($"Erro ao consultar transação em Cartão: {item.PaymentId}");
                            _repository.Rollback(session);
                        }
                    }
                }
                catch (Exception err)
                {
                    _logger.LogError(err, err.Message);
                }
            }
            isRunning = false;

        }

        private async Task GravarTransacaoCartaoSeNaoExistirNaBaseDados(NHibernate.IStatelessSession? session, List<string> paymentCardCache, List<PaymentCardTokenized>? pagamentosEmCartaoPendentesProcessamento)
        {
            foreach (var item in paymentCardCache)
            {
                var itemCache = await _cacheStore.GetAsync<TransactionCacheModel>(item, CARD_CACHE_DB, _repository.CancellationToken);
                if (itemCache != null)
                {

                    if (pagamentosEmCartaoPendentesProcessamento == null ||
                        !pagamentosEmCartaoPendentesProcessamento.Any(c => c.PaymentId == itemCache.TransactionId) &&
                        itemCache.TransactionCardResult != null && itemCache.TransactionCard != null)
                    {
                        var itemExistenteEmOutroStatus = (await _repository.FindByHql<PaymentCardTokenized>(@"From 
                                                                             PaymentCardTokenized pc 
                                                                            Where 
                                                                             pc.PaymentId is not null", session)).AsList();
                        if (itemExistenteEmOutroStatus == null)
                        {

                            if (itemCache != null && !string.IsNullOrEmpty(itemCache?.TransactionCardResult?.last_acquirer_response?.response?.authorization) &&
                                !string.IsNullOrEmpty(itemCache?.TransactionCardResult?.last_acquirer_response?.response?.nsu) &&
                                !string.IsNullOrEmpty(itemCache?.TransactionCardResult?.last_acquirer_response?.status) &&
                                itemCache.TransactionCardResult.last_acquirer_response.status.Contains("captured", StringComparison.CurrentCultureIgnoreCase))
                            {

                                var paymentCardTokenized = new PaymentCardTokenized()
                                {
                                    CardTokenized = itemCache.CardTokenized,
                                    Valor = itemCache.TransactionCard?.payment?.value,
                                    PaymentId = itemCache.TransactionId,
                                    Status = itemCache.TransactionCardResult?.last_acquirer_response?.status,
                                    Nsu = itemCache.TransactionCardResult?.last_acquirer_response?.response?.nsu,
                                    CodigoAutorizacao = itemCache.TransactionCardResult?.last_acquirer_response?.response?.authorization,
                                    Adquirente = itemCache.TransactionCardResult?.company?.acquirer,
                                    AdquirentePaymentId = itemCache.TransactionCardResult?.last_acquirer_response?.response?.payment_id,
                                    TransactionId = itemCache.TransactionCardResult?.last_acquirer_response?.response?.transaction,
                                    Url = itemCache.TransactionCardResult?.last_acquirer_response?.response?.url,
                                    CompanyId = itemCache.TransactionCardResult?.company?.id,
                                    DadosEnviados = System.Text.Json.JsonSerializer.Serialize(itemCache.TransactionCard),
                                    Retorno = System.Text.Json.JsonSerializer.Serialize(itemCache.TransactionCardResult),
                                    EmpresaLegadoId = !string.IsNullOrEmpty(itemCache.EmpresaLogadaId) ? int.Parse(itemCache.EmpresaLogadaId) : -1
                                };

                                await _repository.ForcedSave(paymentCardTokenized, session);

                                //Gravo os dos ítens pagos na operação
                                if (itemCache.Contas != null)
                                {
                                    foreach (var conta in itemCache.Contas)
                                    {
                                        var itemPaid = new PaymentCardTokenizedItem()
                                        {
                                            PaymentCardTokenized = paymentCardTokenized,
                                            ValorNaTransacao = conta.ValorAtualizado,
                                            Valor = conta.Valor,
                                            Vencimento = conta.Vencimento,
                                            ItemId = $"{conta.Id}",
                                            DescricaoDoItem = $"PessoaId:{conta.PessoaId}|PessoaProviderId:{conta.PessoaProviderId}|CódigoTipoConta:{conta.CodigoTipoConta}|NomeTipoConta:{conta.NomeTipoConta}|Vencimento:{conta.Vencimento:dd/MM/yyyy}"
                                        };
                                        await _repository.ForcedSave(itemPaid, session);
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }
        
    }
}
