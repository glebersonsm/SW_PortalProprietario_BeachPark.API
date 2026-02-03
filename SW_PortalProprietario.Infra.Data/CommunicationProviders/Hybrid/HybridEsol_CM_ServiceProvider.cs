using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Infra.Data.CommunicationProviders.CM;
using SW_PortalProprietario.Infra.Data.CommunicationProviders.Esolution;
using SW_PortalProprietario.Infra.Data.Caching;
using SW_PortalProprietario.Application.Services.Providers;

namespace SW_PortalProprietario.Infra.Data.CommunicationProviders.Hybrid
{
    /// <summary>
    /// Provider híbrido que comunica com ambos os sistemas (Esolution e CM)
    /// Detecta automaticamente qual provider usar baseado na configuração ou contexto do usuário
    /// </summary>
    public class HybridEsol_CM_ServiceProvider : IHybridEsol_CM_ServiceProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HybridEsol_CM_ServiceProvider> _logger;
        private readonly IRepositoryNH _repositorySystem;
        private readonly ICacheStore _cacheStore;
        private readonly IRepositoryNHCm _repositoryCm;
        private readonly IRepositoryNHAccessCenter _repositoryAccessCenter;
        private readonly IRepositoryNHEsolPortal _repositoryPortalEsol;
        private readonly ICommunicationEsolutionProvider _esolutionProvider;
        private readonly ICommunicationCmProvider _cmProvider;

        public string CommunicationProviderName => "Hybrid (Esolution + CM)";
        public string PrefixoTransacaoFinanceira => "HYB";

        public HybridEsol_CM_ServiceProvider(
            IConfiguration configuration,
            IRepositoryNHAccessCenter repositoryAccessCenter,
            IRepositoryNHEsolPortal repositoryPortalEsol,
            IRepositoryNHCm repositoryCm,
            IRepositoryNH repositorySystem,
            ILogger<HybridEsol_CM_ServiceProvider> logger,
            ICacheStore cacheStore,
            ICommunicationEsolutionProvider esolutionProvider,
            ICommunicationCmProvider cmProvider)
        {
            _configuration = configuration;
            _repositorySystem = repositorySystem;
            _logger = logger;
            _cacheStore = cacheStore;
            _repositoryCm = repositoryCm;
            _repositoryAccessCenter = repositoryAccessCenter;
            _repositoryPortalEsol = repositoryPortalEsol;
            _cmProvider = cmProvider;
            _esolutionProvider = esolutionProvider;
        }

        /// <summary>
        /// Determina qual provider usar baseado no contexto
        /// Pode ser configurado por: empresa, usuário, tipo de operação, etc.
        /// </summary>
        private ICommunicationProvider GetProviderForContext(string? empresaId = null, string? pessoaProviderId = null)
        {
            // Estratégia 1: Configuração explícita
            var providerConfig = _configuration.GetValue<string>($"Empresas:{empresaId}:Provider");
            if (!string.IsNullOrEmpty(providerConfig))
            {
                return providerConfig.Equals("cm", StringComparison.OrdinalIgnoreCase) 
                    ? _cmProvider 
                    : _esolutionProvider;
            }

            // Estratégia 2: Detectar pelo ID da pessoa (range de IDs por exemplo)
            if (!string.IsNullOrEmpty(pessoaProviderId) && int.TryParse(pessoaProviderId, out var pessoaId))
            {
                // Exemplo: IDs < 100000 = CM, >= 100000 = Esolution
                var cmMaxId = _configuration.GetValue<int>("Providers:CM:MaxPessoaId", 100000);
                if (pessoaId < cmMaxId)
                {
                    _logger.LogDebug($"Usando CM Provider para pessoaId: {pessoaId}");
                    return _cmProvider;
                }
            }

            // Estratégia 3: Verificar se existe dados em cada sistema
            // Este é um fallback mais custoso, use com cuidado
            
            // Default: Esolution
            _logger.LogDebug("Usando Esolution Provider (padrão)");
            return _esolutionProvider;
        }

