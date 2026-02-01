using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Providers;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Infra.Data.CommunicationProviders.Default
{
    public class CommunicationProviderDefault : ICommunicationProvider
    {
        public string CommunicationProviderName => "Default";

        public string PrefixoTransacaoFinanceira => throw new NotImplementedException();

        public CommunicationProviderDefault()
        {
        }
        public async Task<IAccessValidateResultModel> ValidateAccess(string login, string senha, string pessoaProviderId = "")
        {
            return await Task.FromResult(new AccessValidateResultModel() { ProviderName = CommunicationProviderName });
        }

        public async Task<bool> IsDefault()
        {
            return await Task.FromResult(true);
        }

        public async Task GravarVinculoUsuario(IAccessValidateResultModel result, Usuario usuario)
        {
            await Task.CompletedTask;
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario(SearchContasPendentesUsuarioLogado searchModel)
        {
            return await Task.FromResult((-1, -1, new List<ContaPendenteModel>()));
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis(SearchImovelModel searchModel)
        {
            return await Task.FromResult((-1, -1, new List<ImovelSimplificadoModel>()));
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios(SearchProprietarioModel searchModel)
        {
            return await Task.FromResult((-1, -1, new List<ProprietarioSimplificadoModel>()));
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral(SearchContasPendentesGeral searchModel)
        {
            return await Task.FromResult((-1, -1, new List<ContaPendenteModel>()));
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<PessoaParaTransacaoBrokerModel> GetDadosPessoa(int pessoaProviderId, DateTime? dataInicial, DateTime? dataFinal)
        {
            return await Task.FromResult(new PessoaParaTransacaoBrokerModel());
        }

        public async Task<List<ContaPendenteModel>> GetContasPorIds(List<int> itensToPay)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa(int pessoaProviderId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<CotaPeriodoModel>> GetCotaPeriodo(int pessoaId, DateTime? dataInicial, DateTime? dataFinal)
        {
            return await Task.FromResult(new List<CotaPeriodoModel>());
        }

        public async Task<List<CotaPeriodoModel>> ProprietarioNoPoolHoje(int pessoaId)
        {
            return await Task.FromResult(new List<CotaPeriodoModel>());
        }

        public async Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail(bool pool, bool naoPool)
        {
            return await Task.FromResult(new List<CotaPeriodoModel>());
        }

        public async Task<BoletoModel> DownloadBoleto(DownloadBoleto model)
        {
            return await Task.FromResult(new BoletoModel());
        }

        public async Task<LoginResult> GetToken(LoginRequest request)
        {
            return await Task.FromResult(new LoginResult());
        }

        public async Task<bool> GravarUsuarioNoLegado(string pessoaProviderId, string login, string senha)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> AlterarSenhaNoLegado(string pessoaProviderId, string login, string senha)
        {
            return await Task.FromResult(true);

        }

        public async Task<VinculoAccessXPortalBase> GetOutrosDadosPessoaProvider(string pessoaProviderId)
        {
            return await Task.FromResult(new VinculoAccessXPortalBase());
        }

        public async Task<EmpresaSimplificadaModel> GetEmpresaVinculadaLegado(int id)
        {
            return await Task.FromResult(new EmpresaSimplificadaModel { Id = id });
        }

        public async Task<List<PaisModel>> GetPaisesLegado()
        {
            return await Task.FromResult(new List<PaisModel>());
        }

        public async Task<List<EstadoModel>> GetEstadosLegado()
        {
            return await Task.FromResult(new List<EstadoModel>());
        }

        public async Task<List<CidadeModel>> GetCidade()
        {
            return await Task.FromResult(new List<CidadeModel>());
        }

        public Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade(CidadeSearchModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado()
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado(ParametroSistemaViewModel parametroSistema)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DesativarUsuariosSemCotaOuContrato()
        {
            throw new NotImplementedException();
        }

        public Task GetOutrosDadosUsuario(TokenResultModel userReturn)
        {
            throw new NotImplementedException();
        }

        public Task<List<StatusCrcContratoModel>?> GetStatusCrc(string? contratoId = "", string? pessoa1Id = "", string? pessoa2Id = "", string? cotaId = "")
        {
            throw new NotImplementedException();
        }


        public Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(List<string> empresasIds)
        {
            throw new NotImplementedException();
        }

        public Task<UsuarioValidateResultModel> GerUserFromLegado(UserRegisterInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInputModel>> GetUsuariosCotasCanceladasSistemaLegado()
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado()
        {
            throw new NotImplementedException();
        }

        public Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar)
        {
            throw new NotImplementedException();
        }

        public Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException();
        }

        public bool? ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            throw new NotImplementedException();
        }
    }
}
