using AccessCenterDomain.AccessCenter;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Application.Services.Providers.Interfaces
{
    public interface IFinanceiroProviderService
    {
        string ProviderName { get; }
        string PrefixoTransacaoFinanceira { get; }

        #region Acesso de administradores
        Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral(SearchContasPendentesGeral searchModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral(DoTransactionPixInputModel getContasParaPagamentoEmPixModel);

        #region Boleto
        Task<BoletoModel> DownloadBoleto(DownloadBoleto model);

        #endregion


        #endregion

        #region Acesso do proprietário
        Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario(SearchContasPendentesUsuarioLogado searchModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario(DoTransactionPixInputModel getContasParaPagamentoEmPixModel);

        #endregion

        #region Uso Geral

        Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa(int pessoaProviderId);
        Task<List<ContaPendenteModel>> GetContasPorIds(List<int> itensToPay);
        Task<List<CotaPeriodoModel>> GetCotaPeriodo(int pessoaId, DateTime? dataInicial, DateTime? dataFinal);
        Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail(bool pool, bool naoPool);
        Task<BaixaResultModel> BaixarValoresPagosEmPix(PaymentPix item, NHibernate.IStatelessSession? session);
        Task<BaixaResultModel> AlterarTipoContaReceberPagasEmCartao(PaymentCardTokenized item, NHibernate.IStatelessSession? session);
        Task<bool> VoltarParaTiposOriginais(PaymentCardTokenized item, NHibernate.IStatelessSession? session);
        Task<List<ClienteContaBancariaViewModel>> GetContasBancarias(int pessoaId);
        Task<int> SalvarContaBancaria(ClienteContaBancariaInputModel model);
        Task<int> SalvarMinhaContaBancaria(ClienteContaBancariaInputModel model);
        Task<List<ClienteContaBancariaViewModel>> GetMinhasContasBancarias();
        Task<ClienteContaBancaria?> SalvarContaBancariaInterna(ClienteContaBancariaInputModel model, ParametroSistemaViewModel? parametroSistema = null);

        #endregion

    }
}
