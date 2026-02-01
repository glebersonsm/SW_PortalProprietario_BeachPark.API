using Microsoft.Extensions.Configuration;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.Tse;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Providers;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System.Net.Http.Headers;

namespace SW_PortalProprietario.Infra.Data.CommunicationProviders.Tse
{
    public class TSECommunicationProvider : ICommunicationProvider
    {

        private readonly IConfiguration _configuration;
        private readonly ICacheStore _cache;
        private readonly IRepositoryNH _repositoryNH;
        public TSECommunicationProvider(IConfiguration configuration,
            ICacheStore cache,
            IRepositoryNH repositoryNH)
        {
            _configuration = configuration;
            _cache = cache;
            _repositoryNH = repositoryNH;

            var admsAdd = (_repositoryNH.FindBySql<TseCustomerModel>("Select p.Nome, p.CpfCnpj, p.Nome as Cliente, p.Email, -1 as IdContrato, '-1' as NumeroContrato, Coalesce(p.Administrador,'N') as Administrador From " +
                "UsuarioMultiPropTemp p").Result);

            adms.AddRange(admsAdd);
        }

        public string CommunicationProviderName => "TSE";

        public string PrefixoTransacaoFinanceira => "SWPORTAL_TSE";

        readonly List<TseCustomerModel> adms = new()
            {
                new TseCustomerModel()
                {
                    CpfCnpj = "76473430172",
                    Nome = "Gleberson Simão de Moura",
                    Cliente = "Gleberson Simão de Moura",
                    Email = "glebersonsm@gmail.com",
                    IdContrato = -1,
                    NumeroContrato = "-1",
                    Administrador = "S"
                }
            };

        public async Task<IAccessValidateResultModel> ValidateAccess(string login, string senha, string pessoaProviderId = "")
        {
            IAccessValidateResultModel modelReturn = new AccessValidateResultModel() { ProviderName = CommunicationProviderName };

            if (string.IsNullOrEmpty(login))
            {
                modelReturn.Erros.Add($"Deve ser informado o login para logar pelo provider: '{CommunicationProviderName}'");
            }

            var loginComCpf_Cnpj = false;

            var apensasNumeros = Helper.ApenasNumeros(login);
            if (apensasNumeros.Length >= 11)
            {
                loginComCpf_Cnpj = Helper.IsCnpj(apensasNumeros) || Helper.IsCpf(apensasNumeros);
            }

            if (loginComCpf_Cnpj)
            {
                if (apensasNumeros.Length > 11)
                {
                    if (!Helper.IsCnpj(apensasNumeros))
                        modelReturn.Erros.Add($"O CNPJ informado: {login} não é válido");
                }
                else if (apensasNumeros.Length <= 11)
                {
                    if (!Helper.IsCpf(apensasNumeros))
                        modelReturn.Erros.Add($"O CPF informado: {login} não é válido");
                }
            }

            if (modelReturn.Erros.Count > 0)
                return await Task.FromResult(modelReturn);

            var tseClient = await GetMemberByCpf(login);

            if (tseClient == null || tseClient.Count == 0)
            {
                modelReturn.Erros.Add($"Não foi encontrado cliente correspondente no provider: {CommunicationProviderName} com os dados informados: '{login}'");
            }
            else
            {
                modelReturn.PessoaId = tseClient.First().IdPessoa;
                modelReturn.ClienteId = modelReturn.PessoaId;
            }

            return modelReturn;
        }

