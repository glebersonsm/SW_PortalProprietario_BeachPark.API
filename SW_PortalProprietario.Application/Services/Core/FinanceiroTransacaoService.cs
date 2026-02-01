using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using SW_PortalProprietario.Domain.Functions;
using SW_Utils.Auxiliar;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class FinanceiroTransacaoService : IFinanceiroTransacaoService
    {
        private const string CARD_TRANSACTION_CACHE_KEY = "CardTransactionResultFinalizacaoPendente_";
        private const int CARD_CACHE_DB = 1;
        private readonly IRepositoryNH _repository;
        private readonly ILogger<FinanceiroTransacaoService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;
        private readonly IBroker _broker;
        private readonly IFinanceiroProviderService _financeiroProviderService;
        private readonly BrokerModel? _brokerModel;
        private readonly ICacheStore _cache;

        public FinanceiroTransacaoService(IRepositoryNH repository,
            ILogger<FinanceiroTransacaoService> logger,
            IProjectObjectMapper mapper,
            IConfiguration configuration,
            IServiceBase serviceBase,
            IBroker broker,
            IFinanceiroProviderService financeiroProviderService,
            ICommunicationProvider communicationProvider,
            IOptions<BrokerModel> brokerConfig,
            ICacheStore cache)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            _serviceBase = serviceBase;
            _broker = broker;
            _financeiroProviderService = financeiroProviderService;
            _brokerModel = brokerConfig.Value;
            _cache = cache;
        }

        public async Task<bool?> CancelCardTransaction(string paymentId)
        {
            try
            {
                _repository.BeginTransaction();
                var payment = (await _repository.FindByHql<PaymentCardTokenized>($"From PaymentCardTokenized pct Inner Join Fetch pct.CardTokenized ct Where pct.PaymentId = '{paymentId}'")).FirstOrDefault();
                if (payment == null || payment.CardTokenized == null || payment.CardTokenized.EmpresaLegadoId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException($"Não foi encontrado o pagamento em cartão com o id: '{paymentId}' informado");

                var result = await _broker.CancelCardTransaction(new TransactionCancelModel() { payment_id = paymentId, value = payment.Valor },payment.CardTokenized.EmpresaLegadoId.GetValueOrDefault(0));

                if (result != null && !string.IsNullOrWhiteSpace(result.status) && result.status.Equals("cancelled", StringComparison.CurrentCultureIgnoreCase))
                {
                    payment.Status = "cancelled";
                    await _repository.Save(payment);
                    var resultCommit = await _repository.CommitAsync();

                    try
                    {
                        await _cache.DeleteByKey($"{CARD_TRANSACTION_CACHE_KEY}{paymentId}", CARD_CACHE_DB, _repository.CancellationToken);
                    }
                    catch (Exception err)
                    {
                        _logger.LogError("Erro ao deletar dados da transação do cache", err.Message);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                return false;
            }
        }

        public async Task<TransactionCardResultModel?> DoCardTransaction(DoTransactionCardInputModel doTransactionModel)
        {
            throw new NotImplementedException();
            TransactionCardResultModel? transactionCardResultModel = null;
            var jsonBodyRequest = string.Empty;
            var jsonBodyResponse = string.Empty;

            var loggedUser = await _repository.GetLoggedUser();


            if (_repository.IsAdm)
            {
                var systemConfiguration = await _repository.GetParametroSistemaViewModel();
                if (systemConfiguration != null && systemConfiguration.HabilitarPagamentoEmCartao.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)
                    throw new ArgumentException("O sistema está configurado para não permitir pagamento em cartão de crédito/débito.");
            }

            try
            {
                _repository.BeginTransaction();

                if (doTransactionModel.PessoaId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o parâmetro PessoaId");

                var vinculoPessoaProvider = (await _serviceBase.GetPessoaProviderVinculadaPessoaSistema($"{doTransactionModel.PessoaId.GetValueOrDefault()}", _financeiroProviderService.ProviderName));
                if (vinculoPessoaProvider == null || string.IsNullOrEmpty(vinculoPessoaProvider.PessoaProvider))
                    throw new ArgumentException($"Não foi encontrado vínculos da PessoaId informada: {doTransactionModel.PessoaId}");

                if (!loggedUser.Value.isAdm)
                {
                    if (vinculoPessoaProvider == null)
                        throw new ArgumentException("Não foi possível identificar os dados no sistema legado");

                    if (!string.IsNullOrEmpty(vinculoPessoaProvider.PessoaProvider))
                    {
                        var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(vinculoPessoaProvider.PessoaProvider!) });
                        if (propCache != null && propCache.Any(b => b.frAtendimentoStatusCrcModels.Any(b => (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && b.AtendimentoStatusCrcStatus == "A")))
                        {
                            throw new ArgumentException("Não foi possível efetuar o pagamento em cartão, motivo 0001BL");
                        }
                    }
                }


                string idPessoaUtilizar = vinculoPessoaProvider != null && !string.IsNullOrEmpty(vinculoPessoaProvider.PessoaSistema) ? vinculoPessoaProvider.PessoaSistema : $"{doTransactionModel.PessoaId.GetValueOrDefault()}";

                var pessoa = await _repository.FindById<Domain.Entities.Core.DadosPessoa.Pessoa>(Convert.ToInt32(idPessoaUtilizar));
                if (pessoa == null)
                    throw new ArgumentException($"Não foi encontrada pessoa com o Id: {idPessoaUtilizar}");

                if (doTransactionModel.CardTokenizedId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException($"Deve ser informado o CardTokenizedId");

                var usuarioVinculadoPessoa = (await _repository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where p.Id = {pessoa.Id} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();

                var cardExistente = (await _repository.FindByHql<CardTokenized>(@$"From 
                        CardTokenized ct 
                        Inner Join Fetch ct.Pessoa p 
                    Where 
                        p.Id = {pessoa.Id} and 
                        ct.Id = {doTransactionModel.CardTokenizedId.GetValueOrDefault()} and 
                        ct.Visivel = 1 ")).FirstOrDefault();

                if (cardExistente == null)
                    throw new ArgumentException($"Não foi localizado o CardTokenized com o id informado: {doTransactionModel.CardTokenizedId} vinculado a pessoa id: {idPessoaUtilizar}");

                var contas = await _financeiroProviderService.GetContasParaPagamentoEmCartaoGeral(doTransactionModel);
                await ValidarPagamentoEmDuplicidade(contas, pessoa);

                var dadosPessoa = await _financeiroProviderService.GetDadosPessoa(Convert.ToInt32(vinculoPessoaProvider?.PessoaProvider));
                if (dadosPessoa == null)
                    throw new ArgumentException($"Não foi possível encontrar a pessoa com Id: {vinculoPessoaProvider?.PessoaProvider} no provider: {_financeiroProviderService.ProviderName}");


                var cardUtilizar = await GetCardUtilizar(cardExistente.Acquirer,cardExistente.Brand,cardExistente.CardNumber,cardExistente.ClienteId,cardExistente.Pessoa,contas.First().EmpresaId);

                var customerUtilizar = new CustomerModel()
                {
                    rid = pessoa.Id,
                    type = pessoa.TipoPessoa == 0 ? "F" : "J",
                    name = pessoa.Nome,
                    document = pessoa.TipoPessoa == 0 ? dadosPessoa?.Cpf?.PadLeft(11, '0') : dadosPessoa?.Cnpj?.PadLeft(14, '0'),
                    document_type = pessoa.TipoPessoa == 0 ? "CPF" : "CNPJ",
                    email = !string.IsNullOrEmpty(dadosPessoa.Email) && dadosPessoa.Email.Contains(";") ? dadosPessoa.Email.Split(";")[0] : dadosPessoa?.Email,
                    phones = new List<PhoneModel>()
                    {
                        new PhoneModel()
                        {
                            type = dadosPessoa != null && !string.IsNullOrEmpty(dadosPessoa.TipoTelefone) && dadosPessoa.TipoTelefone.StartsWith("CELU", StringComparison.InvariantCultureIgnoreCase) ? "cellphone" : "home",
                            number = Helper.ApenasNumeros(dadosPessoa?.NumeroTelefone)
                        }
                    },
                    address =
                            new AddressModel()
                            {
                                street = dadosPessoa?.Logradouro,
                                number = dadosPessoa?.Numero,
                                neighborhood = dadosPessoa?.Bairro,
                                zip_code = dadosPessoa?.Cep,
                                city = dadosPessoa?.CidadeNome,
                                state = !string.IsNullOrEmpty(dadosPessoa?.EstadoSigla) && dadosPessoa?.EstadoSigla.Length > 2 ? dadosPessoa?.EstadoSigla.Substring(0, 2) : "",
                                country = dadosPessoa?.SiglaPais
                            },
                    foreigner = dadosPessoa?.SiglaPais != "BR",
                    gender = dadosPessoa?.Sexo ?? "M",
                    birth = $"{dadosPessoa?.DataNascimento.GetValueOrDefault().Date:yyyy-MM-dd} 00:00:00",
                    registered = true,
                    created = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                };


                //Monto o objeto para a transação no cartão
                var transactionModel = new TransactionCardModel()
                {
                    merchant_id = $"{_financeiroProviderService.PrefixoTransacaoFinanceira}PessoaId_{doTransactionModel.PessoaId}_{DateTime.Now:ddMMyyyyHHmmss}",
                    channel = "SWPortalProprietario",
                    customer = customerUtilizar,
                    description = "PAGTOCONTAS",
                    payment = new PaymentModel()
                    {
                        value = doTransactionModel.ValorTotal,
                        installments = 1,
                        capture = true,
                        card = new TransactionTokenizedCardModel()
                        {
                            token = cardUtilizar.Token
                        },
                        items = new List<TransactionItemInputModel>()
                    {
                        new TransactionItemInputModel()
                        {
                            item_id = $"{contas.First().Id}",
                            value = doTransactionModel.ValorTotal,
                            name = "PGTOCONTAS",
                            amount = doTransactionModel.ValorTotal,
                            date = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                            end_date = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                            travelers_amount = 1,
                            category = "PGTOCONTAS",
                            travelers = new List<CustomerModel>()
                            {
                                customerUtilizar
                            }
                        }
                    }
                    },
                    ip = null,
                    vip = false,
                    fligth = false,
                    finger_print = null

                };


                transactionCardResultModel = await _broker.DoCardTransaction(transactionModel, contas.First().EmpresaId.GetValueOrDefault());

                await GravarPaymentCardHistory(transactionModel, transactionCardResultModel, doTransactionModel, contas);

                jsonBodyRequest = System.Text.Json.JsonSerializer.Serialize(transactionModel);
                jsonBodyResponse = System.Text.Json.JsonSerializer.Serialize(transactionCardResultModel);
                //Efetuar alteração nas contas relacionadas ao pagamento efetuado


                var result = await _repository.CommitAsync();
                if (!result.executed)
                    throw result.exception ?? new Exception("Erro na operação");

                return transactionCardResultModel;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                try
                {
                    _repository.BeginTransaction();

                    transactionCardResultModel = transactionCardResultModel ?? new TransactionCardResultModel() { status = "undefinid", errors = new List<string>() { err.Message, err.InnerException?.Message } };
                    jsonBodyResponse = System.Text.Json.JsonSerializer.Serialize(transactionCardResultModel);
                    if (string.IsNullOrEmpty(jsonBodyRequest))
                        jsonBodyRequest = System.Text.Json.JsonSerializer.Serialize(doTransactionModel);

                    await GravarTentativa(transactionCardResultModel ?? new TransactionCardResultModel() { status = "undefinid", errors = new List<string>() { err.Message, err.InnerException?.Message } }, doTransactionModel, jsonBodyRequest, jsonBodyResponse);
                    var result = await _repository.CommitAsync();
                }
                catch
                {
                    _repository.Rollback();
                }
                throw;
            }
        }

        private async Task<CardTokenized> GetCardUtilizar(string? acquirer, 
            string? brand, 
            string? cardNumber, 
            string? clienteId, 
            Domain.Entities.Core.DadosPessoa.Pessoa? pessoa, 
            int? empresaId)
        {
            if (string.IsNullOrEmpty(acquirer) || 
                string.IsNullOrEmpty(brand) || 
                string.IsNullOrEmpty(cardNumber) || 
                string.IsNullOrEmpty(clienteId) || 
                pessoa == null || pessoa.Id == 0 ||
                empresaId.GetValueOrDefault(0) == 0)
                throw new ArgumentNullException($"Não foi localizado o CardTokenized com os dados informados, para transacionar na empresa: {empresaId.GetValueOrDefault()}");

            var cardExistente = (await _repository.FindByHql<CardTokenized>(@$"From 
                        CardTokenized ct 
                        Inner Join Fetch ct.Pessoa p 
                    Where 
                        p.Id = {pessoa.Id} and 
                        ct.Acquirer = '{acquirer}' and 
                        ct.CardNumber = '{cardNumber}' and
                        ct.Brand = '{brand}' and
                        ct.ClienteId = '{clienteId}' and
                        ct.EmpresaLegadoId = {empresaId} and
                        p.Id = {pessoa.Id} and
                        ct.Visivel = 1 ")).FirstOrDefault();

            if (cardExistente == null)
                throw new ArgumentException($"Não foi localizado o CardTokenized com os dados informados, para transacionar na empresa: {empresaId.GetValueOrDefault()}");


            var cardsMesmaCompanyId = (await _repository.FindByHql<CardTokenized>(@$"From 
                        CardTokenized ct 
                        Inner Join Fetch ct.Pessoa p 
                    Where 
                        p.Id = {pessoa.Id} and 
                        ct.Acquirer = '{acquirer}' and 
                        ct.CardNumber = '{cardNumber}' and
                        ct.Brand = '{brand}' and
                        ct.ClienteId = '{clienteId}' and
                        p.Id = {pessoa.Id} and
                        ct.Visivel = 1 and 
                        ct.CompanyId = '{cardExistente.CompanyId}' and 
                        ct.CompanyToken = '{cardExistente.CompanyToken}' 
                        Order by ct.Id desc")).FirstOrDefault();

            return cardExistente;

        }

        private async Task ValidarPagamentoEmDuplicidade(List<ContaPendenteModel> contas, Domain.Entities.Core.DadosPessoa.Pessoa pessoa)
        {
            var itensJaPagosEmCartao = (await _repository.FindByHql<PaymentCardTokenizedItem>(@$"From 
                                PaymentCardTokenizedItem pcti 
                                Inner Join Fetch pcti.PaymentCardTokenized pct
                                Inner Join Fetch pct.CardTokenized cc
                                Inner Join Fetch cc.Pessoa p
                                Where 
                                p.Id = {pessoa.Id} and
                                upper(pct.Status) = 'CAPTURED' and                                
                                pcti.ItemId in ({string.Join(",", contas.Select(b => b.Id).AsList())})")).AsList();

            if (itensJaPagosEmCartao.Any())
            {
                var itemFirst = itensJaPagosEmCartao.FirstOrDefault(a => !string.IsNullOrEmpty(a.PaymentCardTokenized?.Nsu));
                if (itemFirst != null)
                {
                    throw new ArgumentException($"As contas de ids: {string.Join(",", itensJaPagosEmCartao.Select(b => b.ItemId))} já foram pagas em cartão crédito anteriormente, transação em cartão de Id: {itemFirst?.PaymentCardTokenized?.Id}, nsu: {itemFirst?.PaymentCardTokenized?.Nsu}, autorizaão: {itemFirst?.PaymentCardTokenized?.Nsu}");
                }
                else throw new ArgumentException($"As contas de ids: {string.Join(",", itensJaPagosEmCartao.Select(b => b.ItemId))} já foram pagas em cartão crédito anteriormente");
            }

            var itensJaPagosEmPix = (await _repository.FindByHql<PaymentPixItem>(@$"From 
                                PaymentPixItem pcti 
                                Inner Join Fetch pcti.PaymentPix pp
                                Inner Join Fetch pp.Pessoa p
                                Where 
                                p.Id = {pessoa.Id} and
                                upper(pp.Status) = 'CAPTURED' and                                
                                pcti.ItemId in ({string.Join(",", contas.Select(b => b.Id).AsList())})")).AsList();

            if (itensJaPagosEmPix.Any())
            {
                var itemFirst = itensJaPagosEmPix.FirstOrDefault(a => !string.IsNullOrEmpty(a.PaymentPix?.TransactionId));
                if (itemFirst != null)
                {
                    throw new ArgumentException($"As contas de ids: {string.Join(",", itensJaPagosEmCartao.Select(b => b.ItemId))} já foram pagas em PIX anteriormente, transação Id: {itemFirst?.PaymentPix?.Id}, transação pix id: {itemFirst?.PaymentPix?.TransactionId}");
                }
                else throw new ArgumentException($"As contas de ids: {string.Join(",", itensJaPagosEmCartao.Select(b => b.ItemId))} já foram pagas em PIX anteriormente");
            }

        }

        private async Task GravarPaymentCardHistory(TransactionCardModel transactionModel, TransactionCardResultModel transactionCardResultModel, DoTransactionCardInputModel doTransactionModel, List<ContaPendenteModel> contas)
        {
            if (transactionCardResultModel != null && !string.IsNullOrEmpty(transactionCardResultModel?.last_acquirer_response?.response?.authorization) &&
             !string.IsNullOrEmpty(transactionCardResultModel?.last_acquirer_response?.response?.nsu) &&
             !string.IsNullOrEmpty(transactionCardResultModel?.last_acquirer_response?.status) &&
             transactionCardResultModel.last_acquirer_response.status.Contains("captured", StringComparison.CurrentCultureIgnoreCase))
            {
                var paymentCardTokenized = new PaymentCardTokenized()
                {
                    CardTokenized = new CardTokenized() { Id = doTransactionModel.CardTokenizedId.GetValueOrDefault() },
                    Valor = doTransactionModel.ValorTotal,
                    PaymentId = transactionCardResultModel?.payment_id,
                    Status = transactionCardResultModel?.last_acquirer_response?.status,
                    Nsu = transactionCardResultModel?.last_acquirer_response?.response?.nsu,
                    CodigoAutorizacao = transactionCardResultModel?.last_acquirer_response?.response?.authorization,
                    Adquirente = transactionCardResultModel?.company?.acquirer,
                    AdquirentePaymentId = transactionCardResultModel?.last_acquirer_response?.response?.payment_id,
                    TransactionId = transactionCardResultModel?.last_acquirer_response?.response?.transaction,
                    Url = transactionCardResultModel?.last_acquirer_response?.response?.url,
                    CompanyId = transactionCardResultModel?.company?.id,
                    EmpresaLegadoId = contas.First().EmpresaId
                };

                await _repository.Save(paymentCardTokenized);

                //Gravo os ítens pagos na operação
                foreach (var item in contas)
                {
                    var itemPaid = new PaymentCardTokenizedItem()
                    {
                        PaymentCardTokenized = paymentCardTokenized,
                        Valor = item.Valor,
                        ItemId = $"{item.Id}",
                        ValorNaTransacao = item.ValorAtualizado,
                        Vencimento = item.Vencimento,
                        DescricaoDoItem = $"PessoaId:{item.PessoaId}|PessoaProviderId:{item.PessoaProviderId}|CódigoTipoConta:{item.CodigoTipoConta}|NomeTipoConta:{item.NomeTipoConta}|Vencimento:{item.Vencimento:dd/MM/yyyy}"
                    };
                    await _repository.Save(itemPaid);
                }
            }
            else
            {
                var jsonBodyRequest = System.Text.Json.JsonSerializer.Serialize(transactionModel);
                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(transactionCardResultModel);

                await GravarTentativa(transactionCardResultModel, doTransactionModel, jsonBodyRequest, jsonResponse);
            }
        }

        private async Task GravarTentativa(TransactionCardResultModel? transactionCardResultModel, DoTransactionCardInputModel doTransactionModel, string jsonBodyRequest, string jsonResponse)
        {
            var tentativa = new PaymentCardTokenizedAttempt()
            {
                CardTokenized = new CardTokenized()
                {
                    Id = doTransactionModel.CardTokenizedId.GetValueOrDefault(),
                },
                Valor = doTransactionModel.ValorTotal,
                DadosEnviados = jsonBodyRequest,
                Retorno = jsonResponse,
                RetornoAmigavel = transactionCardResultModel != null && transactionCardResultModel.last_acquirer_response != null ? $"{transactionCardResultModel?.last_acquirer_response?.response?.code}-{transactionCardResultModel?.last_acquirer_response?.response?.message}" : "Não foi possível processar o pagamento"
            };
            await _repository.Save(tentativa);
        }

        public async Task<TransactionPixResultModel?> GeneratePixTransaction(DoTransactionPixInputModel doTransactionModel)
        {
            if (_repository.IsAdm)
            {
                var systemConfiguration = await _repository.GetParametroSistemaViewModel();
                if (systemConfiguration != null && systemConfiguration.HabilitarPagamentoEmPix.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)
                    throw new ArgumentException("O sistema está configurado para não permitir pagamento em PIX.");
            }

            try
            {
                _repository.BeginTransaction();

                if (doTransactionModel.PessoaId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o parâmetro PessoaId");

                var vinculoPessoaProvider = (await _serviceBase.GetPessoaProviderVinculadaPessoaSistema($"{doTransactionModel.PessoaId.GetValueOrDefault()}", _financeiroProviderService.ProviderName));
                if (vinculoPessoaProvider == null || string.IsNullOrEmpty(vinculoPessoaProvider.PessoaProvider))
                    throw new ArgumentException($"Não foi encontrado vínculos da PessoaId informada: {doTransactionModel.PessoaId}");

                string idPessoaUtilizar = vinculoPessoaProvider != null && !string.IsNullOrEmpty(vinculoPessoaProvider.PessoaSistema) ? vinculoPessoaProvider.PessoaSistema : $"{doTransactionModel.PessoaId.GetValueOrDefault()}";
                if (vinculoPessoaProvider == null)
                    throw new ArgumentException("Deve ser informado o parâmetro PessoaId");

                if (string.IsNullOrEmpty(idPessoaUtilizar))
                    throw new ArgumentException($"Não foi encontrada a pessoa do sistema com os dados informados Pessoaid: {doTransactionModel.PessoaId}");

                var pessoa = await _repository.FindById<Domain.Entities.Core.DadosPessoa.Pessoa>(Convert.ToInt32(idPessoaUtilizar));
                if (pessoa == null)
                    throw new ArgumentException($"Não foi encontrada pessoa com o Id: {doTransactionModel.PessoaId.GetValueOrDefault()}");

                var usuarioVinculadoPessoa = (await _repository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where p.Id = {pessoa.Id} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ")).FirstOrDefault();

                if (usuarioVinculadoPessoa == null)
                    throw new ArgumentException($"Não foi encontrado usuário no sistema com a pessoa id: {doTransactionModel.PessoaId.GetValueOrDefault()}, primeiro é necessário cadastrar a pessoa como usuário do sistema");


                var contas = await _financeiroProviderService.GetContasParaPagamentoEmPixGeral(doTransactionModel);
                await ValidarPagamentoEmDuplicidade(contas, pessoa);

                var dadosPessoa = await _financeiroProviderService.GetDadosPessoa(Convert.ToInt32(vinculoPessoaProvider.PessoaProvider));
                if (dadosPessoa == null)
                    throw new ArgumentException($"Não foi possível encontrar a pessoa com Id: {vinculoPessoaProvider.PessoaProvider} no provider: {_financeiroProviderService.ProviderName}");

                var customerUtilizar = new CustomerModel()
                {
                    rid = pessoa.Id,
                    type = pessoa.TipoPessoa == 0 ? "F" : "J",
                    name = pessoa.Nome,
                    document = pessoa.TipoPessoa == 0 ? dadosPessoa?.Cpf?.PadLeft(11, '0') : dadosPessoa?.Cnpj?.PadLeft(14, '0'),
                    document_type = pessoa.TipoPessoa == 0 ? "CPF" : "CNPJ",
                    email = !string.IsNullOrEmpty(dadosPessoa.Email) && dadosPessoa.Email.Contains(";") ? dadosPessoa.Email.Split(";")[0] : dadosPessoa?.Email,
                    phones = new List<PhoneModel>()
                    {
                        new PhoneModel()
                        {
                            type = dadosPessoa != null && !string.IsNullOrEmpty(dadosPessoa.TipoTelefone) && dadosPessoa.TipoTelefone.StartsWith("CELU", StringComparison.InvariantCultureIgnoreCase) ? "cellphone" : "home",
                            number = Helper.ApenasNumeros(dadosPessoa?.NumeroTelefone)
                        }
                    },
                    address =
                            new AddressModel()
                            {
                                street = dadosPessoa.Logradouro,
                                number = dadosPessoa.Numero,
                                neighborhood = dadosPessoa.Bairro,
                                zip_code = dadosPessoa.Cep,
                                city = dadosPessoa.CidadeNome,
                                state = !string.IsNullOrEmpty(dadosPessoa?.EstadoSigla) && dadosPessoa?.EstadoSigla.Length > 2 ? dadosPessoa?.EstadoSigla.Substring(0, 2) : "",
                                country = dadosPessoa.SiglaPais
                            },
                    foreigner = dadosPessoa.SiglaPais != "BR",
                    gender = dadosPessoa.Sexo ?? "M",
                    birth = $"{dadosPessoa.DataNascimento.GetValueOrDefault().Date:yyyy-MM-dd} 00:00:00",
                    registered = true,
                    created = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                };


                //Monto o objeto para a transação no PIX
                var transactionModel = new TransactionPixModel()
                {
                    merchant_id = $"{_financeiroProviderService.PrefixoTransacaoFinanceira}PessoaId_{doTransactionModel.PessoaId}_{DateTime.Now:ddMMyyyyHHmmss}",
                    channel = "SWPortalProprietario",
                    customer = customerUtilizar,
                    description = "PAGTOCONTAS",
                    web_payment = new WebPaymentModel()
                    {
                        value = doTransactionModel.ValorTotal,
                        items = new List<TransactionItemInputModel>()
                    {
                        new TransactionItemInputModel()
                        {
                            item_id = $"{contas.First().Id}",
                            value = doTransactionModel.ValorTotal,
                            name = "PGTOCONTAS",
                            amount = doTransactionModel.ValorTotal,
                            date = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                            end_date = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                            travelers_amount = 1,
                            category = "PGTOCONTAS",
                            travelers = new List<CustomerModel>()
                            {
                                customerUtilizar
                            }
                        }
                    }
                    },
                    ip = null,
                    vip = false,
                    fligth = false,
                    finger_print = null

                };


                var transactionCardResultModel = await _broker.GeneratePixTransaction(transactionModel, contas.First().EmpresaId.GetValueOrDefault());

                await GravarGeracaoPixHistory(transactionModel, transactionCardResultModel, doTransactionModel, contas);


                //Efetuar alteração nas contas relacionadas ao pagamento efetuado


                var result = await _repository.CommitAsync();
                if (!result.executed)
                    throw result.exception ?? new Exception("Erro na operação");

                return transactionCardResultModel;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                throw;
            }
        }

        private async Task GravarGeracaoPixHistory(TransactionPixModel transactionModel, TransactionPixResultModel transactionPixResultModel, DoTransactionPixInputModel doTransactionModel, List<ContaPendenteModel> contas)
        {

            var json = System.Text.Json.JsonSerializer.Serialize(transactionPixResultModel);

            if (transactionPixResultModel != null && !string.IsNullOrEmpty(transactionPixResultModel?.last_acquirer_response?.status) &&
             !string.IsNullOrEmpty(transactionPixResultModel?.last_acquirer_response?.response?.transaction) &&
             (transactionPixResultModel.last_acquirer_response.status.Contains("pending", StringComparison.CurrentCultureIgnoreCase) ||
             transactionPixResultModel.last_acquirer_response.status.Contains("captured", StringComparison.CurrentCultureIgnoreCase) ||
             transactionPixResultModel.last_acquirer_response.status.Contains("cancelled", StringComparison.CurrentCultureIgnoreCase)))
            {
                var paymentPix = new PaymentPix()
                {
                    CompanyId = transactionPixResultModel?.company?.id,
                    Valor = transactionPixResultModel?.web_payment?.value,
                    PaymentId = transactionPixResultModel?.payment_id,
                    Status = transactionPixResultModel?.last_acquirer_response.status,
                    Acquirer = transactionPixResultModel?.company?.acquirer,
                    Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = doTransactionModel.PessoaId.GetValueOrDefault(0) },
                    Pdf = transactionPixResultModel?.last_acquirer_response?.response.pdf,
                    Payment_Id = transactionPixResultModel?.last_acquirer_response?.response?.payment_id,
                    QrCode = transactionPixResultModel?.last_acquirer_response?.response?.qrcode,
                    TransactionId = transactionPixResultModel?.last_acquirer_response?.response?.transaction,
                    Url = transactionPixResultModel?.last_acquirer_response?.response?.url,
                    Retorno = json,
                    ValidoAte = transactionPixResultModel?.expiration_date.GetValueOrDefault()
                };

                await _repository.Save(paymentPix);

                //Gravo os vinculados na operação
                foreach (var item in contas)
                {
                    var itemPaid = new PaymentPixItem()
                    {
                        PaymentPix = paymentPix,
                        Valor = item.Valor,
                        ItemId = $"{item.Id}",
                        DescricaoDoItem = $"PessoaId:{item.PessoaId}|PessoaProviderId:{item.PessoaProviderId}|CódigoTipoConta:{item.CodigoTipoConta}|NomeTipoConta:{item.NomeTipoConta}|Vencimento:{item.Vencimento:dd/MM/yyyy}"
                    };
                    await _repository.Save(itemPaid);
                }
            }
            else
            {

                var paymentPix = new PaymentPix()
                {
                    Valor = doTransactionModel.ValorTotal,
                    Status = "Fail",
                    Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = doTransactionModel.PessoaId.GetValueOrDefault(0) },
                    Retorno = json
                };

                await _repository.Save(paymentPix);

                //Gravo os vinculados na operação
                foreach (var item in contas)
                {
                    var itemPaid = new PaymentPixItem()
                    {
                        PaymentPix = paymentPix,
                        Valor = item.Valor,
                        ItemId = $"{item.Id}",
                        DescricaoDoItem = $"PessoaId:{item.PessoaId}|PessoaProviderId:{item.PessoaProviderId}|CódigoTipoConta:{item.CodigoTipoConta}|NomeTipoConta:{item.NomeTipoConta}|Vencimento:{item.Vencimento:dd/MM/yyyy}"
                    };
                    await _repository.Save(itemPaid);
                }
            }
        }

        public async Task<List<CardTokenizedModel>> GetAllTokenizedCardFromUser(SearchTokenizedCardFromUserModel searchModel)
        {

            var sb = new StringBuilder(@"From 
                            CardTokenized ct 
                            Inner Join Fetch ct.Pessoa p 
                            Where 1 = 1 ");



            if (searchModel.PessoaId.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and p.Id = {searchModel.PessoaId} ");
            }

            if (searchModel.PessoaProviderId.GetValueOrDefault(0) > 0)
            {
                var vinculoPessoaProvider = searchModel.PessoaProviderId.GetValueOrDefault(0) > 0 ?
                    (await _serviceBase.GetPessoaSistemaVinculadaPessoaProvider($"{searchModel.PessoaProviderId.GetValueOrDefault()}", _financeiroProviderService.ProviderName)) :
                    null;

                if (vinculoPessoaProvider != null && !string.IsNullOrEmpty(vinculoPessoaProvider.PessoaSistema))
                    sb.AppendLine($" and p.Id = {vinculoPessoaProvider.PessoaSistema} ");
                else throw new ArgumentException($"Não foi encontrada pessoa do sistema vinculada ao PessoaProviderId informado: {searchModel.PessoaProviderId.GetValueOrDefault()}");
            }

            if (!string.IsNullOrEmpty(searchModel.PessoaNome))
            {
                sb.AppendLine($" and Lower(p.Nome) like '{searchModel.PessoaNome.ToLower()}%' ");
            }

            var cardsExistentes = (await _repository.FindByHql<CardTokenized>(sb.ToString())).AsList();


            List<CardTokenizedModel> cardsRetornar = new List<CardTokenizedModel>();

            if (cardsExistentes != null && cardsExistentes.Any())
            {
                cardsRetornar = cardsExistentes.Select(b =>
                    new CardTokenizedModel()
                    {
                        Id = b.Id,
                        UsuarioCriacao = b.UsuarioCriacao,
                        UsuarioAlteracao = b.UsuarioAlteracao,
                        DataHoraCriacao = b.DataHoraCriacao,
                        DataHoraAlteracao = b.DataHoraAlteracao,
                        PessoaId = b.Pessoa?.Id,
                        PessoaNome = b.Pessoa?.Nome,
                        CardHolder = b.CardHolder,
                        card = new CardInputModel()
                        {
                            brand = b.Brand,
                            card_number = b.CardNumber,
                            cvv = b.Cvv,
                            due_date = b.DueDate,
                            card_holder = b.CardHolder
                        },
                        token = b.Token,
                        token2 = b.Token2,
                        company = new CompanyModel()
                        {
                            id = b.CompanyId,
                            acquirer = b.Acquirer
                        },
                        status = b.Status
                    }).AsList();
            };


            return cardsRetornar;
        }

        public async Task<TransactionCardResultModel?> GetTransactionResult(string paymentId)
        {
            TransactionCardResultModel? searchResult = null;
            throw new NotImplementedException();

            try
            {
                _repository.BeginTransaction();
                var resultConsulta = await _broker.GetTransactionResult(paymentId,-1);


                //await GravarGeracaoPixHistory(transactionModel, transactionCardResultModel, doTransactionModel, contas);


                //Efetuar alteração nas contas relacionadas ao pagamento efetuado


                var result = await _repository.CommitAsync();
                if (!result.executed)
                    throw result.exception ?? new Exception("Erro na operação");

                return searchResult;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                throw;
            }
        }

        public async Task<CardTokenizedModel?> Tokenize(CardTokenizeRequestModel cardModel)
        {
            if (cardModel.pessoaid.GetValueOrDefault(0) == 0)
            {
                throw new ArgumentException("Deve ser informado o campo PessoaId");
            }

            var pessoa = await _repository.FindById<Domain.Entities.Core.DadosPessoa.Pessoa>(cardModel.pessoaid.GetValueOrDefault(0));
            if (pessoa == null)
                throw new ArgumentException($"Não foi encontrada pessoa com o Id: {cardModel.pessoaid.GetValueOrDefault()}");

            var parametrosSistema = await _repository.GetParametroSistemaViewModel();
            if (parametrosSistema == null || string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds))
                throw new ArgumentException("Deve ser configurado o valor para 'ExibirFinanceirosDasEmpresaIds' nos parâmetros do sistema.");

            var usuarioVinculadoPessoa = (await _repository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where p.Id = {pessoa.Id} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();

            if (usuarioVinculadoPessoa == null)
                throw new ArgumentException($"Não foi encontrada usuário no sistema com a pessoa id: {cardModel.pessoaid.GetValueOrDefault()}, primeiro é necessário cadastrar a pessoa como usuário do sistema");

            if (cardModel.card == null)
                throw new ArgumentException("Deve ser informado os dados do cartão");

            if (string.IsNullOrEmpty(cardModel.card?.due_date))
                throw new ArgumentException("Deve ser informada a data de validade do cartão");

            if (string.IsNullOrEmpty(cardModel.card?.brand) && !string.IsNullOrEmpty(cardModel.card?.card_number))
            {
                if (cardModel.card.card_number.StartsWith("37") || cardModel.card.card_number.StartsWith("34"))
                    cardModel.card.brand = "Amex";
                else if (cardModel.card.card_number.StartsWith("3") || cardModel.card.card_number.StartsWith("3"))
                    cardModel.card.brand = "diners club";
                else if (cardModel.card.card_number.StartsWith("4"))
                    cardModel.card.brand = "visa";
                else if (cardModel.card.card_number.StartsWith("50"))
                    cardModel.card.brand = "aura";
                else if (cardModel.card.card_number.StartsWith("60"))
                    cardModel.card.brand = "discover";
                else if (cardModel.card.card_number.StartsWith("5"))
                    cardModel.card.brand = "mastercard";
                else if (cardModel.card.card_number.StartsWith("6"))
                    cardModel.card.brand = "discover";
                else if (cardModel.card.card_number.StartsWith("8"))
                    cardModel.card.brand = "discover";


                if (string.IsNullOrEmpty(cardModel.card?.brand))
                    throw new ArgumentException("Deve ser informada a bandeira do cartão");
            }

            var numeroCartaoNormalizado = Helper.ApenasNumeros(cardModel.card.card_number);

            if (cardModel.card.due_date.Length == 5 && cardModel.card.due_date.Contains("/"))
            {
                var dueDateToChange = cardModel.card.due_date;
                cardModel.card.due_date = $"{dueDateToChange.Split("/")[0]}/20{dueDateToChange.Split("/")[1]}";
            }


            if (string.IsNullOrEmpty(numeroCartaoNormalizado) || (numeroCartaoNormalizado.Length != 16 && numeroCartaoNormalizado.Length != 15))
                throw new ArgumentException("O número do cartão deve ser informado com 15 ou 16 caracteres");

            var hash = Helper.ApenasNumeros(cardModel.card.card_number).GetHashCode();

            CardTokenizedModel? tokenizedReturn = null; 
            var cardExistente = (await _repository.FindByHql<CardTokenized>(@$"From 
                        CardTokenized ct 
                        Inner Join Fetch ct.Pessoa p 
                    Where 
                        ct.Hash = '{hash}' and p.Id = {pessoa.Id}")).AsList();

            if (cardExistente != null && cardExistente.Any())
            {
                foreach (var item in cardExistente)
                {
                    item.Visivel = Domain.Enumns.EnumSimNao.Não;
                    await _repository.Save(item);
                }
            }

            var cardTokenizedToSend = new CardTokenizeInputModel()
            {
                card = new CardInputModel()
                {
                    brand = cardModel.card?.brand,
                    card_holder = cardModel.card?.card_holder,
                    cvv = cardModel.card?.cvv,
                    due_date = cardModel.card?.due_date,
                    card_number = cardModel.card?.card_number
                }
            };


            foreach (var item in parametrosSistema!.ExibirFinanceirosDasEmpresaIds!.Split(",")
                .Where(c=> int.TryParse(c,out _)))
            {
                tokenizedReturn = await _broker.Tokenize(cardTokenizedToSend,int.Parse(item));
                if (tokenizedReturn != null && !tokenizedReturn.errors.Any())
                {
                    var cardTokenized = new CardTokenized()
                    {
                        CardHolder = tokenizedReturn.card?.card_holder ?? cardModel?.card?.card_holder,
                        Brand = tokenizedReturn.card?.brand ?? cardModel?.card?.brand,
                        CardNumber = tokenizedReturn.card?.card_number ?? cardModel?.card?.card_number,
                        Cvv = tokenizedReturn.card?.cvv ?? cardModel?.card?.cvv,
                        DueDate = tokenizedReturn.card?.due_date ?? cardModel?.card?.due_date,
                        Visivel = Domain.Enumns.EnumSimNao.Sim,
                        Token = tokenizedReturn.token,
                        Token2 = tokenizedReturn.token2,
                        Status = tokenizedReturn.status,
                        CompanyId = tokenizedReturn.company?.id,
                        Acquirer = tokenizedReturn.company?.acquirer,
                        CompanyToken = _brokerModel.GetCardCardCompanyToken(int.Parse(item)),
                        ClienteId = usuarioVinculadoPessoa.ProviderChaveUsuario,
                        Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = pessoa.Id },
                        EmpresaLegadoId = int.Parse(item)
                    };

                    await _repository.Save(cardTokenized);
                }
            }

            return tokenizedReturn;
        }

        public async Task<(int pageNumber, int lastPageNumber, List<TransactionSimplifiedResultModel> transactionResult)?> SearchTransacoes(SearchTransacoesModel searchTransacoesModel)
        {
            var listItensRetornar = new List<TransactionSimplifiedResultModel>();

            if (searchTransacoesModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchTransacoesModel.QuantidadeRegistrosRetornar = 20;

            if (searchTransacoesModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchTransacoesModel.NumeroDaPagina = 1;

            var sqlTransactionsCartao = new StringBuilder();
            List<Parameter> parameters = new List<Parameter>();

            if (searchTransacoesModel.DataInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataInicial", searchTransacoesModel.DataInicial.GetValueOrDefault().Date));
            }

            if (searchTransacoesModel.DataFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataFinal", searchTransacoesModel.DataFinal.GetValueOrDefault().Date.AddDays(1).AddMilliseconds(-1)));
            }

            if (!searchTransacoesModel.Cartao.HasValue || searchTransacoesModel.Cartao.GetValueOrDefault(false))
            {

                sqlTransactionsCartao.AppendLine($@"Select
                        pct.Id as InternalId,
                        'DoPayment' as TipoOperacao,
                        pct.PaymentId,
                        p.Id as PessoaId,
                        p.Nome as PessoaNome,
                        0 as Pix,
                        1 as Cartao,
                        pct.Valor as ValorTransacao,
                        Coalesce(Case 
                        when Lower(pct.Status) like '%captured%' then 1
                        when Lower(pct.Status) like '%cancelled%' then 0 end, 0) as Efetivada,
                        Coalesce(Case 
                        when Lower(pct.Status) like '%captured%' then 'Autorizada'
                        when Lower(pct.Status) like '%cancelled%' then 'Cancelada' end, 'Negada') as Status,                        
                        pct.DataHoraCriacao as DataTransacao,
                        '' as QrCode,
                        '' as Url,
                        pct.Nsu,
                        pct.CodigoAutorizacao as Autorizacao,
                        pct.Adquirente,
                        pct.TransactionId,
                        pct.DadosEnviados,
                        pct.Retorno
                        From
                        PaymentCardTokenized pct 
                        Inner Join CardTokenized cc on pct.CardTokenized = cc.Id
                        Inner Join Pessoa p on cc.Pessoa = p.Id 
                        Where 1 = 1 ");


                if (searchTransacoesModel.PessoaId.GetValueOrDefault(0) > 0)
                {
                    sqlTransactionsCartao.AppendLine($" and p.Id = {searchTransacoesModel.PessoaId.GetValueOrDefault()} ");
                }

                if (!string.IsNullOrEmpty(searchTransacoesModel.PessoaNome))
                {
                    sqlTransactionsCartao.AppendLine($" and Lower(p.Nome) like '{searchTransacoesModel.PessoaNome.ToLower()}%'");
                }

                if (searchTransacoesModel.DataInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
                {
                    sqlTransactionsCartao.AppendLine(" and pct.DataHoraCriacao >= :dataInicial ");
                }

                if (searchTransacoesModel.DataFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
                {
                    sqlTransactionsCartao.AppendLine(" and pct.DataHoraCriacao <= :dataFinal ");
                }

                if (!string.IsNullOrEmpty(searchTransacoesModel.StatusTransacao) && !searchTransacoesModel.StatusTransacao.Contains("default", StringComparison.CurrentCultureIgnoreCase))
                {
                    sqlTransactionsCartao.AppendLine($" and Lower(pct.Status) like '%{searchTransacoesModel.StatusTransacao.ToLower().TrimEnd()}%' ");
                }

                if (searchTransacoesModel.EmpresaId.GetValueOrDefault(0) > 0)
                {
                    sqlTransactionsCartao.AppendLine($" and pct.EmpresaLegadoId = {searchTransacoesModel.EmpresaId.GetValueOrDefault(0)} ");
                }

                if (!string.IsNullOrEmpty(searchTransacoesModel.StatusTransacao) &&
                    (searchTransacoesModel.StatusTransacao.Contains("default", StringComparison.InvariantCultureIgnoreCase) ||
                    searchTransacoesModel.StatusTransacao.Contains("notpaid", StringComparison.InvariantCultureIgnoreCase)))
                {

                    sqlTransactionsCartao.AppendLine($@"
                        Union all

                        Select
                        pct.Id as InternalId,
                        'AttemptDoPayment' as TipoOperacao,
                        '' as PaymentId,
                        p.Id as PessoaId,
                        p.Nome as PessoaNome,
                        0 as Pix,
                        1 as Cartao,
                        pct.Valor as ValorTransacao,
                        0 as Efetivada,
                        Coalesce(pct.RetornoAmigavel,'Não processada') as Status,
                        pct.DataHoraCriacao as DataTransacao,
                        '' as QrCode,
                        '' as Url,
                        '' as Nsu,
                        '' as Autorizacao,
                        '' as Adquirente,
                        '' as TransactionId,
                        pct.DadosEnviados,
                        pct.Retorno
                        From
                        PaymentCardTokenizedAttempt pct 
                        Inner Join CardTokenized cc on pct.CardTokenized = cc.Id
                        Inner Join Pessoa p on cc.Pessoa = p.Id 
                        Where 1 = 1 ");

                    if (searchTransacoesModel.PessoaId.GetValueOrDefault(0) > 0)
                    {
                        sqlTransactionsCartao.AppendLine($" and p.Id = {searchTransacoesModel.PessoaId.GetValueOrDefault()} ");
                    }

                    if (!string.IsNullOrEmpty(searchTransacoesModel.PessoaNome))
                    {
                        sqlTransactionsCartao.AppendLine($" and Lower(p.Nome) like '{searchTransacoesModel.PessoaNome.ToLower()}%'");
                    }

                    if (searchTransacoesModel.DataInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
                    {
                        sqlTransactionsCartao.AppendLine(" and pct.DataHoraCriacao >= :dataInicial ");
                    }

                    if (searchTransacoesModel.DataFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
                    {
                        sqlTransactionsCartao.AppendLine(" and pct.DataHoraCriacao <= :dataFinal ");
                    }

                    if (searchTransacoesModel.EmpresaId.GetValueOrDefault(0) > 0)
                    {
                        sqlTransactionsCartao.AppendLine($" and pct.EmpresaLegadoId = {searchTransacoesModel.EmpresaId.GetValueOrDefault(0)} ");
                    }
                }

            }


            if (!searchTransacoesModel.Pix.HasValue || searchTransacoesModel.Pix.GetValueOrDefault(false))
            {

                if (!searchTransacoesModel.Cartao.HasValue || searchTransacoesModel.Cartao.GetValueOrDefault(false))
                    sqlTransactionsCartao.AppendLine(" Union all ");

                sqlTransactionsCartao.AppendLine($@"
                        Select 
                        pct.Id as InternalId,
                        'PixGenerate' as TipoOperacao,
                        pct.PaymentId,
                        p.Id as PessoaId,
                        p.Nome as PessoaNome,
                        1 as Pix,
                        0 as Cartao,
                        pct.Valor as ValorTransacao,
                        Case when Lower(pct.Status) like '%captured%' then 1 else 0 end as Efetivada,
                        Coalesce(Case 
                        when Lower(pct.Status) like '%captured%' then 'Autorizada'
                        when Lower(pct.Status) like '%cancelled%' then 'Cancelada' end, 'Pendente') as Status,    
                        pct.DataHoraCriacao as DataTransacao,
                        pct.QrCode,
                        pct.Url,
                        '' as Nsu,
                        '' as Autorizacao,
                        pct.Acquirer as Adquirente,
                        pct.TransactionId,
                        pct.DadosEnviados,
                        pct.Retorno
                        From
                        PaymentPix pct 
                        Inner Join Pessoa p on pct.Pessoa = p.Id 
                        Where 1 = 1 ");

                if (searchTransacoesModel.PessoaId.GetValueOrDefault(0) > 0)
                {
                    sqlTransactionsCartao.AppendLine($" and p.Id = {searchTransacoesModel.PessoaId.GetValueOrDefault()} ");
                }

                if (!string.IsNullOrEmpty(searchTransacoesModel.PessoaNome))
                {
                    sqlTransactionsCartao.AppendLine($" and Lower(p.Nome) like '{searchTransacoesModel.PessoaNome.ToLower()}%'");
                }

                if (searchTransacoesModel.DataInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
                {
                    sqlTransactionsCartao.AppendLine(" and pct.DataHoraCriacao >= :dataInicial ");
                }

                if (searchTransacoesModel.DataFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
                {
                    sqlTransactionsCartao.AppendLine(" and pct.DataHoraCriacao <= :dataFinal ");
                }

                if (!string.IsNullOrEmpty(searchTransacoesModel.StatusTransacao) && !searchTransacoesModel.StatusTransacao.Contains("default", StringComparison.CurrentCultureIgnoreCase))
                {
                    sqlTransactionsCartao.AppendLine($" and Lower(pct.Status) like '%{searchTransacoesModel.StatusTransacao.ToLower().TrimEnd()}%' ");
                }

                if (searchTransacoesModel.EmpresaId.GetValueOrDefault(0) > 0)
                {
                    sqlTransactionsCartao.AppendLine($" and pct.EmpresaLegadoId = {searchTransacoesModel.EmpresaId.GetValueOrDefault(0)} ");
                }

            }

            var strConsulta = sqlTransactionsCartao.ToString();

            var totalRegistros = await _repository.CountTotalEntry(strConsulta, parameters.ToArray());

            if (searchTransacoesModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                searchTransacoesModel.NumeroDaPagina.GetValueOrDefault(0) >
                searchTransacoesModel.QuantidadeRegistrosRetornar.GetValueOrDefault() ||
                totalRegistros < (searchTransacoesModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchTransacoesModel.NumeroDaPagina.GetValueOrDefault()))
                searchTransacoesModel.NumeroDaPagina = 1;

            var strConsultaPronta = $"Select b.* From ({strConsulta}) b ";

            strConsultaPronta += " Order by Cast(b.TipoOperacao as varchar), cast(b.InternalId as varchar)";

            if (totalRegistros > 0)
            {
                listItensRetornar = (await _repository.FindBySql<TransactionSimplifiedResultModel>(strConsultaPronta, searchTransacoesModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchTransacoesModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

                foreach (var item in listItensRetornar)
                {
                    item.Chave = $"{item.InternalId}-{item.TipoOperacao}";
                    item.HashCode = item.Chave.GetHashCode();
                    if (item.HashCode < 0)
                        item.HashCode = item.HashCode * (-1);


                    if (string.IsNullOrEmpty(item.TransactionId))
                        item.TransactionId = $"{item.HashCode * (-1)}";
                }

                if (searchTransacoesModel.RetornarContasVinculadas.GetValueOrDefault(false))
                {
                    foreach (var item in listItensRetornar)
                    {
                        item.ContasVinculadas = new List<PaymentItemModel>();
                    }

                    foreach (var itemRetornar in listItensRetornar.Where(c => c.Pix.GetValueOrDefault(false) == true))
                    {
                        var contasVinculadas = (await _repository.FindBySql<PaymentItemModel>($"Select * From PaymentPixItem Where PaymentPix = {itemRetornar.InternalId.GetValueOrDefault()}")).AsList();
                        if (contasVinculadas != null && contasVinculadas.Any())
                        {
                            itemRetornar.ContasVinculadas!.AddRange(contasVinculadas);
                        }
                    }

                    foreach (var itemRetornar in listItensRetornar)
                    {
                        if (itemRetornar.ContasVinculadas == null || itemRetornar.ContasVinculadas?.Count == 0)
                        {
                            var contasVinculadas = (await _repository.FindBySql<PaymentItemModel>($"Select * From PaymentCardTokenizedItem Where PaymentCardTokenized = {itemRetornar.InternalId.GetValueOrDefault()}")).AsList();
                            if (contasVinculadas != null && contasVinculadas.Any())
                            {
                                itemRetornar.ContasVinculadas!.AddRange(contasVinculadas);
                            }
                        }
                    }

                    if (searchTransacoesModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                    {
                        Int64 totalPage = Helper.TotalPaginas(searchTransacoesModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0), totalRegistros);
                        var retornoResult = (searchTransacoesModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), listItensRetornar);
                        return retornoResult;
                    }
                }
            }

            return (1, 1, listItensRetornar);
        }


    }
}