        private async Task<IAccessValidateResultModel> ValidateAccess(string login, string senha, string pessoaProviderId = "")
        {
            _logger.LogInformation($"Validando acesso híbrido para login: {login}");

            // Tenta primeiro no Esolution
            try
            {
                var esolutionResult = await _esolutionProvider.ValidateAccess(login, senha, pessoaProviderId);
                if (esolutionResult != null && !string.IsNullOrEmpty(esolutionResult.PessoaId))
                {
                    _logger.LogInformation($"Autenticação bem-sucedida via Esolution para: {login}");
                    return esolutionResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Falha ao validar em Esolution: {ex.Message}");
            }

            // Se falhou no Esolution, tenta no CM
            try
            {
                var cmResult = await _cmProvider.ValidateAccess(login, senha, pessoaProviderId);
                if (cmResult != null && !string.IsNullOrEmpty(cmResult.PessoaId))
                {
                    _logger.LogInformation($"Autenticação bem-sucedida via CM para: {login}");
                    return cmResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Falha ao validar em CM: {ex.Message}");
            }

            // Se falhou em ambos
            _logger.LogWarning($"Falha na autenticação híbrida para: {login}");
            return new AccessValidateResultModel 
            {
                UsuarioValido = false,
                Erros = new List<string> { "Credenciais inválidas em ambos os sistemas" }
            };
        }

        private async Task GravarVinculoUsuario(IAccessValidateResultModel result, Usuario usuario)
        {
            var provider = GetProviderForContext("1", "Hybrido");
            await provider.GravarVinculoUsuario(result, usuario);
        }

        private bool IsDefault()
        {
            return false; // Hybrid provider nunca é default
        }

        private async Task<bool> GravarUsuarioNoLegado(string pessoaProviderId, string login, string senha)
        {
            var provider = GetProviderForContext(pessoaProviderId: pessoaProviderId);
            return await provider.GravarUsuarioNoLegado(pessoaProviderId, login, senha);
        }

        private async Task<bool> AlterarSenhaNoLegado(string pessoaProviderId, string login, string senha)
        {
            var provider = GetProviderForContext(pessoaProviderId: pessoaProviderId);
            return await provider.AlterarSenhaNoLegado(pessoaProviderId, login, senha);
        }

        private async Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider(string pessoaProviderId)
        {
            var provider = GetProviderForContext(pessoaProviderId: pessoaProviderId);
            return await provider.GetOutrosDadosPessoaProvider(pessoaProviderId);
        }

        private async Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado(int id)
        {
            // Tenta buscar em ambos os sistemas e retorna o primeiro que encontrar
            try
            {
                var esolutionResult = await _esolutionProvider.GetEmpresaVinculadaLegado(id);
                if (esolutionResult != null) return esolutionResult;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar empresa no Esolution: {ex.Message}");
            }

            try
            {
                return await _cmProvider.GetEmpresaVinculadaLegado(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar empresa no CM: {ex.Message}");
                return null;
            }
        }

        private async Task<List<PaisModel>> GetPaisesLegado()
        {
            // Consolida dados de ambos os sistemas
            var paisesList = new List<PaisModel>();

            try
            {
                var esolutionPaises = await _esolutionProvider.GetPaisesLegado();
                paisesList.AddRange(esolutionPaises);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar países do Esolution: {ex.Message}");
            }

            try
            {
                var cmPaises = await _cmProvider.GetPaisesLegado();
                paisesList.AddRange(cmPaises);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar países do CM: {ex.Message}");
            }

            // Remove duplicados baseado no ID
            return paisesList
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();
        }

        private async Task<List<EstadoModel>> GetEstadosLegado()
        {
            var estadosList = new List<EstadoModel>();

            try
            {
                var esolutionEstados = await _esolutionProvider.GetEstadosLegado();
                estadosList.AddRange(esolutionEstados);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar estados do Esolution: {ex.Message}");
            }

            try
            {
                var cmEstados = await _cmProvider.GetEstadosLegado();
                estadosList.AddRange(cmEstados);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar estados do CM: {ex.Message}");
            }

            return estadosList
                .GroupBy(e => e.Id)
                .Select(g => g.First())
                .ToList();
        }

        private async Task<List<CidadeModel>> GetCidade()
        {
            var cidadesList = new List<CidadeModel>();

            try
            {
                var esolutionCidades = await _esolutionProvider.GetCidade();
                cidadesList.AddRange(esolutionCidades);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar cidades do Esolution: {ex.Message}");
            }

            try
            {
                var cmCidades = await _cmProvider.GetCidade();
                cidadesList.AddRange(cmCidades);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar cidades do CM: {ex.Message}");
            }

            return cidadesList
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();
        }

        private async Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado()
        {
            var usuariosList = new List<UserRegisterInputModel>();

            try
            {
                var esolutionUsuarios = await _esolutionProvider.GetUsuariosAtivosSistemaLegado();
                usuariosList.AddRange(esolutionUsuarios);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar usuários do Esolution: {ex.Message}");
            }

            try
            {
                var cmUsuarios = await _cmProvider.GetUsuariosAtivosSistemaLegado();
                usuariosList.AddRange(cmUsuarios);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar usuários do CM: {ex.Message}");
            }

            return usuariosList;
        }

        private async Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado(ParametroSistemaViewModel parametroSistema)
        {
            var usuariosList = new List<UserRegisterInputModel>();

            try
            {
                var esolutionUsuarios = await _esolutionProvider.GetClientesUsuariosLegado(parametroSistema);
                usuariosList.AddRange(esolutionUsuarios);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar clientes do Esolution: {ex.Message}");
            }

            try
            {
                var cmUsuarios = await _cmProvider.GetClientesUsuariosLegado(parametroSistema);
                usuariosList.AddRange(cmUsuarios);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar clientes do CM: {ex.Message}");
            }

            return usuariosList;
        }

        private async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade(CidadeSearchModel searchModel)
        {
            // Implementação simplificada - retorna do provider preferencial
            var provider = GetProviderForContext();
            return await provider.SearchCidade(searchModel);
        }

        private async Task<bool> DesativarUsuariosSemCotaOuContrato()
        {
            var esolutionResult = await _esolutionProvider.DesativarUsuariosSemCotaOuContrato();
            var cmResult = await _cmProvider.DesativarUsuariosSemCotaOuContrato();
            return esolutionResult && cmResult;
        }

        private async Task GetOutrosDadosUsuario(TokenResultModel userReturn)
        {
            var provider = GetProviderForContext(userReturn.CompanyId!.ToString(), userReturn.ProviderKeyUser);
            await provider.GetOutrosDadosUsuario(userReturn);
        }

        private async Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar)
        {
            var contratosList = new List<DadosContratoModel>();

            try
            {
                var esolutionContratos = await _esolutionProvider.GetContratos(pessoasPesquisar);
                if (esolutionContratos != null) contratosList.AddRange(esolutionContratos);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contratos do Esolution: {ex.Message}");
            }

            try
            {
                var cmContratos = await _cmProvider.GetContratos(pessoasPesquisar);
                if (cmContratos != null) contratosList.AddRange(cmContratos);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar contratos do CM: {ex.Message}");
            }

            return contratosList;
        }

        private async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(List<string> empresasIds)
        {
            var empresasList = new List<EmpresaVinculadaModel>();

            try
            {
                var esolutionEmpresas = await _esolutionProvider.GetEmpresasVinculadas(empresasIds);
                if (esolutionEmpresas != null) empresasList.AddRange(esolutionEmpresas);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar empresas do Esolution: {ex.Message}");
            }

            try
            {
                var cmEmpresas = await _cmProvider.GetEmpresasVinculadas(empresasIds);
                if (cmEmpresas != null) empresasList.AddRange(cmEmpresas);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar empresas do CM: {ex.Message}");
            }

            return empresasList;
        }

        private async Task<UsuarioValidateResultModel> GerUserFromLegado(UserRegisterInputModel model)
        {
            // Tenta em ambos os sistemas
            try
            {
                var esolutionResult = await _esolutionProvider.GerUserFromLegado(model);
                if (esolutionResult != null && esolutionResult.Id > 0)
                    return esolutionResult;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar usuário no Esolution: {ex.Message}");
            }

            return await _cmProvider.GerUserFromLegado(model);
        }

        private async Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado()
        {
            var usuariosList = new List<UserRegisterInputModel>();

            try
            {
                var esolutionUsuarios = await _esolutionProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado();
                usuariosList.AddRange(esolutionUsuarios);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro no Esolution: {ex.Message}");
            }

            try
            {
                var cmUsuarios = await _cmProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado();
                usuariosList.AddRange(cmUsuarios);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro no CM: {ex.Message}");
            }

            return usuariosList;
        }

        private async Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null)
        {
            var inadimplentesList = new List<ClientesInadimplentes>();

            try
            {
                var esolutionInadimplentes = await _esolutionProvider.Inadimplentes(pessoasPesquisar);
                inadimplentesList.AddRange(esolutionInadimplentes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar inadimplentes do Esolution: {ex.Message}");
            }

            try
            {
                var cmInadimplentes = await _cmProvider.Inadimplentes(pessoasPesquisar);
                inadimplentesList.AddRange(cmInadimplentes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar inadimplentes do CM: {ex.Message}");
            }

            return inadimplentesList;
        }

        private async Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false)
        {
            var reservasList = new List<ReservaInfo>();

            try
            {
                var esolutionReservas = await _esolutionProvider.GetReservasWithCheckInDateMultiPropriedadeAsync(checkInDate, simulacao);
                reservasList.AddRange(esolutionReservas);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar reservas do Esolution: {ex.Message}");
            }

            try
            {
                var cmReservas = await _cmProvider.GetReservasWithCheckInDateMultiPropriedadeAsync(checkInDate, simulacao);
                reservasList.AddRange(cmReservas);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar reservas do CM: {ex.Message}");
            }

            return reservasList;
        }

        private async Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync(DateTime checkInDate, bool simulacao = false)
        {
            var reservasList = new List<ReservaInfo>();

            try
            {
                var esolutionReservas = await _esolutionProvider.GetReservasWithCheckInDateTimeSharingAsync(checkInDate, simulacao);
                reservasList.AddRange(esolutionReservas);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar reservas TS do Esolution: {ex.Message}");
            }

            try
            {
                var cmReservas = await _cmProvider.GetReservasWithCheckInDateTimeSharingAsync(checkInDate, simulacao);
                reservasList.AddRange(cmReservas);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao buscar reservas TS do CM: {ex.Message}");
            }

            return reservasList;
        }

        private bool? ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            // Usa a lógica do provider configurado ou Esolution como padrão
            var provider = GetProviderForContext(reserva.EmpresaId?.ToString());
            return provider.ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes);
        }

