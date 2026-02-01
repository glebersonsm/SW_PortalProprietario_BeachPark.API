using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Providers.Interfaces
{
    public interface IEmpreendimentoProviderService
    {
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
        Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP(GetHtmlValuesModel model,string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false);
        Task<DownloadContratoResultModel?> DownloadContratoSCP(int cotaId);
        Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(string agendamentoId);
        Task<List<StatusCrcModel>?> ConsultarStatusCrc();
        Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false);
        Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null);
        Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync(int ano, int? uhCondominioId = null, int? cotaPortalId = null);
        DadosContratoModel? GetContrato(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos);
    }
}