        private async Task<LoginTseResponse?> GetToken(bool renovarToken = false)
        {
            LoginTseResponse? token = null;
            try
            {
                LoginTseResponse? tokenCache = !renovarToken ? await _cache.GetAsync<LoginTseResponse>("Token_tse", 0, new CancellationToken()) : null;

                if (tokenCache != null && tokenCache.ValidoAte > DateTime.Now.AddMinutes(50))
                {
                    token = tokenCache;
                }

                if (token == null)
                {
                    var userName = $"{_configuration.GetValue<object>("TseConfig:User")}";
                    var passWord = $"{_configuration.GetValue<string>("TseConfig:Pass")}";
                    var loginPath = $"{_configuration.GetValue<string>("TseConfig:LoginPath")}";

                    var dict = new Dictionary<string, string>
                    {
                        { "username", userName },
                        { "password", passWord },
                        { "grant_type", "password" }
                    };

                    using var client = new HttpClient();
                    client.BaseAddress = new Uri($"{_configuration.GetValue<string>("TseConfig:ApiAddress")}");
                    var req = new HttpRequestMessage(HttpMethod.Post, loginPath) { Content = new FormUrlEncodedContent(dict) };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    HttpResponseMessage respLogin = await client.SendAsync(req);

                    string result = await respLogin.Content.ReadAsStringAsync();

                    if (respLogin.IsSuccessStatusCode)
                    {
                        var tk = System.Text.Json.JsonSerializer.Deserialize<TSE_Token>(result);

                        if (tk != null)
                        {
                            token = new LoginTseResponse()
                            {
                                Token = tk,
                                UserName = tk.UserName,
                                Exp = $"{tk.Expires}",
                                Partner = tk.UserName,
                                Type = tk.TokenType,
                                ExpireValue = $"{tk.ExpiresValue}"
                            };
                            token.DataLiberacao = DateTime.Now.AddSeconds(-30);
                            await _cache.AddAsync("Token_tse", token, DateTimeOffset.Now.AddHours(5), 0, new CancellationToken());
                        }
                    }
                }

                return token ?? new LoginTseResponse();

            }
            catch (Exception)
            {

                throw;
            }

        }

        private static async Task<IEnumerable<TseCustomerModel>> GetCustomersFromTse(ITseCommunication customerClient, string cpf)
        {
            List<TseParametro> par = new()
                    {
                        new TseParametro()
                        {
                            NomeParametro = "cpf",
                            Value = $"{cpf}",
                            Type = "String",
                            Criptografar = true
                        },
                    };

            return await customerClient.GetContractByCpf(par);
        }

        public async Task<List<TseCustomerModel>> GetMemberByCpf(string cpf)
        {
            return await GetMemberByCpfExecute(cpf);

        }

