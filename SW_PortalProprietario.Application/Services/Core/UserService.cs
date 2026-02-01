using CMDomain.Models.Pessoa;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate.Linq.Functions;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Auxiliar;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Domain.Functions;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System.Diagnostics;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class UserService : IUserService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<UserService> _logger;
        private readonly IServiceBase _serviceBase;
        private readonly IProjectObjectMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ICommunicationProvider _communicationProvider;

        public UserService(IRepositoryNH repository,
            ILogger<UserService> logger,
            IServiceBase serviceBase,
            IProjectObjectMapper mapper,
            IEmailService emailService,
            IConfiguration configuration,
            ICommunicationProvider communicationProvider)
        {
            _repository = repository;
            _logger = logger;
            _serviceBase = serviceBase;
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _communicationProvider = communicationProvider;
        }

        public async Task<ChangePasswordResultModel> ChangePassword(ChangePasswordInputModel changePasswordInputModel)
        {
            try
            {
                _repository.BeginTransaction();
                ArgumentNullException.ThrowIfNull(changePasswordInputModel, nameof(changePasswordInputModel));
                var loggedUser = await _repository.GetLoggedUser() ?? throw new ArgumentException("Usuário não encontrado");

                if (string.IsNullOrEmpty(changePasswordInputModel.ActualPassword))
                    throw new ArgumentException("Deve ser informada a senha atual'");

                if (string.IsNullOrEmpty(changePasswordInputModel.NewPassword))
                    throw new ArgumentException("Deve ser informada a nova senha");

                if (string.IsNullOrEmpty(changePasswordInputModel.NewPasswordConfirmation))
                    throw new ArgumentException("Deve ser informada a senha de confirmação");

                if (changePasswordInputModel.NewPassword != changePasswordInputModel.NewPasswordConfirmation)
                    throw new ArgumentException("Senha de confirmação de senha está diferente da senha");

                var user = (await _repository.FindByHql<Usuario>(@"From Usuario us 
                                                        Where 
                                                            us.Id = :usuarioLogado and us.DataHoraRemocao is null and coalesce(us.Removido,0) = 0 ",
                                                            new Parameter("usuarioLogado", loggedUser.userId))).FirstOrDefault() ?? throw new Exception($"Falha na alteração da senha");

                if (!BCrypt.Net.BCrypt.Verify(changePasswordInputModel.ActualPassword, user.PasswordHash))
                    throw new ArgumentException($"A senha atual informada não está correta para o usuário: {user.Login}");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordInputModel.NewPassword);
                await _repository.Save(user);

                if (!(await _communicationProvider.IsDefault()) && !loggedUser.isAdm)
                {

                    var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(user.Id, _communicationProvider.CommunicationProviderName);
                    if (pessoaProvider == null || string.IsNullOrEmpty(pessoaProvider.PessoaProvider))
                        throw new Exception($"Não foi possível alterar a senha do usuário no sistema legado: {_communicationProvider.CommunicationProviderName}");

                    if (_communicationProvider.CommunicationProviderName.Contains("ESOLUTION", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await _communicationProvider.AlterarSenhaNoLegado(pessoaProvider.PessoaProvider, user.Login!, SW_Utils.Functions.Helper.CriptografarPadraoEsol("", changePasswordInputModel.NewPassword));
                    }
                }

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    return new ChangePasswordResultModel()
                    {
                        PasswordChanged = true,
                    };
                }
                else throw exception ?? new Exception("Erro na operação");

            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, err.Message);
                throw;
            }
        }


        public async Task<(int pageNumber, int lastPageNumber, List<UsuarioModel> usuarios)?> Search(UsuarioSearchPaginatedModel searchModel)
        {
            bool carregarFull = true;
            var parametroSistema = await _repository.GetParametroSistemaViewModel();

            var loggedUser = await _repository.GetLoggedUser();

            List<Parameter> parameters = new();
            StringBuilder sb = new(@"Select 
                u.Id, 
                u.Login,
                p.Nome as NomePessoa,
                u.Status,
                Coalesce(u.Administrador,0) as Administrador,
                Coalesce(u.GestorFinanceiro,0) as GestorFinanceiro,
                Coalesce(u.GestorReservasAgendamentos,0) as GestorReservasAgendamentos,
                p.Id as PessoaId,
                pprov.pessoaprovider as PessoaProviderId,
                pprov.nomeprovider,
                u.LoginPms,
                u.LoginSistemaVenda
                From 
                Usuario u
                Inner Join Pessoa p on u.Pessoa = p.Id
                left join pessoasistemaxprovider pprov on p.id = cast(pprov.PessoaSistema as int)
                Where 1 = 1 and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ");

            if (!string.IsNullOrEmpty(searchModel.NomePessoa))
                sb.AppendLine($@" and (
                                        (translate(Lower(p.Nome),'áàãâäéèêëíìîïóòõôöúùûüç','aaaaaeeeeiiiiooooouuuuc') like '%{searchModel.NomePessoa.ToLower().RemoveAccents()}%') or 
                                        (translate(Lower(u.Login),'áàãâäéèêëíìîïóòõôöúùûüç','aaaaaeeeeiiiiooooouuuuc') like '%{searchModel.NomePessoa.ToLower().RemoveAccents()}%') 
                                      )");


            if (!string.IsNullOrEmpty(searchModel.Email))
            {
                sb.AppendLine(@$" and (Lower(p.EmailPreferencial) like '{searchModel.Email.TrimEnd().ToLower()}%' or  
                    Lower(p.EmailAlternativo) like '{searchModel.Email.TrimEnd().ToLower()}%')");
            }

            if (!string.IsNullOrEmpty(searchModel.CpfCnpj))
            {
                var apenasNumeros = searchModel.CpfCnpj;
                if (!string.IsNullOrEmpty(apenasNumeros) && (SW_Utils.Functions.Helper.IsCnpj(apenasNumeros) || SW_Utils.Functions.Helper.IsCpf(apenasNumeros)))
                {
                    sb.AppendLine(@$" and Exists(Select pd.Pessoa From PessoaDocumento pd Where pd.Numero like '%{apenasNumeros}%' and pd.Pessoa = p.Id)");
                }
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and u.Id = :id ");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (searchModel.Admininistrador.HasValue)
            {
                sb.AppendLine($" and u.Administrador = {(int)searchModel.Admininistrador} ");
            }

            if (searchModel.GestorFinanceiro.HasValue)
            {
                sb.AppendLine($" and u.GestorFinanceiro = {(int)searchModel.GestorFinanceiro} ");
            }

            if (searchModel.GestorReservasAgendamentos.HasValue)
            {
                sb.AppendLine($" and u.GestorReservasAgendamentos = {(int)searchModel.GestorReservasAgendamentos} ");
            }

            if (!loggedUser.Value.isAdm)
            {
                sb.AppendLine($" and u.Id = {loggedUser.Value.userId} ");
            }

            var sql = sb.ToString();

            var totalRegistros = await _repository.CountTotalEntry(sql, parameters.ToArray());
            if (totalRegistros == 0)
                return (1, 1, new List<UsuarioModel>());

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 10;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
            if (totalPage < searchModel.NumeroDaPagina)
                searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);

            sb.AppendLine("ORDER BY u.Id");


            var users = await _repository.FindBySql<UsuarioModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(1), parameters.ToArray());
            await PopulateCompaniesOfUser(users);

            var integradoWith = _configuration.GetValue<string>("IntegradoCom", "eSolution");
            

            if (!string.IsNullOrEmpty(integradoWith))
            {
                var integradoComTimeSharing = !integradoWith.Contains("eSolution", StringComparison.InvariantCultureIgnoreCase) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Não;
                var integradoComMultipropriedade = integradoWith.Contains("eSolution", StringComparison.InvariantCultureIgnoreCase) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Não;
                foreach (var item in users)
                {
                    item.IntegradoComMultiPropriedade = integradoComMultipropriedade;
                    item.IntegradoComTimeSharing = integradoComTimeSharing;
                }
            }

            if (carregarFull)
            {
                if (searchModel.CarregarPermissoes.GetValueOrDefault(false))
                {
                    await PopulatePemissionsOfUsers(users);
                }

                if (searchModel.CarregarGruposDeUsuarios.GetValueOrDefault(false))
                {
                    await PopulateUserGroups(users);
                }

                if (!searchModel.CarregarDadosPessoa.HasValue || searchModel.CarregarDadosPessoa.GetValueOrDefault(false))
                {
                    await PopularPessoaUsuario(users);
                }

                if (users.Any())
                {
                    var tagsDosUsuarios = (await _repository.FindByHql<UsuarioTags>($"From UsuarioTags dt Inner Join Fetch dt.Usuario d Inner Join Fetch dt.Tags t Where d.Id in ({string.Join(",", users.Select(a => a.Id).AsList())})")).AsList();
                    foreach (var item in users)
                    {
                        var tagsRelacionadas = tagsDosUsuarios.Where(b => b.Usuario.Id == item.Id).AsList();
                        if (tagsRelacionadas.Any())
                        {
                            item.TagsRequeridas = tagsRelacionadas.Select(b => new UsuarioTagsModel()
                            {
                                UsuarioId = b.Usuario.Id,
                                Id = b.Id,
                                Tags = _mapper.Map(b.Tags, new TagsModel())
                            }).AsList();
                        }
                    }
                }
            }

            if (users.Any())
            {
                var preparedUsers = await _serviceBase.SetUserName(users.AsList());

                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), preparedUsers);
                }
            }

            return default;
        }

        public async Task<List<UsuarioModel>?> SearchNotPaginated(UsuarioSearchModel searchModel)
        {

            var loggedUser = await _repository.GetLoggedUser();

            List<Parameter> parameters = new();
            StringBuilder sb = new(@"Select 
                                    u.Id, 
                                    u.Login,
                                    p.Nome as NomePessoa,
                                    u.Status,
                                    Coalesce(u.Administrador,0) as Administrador,
                                    Coalesce(u.GestorFinanceiro,0) as GestorFinanceiro,
                                    Coalesce(u.GestorReservasAgendamentos,0) as GestorReservasAgendamentos,                                    
                                    p.Id as PessoaId,
                                    u.LoginPms,
                                    u.LoginSistemaVenda
                                    From 
                                    Usuario u
                                    Inner Join Pessoa p on u.Pessoa = p.Id
                                    Where 1 = 1 and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ");

            
            if (!string.IsNullOrEmpty(searchModel.NomePessoa))
                sb.AppendLine($" and Translate(Lower(p.Nome),'áàãâäéèêëíìîïóòõôöúùûüç', 'aaaaaeeeeiiiiooooouuuuc') like '{searchModel.NomePessoa.ToLower().RemoveAccents()}%' ");

            if (!string.IsNullOrEmpty(searchModel.Login))
            {
                sb.AppendLine($" and Translate(Lower(u.Login),'áàãâäéèêëíìîïóòõôöúùûüç', 'aaaaaeeeeiiiiooooouuuuc') like '{searchModel.Login.TrimEnd().ToLower().RemoveAccents()}%' ");
            }


            if (!string.IsNullOrEmpty(searchModel.LoginNomeEmail))
            {

                sb.AppendLine(@$" and (Translate(Lower(u.Login),'áàãâäéèêëíìîïóòõôöúùûüç', 'aaaaaeeeeiiiiooooouuuuc') like '{searchModel.LoginNomeEmail.TrimEnd().ToLower().RemoveAccents()}%') or 
                    Lower(p.EmailPreferencial) like '{searchModel.LoginNomeEmail.TrimEnd().ToLower()}%' or  
                    Lower(p.EmailAlternativo) like '{searchModel.LoginNomeEmail.TrimEnd().ToLower()}%' or  
                    Lower(p.Nome) like '{searchModel.LoginNomeEmail.TrimEnd().ToLower()}%')");
            }

            if (!string.IsNullOrEmpty(searchModel.Email))
            {
                sb.AppendLine(@$" and ((Lower(p.EmailPreferencial) like '{searchModel.Email.TrimEnd().ToLower()}%' or  
                    Lower(p.EmailAlternativo) like '{searchModel.Email.TrimEnd().ToLower()}%') or (
                    Upper(p.EmailPreferencial) like '{searchModel.Email.TrimEnd().ToUpper()}%' or  
                    Upper(p.EmailAlternativo) like '{searchModel.Email.TrimEnd().ToUpper()}%'))");
            }

            if (!string.IsNullOrEmpty(searchModel.CpfCnpj))
            {
                var apenasNumeros = searchModel.CpfCnpj;
                if (!string.IsNullOrEmpty(apenasNumeros) && (SW_Utils.Functions.Helper.IsCnpj(apenasNumeros) || SW_Utils.Functions.Helper.IsCpf(apenasNumeros)))
                {

                    sb.AppendLine(@$" and Exists(Select pd.Pessoa From PessoaDocumento pd Where pd.Numero = '{apenasNumeros}' and pd.Pessoa = p.Id)");
                }
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and u.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            if (searchModel.Status.HasValue)
            {
                sb.AppendLine($" and u.Status = :status");
                parameters.Add(new Parameter("status", (int)searchModel.Status));
            }

            if (!loggedUser.Value.isAdm)
            {
                sb.AppendLine($" and u.Id = {loggedUser.Value.userId} ");
            }

            var sql = sb.ToString();

            sb.AppendLine("ORDER BY u.Id");


            var users = await _repository.FindBySql<UsuarioModel>(sb.ToString(), parameters.ToArray());
            if (searchModel.CarregarPermissoes.GetValueOrDefault(false))
            {
                await PopulatePemissionsOfUsers(users);
            }

            if (searchModel.CarregarEmpresas.GetValueOrDefault(false))
            {
                await PopulateCompaniesOfUser(users);

            }

            if (searchModel.CarregarGruposDeUsuarios.GetValueOrDefault(false))
            {
                await PopulateUserGroups(users);
            }

            if (!searchModel.CarregarDadosPessoa.HasValue || searchModel.CarregarDadosPessoa.GetValueOrDefault(false))
            {
                await PopularPessoaUsuario(users);
            }

            if (users.Any())
            {
                var tagsDosUsuarios = (await _repository.FindByHql<UsuarioTags>($"From UsuarioTags dt Inner Join Fetch dt.Usuario d Inner Join Fetch dt.Tags t Where d.Id in ({string.Join(",", users.Select(a => a.Id).AsList())})")).AsList();
                foreach (var item in users)
                {
                    var tagsRelacionadas = tagsDosUsuarios.Where(b => b.Usuario.Id == item.Id).AsList();
                    if (tagsRelacionadas.Any())
                    {
                        item.TagsRequeridas = tagsRelacionadas.Select(b => new UsuarioTagsModel()
                        {
                            UsuarioId = b.Usuario.Id,
                            Id = b.Id,
                            Tags = _mapper.Map(b.Tags, new TagsModel())
                        }).AsList();
                    }
                }
            }

            if (users.Any())
            {
                var preparedUsers = await _serviceBase.SetUserName(users.AsList());
                return preparedUsers;
            }

            return default;
        }

        private async Task PopulateUserGroups(IList<UsuarioModel> users)
        {
            foreach (var user in users)
            {
                var groupsOfUser = (await _repository.FindBySql<UsuarioGrupoUsuarioModel>(@$"Select 
                                    ugu.Usuario as UsuarioId,
                                    gu.Id as GrupoUsuarioId, 
                                    ugu.Id as Id,
                                    ugu.DataHoraCriacao,
                                    ugu.UsuarioCriacao,
                                    ugu.DataHoraAlteracao,
                                    ugu.UsuarioAlteracao,
                                    gu.Nome as GrupoUsuarioNome
                                    From 
                                    UsuarioGrupoUsuario ugu
                                    Inner Join GrupoUsuario gu on ugu.GrupoUsuario = gu.Id
                                    Where
                                    ugu.Usuario = {user.Id}"
                                )).ToList();

                user.UsuarioGruposUsuarios = groupsOfUser.Any() ? groupsOfUser : null;

            }
        }

        private async Task PopulatePemissionsOfUsers(IList<UsuarioModel> users)
        {
            foreach (var user in users)
            {
                var itensPermissions = (await _repository.FindBySql<UsuarioModuloPermissaoModel>(@$"Select 
                                    p.Id as PermissaoId, 
                                    goump.DataHoraCriacao,
                                    goump.UsuarioCriacao,
                                    goump.DataHoraAlteracao,
                                    goump.UsuarioAlteracao,
                                    p.Nome as PermissaoNome,
                                    p.NomeInterno as PermissaoNomeInterno,
                                    p.TipoPermissao,
                                    m.Id as ModuloId,
                                    m.Codigo as ModuloCodigo,
                                    m.Nome as ModuloNome
                                    From 
                                    GrupoUsuarioModuloPermissao goump
                                    Inner Join ModuloPermissao mp on goump.ModuloPermissao = mp.Id
                                    Inner Join Modulo m on mp.Modulo = m.Id 
                                    Inner Join Permissao p on mp.Permissao = p.Id
                                    Where
                                    Exists(Select gou.GrupoUsuario From UsuarioGrupoUsuario gou Where gou.GrupoUsuario = goump.GrupoUsuario and gou.Usuario = {user.Id})"
                                )).ToList();


                if (itensPermissions.Any())
                    user.UsuarioModuloPermissoes = new List<UsuarioModuloPermissaoModel>();

                foreach (var permissionGroupped in itensPermissions.GroupBy(a => new
                {
                    a.ModuloId,
                    a.PermissaoId,
                }
                ))
                {
                    if (user.UsuarioModuloPermissoes == null)
                        user.UsuarioModuloPermissoes = new List<UsuarioModuloPermissaoModel>();

                    var permission = permissionGroupped.First();
                    user.UsuarioModuloPermissoes.Add(new UsuarioModuloPermissaoModel()
                    {
                        PermissaoId = permission.PermissaoId,
                        PermissaoNomeInterno = permission.PermissaoNomeInterno,
                        PermissaoNome = permission.PermissaoNome,
                        TipoPermissao = permission.TipoPermissao,
                        ModuloId = permission.ModuloId,
                        ModuloCodigo = permission.ModuloCodigo,
                        ModuloNome = permission.ModuloNome,
                        DataHoraCriacao = permission.DataHoraCriacao,
                        DataHoraAlteracao = permission.DataHoraAlteracao,
                        UsuarioAlteracao = permission.UsuarioAlteracao,
                        UsuarioCriacao = permission.UsuarioCriacao
                    });
                };


            }
        }

        private async Task PopulateCompaniesOfUser(IList<UsuarioModel> users)
        {
            if (users == null || !users.Any(c => c.PessoaId.GetValueOrDefault() > 0))
            {
                await Task.CompletedTask;
                return;
            }

            var companies = (await _repository.FindBySql<EmpresaUsuarioModel>(@$"Select 
                                    uc.Id,
                                    uc.UsuarioCriacao,
                                    uc.DataHoraCriacao,
                                    uc.UsuarioAlteracao,
                                    uc.DataHoraAlteracao,
                                    c.Id as EmpresaId, 
                                    uc.Usuario as UsuarioId,
                                    c.Codigo as EmpresaCodigo,
                                    per.NomeFantasia,
                                    per.Nome as PessoaJuridicaNome,
                                    cg.Id as GrupoEmpresaId,
                                    cg.Codigo as GrupoEmpresaCodigo,                                    
                                    cgp.Nome as GrupoEmpresaNome
                                    From 
                                    EmpresaUsuario uc
                                    Inner Join Empresa c on uc.Empresa = c.Id
                                    Inner Join Pessoa per on c.Pessoa = per.Id
                                    Left Join GrupoEmpresa cg on c.GrupoEmpresa = cg.Id
                                    Left Join Pessoa cgp on cg.Pessoa = cgp.Id
                                    Where
                                    uc.Usuario in ({string.Join(",", users.Select(b => b.Id))})"
                                )).ToList();

            foreach (var user in users)
            {
                var companiesOfUser = companies.Where(b => b.UsuarioId == user.Id).AsList();
                if (companiesOfUser.Any())
                    user.UsuarioEmpresas = companiesOfUser;

            }
        }

        private async Task PopularPessoaUsuario(IList<UsuarioModel> users)
        {
            if (users == null || !users.Any(c => c.PessoaId.GetValueOrDefault() > 0))
            {
                await Task.CompletedTask;
                return;
            }

            var pessoas = await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>($"From Pessoa p Where p.Id in ({string.Join(",", users.Select(c => c.PessoaId.GetValueOrDefault()).AsList())})");

            if (pessoas.Any())
            {

                List<PessoaCompletaModel> listRetorno = pessoas.Select(a => _mapper.Map(a, new PessoaCompletaModel())).AsList();

                var telefones = listRetorno.Any() ?
                    (await _repository.FindBySql<PessoaTelefoneModel>(@$"Select 
												pt.Id,
												pt.DataHoraCriacao,
												pt.UsuarioCriacao,
												pt.DataHoraAlteracao,
												pt.UsuarioAlteracao,
												pt.Pessoa as PessoaId, 
												pt.TipoTelefone as TipoTelefoneId, 
												tt.Nome as TipoTelefoneNome,
												pt.Numero,
												tt.Mascara as TipoTelefoneMascara,
												Coalesce(pt.Preferencial,0) AS Preferencial,
                                                pt.NumeroFormatado
												From 
												PessoaTelefone pt 
												Inner Join TipoTelefone tt on pt.TipoTelefone = tt.Id
												Where 
												pt.Pessoa in ({string.Join(",", listRetorno.Select(b => b.Id.GetValueOrDefault()))})")).AsList() : new List<PessoaTelefoneModel>();

                var enderecos = listRetorno.Any() ?
                    (await _repository.FindBySql<PessoaEnderecoModel>(@$"Select
												pt.Id,
												pt.UsuarioCriacao,
												pt.DataHoraCriacao,
												pt.UsuarioAlteracao,
												pt.DataHoraAlteracao,
												pt.Pessoa as PessoaId,
												c.Id as CidadeId,
												c.Nome as CidadeNome,
												e.Id as EstadoId,
												e.Sigla as EstadoSigla,
												e.Nome as EstadoNome,
												pt.TipoEndereco as TipoEnderecoId, 
												tt.Nome as TipoEnderecoNome,
												pt.Numero,
												pt.Logradouro,
												pt.Bairro,
												pt.Complemento,
												pt.Cep,
												Coalesce(pt.Preferencial,0) AS Preferencial
												From 
												PessoaEndereco pt 
												Inner Join Cidade c ON pt.Cidade = c.Id
												Inner Join Estado e on c.Estado = e.Id
												Inner Join TipoEndereco tt on pt.TipoEndereco = tt.Id
												Where 
												pt.Pessoa in ({string.Join(",", listRetorno.Select(b => b.Id.GetValueOrDefault()))})")).AsList() : new List<PessoaEnderecoModel>();

                var documentos = listRetorno.Any() ?
                    (await _repository.FindBySql<PessoaDocumentoModel>(@$"Select
																		pd.Id,
																		pd.UsuarioCriacao,
																		pd.DataHoraCriacao,
																		pd.UsuarioAlteracao,
																		pd.DataHoraAlteracao,
																		pd.Pessoa as PessoaId,
																		td.Id as TipoDocumentoId, 
																		td.Nome as TipoDocumentoNome,
																		pd.Numero,
																		pd.OrgaoEmissor,
																		pd.DataEmissao,
																		pd.DataValidade,
																		td.Mascara as TipoDocumentoMascara,
                                                                        pd.NumeroFormatado
																		From 
																		PessoaDocumento pd
																		Inner Join TipoDocumentoPessoa td on pd.TipoDocumento = td.Id
																		Where 
																		pd.Pessoa in ({string.Join(",", listRetorno.Select(b => b.Id.GetValueOrDefault()))})")).AsList() : new List<PessoaDocumentoModel>();

                foreach (var item in listRetorno)
                {
                    var telefonesPessoa = telefones.Where(b => b.PessoaId == item.Id).AsList();
                    item.Telefones = telefonesPessoa;

                    var enderecosPessoa = enderecos.Where(b => b.PessoaId == item.Id).AsList();
                    item.Enderecos = enderecosPessoa;

                    var documentosPessoa = documentos.Where(b => b.PessoaId == item.Id).AsList();
                    item.Documentos = documentosPessoa;

                }

                foreach (var usu in users)
                {
                    var pessoa = listRetorno.FirstOrDefault(a => a.Id == usu.PessoaId.GetValueOrDefault());
                    if (pessoa != null)
                        usu.Pessoa = pessoa;
                }

            }

            await Task.CompletedTask;
        }

        public async Task<UserRegisterResultModel> SaveUser(RegistroUsuarioFullInputModel userInputModel)
        {
            try
            {
                _repository.BeginTransaction();

                ArgumentNullException.ThrowIfNull(userInputModel, nameof(userInputModel));
                if (string.IsNullOrEmpty(userInputModel.Pessoa?.Nome) && userInputModel.Id.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o nome da pessoa física do usuário");


                if ((string.IsNullOrEmpty(userInputModel?.Pessoa?.EmailPreferencial) || !userInputModel.Pessoa.EmailPreferencial.Contains("@")) && userInputModel.Id.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o eMail do usuário");

                var tipoDocumento = (await _repository.FindBySql<TipoDocumentoPessoaModel>("Select td.* From TipoDocumentoPessoa td Where Lower(td.Nome) in ('cpf','cnpj')")).AsList();
                if (tipoDocumento == null || !tipoDocumento.Any())
                    throw new ArgumentException("Deve ser cadastrados os Tipos de Documentos CPF e CNPJ antes de iniciar a criação de usuários no sistema.");

                if (userInputModel.Id == 0 && (userInputModel.Pessoa?.Documentos == null || !userInputModel.Pessoa.Documentos.Any(a => tipoDocumento.Any(b => b.Id.GetValueOrDefault() == a.TipoDocumentoId.GetValueOrDefault()))))
                    throw new ArgumentException("Deve ser informado o CPF e ou CNPJ do usuário");


                if (string.IsNullOrEmpty(userInputModel.Login) && userInputModel.Pessoa != null && !string.IsNullOrEmpty(userInputModel.Pessoa.Nome))
                {
                    var arr = userInputModel.Pessoa.Nome.Split(' ');
                    if (arr.Length > 1)
                    {
                        userInputModel.Login = (arr.First().Substring(0,1) + "." + arr.Last()).ToLower();
                    }
                    else if (userInputModel.Pessoa.Documentos != null && userInputModel.Pessoa.Documentos.Any(c => c.TipoDocumentoId == 1 || c.TipoDocumentoId == 2))
                    {
                        var documento = userInputModel.Pessoa.Documentos.First(a => a.TipoDocumentoId == 1 || a.TipoDocumentoId == 2);
                        userInputModel.Login = SW_Utils.Functions.Helper.ApenasNumeros(documento.Numero);
                    }

                    if (userInputModel.Administrador.GetValueOrDefault(EnumSimNao.Não) != EnumSimNao.Sim)
                    {

                        if (userInputModel.Pessoa.Documentos != null && userInputModel.Pessoa.Documentos.Any(c => c.TipoDocumentoId == 1 || c.TipoDocumentoId == 2))
                        {
                            var documento = userInputModel.Pessoa.Documentos.First(a => a.TipoDocumentoId == 1 || a.TipoDocumentoId == 2);
                            userInputModel.Login = SW_Utils.Functions.Helper.ApenasNumeros(documento.Numero);
                        }
                    }
                }

                Usuario? user = await RegistrarAlterarUsuarioExecute(userInputModel);

                if (user != null)
                {
                    if (userInputModel.Administrador.HasValue)
                    {
                        if (userInputModel.Administrador.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                            user.Administrador = EnumSimNao.Sim;
                        else user.Administrador = EnumSimNao.Não;
                    }

                    if (userInputModel.GestorFinanceiro.HasValue)
                    {
                        if (userInputModel.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                            user.GestorFinanceiro = EnumSimNao.Sim;
                        else user.GestorFinanceiro = EnumSimNao.Não;
                    }

                    if (userInputModel.GestorReservasAgendamentos.HasValue)
                    {
                        if (userInputModel.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Sim)
                            user.GestorReservasAgendamentos = EnumSimNao.Sim;
                        else user.GestorReservasAgendamentos = EnumSimNao.Não;
                    }

                    var (executed, exception) = await _repository.CommitAsync();
                    if (executed)
                    {
                        return new UserRegisterResultModel()
                        {
                            UserId = user.Id,
                            Login = user.Login?.RemoveAccents(),
                        };
                    }
                    else throw exception ?? new Exception("Erro na operação");
                }

                throw new Exception("Não foi possível cadastrar o usuário");

            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        private async Task<Usuario?> RegistrarAlterarUsuarioExecute(RegistroUsuarioFullInputModel userInputModel)
        {
            if (userInputModel == null)
                throw new Exception("Deve ser enviado os dados do usuário para inclusão");

            PessoaCompletaModel? pessoaEmailPreferencial = null;
            PessoaCompletaModel? pessoaEmailAlternativo = null;
            PessoaCompletaModel? pessoaDocumentoInformado = null;

            var isAdm = _repository.IsAdm;

            var companyConfiguration = await _serviceBase.GetParametroSistema();

            Usuario? user = null;


            if (userInputModel.Id.GetValueOrDefault(0) > 0)
            {
                user = userInputModel.Pessoa != null && userInputModel.Pessoa?.Id > 0 ?
                    (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where (p.Id = {userInputModel.Pessoa.Id} or u.Id = {userInputModel.Id}) and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ")).FirstOrDefault() :
                    (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {userInputModel.Id} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ")).FirstOrDefault();
            }


            if (!string.IsNullOrEmpty(userInputModel?.Pessoa?.EmailPreferencial))
            {
                pessoaEmailPreferencial = (await _repository.FindBySql<PessoaCompletaModel>($"Select p.* From Pessoa p Where Lower(p.EmailPreferencial) like '%{userInputModel.Pessoa.EmailPreferencial.TrimEnd().ToLower()}%' or Lower(p.EmailAlternativo) like '%{userInputModel.Pessoa.EmailPreferencial.TrimEnd().ToLower()}%'")).FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(userInputModel?.Pessoa?.EmailAlternativo))
            {
                pessoaEmailAlternativo = (await _repository.FindBySql<PessoaCompletaModel>($"Select p.* From Pessoa p Where Lower(p.EmailPreferencial) like '%{userInputModel.Pessoa.EmailAlternativo.TrimEnd().ToLower()}%' or Lower(p.EmailAlternativo) like '%{userInputModel.Pessoa.EmailAlternativo.TrimEnd().ToLower()}%'")).FirstOrDefault();
            }

            if (userInputModel?.Pessoa?.Documentos != null)
            {
                foreach (var documento in userInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)))
                {
                    pessoaDocumentoInformado = (await _repository.FindBySql<PessoaCompletaModel>(@$"Select 
                                                                                        p.* 
                                                                                       From 
                                                                                        Pessoa p 
                                                                                        Inner Join PessoaDocumento pd on pd.Pessoa = p.Id 
                                                                                       Where 
                                                                                        pd.TipoDocumento = {documento.TipoDocumentoId.GetValueOrDefault()} and 
                                                                                        (
                                                                                            pd.Numero = '{documento?.Numero?.TrimEnd()}' or 
                                                                                            pd.ValorNumerico = '{(documento?.Numero?.TrimEnd()).RemoveAccents(new List<string>() { ".", "-", "/" })}'                                                                    
                                                                                        )")).FirstOrDefault();

                    if (pessoaDocumentoInformado != null)
                        break;
                }
            }

            if (userInputModel != null && userInputModel.Pessoa != null && userInputModel.Pessoa.Id.GetValueOrDefault(0) == 0 && user != null && user.Pessoa.Id > 0)
            {
                userInputModel.Pessoa.Id = user.Pessoa.Id;
            }

            var pessoa = userInputModel?.Pessoa != null && userInputModel.Pessoa.Id.GetValueOrDefault(0) > 0 ?
                (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(@$"From Pessoa p
                                                        Where 
                                                            p.Id = {userInputModel.Pessoa.Id.GetValueOrDefault()}")).FirstOrDefault() :
                (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(@$"From Pessoa p
                                                        Where 
                                                            Lower(p.EmailPreferencial) like '%{userInputModel?.Pessoa?.EmailPreferencial?.ToLower()}%' or 
                                                            Lower(p.EmailAlternativo) like '%{(!string.IsNullOrEmpty(userInputModel?.Pessoa?.EmailAlternativo) ? userInputModel?.Pessoa?.EmailAlternativo?.ToLower() :
                                                            userInputModel?.Pessoa?.EmailPreferencial?.ToLower())}%'")).FirstOrDefault();

            Domain.Entities.Core.DadosPessoa.Pessoa pessoaOld = null;

            if (pessoa == null && userInputModel.Id.GetValueOrDefault(0) > 0)
            {
                user = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {userInputModel.Id.GetValueOrDefault()} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ")).FirstOrDefault();
                if (user != null)
                    pessoa = user.Pessoa;
            }


            if (pessoa == null)
            {
                if (pessoaEmailPreferencial != null)
                    throw new ArgumentException($"O email prefencial informado: '{userInputModel?.Pessoa?.EmailPreferencial}' já está sendo utilizado por outra pessoa");

                if (pessoaEmailAlternativo != null)
                    throw new ArgumentException($"O email alternativo informado: '{userInputModel?.Pessoa?.EmailAlternativo}' já está sendo utilizado por outra pessoa");

                if (pessoaDocumentoInformado != null)
                    throw new ArgumentException($"Um dos documentos informados: '{string.Join(",", userInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)).Select(a => a.Numero?.ToString()).ToList())}' já está sendo utilizado por outra pessoa");

                pessoa = _mapper.Map(userInputModel.Pessoa, new Domain.Entities.Core.DadosPessoa.Pessoa());

            }
            else
            {

                pessoaOld = await _serviceBase.GetObjectOld<Domain.Entities.Core.DadosPessoa.Pessoa>(pessoa.Id);
                if (!isAdm)
                {
                    if (companyConfiguration != null && companyConfiguration.PermitirUsuarioAlterarSeuEmail.GetValueOrDefault(EnumSimNao.Não) == EnumSimNao.Não)
                    {
                        if (!string.IsNullOrEmpty(pessoa.EmailAlternativo) && ((!string.IsNullOrEmpty(userInputModel?.Pessoa?.EmailAlternativo) && pessoa.EmailAlternativo != userInputModel?.Pessoa?.EmailAlternativo) ||
                            (!string.IsNullOrEmpty(pessoa.EmailAlternativo) && userInputModel?.Pessoa?.EmailAlternativo is null)))
                            throw new ArgumentException("O usuário não pode alterar o seu email alternativo");

                        if (!string.IsNullOrEmpty(pessoa.EmailPreferencial) && ((!string.IsNullOrEmpty(userInputModel?.Pessoa?.EmailPreferencial) && pessoa.EmailAlternativo != userInputModel?.Pessoa?.EmailPreferencial) ||
                            (!string.IsNullOrEmpty(pessoa.EmailPreferencial) && userInputModel?.Pessoa?.EmailPreferencial is null)))
                            throw new ArgumentException("O usuário não pode alterar o seu email preferencial");
                    }
                }


                pessoa = _mapper.Map(userInputModel.Pessoa, pessoa);

            }

            _serviceBase.Compare(pessoaOld, pessoa);

            await _repository.Save(pessoa);

            var pessoaSincronizacaoAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);


            if (userInputModel?.Pessoa != null && userInputModel.Pessoa.Enderecos != null && userInputModel.Pessoa.Enderecos.Any())
                await pessoaSincronizacaoAuxiliar.SincronizarEnderecos(pessoa, userInputModel.Pessoa.Enderecos.ToArray());

            if (userInputModel?.Pessoa != null && userInputModel.Pessoa.Telefones != null && userInputModel.Pessoa.Telefones.Any())
                await pessoaSincronizacaoAuxiliar.SincronizarTelefones(pessoa, userInputModel.Pessoa.Telefones.ToArray());

            if (userInputModel?.Pessoa != null && userInputModel.Pessoa.Documentos != null && userInputModel.Pessoa.Documentos.Any())
                await pessoaSincronizacaoAuxiliar.SincronizarDocumentos(pessoa, true, userInputModel.Pessoa.Documentos.ToArray());

            string senhaGerada = user == null ? "Abc@123" : "";

            if (user == null)
            {
               
                if (userInputModel != null && string.IsNullOrEmpty(userInputModel.Login) && userInputModel.Id == 0)
                {
                    if (userInputModel.Pessoa != null && userInputModel.Pessoa.Documentos != null && userInputModel.Pessoa.Documentos.Any(b => !string.IsNullOrEmpty(b.Numero)))
                    {
                        var fst = userInputModel.Pessoa.Documentos.First(b => !string.IsNullOrEmpty(b.Numero));
                        var apenasNumeros = SW_Utils.Functions.Helper.ApenasNumeros(fst.Numero);
                        userInputModel.Login = apenasNumeros;
                    }
                    else if (userInputModel.Pessoa != null && !string.IsNullOrEmpty(userInputModel.Pessoa.EmailPreferencial))
                    {
                        userInputModel.Login = userInputModel.Pessoa.EmailPreferencial;
                    }
                }

                var outroUsuarioMesmoLogin = (await _repository.FindBySql<Usuario>($"Select u.* From Usuario u Where Lower(u.Login) = '{userInputModel!.Login!.ToLower()}' and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                if (outroUsuarioMesmoLogin != null)
                    throw new Exception($"O Login: '{userInputModel.Login}' já está sendo utilizado por outro usuário");

                user = new()
                {
                    Pessoa = pessoa,
                    Login = userInputModel?.Login.RemoveAccents(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(senhaGerada),
                    DataHoraCriacao = DateTime.Now,
                    Status = EnumStatus.Ativo,
                    Administrador = userInputModel != null ? userInputModel.Administrador.GetValueOrDefault(EnumSimNao.Não) : EnumSimNao.Não,
                    GestorFinanceiro = userInputModel != null ? userInputModel.GestorFinanceiro.GetValueOrDefault(EnumSimNao.Não) : EnumSimNao.Não,
                    GestorReservasAgendamentos = userInputModel != null ? userInputModel.GestorReservasAgendamentos.GetValueOrDefault(EnumSimNao.Não) : EnumSimNao.Não,
                    LoginPms = userInputModel?.LoginPms,
                    LoginSistemaVenda = userInputModel?.LoginSistemaVenda
                };


            }
            else
            {
                if (userInputModel != null && userInputModel.Status.HasValue)
                {
                    user.Status = userInputModel.Status.GetValueOrDefault(EnumStatus.Ativo);
                }

                if (userInputModel != null && userInputModel.Administrador.HasValue)
                {
                    user.Administrador = userInputModel != null ? userInputModel.Administrador.GetValueOrDefault(EnumSimNao.Não) : EnumSimNao.Não;
                }
                if (!string.IsNullOrEmpty(userInputModel?.Login))
                {
                    var outroUsuarioMesmoLogin = (await _repository.FindBySql<Usuario>($"Select u.* From Usuario u Where Lower(u.Login) = '{userInputModel.Login.ToLower()}' and u.Id <> {user.Id}")).FirstOrDefault();
                    if (outroUsuarioMesmoLogin != null)
                        throw new Exception($"O Login: '{userInputModel.Login}' já está sendo utilizado por outro usuário");

                    user.Login = userInputModel.Login;
                }

                if (userInputModel != null)
                {
                    user.LoginPms = userInputModel.LoginPms;
                    user.LoginSistemaVenda = userInputModel.LoginSistemaVenda;
                }

            }

            await _repository.Save(user);

            var usuarioSincronizacaoAcessoAuxiliar = new UsuarioAcessoaSincronizacaoAuxiliar(_repository, _logger);

            if ((userInputModel?.UsuarioGruposUsuarios != null && userInputModel.UsuarioGruposUsuarios.Any()) || userInputModel.RemoverGrupoUsuariosNaoEnviados)
                await usuarioSincronizacaoAcessoAuxiliar.SincronizarGruposUsuarios(userInputModel.UsuarioGruposUsuarios, user, userInputModel.RemoverGrupoUsuariosNaoEnviados);

            if ((userInputModel.UsuarioEmpresas != null && userInputModel.UsuarioEmpresas.Any()) || userInputModel?.RemoverEmpresasNaoEnviadas == true)
                await usuarioSincronizacaoAcessoAuxiliar.SincronizarEmpresas(userInputModel?.UsuarioEmpresas, user, userInputModel.RemoverEmpresasNaoEnviadas);

            await SincronizarTagsRequeridas(user, userInputModel?.TagsRequeridas != null ? userInputModel.TagsRequeridas.AsList() : new List<int>(), userInputModel.RemoverTagsNaoEnviadas);

            if (!string.IsNullOrEmpty(senhaGerada))
            {
                var emailUtilizar = pessoa.EmailPreferencial ?? pessoa.EmailAlternativo;

                var emailsPermitidos = _configuration.GetValue<string>("DestinatarioEmailPermitido");
                var enviarEmailApenasParaDestinatariosPermitidos = _configuration.GetValue<bool>("EnviarEmailApenasParaDestinatariosPermitidos", true);


                if (enviarEmailApenasParaDestinatariosPermitidos)
                {
                    if (string.IsNullOrEmpty(emailsPermitidos) || string.IsNullOrEmpty(emailUtilizar) ||
                                        !emailsPermitidos.Contains(emailUtilizar, StringComparison.CurrentCultureIgnoreCase))
                    {
                        emailUtilizar = "glebersonsm@gmail.com;e.probst@mymabu.com.br";
                    }
                }

                if (!string.IsNullOrEmpty(emailUtilizar) && emailUtilizar.Contains("@"))
                {
                    await _emailService.SaveInternal(new EmailInputInternalModel()
                    {
                        UsuarioCriacao = user.UsuarioCriacao.GetValueOrDefault(_configuration.GetValue<int>("UsuarioSistemaId", 1)),
                        Assunto = "Senha gerada automaticamente pelo Portal do Proprietário",
                        Destinatario = emailUtilizar,
                        ConteudoEmail = $"Olá, {user.Pessoa?.Nome}!{Environment.NewLine}Sua senha de acesso ao sistema para o primeiro acesso é: <b>{senhaGerada}</b> favor altere ela por uma de sua escolha, após o primeiro login!"
                    });

                }
            }

            return user;
        }


        private async Task SincronizarTagsRequeridas(Usuario usuario, List<int> listTags, bool removerTagsNaoEnviadas = false)
        {
            if (removerTagsNaoEnviadas)
            {
                if (listTags == null || listTags.Count == 0)
                {
                    await _repository.ExecuteSqlCommand($"Delete From UsuarioTags Where Usuario = {usuario.Id}");
                    return;
                }
                else
                {
                    await _repository.ExecuteSqlCommand($"Delete From UsuarioTags Where Usuario = {usuario.Id} and tags not in ({string.Join(",", listTags)})");
                }
            }

            var allTags = listTags != null && listTags.Any() ? (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)})")).AsList() : new List<TagsModel>();
            var tagsInexistentes = listTags != null && listTags.Any() ? listTags.Where(c => !allTags.Any(b => b.Id == c)).AsList() : new List<int>();
            if (tagsInexistentes.Count > 0)
            {
                throw new ArgumentException($"Tags não encontradas: {string.Join(",", tagsInexistentes)}");
            }

            if (listTags != null && listTags.Any())
            {

                var tags = (await _repository.FindBySql<TagsModel>($"Select t.Id From Tags t Where t.Id in ({string.Join(",", listTags)}) and Not Exists(Select dc.Tags From UsuarioTags dc Where dc.Usuario = {usuario.Id} and dc.Tags = t.Id)")).AsList();

                foreach (var t in tags)
                {
                    var usuarioTags = new UsuarioTags()
                    {
                        Usuario = usuario,
                        Tags = new Tags() { Id = t.Id.GetValueOrDefault(0) }
                    };

                    await _repository.Save(usuarioTags);
                }
            }

        }

        public async Task<string> ResetPassword(ResetPasswordoUserModel model)
        {
            try
            {
                _repository.BeginTransaction();
                ArgumentNullException.ThrowIfNull(model, nameof(model));

                if (string.IsNullOrEmpty(model.Login))
                    throw new ArgumentException("O login do usuário deve ser informado");

                var usuario = (await _repository.FindByHql<Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Login = '{model.Login}'")).FirstOrDefault();
                if (usuario != null && !string.IsNullOrEmpty(usuario.Pessoa?.EmailPreferencial))
                {
                    if (!string.IsNullOrEmpty(model.Login) && !model.Login.Contains("@") && model.Login != model.Login.RemoveAccents())
                    {
                        model.Login = model.Login.RemoveAccents();
                        usuario.Login = model.Login;
                        await _repository.ForcedSave(usuario);
                    }
                    model.Login = usuario.Pessoa?.EmailPreferencial.Split(';')[0];
                    
                }
            

                var users = await GetUsuario(new LoginInputModel { Login = model.Login });
                if (users != null && users.Any())
                {
                    var userManter = users.OrderByDescending(x => x.Id).FirstOrDefault();
                    foreach (var item in users)
                    {
                        if (item.Id != userManter!.Id)
                        {
                            item.DataHoraRemocao = DateTime.Now;
                            item.Removido = EnumSimNao.Sim;
                            await _repository.ForcedSave(item);
                        }
                    }


                    if (userManter == null || userManter.Status != EnumStatus.Ativo)
                        throw new ArgumentException("Usuário não encontrado");

                    if (string.IsNullOrEmpty(userManter.Pessoa?.EmailAlternativo) && string.IsNullOrEmpty(userManter.Pessoa?.EmailPreferencial))
                        throw new ArgumentException("Não é possível resetar a senha do usuário, pois ele não possui um email cadastrado");


                    Random rd = new();
                    var codToSend = rd.Next(10000, 80000);

                    var password = $"Ab{codToSend}$";

                    userManter.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    await _repository.Save(userManter);

                    var emailsPermitidos = _configuration.GetValue<string>("DestinatarioEmailPermitido");
                    var enviarEmailApenasParaDestinatariosPermitidos = _configuration.GetValue<bool>("EnviarEmailApenasParaDestinatariosPermitidos", true);

                    var pessoa = (await _repository.FindBySql<PessoaModel>($"Select Coalesce(p.EmailPreferencial,EmailAlternativo) as Email, p.Id as IdPessoa From Pessoa p Where p.Id = {userManter.Pessoa.Id}")).FirstOrDefault();

                    var emailUtilizar = pessoa?.Email;

                    if (enviarEmailApenasParaDestinatariosPermitidos)
                    {
                        if (string.IsNullOrEmpty(emailsPermitidos) || string.IsNullOrEmpty(emailUtilizar) ||
                                            !emailsPermitidos.Contains(emailUtilizar, StringComparison.CurrentCultureIgnoreCase))
                        {
                            emailUtilizar = "glebersonsm@gmail.com";
                        }
                    }

                    if (!string.IsNullOrEmpty(emailUtilizar) && emailUtilizar.Contains("@"))
                    {
                        await _emailService.SaveInternal(new EmailInputInternalModel()
                        {
                            UsuarioCriacao = userManter.UsuarioCriacao.GetValueOrDefault(_configuration.GetValue<int>("UsuarioSistemaId", 1)),
                            Assunto = "Nova senha gerada automaticamente pelo Portal do Proprietário",
                            Destinatario = emailUtilizar,
                            ConteudoEmail = $"Olá, {userManter.Pessoa?.Nome}!{Environment.NewLine}Sua senha de acesso ao sistema foi modificada para: <b>{password}</b> favor altere ela novamente por uma de sua escolha, após o primeiro login!"
                        });

                    }

                    var (executed, exception) = await _repository.CommitAsync();
                    if (executed)
                    {
                        return $"A nova senha foi enviada para o email: {emailUtilizar}";
                    }
                    else throw exception ?? new Exception("Erro na operação");
                }
                else throw new ArgumentException("Usuário não encontrado");
            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        private async Task<List<Usuario>?> GetUsuario(LoginInputModel userLoginInputModel)
        {
            //'áàãâäéèêëíìîïóòõôöúùûüç', 'aaaaaeeeeiiiiooooouuuuc'
            var resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p 
                                                                      Where 
                                                                         (Translate(Lower(u.Login),'áàãâäéèêëíìîïóòõôöúùûüç', 'aaaaaeeeeiiiiooooouuuuc') = '{userLoginInputModel.Login.ToLower().RemoveAccents().TrimEnd().TrimStart()}') 
                                                                         and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null")).AsList();

            if (resultNew == null || resultNew.Count() == 0)
            {
                resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p 
                                                                      Where 
                                                                         (p.EmailPreferencial is not null and p.EmailPreferencial like '%@%' and Lower(split_part(p.EmailPreferencial,';',1)) = '{userLoginInputModel.Login.ToLower().TrimEnd().TrimStart()}')
                                                                         and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null
                                                                      ")).AsList();
            }

            if (resultNew == null || resultNew.Count() == 0)
            {
                resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p
                                                                      Where 
                                                                        Exists(Select pd.Pessoa From PessoaDocumento pd Where pd.Numero is not null and Lower(pd.Numero) = '{userLoginInputModel.Login.ToLower().TrimEnd().TrimStart()}' and pd.Pessoa = p.Id) 
                                                                        and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null
                                                                      ")).AsList();
            }

            if (resultNew == null || resultNew.Count() == 0)
            {
                resultNew = (await _repository.FindByHql<Usuario>(@$"From 
                                                                        Usuario u 
                                                                        Inner Join Fetch u.Pessoa p
                                                                      Where 
                                                                        Exists(Select pd.Pessoa From PessoaDocumento pd Where pd.Numero is not null and Lower(pd.NumeroFormatado) = '{userLoginInputModel.Login.ToLower().TrimEnd().TrimStart()}' and pd.Pessoa = p.Id) 
                                                                        and Coalesce(u.Removido,0) = 0 and u.DataHoraRemocao is null
                                                                      ")).AsList();
            }
                

            //if (userLoginInputModel.Login.Contains("@"))
            //{
            //    var users = (await _repository.FindBySql<UsuarioModel>(@$"Select 
            //                                                                u.Id as UsuarioId, p.Id as PessoaId 
            //                                                              From 
            //                                                                Usuario u 
            //                                                                Inner Join Pessoa p on u.Pessoa = p.Id
            //                                                          Where 
            //                                                                 p.EmailPreferencial is not null and 
            //                                                                p.EmailPreferencial like '%@%' and 
            //                                                                Lower(Coalesce(split_part(p.EmailPreferencial,';',1),p.EmailPreferencial)) = '{userLoginInputModel.Login.ToLower()}'")).AsList();

            //    if (users != null && users.Count() > 1)
            //        throw new ArgumentException("Não foi possível resetar a senha pelo seu email, utilize o login ou o CPF/CNPJ/Passaporte para realizar a operação.");

            //}

            return resultNew;

        }
    }
}
