using AccessCenterDomain.AccessCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras.Boleto;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Cm;
using SW_PortalProprietario.Application.Services.Providers.Esolution;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using SW_Utils.Auxiliar;

namespace SW_PortalProprietario.Application.Services.Providers.Hybrid
{
    /// <summary>
    /// Service híbrido para operações financeiras que integra Esolution e CM
    /// Consolida dados de ambos os sistemas e roteia operações baseado no contexto
    /// </summary>
    public class FinanceiroHybridService : IFinanceiroHybridProviderService
    {
        private const string PREFIXO_TRANSACOES_FINANCEIRAS = "PORTALprophybrid_";
        private readonly ILogger<FinanceiroHybridService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRepositoryNH _repositorySystem;
        private readonly IServiceBase _serviceBase;
        private readonly FinanceiroEsolutionService _esolutionService;
        private readonly FinanceiroCmService _cmService;

        public string PrefixoTransacaoFinanceira => PREFIXO_TRANSACOES_FINANCEIRAS;
        public string ProviderName => "Hybrid (Esolution + CM)";

        public FinanceiroHybridService(
            ILogger<FinanceiroHybridService> logger,
            IConfiguration configuration,
            IRepositoryNHAccessCenter repositoryAccessCenter,
            IRepositoryNHCm repositoryCm,
            IRepositoryNH repositorySystem,
            IServiceBase serviceBase,
            ICacheStore cacheStore,
            ICommunicationProvider communicationProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _repositorySystem = repositorySystem;
            _serviceBase = serviceBase;
        }

