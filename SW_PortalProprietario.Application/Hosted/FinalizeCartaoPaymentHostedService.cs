using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Application.Hosted
{
    public class FinalizeCartaoPaymentHostedService : BackgroundService, IBackGroundProcessFinalizeTransactionCard
    {
        static bool _stopped = false;
        private readonly IRepositoryHosted _repository;
        private readonly ILogger<FinalizeCartaoPaymentHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFinanceiroHybridProviderService _financeiroProviderService;

        public FinalizeCartaoPaymentHostedService(IRepositoryHosted repository,
            ILogger<FinalizeCartaoPaymentHostedService> logger,
            IConfiguration configuration,
            IBroker broker,
            IFinanceiroHybridProviderService financeiroProviderService)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
            _financeiroProviderService = financeiroProviderService;
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
            while (!Stopped)
            {
                await Execute();
            }
        }

        private async Task Execute()
        {
            await Task.Delay(_configuration.GetValue("TimeWaitInSecondsFinalizeCartaoResult", 10) * 1000);
            await FinalizeTransactions();

        }

        private async Task FinalizeTransactions()
        {
            try
            {
                using (var session = _repository.CreateSession())
                {
                    var itensToFinalize = (await _repository.FindByHql<PaymentCardTokenized>(@"From 
                                                                             PaymentCardTokenized pp 
                                                                            Where 
                                                                             Lower(pp.Status) = 'captured' and 
                                                                             pp.PaymentId is not null and 
                                                                             (pp.ParcelasSincronizadas is null or pp.ParcelasSincronizadas = 0)", session)).AsList();


                    foreach (var item in itensToFinalize)
                    {
                        var itemResult = await _financeiroProviderService.AlterarTipoContaReceberPagasEmCartao(item, session);
                        if (itemResult != null && itemResult.Erros != null && itemResult.Erros.Any())
                        {
                            throw new Exception(itemResult.Erros.First());
                        }
                    }
                }


            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
            }

        }
    }
}
