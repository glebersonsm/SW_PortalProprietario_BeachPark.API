using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IHybridEsol_CM_ServiceProvider
    {
        // Métodos do ICommunicationCmProvider com sufixo _CM
        #region CM_Region
        Task<IAccessValidateResultModel> ValidateAccess_CM(string login, string senha, string pessoaProviderId = "");
        Task<UsuarioValidateResultModel> GerUserFromLegado_CM(UserRegisterInputModel model);
        Task<bool> GravarUsuarioNoLegado_CM(string pessoaProviderId, string login, string senha);
        Task<bool> AlterarSenhaNoLegado_CM(string pessoaProviderId, string login, string senha);
        Task<bool> IsDefault_CM();
        string CommunicationProviderName_CM { get; }
        string PrefixoTransacaoFinanceira_CM { get; }
        Task GravarVinculoUsuario_CM(IAccessValidateResultModel result, Usuario usuario);
        Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_CM(string pessoaProviderId);
        Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_CM(int id);
        Task<List<PaisModel>> GetPaisesLegado_CM();
        Task<List<EstadoModel>> GetEstadosLegado_CM();
        Task<List<CidadeModel>> GetCidade_CM();
        Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_CM();
        Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_CM(ParametroSistemaViewModel parametroSistema);
        Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_CM(CidadeSearchModel searchModel);
        Task<bool> DesativarUsuariosSemCotaOuContrato_CM();
        Task GetOutrosDadosUsuario_CM(TokenResultModel userReturn);
        Task<List<DadosContratoModel>?> GetContratos_CM(List<int> pessoasPesquisar);
        Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_CM(List<string> empresasIds);
        Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_CM();
        Task<List<ClientesInadimplentes>> Inadimplentes_CM(List<int>? pessoasPesquisar = null);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_CM(DateTime checkInDate, bool simulacao = false);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_CM(DateTime checkInDate, bool simulacao = false);
        bool? ShouldSendEmailForReserva_CM(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes);
        #endregion

        // Métodos do ICommunicationEsolutionProvider com sufixo _Esol
        #region Esol_Region
        Task<IAccessValidateResultModel> ValidateAccess_Esol(string login, string senha, string pessoaProviderId = "");
        Task<UsuarioValidateResultModel> GerUserFromLegado_Esol(UserRegisterInputModel model);
        Task<bool> GravarUsuarioNoLegado_Esol(string pessoaProviderId, string login, string senha);
        Task<bool> AlterarSenhaNoLegado_Esol(string pessoaProviderId, string login, string senha);
        Task<bool> IsDefault_Esol();
        string CommunicationProviderName_Esol { get; }
        string PrefixoTransacaoFinanceira_Esol { get; }
        Task GravarVinculoUsuario_Esol(IAccessValidateResultModel result, Usuario usuario);
        Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_Esol(string pessoaProviderId);
        Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_Esol(int id);
        Task<List<PaisModel>> GetPaisesLegado_Esol();
        Task<List<EstadoModel>> GetEstadosLegado_Esol();
        Task<List<CidadeModel>> GetCidade_Esol();
        Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_Esol();
        Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_Esol(ParametroSistemaViewModel parametroSistema);
        Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_Esol(CidadeSearchModel searchModel);
        Task<bool> DesativarUsuariosSemCotaOuContrato_Esol();
        Task GetOutrosDadosUsuario_Esol(TokenResultModel userReturn);
        Task<List<DadosContratoModel>?> GetContratos_Esol(List<int> pessoasPesquisar);
        Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_Esol(List<string> empresasIds);
        Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol();
        Task<List<ClientesInadimplentes>> Inadimplentes_Esol(List<int>? pessoasPesquisar = null);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(DateTime checkInDate, bool simulacao = false);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_Esol(DateTime checkInDate, bool simulacao = false);
        bool? ShouldSendEmailForReserva_Esol(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes); 
        #endregion
    }
}