        #region CM Methods
        public string CommunicationProviderName_CM => _cmProvider.CommunicationProviderName;
        public string PrefixoTransacaoFinanceira_CM => _cmProvider.PrefixoTransacaoFinanceira;

        public async Task<IAccessValidateResultModel> ValidateAccess_CM(string login, string senha, string pessoaProviderId = "")
        {
            return await _cmProvider.ValidateAccess(login, senha, pessoaProviderId);
        }

        public async Task<UsuarioValidateResultModel> GerUserFromLegado_CM(UserRegisterInputModel model)
        {
            return await _cmProvider.GerUserFromLegado(model);
        }

        public async Task<bool> GravarUsuarioNoLegado_CM(string pessoaProviderId, string login, string senha)
        {
            return await _cmProvider.GravarUsuarioNoLegado(pessoaProviderId, login, senha);
        }

        public async Task<bool> AlterarSenhaNoLegado_CM(string pessoaProviderId, string login, string senha)
        {
            return await _cmProvider.AlterarSenhaNoLegado(pessoaProviderId, login, senha);
        }

        public async Task<bool> IsDefault_CM()
        {
            return await _cmProvider.IsDefault();
        }

        public async Task GravarVinculoUsuario_CM(IAccessValidateResultModel result, Usuario usuario)
        {
            await _cmProvider.GravarVinculoUsuario(result, usuario);
        }

