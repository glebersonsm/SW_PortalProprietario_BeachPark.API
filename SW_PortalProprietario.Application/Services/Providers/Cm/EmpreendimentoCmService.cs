using CMDomain.Entities;
using Dapper;
using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Functions;

namespace SW_PortalProprietario.Application.Services.Providers.Cm
{
    public class EmpreendimentoCmService : IEmpreendimentoProviderService
    {
        private readonly ICommunicationProvider _communicationProvider;
        private readonly ILogger<EmpreendimentoCmService> _logger;
        private readonly IRepositoryNHCm _repositoryCM;
        private readonly ICacheStore _cacheStore;
        public EmpreendimentoCmService(ICommunicationProvider communicationProvider,
            ILogger<EmpreendimentoCmService> logger,
            IRepositoryNHCm repositoryCM,
            ICacheStore cacheStore)
        {
            _communicationProvider = communicationProvider;
            _logger = logger;
            _repositoryCM = repositoryCM;
            _cacheStore = cacheStore;
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

        public async Task<List<StatusCrcModel>?> ConsultarStatusCrc()
        {

            var statusCache = await _cacheStore.GetAsync<List<StatusCrcModel>>("MotivoBloqueioTS_", 2, _repositoryCM.CancellationToken);
            if (statusCache != null && statusCache.Any())
                return statusCache.AsList();

            var statusRetorno = (await _repositoryCM.FindBySql<StatusCrcModel>(@$"Select
                s.IdMotivoTs as Id,
                s.CodReduzido AS Codigo,
                s.Descricao AS Nome
                From
                MotivoTs s
                Where s.Aplicacao LIKE '%B%' and s.FlgAtivo = 'S'")).AsList();

            if (statusRetorno != null && statusRetorno.Any())
                await _cacheStore.AddAsync("MotivoBloqueioTS_", statusRetorno, DateTimeOffset.Now.AddHours(1), 2, _repositoryCM.CancellationToken);

            return statusRetorno != null && statusRetorno.Any() ? statusRetorno.AsList() : default;

            
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

        public DadosContratoModel? GetContrato(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
        {
            var contrato = contratos.FirstOrDefault();
            if (contrato != null)
            {
                dadosReserva.Contrato = contrato.NumeroContrato;
                dadosReserva.NomeCliente = contrato.PessoaTitular1Nome;
            }
            return contrato;
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
            throw new NotImplementedException();
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

        public Task<bool> ValidarCodigo(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true)
        {
            throw new NotImplementedException();
        }


    }
}
