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
        Task<IAccessValidateResultModel> ValidateAccess(string login, string senha, string pessoaProviderId = "");
        Task<UsuarioValidateResultModel> GerUserFromLegado(UserRegisterInputModel model);
        Task<bool> GravarUsuarioNoLegado(string pessoaProviderId, string login, string senha);
        Task<bool> AlterarSenhaNoLegado(string pessoaProviderId, string login, string senha);
        Task<bool> IsDefault();
        string CommunicationProviderName { get; }
        string PrefixoTransacaoFinanceira { get; }
        Task GravarVinculoUsuario(IAccessValidateResultModel result, Usuario usuario);
        Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider(string pessoaProviderId);
        Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado(int id);
        Task<List<PaisModel>> GetPaisesLegado();
        Task<List<EstadoModel>> GetEstadosLegado();
        Task<List<CidadeModel>> GetCidade();
        Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado();
        Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado(ParametroSistemaViewModel parametroSistema);
        Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade(CidadeSearchModel searchModel);
        Task<bool> DesativarUsuariosSemCotaOuContrato();
        Task GetOutrosDadosUsuario(TokenResultModel userReturn);
        Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar);
        Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(List<string> empresasIds);
        Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado();
        Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync(DateTime checkInDate, bool simulacao = false);
        bool? ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes);
    }
}