        private async Task<List<TseCustomerModel>> GetMemberByCpfExecute(string cpf)
        {
            try
            {
                List<TseCustomerModel> listResult = new();
                var adm = adms.FirstOrDefault(a => a.CpfCnpj is not null && a.CpfCnpj.Replace(".", "").Replace("-", "") == cpf.Replace(".", "").Replace("-", ""));
                if (adm != null)
                    return await Task.FromResult(new List<TseCustomerModel>() { adm });
                else
                {
                    var token = await GetToken();

                    //if (token != null && token.Token != null && !string.IsNullOrEmpty(token.Token?.AccessToken))
                    //{
                    //    var customerClient = RestService.For<ITseCommunication>($"{_configuration.GetValue<string>("TSEConfig:ApiAddress")}", new RefitSettings()
                    //    {
                    //        AuthorizationHeaderValueGetter = () => Task.FromResult(token.Token?.AccessToken)
                    //    });

                    //    var result = (await GetCustomersFromTse(customerClient, cpf.Replace(".", "").Replace("-", ""))).ToList();
                    //    if (result != null && result.Count > 0)
                    //    {
                    //        listResult.AddRange(result);
                    //    }

                    //}
                }

                return listResult;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> IsDefault()
        {
            return await Task.FromResult(false);
        }

        public async Task GravarVinculoUsuario(IAccessValidateResultModel result, Usuario usuario)
        {
            if (usuario == null) throw new Exception("A propriedade user deve ser informada!");
            if (result == null) throw new Exception("A propriedade accessValidateResult deve ser informada!");
            throw new NotImplementedException("Não foi implementada a gravação do vínculo do usuário com o sistema TSE");

            //if (!string.IsNullOrEmpty(result?.PessoaId))
            //    usuario.ProviderChaveUsuario = $"PessoaId:{result?.PessoaId}";

            //if (result?.UsuarioSistema.GetValueOrDefault(0) > 0)
            //{
            //    if (!string.IsNullOrEmpty(usuario.ProviderChaveUsuario))
            //        usuario.ProviderChaveUsuario += $"|UsuarioId:{result?.UsuarioSistema.GetValueOrDefault()}";
            //    else usuario.ProviderChaveUsuario += $"UsuarioId:{result?.UsuarioSistema.GetValueOrDefault()}";
            //}
            //await Task.CompletedTask;
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario(SearchContasPendentesUsuarioLogado searchModel)
        {
            return await Task.FromResult((-1, -1, new List<ContaPendenteModel>()));
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis(SearchImovelModel searchModel)
        {
            return await Task.FromResult((-1, -1, new List<ImovelSimplificadoModel>()));
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios(SearchProprietarioModel searchModel)
        {
            return await Task.FromResult((-1, -1, new List<ProprietarioSimplificadoModel>()));
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral(SearchContasPendentesGeral searchModel)
        {
            return await Task.FromResult((-1, -1, new List<ContaPendenteModel>()));
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa(int pessoaId)
        {
            return await Task.FromResult(new PessoaParaTransacaoBrokerModel());
        }

        public async Task<List<ContaPendenteModel>> GetContasPorIds(List<int> itensToPay)
        {
            return await Task.FromResult(new List<ContaPendenteModel>());
        }

        public async Task<List<CotaPeriodoModel>> GetCotaPeriodo(int pessoaId, DateTime? dataInicial, DateTime? dataFinal)
        {
            return await Task.FromResult(new List<CotaPeriodoModel>());
        }

        public async Task<List<CotaPeriodoModel>> ProprietarioNoPoolHoje(int pessoaId)
        {
            return await Task.FromResult(new List<CotaPeriodoModel>());
        }

        public async Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail(bool pool, bool naoPool)
        {
            return await Task.FromResult(new List<CotaPeriodoModel>());
        }

        public async Task<BoletoModel> DownloadBoleto(DownloadBoleto model)
        {
            return await Task.FromResult(new BoletoModel());
        }

        public async Task<LoginResult> GetToken(LoginRequest request)
        {
            return await Task.FromResult(new LoginResult());
        }

        public async Task<bool> GravarUsuarioNoLegado(string pessoaProviderId, string login, string senha)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> AlterarSenhaNoLegado(string pessoaProviderId, string login, string senha)
        {
            return await Task.FromResult(true);

        }

        public async Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider(string pessoaProviderId)
        {
            return await Task.FromResult(new VinculoAccessXPortalBase());
        }

        public async Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado(int id)
        {
            return await Task.FromResult(new EmpresaSimplificadaModel { Id = id });
        }

        public async Task<List<PaisModel>> GetPaisesLegado()
        {
            return await Task.FromResult(new List<PaisModel>());
        }

        public async Task<List<EstadoModel>> GetEstadosLegado()
        {
            return await Task.FromResult(new List<EstadoModel>());
        }

        public async Task<List<CidadeModel>> GetCidade()
        {
            return await Task.FromResult(new List<CidadeModel>());
        }

        public Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade(CidadeSearchModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado()
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado(ParametroSistemaViewModel parametroSistema)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DesativarUsuariosSemCotaOuContrato()
        {
            throw new NotImplementedException();
        }

        public Task GetOutrosDadosUsuario(TokenResultModel userReturn)
        {
            throw new NotImplementedException();
        }

        public Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(List<string> empresasIds)
        {
            throw new NotImplementedException();
        }

        public Task<UsuarioValidateResultModel> GerUserFromLegado(UserRegisterInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInputModel>> GetUsuariosCotasCanceladasSistemaLegado()
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado()
        {
            throw new NotImplementedException();
        }

        public Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar)
        {
            throw new NotImplementedException();
        }

        public Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException();
        }

        public bool? ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            throw new NotImplementedException();
        }
    }
}
