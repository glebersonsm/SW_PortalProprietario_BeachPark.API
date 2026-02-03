using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using EsolutionPortalDomain.Portal;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Providers.Cm;
using SW_PortalProprietario.Application.Services.Providers.Esolution;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Services.Providers.Hybrid
{
    public class HybridProviderService : IHybridProviderService
    {
        private readonly ICommunicationCmProvider _cmCommunicationProvider;
        private readonly ICommunicationEsolutionProvider _esolCommunicationProvider;
        private readonly EmpreendimentoCmService _cmEmpreendimentoService;
        private readonly EmpreendimentoEsolutionService _esolEmpreendimentoService;
        private readonly TimeSharingCmService _cmTimeSharingService;
        private readonly TimeSharingEsolutionService _esolTimeSharingService;
        private readonly ILogger<HybridProviderService> _logger;

        public HybridProviderService(
            ICommunicationCmProvider cmCommunicationProvider,
            ICommunicationEsolutionProvider esolCommunicationProvider,
            EmpreendimentoCmService cmEmpreendimentoService,
            EmpreendimentoEsolutionService esolEmpreendimentoService,
            TimeSharingCmService cmTimeSharingService,
            TimeSharingEsolutionService esolTimeSharingService,
            ILogger<HybridProviderService> logger)
        {
            _cmCommunicationProvider = cmCommunicationProvider ?? throw new ArgumentNullException(nameof(cmCommunicationProvider));
            _esolCommunicationProvider = esolCommunicationProvider ?? throw new ArgumentNullException(nameof(esolCommunicationProvider));
            _cmEmpreendimentoService = cmEmpreendimentoService ?? throw new ArgumentNullException(nameof(cmEmpreendimentoService));
            _esolEmpreendimentoService = esolEmpreendimentoService ?? throw new ArgumentNullException(nameof(esolEmpreendimentoService));
            _cmTimeSharingService = cmTimeSharingService ?? throw new ArgumentNullException(nameof(cmTimeSharingService));
            _esolTimeSharingService = esolTimeSharingService ?? throw new ArgumentNullException(nameof(esolTimeSharingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Communication Methods - CM
        public Task<IAccessValidateResultModel> ValidateAccess_CM(string login, string senha, string pessoaProviderId = "")
            => _cmCommunicationProvider.ValidateAccess(login, senha, pessoaProviderId);

        public Task<UsuarioValidateResultModel> GerUserFromLegado_CM(UserRegisterInputModel model)
            => _cmCommunicationProvider.GerUserFromLegado(model);

        public Task<bool> GravarUsuarioNoLegado_CM(string pessoaProviderId, string login, string senha)
            => _cmCommunicationProvider.GravarUsuarioNoLegado(pessoaProviderId, login, senha);

        public Task<bool> AlterarSenhaNoLegado_CM(string pessoaProviderId, string login, string senha)
            => _cmCommunicationProvider.AlterarSenhaNoLegado(pessoaProviderId, login, senha);

        public Task<bool> IsDefault_CM()
            => _cmCommunicationProvider.IsDefault();

        public string CommunicationProviderName_CM => _cmCommunicationProvider.CommunicationProviderName;
        public string PrefixoTransacaoFinanceira_CM => _cmCommunicationProvider.PrefixoTransacaoFinanceira;

        public Task GravarVinculoUsuario_CM(IAccessValidateResultModel result, Usuario usuario)
            => _cmCommunicationProvider.GravarVinculoUsuario(result, usuario);

        public Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_CM(string pessoaProviderId)
            => _cmCommunicationProvider.GetOutrosDadosPessoaProvider(pessoaProviderId);

        public Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_CM(int id)
            => _cmCommunicationProvider.GetEmpresaVinculadaLegado(id);

        public Task<List<PaisModel>> GetPaisesLegado_CM()
            => _cmCommunicationProvider.GetPaisesLegado();

        public Task<List<EstadoModel>> GetEstadosLegado_CM()
            => _cmCommunicationProvider.GetEstadosLegado();

        public Task<List<CidadeModel>> GetCidade_CM()
            => _cmCommunicationProvider.GetCidade();

        public Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_CM()
            => _cmCommunicationProvider.GetUsuariosAtivosSistemaLegado();

        public Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_CM(ParametroSistemaViewModel parametroSistema)
            => _cmCommunicationProvider.GetClientesUsuariosLegado(parametroSistema);

        public Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_CM(CidadeSearchModel searchModel)
            => _cmCommunicationProvider.SearchCidade(searchModel);

        public Task<bool> DesativarUsuariosSemCotaOuContrato_CM()
            => _cmCommunicationProvider.DesativarUsuariosSemCotaOuContrato();

        public Task GetOutrosDadosUsuario_CM(TokenResultModel userReturn)
            => _cmCommunicationProvider.GetOutrosDadosUsuario(userReturn);

        public Task<List<DadosContratoModel>?> GetContratos_CM(List<int> pessoasPesquisar)
            => _cmCommunicationProvider.GetContratos(pessoasPesquisar);

        public Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_CM(List<string> empresasIds)
            => _cmCommunicationProvider.GetEmpresasVinculadas(empresasIds);

        public Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_CM()
            => _cmCommunicationProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado();

        public Task<List<ClientesInadimplentes>> Inadimplentes_CM(List<int>? pessoasPesquisar = null)
            => _cmCommunicationProvider.Inadimplentes(pessoasPesquisar);

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_CM(DateTime checkInDate, bool simulacao = false)
            => _cmCommunicationProvider.GetReservasWithCheckInDateMultiPropriedadeAsync(checkInDate, simulacao);

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_CM(DateTime checkInDate, bool simulacao = false)
            => _cmCommunicationProvider.GetReservasWithCheckInDateTimeSharingAsync(checkInDate, simulacao);

        public bool? ShouldSendEmailForReserva_CM(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
            => _cmCommunicationProvider.ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes);
        #endregion

        #region Communication Methods - Esol
        public Task<IAccessValidateResultModel> ValidateAccess_Esol(string login, string senha, string pessoaProviderId = "")
            => _esolCommunicationProvider.ValidateAccess(login, senha, pessoaProviderId);

        public Task<UsuarioValidateResultModel> GerUserFromLegado_Esol(UserRegisterInputModel model)
            => _esolCommunicationProvider.GerUserFromLegado(model);

        public Task<bool> GravarUsuarioNoLegado_Esol(string pessoaProviderId, string login, string senha)
            => _esolCommunicationProvider.GravarUsuarioNoLegado(pessoaProviderId, login, senha);

        public Task<bool> AlterarSenhaNoLegado_Esol(string pessoaProviderId, string login, string senha)
            => _esolCommunicationProvider.AlterarSenhaNoLegado(pessoaProviderId, login, senha);

        public Task<bool> IsDefault_Esol()
            => _esolCommunicationProvider.IsDefault();

        public string CommunicationProviderName_Esol => _esolCommunicationProvider.CommunicationProviderName;
        public string PrefixoTransacaoFinanceira_Esol => _esolCommunicationProvider.PrefixoTransacaoFinanceira;

        public Task GravarVinculoUsuario_Esol(IAccessValidateResultModel result, Usuario usuario)
            => _esolCommunicationProvider.GravarVinculoUsuario(result, usuario);

        public Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_Esol(string pessoaProviderId)
            => _esolCommunicationProvider.GetOutrosDadosPessoaProvider(pessoaProviderId);

        public Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_Esol(int id)
            => _esolCommunicationProvider.GetEmpresaVinculadaLegado(id);

        public Task<List<PaisModel>> GetPaisesLegado_Esol()
            => _esolCommunicationProvider.GetPaisesLegado();

        public Task<List<EstadoModel>> GetEstadosLegado_Esol()
            => _esolCommunicationProvider.GetEstadosLegado();

        public Task<List<CidadeModel>> GetCidade_Esol()
            => _esolCommunicationProvider.GetCidade();

        public Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_Esol()
            => _esolCommunicationProvider.GetUsuariosAtivosSistemaLegado();

        public Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_Esol(ParametroSistemaViewModel parametroSistema)
            => _esolCommunicationProvider.GetClientesUsuariosLegado(parametroSistema);

        public Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_Esol(CidadeSearchModel searchModel)
            => _esolCommunicationProvider.SearchCidade(searchModel);

        public Task<bool> DesativarUsuariosSemCotaOuContrato_Esol()
            => _esolCommunicationProvider.DesativarUsuariosSemCotaOuContrato();

        public Task GetOutrosDadosUsuario_Esol(TokenResultModel userReturn)
            => _esolCommunicationProvider.GetOutrosDadosUsuario(userReturn);

        public Task<List<DadosContratoModel>?> GetContratos_Esol(List<int> pessoasPesquisar)
            => _esolCommunicationProvider.GetContratos(pessoasPesquisar);

        public Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_Esol(List<string> empresasIds)
            => _esolCommunicationProvider.GetEmpresasVinculadas(empresasIds);

        public Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol()
            => _esolCommunicationProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado();

        public Task<List<ClientesInadimplentes>> Inadimplentes_Esol(List<int>? pessoasPesquisar = null)
            => _esolCommunicationProvider.Inadimplentes(pessoasPesquisar);

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(DateTime checkInDate, bool simulacao = false)
            => _esolCommunicationProvider.GetReservasWithCheckInDateMultiPropriedadeAsync(checkInDate, simulacao);

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_Esol(DateTime checkInDate, bool simulacao = false)
            => _esolCommunicationProvider.GetReservasWithCheckInDateTimeSharingAsync(checkInDate, simulacao);

        public bool? ShouldSendEmailForReserva_Esol(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
            => _esolCommunicationProvider.ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes);
        #endregion

        #region Empreendimento Methods - CM
        public Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_CM(SearchImovelModel searchModel)
            => _cmEmpreendimentoService.GetImoveis(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_CM(SearchProprietarioModel searchModel)
            => _cmEmpreendimentoService.GetProprietarios(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_CM(SearchMyContractsModel searchModel)
            => _cmEmpreendimentoService.GetMyContracts(searchModel);

        public Task<bool> GerarCodigoVerificacaoLiberacaoPool_CM(int agendamentoId)
            => _cmEmpreendimentoService.GerarCodigoVerificacaoLiberacaoPool(agendamentoId);

        public Task<bool> ValidarCodigo_CM(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true)
            => _cmEmpreendimentoService.ValidarCodigo(agendamentoId, codigoVerificacao, controlarTransacao);

        public Task<ResultModel<int>?> SalvarReservaEmAgendamento_CM(CriacaoReservaAgendamentoInputModel modelReserva)
            => _cmEmpreendimentoService.SalvarReservaEmAgendamento(modelReserva);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais_CM(ReservasMultiPropriedadeSearchModel model)
            => _cmEmpreendimentoService.ConsultarAgendamentosGerais(model);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos_CM(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
            => _cmEmpreendimentoService.ConsultarMeusAgendamentos(model);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId_CM(string agendamento)
            => _cmEmpreendimentoService.ConsultarReservaByAgendamentoId(agendamento);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId_CM(string agendamento)
            => _cmEmpreendimentoService.ConsultarMinhasReservaByAgendamentoId(agendamento);

        public Task<ResultModel<bool>?> CancelarReservaAgendamento_CM(CancelamentoReservaAgendamentoModel model)
            => _cmEmpreendimentoService.CancelarReservaAgendamento(model);

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento_CM(CancelamentoReservaAgendamentoModel model)
            => _cmEmpreendimentoService.CancelarMinhaReservaAgendamento(model);

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva_CM(int id)
            => _cmEmpreendimentoService.EditarMinhaReserva(id);

        public Task<ResultModel<ReservaForEditModel>?> EditarReserva_CM(int id)
            => _cmEmpreendimentoService.EditarReserva(id);

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios_CM(InventarioSearchModel searchModel)
            => _cmEmpreendimentoService.ConsultarInventarios(searchModel);

        public Task<ResultModel<bool>?> RetirarSemanaPool_CM(AgendamentoInventarioModel modelAgendamentoPool)
            => _cmEmpreendimentoService.RetirarSemanaPool(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarSemanaPool_CM(LiberacaoAgendamentoInputModel modelAgendamentoPool)
            => _cmEmpreendimentoService.LiberarSemanaPool(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarMinhaSemanaPool_CM(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
            => _cmEmpreendimentoService.LiberarMinhaSemanaPool(modelAgendamentoPool);

        public Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_CM(int agendamentoId)
            => _cmEmpreendimentoService.ConsultarHistoricos(agendamentoId);

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_CM(DispobilidadeSearchModel searchModel)
            => _cmEmpreendimentoService.ConsultarDisponibilidadeCompativel(searchModel);

        public Task<ResultModel<int>?> TrocarSemana_CM(TrocaSemanaInputModel model)
            => _cmEmpreendimentoService.TrocarSemana(model);

        public Task<ResultModel<int>?> TrocarTipoUso_CM(TrocaSemanaInputModel model)
            => _cmEmpreendimentoService.TrocarTipoUso(model);

        public Task<ResultModel<int>?> IncluirSemana_CM(IncluirSemanaInputModel model)
            => _cmEmpreendimentoService.IncluirSemana(model);

        public Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_CM(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false)
            => _cmEmpreendimentoService.GetKeyValueListFromContratoSCP(model, codigoVerificacao, dataAssinatura, espanhol);

        public Task<DownloadContratoResultModel?> DownloadContratoSCP_CM(int cotaId)
            => _cmEmpreendimentoService.DownloadContratoSCP(cotaId);

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_CM(string agendamentoId)
            => _cmEmpreendimentoService.GetDadosImpressaoVoucher(agendamentoId);

        public Task<List<StatusCrcModel>?> ConsultarStatusCrc_CM()
            => _cmEmpreendimentoService.ConsultarStatusCrc();

        public Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_CM(int ano, int? uhCondominioId = null, int? cotaPortalId = null)
            => _cmEmpreendimentoService.GetPosicaoAgendamentoAnoAsync(ano, uhCondominioId, cotaPortalId);

        public DadosContratoModel? GetContrato_CM(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
            => _cmEmpreendimentoService.GetContrato(dadosReserva, contratos);
        #endregion

        #region Empreendimento Methods - Esol
        public Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_Esol(SearchImovelModel searchModel)
            => _esolEmpreendimentoService.GetImoveis(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_Esol(SearchProprietarioModel searchModel)
            => _esolEmpreendimentoService.GetProprietarios(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_Esol(SearchMyContractsModel searchModel)
            => _esolEmpreendimentoService.GetMyContracts(searchModel);

        public Task<bool> GerarCodigoVerificacaoLiberacaoPool_Esol(int agendamentoId)
            => _esolEmpreendimentoService.GerarCodigoVerificacaoLiberacaoPool(agendamentoId);

        public Task<bool> ValidarCodigo_Esol(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true)
            => _esolEmpreendimentoService.ValidarCodigo(agendamentoId, codigoVerificacao, controlarTransacao);

        public Task<ResultModel<int>?> SalvarReservaEmAgendamento_Esol(CriacaoReservaAgendamentoInputModel modelReserva)
            => _esolEmpreendimentoService.SalvarReservaEmAgendamento(modelReserva);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais_Esol(ReservasMultiPropriedadeSearchModel model)
            => _esolEmpreendimentoService.ConsultarAgendamentosGerais(model);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos_Esol(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
            => _esolEmpreendimentoService.ConsultarMeusAgendamentos(model);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId_Esol(string agendamento)
            => _esolEmpreendimentoService.ConsultarReservaByAgendamentoId(agendamento);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId_Esol(string agendamento)
            => _esolEmpreendimentoService.ConsultarMinhasReservaByAgendamentoId(agendamento);

        public Task<ResultModel<bool>?> CancelarReservaAgendamento_Esol(CancelamentoReservaAgendamentoModel model)
            => _esolEmpreendimentoService.CancelarReservaAgendamento(model);

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento_Esol(CancelamentoReservaAgendamentoModel model)
            => _esolEmpreendimentoService.CancelarMinhaReservaAgendamento(model);

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva_Esol(int id)
            => _esolEmpreendimentoService.EditarMinhaReserva(id);

        public Task<ResultModel<ReservaForEditModel>?> EditarReserva_Esol(int id)
            => _esolEmpreendimentoService.EditarReserva(id);

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios_Esol(InventarioSearchModel searchModel)
            => _esolEmpreendimentoService.ConsultarInventarios(searchModel);

        public Task<ResultModel<bool>?> RetirarSemanaPool_Esol(AgendamentoInventarioModel modelAgendamentoPool)
            => _esolEmpreendimentoService.RetirarSemanaPool(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarSemanaPool_Esol(LiberacaoAgendamentoInputModel modelAgendamentoPool)
            => _esolEmpreendimentoService.LiberarSemanaPool(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarMinhaSemanaPool_Esol(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
            => _esolEmpreendimentoService.LiberarMinhaSemanaPool(modelAgendamentoPool);

        public Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_Esol(int agendamentoId)
            => _esolEmpreendimentoService.ConsultarHistoricos(agendamentoId);

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_Esol(DispobilidadeSearchModel searchModel)
            => _esolEmpreendimentoService.ConsultarDisponibilidadeCompativel(searchModel);

        public Task<ResultModel<int>?> TrocarSemana_Esol(TrocaSemanaInputModel model)
            => _esolEmpreendimentoService.TrocarSemana(model);

        public Task<ResultModel<int>?> TrocarTipoUso_Esol(TrocaSemanaInputModel model)
            => _esolEmpreendimentoService.TrocarTipoUso(model);

        public Task<ResultModel<int>?> IncluirSemana_Esol(IncluirSemanaInputModel model)
            => _esolEmpreendimentoService.IncluirSemana(model);

        public Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_Esol(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false)
            => _esolEmpreendimentoService.GetKeyValueListFromContratoSCP(model, codigoVerificacao, dataAssinatura, espanhol);

        public Task<DownloadContratoResultModel?> DownloadContratoSCP_Esol(int cotaId)
            => _esolEmpreendimentoService.DownloadContratoSCP(cotaId);

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_Esol(string agendamentoId)
            => _esolEmpreendimentoService.GetDadosImpressaoVoucher(agendamentoId);

        public Task<List<StatusCrcModel>?> ConsultarStatusCrc_Esol()
            => _esolEmpreendimentoService.ConsultarStatusCrc();

        public Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_Esol(int ano, int? uhCondominioId = null, int? cotaPortalId = null)
            => _esolEmpreendimentoService.GetPosicaoAgendamentoAnoAsync(ano, uhCondominioId, cotaPortalId);

        public DadosContratoModel? GetContrato_Esol(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
            => _esolEmpreendimentoService.GetContrato(dadosReserva, contratos);
        #endregion

        #region TimeSharing Methods - CM
        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucherTimeSharing_CM(long numReserva)
            => _cmTimeSharingService.GetDadosImpressaoVoucher(numReserva);

        public DadosContratoModel? GetContratoTimeSharing_CM(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
            => _cmTimeSharingService.GetContrato(dadosReserva, contratos);

        public Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing_CM(SearchContratosTimeSharingModel searchModel)
            => _cmTimeSharingService.GetContratosTimeSharing(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos_CM(SearchReservaTsModel searchModel)
            => _cmTimeSharingService.GetReservasGeralComConsumoPontos(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral_CM(SearchReservasGeralModel searchModel)
            => _cmTimeSharingService.GetReservasGeral(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci_CM(SearchReservasRciModel searchModel)
            => _cmTimeSharingService.GetReservasRci(searchModel);

        public Task<List<HotelModel>> HoteisVinculados_CM()
            => _cmTimeSharingService.HoteisVinculados();

        public Task<bool> VincularReservaRCI_CM(VincularReservaRciModel vincularModel)
            => _cmTimeSharingService.VincularReservaRCI(vincularModel);

        public Task<CalcularPontosResponseModel> CalcularPontosNecessarios_CM(CalcularPontosRequestModel request)
            => _cmTimeSharingService.CalcularPontosNecessarios(request);
        #endregion

        #region TimeSharing Methods - Esol
        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucherTimeSharing_Esol(long numReserva)
            => _esolTimeSharingService.GetDadosImpressaoVoucher(numReserva);

        public DadosContratoModel? GetContratoTimeSharing_Esol(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
            => _esolTimeSharingService.GetContrato(dadosReserva, contratos);

        public Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing_Esol(SearchContratosTimeSharingModel searchModel)
            => _esolTimeSharingService.GetContratosTimeSharing(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos_Esol(SearchReservaTsModel searchModel)
            => _esolTimeSharingService.GetReservasGeralComConsumoPontos(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral_Esol(SearchReservasGeralModel searchModel)
            => _esolTimeSharingService.GetReservasGeral(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci_Esol(SearchReservasRciModel searchModel)
            => _esolTimeSharingService.GetReservasRci(searchModel);

        public Task<List<HotelModel>> HoteisVinculados_Esol()
            => _esolTimeSharingService.HoteisVinculados();

        public Task<bool> VincularReservaRCI_Esol(VincularReservaRciModel vincularModel)
            => _esolTimeSharingService.VincularReservaRCI(vincularModel);

        public Task<CalcularPontosResponseModel> CalcularPontosNecessarios_Esol(CalcularPontosRequestModel request)
            => _esolTimeSharingService.CalcularPontosNecessarios(request);
        #endregion
    }
}
