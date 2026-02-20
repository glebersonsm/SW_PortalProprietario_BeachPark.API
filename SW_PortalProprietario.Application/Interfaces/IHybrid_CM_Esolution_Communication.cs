using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras.Boleto;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IHybrid_CM_Esolution_Communication : ICommunicationProvider
    {
        // MÃ©todos CM com sufixo _Cm
        Task<IAccessValidateResultModel> ValidateAccess_Cm(string login, string senha, string pessoaProviderId = "");
        Task GravarVinculoUsuario_Cm(IAccessValidateResultModel result, Usuario usuario);
        Task<bool> IsDefault_Cm();
        Task<bool> GravarUsuarioNoLegado_Cm(string pessoaProviderId, string login, string senha);
        Task<bool> AlterarSenhaNoLegado_Cm(string pessoaProviderId, string login, string senha);
        Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_Cm(string pessoaProviderId);
        Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_Cm(int id);
        Task<List<PaisModel>> GetPaisesLegado_Cm();
        Task<List<EstadoModel>> GetEstadosLegado_Cm();
        Task<List<CidadeModel>> GetCidade_Cm();
        Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_Cm(CidadeSearchModel searchModel);
        Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_Cm();
        Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_Cm(ParametroSistemaViewModel parametroSistema);
        Task<bool> DesativarUsuariosSemCotaOuContrato_Cm();
        Task GetOutrosDadosUsuario_Cm(TokenResultModel userReturn);
        Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_Cm(List<string> empresasIds);
        Task<UsuarioValidateResultModel> GerUserFromLegado_Cm(UserRegisterInputModel model);
        Task<List<UserRegisterInputModel>> GetUsuariosCotasCanceladasSistemaLegado_Cm();
        Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Cm();
        Task<List<DadosContratoModel>?> GetContratos_Cm(List<int> pessoasPesquisar);
        Task<List<StatusCrcContratoModel>?> GetStatusCrc_Cm(List<int> frAtendimentoVendaIds);
        Task<List<ClientesInadimplentes>> Inadimplentes_Cm(List<int>? pessoasPesquisar = null);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Cm(DateTime checkInDate, bool simulacao = false);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_Cm(DateTime checkInDate, bool simulacao = false);
        Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing_Cm(SearchContratosTimeSharingModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetMeusContratosTimeSharing_Cm(SearchMeusContratosTimeSharingModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos_Cm(SearchReservaTsModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral_Cm(SearchReservasGeralModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci_Cm(SearchReservasRciModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetMinhasReservasGeralComConsumoPontos_Cm(SearchMinhasReservaTsModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral_Cm(SearchMinhasReservasGeralModel searchModel);
        bool? ShouldSendEmailForReserva_Cm(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes);


        // MÃ©todos Esolution com sufixo _Esol
        Task<IAccessValidateResultModel> ValidateAccess_Esol(string login, string senha, string pessoaProviderId = "");
        Task GravarVinculoUsuario_Esol(IAccessValidateResultModel result, Usuario usuario);
        Task<bool> IsDefault_Esol();
        Task<bool> GravarUsuarioNoLegado_Esol(string pessoaProviderId, string login, string senha);
        Task<bool> AlterarSenhaNoLegado_Esol(string pessoaProviderId, string login, string senha);
        Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_Esol(string pessoaProviderId);
        Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_Esol(int id);
        Task<List<PaisModel>> GetPaisesLegado_Esol();
        Task<List<EstadoModel>> GetEstadosLegado_Esol();
        Task<List<CidadeModel>> GetCidade_Esol();
        Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_Esol(CidadeSearchModel searchModel);
        Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_Esol();
        Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_Esol(ParametroSistemaViewModel parametroSistema);
        Task<bool> DesativarUsuariosSemCotaOuContrato_Esol();
        Task GetOutrosDadosUsuario_Esol(TokenResultModel userReturn);
        Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_Esol(List<string> empresasIds);
        Task<UsuarioValidateResultModel> GerUserFromLegado_Esol(UserRegisterInputModel model);
        Task<List<UserRegisterInputModel>> GetUsuariosCotasCanceladasSistemaLegado_Esol();
        Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol();
        Task<List<DadosContratoModel>?> GetContratos_Esol(List<int> pessoasPesquisar);
        Task<List<StatusCrcContratoModel>?> GetStatusCrc_Esol(List<int> frAtendimentoVendaIds);
        Task<List<StatusCrcContratoModel>?> GetStatusCrcPorTipoStatusIds_Esol(List<int> statusCrcIds);
        Task<List<ClientesInadimplentes>> Inadimplentes_Esol(List<int>? pessoasPesquisar = null);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(DateTime checkInDate, bool simulacao = false);
        Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_Esol(DateTime checkInDate, bool simulacao = false);
        Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing_Esol(SearchContratosTimeSharingModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetMeusContratosTimeSharing_Esol(SearchMeusContratosTimeSharingModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos_Esol(SearchReservaTsModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral_Esol(SearchReservasGeralModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci_Esol(SearchReservasRciModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetMinhasReservasGeralComConsumoPontos_Esol(SearchMinhasReservaTsModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral_Esol(SearchMinhasReservasGeralModel searchModel);
        bool? ShouldSendEmailForReserva_Esol(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes);

    }
}
