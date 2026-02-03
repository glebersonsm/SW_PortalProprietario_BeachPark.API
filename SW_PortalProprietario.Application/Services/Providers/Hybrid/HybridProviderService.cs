using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
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
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.Application.Services.Providers.Hybrid
{
    public class HybridProviderService : IHybridProviderService
    {
        private readonly IHybrid_CM_Esolution_Communication _hybridCommunication;
        private readonly IEmpreendimentoHybridProviderService _empreendimentoHybridService;
        private readonly TimeSharingCmService _cmTimeSharingService;
        private readonly TimeSharingEsolutionService _esolTimeSharingService;
        private readonly ILogger<HybridProviderService> _logger;

        public HybridProviderService(
            IHybrid_CM_Esolution_Communication hybridCommunication,
            IEmpreendimentoHybridProviderService empreendimentoHybridService,
            TimeSharingCmService cmTimeSharingService,
            TimeSharingEsolutionService esolTimeSharingService,
            ILogger<HybridProviderService> logger)
        {
            _hybridCommunication = hybridCommunication ?? throw new ArgumentNullException(nameof(hybridCommunication));
            _empreendimentoHybridService = empreendimentoHybridService ?? throw new ArgumentNullException(nameof(empreendimentoHybridService));
            _cmTimeSharingService = cmTimeSharingService ?? throw new ArgumentNullException(nameof(cmTimeSharingService));
            _esolTimeSharingService = esolTimeSharingService ?? throw new ArgumentNullException(nameof(esolTimeSharingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Communication Methods - CM
        public Task<IAccessValidateResultModel> ValidateAccess_CM(string login, string senha, string pessoaProviderId = "")
            => _hybridCommunication.ValidateAccess_Cm(login, senha, pessoaProviderId);

        public Task<UsuarioValidateResultModel> GerUserFromLegado_CM(UserRegisterInputModel model)
            => _hybridCommunication.GerUserFromLegado_Cm(model);

        public Task<bool> GravarUsuarioNoLegado_CM(string pessoaProviderId, string login, string senha)
            => _hybridCommunication.GravarUsuarioNoLegado_Cm(pessoaProviderId, login, senha);

        public Task<bool> AlterarSenhaNoLegado_CM(string pessoaProviderId, string login, string senha)
            => _hybridCommunication.AlterarSenhaNoLegado_Cm(pessoaProviderId, login, senha);

        public Task<bool> IsDefault_CM()
            => _hybridCommunication.IsDefault_Cm();

        public string CommunicationProviderName_CM => "CM"; // Hardcoded logic as ICommunicationProvider access is limited
        public string PrefixoTransacaoFinanceira_CM => "PORTALPROPCM_";

        public Task GravarVinculoUsuario_CM(IAccessValidateResultModel result, Usuario usuario)
            => _hybridCommunication.GravarVinculoUsuario_Cm(result, usuario);

        public Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_CM(string pessoaProviderId)
            => _hybridCommunication.GetOutrosDadosPessoaProvider_Cm(pessoaProviderId);

        public Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_CM(int id)
            => _hybridCommunication.GetEmpresaVinculadaLegado_Cm(id);

        public Task<List<PaisModel>> GetPaisesLegado_CM()
            => _hybridCommunication.GetPaisesLegado_Cm();

        public Task<List<EstadoModel>> GetEstadosLegado_CM()
            => _hybridCommunication.GetEstadosLegado_Cm();

        public Task<List<CidadeModel>> GetCidade_CM()
            => _hybridCommunication.GetCidade_Cm();

        public Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_CM()
            => _hybridCommunication.GetUsuariosAtivosSistemaLegado_Cm();

        public Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_CM(ParametroSistemaViewModel parametroSistema)
            => _hybridCommunication.GetClientesUsuariosLegado_Cm(parametroSistema);

        public Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_CM(CidadeSearchModel searchModel)
            => _hybridCommunication.SearchCidade_Cm(searchModel);

        public Task<bool> DesativarUsuariosSemCotaOuContrato_CM()
            => _hybridCommunication.DesativarUsuariosSemCotaOuContrato_Cm();

        public Task GetOutrosDadosUsuario_CM(TokenResultModel userReturn)
            => _hybridCommunication.GetOutrosDadosUsuario_Cm(userReturn);

        public Task<List<DadosContratoModel>?> GetContratos_CM(List<int> pessoasPesquisar)
            => _hybridCommunication.GetContratos_Cm(pessoasPesquisar);

        public Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_CM(List<string> empresasIds)
            => _hybridCommunication.GetEmpresasVinculadas_Cm(empresasIds);

        public Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_CM()
            => _hybridCommunication.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Cm();

        public Task<List<ClientesInadimplentes>> Inadimplentes_CM(List<int>? pessoasPesquisar = null)
            => _hybridCommunication.Inadimplentes_Cm(pessoasPesquisar);

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_CM(DateTime checkInDate, bool simulacao = false)
            => _hybridCommunication.GetReservasWithCheckInDateMultiPropriedadeAsync_Cm(checkInDate, simulacao);

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_CM(DateTime checkInDate, bool simulacao = false)
            => _hybridCommunication.GetReservasWithCheckInDateTimeSharingAsync_Cm(checkInDate, simulacao);

        public bool? ShouldSendEmailForReserva_CM(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
            => _hybridCommunication.ShouldSendEmailForReserva_Cm(reserva, config, contratos, inadimplentes);
        #endregion

        #region Communication Methods - Esol
        public Task<IAccessValidateResultModel> ValidateAccess_Esol(string login, string senha, string pessoaProviderId = "")
            => _hybridCommunication.ValidateAccess_Esol(login, senha, pessoaProviderId);

        public Task<UsuarioValidateResultModel> GerUserFromLegado_Esol(UserRegisterInputModel model)
            => _hybridCommunication.GerUserFromLegado_Esol(model);

        public Task<bool> GravarUsuarioNoLegado_Esol(string pessoaProviderId, string login, string senha)
            => _hybridCommunication.GravarUsuarioNoLegado_Esol(pessoaProviderId, login, senha);

        public Task<bool> AlterarSenhaNoLegado_Esol(string pessoaProviderId, string login, string senha)
            => _hybridCommunication.AlterarSenhaNoLegado_Esol(pessoaProviderId, login, senha);

        public Task<bool> IsDefault_Esol()
            => _hybridCommunication.IsDefault_Esol();

        public string CommunicationProviderName_Esol => "ESOLUTION";
        public string PrefixoTransacaoFinanceira_Esol => "PORTALPROPESOL_";

        public Task GravarVinculoUsuario_Esol(IAccessValidateResultModel result, Usuario usuario)
            => _hybridCommunication.GravarVinculoUsuario_Esol(result, usuario);

        public Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_Esol(string pessoaProviderId)
            => _hybridCommunication.GetOutrosDadosPessoaProvider_Esol(pessoaProviderId);

        public Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_Esol(int id)
            => _hybridCommunication.GetEmpresaVinculadaLegado_Esol(id);

        public Task<List<PaisModel>> GetPaisesLegado_Esol()
            => _hybridCommunication.GetPaisesLegado_Esol();

        public Task<List<EstadoModel>> GetEstadosLegado_Esol()
            => _hybridCommunication.GetEstadosLegado_Esol();

        public Task<List<CidadeModel>> GetCidade_Esol()
            => _hybridCommunication.GetCidade_Esol();

        public Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_Esol()
            => _hybridCommunication.GetUsuariosAtivosSistemaLegado_Esol();

        public Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_Esol(ParametroSistemaViewModel parametroSistema)
            => _hybridCommunication.GetClientesUsuariosLegado_Esol(parametroSistema);

        public Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_Esol(CidadeSearchModel searchModel)
            => _hybridCommunication.SearchCidade_Esol(searchModel);

        public Task<bool> DesativarUsuariosSemCotaOuContrato_Esol()
            => _hybridCommunication.DesativarUsuariosSemCotaOuContrato_Esol();

        public Task GetOutrosDadosUsuario_Esol(TokenResultModel userReturn)
            => _hybridCommunication.GetOutrosDadosUsuario_Esol(userReturn);

        public Task<List<DadosContratoModel>?> GetContratos_Esol(List<int> pessoasPesquisar)
            => _hybridCommunication.GetContratos_Esol(pessoasPesquisar);

        public Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_Esol(List<string> empresasIds)
            => _hybridCommunication.GetEmpresasVinculadas_Esol(empresasIds);

        public Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol()
            => _hybridCommunication.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol();

        public Task<List<ClientesInadimplentes>> Inadimplentes_Esol(List<int>? pessoasPesquisar = null)
            => _hybridCommunication.Inadimplentes_Esol(pessoasPesquisar);

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(DateTime checkInDate, bool simulacao = false)
            => _hybridCommunication.GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(checkInDate, simulacao);

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_Esol(DateTime checkInDate, bool simulacao = false)
            => _hybridCommunication.GetReservasWithCheckInDateTimeSharingAsync_Esol(checkInDate, simulacao);

        public bool? ShouldSendEmailForReserva_Esol(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
            => _hybridCommunication.ShouldSendEmailForReserva_Esol(reserva, config, contratos, inadimplentes);
        #endregion

        #region Empreendimento Methods - CM
        public Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_CM(SearchImovelModel searchModel)
            => _empreendimentoHybridService.GetImoveis_CM(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_CM(SearchProprietarioModel searchModel)
            => _empreendimentoHybridService.GetProprietarios_CM(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_CM(SearchMyContractsModel searchModel)
            => _empreendimentoHybridService.GetMyContracts_CM(searchModel);

        public Task<bool> GerarCodigoVerificacaoLiberacaoPool_CM(int agendamentoId)
            => _empreendimentoHybridService.GerarCodigoVerificacaoLiberacaoPool_CM(agendamentoId);

        public Task<bool> ValidarCodigo_CM(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true)
            => _empreendimentoHybridService.ValidarCodigo_CM(agendamentoId, codigoVerificacao, controlarTransacao);

        public Task<ResultModel<int>?> SalvarReservaEmAgendamento_CM(CriacaoReservaAgendamentoInputModel modelReserva)
            => _empreendimentoHybridService.SalvarReservaEmAgendamento_CM(modelReserva);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais_CM(ReservasMultiPropriedadeSearchModel model)
            => _empreendimentoHybridService.ConsultarAgendamentosGerais_CM(model);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos_CM(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
            => _empreendimentoHybridService.ConsultarMeusAgendamentos_CM(model);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId_CM(string agendamento)
            => _empreendimentoHybridService.ConsultarReservaByAgendamentoId_CM(agendamento);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId_CM(string agendamento)
            => _empreendimentoHybridService.ConsultarMinhasReservaByAgendamentoId_CM(agendamento);

        public Task<ResultModel<bool>?> CancelarReservaAgendamento_CM(CancelamentoReservaAgendamentoModel model)
            => _empreendimentoHybridService.CancelarReservaAgendamento_CM(model);

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento_CM(CancelamentoReservaAgendamentoModel model)
            => _empreendimentoHybridService.CancelarMinhaReservaAgendamento_CM(model);

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva_CM(int id)
            => _empreendimentoHybridService.EditarMinhaReserva_CM(id);

        public Task<ResultModel<ReservaForEditModel>?> EditarReserva_CM(int id)
            => _empreendimentoHybridService.EditarReserva_CM(id);

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios_CM(InventarioSearchModel searchModel)
            => _empreendimentoHybridService.ConsultarInventarios_CM(searchModel);

        public Task<ResultModel<bool>?> RetirarSemanaPool_CM(AgendamentoInventarioModel modelAgendamentoPool)
            => _empreendimentoHybridService.RetirarSemanaPool_CM(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarSemanaPool_CM(LiberacaoAgendamentoInputModel modelAgendamentoPool)
            => _empreendimentoHybridService.LiberarSemanaPool_CM(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarMinhaSemanaPool_CM(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
            => _empreendimentoHybridService.LiberarMinhaSemanaPool_CM(modelAgendamentoPool);

        public Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_CM(int agendamentoId)
            => _empreendimentoHybridService.ConsultarHistoricos_CM(agendamentoId);

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_CM(DispobilidadeSearchModel searchModel)
            => _empreendimentoHybridService.ConsultarDisponibilidadeCompativel_CM(searchModel);

        public Task<ResultModel<int>?> TrocarSemana_CM(TrocaSemanaInputModel model)
            => _empreendimentoHybridService.TrocarSemana_CM(model);

        public Task<ResultModel<int>?> TrocarTipoUso_CM(TrocaSemanaInputModel model)
            => _empreendimentoHybridService.TrocarTipoUso_CM(model);

        public Task<ResultModel<int>?> IncluirSemana_CM(IncluirSemanaInputModel model)
            => _empreendimentoHybridService.IncluirSemana_CM(model);

        public Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_CM(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false)
            => _empreendimentoHybridService.GetKeyValueListFromContratoSCP_CM(model, codigoVerificacao, dataAssinatura, espanhol);

        public Task<DownloadContratoResultModel?> DownloadContratoSCP_CM(int cotaId)
            => _empreendimentoHybridService.DownloadContratoSCP_CM(cotaId);

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_CM(string agendamentoId)
            => _empreendimentoHybridService.GetDadosImpressaoVoucher_CM(agendamentoId);

        public Task<List<StatusCrcModel>?> ConsultarStatusCrc_CM()
            => _empreendimentoHybridService.ConsultarStatusCrc_CM();

        public Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_CM(int ano, int? uhCondominioId = null, int? cotaPortalId = null)
            => _empreendimentoHybridService.GetPosicaoAgendamentoAnoAsync_CM(ano, uhCondominioId, cotaPortalId);

        public DadosContratoModel? GetContrato_CM(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
            => _empreendimentoHybridService.GetContrato_CM(dadosReserva, contratos);
        #endregion

        #region Empreendimento Methods - Esol
        public Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_Esol(SearchImovelModel searchModel)
            => _empreendimentoHybridService.GetImoveis_Esol(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_Esol(SearchProprietarioModel searchModel)
            => _empreendimentoHybridService.GetProprietarios_Esol(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_Esol(SearchMyContractsModel searchModel)
            => _empreendimentoHybridService.GetMyContracts_Esol(searchModel);

        public Task<bool> GerarCodigoVerificacaoLiberacaoPool_Esol(int agendamentoId)
            => _empreendimentoHybridService.GerarCodigoVerificacaoLiberacaoPool_Esol(agendamentoId);

        public Task<bool> ValidarCodigo_Esol(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true)
            => _empreendimentoHybridService.ValidarCodigo_Esol(agendamentoId, codigoVerificacao, controlarTransacao);

        public Task<ResultModel<int>?> SalvarReservaEmAgendamento_Esol(CriacaoReservaAgendamentoInputModel modelReserva)
            => _empreendimentoHybridService.SalvarReservaEmAgendamento_Esol(modelReserva);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais_Esol(ReservasMultiPropriedadeSearchModel model)
            => _empreendimentoHybridService.ConsultarAgendamentosGerais_Esol(model);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos_Esol(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
            => _empreendimentoHybridService.ConsultarMeusAgendamentos_Esol(model);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId_Esol(string agendamento)
            => _empreendimentoHybridService.ConsultarReservaByAgendamentoId_Esol(agendamento);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId_Esol(string agendamento)
            => _empreendimentoHybridService.ConsultarMinhasReservaByAgendamentoId_Esol(agendamento);

        public Task<ResultModel<bool>?> CancelarReservaAgendamento_Esol(CancelamentoReservaAgendamentoModel model)
            => _empreendimentoHybridService.CancelarReservaAgendamento_Esol(model);

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento_Esol(CancelamentoReservaAgendamentoModel model)
            => _empreendimentoHybridService.CancelarMinhaReservaAgendamento_Esol(model);

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva_Esol(int id)
            => _empreendimentoHybridService.EditarMinhaReserva_Esol(id);

        public Task<ResultModel<ReservaForEditModel>?> EditarReserva_Esol(int id)
            => _empreendimentoHybridService.EditarReserva_Esol(id);

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios_Esol(InventarioSearchModel searchModel)
            => _empreendimentoHybridService.ConsultarInventarios_Esol(searchModel);

        public Task<ResultModel<bool>?> RetirarSemanaPool_Esol(AgendamentoInventarioModel modelAgendamentoPool)
            => _empreendimentoHybridService.RetirarSemanaPool_Esol(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarSemanaPool_Esol(LiberacaoAgendamentoInputModel modelAgendamentoPool)
            => _empreendimentoHybridService.LiberarSemanaPool_Esol(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarMinhaSemanaPool_Esol(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
            => _empreendimentoHybridService.LiberarMinhaSemanaPool_Esol(modelAgendamentoPool);

        public Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_Esol(int agendamentoId)
            => _empreendimentoHybridService.ConsultarHistoricos_Esol(agendamentoId);

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_Esol(DispobilidadeSearchModel searchModel)
            => _empreendimentoHybridService.ConsultarDisponibilidadeCompativel_Esol(searchModel);

        public Task<ResultModel<int>?> TrocarSemana_Esol(TrocaSemanaInputModel model)
            => _empreendimentoHybridService.TrocarSemana_Esol(model);

        public Task<ResultModel<int>?> TrocarTipoUso_Esol(TrocaSemanaInputModel model)
            => _empreendimentoHybridService.TrocarTipoUso_Esol(model);

        public Task<ResultModel<int>?> IncluirSemana_Esol(IncluirSemanaInputModel model)
            => _empreendimentoHybridService.IncluirSemana_Esol(model);

        public Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_Esol(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false)
            => _empreendimentoHybridService.GetKeyValueListFromContratoSCP_Esol(model, codigoVerificacao, dataAssinatura, espanhol);

        public Task<DownloadContratoResultModel?> DownloadContratoSCP_Esol(int cotaId)
            => _empreendimentoHybridService.DownloadContratoSCP_Esol(cotaId);

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_Esol(string agendamentoId)
            => _empreendimentoHybridService.GetDadosImpressaoVoucher_Esol(agendamentoId);

        public Task<List<StatusCrcModel>?> ConsultarStatusCrc_Esol()
            => _empreendimentoHybridService.ConsultarStatusCrc_Esol();

        public Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_Esol(int ano, int? uhCondominioId = null, int? cotaPortalId = null)
            => _empreendimentoHybridService.GetPosicaoAgendamentoAnoAsync_Esol(ano, uhCondominioId, cotaPortalId);

        public DadosContratoModel? GetContrato_Esol(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
            => _empreendimentoHybridService.GetContrato_Esol(dadosReserva, contratos);
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


