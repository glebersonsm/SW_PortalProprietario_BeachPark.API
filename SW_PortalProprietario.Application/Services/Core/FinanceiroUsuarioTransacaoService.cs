using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Linq.Functions;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Domain.Functions;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class FinanceiroUsuarioTransacaoService : IFinanceiroTransacaoUsuarioService
    {
        private const string CARD_TRANSACTION_CACHE_KEY = "CardTransactionResultFinalizacaoPendente_";
        private const int CARD_CACHE_DB = 1;
        private const string PIX_TRANSACTION_CACHE_KEY = "PixTransactionResultFinalizacaoPendente_";
        private const int PIX_CACHE_DB = 1;
        private readonly IRepositoryNH _repository;
        private readonly ILogger<FinanceiroUsuarioTransacaoService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBroker _broker;
        private readonly IFinanceiroProviderService _financeiroProviderService;
        private readonly ICommunicationProvider _communicationProvider;
        private readonly IServiceBase _serviceBase;
        private readonly BrokerModel? _brokerModel;
        private readonly ICacheStore _cacheStore;

        public FinanceiroUsuarioTransacaoService(IRepositoryNH repository,
            ILogger<FinanceiroUsuarioTransacaoService> logger,
            IConfiguration configuration,
            IBroker broker,
            IFinanceiroProviderService financeiroUsuarioProviderService,
            ICommunicationProvider communicationProvider,
            IServiceBase serviceBase,
            IOptions<BrokerModel> brokerConfig,
            ICacheStore cacheStore)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
            _broker = broker;
            _financeiroProviderService = financeiroUsuarioProviderService;
            _communicationProvider = communicationProvider;
            _serviceBase = serviceBase;
            _brokerModel = brokerConfig.Value;
            _cacheStore = cacheStore;
        }

        public async Task<TransactionCardResultModel?> DoCardTransaction(DoTransactionCardInputModel doTransactionModel)
        {

            TransactionCardResultModel? transactionCardResultModel = null;
            var jsonBodyRequest = String.Empty;
            var jsonBodyResponse = String.Empty;
            try
            {
                _repository.BeginTransaction();

                if (doTransactionModel.PessoaId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o parâmetro PessoaId");

                var vinculoPessoaProvider = (await _serviceBase.GetPessoaProviderVinculadaPessoaSistema($"{doTransactionModel.PessoaId.GetValueOrDefault()}", _communicationProvider.CommunicationProviderName));
                if (vinculoPessoaProvider == null)
                    throw new ArgumentException($"Não foi encontrada a pessoa do sistema com os dados informados pessoaId: {doTransactionModel.PessoaId}");

                if (!string.IsNullOrEmpty(vinculoPessoaProvider.PessoaProvider))
                {
                    var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(vinculoPessoaProvider.PessoaProvider!) });
                    if (propCache != null && propCache.Any(b => b.frAtendimentoStatusCrcModels.Any(b => b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S")))
                    {
                        throw new ArgumentException("Não foi possível efetuar o pagamento em cartão, motivo 0001BL");
                    }
                }

                var pessoa = await _repository.FindById<Domain.Entities.Core.DadosPessoa.Pessoa>(doTransactionModel.PessoaId.GetValueOrDefault());
                if (pessoa == null)
                    throw new ArgumentException($"Não foi encontrada pessoa com o Id: {doTransactionModel.PessoaId}");

                if (doTransactionModel.CardTokenizedId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException($"Deve ser informado o CardTokenizedId");

                var usuarioVinculadoPessoa = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where p.Id = {pessoa.Id}  and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ")).FirstOrDefault();

                var cardExistente = (await _repository.FindByHql<CardTokenized>(@$"From 
                        CardTokenized ct 
                        Inner Join Fetch ct.Pessoa p 
                    Where 
                        p.Id = {pessoa.Id} and 
                        (ct.Visivel is null or Coalesce(ct.Visivel,0) = 1) and
                        ct.Id = {doTransactionModel.CardTokenizedId.GetValueOrDefault()}")).FirstOrDefault();

                if (cardExistente == null)
                    throw new ArgumentException($"Não foi localizado o CardTokenized com o id informado: {doTransactionModel.CardTokenizedId} vinculado a pessoa id: {doTransactionModel.PessoaId.GetValueOrDefault()}");


                var contas = await _financeiroProviderService.GetContasParaPagamentoEmCartaoDoUsuario(doTransactionModel);
                var dadosPessoa = await _financeiroProviderService.GetDadosPessoa(Convert.ToInt32(vinculoPessoaProvider.PessoaProvider));
                if (dadosPessoa == null)
                    throw new ArgumentException($"Não foi possível encontrar a pessoa com Id: {vinculoPessoaProvider.PessoaProvider} no provider: {_communicationProvider.CommunicationProviderName}");

                var cardUtilizar = await GetCardUtilizar(cardExistente.Acquirer, cardExistente.Brand, cardExistente.CardNumber, cardExistente.ClienteId, cardExistente.Pessoa, contas.First().EmpresaId);

                await ValidarPagamentoEmDuplicidade(contas, pessoa);

                var documento = dadosPessoa.TipoPessoa == 0 && !string.IsNullOrEmpty(dadosPessoa.Cpf) ? dadosPessoa.Cpf : "";
                if (string.IsNullOrEmpty(documento) && !string.IsNullOrEmpty(dadosPessoa.Cnpj) && dadosPessoa.TipoPessoa == EnumTipoPessoa.Juridica)
                    documento = dadosPessoa.Cnpj;

                var tipoDocumento = dadosPessoa.TipoPessoa == 0 && !string.IsNullOrEmpty(dadosPessoa.Cpf) ? "CPF" : "";
                if (string.IsNullOrEmpty(documento) && !string.IsNullOrEmpty(dadosPessoa.Cnpj) && dadosPessoa.TipoPessoa == EnumTipoPessoa.Juridica)
                    tipoDocumento = "CNPJ";


                var customerUtilizar = new CustomerModel()
                {
                    rid = pessoa.Id,
                    type = pessoa.TipoPessoa == 0 ? "F" : "J",
                    name = pessoa.Nome,
                    document = documento,
                    document_type = tipoDocumento,
                    email = !string.IsNullOrEmpty(dadosPessoa?.Email) && dadosPessoa.Email.Contains(";") ? dadosPessoa.Email.Split(";")[0] : dadosPessoa?.Email,
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
                                state = !string.IsNullOrEmpty(dadosPessoa?.EstadoSigla) && dadosPessoa?.EstadoSigla.Length > 2 ? dadosPessoa?.EstadoSigla.Substring(0,2) : "",
                                country = dadosPessoa?.SiglaPais
                            },
                    foreigner = dadosPessoa?.SiglaPais != "BR",
                    gender = dadosPessoa?.Sexo ?? "M",
                    birth = $"{dadosPessoa?.DataNascimento.GetValueOrDefault().Date:yyyy-MM-dd} 00:00:00",
                    created = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    registered = true
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
                jsonBodyRequest = System.Text.Json.JsonSerializer.Serialize(transactionModel);
                jsonBodyResponse = System.Text.Json.JsonSerializer.Serialize(transactionCardResultModel);

                try
                {

                    await GravarPaymentCardHistory(transactionModel, transactionCardResultModel!, doTransactionModel, contas);

                    await _cacheStore.AddAsync($"{CARD_TRANSACTION_CACHE_KEY}{transactionCardResultModel!.payment_id}",
                        new TransactionCacheModel()
                        {
                            TransactionId = transactionCardResultModel.payment_id,
                            TransactionCard = transactionModel,
                            TransactionCardResult = transactionCardResultModel,
                            ContasVinculadasIds = string.Join(",", contas.Select(b => b.Id)),
                            EmpresaLogadaId = contas.First().EmpresaId.GetValueOrDefault().ToString(),
                            CardTokenized = cardUtilizar,
                            Contas = contas,
                        }, DateTimeOffset.Now.AddDays(10),CARD_CACHE_DB,_repository.CancellationToken);
                }
                catch (Exception err)
                {
                    _logger.LogError("Falha na gravação da transação no cache", err.Message);
                    var resultCancelamento = await _broker.CancelCardTransaction(new TransactionCancelModel() { payment_id = transactionCardResultModel!.payment_id, value = doTransactionModel.ValorTotal }, cardUtilizar.EmpresaLegadoId.GetValueOrDefault());
                    if (transactionCardResultModel.errors == null) transactionCardResultModel.errors = new List<string>();

                    transactionCardResultModel.errors.Add("Prrocessamento cancelado, falha na gravação da transação.");
                }

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

            return cardsMesmaCompanyId ?? cardExistente;

        }

        private async Task ValidarPagamentoEmDuplicidade(List<ContaPendenteModel> contas, Domain.Entities.Core.DadosPessoa.Pessoa pessoa)
        {
            var strIn = "";
            foreach (var item in contas)
            {
                if (string.IsNullOrEmpty(strIn))
                {
                    strIn = $"'{item}'";
                }
                else strIn += $",'{item}'";
            }

            var itensJaPagosEmCartao = (await _repository.FindByHql<PaymentCardTokenizedItem>(@$"From 
                                PaymentCardTokenizedItem pcti 
                                Inner Join Fetch pcti.PaymentCardTokenized pct
                                Inner Join Fetch pct.CardTokenized  cc
                                Inner Join Fetch cc.Pessoa p
                                Where 
                                p.Id = {pessoa.Id} and
                                Lower(pct.Status) = 'captured' and                                
                                pcti.ItemId in ({strIn})")).AsList();

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
                                lower(pp.Status) = 'captured' and                                
                                pcti.ItemId in ({strIn})")).AsList();

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

        private async Task GravarPaymentCardHistory(TransactionCardModel transactionModel, TransactionCardResultModel transactionCartResultModel,
            DoTransactionCardInputModel doTransactionModel, List<ContaPendenteModel> contas)
        {
            var jsonBodyRequest = System.Text.Json.JsonSerializer.Serialize(transactionModel);
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(transactionCartResultModel);

            if (transactionCartResultModel != null && !string.IsNullOrEmpty(transactionCartResultModel?.last_acquirer_response?.response?.authorization) &&
             !string.IsNullOrEmpty(transactionCartResultModel?.last_acquirer_response?.response?.nsu) &&
             !string.IsNullOrEmpty(transactionCartResultModel?.last_acquirer_response?.status) &&
             transactionCartResultModel.last_acquirer_response.status.Contains("captured", StringComparison.CurrentCultureIgnoreCase))
            {
                var paymentCardTokenized = new PaymentCardTokenized()
                {
                    CardTokenized = new CardTokenized() { Id = doTransactionModel.CardTokenizedId.GetValueOrDefault() },
                    Valor = doTransactionModel.ValorTotal,
                    PaymentId = transactionCartResultModel?.payment_id,
                    Status = transactionCartResultModel?.last_acquirer_response?.status,
                    Nsu = transactionCartResultModel?.last_acquirer_response?.response?.nsu,
                    CodigoAutorizacao = transactionCartResultModel?.last_acquirer_response?.response?.authorization,
                    Adquirente = transactionCartResultModel?.company?.acquirer,
                    AdquirentePaymentId = transactionCartResultModel?.last_acquirer_response?.response?.payment_id,
                    TransactionId = transactionCartResultModel?.last_acquirer_response?.response?.transaction,
                    Url = transactionCartResultModel?.last_acquirer_response?.response?.url,
                    CompanyId = transactionCartResultModel?.company?.id,
                    DadosEnviados = jsonBodyRequest,
                    Retorno = jsonResponse,
                    EmpresaLegadoId = contas.First().EmpresaId
                };

                await _repository.Save(paymentCardTokenized);

                //Gravo os dos ítens pagos na operação
                foreach (var item in contas)
                {
                    var itemPaid = new PaymentCardTokenizedItem()
                    {
                        PaymentCardTokenized = paymentCardTokenized,
                        ValorNaTransacao = item.ValorAtualizado,
                        Valor = item.Valor,
                        Vencimento = item.Vencimento,
                        ItemId = $"{item.Id}",
                        DescricaoDoItem = $"PessoaId:{item.PessoaId}|PessoaProviderId:{item.PessoaProviderId}|CódigoTipoConta:{item.CodigoTipoConta}|NomeTipoConta:{item.NomeTipoConta}|Vencimento:{item.Vencimento:dd/MM/yyyy}"
                    };
                    await _repository.Save(itemPaid);
                }

                //var baixa = await _financeiroProviderService.AlterarTipoContaReceberPagasEmCartao(paymentCardTokenized, null);
                await _repository.Save(paymentCardTokenized);
            }
            else
            {
                await GravarTentativa(transactionCartResultModel, doTransactionModel, jsonBodyRequest, jsonResponse);
            }
        }

        private async Task GravarTentativa(TransactionCardResultModel? transactionCartResultModel, DoTransactionCardInputModel doTransactionModel, string jsonBodyRequest, string jsonResponse)
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
                RetornoAmigavel = transactionCartResultModel != null && transactionCartResultModel.last_acquirer_response != null ? $"{transactionCartResultModel?.last_acquirer_response?.response?.code}-{transactionCartResultModel?.last_acquirer_response?.response?.message}" : "Não foi possível processar o pagamento"
            };
            await _repository.Save(tentativa);
        }

        public async Task<TransactionPixResultModel?> GeneratePixTransaction(DoTransactionPixInputModel doTransactionModel)
        {
            try
            {
                _repository.BeginTransaction();

                var loggedUser = await _repository.GetLoggedUser();

                if (doTransactionModel.PessoaId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o parâmetro PessoaId");

                var vinculoPessoaProvider = (await _serviceBase.GetPessoaProviderVinculadaPessoaSistema($"{doTransactionModel.PessoaId.GetValueOrDefault()}", _financeiroProviderService.ProviderName));

                if (!loggedUser.Value.isAdm)
                {
                    if (vinculoPessoaProvider == null)
                        throw new ArgumentException("Não foi possível identificar os dados no sistema legado");

                    if (!string.IsNullOrEmpty(vinculoPessoaProvider.PessoaProvider))
                    {
                        var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(vinculoPessoaProvider.PessoaProvider!) });
                        if (propCache != null && propCache.Any(b => b.frAtendimentoStatusCrcModels.Any(b => b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S")))
                        {
                            throw new ArgumentException("Não foi possível efetuar a geração do QRCode para pagamento, motivo 0001BL");
                        }
                    }
                }


                string idPessoaUtilizar = vinculoPessoaProvider != null && !string.IsNullOrEmpty(vinculoPessoaProvider.PessoaSistema) ? vinculoPessoaProvider.PessoaSistema : $"{doTransactionModel.PessoaId.GetValueOrDefault()}";

                if (string.IsNullOrEmpty(idPessoaUtilizar))
                    throw new ArgumentException($"Não foi encontrada a pessoa do sistema com os dados informados Pessoaid: {doTransactionModel.PessoaId}");

                var pessoa = await _repository.FindById<Domain.Entities.Core.DadosPessoa.Pessoa>(Convert.ToInt32(idPessoaUtilizar));
                if (pessoa == null)
                    throw new ArgumentException($"Não foi encontrada pessoa com o Id: {doTransactionModel.PessoaId.GetValueOrDefault()}");

                var usuarioVinculadoPessoa = (await _repository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where p.Id = {pessoa.Id}  and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();

                if (usuarioVinculadoPessoa == null)
                    throw new ArgumentException($"Não foi encontrado usuário no sistema com a pessoa id: {doTransactionModel.PessoaId.GetValueOrDefault()}, primeiro é necessário cadastrar a pessoa como usuário do sistema");


                var contas = await _financeiroProviderService.GetContasParaPagamentoEmPixDoUsuario(doTransactionModel);
                await ValidarPagamentoEmDuplicidade(contas, pessoa);

                var dadosPessoa = await _financeiroProviderService.GetDadosPessoa(Convert.ToInt32(vinculoPessoaProvider?.PessoaProvider));
                if (dadosPessoa == null)
                    throw new ArgumentException($"Não foi possível encontrar a pessoa com Id: {vinculoPessoaProvider?.PessoaSistema} no provider: {_financeiroProviderService.ProviderName}");

                var documento = dadosPessoa.TipoPessoa == 0 && !string.IsNullOrEmpty(dadosPessoa.Cpf) ? dadosPessoa.Cpf : "";
                if (string.IsNullOrEmpty(documento) && !string.IsNullOrEmpty(dadosPessoa.Cnpj) && dadosPessoa.TipoPessoa == EnumTipoPessoa.Juridica)
                    documento = dadosPessoa.Cnpj;

                var tipoDocumento = dadosPessoa.TipoPessoa == 0 && !string.IsNullOrEmpty(dadosPessoa.Cpf) ? "CPF" : "";
                if (string.IsNullOrEmpty(documento) && !string.IsNullOrEmpty(dadosPessoa.Cnpj) && dadosPessoa.TipoPessoa == EnumTipoPessoa.Juridica)
                    tipoDocumento = "CNPJ";

                var customerUtilizar = new CustomerModel()
                {
                    rid = pessoa.Id,
                    type = pessoa.TipoPessoa == 0 ? "F" : "J",
                    name = pessoa.Nome,
                    document = documento,
                    document_type = tipoDocumento,
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
                                number = dadosPessoa.Numero,
                                neighborhood = dadosPessoa.Bairro,
                                zip_code = dadosPessoa.Cep,
                                city = dadosPessoa.CidadeNome,
                                state = !string.IsNullOrEmpty(dadosPessoa?.EstadoSigla) && dadosPessoa?.EstadoSigla.Length > 2 ? dadosPessoa?.EstadoSigla.Substring(0, 2) : "",
                                country = dadosPessoa?.SiglaPais
                            },
                    foreigner = dadosPessoa?.SiglaPais != "BR",
                    gender = dadosPessoa?.Sexo ?? "M",
                    birth = $"{dadosPessoa?.DataNascimento.GetValueOrDefault().Date:yyyy-MM-dd} 00:00:00",
                    registered = true,
                    created = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                };
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

                try
                {

                    await GravarGeracaoPixHistory(transactionModel, transactionCardResultModel!, doTransactionModel, contas);
                    transactionCardResultModel!.qrCode = transactionCardResultModel?.last_acquirer_response?.response?.qrcode;

                    await _cacheStore.AddAsync($"{PIX_TRANSACTION_CACHE_KEY}{transactionCardResultModel!.payment_id}",
                        new TransactionCacheModel()
                        {
                            TransactionId = transactionCardResultModel.payment_id,
                            TransactionPixResult = transactionCardResultModel,
                            TransactionPix = transactionModel,
                            ContasVinculadasIds = string.Join(",", contas.Select(b => b.Id)),
                            EmpresaLogadaId = contas.First().EmpresaId.GetValueOrDefault().ToString(),
                            Contas = contas,
                        }, DateTimeOffset.Now.AddDays(10), PIX_CACHE_DB, _repository.CancellationToken);
                }
                catch (Exception err)
                {
                    _logger.LogError("Falha na gravação da transação no cache", err.Message);
                    if (transactionCardResultModel != null)
                    {
                        if (transactionCardResultModel.errors == null) 
                            transactionCardResultModel.errors = new List<string>();

                        transactionCardResultModel.errors.Add("Prrocessamento cancelado, falha na gravação da transação.");
                    }
                }


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
            var requestJson = System.Text.Json.JsonSerializer.Serialize(transactionModel);

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
                    ValidoAte = transactionPixResultModel?.expiration_date.GetValueOrDefault(),
                    Retorno = json,
                    DadosEnviados = requestJson
                };

                await _repository.Save(paymentPix);

                //Gravo os vinculados na operação
                foreach (var item in contas)
                {
                    var itemPaid = new PaymentPixItem()
                    {
                        PaymentPix = paymentPix,
                        Valor = item.Valor,
                        ValorNaTransacao = item.ValorAtualizado,
                        Vencimento = item.Vencimento,
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
                        ValorNaTransacao = item.ValorAtualizado,
                        Vencimento = item.Vencimento,
                        ItemId = $"{item.Id}",
                        DescricaoDoItem = $"PessoaId:{item.PessoaId}|PessoaProviderId:{item.PessoaProviderId}|CódigoTipoConta:{item.CodigoTipoConta}|NomeTipoConta:{item.NomeTipoConta}|Vencimento:{item.Vencimento:dd/MM/yyyy}"
                    };
                    await _repository.Save(itemPaid);
                }
            }
        }

        public async Task<List<CardTokenizedModel>> GetMyTokenizedCards()
        {

            try
            {
                _repository.BeginTransaction();
                var loggedUser = await _repository.GetLoggedUser();
                if (string.IsNullOrEmpty(loggedUser.Value.userId))
                    throw new ArgumentException("Não foi possível identificar o usuário logado no sistema para consultar os seus meios de pagamentos");

                var usuario = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {Convert.ToInt32(loggedUser.Value.userId)} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();


                var cardsExistentes = (await _repository.FindByHql<CardTokenized>(@$"From 
                            CardTokenized ct 
                            Inner Join Fetch ct.Pessoa p 
                            Where
                            (ct.Visivel is null or Coalesce(ct.Visivel,0) = 1) and
                            Lower(ct.Status) = 'created' and
                            p.Id = {usuario?.Pessoa?.Id}")).AsList();

                foreach (var item in cardsExistentes)
                {
                    if (item.Status == "created" && DateTime.Now.Subtract(item.DataHoraCriacao.GetValueOrDefault()).TotalMinutes >= 30)
                    {
                        item.Status = "expired";
                        await _repository.Save(item);
                    }
                }


                List<CardTokenizedModel> cardsRetornar = new List<CardTokenizedModel>();

                if (cardsExistentes != null && cardsExistentes.Any())
                {
                    cardsRetornar = cardsExistentes.Select(b =>
                        new CardTokenizedModel()
                        {
                            Id = b.Id,
                            PessoaId = b.Pessoa?.Id,
                            PessoaNome = b.Pessoa?.Nome,
                            UsuarioCriacao = b.UsuarioCriacao,
                            UsuarioAlteracao = b.UsuarioAlteracao,
                            DataHoraCriacao = b.DataHoraCriacao,
                            DataHoraAlteracao = b.DataHoraAlteracao,
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
                        }).DistinctBy(a => a.card!.card_number).AsList();
                }

                var resultCommit = await _repository.CommitAsync();

                return cardsRetornar;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                throw;
            }
        }

        public async Task<TransactionCardResultModel?> GetTransactionResult(string paymentId)
        {
            throw new NotImplementedException();
        }

        public async Task<CardTokenizedModel?> TokenizeMyCard(TokenizeMyCardInputModel cardModel)
        {
            var loggedUser = await _repository.GetLoggedUser();
            if (string.IsNullOrEmpty(loggedUser.Value.userId))
                throw new ArgumentException("Não foi possível identificar o usuário logado no sistema.");

            var usuarioLogado = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {loggedUser.Value.userId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
            if (usuarioLogado == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            var parametrosSistema = await _repository.GetParametroSistemaViewModel();
            if (parametrosSistema == null || string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds))
                throw new ArgumentException("Não foi configurado o parâmetro: 'ExibirFinanceirosDasEmpresasIds'");

            var vinculoPessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId), _financeiroProviderService.ProviderName);

            if (!loggedUser.Value.isAdm)
            {
                if (vinculoPessoaProvider == null)
                    throw new ArgumentException("Não foi possível identificar os dados no sistema legado");

                if (!string.IsNullOrEmpty(vinculoPessoaProvider.PessoaProvider))
                {
                    var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(vinculoPessoaProvider.PessoaProvider!) });
                    if (propCache != null && propCache.Any(b => b.frAtendimentoStatusCrcModels.Any(b => (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && b.AtendimentoStatusCrcStatus == "A")))
                    {
                        throw new ArgumentException("Não foi possível efetuar a tokenização de cartão, motivo 0001BL");
                    }
                }
            }

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
                else if (cardModel.card.card_number.StartsWith("60"))
                    cardModel.card.brand = "discover";
                else if (cardModel.card.card_number.StartsWith("50"))
                    cardModel.card.brand = "aura";
                else if (cardModel.card.card_number.StartsWith("5"))
                    cardModel.card.brand = "mastercard";
                else if (cardModel.card.card_number.StartsWith("6"))
                    cardModel.card.brand = "discover";
                else if (cardModel.card.card_number.StartsWith("8"))
                    cardModel.card.brand = "discover";


                if (string.IsNullOrEmpty(cardModel.card?.brand))
                    throw new ArgumentException("Não foi possível identificar a bandeira do cartão informado.");
            }

            var numeroCartaoNormalizado = Helper.ApenasNumeros(cardModel?.card?.card_number);

            if (string.IsNullOrEmpty(numeroCartaoNormalizado) || (numeroCartaoNormalizado.Length != 16 && numeroCartaoNormalizado.Length != 15))
                throw new ArgumentException("O número do cartão deve ser informado com 15 ou 16 caracteres");

            if (cardModel != null && cardModel.card.due_date.Length == 5 && cardModel.card.due_date.Contains("/"))
            {
                var dueDateToChange = cardModel.card.due_date;
                cardModel.card.due_date = $"{dueDateToChange.Split("/")[0]}/20{dueDateToChange.Split("/")[1]}";
            }

            var hash = Helper.ApenasNumeros(cardModel.card.card_number).GetHashCode();

            var cardExistente = (await _repository.FindByHql<CardTokenized>(@$"From 
                        CardTokenized ct 
                        Inner Join Fetch ct.Pessoa p 
                    Where 
                        ct.Hash = '{hash}' and p.Id = {usuarioLogado.Pessoa.Id} and 
                        (ct.Visivel is null or Coalesce(ct.Visivel,0) = 1)")).FirstOrDefault();

            if (cardExistente != null)
            {
                if (cardModel.KeepCardData.GetValueOrDefault(false) == true)
                {
                    cardExistente.Visivel = Domain.Enumns.EnumSimNao.Não;
                    await _repository.Save(cardExistente);
                    return null;
                }

                return new CardTokenizedModel()
                {
                    card = new CardInputModel()
                    {
                        brand = cardExistente.Brand,
                        card_number = cardExistente.CardNumber!.Replace(" ", ""),
                        cvv = cardExistente.Cvv,
                        due_date = cardExistente.DueDate,
                        card_holder = cardExistente.CardHolder
                    },
                    token = cardExistente.Token,
                    token2 = cardExistente.Token2,
                    company = new CompanyModel()
                    {
                        id = cardExistente.CompanyId,
                        acquirer = cardExistente.Acquirer
                    },
                    status = cardExistente.Status
                };
            }

            var cardTokenizedToSend = new CardTokenizeInputModel()
            {
                card = new CardInputModel()
                {
                    brand = cardModel.card?.brand,
                    card_holder = cardModel.card?.card_holder,
                    cvv = cardModel.card?.cvv,
                    due_date = cardModel.card?.due_date,
                    card_number = cardModel.card?.card_number!.Replace(" ", "")
                }
            };

            CardTokenizedModel? tokenizedReturn = null;

            foreach (var item in parametrosSistema!.ExibirFinanceirosDasEmpresaIds!.Split(",")
                .Where(c => int.TryParse(c, out _)))
            {
                tokenizedReturn = await _broker.Tokenize(cardTokenizedToSend, int.Parse(item));
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
                        ClienteId = usuarioLogado.ProviderChaveUsuario,
                        Pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa() { Id = usuarioLogado.Pessoa.Id },
                        EmpresaLegadoId = int.Parse(item)
                    };

                    await _repository.Save(cardTokenized);
                }
            }

            return tokenizedReturn;
        }

        public async Task<bool> RemoveMyCardTokenized(int cardTokenizedId)
        {

            try
            {
                _repository.BeginTransaction();

                var loggedUser = await _repository.GetLoggedUser();
                if (string.IsNullOrEmpty(loggedUser.Value.userId))
                    throw new ArgumentException("Não foi possível identificar o usuário logado no sistema.");

                var usuarioLogado = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {loggedUser.Value.userId}  and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                if (usuarioLogado == null)
                    throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

                if (cardTokenizedId == 0)
                    throw new ArgumentException("Deve ser informado o id do cartão a ser removido!");

                var vinculoPessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId), _financeiroProviderService.ProviderName);

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

                var cardToRemove = (await _repository.FindByHql<CardTokenized>(@$"From 
                        CardTokenized ct 
                        Inner Join Fetch ct.Pessoa p 
                    Where 
                        p.Id = {usuarioLogado.Pessoa.Id} and 
                        Coalesce(ct.Visivel,0) = 1 and 
                        ct.Id = {cardTokenizedId}")).FirstOrDefault();

                if (cardToRemove == null)
                    throw new ArgumentException($"Não foi encontrado o cartão com o id informado: {cardTokenizedId}");


                cardToRemove.Visivel = Domain.Enumns.EnumSimNao.Não;
                await _repository.Save(cardToRemove);

                var result = await _repository.CommitAsync();
                if (!result.executed)
                    throw result.exception ?? new Exception("Erro na operação");

                return true;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                throw;
            }

        }
    }
}
