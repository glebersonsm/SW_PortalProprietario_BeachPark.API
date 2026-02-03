using AccessCenterDomain.AccessCenter;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Application.Services.Providers.Interfaces
{
    public interface IFinanceiroHybridProviderService
    {
        #region CM Properties
        string ProviderName_CM { get; }
        string PrefixoTransacaoFinanceira_CM { get; }
        #endregion

        #region CM Methods
        Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral_CM(SearchContasPendentesGeral searchModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral_CM(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral_CM(DoTransactionPixInputModel getContasParaPagamentoEmPixModel);

        Task<BoletoModel> DownloadBoleto_CM(DownloadBoleto model);

        Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario_CM(SearchContasPendentesUsuarioLogado searchModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario_CM(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario_CM(DoTransactionPixInputModel getContasParaPagamentoEmPixModel);

        Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa_CM(int pessoaProviderId);
        Task<List<ContaPendenteModel>> GetContasPorIds_CM(List<int> itensToPay);
        Task<List<CotaPeriodoModel>> GetCotaPeriodo_CM(int pessoaId, DateTime? dataInicial, DateTime? dataFinal);
        Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail_CM(bool pool, bool naoPool);
        Task<BaixaResultModel> BaixarValoresPagosEmPix_CM(PaymentPix item, NHibernate.IStatelessSession? session);
        Task<BaixaResultModel> AlterarTipoContaReceberPagasEmCartao_CM(PaymentCardTokenized item, NHibernate.IStatelessSession? session);
        Task<bool> VoltarParaTiposOriginais_CM(PaymentCardTokenized item, NHibernate.IStatelessSession? session);
        Task<List<ClienteContaBancariaViewModel>> GetContasBancarias_CM(int pessoaId);
        Task<int> SalvarContaBancaria_CM(ClienteContaBancariaInputModel model);
        Task<int> SalvarMinhaContaBancaria_CM(ClienteContaBancariaInputModel model);
        Task<List<ClienteContaBancariaViewModel>> GetMinhasContasBancarias_CM();
        Task<ClienteContaBancaria?> SalvarContaBancariaInterna_CM(ClienteContaBancariaInputModel model, ParametroSistemaViewModel? parametroSistema = null);
        #endregion

        #region Esolution Properties
        string ProviderName_Esol { get; }
        string PrefixoTransacaoFinanceira_Esol { get; }
        #endregion

        #region Esolution Methods
        Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral_Esol(SearchContasPendentesGeral searchModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral_Esol(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral_Esol(DoTransactionPixInputModel getContasParaPagamentoEmPixModel);

        Task<BoletoModel> DownloadBoleto_Esol(DownloadBoleto model);

        Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario_Esol(SearchContasPendentesUsuarioLogado searchModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario_Esol(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario_Esol(DoTransactionPixInputModel getContasParaPagamentoEmPixModel);

        Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa_Esol(int pessoaProviderId);
        Task<List<ContaPendenteModel>> GetContasPorIds_Esol(List<int> itensToPay);
        Task<List<CotaPeriodoModel>> GetCotaPeriodo_Esol(int pessoaId, DateTime? dataInicial, DateTime? dataFinal);
        Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail_Esol(bool pool, bool naoPool);
        Task<BaixaResultModel> BaixarValoresPagosEmPix_Esol(PaymentPix item, NHibernate.IStatelessSession? session);
        Task<BaixaResultModel> AlterarTipoContaReceberPagasEmCartao_Esol(PaymentCardTokenized item, NHibernate.IStatelessSession? session);
        Task<bool> VoltarParaTiposOriginais_Esol(PaymentCardTokenized item, NHibernate.IStatelessSession? session);
        Task<List<ClienteContaBancariaViewModel>> GetContasBancarias_Esol(int pessoaId);
        Task<int> SalvarContaBancaria_Esol(ClienteContaBancariaInputModel model);
        Task<int> SalvarMinhaContaBancaria_Esol(ClienteContaBancariaInputModel model);
        Task<List<ClienteContaBancariaViewModel>> GetMinhasContasBancarias_Esol();
        Task<ClienteContaBancaria?> SalvarContaBancariaInterna_Esol(ClienteContaBancariaInputModel model, ParametroSistemaViewModel? parametroSistema = null);
        #endregion

        #region Default Properties and Methods (delegates to Esol)
        string ProviderName { get; }
        string PrefixoTransacaoFinanceira { get; }

        Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral(SearchContasPendentesGeral searchModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral(DoTransactionPixInputModel getContasParaPagamentoEmPixModel);
        Task<BoletoModel> DownloadBoleto(DownloadBoleto model);
        Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario(SearchContasPendentesUsuarioLogado searchModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel);
        Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario(DoTransactionPixInputModel getContasParaPagamentoEmPixModel);
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
