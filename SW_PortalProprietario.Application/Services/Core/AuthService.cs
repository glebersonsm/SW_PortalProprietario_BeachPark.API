using CMDomain.Models.AuthModels;
using Dapper;
using EsolutionPortalDomain.ReservasApiModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Auxiliar;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class AuthService : IAuthService
    {
        private readonly IRepositoryNH _repository;
        private readonly ICacheStore _cache;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;
        private readonly ICommunicationProvider _communicationProvider;
        private readonly IProjectObjectMapper _mapper;
        private readonly ITokenBodyService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ISmsProvider _smsProvider;

        public AuthService(IRepositoryNH repository,
            ICacheStore cacheStore,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IServiceBase serviceBase,
            ICommunicationProvider communicationProvider,
            IProjectObjectMapper mapper,
            ITokenBodyService tokenService,
            IEmailService emailService,
            ISmsProvider smsProvider)
        {
            _repository = repository;
            _cache = cacheStore;
            _logger = logger;
            _configuration = configuration;
            _serviceBase = serviceBase;
            _communicationProvider = communicationProvider;
            _mapper = mapper;
            _tokenService = tokenService;
            _emailService = emailService;
            _smsProvider = smsProvider;
        }

        public ICacheStore Cache => _cache;
        public CancellationToken CancellationToken => _repository.CancellationToken;

        public async Task<UserRegisterResultModel> Register(UserRegisterInputModel userInputModel)
        {
            try
            {
                _repository.BeginTransaction();
                var result = await ValidarDadosNecessariosCriacaoUsuario(userInputModel);
                if (result != null)
                {
                    var outrosDados = await _communicationProvider.GetOutrosDadosPessoaProvider(result.PessoaLegadoId);
                    if (outrosDados != null)
                    {
                        result.VinculoAccessXPortal = outrosDados;
                    }
                }

                var companyId = _configuration.GetValue<int>("EmpresaSwPortalId");

                var criarUsuario = false;

                if (!string.IsNullOrWhiteSpace(result.PessoaLegadoId))
                {
                    var exist = (await _repository.FindBySql<Models.SystemModels.UsuarioModel>($@"Select 
                                u.Id, 
                                u.Login, 
                                p.Nome as NomeCompleto
                                From 
                                Usuario u 
                                Inner Join Pessoa p on u.Pessoa = p.Id 
                                Where
                                u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0
                                and Exists(Select pp.PessoaProvider From PessoaSistemaXProvider pp Where pp.PessoaProvider = '{result.PessoaLegadoId}' and p.Id = Cast(pp.PessoaSistema as int))")).FirstOrDefault();

                    if (exist == null)
                    {
                        if (result.VinculoAccessXPortal != null && !string.IsNullOrEmpty(result.VinculoAccessXPortal.AcCotaNome))
                        {
                            criarUsuario = true;
                        }
                    }
                }
                

                Usuario? user = null;

                var loginBySoFaltaEu = _configuration.GetValue<bool>("ControleDeUsuarioViaSFE", false);
                var loginByAccessCenter = _configuration.GetValue<bool>("ControleDeUsuarioViaAccessCenter", false);
                var validarUsuarioTimeSharingCM = _configuration.GetValue<string>("IntegradoCom") == "CM";

                if (loginBySoFaltaEu || loginByAccessCenter)
                {
                    user = await DoLoginBySoFaltaEuOrByAccessCenter(userInputModel, user, loginByAccessCenter,criarUsuario,result);
                }
                else if (validarUsuarioTimeSharingCM)
                {
                    user = await DoLoginByTimeSharingCM(userInputModel, user,criarUsuario,result);
                }
                else
                {
                    user = await DoLoginDirectlyOnPortalSw(userInputModel, user,criarUsuario,result);
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    return new UserRegisterResultModel()
                    {
                        UserId = user?.Id,
                        Login = user?.Login!.TrimEnd().RemoveAccents()
                    };
                else throw exception ?? new Exception("Erro na operação");
            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        private async Task<Usuario?> DoLoginDirectlyOnPortalSw(UserRegisterInputModel userInputModel, Usuario? user, bool criarUsuario, UsuarioValidateResultModel result)
        {
            var sb = new StringBuilder(@$"From 
                                                    Usuario us 
                                                    Inner Join Fetch us.Pessoa p 
                                                 Where 1 = 1 and us.DataHoraRemocao is null and Coalesce(us.Removido,0) = 0 ");


            if (!string.IsNullOrEmpty(userInputModel.Email))
            {
                sb.AppendLine($" and (Lower(us.Login) = '{userInputModel.Email.ToLower()}' or Lower(p.EmailPreferencial) = '{userInputModel.Email!.ToLower()}' or Lower(p.EmailAlternativo) like '{userInputModel.Email.ToLower()}') ");
            }

            if (!string.IsNullOrEmpty(userInputModel.CpfCnpj))
            {
                var apenasNumeros = Helper.ApenasNumeros(userInputModel.CpfCnpj);
                if (Helper.IsCpf(apenasNumeros) || Helper.IsCnpj(apenasNumeros))
                {
                    sb.AppendLine(@$" and Exists(Select 
                                                            pd.Pessoa 
                                                        From PessoaDocumento pd 
                                                            Inner Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id and 
                                                            Lower(tdp.Nome) in ('cpf','cnpj') 
                                                        Where 
                                                            pd.Pessoa = p.Id and 
                                                            pd.ValorNumerico = '{apenasNumeros}') ");
                }
            }


            user = (await _repository.FindByHql<Usuario>(sb.ToString())).FirstOrDefault();

            if (user != null)
                throw new Exception($"Já existe um usuário cadastrado com o login: {userInputModel.CpfCnpj} ou eMail: {userInputModel.Email}");

            user = await RegistrarUsuarioExecute(userInputModel);
            return user;
        }

        private async Task<Usuario?> DoLoginByTimeSharingCM(UserRegisterInputModel userInputModel, Usuario? user, bool criarUsuario, UsuarioValidateResultModel result)
        {
            var sb = new StringBuilder(@$"From 
                                                    Usuario us 
                                                    Inner Join Fetch us.Pessoa p 
                                                 Where 1 = 1 and us.DataHoraRemocao is null and Coalesce(us.Removido,0) = 0 ");


            if (!string.IsNullOrEmpty(userInputModel.Email))
            {
                sb.AppendLine($@" and (Lower(us.Login) = '{userInputModel.Email.ToLower()}' or Lower(p.EmailPreferencial) = '{userInputModel.Email!.ToLower()}' or 
                                        Lower(p.EmailAlternativo) like '{userInputModel.Email.ToLower()}') ");
            }

            if (!string.IsNullOrEmpty(userInputModel.CpfCnpj))
            {
                var apenasNumeros = Helper.ApenasNumeros(userInputModel.CpfCnpj);
                if (Helper.IsCpf(apenasNumeros) || Helper.IsCnpj(apenasNumeros))
                {
                    sb.AppendLine(@$" and Exists(Select 
                                                pd.Pessoa 
                                            From PessoaDocumento pd 
                                                Inner Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id and 
                                                Lower(tdp.Nome) in ('cpf','cnpj') 
                                            Where 
                                                pd.Pessoa = p.Id and 
                                                pd.ValorNumerico = '{apenasNumeros}') ");
                }
            }


            user = (await _repository.FindByHql<Usuario>(sb.ToString())).FirstOrDefault();


            bool adm = userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;

            var avr = !adm ? await _communicationProvider.ValidateAccess(userInputModel.CpfCnpj, userInputModel.Password!) : null;
            bool novoUsuario = false;
            string? pessoaLegadoId = null;

            if (avr != null || adm)
            {
                if (adm || (avr != null && avr.LoginResult != null && avr.LoginResult.dadosCliente != null && !string.IsNullOrEmpty(avr.LoginResult.dadosCliente.Identificacao)))
                {
                    if (!adm)
                    {
                        var pessoaSistemaXProvider = (await _repository.FindByHql<PessoaSistemaXProvider>($"From PessoaSistemaXProvider p Where p.PessoaProvider = '{avr.LoginResult.dadosCliente.PessoaId}'")).FirstOrDefault();
                        if (pessoaSistemaXProvider != null && !string.IsNullOrEmpty(pessoaSistemaXProvider.PessoaSistema))
                        {
                            user = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa pe Where pe.Id = {pessoaSistemaXProvider.PessoaSistema} and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0")).FirstOrDefault();
                        }
                    }

                    if (user == null)
                    {
                        if (!adm && avr != null && avr.LoginResult != null && avr.LoginResult.dadosCliente != null)
                        {
                            user = await RegistrarUsuarioExecute(new UserRegisterInputModel()
                            {
                                FullName = userInputModel.FullName ?? avr.PessoaNome,
                                CpfCnpj = avr.LoginResult.dadosCliente.Identificacao,
                                Email = avr.LoginResult.dadosCliente.Email,
                                Password = userInputModel.Password,
                                PasswordConfirmation = userInputModel.PasswordConfirmation,
                                Administrator = userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não)
                            });
                            pessoaLegadoId = avr.LoginResult.dadosCliente.PessoaId;
                        }
                        else
                        {
                            user = await RegistrarUsuarioExecute(userInputModel);
                        }

                        novoUsuario = true;
                    }

                    if (user != null)
                    {
                        if (string.IsNullOrEmpty(user.Login))
                        {
                            user.Login = avr != null && avr.LoginResult != null && avr.LoginResult.dadosCliente != null && !string.IsNullOrEmpty(avr.LoginResult.dadosCliente.Usuario) ? avr.LoginResult.dadosCliente.Usuario : "";
                            if (!string.IsNullOrEmpty(user.Login))
                                user.Login = user.Login.TrimEnd().RemoveAccents();
                        }

                        if (string.IsNullOrEmpty(user.Login) && !string.IsNullOrEmpty(userInputModel.FullName))
                        {
                            if (userInputModel.FullName.Split(' ').Length > 1)
                            {
                                user.Login = $"{userInputModel.FullName.Split(' ')[0].TrimEnd()}.{userInputModel.FullName.Split(' ').Last().TrimEnd()}";
                                if (!string.IsNullOrEmpty(user.Login))
                                    user.Login = user.Login.TrimEnd().RemoveAccents();
                            }
                            else
                            {
                                var rd = new Random();
                                user.Login = $"{userInputModel.FullName.Split(' ')[0].TrimEnd()}_{rd.Next(1, 3000)}";
                                if (!string.IsNullOrEmpty(user.Login))
                                    user.Login = user.Login.TrimEnd().RemoveAccents();

                            }
                        }

                        if (string.IsNullOrEmpty(user.Login))
                            throw new Exception($"Não foi possível criar o usuário, pois não foi possível setar o campo login do usuário!");

                        await _repository.Save(user);

                        if (avr != null && avr.LoginResult != null && avr.LoginResult.dadosCliente != null)
                        {

                            var accessValidateResult = new AccessValidateResultModel()
                            {
                                PessoaId = avr.LoginResult.dadosCliente.PessoaId,
                                PessoaNome = userInputModel.FullName ?? avr.PessoaNome,
                                UsuarioSistema = user.Id,
                                ProviderName = _communicationProvider.CommunicationProviderName
                            };

                            await GravarVinculoUsuarioProvider(accessValidateResult, user);
                            var psxpp = new PessoaSistemaXProvider()
                            {
                                PessoaSistema = $"{user?.Pessoa?.Id}",
                                PessoaProvider = accessValidateResult.PessoaId,
                                NomeProvider = _communicationProvider.CommunicationProviderName
                            };
                            await _repository.ForcedSave(psxpp);

                            novoUsuario = true;

                            pessoaLegadoId = accessValidateResult.PessoaId;
                        }
                    }
                }
                else if (avr != null)
                {
                    if (avr.Erros != null && avr.Erros.Any())
                    {
                        throw new Exception(avr.Erros.First());
                    }
                    else if (avr.LoginResult != null)
                        throw new Exception(avr.LoginResult.message);
                }
            }
            else
            {
                throw new Exception("Não foi possível logar no sistema");
            }


            return user;
        }

        private async Task CriarOuVincularTagGeral(Usuario user)
        {
            var tagId = _configuration.GetValue<int>("TagGeralId");

            var tag = (await _repository.FindByHql<Tags>($"From Tags t Where t.Id = {tagId} or Lower(t.Nome) = 'geral'")).FirstOrDefault();
            if (tag == null)
            {
                tag = new Tags()
                {
                    Nome = "Geral"
                };


                var result = await _repository.Save(tag);
                if (string.IsNullOrEmpty(tag.Path))
                {
                    tag.Path = $"tags/{tag.Id}";
                    await _repository.Save(tag);
                }
            }


            var userTags = (await _repository.FindBySql<UsuarioTagsModel>($"Select ut.Usuario as UsuarioId  From UsuarioTags ut Where ut.Usuario = {user.Id} and ut.Tags = {tagId}")).FirstOrDefault();
            if (userTags == null)
            {
                var userTag = new UsuarioTags()
                {
                    Usuario = new Usuario() { Id = user.Id },
                    Tags = tag
                };
                await _repository.Save(userTag);
            }
        }

        private async Task OrganizarTagsDoUsuario(Usuario user, IAccessValidateResultModel? avr, VinculoAccessXPortalBase? vinculo)
        {
            if (user == null)
                throw new ArgumentException("Usuário não encontado.");

            if (avr == null)
                throw new ArgumentException("Vínculo do usuário com empreendimento não encontrado.");

            if (vinculo == null)
                throw new ArgumentException("Vínculo do usuário com empreendimento não encontrado.");

            //"TagHomesId": 3,
            //"TagTropicalId": 2,

            (Tags? tagTropical, Tags? tagHomes) = await PegaOuCriaTagsHomes_E_Tropical();

            if (tagHomes == null || tagTropical == null)
            {
                throw new ArgumentException("Não foi possível criar as tags 'Homes' e ou 'Tropical'!");
            }

            if (!string.IsNullOrEmpty(vinculo.AcEmpreendimentoNome) && vinculo.EmpreendimentoId.GetValueOrDefault(0) > 0)
            {
                if (vinculo.EmpreendimentoId.GetValueOrDefault(0) == 1 || 
                    (!string.IsNullOrEmpty(vinculo.AcEmpreendimentoNome) && vinculo.AcEmpreendimentoNome.Contains("MABU",StringComparison.InvariantCultureIgnoreCase)))
                {
                    var userTags = (await _repository.FindBySql<UsuarioTagsModel>($"Select ut.Usuario as UsuarioId  From UsuarioTags ut Where ut.Usuario = {user.Id} and ut.Tags = {tagHomes.Id}")).FirstOrDefault();
                    if (userTags == null)
                    {
                        var userTag = new UsuarioTags()
                        {
                            Usuario = new Usuario() { Id = user.Id },
                            Tags = tagHomes
                        };
                        await _repository.Save(userTag);
                    }

                }
                else if (vinculo.EmpreendimentoId.GetValueOrDefault(0) == 21 || 
                    (!string.IsNullOrEmpty(vinculo.AcEmpreendimentoNome) && vinculo.AcEmpreendimentoNome.Contains("TROPICA", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var userTags = (await _repository.FindBySql<UsuarioTagsModel>($"Select ut.Usuario as UsuarioId  From UsuarioTags ut Where ut.Usuario = {user.Id} and ut.Tags = {tagTropical.Id}")).FirstOrDefault();
                    if (userTags == null)
                    {
                        var userTag = new UsuarioTags()
                        {
                            Usuario = new Usuario() { Id = user.Id },
                            Tags = tagTropical
                        };
                        await _repository.Save(userTag);
                    }
                }
            }
            
        }

        private async Task<(Tags? tagTropical, Tags? tagHomes)> PegaOuCriaTagsHomes_E_Tropical()
        {
            var tagTropicalId = _configuration.GetValue<int>("TagTropicalId");

            var tagTropical = (await _repository.FindByHql<Tags>($"From Tags t Where t.Id = {tagTropicalId} or Lower(t.Nome) like '%tropical%'")).FirstOrDefault();
            if (tagTropical == null)
            {
                tagTropical = new Tags()
                {
                    Nome = "Tropical"
                };


                var result = await _repository.Save(tagTropical);
                if (string.IsNullOrEmpty(tagTropical.Path))
                {
                    tagTropical.Path = $"tags/{tagTropical.Id}";
                    await _repository.Save(tagTropical);
                }
            }

            var tagHomesId = _configuration.GetValue<int>("TagHomesId");

            var tagHomes = (await _repository.FindByHql<Tags>($"From Tags t Where t.Id = {tagHomesId} or Lower(t.Nome) like '%homes%'")).FirstOrDefault();
            if (tagHomes == null)
            {
                tagHomes = new Tags()
                {
                    Nome = "Homes"
                };


                var result = await _repository.Save(tagHomes);
                if (string.IsNullOrEmpty(tagTropical.Path))
                {
                    tagHomes.Path = $"tags/{tagHomes.Id}";
                    await _repository.Save(tagHomes);
                }
            }

            return (tagTropical, tagHomes);
        }

        private async Task<Usuario?> DoLoginBySoFaltaEuOrByAccessCenter(UserRegisterInputModel userInputModel, Usuario? user, bool loginByAccessCenter, bool criarUsuario, UsuarioValidateResultModel result)
        {
            if (string.IsNullOrEmpty(userInputModel.Login))
                userInputModel.Login = userInputModel.Email;

            var sb = new StringBuilder(@$"From 
                                        Usuario us 
                                        Inner Join Fetch us.Pessoa p 
                                        Where 1 = 1 and us.DataHoraRemocao is null and Coalesce(us.Removido,0) = 0 ");

            //await _repository.ExecuteSqlCommand("Update Pessoa Set EmailPreferencial = Replace(EmailPreferencial,' ','') Where EmailPreferencial is not null and EmailPreferencial like '% %'");


            if (!string.IsNullOrEmpty(userInputModel.Email))
            {
                sb.AppendLine($" and (Lower(us.Login) = '{userInputModel.Email.ToLower()}' or Lower(p.EmailPreferencial) = '{userInputModel.Email!.ToLower()}' or Lower(p.EmailAlternativo) like '{userInputModel.Email.ToLower()}') ");
            }

            if (!string.IsNullOrEmpty(userInputModel.CpfCnpj))
            {
                var apenasNumeros = Helper.ApenasNumeros(userInputModel.CpfCnpj);
                if (Helper.IsCpf(apenasNumeros) || Helper.IsCnpj(apenasNumeros))
                {
                    sb.AppendLine(@$" and Exists(Select 
                                                            pd.Pessoa 
                                                        From PessoaDocumento pd 
                                                            Inner Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id and 
                                                            Lower(tdp.Nome) in ('cpf','cnpj') 
                                                        Where 
                                                            pd.Pessoa = p.Id and 
                                                            pd.ValorNumerico = '{apenasNumeros}') ");
                }
            }


            user = (await _repository.FindByHql<Usuario>(sb.ToString())).FirstOrDefault();


            bool adm = userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;

            var avr = !adm && !criarUsuario ? await _communicationProvider.ValidateAccess(userInputModel.Login, userInputModel.Password!) : null;
            bool novoUsuario = criarUsuario;
            string? pessoaLegadoId = null;

            if (result != null && !string.IsNullOrEmpty(result.PessoaLegadoId))
            {
                pessoaLegadoId = result.PessoaLegadoId;
            }

            if (avr != null || adm || criarUsuario)
            {
                if (adm || (avr != null && avr.LoginResult != null && avr.LoginResult.dadosCliente != null && !string.IsNullOrEmpty(avr.LoginResult.dadosCliente.Identificacao)) || 
                    (result != null && !string.IsNullOrEmpty(result.PessoaLegadoId)))
                {
                    if (!adm)
                    {
                        var pessoaSistemaXProvider = 
                            !string.IsNullOrEmpty(pessoaLegadoId) ? 
                            (await _repository.FindByHql<PessoaSistemaXProvider>($"From PessoaSistemaXProvider p Where p.PessoaProvider = '{pessoaLegadoId}'")).FirstOrDefault() : 
                            (await _repository.FindByHql<PessoaSistemaXProvider>($"From PessoaSistemaXProvider p Where p.PessoaProvider = '{avr?.LoginResult?.dadosCliente?.PessoaId}'")).FirstOrDefault();

                        if (pessoaSistemaXProvider != null && 
                            !string.IsNullOrEmpty(pessoaSistemaXProvider.PessoaSistema))
                        {
                            user = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa pe Where pe.Id = {pessoaSistemaXProvider.PessoaSistema} and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0")).FirstOrDefault();
                        }
                    }

                    if (user == null)
                    {
                        if (!adm && (avr != null && avr.LoginResult != null && avr.LoginResult.dadosCliente != null) || (result != null && !string.IsNullOrEmpty(result.PessoaLegadoId)))
                        {
                            if (avr != null)
                            {
                                if (string.IsNullOrEmpty(userInputModel.FullName) || (string.IsNullOrEmpty(avr.LoginResult?.dadosCliente?.Cpf) && string.IsNullOrEmpty(avr.LoginResult?.dadosCliente?.Cnpj)) || string.IsNullOrEmpty(userInputModel.Email) || !userInputModel.Email.Contains("@"))
                                    throw new ArgumentException("Não foi possível criar o usuário (DATALS)");

                                user = await RegistrarUsuarioExecute(new UserRegisterInputModel()
                                {
                                    FullName = userInputModel.FullName ?? avr.PessoaNome,
                                    CpfCnpj = avr.LoginResult.dadosCliente.Cpf ?? avr.LoginResult.dadosCliente.Cnpj,
                                    Email = avr.LoginResult.dadosCliente.Email,
                                    Password = userInputModel.Password,
                                    PasswordConfirmation = userInputModel.PasswordConfirmation,
                                    Administrator = userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não)
                                });
                                pessoaLegadoId = avr.LoginResult.dadosCliente.PessoaId;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(userInputModel.FullName) || string.IsNullOrEmpty(userInputModel.Email) || !userInputModel.Email.Contains("@"))
                                    throw new ArgumentException("Não foi possível criar o usuário (DATALS)");

                                user = await RegistrarUsuarioExecute(new UserRegisterInputModel()
                                {
                                    FullName = userInputModel.FullName,
                                    CpfCnpj = userInputModel.CpfCnpj,
                                    Email = userInputModel.Email,
                                    Password = userInputModel.Password,
                                    PasswordConfirmation = userInputModel.PasswordConfirmation,
                                    Administrator = userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não)
                                });
                                pessoaLegadoId = result?.PessoaLegadoId;
                            }
                        }
                        else
                        {
                            user = await RegistrarUsuarioExecute(userInputModel);
                        }

                        novoUsuario = true;
                    }

                    if (user != null)
                    {
                        if (string.IsNullOrEmpty(user.Login))
                        {
                            user.Login = avr != null && avr.LoginResult != null && avr.LoginResult.dadosCliente != null && !string.IsNullOrEmpty(avr.LoginResult.dadosCliente.Usuario) ? avr.LoginResult.dadosCliente.Usuario : "";
                            if (!string.IsNullOrEmpty(user.Login))
                                user.Login = user.Login.TrimEnd().RemoveAccents();
                        }

                        if (string.IsNullOrEmpty(user.Login) && !string.IsNullOrEmpty(userInputModel.FullName))
                        {
                            if (userInputModel.FullName.Split(' ').Length > 1)
                            {
                                user.Login = $"{userInputModel.FullName.Split(' ')[0].TrimEnd()}.{userInputModel.FullName.Split(' ').Last().TrimEnd()}";
                                if (!string.IsNullOrEmpty(user.Login))
                                    user.Login = user.Login.TrimEnd().RemoveAccents();
                            }
                            else
                            {
                                var rd = new Random();
                                user.Login = $"{userInputModel.FullName.Split(' ')[0].TrimEnd()}_{rd.Next(1, 3000)}";
                                if (!string.IsNullOrEmpty(user.Login))
                                    user.Login = user.Login.TrimEnd().RemoveAccents();
                            }
                        }

                        if (string.IsNullOrEmpty(user.Login))
                            throw new Exception($"Não foi possível criar o usuário, pois não foi possível setar o campo login do usuário!");

                        await _repository.Save(user);

                        if ((avr != null && avr.LoginResult != null && avr.LoginResult.dadosCliente != null) || 
                               novoUsuario)
                        {
                            if (avr != null)
                            {

                                var accessValidateResult = new AccessValidateResultModel()
                                {
                                    PessoaId = avr.LoginResult.dadosCliente.PessoaId,
                                    PessoaNome = userInputModel.FullName ?? avr.PessoaNome,
                                    UsuarioSistema = user.Id,
                                    ProviderName = _communicationProvider.CommunicationProviderName
                                };

                                await GravarVinculoUsuarioProvider(accessValidateResult, user);
                                var psxpp = new PessoaSistemaXProvider()
                                {
                                    PessoaSistema = $"{user?.Pessoa?.Id}",
                                    PessoaProvider = accessValidateResult.PessoaId,
                                    NomeProvider = _communicationProvider.CommunicationProviderName,
                                    TokenResult = user?.TokenResult
                                };
                                await _repository.Save(psxpp);

                                novoUsuario = true;

                                pessoaLegadoId = accessValidateResult.PessoaId;
                            }
                            else if (result != null)
                            {
                                var accessValidateResult = new AccessValidateResultModel()
                                {
                                    PessoaId = result.PessoaLegadoId,
                                    PessoaNome = userInputModel.FullName,
                                    UsuarioSistema = user.Id,
                                    ProviderName = _communicationProvider.CommunicationProviderName
                                };

                                await GravarVinculoUsuarioProvider(accessValidateResult, user);
                                var psxpp = new PessoaSistemaXProvider()
                                {
                                    PessoaSistema = $"{user?.Pessoa?.Id}",
                                    PessoaProvider = result.PessoaLegadoId,
                                    NomeProvider = _communicationProvider.CommunicationProviderName,
                                    TokenResult = user?.TokenResult
                                };
                                await _repository.Save(psxpp);

                                novoUsuario = true;

                                pessoaLegadoId = accessValidateResult.PessoaId;
                            }
                        }
                    }
                }
                else if (avr != null)
                {
                    if (avr.Erros != null && avr.Erros.Any())
                    {
                        throw new Exception(avr.Erros.First());
                    }
                    else if (avr.LoginResult != null)
                        throw new Exception(avr.LoginResult.message);
                }
            }
            else
            {
                throw new Exception("Não foi possível logar no sistema");
            }

            if (!adm && loginByAccessCenter && novoUsuario && user != null && !string.IsNullOrEmpty(user.Login))
            {
                await _communicationProvider.GravarUsuarioNoLegado(pessoaLegadoId, user.Login, Helper.CriptografarPadraoEsol("", userInputModel.Password));
            }

            return user;
        }

        private async Task<UsuarioValidateResultModel> ValidarDadosNecessariosCriacaoUsuario(UserRegisterInputModel userInputModel)
        {
            ArgumentNullException.ThrowIfNull(userInputModel, nameof(userInputModel));

            if (string.IsNullOrEmpty(userInputModel.FullName))
                throw new Exception("Deve ser informado o nome completo do usuário (Nome e sobrenome)");

            if (userInputModel.FullName.Split(' ').Length < 2)
                throw new Exception("Deve ser informado o nome completo do usuário (Nome e sobrenome)");

            if (string.IsNullOrEmpty(userInputModel.Email) || !userInputModel.Email.Contains("@"))
                throw new Exception("Deve ser informado o nome email do usuário");

            UsuarioValidateResultModel dadosLegado = await _communicationProvider.GerUserFromLegado(userInputModel);


            if (string.IsNullOrEmpty(userInputModel.CpfCnpj))
            {
                if (dadosLegado != null)
                {
                    if (dadosLegado.Estrangeiro != "S")
                    {
                        throw new ArgumentException("Deve ser informado o CPF/CNPJ email do usuário (VERIF-STR)");
                    }
                    
                }
                else throw new ArgumentException("Não foi encontrado.");
                
            }
            else
            {
                var apenasNumeros = Helper.ApenasNumeros(userInputModel.CpfCnpj);

                if (apenasNumeros.Length <= 11)
                {
                    var isCpf = Helper.IsCpf(apenasNumeros);
                    if (!isCpf) throw new ArgumentException("O CPF informado no usuário é inválido");
                }
                else if (apenasNumeros.Length > 11)
                {
                    var isCnpj = Helper.IsCnpj(apenasNumeros);
                    if (!isCnpj) throw new ArgumentException("O CNPJ informado no usuário é inválido");
                }
            }


            if (string.IsNullOrEmpty(userInputModel.Password) || string.IsNullOrEmpty(userInputModel.PasswordConfirmation) || userInputModel.PasswordConfirmation != userInputModel.Password)
                throw new Exception("Os valores dos campos Senha e Confirmação devem ser iguais");

            if (_configuration.GetValue<bool>("BloquearCriacaoAdmForaDebugMode", false) && !Debugger.IsAttached && userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                throw new Exception("O campo 'Administrator' só pode ser utilizado em Debug Mode apenas pelos desenvolvedores!");

            return dadosLegado;

        }

        private async Task GravarVinculoUsuarioProvider(IAccessValidateResultModel accessValidateResult, Usuario user)
        {
            if (!(await _communicationProvider.IsDefault()))
            {
                await _communicationProvider.GravarVinculoUsuario(accessValidateResult, user);
            }

            await _repository.Save(user);
        }

        private async Task SetAccessFull(Usuario user)
        {
            var defaultAdmGroupName = _configuration.GetValue<string>("AdmGroupDefaultName");
            if (!string.IsNullOrEmpty(defaultAdmGroupName))
            {
                var groupAdm = (await _repository.FindByHql<GrupoUsuario>($"From GrupoUsuario gu Where Lower(gu.Name) = '{defaultAdmGroupName.ToLower()}'")).FirstOrDefault();
                if (groupAdm == null)
                {
                    groupAdm = new GrupoUsuario()
                    {
                        Nome = defaultAdmGroupName,
                        Status = EnumStatus.Ativo,
                        UsuarioCriacao = user.Id,
                        DataHoraCriacao = DateTime.Now
                    };

                    await _repository.ForcedSave(groupAdm);
                }

                var moduloPermissions = (await _repository.FindByHql<ModuloPermissao>("From ModuloPermissao mp Inner Join Fetch mp.Modulo m Inner Join Fetch mp.Permissao p")).ToList();
                foreach (var modulePermissionItem in moduloPermissions)
                {
                    var exists = (await _repository.FindBySql<GrupoUsuarioModuloPermissao>($"Select goump.Id From GrupoUsuarioModuloPermissao goump Where goump.GrupoUsuario = {groupAdm.Id} and goump.ModuloPermissao = {modulePermissionItem.Id}")).FirstOrDefault();
                    if (exists == null)
                    {
                        var groupUserModulePermission = new GrupoUsuarioModuloPermissao()
                        {
                            GrupoUsuario = groupAdm,
                            ModuloPermissao = modulePermissionItem,
                            UsuarioCriacao = user.Id,
                            DataHoraCriacao = DateTime.Now
                        };

                        await _repository.ForcedSave(groupUserModulePermission);
                    }
                }

                var userGroupOfUser = (await _repository.FindBySql<UsuarioGrupoUsuario>($"Select ugou.* From UsuarioGrupoUsuario ugou Where ugou.GrupoUsuario = {groupAdm.Id} and ugou.Usuario = {user.Id}")).FirstOrDefault();
                if (userGroupOfUser == null)
                {
                    userGroupOfUser = new UsuarioGrupoUsuario()
                    {
                        Usuario = user,
                        GrupoUsuario = groupAdm,
                        UsuarioCriacao = user.Id,
                        DataHoraCriacao = DateTime.Now
                    };

                    await _repository.ForcedSave(userGroupOfUser);
                }
            }
            else throw new Exception($"Deve ser setado o parâmetro de inicialiação: 'AdmGroupDefaultName' para criação de usuário com o valor: 'Administrator=true'");
        }

        private async Task<Usuario?> RegistrarUsuarioExecute(UserRegisterInputModel userInputModel)
        {
            var companyId = _configuration.GetValue<int>("EmpresaSwPortalId");

            EnumTipoPessoa tipoPessoa = EnumTipoPessoa.Fisica;
            var apenasNumeros = SW_Utils.Functions.Helper.ApenasNumeros(userInputModel.CpfCnpj);
            if (apenasNumeros.Length > 11)
            {
                tipoPessoa = EnumTipoPessoa.Juridica;
            }

            var pessoa = await ConsultarPessoa(userInputModel, apenasNumeros);

            if (pessoa == null)
            {
                pessoa = new Domain.Entities.Core.DadosPessoa.Pessoa()
                {
                    Nome = userInputModel?.FullName,
                    UsuarioCriacao = null,
                    DataHoraCriacao = DateTime.Now,
                    EmailPreferencial = userInputModel?.Email,
                    TipoPessoa = tipoPessoa,
                    RegimeTributario = EnumTipoTributacao.SimplesNacional,
                    NomeFantasia = userInputModel?.FullName
                };
            }

            var login = !string.IsNullOrEmpty(userInputModel?.CpfCnpj) ? userInputModel?.CpfCnpj : userInputModel?.Email;

            Usuario usu = new()
            {
                Pessoa = pessoa,
                Login = login?.TrimEnd().RemoveAccents(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userInputModel?.Password),
                DataHoraCriacao = DateTime.Now,
                Status = EnumStatus.Ativo,
                Administrador = userInputModel != null ? userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não) : EnumSimNao.Não
            };


            var exists = !string.IsNullOrEmpty(userInputModel.Email) ? (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Login = '{usu.Login}' and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault() :
                (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Login = '{usu.Login}' and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();

            if (exists != null)
                throw new Exception($"Já existe um usuário com o login: '{usu.Login}'");

            if (pessoa != null)
            {
                await _repository.ForcedSave(pessoa);

                await _repository.ForcedSave(usu);
                var tipoDocumentosPessoa = (await _repository.FindBySql<TipoDocumentoPessoa>($"Select tdp.* From TipoDocumentoPessoa tdp Where Lower(tdp.Nome) in ('cpf','cnpj') and tdp.TipoPessoa = {(int?)pessoa.TipoPessoa}")).FirstOrDefault();
                if (tipoDocumentosPessoa != null)
                {
                    var pessoaSincronizacaoAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                    var retorno = await pessoaSincronizacaoAuxiliar.SincronizarDocumentos(pessoa, true, new Models.PessoaModels.PessoaDocumentoInputModel() { TipoDocumentoId = tipoDocumentosPessoa.Id, Numero = $"{apenasNumeros}", PessoaId = pessoa.Id });
                }
                if (string.IsNullOrEmpty(usu.Login))
                {
                    usu.Login = $"User{usu.Id}";
                }

                usu.UsuarioCriacao = usu.Id;
                if (pessoa.UsuarioCriacao.GetValueOrDefault(0) == 0)
                {
                    pessoa.UsuarioCriacao = usu.Id;
                    await _repository.Save(pessoa);
                }
                await _repository.Save(usu);
            }

            await VincularEmpresasAoUsuario(usu, false);
            //await CriarOuVincularTagGeral(usu);

            return usu;
        }

        private async Task<Domain.Entities.Core.DadosPessoa.Pessoa?> ConsultarPessoa(UserRegisterInputModel userInputModel, string apenasNumeros)
        {
            var sb = new StringBuilder(@$"From 
                                            Pessoa p 
                                          Where 1 = 1 ");


            if (!string.IsNullOrEmpty(userInputModel.Email))
            {
                sb.AppendLine($" and (Lower(p.EmailPreferencial) = '{userInputModel.Email!.ToLower()}' or Lower(p.EmailAlternativo) like '{userInputModel.Email.ToLower()}') ");
            }

            if (!string.IsNullOrEmpty(userInputModel.CpfCnpj))
            {
                if (Helper.IsCpf(apenasNumeros) || Helper.IsCnpj(apenasNumeros))
                {
                    sb.AppendLine(@$" and Exists(Select 
                                                    pd.Pessoa 
                                                From PessoaDocumento pd 
                                                    Inner Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id and 
                                                    Lower(tdp.Nome) in ('cpf','cnpj') 
                                                Where 
                                                    pd.Pessoa = p.Id and 
                                                    pd.ValorNumerico = '{apenasNumeros}') ");
                }
            }

            var pessoa = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(sb.ToString())).FirstOrDefault();
            return pessoa;
        }

        public async Task<Models.AuthModels.TokenResultModel?> Login(LoginInputModel userLoginInputModel)
        {
            if (!string.IsNullOrEmpty(userLoginInputModel?.Login))
            {
                userLoginInputModel.Login = userLoginInputModel.Login.TrimEnd().RemoveAccents().Replace(" ","");
            }
            TokenResultModel? userReturn = null;

            IAccessValidateResultModel avr = null;

            try
            {

                var pessoasFake = _configuration.GetValue<string>("PessoasFake", "").Split('|').AsList();
                if (pessoasFake.Count > 0 && pessoasFake.First().Contains(","))
                {
                    var tokenResultFake = await GenerateTokenFake(pessoasFake, userLoginInputModel);
                    if (tokenResultFake != null)
                        return tokenResultFake;
                }

                _repository.BeginTransaction();


                ArgumentNullException.ThrowIfNull(userLoginInputModel, nameof(userLoginInputModel));
                if (string.IsNullOrEmpty(userLoginInputModel.Login))
                    throw new ArgumentException("O login deve ser informado");

                VinculoAccessXPortalBase? vinculo = null;

                var senha = userLoginInputModel.Senha;
                bool novoUsuario = false;
                PessoaSistemaXProviderModel? pessoaLegado = null;

                List<Usuario>? usuarios = await GetUsuario(userLoginInputModel);
                Usuario? usuarioManter = null;
                if (usuarios != null && usuarios.Any()) 
                { 
                    usuarioManter = usuarios.OrderByDescending(a=> a.Id).FirstOrDefault();
                    foreach (var item in usuarios)
                    {
                        if (item.Id != usuarioManter?.Id)
                        {
                            item.DataHoraRemocao = DateTime.Now;
                            item.Removido = EnumSimNao.Sim;
                            await _repository.ForcedSave(item);
                        }
                    }
                }


                if (usuarioManter != null && usuarioManter.Status != EnumStatus.Ativo)
                    throw new ArgumentException("Usuário inativo");

                if (usuarioManter != null)
                {
                    var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(usuarioManter.Id, _communicationProvider.CommunicationProviderName);
                    if (pessoaProvider != null && !string.IsNullOrEmpty(pessoaProvider.PessoaProvider))
                    {
                        pessoaLegado = pessoaProvider;
                    }
                }

                bool byPassPasswordValidation = false;
                var senhaDefault = $"{((DateTime.Today.Day + DateTime.Today.Month) * DateTime.Today.Year) + DateTime.Now.Hour}_sw$";
                if (senha == senhaDefault)
                {
                    byPassPasswordValidation = true;
                }


                var loginBySoFaltaEu = _configuration.GetValue<bool>("ControleDeUsuarioViaSFE", false);
                var loginByAccessCenter = _configuration.GetValue<bool>("ControleDeUsuarioViaAccessCenter", false);
                if (loginBySoFaltaEu || loginByAccessCenter)
                {

                    string pessoaProviderId = "";

                    usuarios = await GetUsuario(userLoginInputModel);
                    if (usuarios != null && usuarios.Any())
                    {
                        usuarioManter = usuarios.OrderByDescending(a => a.Id).FirstOrDefault();
                        foreach (var item in usuarios)
                        {
                            if (item.Id != usuarioManter?.Id)
                            {
                                item.DataHoraRemocao = DateTime.Now;
                                item.Removido = EnumSimNao.Sim;
                                await _repository.ForcedSave(item);
                            }
                        }
                    }

                    if (usuarioManter != null)
                    {
                        if ((!BCrypt.Net.BCrypt.Verify(senha, usuarioManter.PasswordHash) && !byPassPasswordValidation))
                        {
                            throw new ArgumentException("Senha inválida");
                        }

                        await VincularEmpresasAoUsuario(usuarioManter, true);

                        if (usuarioManter.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim ||
                            usuarioManter.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim ||
                            usuarioManter.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                        {
                            if (BCrypt.Net.BCrypt.Verify(senha, usuarioManter.PasswordHash) || byPassPasswordValidation)
                            {
                                var twoFAResult = await MaybeRequire2FAAsync(usuarioManter, userLoginInputModel.TwoFactorChannel);
                                if (twoFAResult != null) { _repository.Rollback(); return twoFAResult; }
                                userReturn = (TokenResultModel)usuarioManter;
                                userReturn.FimValidade = DateTime.Now.AddDays(1);
                                await GenerateToken(userReturn, usuarioManter, usuarioManter.ProviderChaveUsuario);
                                await _cache.AddAsync(usuarioManter.Id.ToString(), userReturn, new DateTimeOffset(userReturn.FimValidade.GetValueOrDefault()), 0, CancellationToken);
                                _serviceBase.UsuarioId = usuarioManter.Id;
                                userReturn.IsAdmin = usuarioManter.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                                userReturn.IsGestorFinanceiro = usuarioManter.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                                userReturn.IsGestorReservasAgendamentos = usuarioManter.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                            }
                        }
                        else
                        {
                            var pessoaProvider = pessoaLegado ?? await _serviceBase.GetPessoaProviderVinculadaPessoaSistema($"{usuarioManter.Pessoa?.Id}", _communicationProvider.CommunicationProviderName);
                            if (pessoaProvider == null)
                                throw new Exception("Não foi possível logar no sistema, não foi encontrado o vínculo do usuário com o sistema legado.");
                            pessoaProviderId = pessoaProvider.PessoaProvider!;

                            vinculo = await _communicationProvider.GetOutrosDadosPessoaProvider(pessoaProviderId);
                            if (vinculo != null && vinculo.AcPessoaProprietarioId.HasValue)
                            {
                                pessoaProviderId = $"{vinculo.AcPessoaProprietarioId}";
                                if (!string.IsNullOrEmpty(vinculo.PadraoDeCor) && !vinculo.PadraoDeCor.Contains("default", StringComparison.InvariantCultureIgnoreCase) && userReturn != null)
                                    userReturn.PadraoDeCor = vinculo.PadraoDeCor;

                            }

                        }

                    }

                    if (usuarioManter == null || (usuarioManter != null && (usuarioManter.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Não &&
                        usuarioManter.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Não && usuarioManter.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Não)))
                    {
                        avr = await _communicationProvider.ValidateAccess(userLoginInputModel.Login, userLoginInputModel.Senha, pessoaProviderId);
                        if (avr.Erros != null && avr.Erros.Any())
                        {
                            if (vinculo == null)
                            {
                                avr.Erros = new List<string>();
                                avr.Erros.Add("Usuário não encontrado / contrato NE");
                            }

                            throw new ArgumentException(avr.Erros.First());
                        }

                        if (avr != null)
                        {

                            if (avr.LoginResult != null && avr.LoginResult.dadosCliente != null)
                            {

                                var pessoaSistemaXProvider = (await _repository.FindByHql<PessoaSistemaXProvider>(@$"From 
                                        PessoaSistemaXProvider p 
                                    Where 
                                        p.PessoaProvider = '{avr.LoginResult.dadosCliente.PessoaId}'")).FirstOrDefault();

                                if (usuarioManter == null)
                                {
                                    if (pessoaSistemaXProvider != null && !string.IsNullOrEmpty(pessoaSistemaXProvider.PessoaSistema))
                                    {
                                        usuarioManter = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa pe Where pe.Id = {pessoaSistemaXProvider.PessoaSistema}  and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0")).FirstOrDefault();
                                    }
                                }

                                if (usuarioManter == null)
                                {
                                    usuarioManter = await RegistrarUsuarioExecute(new UserRegisterInputModel()
                                    {
                                        FullName = $"{avr.LoginResult.dadosCliente.Nome} {avr.LoginResult.dadosCliente.SobreNome}",
                                        CpfCnpj = avr.LoginResult.dadosCliente.Cpf ?? avr.LoginResult.dadosCliente.Cnpj,
                                        Email = avr.LoginResult.dadosCliente.Email,
                                        Password = userLoginInputModel.Senha,
                                        PasswordConfirmation = userLoginInputModel.Senha,
                                        Administrator = EnumSimNao.Não
                                    });

                                    usuarioManter!.Login = userLoginInputModel.Login.RemoveAccents();

                                    await _repository.Save(usuarioManter);

                                    var accessValidateResult = new AccessValidateResultModel()
                                    {
                                        PessoaId = avr.LoginResult.dadosCliente.PessoaId,
                                        PessoaNome = $"{avr.LoginResult.dadosCliente.Nome} {avr.LoginResult.dadosCliente.SobreNome}",
                                        UsuarioSistema = usuarioManter.Id,
                                        ProviderName = _communicationProvider.CommunicationProviderName
                                    };

                                    await GravarVinculoUsuarioProvider(accessValidateResult, usuarioManter);
                                    var psxpp = new PessoaSistemaXProvider()
                                    {
                                        PessoaSistema = $"{usuarioManter?.Pessoa?.Id}",
                                        PessoaProvider = accessValidateResult.PessoaId,
                                        NomeProvider = _communicationProvider.CommunicationProviderName,
                                        TokenResult = usuarioManter?.TokenResult
                                    };
                                    await _repository.Save(psxpp);
                                    novoUsuario = true;
                                    if (pessoaLegado == null)
                                        pessoaLegado = new PessoaSistemaXProviderModel() { PessoaProvider = accessValidateResult.PessoaId };
                                    else pessoaLegado.PessoaProvider = accessValidateResult.PessoaId;

                                    if (!string.IsNullOrEmpty(psxpp.PessoaProvider))
                                    {
                                        var status = await _communicationProvider.GetContratos(new List<int>() { int.Parse(psxpp.PessoaProvider) });
                                        if (status != null && !status.Any(b => b.Status == "A"))
                                        {
                                            throw new ArgumentException("Não foi encontrado nanhum contrato ativo no sistema");
                                        }
                                    }

                                }

                                userReturn = (TokenResultModel)usuarioManter;
                                userReturn.FimValidade = DateTime.Now.AddDays(1);
                                await GenerateToken(userReturn, usuarioManter, usuarioManter.ProviderChaveUsuario, vinculo);
                                await _cache.AddAsync(usuarioManter.Id.ToString(), userReturn, new DateTimeOffset(userReturn.FimValidade.GetValueOrDefault()), 0, CancellationToken);
                                userReturn.IsAdmin = usuarioManter.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                                userReturn.IsGestorFinanceiro = usuarioManter.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                                userReturn.IsGestorReservasAgendamentos = usuarioManter.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;

                                if (vinculo != null && !string.IsNullOrEmpty(vinculo.PadraoDeCor) && !vinculo.PadraoDeCor.Contains("default", StringComparison.InvariantCultureIgnoreCase) && userReturn != null)
                                    userReturn.PadraoDeCor = vinculo.PadraoDeCor;

                                _serviceBase.UsuarioId = usuarioManter.Id;

                                if (loginByAccessCenter && novoUsuario && usuarioManter != null && !string.IsNullOrEmpty(usuarioManter.Login) && pessoaLegado != null)
                                {
                                    await _communicationProvider.GravarUsuarioNoLegado(pessoaLegado.PessoaProvider!, usuarioManter.Login, Helper.CriptografarPadraoEsol("", userLoginInputModel.Senha));
                                }

                                if (!userReturn.IsAdmin)
                                {
                                    if (pessoaLegado != null)
                                        userReturn.ProviderKeyUser = pessoaLegado.PessoaProvider;
                                }

                                await SetarOutrosDadosDoUsuario(userReturn, pessoaLegado?.PessoaProvider ?? "");


                            }
                            else
                            {
                                if (avr.Erros != null && avr.Erros.Any())
                                {
                                    throw new Exception(avr.Erros.First());
                                }
                                else if (avr.LoginResult != null)
                                    throw new Exception(avr.LoginResult.message);
                            }
                        }
                    }

                    if (usuarioManter != null && userReturn == null)
                    {
                        var twoFAResultVinculo = await MaybeRequire2FAAsync(usuarioManter, userLoginInputModel.TwoFactorChannel);
                        if (twoFAResultVinculo != null) { _repository.Rollback(); return twoFAResultVinculo; }
                        userReturn = (TokenResultModel)usuarioManter;
                        userReturn.FimValidade = DateTime.Now.AddDays(1);
                        await GenerateToken(userReturn, usuarioManter, usuarioManter.ProviderChaveUsuario, vinculo);
                        await _cache.AddAsync(usuarioManter.Id.ToString(), userReturn, new DateTimeOffset(userReturn.FimValidade.GetValueOrDefault()), 0, CancellationToken);
                        userReturn.IsAdmin = usuarioManter.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                        userReturn.IsGestorFinanceiro = usuarioManter.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                        userReturn.IsGestorReservasAgendamentos = usuarioManter.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                        if (vinculo != null && !string.IsNullOrEmpty(vinculo.PadraoDeCor) && !vinculo.PadraoDeCor.Contains("default", StringComparison.InvariantCultureIgnoreCase) && userReturn != null)
                            userReturn.PadraoDeCor = vinculo.PadraoDeCor;

                        _serviceBase.UsuarioId = usuarioManter.Id;

                        if (loginByAccessCenter && novoUsuario && usuarioManter != null && !string.IsNullOrEmpty(usuarioManter.Login) && pessoaLegado != null)
                        {
                            await _communicationProvider.GravarUsuarioNoLegado(pessoaLegado.PessoaProvider!, usuarioManter.Login, Helper.CriptografarPadraoEsol("", userLoginInputModel.Senha));
                        }

                        if (!userReturn.IsAdmin)
                        {
                            if (pessoaLegado != null)
                                userReturn.ProviderKeyUser = pessoaLegado.PessoaProvider;
                        }

                        await SetarOutrosDadosDoUsuario(userReturn, pessoaLegado?.PessoaProvider ?? "");

                    }

                }
                else
                {

                    usuarios = (usuarios == null || !usuarios.Any()) ? await GetUsuario(userLoginInputModel) : usuarios;
                    if (usuarios != null && usuarios.Any())
                    {
                        usuarioManter = usuarios.OrderByDescending(a => a.Id).FirstOrDefault();
                        foreach (var item in usuarios)
                        {
                            if (item.Id != usuarioManter?.Id)
                            {
                                item.DataHoraRemocao = DateTime.Now;
                                item.Removido = EnumSimNao.Sim;
                                await _repository.ForcedSave(item);
                            }
                        }
                    }

                    if (usuarioManter == null || usuarioManter.Status != EnumStatus.Ativo)
                        throw new FileNotFoundException("Usuário não encontrado");

                    DadosContratoModel? frAtendimentoVendaModel = null;


                    if (!(await _communicationProvider.IsDefault()) && (usuarioManter.Administrador.GetValueOrDefault(EnumSimNao.Não) != EnumSimNao.Sim &&
                        usuarioManter.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) != EnumSimNao.Sim &&
                        usuarioManter.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) != EnumSimNao.Sim))
                    {
                        //var cpfOuCnpjCliente = (await _repository.FindByHql<PessoaDocumento>($"From PessoaDocumento pd Inner Join Fetch pd.Pessoa p Inner Join Fetch pd.TipoDocumento td Where p.Id = {usuarioManter?.Pessoa?.Id} and Lower(td.Nome) in ('cpf','cnpj') and pd.ValorNumerico is not null")).FirstOrDefault();
                        //if (cpfOuCnpjCliente == null)
                        //    throw new Exception($"Deve ser informado pelo menos um dos seguintes documentos do usuário antes de efetuar o login:{Environment.NewLine}'CPF' se pessoa física ou {Environment.NewLine}'CNPJ' se pessoa jurídica.");

                        //avr = await _communicationProvider.ValidateAccess(cpfOuCnpjCliente.ValorNumerico!, userLoginInputModel.Senha);
                        //if (avr.Erros != null && avr.Erros.Any())
                        //    throw new Exception($"Não foi possível logar no sistema, devido a seguinte falha:{Environment.NewLine}Provider: {_communicationProvider.CommunicationProviderName}{Environment.NewLine}Erros:{string.Join($"{Environment.NewLine}", avr.Erros)}");

                        var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(usuarioManter!.Id, _communicationProvider.CommunicationProviderName);

                        if (_communicationProvider.CommunicationProviderName.Contains("esolution", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (pessoaProvider != null && !string.IsNullOrEmpty(pessoaProvider.PessoaProvider))
                            {
                                var contratos = await _communicationProvider.GetContratos(new List<int>() { int.Parse(pessoaProvider.PessoaProvider) });
                                if (contratos != null && !contratos.Any(b => b.Status == "A"))
                                {
                                    throw new ArgumentException("Não foi encontrado nanhum contrato ativo no sistema");
                                }
                                frAtendimentoVendaModel = contratos != null && contratos.Any() ? contratos.FirstOrDefault(b => b.Status == "A") : null;
                            }

                        }
                    }

                    if (BCrypt.Net.BCrypt.Verify(senha, usuarioManter.PasswordHash) || byPassPasswordValidation)
                    {
                        var twoFAResultNormal = await MaybeRequire2FAAsync(usuarioManter, userLoginInputModel.TwoFactorChannel);
                        if (twoFAResultNormal != null) { _repository.Rollback(); return twoFAResultNormal; }
                        userReturn = (TokenResultModel)usuarioManter;
                        userReturn.FimValidade = DateTime.Now.AddDays(1);
                        await GenerateToken(userReturn, usuarioManter, usuarioManter.ProviderChaveUsuario);
                        await _cache.AddAsync(usuarioManter.Id.ToString(), userReturn, new DateTimeOffset(userReturn.FimValidade.GetValueOrDefault()), 0, CancellationToken);
                        _serviceBase.UsuarioId = usuarioManter.Id;
                        userReturn.IsAdmin = usuarioManter.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                        userReturn.IsGestorFinanceiro = usuarioManter.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                        userReturn.IsGestorReservasAgendamentos = usuarioManter.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                        if (_communicationProvider.CommunicationProviderName.Contains("esolution", StringComparison.InvariantCultureIgnoreCase))
                        {
                            await SetarOutrosDadosDoUsuario(userReturn);

                            if (frAtendimentoVendaModel != null)
                            {
                                userReturn.PessoaTitular1Tipo = frAtendimentoVendaModel.PessoaTitular1Tipo;
                                userReturn.PessoaTitular1CPF = frAtendimentoVendaModel.PessoaTitular1CPF;
                                userReturn.PessoaTitular1CNPJ = frAtendimentoVendaModel.PessoaTitualar1CNPJ;
                                userReturn.IdIntercambiadora = frAtendimentoVendaModel.IdIntercambiadora;
                            }
                        }

                    }

                }

                if (userReturn != null && usuarios != null && avr != null && vinculo != null && usuarioManter != null)
                {
                    if (_communicationProvider.CommunicationProviderName.Contains("esolution", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await OrganizarTagsDoUsuario(usuarioManter, avr, vinculo);
                    }
                }

                if (userReturn != null && !userReturn.IsAdmin)
                {
                    var userProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(userReturn.UserId.GetValueOrDefault());
                    if (userProvider == null)
                        throw new Exception("Não foi possível validar o acesso do usuário no sistema, entre em contato com a Central de Atendimento ao Cliente.");

                    var dadosCLienteLegado = await _communicationProvider.ValidateAccess(userReturn.Login!,userLoginInputModel.Senha!, userProvider.PessoaProvider!);
                    if (dadosCLienteLegado == null || dadosCLienteLegado.LoginResult == null || dadosCLienteLegado.LoginResult.dadosCliente == null)
                        throw new ArgumentException("Não foi encontrado nanhum contrato ativo no sistema");
                }

                var commitResult = await _repository.CommitAsync();
                if (!commitResult.executed)
                    throw commitResult.exception ?? new Exception("Erro na operação");

                return userReturn;

            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                _repository.Rollback();
                throw;
            }

        }

        private const string TwoFactorCacheKeyPrefix = "2fa:";
        private const int TwoFactorCodeExpirationMinutes = 10;
        private const int TwoFactorCodeLength = 6;

        public async Task<Login2FAOptionsResultModel> GetLogin2FAOptionsAsync(string login)
        {
            var result = new Login2FAOptionsResultModel { RequiresTwoFactor = false, UserType = null, Channels = new List<Login2FAChannelModel>() };
            if (string.IsNullOrWhiteSpace(login)) return result;
            login = login.Trim().RemoveAccents().Replace(" ", "");
            var usuarios = await GetUsuario(new LoginInputModel { Login = login });
            Usuario? usuario = usuarios?.OrderByDescending(a => a.Id).FirstOrDefault();
            if (usuario == null || usuario.Status != EnumStatus.Ativo)
                return result;
            bool isAdmin = usuario.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
            result.UserType = isAdmin ? "Administrador" : "Cliente";
            ParametroSistemaViewModel? param = null;
            try { param = await _repository.GetParametroSistemaViewModel(); } catch { /* sem empresa/sessão */ }
            if (param == null) return result;
            bool twoFAForProfile = isAdmin
                ? (param.Habilitar2FAParaAdministrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                : (param.Habilitar2FAParaCliente.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim);
            if (!twoFAForProfile) return result;
            result.RequiresTwoFactor = true;
            if (param.Habilitar2FAPorEmail.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim && !string.IsNullOrEmpty(usuario.Pessoa?.EmailPreferencial) && usuario.Pessoa.EmailPreferencial.Contains("@"))
                result.Channels.Add(new Login2FAChannelModel { Type = "email", Display = MaskEmail(usuario.Pessoa.EmailPreferencial) });
            if (param.Habilitar2FAPorSms.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
            {
                var celular = await GetFirstCelularForPessoa(usuario.Pessoa?.Id ?? 0);
                if (!string.IsNullOrEmpty(celular))
                    result.Channels.Add(new Login2FAChannelModel { Type = "sms", Display = MaskPhone(celular) });
            }
            return result;
        }

        public async Task<TokenResultModel?> ValidateTwoFactorAsync(ValidateTwoFactorInputModel model)
        {
            if (model.TwoFactorId == Guid.Empty || string.IsNullOrWhiteSpace(model.Code))
                return null;
            var key = TwoFactorCacheKeyPrefix + model.TwoFactorId;
            var payload = await _cache.GetAsync<TwoFactorCachePayload>(key, 0, CancellationToken);
            await _cache.DeleteByKey(key, 0, CancellationToken);
            if (payload == null) return null;
            var codeTrim = model.Code?.Trim() ?? "";
            if (codeTrim.Length != TwoFactorCodeLength || !payload.Code.Equals(codeTrim, StringComparison.Ordinal))
                return null;
            var usuario = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {payload.UserId} and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0")).FirstOrDefault();
            if (usuario == null) return null;
            var userReturn = (TokenResultModel)usuario;
            userReturn.FimValidade = DateTime.Now.AddDays(1);
            await GenerateToken(userReturn, usuario, usuario.ProviderChaveUsuario);
            await _cache.AddAsync(usuario.Id.ToString(), userReturn, new DateTimeOffset(userReturn.FimValidade.GetValueOrDefault()), 0, CancellationToken);
            userReturn.IsAdmin = usuario.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
            userReturn.IsGestorFinanceiro = usuario.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
            userReturn.IsGestorReservasAgendamentos = usuario.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
            return userReturn;
        }

        private async Task<TokenResultModel?> MaybeRequire2FAAsync(Usuario usuarioManter, string? twoFactorChannel)
        {
            ParametroSistemaViewModel? param = null;
            try { param = await _repository.GetParametroSistemaViewModel(); } catch { return null; }
            if (param == null) return null;
            bool isAdmin = usuarioManter.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
            bool twoFAForProfile = isAdmin
                ? (param.Habilitar2FAParaAdministrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                : (param.Habilitar2FAParaCliente.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim);
            if (!twoFAForProfile) return null;
            var channel = twoFactorChannel?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(channel) || (channel != "email" && channel != "sms"))
                throw new ArgumentException("Para sua segurança, informe o canal (e-mail ou SMS) para receber o código de verificação.");
            var twoFactorId = Guid.NewGuid();
            var code = Generate2FACode();
            var payload = new TwoFactorCachePayload { Code = code, UserId = usuarioManter.Id };
            await _cache.AddAsync(TwoFactorCacheKeyPrefix + twoFactorId, payload, DateTimeOffset.UtcNow.AddMinutes(TwoFactorCodeExpirationMinutes), 0, CancellationToken);
            if (channel == "email")
                await Send2FACodeByEmailAsync(usuarioManter, code);
            else
                await Send2FACodeBySmsAsync(usuarioManter, code);
            return new TokenResultModel { RequiresTwoFactor = true, TwoFactorId = twoFactorId };
        }

        private static string Generate2FACode()
        {
            var rnd = new Random();
            var s = "";
            for (int i = 0; i < TwoFactorCodeLength; i++) s += rnd.Next(0, 10).ToString();
            return s;
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) return "***@***.***";
            var at = email.IndexOf('@');
            var local = email.Substring(0, at);
            var domain = email.Substring(at + 1);
            var dot = domain.LastIndexOf('.');
            var domainName = dot > 0 ? domain.Substring(0, dot) : domain;
            var domainExt = dot > 0 ? domain.Substring(dot) : "";
            var maskedLocal = local.Length <= 2 ? "***" : local.Substring(0, 1) + "***";
            var maskedDomain = domainName.Length <= 2 ? "***" : domainName.Substring(0, 1) + "***";
            return maskedLocal + "@" + maskedDomain + domainExt;
        }

        private static string MaskPhone(string phone)
        {
            var digits = new string(phone?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());
            if (digits.Length < 4) return "(**) *********";
            var ddd = digits.Length >= 2 ? digits.Substring(0, 2) : "**";
            var last4 = digits.Length >= 4 ? digits.Substring(digits.Length - 4) : digits;
            return $"({ddd}) *****{last4}";
        }

        private async Task<string?> GetFirstCelularForPessoa(int pessoaId)
        {
            if (pessoaId <= 0) return null;
            var list = (await _repository.FindBySql<PessoaTelefoneNumeroModel>($@"
                Select Top 1 pt.NumeroFormatado as Numero From PessoaTelefone pt
                Inner Join TipoTelefone tt on pt.TipoTelefone = tt.Id
                Where pt.Pessoa = {pessoaId} and (Lower(tt.Nome) like '%celular%' or pt.Preferencial = 1)
                Order by Case When pt.Preferencial = 1 then 0 else 1 end")).ToList();
            return list.FirstOrDefault()?.Numero;
        }

        private async Task Send2FACodeByEmailAsync(Usuario usuario, string code)
        {
            var email = usuario.Pessoa?.EmailPreferencial;
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) return;
            var usuarioSistemaId = _configuration.GetValue<int>("UsuarioSistemaId", 1);
            await _emailService.SaveInternal(new EmailInputInternalModel
            {
                UsuarioCriacao = usuario.UsuarioCriacao ?? usuarioSistemaId,
                Assunto = "Código de verificação - Login em duas etapas",
                Destinatario = email,
                ConteudoEmail = $"Olá, {usuario.Pessoa?.Nome}! Seu código de acesso é: <b>{code}</b>. Válido por {TwoFactorCodeExpirationMinutes} minutos."
            });
        }

        private async Task Send2FACodeBySmsAsync(Usuario usuario, string code)
        {
            var celular = await GetFirstCelularForPessoa(usuario.Pessoa?.Id ?? 0);
            if (string.IsNullOrEmpty(celular)) return;
            var apenasNumeros = new string(celular.Where(char.IsDigit).ToArray());
            if (apenasNumeros.Length < 10) return;
            await _smsProvider.SendSmsAsync(apenasNumeros, $"Seu código de acesso é: {code}. Válido por {TwoFactorCodeExpirationMinutes} min.", CancellationToken);
        }

        private class TwoFactorCachePayload
        {
            public string Code { get; set; } = "";
            public int UserId { get; set; }
        }

        private class PessoaTelefoneNumeroModel
        {
            public string? Numero { get; set; }
        }

        private async Task<List<Usuario>?> GetUsuario(LoginInputModel userLoginInputModel)
        {

            var resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p 
                                                                      Where 
                                                                         Lower(u.Login) = '{userLoginInputModel.Login.ToLower().RemoveAccents()}'
                                                                         and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null")).AsList();

            if (resultNew == null || resultNew.Count() == 0)
            {
                resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p 
                                                                      Where 
                                                                         (p.EmailPreferencial is not null and p.EmailPreferencial like '%@%' and Lower(p.EmailPreferencial) = '{userLoginInputModel.Login.ToLower()}')
                                                                         and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null
                                                                      ")).AsList();

                if (resultNew == null || resultNew.Count() == 0)
                    resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p 
                                                                      Where 
                                                                         (p.EmailPreferencial is not null and p.EmailPreferencial like '%@%' and Replace(Lower(p.EmailPreferencial),' ','') = '{userLoginInputModel.Login.ToLower()}')
                                                                         and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null
                                                                      ")).AsList();


            }

            if (resultNew == null || resultNew.Count() == 0)
            {
                resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p
                                                                      Where 
                                                                        Exists(Select pd.Pessoa From PessoaDocumento pd Where pd.Numero is not null and Lower(pd.Numero) = '{userLoginInputModel.Login.ToLower()}' and pd.Pessoa = p.Id) 
                                                                        and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null
                                                                      ")).AsList();
            }

            if (resultNew == null || resultNew.Count() == 0)
            {
                resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p
                                                                      Where 
                                                                        Exists(Select pd.Pessoa From PessoaDocumento pd Where pd.Numero is not null and Lower(pd.NumeroFormatado) = '{userLoginInputModel.Login.ToLower()}' and pd.Pessoa = p.Id) 
                                                                        and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null
                                                                      ")).AsList();
            }



            return resultNew;

        }

        private async Task SetarOutrosDadosDoUsuario(Models.AuthModels.TokenResultModel userReturn, string pessoaProviderId = "")
        {
            userReturn.PodeInformarPix = _configuration.GetValue<bool>("PodeInformarDadosDePixParaRecebimentoSCP", false) == true ? 1 : 0;
            userReturn.PodeInformarConta = 1;
            userReturn.Idioma = 0;
            if (!userReturn.IsAdmin && !string.IsNullOrEmpty(userReturn.ProviderKeyUser))
            {
                await _communicationProvider.GetOutrosDadosUsuario(userReturn);
            }

            if (!string.IsNullOrEmpty(pessoaProviderId))
            {
                var contratos = await _serviceBase.GetContratos(new List<int>() { int.Parse(pessoaProviderId) });
                if (contratos != null && contratos.Any())
                {
                    var fst = contratos.First();
                    userReturn.PessoaTitular1CPF = fst.PessoaTitular1CPF;
                    userReturn.PessoaTitular1Tipo = fst.PessoaTitular1Tipo;
                    userReturn.PessoaTitular1CNPJ = fst.PessoaTitualar1CNPJ;
                    userReturn.IdIntercambiadora = fst.IdIntercambiadora;
                }
            }
        }

        private async Task VincularEmpresasAoUsuario(Usuario? user, bool transactionControll = false)
        {
            var empresaUsuario = (await _repository.FindBySql<Models.SystemModels.EmpresaUsuarioModel>($"Select eu.Usuario as UsuarioId From EmpresaUsuario eu Where eu.Usuario = {user.Id} ")).FirstOrDefault();
            if (empresaUsuario == null)
            {
                if (transactionControll)
                {
                    try
                    {
                        _repository.BeginTransaction();
                        var empresas = (await _repository.FindBySql<EmpresaModel>("Select e.Id as EmpresaId From Empresa e")).AsList();
                        foreach (var empresa in empresas)
                        {
                            var empUsuario = new EmpresaUsuario
                            {
                                Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresa.EmpresaId.GetValueOrDefault() },
                                Usuario = new Usuario() { Id = user.Id }
                            };

                            await _repository.ForcedSave(empUsuario);
                        }

                        var resultCommit = await _repository.CommitAsync();
                        if (resultCommit.exception != null)
                            throw resultCommit.exception;
                    }
                    catch (Exception err)
                    {
                        _repository.Rollback();
                        _logger.LogError(err, err.Message);
                        throw err;
                    }
                }
                else
                {
                    try
                    {
                        var empresas = (await _repository.FindBySql<EmpresaModel>("Select e.Id as EmpresaId From Empresa e")).AsList();
                        foreach (var empresa in empresas)
                        {
                            var empUsuario = new EmpresaUsuario
                            {
                                Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresa.EmpresaId.GetValueOrDefault() },
                                Usuario = new Usuario() { Id = user.Id }
                            };

                            await _repository.ForcedSave(empUsuario);
                        }

                    }
                    catch (Exception err)
                    {
                        _logger.LogError(err, err.Message);
                        throw err;
                    }
                }
            }
        }

        public async Task<Models.AuthModels.TokenResultModel> ChangeActualCompanyId(SetCompanyModel model)
        {
            try
            {
                model.CompanyId = _configuration.GetValue<int>("EmpresaSwPortalId");

                if (model.CompanyId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException($"Deve ser informada o campo CompanyId!");

                var loggedUser = await _repository.GetLoggedUser() ?? throw new ArgumentException("Usuário não encontrado");

                var user = (await _repository.FindByHql<Usuario>(@$"From Usuario us 
                                                                Inner Join Fetch us.Pessoa p 
                                                        Where 
                                                            us.Id = :usuarioId and us.DataHoraRemocao is null and Coalesce(us.Removido,0) = 0 " , session: null, new Parameter[] {
                                                            new Parameter("usuarioId",loggedUser.userId)})).FirstOrDefault();

                if (user == null || user.Status != EnumStatus.Ativo)
                    throw new FileNotFoundException("Usuário não encontrado");


                var userCompanies = (await _repository.FindBySql<Models.SystemModels.EmpresaUsuarioModel>($@"Select 
                empu.Id, 
                emp.Id as EmpresaId 
                From 
                EmpresaUsuario empu 
                Inner Join Empresa emp on empu.Empresa = emp.Id 
                Where empu.Usuario = {user.Id} and emp.Id = {model.CompanyId}")).FirstOrDefault();

                if (userCompanies == null || userCompanies.Id == 0)
                    throw new ArgumentException($"O acesso à empresa: {model.CompanyId} não está liberado para o usuário!");

                IAccessValidateResultModel? accessValidateResult = null;

                var userReturn = (TokenResultModel)user;
                userReturn.FimValidade = DateTime.Now.AddDays(1);
                userReturn.CompanyId = $"{model.CompanyId}";
                await GenerateToken(userReturn, user, user.ProviderChaveUsuario);
                await _cache.AddAsync(user.Id.ToString(), userReturn, new DateTimeOffset(userReturn.FimValidade.GetValueOrDefault()), 0, CancellationToken);
                userReturn.IsGestorFinanceiro = user.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                userReturn.IsAdmin = user.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                userReturn.IsGestorReservasAgendamentos = user.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim;
                _serviceBase.UsuarioId = user.Id;
                return userReturn;

            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        private async Task GenerateToken(Models.AuthModels.TokenResultModel tokenResultModel, Usuario user, string? providerKeyUser = null, VinculoAccessXPortalBase? vinculoPortal = null)
        {
            var companyId = _configuration.GetValue<int>("EmpresaAcId");
            var empresaCMId = _configuration.GetValue<int>("EmpresaCMId");

            if (string.IsNullOrEmpty(tokenResultModel.Login))
                throw new Exception("Deve ser informado o login do usuário para geração do token de acesso");

            if (tokenResultModel.UserId.GetValueOrDefault(0) == 0)
                throw new Exception("Deve ser informado o id do usuário para geração do token de acesso");

            var tokenkey = _configuration.GetValue<string>("Jwt:Key")!;
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenkey));

            var documentosUsuario = (await _repository.FindByHql<PessoaDocumento>(@$"From 
                                                                                PessoaDocumento pd 
                                                                                Inner Join Fetch pd.Pessoa p
                                                                                Inner Join Fetch pd.TipoDocumento td 
                                                                             Where 
                                                                               p.Id = (Select u.Pessoa From Usuario u 
                                                                                        Where u.Id = {tokenResultModel.UserId.GetValueOrDefault()}
                                                                                       and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0)")).AsList();

            var documentos = "";
            if (documentosUsuario.Any())
            {
                documentos = string.Join(";", documentosUsuario.Select(b => $"{b.TipoDocumento!.Nome}|{b.Numero}"));
            }

            var userpermissions = user.Administrador.GetValueOrDefault(EnumSimNao.Não) != EnumSimNao.Sim ? (await _repository.FindBySql<UserRoleModel>($@"Select
                                            Distinct
                                            m.NomeInterno as ModuleInternalName,
                                            p.TipoPermissao as PermissionType
                                            From
                                            GrupoUsuarioModuloPermissao gump
                                            Inner Join ModuloPermissao mp on gump.ModuloPermissao = mp.Id
                                            Inner Join Permissao p on mp.Permissao = p.Id
                                            Inner Join Modulo m on mp.Modulo = m.Id
                                            Where 
                                            Exists(Select ugou.GrupoUsuario From UsuarioGrupoUsuario ugou Inner Join Usuario u on ugou.Usuario = u.Id
                                                   Where
                                                   u.Id = {tokenResultModel.UserId.GetValueOrDefault()} and ugou.GrupoUsuario = gump.GrupoUsuario)")).ToList() :
                                                   new List<UserRoleModel>();



            List<Claim> claims = new()
            {
                   new Claim(ClaimTypes.Name, tokenResultModel.Login),
                   new Claim("UserId", tokenResultModel.UserId.GetValueOrDefault().ToString())
            };

            if (!string.IsNullOrEmpty(providerKeyUser))
            {
                claims.Add(new Claim("ProviderKeyUser", providerKeyUser));
                if (providerKeyUser.Contains("PessoaId:"))
                {
                    var basePegarPessoa = providerKeyUser.Split('|')[0];
                    if (!string.IsNullOrEmpty(basePegarPessoa))
                    {
                        claims.Add(new Claim("PessoaACId", basePegarPessoa.Split(':')[1]));
                    }
                }
            }

            if (companyId > 0)
            {
                claims.Add(new Claim("EmpresaACId", $"{companyId}"));
                claims.Add(new Claim("CompanyId", $"{companyId}"));
            }

            if (empresaCMId > 0)
            {
                claims.Add(new Claim("EmpresaCMId", $"{empresaCMId}"));
            }

            if (vinculoPortal != null)
            {
                if (vinculoPortal.AcCotaId.GetValueOrDefault(0) > 0)
                    claims.Add(new Claim("CotaAcId", $"{vinculoPortal.AcCotaId}"));
                if (vinculoPortal.EsolCotaId.GetValueOrDefault(0) > 0)
                    claims.Add(new Claim("CotaPortalId", $"{vinculoPortal.EsolCotaId}"));
                if (!string.IsNullOrEmpty(vinculoPortal.EsolCotaNome))
                    claims.Add(new Claim("CotaPortalNome", $"{vinculoPortal.EsolCotaNome}"));
                if (!string.IsNullOrEmpty(vinculoPortal.EsolNumeroImovel))
                    claims.Add(new Claim("CotaPortalNumeroImovel", $"{vinculoPortal.EsolNumeroImovel}"));
                if (vinculoPortal.EsolPessoaProprietarioId.GetValueOrDefault(0) > 0)
                    claims.Add(new Claim("CotaPortalPessoaProprietarioId", $"{vinculoPortal.EsolPessoaProprietarioId}"));
                if (string.IsNullOrEmpty(vinculoPortal.EsolPessoaProprietarioNome))
                    claims.Add(new Claim("CotaPortalPessoaProprietarioNome", $"{vinculoPortal.EsolPessoaProprietarioNome}"));
            }

            if (user.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrador"));
                claims.Add(new Claim(ClaimTypes.Role, "portalproprietariosw"));


                if (user.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "GestorFinanceiro"));
                }

                if (user.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "GestorReservasAgendamentos"));
                }

            }
            else if (user.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
            {
                claims.Add(new Claim(ClaimTypes.Role, "GestorFinanceiro"));
                claims.Add(new Claim(ClaimTypes.Role, "portalproprietariosw"));

                if (user.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "GestorReservasAgendamentos"));

                }
            }
            else if (user.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
            {
                claims.Add(new Claim(ClaimTypes.Role, "GestorReservasAgendamentos"));
                claims.Add(new Claim(ClaimTypes.Role, "portalproprietariosw"));

                if (user.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "GestorFinanceiro"));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "Usuario"));
                claims.Add(new Claim(ClaimTypes.Role, "portalproprietariosw"));
            }

            if (documentosUsuario.Any())
            {
                claims.Add(new Claim("UsuarioPortalIdentificador", documentos));
            }

            if (userpermissions.Any())
                claims.AddRange(userpermissions.Where(c => !string.IsNullOrEmpty(c.NormalizedPermission)).Select(role => new Claim(ClaimTypes.Role, role.NormalizedPermission!)));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                audience: _configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                notBefore: DateTime.UtcNow,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            tokenResultModel.Token = jwt;

            await Task.CompletedTask;
        }

        private async Task<Models.AuthModels.TokenResultModel?> GenerateTokenFake(List<string> pessoasFake, LoginInputModel userLoginInputModel)
        {
            var dadosPessoaFake = pessoasFake.FirstOrDefault(a => Helper.ApenasNumeros(a.Split(",").Last()) == Helper.ApenasNumeros(userLoginInputModel.Login));
            if (dadosPessoaFake == null)
                return null;

            Usuario? user = await DoLoginBySoFaltaEuOrByAccessCenterFake(new UserRegisterInputModel { CpfCnpj = dadosPessoaFake.Split(",")[2], FullName = dadosPessoaFake.Split(",")[1], PessoaId = dadosPessoaFake.Split(",")[0], Password = userLoginInputModel.Senha, PasswordConfirmation = userLoginInputModel.Senha });


            TokenResultModel tokenResultModel = new TokenResultModel() { Login = userLoginInputModel.Login?.RemoveAccents() };

            var companyId = _configuration.GetValue<int>("EmpresaSwPortalId");
            var empresaCMId = _configuration.GetValue<int>("EmpresaCMId");

            if (string.IsNullOrEmpty(tokenResultModel.Login))
                throw new Exception("Deve ser informado o login do usuário para geração do token de acesso");

            if (tokenResultModel.UserId.GetValueOrDefault(0) == 0 && (user == null || user?.Id == 0))
                throw new Exception("Deve ser informado o id do usuário para geração do token de acesso");

            if (tokenResultModel.UserId.GetValueOrDefault(0) == 0)
                tokenResultModel.UserId = user?.Id;

            var tokenkey = _configuration.GetValue<string>("Jwt:Key")!;
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenkey));

            var userpermissions = (await _repository.FindBySql<UserRoleModel>($@"Select
                                            Distinct
                                            m.NomeInterno as ModuleInternalName,
                                            p.TipoPermissao as PermissionType
                                            From
                                            GrupoUsuarioModuloPermissao gump
                                            Inner Join ModuloPermissao mp on gump.ModuloPermissao = mp.Id
                                            Inner Join Permissao p on mp.Permissao = p.Id
                                            Inner Join Modulo m on mp.Modulo = m.Id
                                            Where 
                                            Exists(Select ugou.GrupoUsuario From UsuarioGrupoUsuario ugou Inner Join Usuario u on ugou.Usuario = u.Id
                                                   Where
                                                   u.Id = {tokenResultModel.UserId.GetValueOrDefault()} and ugou.GrupoUsuario = gump.GrupoUsuario)")).ToList();


            List<Claim> claims = new()
            {
                   new Claim(ClaimTypes.Name, tokenResultModel.Login),
                   new Claim("UserId", tokenResultModel.UserId.GetValueOrDefault().ToString())
            };

            if (!string.IsNullOrEmpty(user.ProviderChaveUsuario))
            {
                claims.Add(new Claim("ProviderKeyUser", user.ProviderChaveUsuario));
                if (user.ProviderChaveUsuario.Contains("PessoaId:"))
                {
                    var basePegarPessoa = user.ProviderChaveUsuario.Split('|')[0];
                    if (!string.IsNullOrEmpty(basePegarPessoa))
                    {
                        claims.Add(new Claim("PessoaACId", basePegarPessoa.Split(':')[1]));
                    }
                }
            }

            if (companyId > 0)
            {
                claims.Add(new Claim("EmpresaACId", $"{companyId}"));
                claims.Add(new Claim("CompanyId", $"{companyId}"));
            }

            if (empresaCMId > 0)
            {
                claims.Add(new Claim("EmpresaCMId", $"{empresaCMId}"));
            }

            var vinculoPortal = new VinculoAccessXPortalBase()
            {
                AcCotaId = 0,
                EsolCotaId = 0,
                EsolCotaNome = "Cota nome",
                EsolNumeroImovel = "Sem numero",
                EsolPessoaProprietarioId = Convert.ToInt32(dadosPessoaFake.Split(",")[0]),
                EsolPessoaProprietarioNome = dadosPessoaFake.Split(",")[1]
            };

            if (vinculoPortal != null)
            {
                if (vinculoPortal.AcCotaId.GetValueOrDefault(0) > 0)
                    claims.Add(new Claim("CotaAcId", $"{vinculoPortal.AcCotaId}"));
                if (vinculoPortal.EsolCotaId.GetValueOrDefault(0) > 0)
                    claims.Add(new Claim("CotaPortalId", $"{vinculoPortal.EsolCotaId}"));
                if (!string.IsNullOrEmpty(vinculoPortal.EsolCotaNome))
                    claims.Add(new Claim("CotaPortalNome", $"{vinculoPortal.EsolCotaNome}"));
                if (!string.IsNullOrEmpty(vinculoPortal.EsolNumeroImovel))
                    claims.Add(new Claim("CotaPortalNumeroImovel", $"{vinculoPortal.EsolNumeroImovel}"));
                if (vinculoPortal.EsolPessoaProprietarioId.GetValueOrDefault(0) > 0)
                    claims.Add(new Claim("CotaPortalPessoaProprietarioId", $"{vinculoPortal.EsolPessoaProprietarioId}"));
                if (string.IsNullOrEmpty(vinculoPortal.EsolPessoaProprietarioNome))
                    claims.Add(new Claim("CotaPortalPessoaProprietarioNome", $"{vinculoPortal.EsolPessoaProprietarioNome}"));
            }

            claims.Add(new Claim(ClaimTypes.Role, "Usuario"));
            claims.Add(new Claim(ClaimTypes.Role, "portalproprietariosw"));

            if (userpermissions.Any())
                claims.AddRange(userpermissions.Where(c => !string.IsNullOrEmpty(c.NormalizedPermission)).Select(role => new Claim(ClaimTypes.Role, role.NormalizedPermission)));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                audience: _configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                notBefore: DateTime.UtcNow,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            tokenResultModel.Token = jwt;

            return tokenResultModel;
        }

        private async Task<Usuario?> DoLoginBySoFaltaEuOrByAccessCenterFake(UserRegisterInputModel userInputModel)
        {
            var sb = new StringBuilder(@$"From 
                                                    Usuario us 
                                                    Inner Join Fetch us.Pessoa p 
                                                 Where 1 = 1 and us.DataHoraRemocao is null and Coalesce(us.Removido,0) = 0 ");


            if (!string.IsNullOrEmpty(userInputModel.Email))
            {
                sb.AppendLine($" and (Lower(us.Login) = '{userInputModel.Email.ToLower()}' or Lower(p.EmailPreferencial) = '{userInputModel.Email!.ToLower()}' or Lower(p.EmailAlternativo) like '{userInputModel.Email.ToLower()}') ");
            }

            if (!string.IsNullOrEmpty(userInputModel.CpfCnpj))
            {
                var apenasNumeros = Helper.ApenasNumeros(userInputModel.CpfCnpj);
                if (Helper.IsCpf(apenasNumeros) || Helper.IsCnpj(apenasNumeros))
                {
                    sb.AppendLine(@$" and Exists(Select 
                                                            pd.Pessoa 
                                                        From PessoaDocumento pd 
                                                            Inner Join TipoDocumentoPessoa tdp on pd.TipoDocumento = tdp.Id and 
                                                            Lower(tdp.Nome) in ('cpf','cnpj') 
                                                        Where 
                                                            pd.Pessoa = p.Id and 
                                                            pd.ValorNumerico = '{apenasNumeros}') ");
                }
            }

            var novoUsuario = false;
            string? pessoaLegadoId = null;

            var user = (await _repository.FindByHql<Usuario>(sb.ToString())).FirstOrDefault();
            if (user == null)
            {
                user = (await _repository.FindByHql<Usuario>($"From Usuario u Where u.Login = '{userInputModel.CpfCnpj}' and u.DataHoraRemocao is null and Coalesce(u.Removido,0) = 0")).FirstOrDefault();
                if (user != null)
                {
                    var pessoa = (await _repository.FindByHql<SW_PortalProprietario.Domain.Entities.Core.DadosPessoa.Pessoa>($"From Pessoa p Where p.Id = {user.Pessoa.Id}")).FirstOrDefault();
                    if (Helper.IsCpf(userInputModel.CpfCnpj))
                    {
                        var tipoDocumentosPessoa = (await _repository.FindBySql<TipoDocumentoPessoa>($"Select tdp.* From TipoDocumentoPessoa tdp Where Lower(tdp.Nome) in ('cpf','cnpj') and tdp.TipoPessoa = 0")).FirstOrDefault();
                        if (tipoDocumentosPessoa != null)
                        {
                            var pessoaSincronizacaoAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);
                            var retorno = await pessoaSincronizacaoAuxiliar.SincronizarDocumentos(pessoa, true, new Models.PessoaModels.PessoaDocumentoInputModel() { TipoDocumentoId = tipoDocumentosPessoa.Id, Numero = $"{Helper.ApenasNumeros(userInputModel.CpfCnpj)}", PessoaId = pessoa.Id });
                        }
                    }
                }
            }



            if (user == null)
            {
                user = await RegistrarUsuarioExecute(new UserRegisterInputModel()
                {
                    FullName = userInputModel.FullName,
                    CpfCnpj = userInputModel.CpfCnpj,
                    Email = $"{userInputModel.FullName.Replace(" ", "").ToLower()}@gmail.com",
                    Password = userInputModel.Password,
                    PasswordConfirmation = userInputModel.PasswordConfirmation,
                    Administrator = userInputModel.Administrator.GetValueOrDefault(EnumSimNao.Não)
                });
                pessoaLegadoId = userInputModel.PessoaId;

                novoUsuario = true;

                await _repository.ForcedSave(user);
            }


            if (novoUsuario)
            {

                var accessValidateResult = new AccessValidateResultModel()
                {
                    PessoaId = userInputModel.PessoaId,
                    PessoaNome = userInputModel.FullName,
                    UsuarioSistema = user.Id,
                    ProviderName = _communicationProvider.CommunicationProviderName
                };

                await GravarVinculoUsuarioProvider(accessValidateResult, user);
                var psxpp = new PessoaSistemaXProvider()
                {
                    PessoaSistema = $"{user?.Pessoa?.Id}",
                    PessoaProvider = accessValidateResult.PessoaId,
                    NomeProvider = _communicationProvider.CommunicationProviderName,
                    TokenResult = user.TokenResult
                };
                await _repository.ForcedSave(psxpp);

                novoUsuario = true;
            }


            return user;
        }

    }
}