        public async Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_CM(string pessoaProviderId)
        {
            return await _cmProvider.GetOutrosDadosPessoaProvider(pessoaProviderId);
        }

        public async Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_CM(int id)
        {
            return await _cmProvider.GetEmpresaVinculadaLegado(id);
        }

        public async Task<List<PaisModel>> GetPaisesLegado_CM()
        {
            return await _cmProvider.GetPaisesLegado();
        }

        public async Task<List<EstadoModel>> GetEstadosLegado_CM()
        {
            return await _cmProvider.GetEstadosLegado();
        }

        public async Task<List<CidadeModel>> GetCidade_CM()
        {
            return await _cmProvider.GetCidade();
        }

        public async Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_CM()
        {
            return await _cmProvider.GetUsuariosAtivosSistemaLegado();
        }

        public async Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_CM(ParametroSistemaViewModel parametroSistema)
        {
            return await _cmProvider.GetClientesUsuariosLegado(parametroSistema);
        }

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_CM(CidadeSearchModel searchModel)
        {
            return await _cmProvider.SearchCidade(searchModel);
        }

        public async Task<bool> DesativarUsuariosSemCotaOuContrato_CM()
        {
            return await _cmProvider.DesativarUsuariosSemCotaOuContrato();
        }

        public async Task GetOutrosDadosUsuario_CM(TokenResultModel userReturn)
        {
            await _cmProvider.GetOutrosDadosUsuario(userReturn);
        }

        public async Task<List<DadosContratoModel>?> GetContratos_CM(List<int> pessoasPesquisar)
        {
            return await _cmProvider.GetContratos(pessoasPesquisar);
        }

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_CM(List<string> empresasIds)
        {
            return await _cmProvider.GetEmpresasVinculadas(empresasIds);
        }

        public async Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_CM()
        {
            return await _cmProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado();
        }