        /// <summary>
        /// Determina qual provider usar baseado no contexto
        /// </summary>
        private IFinanceiroProviderService GetProviderForContext(string? empresaId = null, string? pessoaProviderId = null)
        {
            // Estratégia 1: Configuração explícita por empresa
            if (!string.IsNullOrEmpty(empresaId))
            {
                var providerConfig = _configuration.GetValue<string>($"Empresas:{empresaId}:FinanceiroProvider");
                if (!string.IsNullOrEmpty(providerConfig))
                {
                    return providerConfig.Equals("cm", StringComparison.OrdinalIgnoreCase)
                        ? _cmService
                        : _esolutionService;
                }
            }

            // Estratégia 2: Range de IDs
            if (!string.IsNullOrEmpty(pessoaProviderId) && int.TryParse(pessoaProviderId, out var pessoaId))
            {
                var cmMaxId = _configuration.GetValue<int>("Providers:CM:MaxPessoaId", 100000);
                if (pessoaId < cmMaxId)
                {
                    _logger.LogDebug($"Usando CM Provider para pessoaId: {pessoaId}");
                    return _cmService;
                }
            }

            // Default: Esolution
            _logger.LogDebug("Usando Esolution Provider (padrão)");
            return _esolutionService;
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario(SearchContasPendentesUsuarioLogado searchModel)
        {
            _logger.LogInformation("Buscando contas pendentes do usuário em ambos os sistemas");

            var resultadosConsolidados = new List<ContaPendenteModel>();
            int pageNumber = searchModel.NumeroDaPagina.GetValueOrDefault(1);
            int lastPageNumber = 1;

            // Busca no Esolution
            try
            {
                var esolutionResult = await _esolutionService.GetContaPendenteDoUsuario(searchModel);
                if (esolutionResult != null && esolutionResult.Value.contasPendentes.Any())
                {
                    resultadosConsolidados.AddRange(esolutionResult.Value.contasPendentes);
                    lastPageNumber = Math.Max(lastPageNumber, esolutionResult.Value.lastPageNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contas no Esolution: {ex.Message}");
            }

            // Busca no CM
            try
            {
                var cmResult = await _cmService.GetContaPendenteDoUsuario(searchModel);
                if (cmResult != null && cmResult.Value.contasPendentes.Any())
                {
                    resultadosConsolidados.AddRange(cmResult.Value.contasPendentes);
                    lastPageNumber = Math.Max(lastPageNumber, cmResult.Value.lastPageNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contas no CM: {ex.Message}");
            }

            if (!resultadosConsolidados.Any())
            {
                return (1, 1, new List<ContaPendenteModel>());
            }

            // Ordena os resultados consolidados
            resultadosConsolidados = resultadosConsolidados
                .OrderBy(c => c.Vencimento)
                .ThenBy(c => c.Id)
                .ToList();

            // Aplica paginação aos resultados consolidados
            var pageSize = searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(15);
            var totalItems = resultadosConsolidados.Count;
            lastPageNumber = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var paginatedResults = resultadosConsolidados
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pageNumber, lastPageNumber, paginatedResults);
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral(SearchContasPendentesGeral searchModel)
        {
            _logger.LogInformation("Buscando contas pendentes gerais em ambos os sistemas");

            var resultadosConsolidados = new List<ContaPendenteModel>();
            int pageNumber = searchModel.NumeroDaPagina.GetValueOrDefault(1);
            int lastPageNumber = 1;

            // Busca no Esolution
            try
            {
                var esolutionResult = await _esolutionService.GetContaPendenteGeral(searchModel);
                if (esolutionResult != null && esolutionResult.Value.contasPendentes.Any())
                {
                    resultadosConsolidados.AddRange(esolutionResult.Value.contasPendentes);
                    lastPageNumber = Math.Max(lastPageNumber, esolutionResult.Value.lastPageNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contas gerais no Esolution: {ex.Message}");
            }

            // Busca no CM
            try
            {
                var cmResult = await _cmService.GetContaPendenteGeral(searchModel);
                if (cmResult != null && cmResult.Value.contasPendentes.Any())
                {
                    resultadosConsolidados.AddRange(cmResult.Value.contasPendentes);
                    lastPageNumber = Math.Max(lastPageNumber, cmResult.Value.lastPageNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contas gerais no CM: {ex.Message}");
            }

            if (!resultadosConsolidados.Any())
            {
                return (1, 1, new List<ContaPendenteModel>());
            }

            // Remove duplicatas baseado no ID e Provider
            resultadosConsolidados = resultadosConsolidados
                .GroupBy(c => new { c.Id, c.PessoaProviderId })
                .Select(g => g.First())
                .OrderBy(c => c.Vencimento)
                .ThenBy(c => c.NomePessoa)
                .ToList();

            // Aplica paginação
            var pageSize = searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(15);
            var totalItems = resultadosConsolidados.Count;
            lastPageNumber = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginatedResults = resultadosConsolidados
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pageNumber, lastPageNumber, paginatedResults);
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            _logger.LogInformation($"Buscando contas para pagamento em cartão - PessoaId: {getContasParaPagamentoEmCartaoModel.PessoaId}");

            // Determina o provider baseado no contexto
            var provider = GetProviderForContext(
                getContasParaPagamentoEmCartaoModel.PessoaId?.ToString(),
                getContasParaPagamentoEmCartaoModel.PessoaId?.ToString());

            return await provider.GetContasParaPagamentoEmCartaoGeral(getContasParaPagamentoEmCartaoModel);
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado");

            var pessoaVinculada = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(
                Convert.ToInt32(loggedUser.Value.userId), 
                ProviderName);

            var provider = GetProviderForContext(
                pessoaProviderId: pessoaVinculada?.PessoaProvider);

            return await provider.GetContasParaPagamentoEmCartaoDoUsuario(getContasParaPagamentoEmCartaoModel);
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            var provider = GetProviderForContext(
                getContasParaPagamentoEmPixModel.PessoaId?.ToString(),
                getContasParaPagamentoEmPixModel.PessoaId?.ToString());

            return await provider.GetContasParaPagamentoEmPixGeral(getContasParaPagamentoEmPixModel);
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado");

            var pessoaVinculada = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(
                Convert.ToInt32(loggedUser.Value.userId),
                ProviderName);

            var provider = GetProviderForContext(pessoaProviderId: pessoaVinculada?.PessoaProvider);

            return await provider.GetContasParaPagamentoEmPixDoUsuario(getContasParaPagamentoEmPixModel);
        }

        public async Task<List<ContaPendenteModel>> GetContasPorIds(List<int> itensToPay)
        {
            _logger.LogInformation($"Buscando contas por IDs em ambos os sistemas - Total: {itensToPay.Count}");

            var resultados = new List<ContaPendenteModel>();

            // Tenta no Esolution
            try
            {
                var esolutionResult = await _esolutionService.GetContasPorIds(itensToPay);
                if (esolutionResult != null && esolutionResult.Any())
                {
                    resultados.AddRange(esolutionResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contas por IDs no Esolution: {ex.Message}");
            }

            // Tenta no CM para IDs não encontrados
            var idsNaoEncontrados = itensToPay.Except(resultados.Select(r => r.Id)).ToList();
            if (idsNaoEncontrados.Any())
            {
                try
                {
                    var cmResult = await _cmService.GetContasPorIds(idsNaoEncontrados);
                    if (cmResult != null && cmResult.Any())
                    {
                        resultados.AddRange(cmResult);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Erro ao buscar contas por IDs no CM: {ex.Message}");
                }
            }

            return resultados;
        }

        public async Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa(int pessoaProviderId)
        {
            _logger.LogInformation($"Buscando dados da pessoa: {pessoaProviderId}");

            // Tenta primeiro no Esolution
            try
            {
                var esolutionResult = await _esolutionService.GetDadosPessoa(pessoaProviderId);
                if (esolutionResult != null)
                    return esolutionResult;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar pessoa no Esolution: {ex.Message}");
            }

            // Se não encontrou no Esolution, tenta no CM
            try
            {
                return await _cmService.GetDadosPessoa(pessoaProviderId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar pessoa no CM: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CotaPeriodoModel>> GetCotaPeriodo(int pessoaId, DateTime? dataInicial, DateTime? dataFinal)
        {
            _logger.LogInformation($"Buscando períodos de cota para pessoa: {pessoaId}");

            var resultados = new List<CotaPeriodoModel>();

            // Busca no Esolution
            try
            {
                var esolutionResult = await _esolutionService.GetCotaPeriodo(pessoaId, dataInicial, dataFinal);
                if (esolutionResult != null && esolutionResult.Any())
                {
                    resultados.AddRange(esolutionResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar períodos de cota no Esolution: {ex.Message}");
            }

            // Busca no CM
            try
            {
                var cmResult = await _cmService.GetCotaPeriodo(pessoaId, dataInicial, dataFinal);
                if (cmResult != null && cmResult.Any())
                {
                    resultados.AddRange(cmResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar períodos de cota no CM: {ex.Message}");
            }

            return resultados.OrderBy(c => c.DataInicial).ToList();
        }

        public async Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail(bool pool, bool naoPool)
        {
            _logger.LogInformation($"Buscando proprietários para envio de email - Pool: {pool}, NãoPool: {naoPool}");

            var resultados = new List<CotaPeriodoModel>();

            // Busca no Esolution
            try
            {
                var esolutionResult = await _esolutionService.GetProprietariosParaEnvioEmail(pool, naoPool);
                if (esolutionResult != null && esolutionResult.Any())
                {
                    resultados.AddRange(esolutionResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar proprietários no Esolution: {ex.Message}");
            }

            // Busca no CM
            try
            {
                var cmResult = await _cmService.GetProprietariosParaEnvioEmail(pool, naoPool);
                if (cmResult != null && cmResult.Any())
                {
                    resultados.AddRange(cmResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar proprietários no CM: {ex.Message}");
            }

            // Remove duplicatas baseado em Proprietario e Email
            return resultados
                .GroupBy(p => new { p.Proprietario, p.Email })
                .Select(g => g.First())
                .ToList();
        }

        public async Task<BoletoModel> DownloadBoleto(DownloadBoleto model)
        {
            _logger.LogInformation($"Download de boleto - Linha digitável: {model.LinhaDigitavelBoleto}");

            // Tenta primeiro no Esolution
            try
            {
                return await _esolutionService.DownloadBoleto(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao fazer download do boleto no Esolution: {ex.Message}");
            }

            // Se falhou no Esolution, tenta no CM
            try
            {
                return await _cmService.DownloadBoleto(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao fazer download do boleto no CM: {ex.Message}");
                throw new FileNotFoundException("Boleto não encontrado em nenhum dos sistemas");
            }
        }

        public async Task<BaixaResultModel> BaixarValoresPagosEmPix(PaymentPix item, IStatelessSession? session)
        {
            // Determina o provider baseado na empresa do pagamento
            var provider = GetProviderForContext(item.CompanyId?.ToString());
            return await provider.BaixarValoresPagosEmPix(item, session);
        }

        public async Task<BaixaResultModel> AlterarTipoContaReceberPagasEmCartao(PaymentCardTokenized item, IStatelessSession? session)
        {
            // Determina o provider baseado na empresa do pagamento
            var provider = GetProviderForContext(item.CompanyId?.ToString());
            return await provider.AlterarTipoContaReceberPagasEmCartao(item, session);
        }

        public async Task<bool> VoltarParaTiposOriginais(PaymentCardTokenized item, IStatelessSession? session)
        {
            var provider = GetProviderForContext(item.CompanyId?.ToString());
            return await provider.VoltarParaTiposOriginais(item, session);
        }

        public async Task<int> SalvarContaBancaria(ClienteContaBancariaInputModel model)
        {
            var provider = GetProviderForContext(model.EmpresaId?.ToString());
            return await provider.SalvarContaBancaria(model);
        }

        public async Task<ClienteContaBancaria?> SalvarContaBancariaInterna(ClienteContaBancariaInputModel model, ParametroSistemaViewModel? parametroSistema = null)
        {
            var provider = GetProviderForContext(model.EmpresaId?.ToString());
            return await provider.SalvarContaBancariaInterna(model, parametroSistema);
        }

        public async Task<int> SalvarMinhaContaBancaria(ClienteContaBancariaInputModel model)
        {
            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado");

            var pessoaVinculada = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(
                Convert.ToInt32(loggedUser.Value.userId),
                ProviderName);

            var provider = GetProviderForContext(pessoaProviderId: pessoaVinculada?.PessoaProvider);
            return await provider.SalvarMinhaContaBancaria(model);
        }

        public async Task<List<ClienteContaBancariaViewModel>> GetContasBancarias(int pessoaId)
        {
            _logger.LogInformation($"Buscando contas bancárias para pessoa: {pessoaId}");

            var resultados = new List<ClienteContaBancariaViewModel>();

            // Busca no Esolution
            try
            {
                var esolutionResult = await _esolutionService.GetContasBancarias(pessoaId);
                if (esolutionResult != null && esolutionResult.Any())
                {
                    resultados.AddRange(esolutionResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contas bancárias no Esolution: {ex.Message}");
            }

            // Busca no CM
            try
            {
                var cmResult = await _cmService.GetContasBancarias(pessoaId);
                if (cmResult != null && cmResult.Any())
                {
                    resultados.AddRange(cmResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contas bancárias no CM: {ex.Message}");
            }

            // Remove duplicatas baseado no Id
            return resultados
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();
        }

        public async Task<List<ClienteContaBancariaViewModel>> GetMinhasContasBancarias()
        {
            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado");

            var pessoaVinculada = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(
                Convert.ToInt32(loggedUser.Value.userId),
                ProviderName);

            _logger.LogInformation($"Buscando minhas contas bancárias para pessoa: {pessoaVinculada?.PessoaProvider}");

            var resultados = new List<ClienteContaBancariaViewModel>();

            // Busca no Esolution
            try
            {
                var esolutionResult = await _esolutionService.GetMinhasContasBancarias();
                if (esolutionResult != null && esolutionResult.Any())
                {
                    resultados.AddRange(esolutionResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar minhas contas bancárias no Esolution: {ex.Message}");
            }

            // Busca no CM
            try
            {
                var cmResult = await _cmService.GetMinhasContasBancarias();
                if (cmResult != null && cmResult.Any())
                {
                    resultados.AddRange(cmResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar minhas contas bancárias no CM: {ex.Message}");
            }

            // Remove duplicatas
            return resultados
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();
        }
    }
}