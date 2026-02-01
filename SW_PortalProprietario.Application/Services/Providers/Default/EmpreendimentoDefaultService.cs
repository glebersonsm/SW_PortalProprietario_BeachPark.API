using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;

namespace SW_PortalProprietario.Application.Services.Providers.Default
{
    public class EmpreendimentoDefaultService : IEmpreendimentoProviderService
    {
        private readonly IFinanceiroProviderService _financeiroProvider;
        private readonly ILogger<EmpreendimentoDefaultService> _logger;
        public EmpreendimentoDefaultService(IFinanceiroProviderService financeiroProviderService,
            ILogger<EmpreendimentoDefaultService> logger)
        {
            _financeiroProvider = financeiroProviderService;
            _logger = logger;
        }

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais(ReservasMultiPropriedadeSearchModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel(DispobilidadeSearchModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos(int agendamentoId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamento)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamento)
        {
            throw new NotImplementedException();
        }

        public Task<List<StatusCrcModel>?> ConsultarStatusCrc()
        {
            throw new NotImplementedException();
        }

        public Task<DownloadContratoResultModel> DownloadContratoSCP(int cotaId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GerarCodigoVerificacaoLiberacaoPool(int agendamentoId)
        {
            throw new NotImplementedException();
        }

        public DadosContratoModel GetContrato(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
        {
            throw new NotImplementedException();
        }

        public Task<List<dynamic>> GetContratosSemReservasFuturasAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(string agendamentoId)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis(SearchImovelModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts(SearchMyContractsModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync(int ano, int? uhCondominioId = null, int? cotaPortalId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios(SearchProprietarioModel searchModel)
        {
            throw new NotFiniteNumberException();
        }

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> LiberarMinhaSemanaPool(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<int>> TrocarSemana(TrocaSemanaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<int>?> TrocarTipoUso(TrocaSemanaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidarCodigo(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = false)
        {
            throw new NotImplementedException();
        }

        Task<DadosImpressaoVoucherResultModel?> IEmpreendimentoProviderService.GetDadosImpressaoVoucher(string agendamentoId)
        {
            throw new NotImplementedException();
        }
    }
}
