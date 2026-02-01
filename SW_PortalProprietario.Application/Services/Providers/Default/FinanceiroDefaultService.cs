using AccessCenterDomain.AccessCenter;
using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Application.Services.Providers.Default
{
    public class FinanceiroDefaultService : IFinanceiroProviderService
    {
        private const string PREFIXO_TRANSACOES_FINANCEIRAS = "PORTALPROP_";
        private readonly ICommunicationProvider _communicationProvider;
        private readonly ILogger<FinanceiroDefaultService> _logger;
        public FinanceiroDefaultService(ICommunicationProvider communicationProvider,
            ILogger<FinanceiroDefaultService> logger)
        {
            _communicationProvider = communicationProvider;
            _logger = logger;
        }

        public string ProviderName => "Default";

        public string PrefixoTransacaoFinanceira => PREFIXO_TRANSACOES_FINANCEIRAS;

        public async Task<BaixaResultModel> BaixarValoresPagosEmPix(PaymentPix item, IStatelessSession? session)
        {
            throw new NotImplementedException();
        }

        public async Task<BaixaResultModel> AlterarTipoContaReceberPagasEmCartao(PaymentCardTokenized item, IStatelessSession? session)
        {
            throw new NotImplementedException();
        }

        public Task<BoletoModel> DownloadBoleto(DownloadBoleto model)
        {
            throw new NotImplementedException();
        }


        public Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario(SearchContasPendentesUsuarioLogado searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral(SearchContasPendentesGeral searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasPorIds(List<int> itensToPay)
        {
            throw new NotImplementedException();
        }

        public Task<List<CotaPeriodoModel>> GetCotaPeriodo(int pessoaId, DateTime? dataInicial, DateTime? dataFinal)
        {
            throw new NotImplementedException();
        }

        public Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa(int pessoaProviderId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail(bool pool, bool naoPool)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VoltarParaTiposOriginais(PaymentCardTokenized item, IStatelessSession? session)
        {
            throw new NotImplementedException();
        }

        public Task<int> SalvarContaBancaria(ClienteContaBancariaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<List<ClienteContaBancariaViewModel>> GetContasBancarias(int pessoaId)
        {
            throw new NotImplementedException();
        }

        public Task<int> SalvarMinhaContaBancaria(ClienteContaBancariaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<List<ClienteContaBancariaViewModel>> GetMinhasContasBancarias()
        {
            throw new NotImplementedException();
        }

        public Task<ClienteContaBancaria?> SalvarContaBancariaInterna(ClienteContaBancariaInputModel model, ParametroSistemaViewModel? parametroSistema = null)
        {
            throw new NotImplementedException();
        }
    }
}
