using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface ICommunicationProvider
    {
        Task<IAccessValidateResultModel> ValidateAccess(string login, string senha, string pessoaProviderId = "", string providerName = "esolution");
        Task<UsuarioValidateResultModel> GerUserFromLegado(UserRegisterInputModel model, string providerName = "esolution");
        Task<bool> GravarUsuarioNoLegado(string pessoaProviderId, string login, string senha, string providerName = "esolution");
        Task<bool> AlterarSenhaNoLegado(string pessoaProviderId, string login, string senha, string providerName = "esolution");
        Task<bool> IsDefault();
        string CommunicationProviderName { get; }
        string PrefixoTransacaoFinanceira { get; }
        Task GravarVinculoUsuario(IAccessValidateResultModel result, Usuario usuario, string providerName = "esolution");
        Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider(string pessoaProviderId, string providerName = "esolution");
        Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado(int id, string providerName = "esolution");
        Task<List<PaisModel>> GetPaisesLegado(string providerName = "esolution");
        Task<List<EstadoModel>> GetEstadosLegado(string providerName = "esolution");
        Task<List<CidadeModel>> GetCidade(string providerName = "esolution");
        Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado(string providerName = "esolution");
        Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado(ParametroSistemaViewModel parametroSistema, string providerName = "esolution");
        Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade(CidadeSearchModel searchModel, string providerName = "esolution");
        Task<bool> DesativarUsuariosSemCotaOuContrato(string providerName = "esolution");
        Task GetOutrosDadosUsuario(TokenResultModel userReturn, string providerName = "esolution");
        Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar, string providerName = "esolution");
        Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(List<string> empresasIds, string providerName = "esolution");
        Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado(string providerName = "esolution");
        Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null, string providerName = "esolution");
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false, string providerName = "esolution");
        Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync(DateTime checkInDate, bool simulacao = false, string providerName = "esolution");
        bool? ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes, string providerName = "esolution");
    }
}
