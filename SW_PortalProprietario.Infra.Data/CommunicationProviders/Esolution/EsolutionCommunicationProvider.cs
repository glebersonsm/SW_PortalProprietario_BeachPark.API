using AccessCenterDomain.AccessCenter;
using AccessCenterDomain.AccessCenter.Fractional;
using CMDomain.Models.AuthModels;
using Dapper;
using FluentNHibernate.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Providers;
using SW_PortalProprietario.Application.Services.Providers.Esolution;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Infra.Data.Repositories.Core;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SW_PortalProprietario.Infra.Data.CommunicationProviders.Esolution
{
    public class EsolutionCommunicationProvider : ICommunicationProvider
    {
        private const string PREFIXO_TRANSACOES_FINANCEIRAS = "PORTALPROPESOL_";
        private const string CACHE_CLIENTES_INADIMPLENTES_KEY = "ClientesInadimplentesMP_";
        private const string CACHE_CONTRATOSSCP = "ContratosSCP";
        

        private readonly IConfiguration _configuration;
        private readonly IRepositoryNHAccessCenter _repositoryAccessCenter;
        private readonly IRepositoryNHEsolPortal _repositoryPortalEsol;
        private readonly IRepositoryNH _systemRepository;
        private readonly ITokenBodyService _tokenBodyService;
        private readonly ILogger<EsolutionCommunicationProvider> _logger;
        private readonly ICacheStore _cacheStore;
        private const string CONTRATO_PESSOA_KEY = "PESSOA_{PESSOAID}";

        public string CommunicationProviderName => "EsolutionProvider";

        public string PrefixoTransacaoFinanceira => PREFIXO_TRANSACOES_FINANCEIRAS;

        public EsolutionCommunicationProvider(IConfiguration configuration,
             IRepositoryNHAccessCenter repositoryAccessCenter,
             ITokenBodyService tokenBodyService,
             IRepositoryNHEsolPortal repositoryPortalEsol,
             IRepositoryNH systemRepository,
             ILogger<EsolutionCommunicationProvider> logger,
             ICacheStore cacheStore)
        {
            _configuration = configuration;
            _repositoryAccessCenter = repositoryAccessCenter;
            _tokenBodyService = tokenBodyService;
            _repositoryPortalEsol = repositoryPortalEsol;
            _systemRepository = systemRepository;
            _logger = logger;
            _cacheStore = cacheStore;
        }

        public async Task<IAccessValidateResultModel> ValidateAccess(string login, string senha, string pessoaProviderId = "")
        {
            AccessValidateResultModel modelReturn = new AccessValidateResultModel()
            {
                ProviderName = CommunicationProviderName
            };

            if (string.IsNullOrEmpty(login))
            {
                modelReturn.Erros.Add($"Deve ser informado o login para logar pelo provider: '{CommunicationProviderName}'");
                return modelReturn;
            }

            var loginBySoFaltaEu = _configuration.GetValue<bool>("ControleDeUsuarioViaSFE", false);
            var loginByAccessCenter = _configuration.GetValue<bool>("ControleDeUsuarioViaAccessCenter", false);

            LoginResult? lr = null;

            if (loginBySoFaltaEu)
                lr = await ValidarLoginSoFaltaEu(login, senha, pessoaProviderId);
            else if (loginByAccessCenter)
                lr = await ValidarLoginAccessCenter(login, senha, pessoaProviderId);


            modelReturn.LoginResult = lr;

            if (lr != null && !string.IsNullOrEmpty(lr.message))
                modelReturn.Erros.Add(lr.message);

            return modelReturn;
        }


        private async Task<LoginResult> ValidarLoginAccessCenter(string login, string senha, string pessoaPoviderId = "")
        {
            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);
            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");

            var emprendimento = (await _repositoryAccessCenter.FindBySql<Empreendimento>($"Select e.* From Empreendimento e Where e.Id in ({empreendimentoId})")).AsList();
            if (emprendimento == null || emprendimento.Count() == 0)
                throw new ArgumentException($"Não foi encontrado empreendimento com Id's: {empreendimentoId}");

            var ignorarValidacaoLogin = _configuration.GetValue<bool>("IgnorarValidacaoLogin");


            var loginResult = new LoginResult();

            bool cpf = false;
            bool cnpj = false;

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
                        loginResult.message = $"O CNPJ informado: {login} não é válido";
                }
                else if (apensasNumeros.Length <= 11)
                {
                    if (!Helper.IsCpf(apensasNumeros))
                        loginResult.message = $"O CPF informado: {login} não é válido";
                }
            }



            if (!string.IsNullOrEmpty(loginResult.message))
                return await Task.FromResult(loginResult);

            AccessCenterDomain.AccessCenter.Pessoa? pessoaEsolution = null;


            if (!string.IsNullOrEmpty(pessoaPoviderId))
            {
                pessoaEsolution = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                                        p.* 
                                                    From 
                                                       Pessoa p
                                                        Inner Join FrPessoa fp1 on fp1.Pessoa = p.Id
                                                        Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp1.Id and av.Status = 'A' 
                                                        Inner Join Filial f on av.Filial = f.Id
                                                    Where
                                                        p.Id = {pessoaPoviderId}")).FirstOrDefault();

                if (pessoaEsolution == null)
                {
                    var pessoaPeloId = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                                        p.* 
                                                    From 
                                                       Pessoa p
                                                    Where
                                                        p.Id = {pessoaPoviderId}")).FirstOrDefault();


                    if (pessoaPeloId != null && !string.IsNullOrEmpty(pessoaPeloId.Passaporte))
                    {
                        pessoaEsolution = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                                        p.* 
                                                    From 
                                                       Pessoa p
                                                        Inner Join FrPessoa fp1 on fp1.Pessoa = p.Id
                                                        Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp1.Id and av.Status = 'A' 
                                                        Inner Join Filial f on av.Filial = f.Id
                                                    Where
                                                        Lower(p.Nome) = '{pessoaPeloId.Nome.ToLower()}' and 
                                                        p.Passaporte = '{pessoaPeloId.Passaporte}' ")).FirstOrDefault();

                        if (pessoaEsolution == null && !pessoaPeloId.Nascimento.HasValue)
                        {
                            pessoaEsolution = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                                        p.* 
                                                    From 
                                                       Pessoa p
                                                        Inner Join FrPessoa fp1 on fp1.Pessoa = p.Id
                                                        Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp1.Id and av.Status = 'A' 
                                                        Inner Join Filial f on av.Filial = f.Id
                                                    Where
                                                        Lower(p.Nome) = '{pessoaPeloId.Nome.ToLower()}' and p.Nascimento = :nascimento",
                                                        new Parameter("nascimento",pessoaPeloId.Nascimento.GetValueOrDefault().Date))).FirstOrDefault();
                        }
                    }

                    loginResult.message = $"Usuário não encontrado";
                    return await Task.FromResult(loginResult);
                }
            }

            if (pessoaEsolution == null)
            {
                string documentoFormatado = "";
                if (cpf)
                {
                    documentoFormatado = Helper.FormatarCPF(Convert.ToInt64(apensasNumeros));
                }
                else if (cnpj)
                {
                    documentoFormatado = Helper.FormatarCNPJ(Convert.ToInt64(apensasNumeros));
                }

                if (cpf || cnpj)
                    pessoaEsolution = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                                        p.* 
                                                    From 
                                                        Pessoa p
                                                        Inner Join FrPessoa fp1 on fp1.Pessoa = p.Id
                                                        Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp1.Id and av.Status = 'A'
                                                        Inner Join Filial f on av.Filial = f.Id and f.Empresa in ({string.Join(",",emprendimento.Select(b=> b.Empresa.GetValueOrDefault()))})
                                                        Inner Join DocumentoRegistro dr on dr.Pessoa = p.Id
                                                        Inner Join TipoDocumentoRegistro tdr on dr.TipoDocumentoRegistro = tdr.Id
                                                    Where
                                                        (
                                                            (
                                                                (dr.DocumentoNumerico = '{Convert.ToInt64(apensasNumeros)}' or 
                                                                p.CPF = {Convert.ToInt64(apensasNumeros)} or 
                                                                p.CNPJ = {Convert.ToInt64(apensasNumeros)} or 
                                                                dr.DocumentoAlfanumerico = '{documentoFormatado}'
                                                                ) 
                                                                and Lower(tdr.Nome) in ('cpf','cnpj')
                                                             ) 
                                                        )")).FirstOrDefault();


            }


            if (pessoaEsolution == null)
            {
                loginResult.message = "Usuário não encontrado";
                return await Task.FromResult(loginResult);
            }


            var cotas = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Cota>($@"Select 
                    c.*,
                    Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                From 
                    Cota c
                    Inner Join FrAtendimentoVenda av on av.Cota = c.Id and av.Status = 'A'
                    Inner Join FrPessoa fp1 on av.FrPessoa1 = fp1.Id
                    Inner Join Pessoa p on fp1.Pessoa = p.Id
                    LEFT JOIN Imovel i on c.Imovel = i.Id
                    LEFT JOIN TipoImovel ti on i.TipoImovel = ti.Id
                Where 
                    p.Id = {pessoaEsolution.Id} ")).AsList();

            if (cotas.Count == 0 && pessoaEsolution == null)
            {
                loginResult.message = "Usuário ou senha inválidos";
            }

            if (!aplicarPadraoBlack && cotas.Any())
            {
                foreach (var cota in cotas)
                {
                   cota.PadraoDeCor = "Default";
                }
            }

            string strTelefoneUtilizar = "";

            var pessoaTelefones = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.PessoaTelefone>($"Select pt.* From PessoaTelefone pt Where pt.Pessoa = {pessoaEsolution.Id}")).AsList();
            if (pessoaTelefones.Any())
            {
                var telefoneUtilizar = pessoaTelefones.FirstOrDefault(a => a.Preferencial == "S") ?? pessoaTelefones.First();
                strTelefoneUtilizar = telefoneUtilizar.Numero;
            }

            loginResult.dadosCliente = new DadosClienteLegado()
            {
                Nome = pessoaEsolution.Nome!.Split(' ')[0],
                SobreNome = pessoaEsolution.Nome.Replace(pessoaEsolution.Nome.Split(' ')[0], "").TrimStart(' '),
                Permissao = "cliente",
                Plataforma = "AccessCenter",
                Empreendimento = string.Join(",",emprendimento.Select(a=> a.Nome)),
                Telefone = strTelefoneUtilizar,
                Email = pessoaEsolution.eMail,
                GrupoEmpresa = $"-",
                PessoaId = $"{pessoaEsolution.Id}",
                Identificacao = $"{pessoaEsolution.CPF}" ?? pessoaEsolution.eMail ?? $"{pessoaEsolution.Id}"
            };

            if (pessoaEsolution.Tipo == "F" && pessoaEsolution.CPF.GetValueOrDefault(0) > 0)
            {
                loginResult.dadosCliente.Cpf = Helper.FormatarCPF(pessoaEsolution.CPF.GetValueOrDefault());
            }
            else if (pessoaEsolution.Tipo == "J" && pessoaEsolution.Cnpj.GetValueOrDefault(0) > 0)
            {
                loginResult.dadosCliente.Cnpj = Helper.FormatarCNPJ(pessoaEsolution.Cnpj.GetValueOrDefault());
            }

            if (string.IsNullOrEmpty(loginResult.dadosCliente.Identificacao))
            {
                loginResult.dadosCliente.Identificacao = pessoaEsolution.Tipo == "F" ? loginResult.dadosCliente.Cpf : loginResult.dadosCliente.Cnpj;
            }

            loginResult.dadosCliente.Usuario = pessoaEsolution.CPF.HasValue ? $"{pessoaEsolution.CPF}" : pessoaEsolution.eMail;
            if (string.IsNullOrEmpty(loginResult.dadosCliente.Usuario))
                loginResult.dadosCliente.Usuario = Helper.ApenasNumeros(loginResult.dadosCliente.Identificacao);

            var enderecosCliente = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.PessoaEndereco>($@"From 
                                                                                        PessoaEndereco pe 
                                                                                        Where 
                                                                                        pe.Pessoa = {pessoaEsolution.Id}")).AsList();

            if (enderecosCliente != null && enderecosCliente.Any())
            {
                var enderecoFst = enderecosCliente.FirstOrDefault(a => a.Preferencial == "S") ?? enderecosCliente.First();
                var cidade = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Cidade>($"From Cidade c Where c.Id = {enderecoFst.Cidade.GetValueOrDefault()}")).FirstOrDefault();
                var estado = cidade != null && cidade.Estado.GetValueOrDefault(0) > 0 ? (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Estado>($"From Estado e Where e.Id = {cidade.Estado.GetValueOrDefault()}")).FirstOrDefault() : null;
                if (enderecoFst != null && cidade != null)
                {
                    if (estado != null)
                    {
                        loginResult.dadosCliente.Endereco = $"Logradouro: {enderecoFst.Logradouro} - Bairro: {enderecoFst.Bairro} - Cidade: {cidade.Nome}/{estado.Uf} - CEP: {enderecoFst.Cep}";
                    }
                    else
                    {
                        if (enderecoFst.Cidade != null)
                            loginResult.dadosCliente.Endereco = $"Logradouro: {enderecoFst.Logradouro} - Bairro: {enderecoFst.Bairro} - Cidade: {cidade.Nome} - CEP: {enderecoFst.Cep}";
                        else
                            loginResult.dadosCliente.Endereco = $"Logradouro: {enderecoFst.Logradouro} - Bairro: {enderecoFst.Bairro} - CEP: {enderecoFst.Cep}";
                    }
                }
            }
            loginResult.dadosCliente.AccessToten = "";

            return await Task.FromResult(loginResult);
        }

        private async Task<LoginResult?> ValidarLoginSoFaltaEu(string login, string senha, string pessoaProviderId = "")
        {

            var ignorarValidacaoLogin = _configuration.GetValue<bool>("IgnorarValidacaoLogin");
            var loginResult = await GetToken(new LoginRequest()
            {
                usuario = login,
                senha = senha
            });

            return await Task.FromResult(loginResult);
        }

        public async Task GravarVinculoUsuario(IAccessValidateResultModel result, Domain.Entities.Core.Sistema.Usuario usuario)
        {
            if (usuario == null) throw new Exception("A propriedade user deve ser informada!");
            if (result == null) throw new Exception("A propriedade accessValidateResult deve ser informada!");

            if (!string.IsNullOrEmpty(result?.PessoaId))
                usuario.ProviderChaveUsuario = $"PessoaId:{result?.PessoaId}";

            if (result?.UsuarioSistema.GetValueOrDefault(0) > 0)
            {
                if (!string.IsNullOrEmpty(usuario.ProviderChaveUsuario))
                    usuario.ProviderChaveUsuario += $"|UsuarioId:{result?.UsuarioSistema.GetValueOrDefault()}";
                else usuario.ProviderChaveUsuario += $"UsuarioId:{result?.UsuarioSistema.GetValueOrDefault()}";
            }
            await Task.CompletedTask;
        }

        private async Task<LoginResult?> GetToken(LoginRequest request)
        {
            var ignorarValidacaoLogin = _configuration.GetValue<bool>("IgnorarValidacaoLogin");
            var empresaId = _configuration.GetValue<int>("Empresa", 0);
            if (empresaId == 0)
                throw new ArgumentException("Empresa não configurada");

            if (request != null && !string.IsNullOrEmpty(request.usuario) && request.usuario.Contains(".") && !request.usuario.Contains("@"))
                request.usuario = Helper.RemoverAcentuacaoCpfCnpj(request.usuario);

            var urlLogin = _configuration.GetValue<string>("SoFaltaEuUrl");

            var json = JsonSerializer.Serialize(request);

            using var httpClient = new HttpClient();
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            var resposta = await httpClient.PostAsync(urlLogin, conteudo);

            if (resposta.IsSuccessStatusCode)
            {
                var conteudoResposta = await resposta.Content.ReadAsStringAsync();
                return await GetDadosResult(empresaId, conteudoResposta);
            }
            else
            {
                if (ignorarValidacaoLogin)
                {
                    return await GetDadosResult(empresaId, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c3VhcmlvIjoidGFib3JkYWJyYXZhbXVuZG8iLCJjcGYiOm51bGwsIm5vbWUiOiJUQUJPUkRBIEdFU1RBTyIsInNvYnJlbm9tZSI6IkUgQURNSU5JU1RSQUNBTyBJTU9CSUxJQVJJQSBFSVJFTEkiLCJwZXJtaXNzYW8iOiJjbGllbnRlIiwicGxhdGFmb3JtYSI6InRyaXAiLCJlbXByZWVuZGltZW50byI6ImVzb2x1dGlvbiIsInRlbGVmb25lIjoiKzU1NDc5OTY3NDAwMzciLCJlbWFpbCI6InRhYm9yZGEuY29uc3VsdG9yaWFAZ21haWwuY29tIiwiZ3J1cG9lbXByZXNhIjoiZXNvbHV0aW9uIiwiaWRlbnRpZmljYWNhbyI6IjE5Nzk0NTIzMDAwMTg1IiwiaWF0IjoxNzIzNzM1NzQwLCJleHAiOjE3MjYzMjc3NDB9.wLVXLAT5q2Qe-uDmp-PwzFNG9JKbKhlk1pFrUL5A5W4");
                }

                string conteudoResposta = await resposta.Content.ReadAsStringAsync();
                var resultLogin = JsonSerializer.Deserialize<LoginResult>(conteudoResposta);

                return resultLogin;
            }
        }

        private async Task<LoginResult?> GetDadosResult(int empresaId, string resposta, bool ignorarValidacaoLogin = false, int pessoaId = 0)
        {
            var resultLogin = JsonSerializer.Deserialize<LoginResult>(resposta);
            if (resultLogin != null)
            {
                resultLogin.code = 200;

                var bodyToken = _tokenBodyService.GetBodyToken(resultLogin.token);
                var tokenObjeto = ObterDadosToken(bodyToken);

                var sb = new StringBuilder(@$"Select 
                                            c.* 
                                            From 
                                            Cota c 
                                            Inner Join Imovel i on c.Imovel = i.Id
                                            Inner Join Empreendimento emp on i.Empreendimento = emp.Id
                                            Inner Join cliente cli on c.Proprietario = cli.Id 
                                            Inner Join Pessoa p on cli.Pessoa = p.Id
                                            Where cli.Empresa = {empresaId} and 
                                            c.Status = 'V' ");

                if (pessoaId == 0)
                {
                    if (!string.IsNullOrEmpty(tokenObjeto.Identificacao))
                    {
                        var apenasNumeros = Helper.RemoverAcentuacaoCpfCnpj(tokenObjeto.Identificacao);
                        sb.AppendLine($" and (p.Cpf = {apenasNumeros} or p.Cnpj = {apenasNumeros}) ");
                    }
                    else if (!string.IsNullOrEmpty(tokenObjeto.Cpf))
                    {
                        var apenasNumeros = Helper.RemoverAcentuacaoCpfCnpj(tokenObjeto.Cpf);
                        sb.AppendLine($" and (p.Cpf = {apenasNumeros} or p.Cnpj = {apenasNumeros}) ");
                    }
                    else if (!string.IsNullOrEmpty(tokenObjeto.Email) && tokenObjeto.Email.Contains(""))
                    {
                        sb.AppendLine($" and Lower(p.Email) = '{tokenObjeto.Email.ToLower()}' ");
                    }
                }
                else
                {
                    sb.AppendLine($" and p.Id = {pessoaId} ");
                }

                var cotasDoUsuario = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Cota>(sb.ToString())).AsList();
                if (cotasDoUsuario == null || !cotasDoUsuario.Any())
                {
                    return resultLogin = new LoginResult()
                    {
                        code = 401,
                        token = null,
                        message = "Usuário não encontrado"
                    };
                }

                resultLogin.dadosCliente = tokenObjeto;




                var pessoa = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = (Select c.Pessoa From Cliente c Where c.Id = {cotasDoUsuario.First().Proprietario.GetValueOrDefault()} and c.Pessoa = p.Id)")).FirstOrDefault();
                if (pessoa != null)
                {
                    if (pessoa.Tipo == "F")
                        resultLogin.dadosCliente.Cpf = pessoa.CPF.GetValueOrDefault(0) > 0 ? Helper.FormatarCPF(pessoa.CPF.GetValueOrDefault()) : "";
                    else if (pessoa.Tipo == "J")
                        resultLogin.dadosCliente.Cnpj = pessoa.Cnpj.GetValueOrDefault(0) > 0 ? Helper.FormatarCNPJ(pessoa.Cnpj.GetValueOrDefault()) : "";

                    tokenObjeto.PessoaId = $"{pessoa.Id}";
                }

                var enderecosCliente = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.PessoaEndereco>($@"From 
                                                                                        PessoaEndereco pe 
                                                                                        Where 
                                                                                        pe.Pessoa = (Select c.Pessoa From Cliente c Where c.Id = {cotasDoUsuario.First().Proprietario.GetValueOrDefault()} and c.Pessoa = pe.Pessoa)")).AsList();

                if (enderecosCliente != null && enderecosCliente.Any())
                {
                    var enderecoFst = enderecosCliente.FirstOrDefault(a => a.Preferencial == "S") ?? enderecosCliente.First();
                    var cidade = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Cidade>($"From Cidade c Where c.Id = {enderecoFst.Cidade.GetValueOrDefault()}")).FirstOrDefault();
                    var estado = cidade != null && cidade.Estado.GetValueOrDefault(0) > 0 ? (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Estado>($"From Estado e Where e.Id = {cidade.Estado.GetValueOrDefault()}")).FirstOrDefault() : null;
                    if (enderecoFst != null && cidade != null)
                    {
                        if (estado != null)
                        {
                            resultLogin.dadosCliente.Endereco = $"Logradouro: {enderecoFst.Logradouro} - Bairro: {enderecoFst.Bairro} - Cidade: {cidade.Nome}/{estado.Uf} - CEP: {enderecoFst.Cep}";
                        }
                        else
                        {
                            if (enderecoFst.Cidade != null)
                                resultLogin.dadosCliente.Endereco = $"Logradouro: {enderecoFst.Logradouro} - Bairro: {enderecoFst.Bairro} - Cidade: {cidade.Nome} - CEP: {enderecoFst.Cep}";
                            else
                                resultLogin.dadosCliente.Endereco = $"Logradouro: {enderecoFst.Logradouro} - Bairro: {enderecoFst.Bairro} - CEP: {enderecoFst.Cep}";
                        }
                    }
                }
                resultLogin.dadosCliente.AccessToten = resultLogin.token;


                if (ignorarValidacaoLogin && pessoaId > 0 && pessoa != null && (pessoa.CPF.GetValueOrDefault(0) > 0 || pessoa.Cnpj.GetValueOrDefault(0) > 0))
                {
                    resultLogin.dadosCliente.Usuario = $"{(pessoa != null && pessoa.CPF.GetValueOrDefault(0) > 0 ? pessoa.CPF : pessoa?.Cnpj.GetValueOrDefault(0))}";
                    if (!string.IsNullOrEmpty(pessoa?.eMail))
                    {
                        resultLogin.dadosCliente.Email = pessoa.eMail;
                        resultLogin.dadosCliente.Telefone = "(XX) XXXXX-XXXX";
                        resultLogin.dadosCliente.Identificacao = resultLogin.dadosCliente.Usuario;
                    }
                }
            }

            return await Task.FromResult(resultLogin);
        }

        public async Task<bool> IsDefault()
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> GravarUsuarioNoLegado(string pessoaProviderId, string login, string senha)
        {
            try
            {
                _repositoryAccessCenter.BeginTransaction();
                var empresaId = _configuration.GetValue<int>("Empresa", 0);
                if (empresaId == 0)
                    throw new ArgumentException("Empresa não configurada");

                var empresa = (await _repositoryAccessCenter.FindByHql<Empresa>($"From Empresa emp Where emp.id = {empresaId}")).FirstOrDefault();
                if (empresa == null)
                    throw new ArgumentException($"Não foi encontrada empresa com Id: {empresaId}");


                var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");

                var emprendimento = (await _repositoryAccessCenter.FindBySql<Empreendimento>($"Select e.* From Empreendimento e Where e.Id in ({empreendimentoId})")).FirstOrDefault();
                if (emprendimento == null)
                    throw new ArgumentException($"Não foi encontrado empreendimento com Id: {empreendimentoId}");

                var cliente = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Cliente>($"From Cliente cli Where cli.Pessoa = {pessoaProviderId} and cli.Empresa = {empresaId}")).FirstOrDefault();
                if (cliente != null)
                {
                    cliente.CondominioSenha = senha;
                    cliente.CondominioUsuario = login;
                    await _repositoryAccessCenter.Save(cliente);
                }

                var commitResult = _repositoryAccessCenter.CommitAsync();
                if (commitResult.Exception != null)
                    throw commitResult.Exception;

                return true;

            }
            catch (Exception)
            {
                _repositoryAccessCenter.Rollback();
                throw;
            }

        }

        public async Task<bool> AlterarSenhaNoLegado(string pessoaProviderId, string login, string senha)
        {
            try
            {
                _repositoryAccessCenter.BeginTransaction();
                var empresaId = _configuration.GetValue<int>("Empresa", 0);
                if (empresaId == 0)
                    throw new ArgumentException("Empresa não configurada");

                var empresa = (await _repositoryAccessCenter.FindByHql<Empresa>($"From Empresa emp Where emp.id = {empresaId}")).FirstOrDefault();
                if (empresa == null)
                    throw new ArgumentException($"Não foi encontrada empresa com Id: {empresaId}");


                var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");

                var emprendimento = (await _repositoryAccessCenter.FindBySql<Empreendimento>($"Select e.* From Empreendimento e Where e.Id in ({empreendimentoId})")).FirstOrDefault();
                if (emprendimento == null)
                    throw new ArgumentException($"Não foi encontrado empreendimento com Id: {empreendimentoId}");

                var cliente = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Cliente>($"From Cliente cli Where cli.Pessoa = {pessoaProviderId} and cli.Empresa = {empresaId}")).FirstOrDefault();
                if (cliente != null)
                {
                    cliente.CondominioSenha = senha;
                    cliente.CondominioUsuario = login;
                    await _repositoryAccessCenter.Save(cliente);
                }

                var commitResult = _repositoryAccessCenter.CommitAsync();
                if (commitResult.Exception != null)
                    throw commitResult.Exception;

                return true;

            }
            catch (Exception)
            {
                _repositoryAccessCenter.Rollback();
                throw;
            }

        }

        private static DadosClienteLegado ObterDadosToken(Dictionary<string, object> body)
        {
            var retornoSoFaltaEu = new DadosClienteLegado();

            foreach (var item in body)
            {
                switch (item.Key)
                {
                    case "usuario":
                        retornoSoFaltaEu.Usuario = (string)item.Value;
                        break;
                    case "cpf":
                        retornoSoFaltaEu.Cpf = (string)item.Value;
                        break;
                    case "nome":
                        retornoSoFaltaEu.Nome = (string)item.Value;
                        break;
                    case "sobrenome":
                        retornoSoFaltaEu.SobreNome = (string)item.Value;
                        break;
                    case "permissao":
                        retornoSoFaltaEu.Permissao = (string)item.Value;
                        break;
                    case "plataforma":
                        retornoSoFaltaEu.Plataforma = (string)item.Value;
                        break;
                    case "empreendimento":
                        retornoSoFaltaEu.Empreendimento = (string)item.Value;
                        break;
                    case "telefone":
                        retornoSoFaltaEu.Telefone = (string)item.Value;
                        break;
                    case "email":
                        retornoSoFaltaEu.Email = (string)item.Value;
                        break;
                    case "grupoempresa":
                        retornoSoFaltaEu.GrupoEmpresa = (string)item.Value;
                        break;
                    case "identificacao":
                        retornoSoFaltaEu.Identificacao = (string)item.Value;
                        break;
                    case "accesstoten":
                        retornoSoFaltaEu.AccessToten = (string)item.Value;
                        break;
                    default:
                        break;
                }
            }

            return retornoSoFaltaEu;
        }

        public async Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider(string pessoaProviderId)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);
            VinculoAccessXPortalBase? vinculo = null;

            var pessoaProviderOriginal = pessoaProviderId;

            var contratoPessoaAtualAtivo = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                            p.*     
                                        From 
                                            Pessoa p Inner Join FrPessoa fp on fp.Pessoa = p.Id 
                                            Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp.Id 
                                        Where 
                                            p.Id = {pessoaProviderId} and av.Status = 'A'")).FirstOrDefault();


            var pessoProviderObject = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($"Select p.* From Pessoa p Where p.Id = {pessoaProviderId}")).FirstOrDefault();
            if (pessoProviderObject != null)
            {
                if (contratoPessoaAtualAtivo == null)
                {
                    if (pessoProviderObject.CPF.GetValueOrDefault(0) > 0)
                    {
                        var outrasPessoas = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                            p.*     
                                        From 
                                            Pessoa p Inner Join FrPessoa fp on fp.Pessoa = p.Id 
                                            Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp.Id 
                                        Where 
                                            p.CPF = {pessoProviderObject.CPF} and p.Id <> {pessoaProviderId} and av.Status = 'A'")).AsList();

                        if (contratoPessoaAtualAtivo == null && outrasPessoas.Any())
                        {
                            var pessoaContratoAtivoAtual = outrasPessoas.FirstOrDefault(a => !string.IsNullOrEmpty(a.Nome) && a.Nome.Contains(pessoProviderObject.Nome!));
                            if (pessoaContratoAtivoAtual != null)
                            {
                                var dadosPessoaProvider = (await _systemRepository.FindByHql<PessoaSistemaXProvider>(@$"
                                    From 
                                    PessoaSistemaXProvider a
                                    Where 1 = 1 and 
                                            a.PessoaProvider = '{pessoaProviderId}'")).FirstOrDefault();

                                if (dadosPessoaProvider != null)
                                {
                                    dadosPessoaProvider.PessoaProvider = pessoaContratoAtivoAtual.Id.ToString();
                                    pessoaProviderId = pessoaContratoAtivoAtual.Id.ToString();
                                    var usuario = (await _systemRepository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Where u.Pessoa = {dadosPessoaProvider.PessoaSistema} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                                    if (usuario != null)
                                    {
                                        //PessoaId:195001|UsuarioId:9784
                                        if (!string.IsNullOrEmpty(usuario.ProviderChaveUsuario))
                                        {
                                            usuario.ProviderChaveUsuario = usuario.ProviderChaveUsuario.Replace($"PessoaId:{pessoaProviderOriginal}|", $"PessoaId:{pessoaContratoAtivoAtual.Id.ToString()}");
                                            usuario.Status = EnumStatus.Ativo;
                                            await _systemRepository.Save(usuario);
                                        }
                                    }

                                    await _systemRepository.Save(dadosPessoaProvider);
                                }


                            }
                        }


                    }
                    else if (pessoProviderObject.Cnpj.GetValueOrDefault(0) > 0)
                    {
                        var outrasPessoas = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                            p.*     
                                        From 
                                            Pessoa p Inner Join FrPessoa fp on fp.Pessoa = p.Id 
                                            Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp.Id 
                                        Where 
                                            p.CNPJ = {pessoProviderObject.Cnpj} and p.Id <> {pessoaProviderId} and av.Status = 'A'")).AsList();

                        if (contratoPessoaAtualAtivo == null && outrasPessoas.Any())
                        {
                            var pessoaContratoAtivoAtual = outrasPessoas.FirstOrDefault(a => !string.IsNullOrEmpty(a.Nome) && a.Nome.Contains(pessoProviderObject.Nome!));
                            if (pessoaContratoAtivoAtual != null)
                            {
                                var dadosPessoaProvider = (await _systemRepository.FindByHql<PessoaSistemaXProvider>(@$"
                                    From 
                                    PessoaSistemaXProvider a
                                    Where 1 = 1 and 
                                            a.PessoaProvider = '{pessoaProviderId}'")).FirstOrDefault();

                                if (dadosPessoaProvider != null)
                                {
                                    dadosPessoaProvider.PessoaProvider = pessoaContratoAtivoAtual.Id.ToString();
                                    pessoaProviderId = pessoaContratoAtivoAtual.Id.ToString();
                                    var usuario = (await _systemRepository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Where u.Pessoa = {dadosPessoaProvider.PessoaSistema} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                                    if (usuario != null)
                                    {
                                        //PessoaId:195001|UsuarioId:9784
                                        if (!string.IsNullOrEmpty(usuario.ProviderChaveUsuario))
                                        {
                                            usuario.ProviderChaveUsuario = usuario.ProviderChaveUsuario.Replace($"PessoaId:{pessoaProviderOriginal}|", $"PessoaId:{pessoaContratoAtivoAtual.Id.ToString()}");
                                            usuario.Status = EnumStatus.Ativo;
                                            await _systemRepository.Save(usuario);
                                        }
                                    }

                                    await _systemRepository.Save(dadosPessoaProvider);
                                }


                            }
                        }
                    }
                    else if (pessoProviderObject.eMail != null && pessoProviderObject.eMail.Contains("@"))
                    {
                        var outrasPessoas = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select 
                                            p.*     
                                        From 
                                            Pessoa p Inner Join FrPessoa fp on fp.Pessoa = p.Id 
                                            Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp.Id 
                                        Where 
                                            Lower(p.Email) like '{pessoProviderObject.eMail.Split(';')[0].ToLower()}%' and p.Id <> {pessoaProviderId} and av.Status = 'A'")).AsList();

                        if (contratoPessoaAtualAtivo == null && outrasPessoas.Any())
                        {
                            var pessoaContratoAtivoAtual = outrasPessoas.FirstOrDefault(a => !string.IsNullOrEmpty(a.Nome) && a.Nome.Contains(pessoProviderObject.Nome!));
                            if (pessoaContratoAtivoAtual != null)
                            {
                                var dadosPessoaProvider = (await _systemRepository.FindByHql<PessoaSistemaXProvider>(@$"
                                    From 
                                    PessoaSistemaXProvider a
                                    Where 1 = 1 and 
                                            a.PessoaProvider = '{pessoaProviderId}'")).FirstOrDefault();

                                if (dadosPessoaProvider != null)
                                {
                                    dadosPessoaProvider.PessoaProvider = pessoaContratoAtivoAtual.Id.ToString();
                                    pessoaProviderId = pessoaContratoAtivoAtual.Id.ToString();
                                    var usuario = (await _systemRepository.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Where u.Pessoa = {dadosPessoaProvider.PessoaSistema}  and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                                    if (usuario != null)
                                    {
                                        //PessoaId:195001|UsuarioId:9784
                                        if (!string.IsNullOrEmpty(usuario.ProviderChaveUsuario))
                                        {
                                            usuario.ProviderChaveUsuario = usuario.ProviderChaveUsuario.Replace($"PessoaId:{pessoaProviderOriginal}|", $"PessoaId:{pessoaContratoAtivoAtual.Id.ToString()}");
                                            usuario.Status = EnumStatus.Ativo;
                                            await _systemRepository.Save(usuario);
                                        }
                                    }

                                    await _systemRepository.Save(dadosPessoaProvider);
                                }


                            }
                        }
                    } 
                }

                var sbPortal = new StringBuilder(@"
                                        Select 
                                        c.Id as EsolCotaId,
                                        c.Nome as EsolCotaNome,
                                        uhc.Numero as EsolNumeroImovel,
                                        p.Id as EsolPessoaProprietarioId,
                                        p.Nome as EsolPessoaProprietarioNome,
                                        uhc.Numero as EsolNumeroImovel 
                                        From
                                        Cota c
                                        Inner Join CotaProprietario cp on cp.Cota = c.Id
                                        Inner Join UhCondominio uhc on uhc.Id = cp.UhCondominio
                                        Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null
                                        Inner Join Cliente cli on pro.Cliente = cli.Id
                                        Inner Join Pessoa p on cli.Pessoa = p.Id
                                        Where 
                                        Lower(c.Nome) = :nomeCota and uhc.Numero = :numeroImovel");

                var sb = new StringBuilder(@$"Select
                                        c.Id as AcCotaId,
                                        gctc.Nome as AcCotaNome,
                                        i.Numero as AcNumeroImovel,
                                        e.Nome as AcEmpreendimentoNome,
                                        p.Id as AcPessoaProprietarioId,
                                        p.Nome as AcPessoaProprietarioNome,
                                        e.Id as EmpreendimentoId,
                                        Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                                        From
                                        Imovel i
                                        Inner Join Cota c on c.Imovel = i.Id
                                        Inner Join GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                                        Inner Join Cliente cli on c.Proprietario = cli.Id
                                        Inner Join Empreendimento e on i.Empreendimento = e.Id
                                        inner Join Pessoa p on cli.Pessoa = p.Id
                                        Left Join TipoImovel ti on i.TipoImovel = ti.Id
                                        where p.Id = {pessoaProviderId}");

                var sql = sb.ToString();
                vinculo = (await _repositoryAccessCenter.FindBySql<VinculoAccessXPortalBase>(sql)).FirstOrDefault();

                if (vinculo == null)
                {
                    sb = new StringBuilder(@$"Select
                                        c.Id as AcCotaId,
                                        gctc.Nome as AcCotaNome,
                                        i.Numero as AcNumeroImovel,
                                        e.Nome as AcEmpreendimentoNome,
                                        p.Id as AcPessoaProprietarioId,
                                        p.Nome as AcPessoaProprietarioNome,
                                        e.Id as EmpreendimentoId,
                                        Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                                        From
                                        Imovel i
                                        Inner Join Cota c on c.Imovel = i.Id
                                        Inner Join GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                                        Inner Join FrAtendimentoVenda av on av.Cota = c.Id
                                        Inner Join FrPessoa fp on av.FrPessoa1 = fp.Id
                                        Inner Join Empreendimento e on i.Empreendimento = e.Id
                                        inner Join Pessoa p on fp.Pessoa = p.Id
                                        Left Join TipoImovel ti on i.TipoImovel = ti.Id
                                        where p.Id = {pessoaProviderId}");

                    sql = sb.ToString();
                    vinculo = (await _repositoryAccessCenter.FindBySql<VinculoAccessXPortalBase>(sql)).FirstOrDefault();
                }

                if (vinculo != null && !string.IsNullOrEmpty(vinculo.AcCotaNome) && !string.IsNullOrEmpty(vinculo.AcNumeroImovel))
                {
                    var vinculoPortal = (await _repositoryPortalEsol.FindBySql<VinculoAccessXPortalBase>(sbPortal.ToString(),
                        new List<Parameter>() {
                                        new Parameter("nomeCota",vinculo.AcCotaNome.ToLower()),
                                        new Parameter("numeroImovel", vinculo.AcNumeroImovel.ToLower()) }.ToArray())).FirstOrDefault();

                    if (vinculoPortal != null)
                    {
                        vinculo.EsolCotaId = vinculoPortal.EsolCotaId;
                        vinculo.EsolCotaNome = vinculoPortal.EsolCotaNome;
                        vinculo.EsolNumeroImovel = vinculoPortal.EsolNumeroImovel;
                        vinculo.EsolPessoaProprietarioId = vinculoPortal.EsolPessoaProprietarioId;
                        vinculo.EsolPessoaProprietarioNome = vinculoPortal.EsolPessoaProprietarioNome;
                        if (!string.IsNullOrEmpty(vinculoPortal.PadraoDeCor) && !vinculoPortal.PadraoDeCor.Contains("default",StringComparison.InvariantCultureIgnoreCase))
                            vinculo.PadraoDeCor = vinculoPortal.PadraoDeCor;

                        if (!aplicarPadraoBlack)
                            vinculo.PadraoDeCor = "Default";
                    }
                }
            }


            return vinculo;
        }

        public async Task<List<StatusCrcContratoModel>?> GetStatusCrc(List<int> frAtendimentoVendaIds)
        {

            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);
            if (!aplicarPadraoBlack)
                tipoImovelPadraoBlack = "-1";

            List<StatusCrcContratoModel>? status = new();
            if (!frAtendimentoVendaIds.Any()) return default;

            var statusIgnorar = _configuration.GetValue<string>("StatusCrcIgnorarFiltro", "621");

            var sbList = Helper.Sublists(frAtendimentoVendaIds, 1000);
            foreach (var item in sbList)
            {
                var sqlStatusCrc = new StringBuilder(@$"
                                            SELECT
                                            avcrc.DataHoraCriacao AS AtendimentoStatusCrcData,
                                            avcrc.Id AS AtendimentoStatusCrcId,
                                            avcrc.Status AS AtendimentoStatusCrcStatus,
                                            av.Id AS FrAtendimentoVendaId,
                                            st.Codigo AS CodigoStatus,
                                            st.Id as FrStatusCrcId,
                                            st.Nome AS NomeStatus,
                                            st.BloquearEmissaoBoletos,
                                            st.Status AS FrCrcStatus,
                                            st.BloquearUtilizacaoCota,
                                            st.BloquearCobrancaPagRec,
                                            Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor,
                                            p1.Nome as NomeTitular,
                                            Case when p1.Tipo = 'F' then p1.CPF else p1.CNPJ end as Cpf_Cnpj_Titular,
                                            p2.Nome as NomeCoCessionario,
                                            Case when p2.Tipo = 'F' then p2.CPF else p2.CNPJ end as Cpf_Cnpj_CoCessionario,
                                            i.Numero as NumeroImovel,
                                            gctc.Nome as GrupoCotaTipoCotaNome,
                                            gctc.Codigo as GrupoCotaTipoCotaCodigo
                                            FROM 
                                            FRATENDIMENTOVENDASTATUSCRC avcrc
                                            INNER JOIN FRSTATUSCRC st ON avcrc.FrStatusCrc = st.ID
                                            INNER JOIN FrAtendimentoVenda av ON avcrc.FrAtendimentoVenda = av.Id
                                            LEFT JOIN FrPessoa fp1 ON av.FrPessoa1 = fp1.Id
                                            lEFT JOIN Pessoa p1 ON fp1.Pessoa = p1.Id
                                            LEFT JOIN FrPessoa fp2 ON av.FrPessoa2 = fp2.Id
                                            LEFT JOIN Pessoa p2 ON fp2.Pessoa = p2.Id
                                            LEFT JOIN Cota c on av.Cota = c.Id
                                            LEFT JOIN GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                                            LEFT JOIN Imovel i on c.Imovel = i.Id
                                            LEFT JOIN TipoImovel ti on i.TipoImovel = ti.Id
                                            WHERE
                                            avcrc.Status = 'A' ");

                if (!string.IsNullOrEmpty(statusIgnorar))
                {
                    sqlStatusCrc.AppendLine($" and st.Id not in ({statusIgnorar}) ");
                }

                sqlStatusCrc.AppendLine($" and av.Id in ({string.Join(",", item)}) ");

                status.AddRange((await _repositoryAccessCenter.FindBySql<StatusCrcContratoModel>(sqlStatusCrc.ToString())).AsList());
            }

            return status;
            
        }


        public async Task<List<StatusCrcContratoModel>?> GetStatusCrcPorTipoStatusIds(List<int> statusCrcIds)
        {

            List<StatusCrcContratoModel>? status = new();
            if (!statusCrcIds.Any()) return default;

            var sqlStatusCrc = new StringBuilder(@$"
                                            SELECT
                                            avcrc.DataHoraCriacao AS AtendimentoStatusCrcData,
                                            avcrc.Id AS AtendimentoStatusCrcId,
                                            avcrc.Status AS AtendimentoStatusCrcStatus,
                                            av.Id AS FrAtendimentoVendaId,
                                            st.Codigo AS CodigoStatus,
                                            st.Nome AS NomeStatus,
                                            st.BloquearEmissaoBoletos,
                                            st.Status AS FrCrcStatus,
                                            st.BloquearUtilizacaoCota,
                                            st.BloquearCobrancaPagRec,
                                            p1.Nome as NomeTitular,
                                            p1.Id as PessoaTitular1Id,
                                            Case when p1.Tipo = 'F' then p1.CPF else p1.CNPJ end as Cpf_Cnpj_Titular,
                                            p2.Nome as NomeCoCessionario,
                                            p2.Id as PessoaTitular2Id,
                                            Case when p2.Tipo = 'F' then p2.CPF else p2.CNPJ end as Cpf_Cnpj_CoCessionario,
                                            i.Numero as NumeroImovel,
                                            gctc.Nome as GrupoCotaTipoCotaNome,
                                            gctc.Codigo as GrupoCotaTipoCotaCodigo
                                            FROM 
                                            FRATENDIMENTOVENDASTATUSCRC avcrc
                                            INNER JOIN FRSTATUSCRC st ON avcrc.FrStatusCrc = st.ID
                                            INNER JOIN FrAtendimentoVenda av ON avcrc.FrAtendimentoVenda = av.Id
                                            LEFT JOIN FrPessoa fp1 ON av.FrPessoa1 = fp1.Id
                                            lEFT JOIN Pessoa p1 ON fp1.Pessoa = p1.Id
                                            LEFT JOIN FrPessoa fp2 ON av.FrPessoa2 = fp2.Id
                                            LEFT JOIN Pessoa p2 ON fp2.Pessoa = p2.Id
                                            LEFT JOIN Cota c on av.Cota = c.Id
                                            LEFT JOIN GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                                            LEFT JOIN Imovel i on c.Imovel = i.Id
                                            LEFT JOIN TipoImovel ti on i.TipoImovel = ti.Id
                                            WHERE
                                            avcrc.Status = 'A' and 
                                            st.Id in ({string.Join(",",statusCrcIds)})");

            return (await _repositoryAccessCenter.FindBySql<StatusCrcContratoModel>(sqlStatusCrc.ToString())).AsList();
        }

        public async Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue("TipoImovelPadraoBlack", "1,4,21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            List<DadosContratoModel> contratos = new();

            var itemCache = await _cacheStore.GetAsync<List<DadosContratoModel>>("contratosCache_", 10, _repositoryAccessCenter.CancellationToken);
            if (itemCache != null && itemCache.Any())
                return itemCache;

            var sqlStatusCrc = new StringBuilder($@"
                                            SELECT
                                            av.Id AS FrAtendimentoVendaId,
                                            av.Codigo AS NumeroContrato,
                                            av.Status,
                                            av.DATAVENDA,
                                            av.DataCancelamento,
                                            av.DATACONTIGENCIA,
                                            av.Contigencia,
                                            av.DATAREVERSAO,
                                            p1.Id AS PessoaTitular1Id,
                                            p1.Tipo AS PessoaTitular1Tipo,
                                            p1.Nome AS PessoaTitular1Nome,
                                            p1.CPF AS PessoaTitular1CPF,
                                            p1.CNPJ AS PessoaTitualar1CNPJ,
                                            p1.eMail AS PessoaTitular1Email,
                                            p2.Id AS PessoaTitular2Id,
                                            p2.Tipo AS PessoaTitular2Tipo,
                                            p2.Nome AS PessoaTitular2Nome,
                                            p2.CPF AS PessoaTitular2CPF,
                                            p2.CNPJ AS PessoaTitualar2CNPJ,
                                            p2.eMail AS PessoaTitular2Email,
                                            fp.NOME AS Produto,
                                            COALESCE(fp.Empreendimento,tc.Empreendimento) AS Empreendimento,
                                            av.IdIntercambiadora,
                                            co.Id as Cota,
                                            av.CotaOriginal,
                                            co.Status as CotaStatus,
                                            i.Numero as NumeroImovel,    
                                            gctc.Nome as GrupoCotaTipoCotaNome,
                                            gctc.Codigo as GrupoCotaTipoCotaCodigo,
                                            Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                                            FROM 
                                            FrAtendimentoVenda av
                                            INNER JOIN FrPessoa fp1 ON av.FrPessoa1 = fp1.Id
                                            INNER JOIN Pessoa p1 ON fp1.Pessoa = p1.Id
                                            LEFT OUTER JOIN FrPessoa fp2 ON av.FrPessoa2 = fp2.Id
                                            LEFT OUTER JOIN Pessoa p2 ON fp2.Pessoa = p2.Id
                                            LEFT OUTER JOIN FrProduto fp ON  av.FrProduto = fp.Id
                                            LEFT OUTER JOIN TipoCota tc ON fp.TipoCota = tc.Id
                                            LEFT JOIN Cota co on av.Cota = co.Id
                                            LEFT JOIN Imovel i on co.Imovel = i.Id
                                            LEFT JOIN TipoImovel ti on i.TipoImovel = ti.Id
                                            LEFT JOIN GrupoCotaTipoCota gctc on co.GrupoCotaTipoCota = gctc.Id
                                            WHERE
                                            1 = 1 and av.Status = 'A' ");


            if (pessoasPesquisar != null && pessoasPesquisar.Any())
            {
                sqlStatusCrc.AppendLine($" and (p1.Id in ({string.Join(",",pessoasPesquisar)})  or (av.FrPessoa2 is not null and p2.Id in ({string.Join(",",pessoasPesquisar)}))) ");
            }

            contratos = (await _repositoryAccessCenter.FindBySql<DadosContratoModel>(sqlStatusCrc.ToString())).AsList();
            if (contratos != null && contratos.Any()) 
            { 
                var statusCrcContratos = await GetStatusCrc(contratos.Select(a=> a.FrAtendimentoVendaId.GetValueOrDefault(0)).AsList());
                if (statusCrcContratos != null && statusCrcContratos.Any())
                {
                    foreach (var item in contratos)
                    {
                        var statusAtivosDoContrato = statusCrcContratos.Where(a => a.FrAtendimentoVendaId.GetValueOrDefault() == item.FrAtendimentoVendaId.GetValueOrDefault()).AsList();
                        item.frAtendimentoStatusCrcModels = statusAtivosDoContrato;

                        if (!aplicarPadraoBlack)
                            item.PadraoDeCor = "Default";
                    }
                }
            }

            if (contratos != null && contratos.Any())
                await _cacheStore.AddAsync("contratosCache_", contratos, DateTimeOffset.Now.AddHours(1), 10, _systemRepository.CancellationToken);

            return contratos;
        }

        public async Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado(int id)
        {

            List<Parameter> parameters = new();

            StringBuilder sb = new(@$"
                        Select
                             e.Codigo,
                             p.Nome,
                             p.NomeFantasia,
                             p.Email,
                             p.Cnpj
                        From 
                            Empresa e
                            Inner Join Pessoa p on e.Pessoa = p.Id
                            Where e.Id = {id} ");

            var empresa = (await _repositoryAccessCenter.FindBySql<EmpresaSimplificadaModel>(sb.ToString(), parameters.ToArray())).FirstOrDefault();

            return empresa;
        }


        public async Task<List<PaisModel>> GetPaisesLegado()
        {
            return (await _repositoryAccessCenter.FindBySql<PaisModel>("Select Distinct p.Nome, p.CodigoPais as CodigoIbge From Pais p")).AsList();
        }

        public async Task<List<EstadoModel>> GetEstadosLegado()
        {
            return (await _repositoryAccessCenter.FindBySql<EstadoModel>(@"Select 
                        Distinct
                        e.Nome, 
                        e.Uf as Sigla,
                        p.CODIGOPAIS AS PaisCodigoIbge,
                        p.Nome as PaisNome,
                        e.CodigoIbge 
                        From Estado e Inner Join Pais p on e.Pais = p.Id")).AsList();
        }

        public async Task<List<CidadeModel>> GetCidade()
        {
            return (await _repositoryAccessCenter.FindBySql<CidadeModel>(@"Select 
                Distinct
                c.Id,
                c.Nome,
                c.CodigoIbge,
                e.Nome as EstadoNome, 
                e.UF as EstadoSigla,
                p.CodigoPais as PaisCodigoIbge,
                p.Nome as PaisNome,
                e.CodigoIbge as EstadoCodigoIbge
                From 
                Cidade c 
                Inner JOIN Estado e ON C.Estado = e.Id
                Inner Join Pais p on e.Pais = p.Id")).AsList();
        }

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade(CidadeSearchModel searchModel)
        {
            var sb = new StringBuilder(@"Select 
                Distinct
                c.Id,
                c.Nome,
                c.CodigoIbge,
                e.Nome as EstadoNome, 
                e.UF as EstadoSigla,
                p.CodigoPais as PaisCodigoIbge,
                p.Nome as PaisNome,
                e.CodigoIbge as EstadoCodigoIbge,
                c.Nome + '/' + e.UF as NomeFormatado
                From 
                Cidade c 
                Inner JOIN Estado e ON C.Estado = e.Id
                Inner Join Pais p on e.Pais = p.Id ");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and c.Id = {searchModel.Id} ");
            }

            if (!string.IsNullOrEmpty(searchModel.CodigoIbge))
            {
                sb.AppendLine($" and Lower(c.CodigoIbge) like '%{searchModel.CodigoIbge.ToLower().TrimEnd()}%' ");
            }

            if (!string.IsNullOrEmpty(searchModel.Nome))
            {
                sb.AppendLine($" and Lower(c.Nome) like '%{searchModel.Nome.ToLower().TrimEnd()}%' ");
            }

            if (!string.IsNullOrEmpty(searchModel.Search))
            {
                if (Helper.IsNumeric(searchModel.Search.Trim()))
                {
                    sb.AppendLine($" and c.Id = {searchModel.Id} ");
                }
                else
                {
                    var arrCidadeSigla = searchModel.Search.Split('/');
                    if (arrCidadeSigla.Length == 2)
                    {
                        sb.AppendLine($" and Lower(c.Nome) like '%{arrCidadeSigla[0].ToLower().Trim()}%' and Lower(e.Uf) like '%{arrCidadeSigla[1].ToLower().Trim()}%'");
                    }
                    else if (arrCidadeSigla.Length == 1)
                    {
                        sb.AppendLine($" and Lower(c.Nome) like '%{arrCidadeSigla[0].ToLower().TrimEnd()}%' ");
                    }
                }
            }

            var sql = sb.ToString();

            int totalRegistros = 0;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 15;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
            {
                totalRegistros = Convert.ToInt32(await _repositoryPortalEsol.CountTotalEntry(sql, new List<Parameter>().ToArray()));
            }

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine(" Order by c.Id ");

            var cidades = searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 ?
                await _repositoryPortalEsol.FindBySql<CidadeModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(1), new List<Parameter>().ToArray())
                : await _repositoryPortalEsol.FindBySql<CidadeModel>(sb.ToString(), new List<Parameter>().ToArray());


            if (cidades.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    Int64 totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);

                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), cidades);
                }

                return (1, 1, cidades);
            }

            return default;
        }

        public async Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado()
        {
            var documentosPessoas = (await _repositoryAccessCenter.FindBySql<DocumentoRegistro>(@$"
                            Select 
                            dr.Pessoa,
                            tdr.Id as TipoDocumentoRegistro,
                            dr.DocumentoAlfanumerico,
                            dr.DocumentoNumerico,
                            dr.Principal,
                            dr.Tipo
                            From 
                            DocumentoRegistro dr 
                            Inner Join TipoDocumentoRegistro tdr on dr.TipoDocumentoRegistro = tdr.Id
                            Where 
                            dr.DocumentoAlfanumerico is not null and 
                            Lower(tdr.Nome) in ('cpf','cnpj') ")).AsList();


            var sb = new StringBuilder(@"select
                    u.Login,
                    p.Nome AS FullName,
                    p.Id AS PessoaId,
                    CASE WHEN p.Tipo = 'F' THEN p.CPF ELSE p.CNPJ END AS CpfCnpj,
                    p.Email,
                    u.Senha AS Password,
                    u.Senha AS PasswordConfirmation
                    from 
                    usuario u
                    Inner Join Pessoa p on u.Pessoa = p.Id
                    WHERE 
                    u.STATUS  = 'A' ");

            var usuariosLegado = (await _repositoryAccessCenter.FindBySql<UserRegisterInputModel>(sb.ToString())).AsList();

            foreach (var item in usuariosLegado)
            {
                if (string.IsNullOrEmpty(item.CpfCnpj) || (!string.IsNullOrEmpty(item.CpfCnpj) && !Helper.IsCnpj(item.CpfCnpj) && !Helper.IsCpf(item.CpfCnpj)))
                {
                    var docRegistro = documentosPessoas.FirstOrDefault(b => b.Pessoa.GetValueOrDefault() == Convert.ToInt32(item.PessoaId));
                    if (docRegistro != null)
                    {
                        item.CpfCnpj = docRegistro.DocumentoAlfanumerico;
                    }
                }

                if (!string.IsNullOrEmpty(item.FullName))
                    item.FullName = item.FullName.Replace("'", " ");

                if (!string.IsNullOrEmpty(item.Login))
                    item.Login = item.Login.Replace("'", " ");
            }

            return usuariosLegado;

        }

        public async Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado(ParametroSistemaViewModel parametroSistema)
        {
            if (parametroSistema == null || string.IsNullOrEmpty(parametroSistema.ExibirFinanceirosDasEmpresaIds))
                return await Task.FromResult(new List<UserRegisterInputModel>());


            var documentosPessoas = (await _repositoryAccessCenter.FindBySql<DocumentoRegistro>(@$"
                            Select 
                            dr.Pessoa,
                            tdr.Id as TipoDocumentoRegistro,
                            dr.DocumentoAlfanumerico,
                            dr.DocumentoNumerico,
                            dr.Principal,
                            dr.Tipo
                            From 
                            DocumentoRegistro dr 
                            Inner Join TipoDocumentoRegistro tdr on dr.TipoDocumentoRegistro = tdr.Id
                            Where 
                            dr.DocumentoAlfanumerico is not null and 
                            Lower(tdr.Nome) in ('cpf','cnpj') and exists(Select c.Pessoa From Cliente c Where c.Empresa in ({parametroSistema.ExibirFinanceirosDasEmpresaIds}) and c.Pessoa = dr.Pessoa) ")).AsList();

            var sb = new StringBuilder(@$"select
                    c.CONDOMINIOUSUARIO as Login,
                    p.Nome AS FullName,
                    p.Id AS PessoaId,
                    CASE WHEN p.Tipo = 'F' THEN p.CPF ELSE p.CNPJ END AS CpfCnpj,
                    p.Email,
                    Nvl(c.CondominioSenha,'UVVANR+GEBVZ1IHpp3rQcg==') AS Password,
                    Nvl(c.CondominioSenha,'UVVANR+GEBVZ1IHpp3rQcg==') AS PasswordConfirmation,
                    c.Codigo
                    from 
                    Cliente c
                    Inner Join Empresa e on c.Empresa = e.Id
                    Inner Join Pessoa p on c.Pessoa = p.Id
                    WHERE 
                    e.Id in ({parametroSistema.ExibirFinanceirosDasEmpresaIds})
                    and ( Exists(Select co.Proprietario From Cota co Where co.Proprietario = c.Id and co.Status = 'V') or 
                          Exists(Select fap1.Pessoa From FrPessoa fap1 Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fap1.Id Where fap1.Pessoa = c.Pessoa and av.Status = 'A')
                        ) ");

            var usuariosLegado = (await _repositoryAccessCenter.FindBySql<UserRegisterInputModel>(sb.ToString())).AsList();
            List<UserRegisterInputModel> clientesDistintosRetornar = new List<UserRegisterInputModel>();
            foreach (var group in usuariosLegado.GroupBy(b => b.PessoaId))
            {
                var item = group.First();
                if (string.IsNullOrEmpty(item.CpfCnpj) || (!string.IsNullOrEmpty(item.CpfCnpj) && !Helper.IsCnpj(item.CpfCnpj) && !Helper.IsCpf(item.CpfCnpj)))
                {
                    var docRegistro = documentosPessoas.FirstOrDefault(b => b.Pessoa.GetValueOrDefault() == Convert.ToInt32(item.PessoaId));
                    if (docRegistro != null)
                    {
                        item.CpfCnpj = docRegistro.DocumentoAlfanumerico;
                    }
                }

                if (string.IsNullOrEmpty(item.Login))
                {
                    if (!string.IsNullOrEmpty(item.CpfCnpj))
                    {
                        item.Login = item.CpfCnpj;
                    }
                    else if (!string.IsNullOrEmpty(item.FullName))
                    {
                        item.Login = $"{item.FullName.Split(' ')[0]}-{item.Codigo.TrimEnd()}";
                    }
                }

                if (!string.IsNullOrEmpty(item.FullName))
                    item.FullName = item.FullName.Replace("'", " ");

                if (!string.IsNullOrEmpty(item.Login))
                    item.Login = item.Login.Replace("'", " ");

                if (!string.IsNullOrEmpty(item.Login))
                    item.Login = item.Login.TrimEnd().RemoveAccents();

                clientesDistintosRetornar.Add(item);
            }

            return clientesDistintosRetornar;
        }

        public async Task<bool> DesativarUsuariosSemCotaOuContrato()
        {

            var quantidadeMantida = 0;
            var quantidadeRemovida = 0;

            try
            {
                var empresaCondominioPortalId = _configuration.GetValue<string>("EmpresaCondominioPortalId", "1,15,16");

                var vinculosPessoasProvider = await _systemRepository.FindByHql<PessoaSistemaXProvider>("From PessoaSistemaXProvider");

                var parametrosSistema = await _systemRepository.GetParametroSistemaViewModel();
                if (parametrosSistema != null && !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds))
                {

                    var cotasAccessCenter = (await _repositoryAccessCenter.FindBySql<CotaAccessCenterModel>($@"select
                                                                                            c.Id as CotaId,
                                                                                            i.Id as Imovel,
                                                                                            ia.Codigo as AndarCodigo,
                                                                                            ia.Nome as AndarNome,
                                                                                            ib.Codigo as CodigoBloco,
                                                                                            ib.Nome as NomeBloco,
                                                                                            i.Numero as NumeroImovel,
                                                                                            i.Tag as TagImovel,
                                                                                            c.Tag as TagCota,
                                                                                            c.Id as CotaId,
                                                                                            c.Proprietario,
                                                                                            clip.Nome as ProprietarioNome,
                                                                                            clip.Cpf as CpfProprietario,
                                                                                            clip.Cnpj as CnpjProprietario,
                                                                                            clip.Email as EmailProprietario,
                                                                                            c.FrAtendimentoVenda,
                                                                                            c.Status as StatusCota,
                                                                                            c.Bloqueado as CotaBloqueada,
                                                                                            gctc.Codigo as GrupoCotaTipoCotaCodigo,
                                                                                            gctc.Nome as GrupoCotaTipoCotaNome,
                                                                                            gc.Codigo as GrupoCotaCodigo,
                                                                                            gc.Nome as GrupoCotaNome,
                                                                                            tc.Codigo as TipoCotaCodigo,
                                                                                            tc.Nome as TipoCotaNome,
                                                                                            clip.Id as PessoaProviderId
                                                                                            From
                                                                                            Cota c
                                                                                            Inner Join Cliente cli on c.Proprietario = cli.Id
                                                                                            Inner Join Pessoa clip on cli.Pessoa = clip.Id
                                                                                            Inner Join Imovel i on c.Imovel = i.Id
                                                                                            Inner Join ImovelAndar ia on i.ImovelAndar = ia.Id
                                                                                            Inner Join ImovelBloco ib on i.ImovelBloco = ib.Id
                                                                                            Inner join Empreendimento emp on i.Empreendimento = emp.Id 
                                                                                            Inner Join GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                                                                                            Inner Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                                                                            Inner Join TipoCota tc on gctc.TipoCota = tc.Id 
                                                                                            Where 
                                                                                            --gctc.Codigo = 'MFA-2SP-06' and i.Numero = '0213' and
                                                                                            emp.Filial in (Select f.Id From Filial f Where f.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds}))")).AsList();

                    foreach (var item in cotasAccessCenter)
                    {

                        var cotaPortal = (await _repositoryPortalEsol.FindBySql<CotaAccessCenterModel>(@$"Select 
                                    c.Id as CotaId, 
                                    u.UHCondominio,
                                    c.Nome as CotaNome,
                                    c.Codigo as CotaCodigo,
                                    gc.Nome as GrupoCotaNome,
                                    cp.Id as CotaProprietarioId,
                                    pro.Id as ProprietarioId,
                                    pro.Id as Proprietario,
                                    clip.Nome as ProprietarioNome,
                                    clip.Cpf as CpfProprietario,
                                    clip.Email as EmailProprietario,
                                    u.Numero as ImovelNumero,
                                    u.Numero as NumeroImovel,
                                    uc.Numero as NumeroUhCondominio
                                    From 
                                    Cota c
                                    Inner Join CotaProprietario cp on cp.Cota = c.Id and cp.DataHoraExclusao is null and cp.UsuarioExclusao is null
                                    Inner Join UhCondominio uc on cp.UhCondominio = uc.Id
                                    Inner Join Uh u on uc.Id = u.UhCondominio
                                    Inner Join GrupoCotas gc on c.GrupoCotas = gc.Id
                                    Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null and pro.UsuarioExclusao IS null
                                    Inner Join Cliente cli on pro.Cliente = cli.Id
                                    Inner Join Pessoa clip on cli.Pessoa = clip.Id
                                    Where 
                                    gc.Empresa in ({empresaCondominioPortalId}) and 
                                    (u.Numero = '{item.NumeroImovel!.TrimEnd()}' or uc.Numero = '{item.NumeroImovel!.TrimEnd()}') and 
                                    Lower(Replace(clip.Nome,'''','')) = '{item.ProprietarioNome!.ToLower().Replace("'", "")}' and 
                                    (Lower(c.Nome) = '{item.GrupoCotaTipoCotaNome!.ToLower()}' or 
                                     Lower(c.Codigo) = '{item.GrupoCotaTipoCotaCodigo}')")).FirstOrDefault();

                        if (cotaPortal != null)
                        {
                            quantidadeMantida++;
                        }
                        else
                        {
                            quantidadeRemovida++;
                            var pessoaProviderDesativarUsuario = vinculosPessoasProvider.FirstOrDefault(a => a.PessoaProvider == $"{item.PessoaProviderId}");
                            if (pessoaProviderDesativarUsuario != null)
                            {
                                var usuarioVinculado = (await _systemRepository.FindByHql<Domain.Entities.Core.Sistema.Usuario>(@$"From 
                                                                            Usuario u 
                                                                            Inner Join Fetch u.Pessoa p 
                                                                            Where 
                                                                            u.Status = {(int)EnumStatus.Ativo} and 
                                                                            p.Id = {pessoaProviderDesativarUsuario.PessoaSistema}", null)).FirstOrDefault();
                                if (usuarioVinculado != null)
                                {
                                    try
                                    {
                                        _systemRepository.BeginTransaction();
                                        usuarioVinculado.Status = EnumStatus.Inativo;
                                        usuarioVinculado.DataHoraAlteracao = DateTime.Now;
                                        usuarioVinculado.UsuarioAlteracao = usuarioVinculado.UsuarioCriacao.GetValueOrDefault(1);
                                        await _systemRepository.ForcedSave(usuarioVinculado);
                                        var resultCommit = await _systemRepository.CommitAsync();
                                        if (!resultCommit.executed)
                                            throw resultCommit.exception ?? new Exception("Operação não realizada");

                                    }
                                    catch (Exception err)
                                    {
                                        _systemRepository.Rollback();
                                    }
                                }
                            }
                        }


                    }

                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
            }

            _logger.LogInformation($"Qtde usuários inativados: {quantidadeRemovida}, Qtde usuários mantidos: {quantidadeMantida}");
            return true;
        }

        public async Task GetOutrosDadosUsuario(TokenResultModel userReturn)
        {
            var loggedUser = await _systemRepository.GetLoggedUser();
            if (loggedUser.Value.isAdm)
            {
                userReturn.Idioma = 0;
                userReturn.PodeInformarConta = 1;
            }

            if (!string.IsNullOrEmpty(userReturn.ProviderKeyUser))
            {
                var pessoa = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = {userReturn.ProviderKeyUser}")).FirstOrDefault();
                if (pessoa != null)
                {
                    if (pessoa.Estrangeiro == "S")
                    {
                        userReturn.Idioma = 2;
                        userReturn.PodeInformarConta = 0;
                    }
                    else
                    {
                        userReturn.Idioma = 0;
                        userReturn.PodeInformarConta = 1;
                    }
                }
            }


            await Task.CompletedTask;
        }

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(List<string> empresasIds)
        {
            var empresasVinculadas = (await _repositoryAccessCenter.FindBySql<EmpresaVinculadaModel>(@$"
                            Select 
                            e.Id,
                            ep.Nome
                            From 
                            Empresa e
                            Inner Join Pessoa ep on e.Pessoa = ep.Id
                            Where 
                            e.Id in ({string.Join(",",empresasIds)}) ")).AsList();

            return empresasVinculadas.Any() ? empresasVinculadas : null;
        }

        public async Task<UsuarioValidateResultModel> GerUserFromLegado(UserRegisterInputModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
                throw new ArgumentException("O campo Email é obrigatório.");
            if (string.IsNullOrEmpty(model.FullName))
                throw new ArgumentException("O campo nome é obrigatório.");

            var parametroSistema = await _systemRepository.GetParametroSistemaViewModel();

            if (parametroSistema == null || string.IsNullOrEmpty(parametroSistema.ExibirFinanceirosDasEmpresaIds))
                return await Task.FromResult(new UsuarioValidateResultModel());



            string sqlComplementar = "";
            if (!string.IsNullOrEmpty(model.CpfCnpj))
            {
                var doc = Helper.ApenasNumeros(model.CpfCnpj);
                var docNumerico = Convert.ToInt64(doc);
                if (!string.IsNullOrEmpty(doc) && doc.Length > 9)
                {
                    sqlComplementar = @$" and (p.CPF = {docNumerico} or p.CNPJ = {docNumerico}) ";
                }
            }

            List<UserRegisterInputModel>? usuariosLegado = new List<UserRegisterInputModel>();

            var documentosPessoas = !string.IsNullOrEmpty(sqlComplementar) ? 
                (await _repositoryAccessCenter.FindBySql<DocumentoRegistro>(@$"
                            Select 
                            dr.Pessoa,
                            tdr.Id as TipoDocumentoRegistro,
                            dr.DocumentoAlfanumerico,
                            dr.DocumentoNumerico,
                            dr.Principal,
                            dr.Tipo
                            From 
                            DocumentoRegistro dr 
                            Inner Join TipoDocumentoRegistro tdr on dr.TipoDocumentoRegistro = tdr.Id
                            Inner Join Pessoa p on dr.Pessoa = p.Id
                            Where 
                            dr.DocumentoAlfanumerico is not null and 
                            Lower(tdr.Nome) in ('cpf','cnpj') {sqlComplementar}")).AsList() : new List<DocumentoRegistro>();

            var sb = new StringBuilder(@$"Select
                    p.Nome AS FullName,
                    p.Id AS PessoaId,
                    CASE WHEN p.Tipo = 'F' THEN p.CPF ELSE p.CNPJ END AS CpfCnpj,
                    p.Email,
                    'P' || to_char(p.Id) as Codigo,
                    Nvl(p.Estrangeiro,'N') as Estrangeiro
                    from 
                    Pessoa p
                    Inner Join FrPessoa fp1 on fp1.Pessoa = p.Id
                    Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp1.Id and av.Status = 'A'
                    Inner Join Filial f on av.Filial = f.Id
                    WHERE 
                    f.Empresa in ({parametroSistema.ExibirFinanceirosDasEmpresaIds})
                    and Lower(p.Nome) = '{model.FullName!.ToLower()}' ");

           
            if (!string.IsNullOrEmpty(model.Email) && model.Email.Contains("@"))
            {
                sb.AppendLine($" and p.Email is not null and Lower(nvl(SUBSTR(p.Email, 1, INSTR(p.Email, ';') -1), p.Email)) = '{model.Email!.ToLower()}' ");
            }
            
            if (documentosPessoas.Count == 0 && string.IsNullOrEmpty(model.Email))
            {
                if (string.IsNullOrEmpty(model.CpfCnpj))
                    throw new ArgumentException("Não foi encontrado o documento informado.");

                throw new ArgumentException("Deve ser informado um documento (CPF/CNPJ ou Passaporte) + eMail");
            }

            if (documentosPessoas.Any())
            {
                    sb = new StringBuilder($@"Select
                    p.Nome AS FullName,
                    p.Id AS PessoaId,
                    CASE WHEN p.Tipo = 'F' THEN p.CPF ELSE p.CNPJ END AS CpfCnpj,
                    p.Email,
                    'P' || to_char(p.Id) as Codigo,
                    Nvl(p.Estrangeiro,'N') as Estrangeiro
                    from 
                    Pessoa p
                    Inner Join FrPessoa fp1 on fp1.Pessoa = p.Id
                    Inner Join FrAtendimentoVenda av on av.FrPessoa1 = fp1.Id and av.Status = 'A'
                    Inner Join Filial f on av.Filial = f.Id
                    WHERE 
                    f.Empresa in ({parametroSistema.ExibirFinanceirosDasEmpresaIds})
                    and p.Id in ({string.Join(",", documentosPessoas.Select(a => a.Pessoa))}) ");

                    usuariosLegado = (await _repositoryAccessCenter.FindBySql<UserRegisterInputModel>(sb.ToString())).AsList();
            }

            if (usuariosLegado == null || !usuariosLegado.Any())
            {
                usuariosLegado = (await _repositoryAccessCenter.FindBySql<UserRegisterInputModel>(sb.ToString())).AsList();
            }


            List<UserRegisterInputModel> clientesDistintosRetornar = new List<UserRegisterInputModel>();
            foreach (var group in usuariosLegado.GroupBy(b => b.PessoaId))
            {
                var item = group.First();
                if (string.IsNullOrEmpty(item.CpfCnpj) || (!string.IsNullOrEmpty(item.CpfCnpj) && !Helper.IsCnpj(item.CpfCnpj) && !Helper.IsCpf(item.CpfCnpj)))
                {
                    var docRegistro = documentosPessoas.FirstOrDefault(b => b.Pessoa.GetValueOrDefault() == Convert.ToInt32(item.PessoaId));
                    if (docRegistro != null)
                    {
                        item.CpfCnpj = docRegistro.DocumentoAlfanumerico;
                    }
                }

                if (string.IsNullOrEmpty(item.Login))
                {
                    if (!string.IsNullOrEmpty(item.CpfCnpj))
                    {
                        item.Login = item.CpfCnpj;
                    }
                    else if (!string.IsNullOrEmpty(item.FullName))
                    {
                        item.Login = $"{item.FullName.Split(' ')[0]}-{item.Codigo.TrimEnd()}";
                    }

                    if (!string.IsNullOrEmpty(item.Login))
                        item.Login = item.Login.TrimEnd().RemoveAccents();
                }

                if (!string.IsNullOrEmpty(item.FullName))
                    item.FullName = item.FullName.Replace("'", " ");

                if (!string.IsNullOrEmpty(item.Login))
                    item.Login = item.Login.Replace("'", " ");

                clientesDistintosRetornar.Add(item);
            }

            if (clientesDistintosRetornar == null || !clientesDistintosRetornar.Any())
            {
                throw new ArgumentException("Não foi possível localizar os dados no sistema!");
            }

            var usuarioRetornar = clientesDistintosRetornar.First();

            var dadosRetorno = new UsuarioValidateResultModel
            {
                PessoaLegadoId = usuarioRetornar.PessoaId,
                Estrangeiro = usuarioRetornar.Estrangeiro
            };

            return dadosRetorno;
        }

        public async Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado()
        {
            var parametroSistema = await _systemRepository.GetParametroSistemaViewModel();

            if (parametroSistema == null || string.IsNullOrEmpty(parametroSistema.ExibirFinanceirosDasEmpresaIds))
                return await Task.FromResult(new List<UserRegisterInputModel>());


            var sb = new StringBuilder(@$"SELECT
                            av.Status as StatusContrato,
                            c.CONDOMINIOUSUARIO as Login,
                            p.Nome AS FullName,
                            p.Id AS PessoaId,
                            pcli.Id AS PessoaFinanceiroId,
                            pcli.Nome AS PessoaFinanceiroNome,
                            CASE WHEN p.Tipo = 'F' THEN p.CPF ELSE p.CNPJ END AS CpfCnpj,
                            p.Email,
                            Nvl(c.CondominioSenha,'UVVANR+GEBVZ1IHpp3rQcg==') AS Password,
                            Nvl(c.CondominioSenha,'UVVANR+GEBVZ1IHpp3rQcg==') AS PasswordConfirmation,
                            To_char(p.Id) as Codigo,
                            c.Empresa AS EmpresaFinanceiroId,
                            f.Empresa AS EmpresaConratoId,
                            p.Id AS Pessoa1ContratoId
                            From 
                            FrAtendimentoVenda av
                            Inner Join FrPessoa fp1 on av.FrPessoa1 = fp1.Id
                            Inner Join Pessoa p on fp1.Pessoa = p.Id
                            Inner Join Filial f on av.Filial = f.Id
                            Inner Join FrAtendimentoVendaContaRec avcr on avcr.FrAtendimentoVenda = av.Id and avcr.Id = (Select Max(avcr1.Id) From FrAtendimentoVendaContaRec avcr1 Where avcr1.FrAtendimentoVenda = avcr.FrAtendimentoVenda)
                            Inner Join ContaReceber cr on avcr.ContaReceber = cr.Id
                            Inner Join Cliente c on cr.Cliente = c.Id
                            Inner Join Empresa e on e.Id = f.Empresa
                            Inner Join Pessoa pcli on c.Pessoa = pcli.Id
                            WHERE 
                            e.Id in ({parametroSistema.ExibirFinanceirosDasEmpresaIds})
                            AND av.Status <> 'A' 
                            AND NOT exists(SELECT fr2.Pessoa FROM FrAtendimentoVenda av1 INNER JOIN FrPessoa fr2 ON av1.FrPessoa1 = fr2.Id Where fr2.Pessoa = p.Id AND av1.Status = 'A')");

            var usuariosLegado = (await _repositoryAccessCenter.FindBySql<UserRegisterInputModel>(sb.ToString())).AsList();
            List<UserRegisterInputModel> clientesDistintosRetornar = new List<UserRegisterInputModel>();
            foreach (var group in usuariosLegado.GroupBy(b => b.PessoaId))
            {
                clientesDistintosRetornar.Add(group.First());
            }

            return clientesDistintosRetornar;
        }

        public async Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null)
        {
            var tiposContasReceberConsiderar = _configuration.GetValue<string>("TiposContasReceberIdsConsiderarNoPortal");
            var strQueryAdicional = "";
            if (!string.IsNullOrEmpty(tiposContasReceberConsiderar))
                strQueryAdicional += $" and tcr.Id in ({tiposContasReceberConsiderar}) ";

            var tiposContaReceberConsiderarBaixados = _configuration.GetValue<string>("TiposContasReceberIdsConsiderarBaixados");
            if (!string.IsNullOrEmpty(tiposContaReceberConsiderarBaixados))
                strQueryAdicional += $" and tcr.Id not in ({tiposContaReceberConsiderarBaixados}) ";

            var parametrosSistema = await _systemRepository.GetParametroSistemaViewModel();

            if (parametrosSistema != null && !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds))
                strQueryAdicional += $" AND cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds}) ";

            List<ClientesInadimplentes> pessoasComPendenciaFinanceiras = new List<ClientesInadimplentes>();
            if (pessoasPesquisar != null && pessoasPesquisar.Any())
            {
                var pessoasSubList = Helper.Sublists<int>(pessoasPesquisar, 1000);

                foreach (var item in pessoasSubList)
                {
                    var pendenciasFinanceiras =
                                        (await _repositoryAccessCenter.FindBySql<ClientesInadimplentes>($@"Select 
                                        cli.Pessoa as PessoaProviderId,
                                        pes.Nome,
                                        Case when pes.Tipo = 'F' then pes.cpf else pes.Cnpj end as CpfCnpj, 
                                        pes.email,  
                                        Sum(crp.SaldoPendente * (Case When lower(tcr.Nome) like '%taxa%manut%' then 1 else 0 end)) as TotalInadimplenciaCondominio,
                                        Sum(crp.SaldoPendente * (Case When lower(tcr.Nome) not like '%taxa%manut%' then 1 else 0 end)) as TotalInadimplenciaContrato
                                     From 
                                        ContaReceberParcela crp
                                        Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id
                                        Inner Join ContaReceber cr on crp.ContaReceber = cr.Id 
                                        Inner Join Cliente cli on cr.Cliente = cli.Id 
                                        Inner Join Pessoa pes ON cli.Pessoa = pes.Id
                                    Where
                                        cli.Pessoa in ({string.Join(",", item)}) and 
                                        crp.Vencimento <= :dataCorte and 
                                        crp.Status = 'P' and 
                                        crp.SaldoPendente > 0 {strQueryAdicional} 
                                    Group by cli.Pessoa, pes.Nome, Case when pes.Tipo = 'F' then pes.cpf else pes.Cnpj end, pes.Email",
                                        new Parameter("dataCorte", DateTime.Today.AddDays(-5).Date))).AsList();

                    pessoasComPendenciaFinanceiras.AddRange(pendenciasFinanceiras);
                }
            }
            else
            {

                var itens = await _cacheStore.GetAsync<List<ClientesInadimplentes>>(CACHE_CLIENTES_INADIMPLENTES_KEY, 10, _systemRepository.CancellationToken);
                if (itens != null && itens.Any()) return itens;

                var pendenciasFinanceiras =
                                        (await _repositoryAccessCenter.FindBySql<ClientesInadimplentes>($@"Select 
                                        cli.Pessoa as PessoaProviderId,
                                        pes.Nome,
                                        Case when pes.Tipo = 'F' then pes.cpf else pes.Cnpj end as CpfCnpj, 
                                        pes.email,  
                                        Sum(crp.SaldoPendente * (Case When lower(tcr.Nome) like '%taxa%manut%' then 1 else 0 end)) as TotalInadimplenciaCondominio,
                                        Sum(crp.SaldoPendente * (Case When lower(tcr.Nome) not like '%taxa%manut%' then 1 else 0 end)) as TotalInadimplenciaContrato
                                     From 
                                        ContaReceberParcela crp
                                        Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id
                                        Inner Join ContaReceber cr on crp.ContaReceber = cr.Id 
                                        Inner Join Cliente cli on cr.Cliente = cli.Id 
                                        Inner Join Pessoa pes ON cli.Pessoa = pes.Id
                                    Where
                                        crp.Vencimento <= :dataCorte and 
                                        crp.Status = 'P' and 
                                        crp.SaldoPendente > 0 {strQueryAdicional} 
                                    Group by cli.Pessoa, pes.Nome, Case when pes.Tipo = 'F' then pes.cpf else pes.Cnpj end, pes.Email",
                                        new Parameter("dataCorte", DateTime.Today.AddDays(-5).Date))).AsList();

                pessoasComPendenciaFinanceiras.AddRange(pendenciasFinanceiras);

                await _cacheStore.AddAsync(CACHE_CLIENTES_INADIMPLENTES_KEY, pessoasComPendenciaFinanceiras, DateTimeOffset.Now.AddMinutes(10), 10, _systemRepository.CancellationToken);
            }

            return pessoasComPendenciaFinanceiras;
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false)
        {
            try
            {
                if (!simulacao)
                {
                    var sb = new StringBuilder(
                        @$"SELECT
                        r.Id as ReservaId,
                        pe.Email as EmailCliente,
                        r.Checkin as DataCheckIn,
                        h.Empresa as EmpresaId,
                        pcd.Id as AgendamentoId,
                        uc.Numero as UhCondominioNumero,
                        ct.Nome as CotaNome
                    FROM 
                        Reserva r
                        Inner Join PeriodoCotaDisponibilidade pcd on r.PeriodoCotaDisponibilidade = pcd.Id and pcd.TipoDisponibilizacao in ('U','C')
                        Inner Join Hotel h on r.Hotel = h.Id
                        Inner Join ReservaCliente rc on r.Id = rc.Reserva and rc.Principal = 'S'
                        Inner Join Cliente c on rc.Cliente = c.Id
                        Inner Join Pessoa p on c.Pessoa = p.Id
                        Inner Join PessoaEmail pe on p.Id = pe.Pessoa and pe.EnviarVoucher = 'S'
                        Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota
                        Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null
                        Inner Join Cliente proca on pro.Cliente = proca.Id
                        Inner Join Pessoa procp on proca.Pessoa = procp.Id
                        Inner Join UhCondominio uc on uc.Id = cp.UhCondominio
                        Inner Join Cota ct on ct.Id = cp.Cota
                    WHERE 
                        r.Status != 'CL' and 
                        pe.Email like '%@%' 
                        and CAST(r.Checkin AS DATE) = @dataCheckIn");

                    var parameter = new Parameter("dataCheckIn", checkInDate.Date);
                    return (await _repositoryPortalEsol.FindBySql<ReservaInfo>(sb.ToString(), parameter)).AsList();
                }
                else
                {
                    var sb = new StringBuilder(
                        @$"SELECT Top 50
                        r.Id as ReservaId,
                        pe.Email as EmailCliente,
                        r.Checkin as DataCheckIn,
                        h.Empresa as EmpresaId,
                        pcd.Id as AgendamentoId,
                        uc.Numero as UhCondominioNumero,
                        ct.Nome as CotaNome
                    FROM 
                        Reserva r
                        Inner Join PeriodoCotaDisponibilidade pcd on r.PeriodoCotaDisponibilidade = pcd.Id and pcd.TipoDisponibilizacao in ('U','C')
                        Inner Join Hotel h on r.Hotel = h.Id
                        Inner Join ReservaCliente rc on r.Id = rc.Reserva and rc.Principal = 'S'
                        Inner Join Cliente c on rc.Cliente = c.Id
                        Inner Join Pessoa p on c.Pessoa = p.Id
                        Inner Join PessoaEmail pe on p.Id = pe.Pessoa and pe.EnviarVoucher = 'S'
                        Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota
                        Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null
                        Inner Join Cliente proca on pro.Cliente = proca.Id
                        Inner Join Pessoa procp on proca.Pessoa = procp.Id
                        Inner Join UhCondominio uc on uc.Id = cp.UhCondominio
                        Inner Join Cota ct on ct.Id = cp.Cota
                    WHERE 
                        r.Status != 'CL' and 
                        pe.Email like '%@%' 
                        and CAST(r.Checkin AS DATE) = @dataCheckIn
                    Order by CAST(r.Checkin AS DATE)");

                    var parameter = new Parameter("dataCheckIn", checkInDate.Date);
                    var resultado = (await _repositoryPortalEsol.FindBySql<ReservaInfo>(sb.ToString(), parameter)).AsList();

                    if (!resultado.Any())
                    {
                        var sbRange = new StringBuilder(
                            @$"SELECT Top 50
                            r.Id as ReservaId,
                            pe.Email as EmailCliente,
                            r.Checkin as DataCheckIn,
                            h.Empresa as EmpresaId,
                            pcd.Id as AgendamentoId,
                            uc.Numero as UhCondominioNumero,
                            ct.Nome as CotaNome
                        FROM 
                            Reserva r
                            Inner Join PeriodoCotaDisponibilidade pcd on r.PeriodoCotaDisponibilidade = pcd.Id and pcd.TipoDisponibilizacao in ('U','C')
                            Inner Join Hotel h on r.Hotel = h.Id
                            Inner Join ReservaCliente rc on r.Id = rc.Reserva and rc.Principal = 'S'
                            Inner Join Cliente c on rc.Cliente = c.Id
                            Inner Join Pessoa p on c.Pessoa = p.Id
                            Inner Join PessoaEmail pe on p.Id = pe.Pessoa and pe.EnviarVoucher = 'S'
                            Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota
                            Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null
                            Inner Join Cliente proca on pro.Cliente = proca.Id
                            Inner Join Pessoa procp on proca.Pessoa = procp.Id
                            Inner Join UhCondominio uc on uc.Id = cp.UhCondominio
                            Inner Join Cota ct on ct.Id = cp.Cota
                        WHERE 
                            r.Status != 'CL' and 
                            pe.Email like '%@%' 
                            and CAST(r.Checkin AS DATE) between @dataCheckInInicial and @dataCheckinFinal 
                        Order by CAST(r.Checkin AS DATE)");

                        var dataInicial = checkInDate.Date.AddDays(-15);
                        var dataFinal = checkInDate.Date.AddDays(15);

                        var parameterInicial = new Parameter("dataCheckInInicial", dataInicial);
                        var parameterFinal = new Parameter("dataCheckinFinal", dataFinal);

                        resultado = (await _repositoryPortalEsol.FindBySql<ReservaInfo>(sbRange.ToString(), parameterInicial, parameterFinal)).AsList();
                    }

                    return resultado;
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException();
        }

        public bool? ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            var contrato = contratos?.FirstOrDefault(c =>
                !string.IsNullOrEmpty(reserva.CotaNome) && !string.IsNullOrEmpty(c.GrupoCotaTipoCotaNome) &&
                 c.GrupoCotaTipoCotaNome.Equals(reserva.CotaNome, StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrEmpty(reserva.UhCondominioNumero) && !string.IsNullOrEmpty(c.NumeroImovel) &&
                 c.NumeroImovel.Equals(reserva.UhCondominioNumero, StringComparison.OrdinalIgnoreCase)
            );

            if (contrato == null)
            {
                _logger.LogWarning("Contrato não encontrado para reserva {ReservaId}. Considerando compatível para simulação.",
                    reserva.ReservaId);
                return true;
            }

            if (contrato.Status != "A")
            {
                _logger.LogDebug("Contrato inativo para reserva {ReservaId}", reserva.ReservaId);
                return false;
            }

            if (config.ExcludedStatusCrcIds != null && config.ExcludedStatusCrcIds.Any())
            {
                var statusCrcAtivos = contrato.frAtendimentoStatusCrcModels?
                    .Where(s => s.AtendimentoStatusCrcStatus == "A" && !string.IsNullOrEmpty(s.FrStatusCrcId))
                    .Select(s => int.Parse(s.FrStatusCrcId!))
                    .ToList() ?? new List<int>();

                if (statusCrcAtivos.Any(statusId => config.ExcludedStatusCrcIds.Contains(statusId)))
                {
                    _logger.LogDebug("Reserva {ReservaId} possui Status CRC excluído", reserva.ReservaId);
                    return false;
                }
            }

            if (config.SendOnlyToAdimplentes)
            {
                var temBloqueio = contrato.frAtendimentoStatusCrcModels?.Any(s =>
                    s.AtendimentoStatusCrcStatus == "A" &&
                    (s.BloquearCobrancaPagRec == "S" || s.BloqueaRemissaoBoletos == "S")) ?? false;

                var clienteInadimplente = inadimplentes?.FirstOrDefault(c =>
                    c.CpfCnpj != null && contrato.PessoaTitular1CPF != null &&
                     c.CpfCnpj.ToString() == contrato.PessoaTitular1CPF ||
                    c.CpfCnpj != null && contrato.PessoaTitular2CPF != null &&
                     c.CpfCnpj.ToString() == contrato.PessoaTitular2CPF
                );

                if (temBloqueio || clienteInadimplente != null)
                {
                    _logger.LogDebug("Reserva {ReservaId} possui inadimplência ou bloqueio", reserva.ReservaId);
                    return false;
                }
            }

            return true;
        }
    }
}
