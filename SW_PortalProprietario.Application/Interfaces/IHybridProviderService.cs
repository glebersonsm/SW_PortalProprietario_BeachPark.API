using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Interfaces
{
    /// <summary>
    /// Interface unificada para todos os providers (Communication, Empreendimento e TimeSharing)
    /// Todos os métodos possuem sufixo _CM ou _Esol para identificar o provider de origem
    /// </summary>
    public interface IHybridProviderService
    {
        #region Communication Methods - CM
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

        #region Communication Methods - Esol
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

        #region Empreendimento Methods - CM
        Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_CM(SearchImovelModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_CM(SearchProprietarioModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_CM(SearchMyContractsModel searchModel);
        Task<bool> GerarCodigoVerificacaoLiberacaoPool_CM(int agendamentoId);
        Task<bool> ValidarCodigo_CM(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true);
        Task<ResultModel<int>?> SalvarReservaEmAgendamento_CM(CriacaoReservaAgendamentoInputModel modelReserva);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais_CM(ReservasMultiPropriedadeSearchModel model);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos_CM(PeriodoCotaDisponibilidadeUsuarioSearchModel model);
        Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId_CM(string agendamento);
        Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId_CM(string agendamento);
        Task<ResultModel<bool>?> CancelarReservaAgendamento_CM(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento_CM(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva_CM(int id);
        Task<ResultModel<ReservaForEditModel>?> EditarReserva_CM(int id);
        Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios_CM(InventarioSearchModel searchModel);
        Task<ResultModel<bool>?> RetirarSemanaPool_CM(AgendamentoInventarioModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarSemanaPool_CM(LiberacaoAgendamentoInputModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarMinhaSemanaPool_CM(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool);
        Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_CM(int agendamentoId);
        Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_CM(DispobilidadeSearchModel searchModel);
        Task<ResultModel<int>?> TrocarSemana_CM(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> TrocarTipoUso_CM(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> IncluirSemana_CM(IncluirSemanaInputModel model);
        Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_CM(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false);
        Task<DownloadContratoResultModel?> DownloadContratoSCP_CM(int cotaId);
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_CM(string agendamentoId);
        Task<List<StatusCrcModel>?> ConsultarStatusCrc_CM();
        Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_CM(int ano, int? uhCondominioId = null, int? cotaPortalId = null);
        DadosContratoModel? GetContrato_CM(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
        #endregion

        #region Empreendimento Methods - Esol
        Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_Esol(SearchImovelModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_Esol(SearchProprietarioModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_Esol(SearchMyContractsModel searchModel);
        Task<bool> GerarCodigoVerificacaoLiberacaoPool_Esol(int agendamentoId);
        Task<bool> ValidarCodigo_Esol(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true);
        Task<ResultModel<int>?> SalvarReservaEmAgendamento_Esol(CriacaoReservaAgendamentoInputModel modelReserva);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais_Esol(ReservasMultiPropriedadeSearchModel model);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos_Esol(PeriodoCotaDisponibilidadeUsuarioSearchModel model);
        Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId_Esol(string agendamento);
        Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId_Esol(string agendamento);
        Task<ResultModel<bool>?> CancelarReservaAgendamento_Esol(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento_Esol(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva_Esol(int id);
        Task<ResultModel<ReservaForEditModel>?> EditarReserva_Esol(int id);
        Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios_Esol(InventarioSearchModel searchModel);
        Task<ResultModel<bool>?> RetirarSemanaPool_Esol(AgendamentoInventarioModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarSemanaPool_Esol(LiberacaoAgendamentoInputModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarMinhaSemanaPool_Esol(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool);
        Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_Esol(int agendamentoId);
        Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_Esol(DispobilidadeSearchModel searchModel);
        Task<ResultModel<int>?> TrocarSemana_Esol(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> TrocarTipoUso_Esol(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> IncluirSemana_Esol(IncluirSemanaInputModel model);
        Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_Esol(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false);
        Task<DownloadContratoResultModel?> DownloadContratoSCP_Esol(int cotaId);
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_Esol(string agendamentoId);
        Task<List<StatusCrcModel>?> ConsultarStatusCrc_Esol();
        Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_Esol(int ano, int? uhCondominioId = null, int? cotaPortalId = null);
        DadosContratoModel? GetContrato_Esol(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
        #endregion

        #region TimeSharing Methods - CM
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucherTimeSharing_CM(long numReserva);
        DadosContratoModel? GetContratoTimeSharing_CM(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
        Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing_CM(SearchContratosTimeSharingModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos_CM(SearchReservaTsModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral_CM(SearchReservasGeralModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci_CM(SearchReservasRciModel searchModel);
        Task<List<HotelModel>> HoteisVinculados_CM();
        Task<bool> VincularReservaRCI_CM(VincularReservaRciModel vincularModel);
        Task<CalcularPontosResponseModel> CalcularPontosNecessarios_CM(CalcularPontosRequestModel request);
        #endregion

        #region TimeSharing Methods - Esol
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucherTimeSharing_Esol(long numReserva);
        DadosContratoModel? GetContratoTimeSharing_Esol(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
        Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing_Esol(SearchContratosTimeSharingModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos_Esol(SearchReservaTsModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral_Esol(SearchReservasGeralModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci_Esol(SearchReservasRciModel searchModel);
        Task<List<HotelModel>> HoteisVinculados_Esol();
        Task<bool> VincularReservaRCI_Esol(VincularReservaRciModel vincularModel);
        Task<CalcularPontosResponseModel> CalcularPontosNecessarios_Esol(CalcularPontosRequestModel request);
        #endregion
    }
}