        public async Task<List<ClientesInadimplentes>> Inadimplentes_CM(List<int>? pessoasPesquisar = null)
        {
            return await _cmProvider.Inadimplentes(pessoasPesquisar);
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_CM(DateTime checkInDate, bool simulacao = false)
        {
            return await _cmProvider.GetReservasWithCheckInDateMultiPropriedadeAsync(checkInDate, simulacao);
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_CM(DateTime checkInDate, bool simulacao = false)
        {
            return await _cmProvider.GetReservasWithCheckInDateTimeSharingAsync(checkInDate, simulacao);
        }

        public bool? ShouldSendEmailForReserva_CM(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            return _cmProvider.ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes);
        }
        #endregion

        #region Esolution Methods
        public string CommunicationProviderName_Esol => _esolutionProvider.CommunicationProviderName;
        public string PrefixoTransacaoFinanceira_Esol => _esolutionProvider.PrefixoTransacaoFinanceira;

        public async Task<IAccessValidateResultModel> ValidateAccess_Esol(string login, string senha, string pessoaProviderId = "")
        {
            return await _esolutionProvider.ValidateAccess(login, senha, pessoaProviderId);
        }

        public async Task<UsuarioValidateResultModel> GerUserFromLegado_Esol(UserRegisterInputModel model)
        {
            return await _esolutionProvider.GerUserFromLegado(model);
        }

        public async Task<bool> GravarUsuarioNoLegado_Esol(string pessoaProviderId, string login, string senha)
        {
            return await _esolutionProvider.GravarUsuarioNoLegado(pessoaProviderId, login, senha);
        }

        public async Task<bool> AlterarSenhaNoLegado_Esol(string pessoaProviderId, string login, string senha)
        {
            return await _esolutionProvider.AlterarSenhaNoLegado(pessoaProviderId, login, senha);
        }

        public async Task<bool> IsDefault_Esol()
        {
            return await _esolutionProvider.IsDefault();
        }

        public async Task GravarVinculoUsuario_Esol(IAccessValidateResultModel result, Usuario usuario)
        {
            await _esolutionProvider.GravarVinculoUsuario(result, usuario);
        }

        public async Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_Esol(string pessoaProviderId)
        {
            return await _esolutionProvider.GetOutrosDadosPessoaProvider(pessoaProviderId);
        }

        public async Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_Esol(int id)
        {
            return await _esolutionProvider.GetEmpresaVinculadaLegado(id);
        }

        public async Task<List<PaisModel>> GetPaisesLegado_Esol()
        {
            return await _esolutionProvider.GetPaisesLegado();
        }

        public async Task<List<EstadoModel>> GetEstadosLegado_Esol()
        {
            return await _esolutionProvider.GetEstadosLegado();
        }

        public async Task<List<CidadeModel>> GetCidade_Esol()
        {
            return await _esolutionProvider.GetCidade();
        }

        public async Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_Esol()
        {
            return await _esolutionProvider.GetUsuariosAtivosSistemaLegado();
        }

        public async Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_Esol(ParametroSistemaViewModel parametroSistema)
        {
            return await _esolutionProvider.GetClientesUsuariosLegado(parametroSistema);
        }

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_Esol(CidadeSearchModel searchModel)
        {
            return await _esolutionProvider.SearchCidade(searchModel);
        }

        public async Task<bool> DesativarUsuariosSemCotaOuContrato_Esol()
        {
            return await _esolutionProvider.DesativarUsuariosSemCotaOuContrato();
        }

        public async Task GetOutrosDadosUsuario_Esol(TokenResultModel userReturn)
        {
            await _esolutionProvider.GetOutrosDadosUsuario(userReturn);
        }

        public async Task<List<DadosContratoModel>?> GetContratos_Esol(List<int> pessoasPesquisar)
        {
            return await _esolutionProvider.GetContratos(pessoasPesquisar);
        }

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_Esol(List<string> empresasIds)
        {
            return await _esolutionProvider.GetEmpresasVinculadas(empresasIds);
        }

        public async Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol()
        {
            return await _esolutionProvider.GetUsuariosClientesSemCotasAtivoasNoSistemaLegado();
        }

        public async Task<List<ClientesInadimplentes>> Inadimplentes_Esol(List<int>? pessoasPesquisar = null)
        {
            return await _esolutionProvider.Inadimplentes(pessoasPesquisar);
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(DateTime checkInDate, bool simulacao = false)
        {
            return await _esolutionProvider.GetReservasWithCheckInDateMultiPropriedadeAsync(checkInDate, simulacao);
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_Esol(DateTime checkInDate, bool simulacao = false)
        {
            return await _esolutionProvider.GetReservasWithCheckInDateTimeSharingAsync(checkInDate, simulacao);
        }

        public bool? ShouldSendEmailForReserva_Esol(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            return _esolutionProvider.ShouldSendEmailForReserva(reserva, config, contratos, inadimplentes);
        }
        #endregion
    }
}