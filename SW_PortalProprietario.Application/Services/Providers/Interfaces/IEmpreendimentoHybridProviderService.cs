using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Providers.Interfaces
{
    public interface IEmpreendimentoHybridProviderService
    {

        #region CM Methods
        Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_CM(SearchImovelModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_CM(SearchProprietarioModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_CM(SearchMyContractsModel searchModel);
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
        Task<bool> GerarCodigoVerificacaoLiberacaoPool_CM(int agendamentoId);
        Task<bool> ValidarCodigo_CM(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true);
        Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_CM(int agendamentoId);
        Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_CM(DispobilidadeSearchModel searchModel);
        Task<ResultModel<int>?> TrocarSemana_CM(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> TrocarTipoUso_CM(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> IncluirSemana_CM(IncluirSemanaInputModel model);
        Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_CM(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false);
        Task<DownloadContratoResultModel?> DownloadContratoSCP_CM(int cotaId);
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_CM(string agendamentoId);
        Task<List<StatusCrcModel>?> ConsultarStatusCrc_CM();
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_CM(DateTime checkInDate, bool simulacao = false);
        Task<List<ClientesInadimplentes>> Inadimplentes_CM(List<int>? pessoasPesquisar = null);
        Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_CM(int ano, int? uhCondominioId = null, int? cotaPortalId = null);
        DadosContratoModel? GetContrato_CM(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
        #endregion

        #region Esolution Methods
        Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_Esol(SearchImovelModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_Esol(SearchProprietarioModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_Esol(SearchMyContractsModel searchModel);
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
        Task<bool> GerarCodigoVerificacaoLiberacaoPool_Esol(int agendamentoId);
        Task<bool> ValidarCodigo_Esol(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true);
        Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_Esol(int agendamentoId);
        Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_Esol(DispobilidadeSearchModel searchModel);
        Task<ResultModel<int>?> TrocarSemana_Esol(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> TrocarTipoUso_Esol(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> IncluirSemana_Esol(IncluirSemanaInputModel model);
        Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_Esol(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false);
        Task<DownloadContratoResultModel?> DownloadContratoSCP_Esol(int cotaId);
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_Esol(string agendamentoId);
        Task<List<StatusCrcModel>?> ConsultarStatusCrc_Esol();
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(DateTime checkInDate, bool simulacao = false);
        Task<List<ClientesInadimplentes>> Inadimplentes_Esol(List<int>? pessoasPesquisar = null);
        Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_Esol(int ano, int? uhCondominioId = null, int? cotaPortalId = null);
        DadosContratoModel? GetContrato_Esol(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
        #endregion

        #region Default Methods (delegates to Esol)
        Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis(SearchImovelModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios(SearchProprietarioModel searchModel);
        Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts(SearchMyContractsModel searchModel);
        Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais(ReservasMultiPropriedadeSearchModel model);
        Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model);
        Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamento);
        Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamento);
        Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model);
        Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id);
        Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id);
        Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel);
        Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool);
        Task<ResultModel<bool>?> LiberarMinhaSemanaPool(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool);
        Task<bool> GerarCodigoVerificacaoLiberacaoPool(int agendamentoId);
        Task<bool> ValidarCodigo(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true);
        Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos(int agendamentoId);
        Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel(DispobilidadeSearchModel searchModel);
        Task<ResultModel<int>?> TrocarSemana(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> TrocarTipoUso(TrocaSemanaInputModel model);
        Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model);
        Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false);
        Task<DownloadContratoResultModel?> DownloadContratoSCP(int cotaId);
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(string agendamentoId);
        Task<List<StatusCrcModel>?> ConsultarStatusCrc();
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false);
        Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null);
        Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync(int ano, int? uhCondominioId = null, int? cotaPortalId = null);
        DadosContratoModel? GetContrato(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
        #endregion
    }
}
