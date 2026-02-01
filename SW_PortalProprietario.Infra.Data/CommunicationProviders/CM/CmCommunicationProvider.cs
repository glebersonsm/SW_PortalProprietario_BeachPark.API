using CMDomain.Entities;
using CMDomain.Models.Empresa;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Remotion.Linq.Parsing;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras.Boleto;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers;
using SW_PortalProprietario.Application.Services.Providers.Esolution;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Infra.Data.Caching;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System.Text;

namespace SW_PortalProprietario.Infra.Data.CommunicationProviders.CM
{
    public class CmCommunicationProvider : ICommunicationProvider
    {
        private const string PREFIXO_TRANSACOES_FINANCEIRAS = "PORTALPROPCM_";
        private const string CLIENTES_INADIMPLENTES = "CLIENTESINADIMPLENTESCM_";
        private readonly IConfiguration _configuration;
        private readonly IRepositoryNHCm _repositoryCm;
        private readonly ILogger<CmCommunicationProvider> _logger;
        private readonly ICacheStore _cacheStore;
        private readonly IRepositoryNH _repositorySystem;

        public string CommunicationProviderName => "CM";

        public string PrefixoTransacaoFinanceira => PREFIXO_TRANSACOES_FINANCEIRAS;

        public CmCommunicationProvider(IConfiguration configuration,
             IRepositoryNHCm repositoryCm,
             ILogger<CmCommunicationProvider> logger,
             ICacheStore cacheStore,
             IRepositoryNH repositorySystem)
        {
            _configuration = configuration;
            _repositoryCm = repositoryCm;
            _logger = logger;
            _cacheStore = cacheStore;
            _repositorySystem = repositorySystem;
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

            var resultValidate = await ValidarLoginTimeSharingCM(login, senha, pessoaProviderId);
            if (resultValidate != null)
                modelReturn.LoginResult = resultValidate;

            return modelReturn;
        }

        private async Task<LoginResult> ValidarLoginTimeSharingCM(string login, string senha, string pessoaPoviderId = "")
        {
            var empresaId = _configuration.GetValue("EmpresaCMId", 0);
            if (empresaId == 0)
                throw new ArgumentException("Empresa não configurada");

            var empresa = (await _repositoryCm.FindBySql<EmpresaPropModel>($"Select emp.* From EmpresaProp emp Where emp.IdPessoa = {empresaId}")).FirstOrDefault();
            if (empresa == null)
                throw new ArgumentException($"Não foi encontrada empresa com Id: {empresaId}");


            var loginResult = new LoginResult();

            bool cpf = false;
            bool cnpj = false;


            var apensasNumeros = Helper.ApenasNumeros(login);


            if (!string.IsNullOrEmpty(login) && !login.Contains("@"))
            {
                if (apensasNumeros.Length > 11)
                {
                    if (!Helper.IsCnpj(apensasNumeros))
                        loginResult.message = $"O CNPJ informado: {login} não é válido";
                    cnpj = true;
                }
                else if (apensasNumeros.Length <= 11)
                {
                    if (!Helper.IsCpf(apensasNumeros))
                        loginResult.message = $"O CPF informado: {login} não é válido";

                    cpf = true;
                }
            }

            if (!string.IsNullOrEmpty(loginResult.message))
                return await Task.FromResult(loginResult);

            CMDomain.Models.Pessoa.PessoaModel? pessoaCM = null;

            if (!string.IsNullOrEmpty(pessoaPoviderId))
            {
                pessoaCM = (await _repositoryCm.FindBySql<CMDomain.Models.Pessoa.PessoaModel>($@"
                                                                    SELECT
                                                                    DISTINCT
                                                                       PESSOA.IDPESSOA,    
                                                                       PESSOA.NOME,
                                                                       PESSOA.NUMDOCUMENTO AS DOCUMENTO,
                                                                       VENDAXCONTRATOTS.NUMEROCONTRATO,
                                                                       PESSOA.EMAIL
                                                                    FROM
                                                                       PESSOA,
                                                                       EMPRESACLIENTE,
                                                                       CLIENTEPESS,
                                                                       ENDPESS,
                                                                       CIDADES,
                                                                       ESTADO,
                                                                       PAIS,
                                                                       TIPOCLIENTE,
                                                                       VENDATS,
                                                                       ATENDCLIENTETS,
                                                                       VENDAXCONTRATOTS,
                                                                       AGENCIATS
                                                                    WHERE
                                                                       ( PESSOA.IDPESSOA           = EMPRESACLIENTE.IDFORCLI ) AND
                                                                       ( PESSOA.IDPESSOA           = CLIENTEPESS.IDPESSOA ) AND
                                                                       ( EMPRESACLIENTE.FLGSTATUS  = 'A' OR EMPRESACLIENTE.FLGSTATUS  IS NULL ) AND
                                                                       ( ENDPESS.IDCIDADES         = CIDADES.IDCIDADES(+) ) AND
                                                                       ( CIDADES.IDESTADO          = ESTADO.IDESTADO(+) ) AND
                                                                       ( ESTADO.IDPAIS             = PAIS.IDPAIS(+) ) AND
                                                                       ( ENDPESS.IDENDERECO(+)     = PESSOA.IDENDCOMERCIAL ) AND
                                                                       ( CLIENTEPESS.IDTIPOCLIENTE = TIPOCLIENTE.IDTIPOCLIENTE ) AND
                                                                       ( ATENDCLIENTETS.IDCLIENTE  = PESSOA.IDPESSOA ) AND
                                                                       ( VENDATS.IDATENDCLIENTETS  = ATENDCLIENTETS.IDATENDCLIENTETS ) AND
                                                                       ( VENDAXCONTRATOTS.IDVENDATS = VENDATS.IDVENDATS ) AND
                                                                       ( VENDAXCONTRATOTS.FLGCANCELADO = 'N' ) AND
                                                                       ( VENDAXCONTRATOTS.FLGREVERTIDO = 'N' ) AND
                                                                       ( ((VENDAXCONTRATOTS.PREVENDA = 'N') OR (VENDAXCONTRATOTS.PREVENDA IS NULL))  ) AND
                                                                       ( VENDAXCONTRATOTS.IDAGENCIATS = AGENCIATS.IDAGENCIATS ) AND
                                                                       (  AGENCIATS.IDPESSOA = {empresaId} ) AND
                                                                       ( PESSOA.IDPESSOA = {pessoaPoviderId})
                                                                     ORDER BY PESSOA.IDPESSOA ASC")).FirstOrDefault();


                if (pessoaCM == null)
                {
                    loginResult.message = $"Usuário não encontrado";
                    return await Task.FromResult(loginResult);
                }
            }

            if (pessoaCM == null && (cpf || cnpj))
            {
                if (cpf || cnpj)
                {
                    if (apensasNumeros.Length > 11)
                    {
                        apensasNumeros = apensasNumeros.PadLeft(14, '0');
                    }
                    else apensasNumeros = apensasNumeros.PadLeft(11, '0');
                }

                pessoaCM = (await _repositoryCm.FindBySql<CMDomain.Models.Pessoa.PessoaModel>($@"SELECT
                                                                    DISTINCT
                                                                       PESSOA.IDPESSOA,    
                                                                       PESSOA.NOME,
                                                                       PESSOA.NUMDOCUMENTO AS DOCUMENTO,
                                                                       VENDAXCONTRATOTS.NUMEROCONTRATO,
                                                                       PESSOA.EMAIL
                                                                    FROM
                                                                       PESSOA,
                                                                       EMPRESACLIENTE,
                                                                       CLIENTEPESS,
                                                                       ENDPESS,
                                                                       CIDADES,
                                                                       ESTADO,
                                                                       PAIS,
                                                                       TIPOCLIENTE,
                                                                       VENDATS,
                                                                       ATENDCLIENTETS,
                                                                       VENDAXCONTRATOTS,
                                                                       AGENCIATS
                                                                    WHERE
                                                                       ( PESSOA.IDPESSOA           = EMPRESACLIENTE.IDFORCLI ) AND
                                                                       ( PESSOA.IDPESSOA           = CLIENTEPESS.IDPESSOA ) AND
                                                                       ( EMPRESACLIENTE.FLGSTATUS  = 'A' OR EMPRESACLIENTE.FLGSTATUS  IS NULL ) AND
                                                                       ( ENDPESS.IDCIDADES         = CIDADES.IDCIDADES(+) ) AND
                                                                       ( CIDADES.IDESTADO          = ESTADO.IDESTADO(+) ) AND
                                                                       ( ESTADO.IDPAIS             = PAIS.IDPAIS(+) ) AND
                                                                       ( ENDPESS.IDENDERECO(+)     = PESSOA.IDENDCOMERCIAL ) AND
                                                                       ( CLIENTEPESS.IDTIPOCLIENTE = TIPOCLIENTE.IDTIPOCLIENTE ) AND
                                                                       ( ATENDCLIENTETS.IDCLIENTE  = PESSOA.IDPESSOA ) AND
                                                                       ( VENDATS.IDATENDCLIENTETS  = ATENDCLIENTETS.IDATENDCLIENTETS ) AND
                                                                       ( VENDAXCONTRATOTS.IDVENDATS = VENDATS.IDVENDATS ) AND
                                                                       ( VENDAXCONTRATOTS.FLGCANCELADO = 'N' ) AND
                                                                       ( VENDAXCONTRATOTS.FLGREVERTIDO = 'N' ) AND
                                                                       ( ((VENDAXCONTRATOTS.PREVENDA = 'N') OR (VENDAXCONTRATOTS.PREVENDA IS NULL))  ) AND
                                                                       ( VENDAXCONTRATOTS.IDAGENCIATS = AGENCIATS.IDAGENCIATS ) AND
                                                                       (  AGENCIATS.IDPESSOA = {empresaId} ) AND
                                                                       exists(Select dp.IdPessoa From DocPessoa dp Where Replace(Replace(Replace(dp.NumDocumento,'-',''),'.',''),'/','') like '{apensasNumeros}%' and dp.IdPessoa = PESSOA.IdPessoa) ")).FirstOrDefault();
            }


            if (pessoaCM == null)
            {
                loginResult.message = "Usuário não encontrado";
                return await Task.FromResult(loginResult);
            }

            string strTelefoneUtilizar = "";

            var pessoaTelefone = (await _repositoryCm.FindBySql<TelEndPess>($"Select tp.* From TelendPess tp Inner Join EndPess ep on ep.IdEndereco = tp.IdEndereco Where ep.IdPessoa = {pessoaCM.IdPessoa}")).FirstOrDefault();
            if (pessoaTelefone != null)
            {
                strTelefoneUtilizar = $"{pessoaTelefone.Ddi} {pessoaTelefone.Ddd}{pessoaTelefone.Numero}";
            }

            loginResult.dadosCliente = new DadosClienteLegado()
            {
                Nome = pessoaCM?.Nome!.Split(' ')[0],
                SobreNome = pessoaCM?.Nome!.Replace(pessoaCM.Nome.Split(' ')[0], "").TrimStart(' '),
                Permissao = "cliente",
                Plataforma = "TimeSharingCM",
                Empreendimento = $"{empresa.NomeEmpresa}",
                Telefone = strTelefoneUtilizar,
                Email = pessoaCM?.Email,
                PessoaId = $"{pessoaCM?.IdPessoa}",
                Identificacao = pessoaCM?.Documento ?? apensasNumeros
            };


            loginResult.dadosCliente.AccessToten = "";

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

        public async Task<bool> IsDefault()
        {
            return await Task.FromResult(false);
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
            return (await _repositoryCm.FindBySql<EmpresaSimplificadaModel>($@"Select 
            p.Nome AS NomeFantasia, 
            p.RazaoSocial as Nome, 
            to_char(ep.IdPessoa) as Codigo, 
            to_char(p.NumDocumento) as Cnpj, 
            p.Email 
            From 
            EmpresaProp ep 
            Inner Join Pessoa p on ep.IdPessoa = p.IdPessoa 
            Where ep.IdPessoa = {id}")).FirstOrDefault();
        }

        public async Task<List<PaisModel>> GetPaisesLegado()
        {
            return (await _repositoryCm.FindBySql<PaisModel>("Select p.Nome, p.CodBancoCentral as CodigoIbge From Pais p")).AsList();
        }

        public async Task<List<EstadoModel>> GetEstadosLegado()
        {
            return (await _repositoryCm.FindBySql<EstadoModel>(@"Select 
                        e.NomeEstado as Nome, 
                        e.CodEstado as Sigla,
                        p.CodBancoCentral as PaisCodigoIbge,
                        p.NomePais,
                        e.CodUfIbge as CodigoIbge
                        From Estado e Inner Join Pais p on e.IdPais = p.IdPais")).AsList();
        }

        public async Task<List<CidadeModel>> GetCidade()
        {
            return (await _repositoryCm.FindBySql<CidadeModel>(@"Select 
                c.Nome,
                c.CODMUNICIPIOIBGE AS CodigoIbge,
                e.NomeEstado as EstadoNome, 
                e.CodEstado as EstadoSigla,
                p.CodBancoCentral as PaisCodigoIbge,
                p.NomePais as PaisNome,
                e.CodUfIbge as EstadoCodigoIbge
                From 
	                Cidades c 
                    Inner JOIN Estado e ON C.IdEstado = e.IdEstado 
                    Inner Join Pais p on e.IdPais = p.IdPais
                                        ")).AsList();
        }

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade(CidadeSearchModel searchModel)
        {
            var sb = new StringBuilder(@"Select distinct
                RTrim(Concat(Concat(c.Nome,'/'),e.CodEstado)) as Nome,
                c.CODMUNICIPIOIBGE AS CodigoIbge,
                e.NomeEstado as EstadoNome, 
                e.CodEstado as EstadoSigla,
                p.CodBancoCentral as PaisCodigoIbge,
                p.NomePais as PaisNome,
                e.CodUfIbge as EstadoCodigoIbge
                From 
	                Cidades c 
                    Inner JOIN Estado e ON C.IdEstado = e.IdEstado 
                    Inner Join Pais p on e.IdPais = p.IdPais 
             Where 1 = 1 ");

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and c.IdCidades = {searchModel.Id} ");
            }

            if (!string.IsNullOrEmpty(searchModel.CodigoIbge))
            {
                sb.AppendLine($" and Lower(c.CODMUNICIPIOIBGE) like '%{searchModel.CodigoIbge.ToLower().TrimEnd()}%' ");
            }

            if (!string.IsNullOrEmpty(searchModel.Nome))
            {
                var arrCidadeSigla = searchModel.Nome.Split('/');
                if (arrCidadeSigla.Length == 2)
                {
                    sb.AppendLine($" and Lower(c.Nome) like '%{arrCidadeSigla[0].ToLower().Trim()}%' and Lower(e.CodEstado) like '%{arrCidadeSigla[1].ToLower().Trim()}%'");
                }
                else if (arrCidadeSigla.Length == 1)
                {
                    sb.AppendLine($" and Lower(c.Nome) like '%{arrCidadeSigla[0].ToLower().TrimEnd()}%' ");
                }
            }

            if (!string.IsNullOrEmpty(searchModel.Search))
            {
                if (Helper.IsNumeric(searchModel.Search.Trim()))
                {
                    sb.AppendLine($" and c.IdCidades = {searchModel.Id} ");
                }
                else
                {
                    var arrCidadeSigla = searchModel.Search.Split('/');
                    if (arrCidadeSigla.Length == 2)
                    {
                        sb.AppendLine($" and Lower(c.Nome) like '%{arrCidadeSigla[0].ToLower().Trim()}%' and Lower(e.CodEstado) like '%{arrCidadeSigla[1].ToLower().Trim()}%'");
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
                totalRegistros = Convert.ToInt32((await _repositoryCm.CountTotalEntry(sql, new List<Parameter>().ToArray())));
            }

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine(" Order by 1 ");

            var cidades = searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 ?
                await _repositoryCm.FindBySql<CidadeModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(1), new List<Parameter>().ToArray())
                : await _repositoryCm.FindBySql<CidadeModel>(sb.ToString(), new List<Parameter>().ToArray());


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
            var usuariosAtivos = (await _repositoryCm.FindBySql<UserRegisterInputModel>(@"SELECT
                c.NomeUsuario as Login,
                pes.IdPessoa AS PessoaId,
                pes.Nome AS FullName,
                pes.NumDocumento AS CpfCnpj,
                tdp.NOMEDOCUMENTO AS TipoDocumentoClienteNome,
                pes.Email,
                1 AS Administrador,
                '' as Password,
                '' as PasswordConfirmation
                FROM
                UsuarioSistema c
                INNER JOIN Pessoa pes ON c.IdUsuario = pes.IdPessoa
                LEFT JOIN TipoDocPessoa tdp ON pes.IDDOCUMENTO = tdp.IDDOCUMENTO
                WHERE
                Nvl(c.Bloqueado,'N') = 'N' AND
                nvl(c.Desativado,'N') = 'N' AND 
                pes.Nome is not null and length(pes.Nome) > 2 and 
                (pes.IdPessoa in (SELECT us.IdUsuario 
                    FROM 
                    USUXMODXEMP uxe 
                    INNER JOIN UsuarioSistema us ON us.idespacesso = uxe.Idespacesso  
                    WHERE uxe.IDMODULO = 769) or Exists(Select 
                                                            gu.IdUsuario 
                                                        From 
                                                            GrupoUsu gu 
                                                            Inner Join GrupoAcesso ga on gu.IdGrupo = ga.IdGrupo
                                                        Where 
                                                            Exists(SELECT uxe1.IdEspAcesso FROM USUXMODXEMP uxe1 WHERE uxe1.IdModulo = 769 AND uxe1.IdEspAcesso = ga.IDESPACESSO) and
                                                        	gu.IdUsuario = c.IdUsuario))")).AsList();


            return usuariosAtivos.DistinctBy(a=> a.PessoaId).AsList();
        }

        public async Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado(ParametroSistemaViewModel parametroSistema)
        {
            var clientesAtivos = (await _repositoryCm.FindBySql<UserRegisterInputModel>(@"SELECT
                cp.IdPessoa AS PessoaId,
                pes.Nome AS FullName,
                pes.NumDocumento AS CpfCnpj,
                tdp.NOMEDOCUMENTO AS TipoDocumentoClienteNome,
                pes.Email,
                0 AS Administrador,
                '' as Password,
                '' as PasswordConfirmation
                FROM
                ContratoTs c
                INNER JOIN Pessoa p ON c.IdPessoa = p.IdPessoa
                INNER JOIN VENDAXCONTRATOTS vts ON c.IdContratoTs = c.IdContratoTs
                INNER JOIN ProjetoTs pro ON vts.IdProjetoTs = pro.IDPROJETOTS
                INNER JOIN VendaTs v ON v.IdVendaTs = vts.IdVendaTs AND vts.IDCONTRATOTS = c.IDCONTRATOTS
                INNER JOIN ATENDCLIENTETS a ON vts.IDATENDCLIENTETS = a.IDATENDCLIENTETS
                INNER JOIN CLIENTEPESS cp ON cp.IdPessoa = a.IDCLIENTE
                INNER JOIN Pessoa pes ON cp.IdPessoa = pes.IdPessoa
                LEFT JOIN TipoDocPessoa tdp ON pes.IDDOCUMENTO = tdp.IDDOCUMENTO
                WHERE
                vts.FLGCANCELADO = 'N' AND
                vts.FlgRevertido = 'N' AND 
                pes.Nome is not null and length(pes.Nome) > 2 ")).AsList();

            foreach (var item in clientesAtivos)
            {
                if (!string.IsNullOrEmpty(item.CpfCnpj))
                {
                    item.Login = $"{item.CpfCnpj.TrimStart().TrimEnd()}";
                }
                else if (!string.IsNullOrEmpty(item.Email) && item.Email.Contains("@"))
                {
                    item.Login = item.Email.Split(';')[0];
                }
                else if (!string.IsNullOrEmpty(item.FullName))
                {
                    item.Login = $"{item.FullName.Split(' ')[0]}.{item.PessoaId}";
                }
            }

            return clientesAtivos.DistinctBy(a=> a.PessoaId).AsList();
        }

        public Task<bool> DesativarUsuariosSemCotaOuContrato()
        {
            throw new NotImplementedException();
        }

        public Task GetOutrosDadosUsuario(TokenResultModel userReturn)
        {
            throw new NotImplementedException();
        }

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(List<string> empresasIds)
        {
            var empresasReturn = (await _repositoryCm.FindBySql<EmpresaVinculadaModel>(@$"SELECT
                    ep.IdPessoa AS Id,
                    ep.NOMEEMPRESA as Nome
                    FROM 
                    EmpresaProp ep
                    WHERE
                    ep.FLGATIVO  = 'S'
                    AND ep.idPessoa IN ({string.Join(",", empresasIds)})
                    Order by ep.IdPessoa ")).AsList();
            return empresasReturn;
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

        public async Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar)
        {

            List<DadosContratoModel> contratos = new();

            var itemCache = await _cacheStore.GetAsync<List<DadosContratoModel>>("contratosTimeSharingCache_", 10, _repositoryCm.CancellationToken);
            if (itemCache != null && itemCache.Any())
                return itemCache;

            var sqlStatusCrc = new StringBuilder(@$"SELECT 
                   vc.IdVendaXContrato as FrAtendimentoVendaId,
                   vc.IdVendaXContrato as IdVendaXContrato,
                   vc.NumeroContrato,
                   CASE 
   	                WHEN nvl(vc.FlgRevertido,'N')= 'S' THEN 'Revertido'
   	                WHEN nvl(vc.FlgCancelado,'N')= 'S' THEN 'Cancelado'
   	                ELSE 'Ativo' END AS Status,
                    V.DataVenda,
                    can.DataCancelamento,
                    rev.DataReversao,
   	                (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(rev.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12) 
                      WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(Rev.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                      ELSE NVL(rev.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END) AS DATAVALIDADE,
                   rev.DataReversao, 
                   can.DataCancelamento,
                   aten.idcliente as PessoaTitular1Id,
                   p.nome AS PessoaTitular1Nome,
                   p.numdocumento AS PessoaTitular1CPF,
                   p.Email AS PessoaTitular1Email,
                   c.IdHotel as Empreendimento,
                   c.Nome as Produto,
                   c.Nome as GrupoCotaTipoCotaNome,
                   To_char(c.IdContratoTs) as GrupoCotaTipoCotaCodigo,
                   Concat(Concat(pro.NumeroProjeto,'-'),to_char(vc.NumeroContrato)) as ProjetoXContrato
                   FROM 
                   vendaxcontratots vc
                   INNER JOIN contratots c ON vc.idcontratots = c.idcontratots
                   INNER JOIN VendaTs v ON vc.idvendats = v.IdVendaTs
                   INNER JOIN AtendClienteTs aten ON v.idatendclientets = aten.idatendclientets
                   INNER JOIN pessoa p ON aten.idcliente = p.idpessoa
                   LEFT OUTER JOIN RevContratoTs rev ON rev.IDVENDAXCONTRNOVO = vc.IdVendaXContrato
                   LEFT OUTER JOIN CANCCONTRATOTS can ON can.IdVendaXContrato = vc.IdVendaXContrato
                   LEFT OUTER JOIN ProjetoTs pro ON vc.IdProjetoTs = pro.IdProjetoTs
                   WHERE
                   1 = 1 and 
                   vc.FlgCancelado = 'N' and 
                   vc.FlgRevertido = 'N' and 
                    CASE 
   	                WHEN nvl(vc.FlgRevertido,'N')= 'S' THEN 'Revertido'
   	                WHEN nvl(vc.FlgCancelado,'N')= 'S' THEN 'Cancelado'
   	                ELSE 'Ativo' END = 'Ativo' ");


            if (pessoasPesquisar != null && pessoasPesquisar.Any())
            {
                sqlStatusCrc.AppendLine($" and (p.IdPessoa in ({string.Join(",", pessoasPesquisar)}) ) ");
            }

            contratos = (await _repositoryCm.FindBySql<DadosContratoModel>(sqlStatusCrc.ToString())).AsList();
            if (contratos != null && contratos.Any())
            {
                var statusCrcContratos = await GetStatusCrc(contratos.Select(a => a.IdVendaXContrato.GetValueOrDefault(0)).AsList());
                if (statusCrcContratos != null && statusCrcContratos.Any())
                {
                    foreach (var item in contratos)
                    {
                        var statusAtivosDoContrato = statusCrcContratos.Where(a => a.IdVendaXContrato.GetValueOrDefault() == item.IdVendaXContrato.GetValueOrDefault()).AsList();
                        item.frAtendimentoStatusCrcModels = statusAtivosDoContrato;
                    }
                }
            }

            if (contratos != null && contratos.Any())
                await _cacheStore.AddAsync("contratosTimeSharingCache_", contratos, DateTimeOffset.Now.AddHours(1), 10, _repositoryCm.CancellationToken);

            return contratos;
        }

        public async Task<List<StatusCrcContratoModel>?> GetStatusCrc(List<int> frAtendimentoVendaIds)
        {

            List<StatusCrcContratoModel>? status = new();
            if (!frAtendimentoVendaIds.Any()) return default;

            var sbList = Helper.Sublists(frAtendimentoVendaIds, 1000);
            foreach (var item in sbList)
            {
                var sqlStatusCrcCM = new StringBuilder($@"SELECT 
                                                        b.IDBLOQCLIENTETS AS AtendimentoStatusCrcId,
                                                        b.IDVENDAXCONTRATO AS FrAtendimentoVendaId,
                                                        b.IDVENDAXCONTRATO,
                                                        p.IDPESSOA,
                                                        p.Nome AS NomeTitular,
                                                        p.NUMDOCUMENTO AS Cpf_Cnpj_Titular,
                                                        m.IDMOTIVOTS AS FrStatusCrcId,
                                                        m.CODREDUZIDO AS CodigoStatus,
                                                        m.DESCRICAO AS NomeStatus
                                                        FROM 
                                                        BLOQCLIENTETS b
                                                        INNER JOIN MOTIVOTS m ON b.IDMOTIVOTS = m.IDMOTIVOTS 
                                                        INNER JOIN Pessoa p ON b.IDCLIENTE = p.IDPESSOA 
                                                        WHERE
                                                        b.FLGLIBERADO  = 'N' ");


                sqlStatusCrcCM.AppendLine($" and b.IdVendaXContrato in ({string.Join(",", item)}) ");

                status.AddRange((await _repositoryCm.FindBySql<StatusCrcContratoModel>(sqlStatusCrcCM.ToString())).AsList());
            }

            return status;

        }

        public async Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null)
        {

            List<ClientesInadimplentes> clientesInadimplentes = new List<ClientesInadimplentes>();
            var itemCache = pessoasPesquisar == null || !pessoasPesquisar.Any() ? 
                await _cacheStore.GetAsync<List<ClientesInadimplentes>>(CLIENTES_INADIMPLENTES, 10, _repositoryCm.CancellationToken) : 
                new List<ClientesInadimplentes>();

            if (itemCache != null && itemCache.Any())
                return itemCache;

            var sbSql = new StringBuilder(@$"Select b.* From (SELECT Nvl(RC.DATAREVERSAO,V.DataVenda) AS DataVenda,
                                      VC.NUMEROCONTRATO,
                                      VC.IDVENDAXCONTRATO,
                                      VC.IDVENDATS,
                                      Coalesce(PAGTO.ABERTO_VENCIDO,0) as SaldoInadimplente,
                                      ROUND((CASE WHEN VC.FLGREVERTIDO = 'N' AND VC.FLGCANCELADO = 'N' THEN
                                         (NVL(VAL.TOTAL, VC.VALORFINAL) + NVL(COMPRADOS.PAGTO,0) - PAGTO.ABERTO_A_VENCER - PAGTO.ABERTO_VENCIDO)
                                        ELSE
                                          (PAGTO.QUITADO + NVL(COMPRADOS.PAGTO,0))
                                        END * 100) / CASE WHEN (NVL(VAL.TOTAL, VC.VALORFINAL) + NVL(COMPRADOS.PAGTO,0)) > 0 THEN (NVL(VAL.TOTAL, VC.VALORFINAL) + NVL(COMPRADOS.PAGTO,0)) ELSE 1 END,5) PERCENTUALINTEGRALIZACAO,
                                       CASE WHEN VC.FLGCANCELADO = 'S' THEN 'CANCELADO'  
                                            WHEN VC.FLGREVERTIDO = 'S' THEN 'REVERTIDO'  
                                            WHEN ((PAR.DATASISTEMA > (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12)  
                                                                           WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                                                                           ELSE NVL(RC.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END))                                          
                                       AND (VC.IDSEMANAFIXAUH IS NULL)) THEN 'EXPIRADO' ELSE 'ATIVO' END AS STATUS,
                                       ROUND((CASE WHEN VC.FLGREVERTIDO = 'N' AND VC.FLGCANCELADO = 'N' THEN
                                         (NVL(VAL.TOTAL, VC.VALORFINAL) + NVL(COMPRADOS.PAGTO,0) - PAGTO.ABERTO_A_VENCER - PAGTO.ABERTO_VENCIDO)
                                        ELSE
                                          (PAGTO.QUITADO + NVL(COMPRADOS.PAGTO,0))
                                        END)) as ValorTotalPago,
                                        NVL(VAL.TOTAL, VC.VALORFINAL) as ValorTotalContrato,
                                        C.NUMEROPONTOS,
                                        A.IDCLIENTE,
                                        P.NOME,
                                        P.NUMDOCUMENTO AS CpfCnpj,
                                        P.EMAIL
                                    FROM   
                                      VENDAXCONTRATOTS VC, 
                                      VENDATS V, 
                                      ATENDCLIENTETS A, 
                                      PESSOA P,
                                      PROJETOTS PJ, 
                                      CONTRATOTS C, 
                                      PESSOA AG,
                                      CANCCONTRATOTS CC,
                                      REVCONTRATOTS RC,
                                      LOCAISATENDTS LA,
                                      HOTEL H,
                                      PARAMTS PAR,
                                      VWENDERECO EP,
                                      PESSOA PRO,
                                      (SELECT VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS) AS IDREVCONTRATOTS, ABS(SUM(L.VLRLANCAMENTO)) AS TOTAL
                                         FROM LANCAMENTOTS L, VENDATS V, VENDAXCONTRATOTS VC, CONTRATOTS C, AJUSTEFINANCTS AJ, REVCONTRATOTS R
                                        WHERE L.IDVENDATS         = V.IDVENDATS
                                          AND V.IDVENDATS         = VC.IDVENDATS
                                          AND VC.IDCONTRATOTS     = C.IDCONTRATOTS
                                          AND L.IDAJUSTEFINANCTS  = AJ.IDAJUSTEFINANCTS (+)
                                          AND AJ.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                                          AND (L.IDTIPOLANCAMENTO IN (1,7) OR L.IDTIPODEBCRED = C.IDTIPODCJUROS OR L.IDTIPODEBCRED = C.IDTIPODCCONTRATO)
                                          AND ((VC.FLGCANCELADO = 'N' AND VC.FLGREVERTIDO = 'N'         AND L.IDMOTIVOESTORNO IS NULL    AND L.IDLANCESTORNO IS NULL)
                                           OR  (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NULL     AND L.IDCANCCONTRATOTS IS NULL)
                                           OR  (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NOT NULL AND L.IDCANCCONTRATOTS IS NULL   AND L.IDAJUSTEFINANCTS IS NOT NULL )
                                           OR  (VC.FLGCANCELADO = 'S' AND L.IDMOTIVOESTORNO IS NULL     AND L.IDCANCCONTRATOTS IS NULL ))
                                          AND L.IDVENDATS IS NOT NULL
                                          AND L.FLGREMOVIDO IS NULL
                                        GROUP BY VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS)) VAL,
                                      (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'D',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS UTILIZACAO
                                         FROM LANCPONTOSTS LP, RESERVASFRONT RF, RESERVAMIGRADATS RM
                                        WHERE NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS = 1)
                                          AND LP.IDRESERVASFRONT  = RF.IDRESERVASFRONT (+)
                                          AND LP.IDRESERVAMIGRADA = RM.IDRESERVAMIGRADA (+)
                                          AND LP.IDTIPOLANCPONTOTS <> 8      
                                        GROUP BY IDVENDAXCONTRATO) U,
                                      (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'C',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS PONTOSCOMPRADOS,
                                              SUM(DECODE(L.IDTIPOLANCAMENTO, 18,L.VLRLANCAMENTO,0)) AS PAGTO 
                                         FROM LANCPONTOSTS LP, LANCAMENTOTS L
                                        WHERE LP.IDLANCPONTOSTS = L.IDLANCPONTOSTS (+)
                                          AND LP.IDTIPOLANCPONTOTS = 8
                                          AND L.IDTIPOLANCAMENTO   = 18
                                          AND (SELECT SUM(VLRLANCAMENTO) AS SALDO FROM LANCAMENTOTS WHERE IDLANCPONTOSTS = LP.IDLANCPONTOSTS GROUP BY IDLANCPONTOSTS) = 0
                                        GROUP BY IDVENDAXCONTRATO) COMPRADOS,
                                      ( SELECT PAG.IDVENDATS, MIN(PAG.DATAPROGRAMADA) AS DATAPROGRAMADA,
                                               ABS(SUM(DECODE(PAG.STATUSCAR, 'QUITADO', PAG.VLRLANCAMENTO, 0))) AS QUITADO,
                                               ABS(SUM(DECODE(PAG.STATUSCAR, 'EM ABERTO', DECODE(PAG.VENCIMENTO, 0, PAG.VLRLANCAMENTO, 0),0))) AS ABERTO_VENCIDO,
                                               ABS(SUM(DECODE(PAG.STATUSCAR, 'EM ABERTO', DECODE(PAG.VENCIMENTO, 1, PAG.VLRLANCAMENTO, 0),0))) AS ABERTO_A_VENCER,
                                               ABS(SUM(DECODE(PAG.STATUSCAR, 'EM ABERTO', DECODE(PAG.VENCIMENTO, 0, 1, 0),0))) AS QUANT_PARC_VENCIDA,
                                               SUM(DECODE(SUBSTR(NVL(PAG.COMPLDOCUMENTO,'E'),1,1), 'P', 0, 1)) AS QUANT_PARC_ENTRADA,
                                               ABS(SUM(DECODE(SUBSTR(NVL(PAG.COMPLDOCUMENTO,'E'),1,1), 'E', PAG.VLRLANCAMENTO, 0))) AS VALOR_ENTRADA,
                                               SUM(DECODE(SUBSTR(NVL(PAG.COMPLDOCUMENTO,'E'),1,1), 'P', 1, 0)) AS QUANT_PARC_FINANC,
                                               ABS(SUM(DECODE(SUBSTR(NVL(PAG.COMPLDOCUMENTO,'E'),1,1), 'P', PAG.VLRLANCAMENTO, 0))) AS VALOR_FINANC,
                                               SUM(DECODE(SUBSTR(NVL(PAG.COMPLDOCUMENTO,'E'),1,1), 'P', 0, 1)) + SUM(DECODE(SUBSTR(NVL(PAG.COMPLDOCUMENTO,'E'),1,1), 'P', 1, 0)) AS QTDE_PAGTO
                                               FROM
                                                    (SELECT L.VLRLANCAMENTO, L.IDVENDATS, CAR.DATAPROGRAMADA, SUBSTR(L.COMPLDOCUMENTO,1,1) AS COMPLDOCUMENTO,
                                                            CASE WHEN P.DATASISTEMA > CAR.DATAPROGRAMADA THEN 0 ELSE 1 END VENCIMENTO,
                                                            DECODE(L.CODDOCUMENTO, NULL, DECODE(P.DATASISTEMA, L.DATALANCAMENTO, DECODE(T.CODTIPDOC, NULL, 'QUITADO',
                                                                                     DECODE(L.IDMOTIVOESTORNO, NULL, DECODE(NVL(L.FLGMIGRADO, 'N'), 'N', 'EM ABERTO', 'QUITADO'), 'QUITADO')),'QUITADO'),
                                                                                     DECODE(NVL(CAR.ESTORNADO,'N'),'N', DECODE(NVL(CAR.SALDOCAR, 0), 0,DECODE(NVL(TOTALCANCELAMENTOS,0),0,'QUITADO','QUITADO'),
                                                                                     DECODE(NVL(CAR.NUMFATURA,0),0, 'EM ABERTO','QUITADO')), 'QUITADO')) AS STATUSCAR
                                                        FROM LANCAMENTOTS L, VENDATS V, TIPODEBCREDHOTEL T, PARAMTS P,
                                                             (SELECT CASE WHEN ( SUM(CASE WHEN TOT.OPERACAO = 2 THEN CASE WHEN TOT.ESTORNO IS NULL THEN 0 ELSE 1 END ELSE 0 END ) ) = 0 THEN 'N' ELSE 'S' END AS ESTORNADO,
                                                                     TOT.CODDOCUMENTO, TOT.IDFORCLI, TOT.DATAPROGRAMADA, TOT.NUMFATURA,
                                                                     NVL(SUM(TO_NUMBER(DECODE(TOT.OPERACAO, 4, TO_NUMBER(DECODE(TOT.DEBCRE,'D',TOT.VALOR,TOT.VALOR*-1)) * TOT.CANCELAMENTO, 0))), 0) AS TOTALCANCELAMENTOS,
                                                                     NVL(SUM(1), 0) AS NUMNAOESTORNADOS,
                                                                     NVL(SUM(TO_NUMBER(DECODE(TOT.DEBCRE,'D',TOT.VALOR,TOT.VALOR*-1))), 0) AS SALDOCAR
                                                                FROM (SELECT L.OPERACAO, ESTORNO, D.CODDOCUMENTO, D.IDFORCLI, D.DATAPROGRAMADA, L.DEBCRE, L.VALOR, L.CODALTERADOR, L.NUMLANCTO, D.NUMFATURA,
                                                                             (SELECT TO_NUMBER(DECODE(NVL(SUM(1),0),0,0,1)) FROM TIPOALTERCANCEL TC WHERE TC.CODALTERADOR = L.CODALTERADOR AND TC.IDAGENCIATS = A.IDAGENCIATS) AS CANCELAMENTO
                                                                                FROM DOCUMENTO D, LANCTODOCUM L, LANCAMENTOTS LTS, VENDATS V, ATENDCLIENTETS A
                                                                               WHERE A.IDATENDCLIENTETS = V.IDATENDCLIENTETS
                                                                                 AND LTS.IDVENDATS      = V.IDVENDATS
                                                                                 AND D.CODDOCUMENTO     = L.CODDOCUMENTO
                                                                                 AND LTS.CODDOCUMENTO   = D.CODDOCUMENTO
                                                                                 AND D.RECPAG           = 'R') TOT
                                                               GROUP BY TOT.IDFORCLI, TOT.CODDOCUMENTO, TOT.DATAPROGRAMADA, TOT.NUMFATURA) CAR
                                                       WHERE L.IDVENDATS          = V.IDVENDATS
                                                         AND T.IDTIPODEBCRED      = L.IDTIPODEBCRED
                                                         AND T.IDHOTEL            = L.IDHOTEL
                                                         AND P.IDHOTEL            = L.IDHOTEL
                                                         AND CAR.CODDOCUMENTO (+) = L.CODDOCUMENTO
                                                         AND L.IDTIPOLANCAMENTO   = 2
                                                         AND L.IDLANCESTORNO      IS NULL
                                                         AND L.IDMOTIVOESTORNO    IS NULL
                                                         ) PAG
                                              GROUP BY PAG.IDVENDATS ) PAGTO
                                    WHERE VC.IDCONTRATOTS       = C.IDCONTRATOTS
                                      AND VC.IDAGENCIATS        = AG.IDPESSOA
                                      AND ((VC.PREVENDA = 'N') OR (VC.PREVENDA IS NULL))
                                      AND VC.IDPROJETOTS        = PJ.IDPROJETOTS
                                      AND VC.IDVENDATS          = V.IDVENDATS
                                      AND VC.IDVENDAXCONTRATO   = CC.IDVENDAXCONTRATO(+)
                                      AND VC.IDVENDAXCONTRATO   = RC.IDVENDAXCONTRNOVO(+)
                                      AND VC.IDVENDATS          = PAGTO.IDVENDATS(+)
                                      AND VC.IDVENDAXCONTRATO   = U.IDVENDAXCONTRATO (+)
                                      AND VC.IDVENDAXCONTRATO   = COMPRADOS.IDVENDAXCONTRATO (+)
                                      AND VC.IDVENDAXCONTRATO   = VAL.IDVENDAXCONTRATO (+)
                                      AND A.IDCLIENTE           = P.IDPESSOA
                                      AND A.IDATENDCLIENTETS    = VC.IDATENDCLIENTETS
                                      AND A.IDHOTEL             = H.IDHOTEL
                                      AND A.IDHOTEL             = PAR.IDHOTEL
                                      AND A.IDLOCALPROSPECAO    = LA.IDLOCAISATEND(+)
                                      AND A.IDPROMAPRESEFET     = PRO.IDPESSOA(+)
                                      AND ((RC.IDREVCONTRATOTS IS NULL AND VAL.IDREVCONTRATOTS IS NULL) OR 
  		                                    (RC.IDREVCONTRATOTS = VAL.IDREVCONTRATOTS))
                                      AND ( (P.IDPESSOA         = EP.IDPESSOA(+) ) 
                                      AND (EP.IDENDERECO = P.IDENDRESIDENCIAL OR
                                            EP.IDENDERECO = P.IDENDCOMERCIAL   OR
                                            EP.IDENDERECO = P.IDENDCOBRANCA    OR
                                            EP.IDENDERECO = P.IDENDCORRESP     OR
                                            EP.IDENDERECO IS NULL
                                            ) )
                                      AND H.IDPESSOA = 3) b Where Lower(b.Status) = 'ativo' ");

            if (pessoasPesquisar != null&& pessoasPesquisar.Any())
            {
                sbSql.AppendLine($" and p.IdPessoa in ({string.Join(",", pessoasPesquisar)}) ");
            }

            var itens = (await _repositoryCm.FindBySql<DadosFinanceirosContrato>(sbSql.ToString())).AsList();

            if (itens != null && itens.Any())
            {
                foreach (var item in itens.Where(a=> a.SaldoInadimplente.GetValueOrDefault(0) > 0).GroupBy(a=> a.IdVendaXContrato))
                {
                    var fst = item.First();
                    var clienteInadimplente = new ClientesInadimplentes
                    {
                        PessoaProviderId = fst.IdCliente,
                        IdVendaXContrato = fst.IdVendaXContrato,
                        FrAtendimentoVendaId = fst.IdVendaXContrato,
                        Nome = fst.Nome,
                        CpfCnpj = long.TryParse(fst.CpfCnpj, out long cpfCnpj) ? cpfCnpj : null,
                        Email = fst.Email,
                        TotalInadimplenciaContrato = item.Sum(a=> a.SaldoInadimplente)
                    };
                    clientesInadimplentes.Add(clienteInadimplente);
                }
            }

            await _cacheStore.AddAsync(CLIENTES_INADIMPLENTES,clientesInadimplentes,DateTimeOffset.Now.AddHours(1), 10, _repositoryCm.CancellationToken);

            return clientesInadimplentes;
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync(DateTime checkInDate, bool simulacao = false)
        {
            try
            {
                if (!simulacao)
                {
                    var sb = new StringBuilder(
                        @$"SELECT
                            r.NumReserva as ReservaId,
                            r.NumReserva as AgendamentoId,
                            r.IdReservasFront,
                            r.NumReserva,
                            r.DATACHEGPREVISTA AS Checkin,
                            r.DATACHEGPREVISTA AS DataCheckin,
                            p.Email as EmailCliente,
                            r.IdHotel as EmpresaId,
                            pcli.IdPessoa AS ClienteId,
                            pcli.Nome AS ClienteNome
                        FROM 
                            ReservasFront r
                            INNER JOIN ReservasTs rt ON r.IdReservasFront = rt.IdReservasFront
                            INNER JOIN MovimentoHospedes mh ON r.idReservasFront = mh.IdReservasFront AND mh.Principal = 'S'
                            INNER JOIN Pessoa p ON mh.IdHospede = p.IdPessoa
                            LEFT JOIN Pessoa pcli ON r.CLIENTERESERVANTE = pcli.IdPessoa
                        WHERE 
                            r.StatusReserva IN (1) 
                            and  (p.Email IS NOT NULL and p.Email like '%@%') 
                            AND NOT exists(SELECT rc.IdReservasFront FROM ReservasRci rc Where rc.IDRESERVASFRONT = mh.IDRESERVASFRONT)
                            and  r.DATACHEGPREVISTA = :dataCheckIn");

                    var parameter = new Parameter("dataCheckIn", checkInDate.Date);
                    return (await _repositoryCm.FindBySql<ReservaInfo>(sb.ToString(), parameter)).AsList();
                }
                else
                {
                    var sb = new StringBuilder(
                        @$"Select b.* From (SELECT
                            r.NumReserva as ReservaId,
                            r.NumReserva as AgendamentoId,
                            r.IdReservasFront,
                            r.NumReserva,
                            r.DATACHEGPREVISTA AS Checkin,
                            r.DATACHEGPREVISTA AS DataCheckin,
                            p.Email as EmailCliente,
                            r.IdHotel as EmpresaId,
                            pcli.IdPessoa AS ClienteId,
                            pcli.Nome AS ClienteNome
                        FROM 
                            ReservasFront r
                            INNER JOIN ReservasTs rt ON r.IdReservasFront = rt.IdReservasFront
                            INNER JOIN MovimentoHospedes mh ON r.idReservasFront = mh.IdReservasFront AND mh.Principal = 'S'
                            INNER JOIN Pessoa p ON mh.IdHospede = p.IdPessoa
                            LEFT JOIN Pessoa pcli ON r.CLIENTERESERVANTE = pcli.IdPessoa
                        WHERE 
                            r.StatusReserva IN (1) 
                            and  (p.Email IS NOT NULL and p.Email like '%@%') 
                            AND NOT exists(SELECT rc.IdReservasFront FROM ReservasRci rc Where rc.IDRESERVASFRONT = mh.IDRESERVASFRONT)
                            and  r.DATACHEGPREVISTA = :dataCheckIn) b Where RowNum <= 50 ");

                    var parameter = new Parameter("dataCheckIn", checkInDate.Date);
                    var resultado = (await _repositoryCm.FindBySql<ReservaInfo>(sb.ToString(), parameter)).AsList();

                    if (!resultado.Any())
                    {
                        var sbRange = new StringBuilder(
                            @$"Select b.* From (SELECT
                            r.NumReserva as ReservaId,
                            r.NumReserva as AgendamentoId,
                            r.IdReservasFront,
                            r.NumReserva,
                            r.DATACHEGPREVISTA AS Checkin,
                            r.DATACHEGPREVISTA AS DataCheckin,
                            p.Email as EmailCliente,
                            r.IdHotel as EmpresaId,
                            pcli.IdPessoa AS ClienteId,
                            pcli.Nome AS ClienteNome
                        FROM 
                            ReservasFront r
                            INNER JOIN ReservasTs rt ON r.IdReservasFront = rt.IdReservasFront
                            INNER JOIN MovimentoHospedes mh ON r.idReservasFront = mh.IdReservasFront AND mh.Principal = 'S'
                            INNER JOIN Pessoa p ON mh.IdHospede = p.IdPessoa
                            LEFT JOIN Pessoa pcli ON r.CLIENTERESERVANTE = pcli.IdPessoa
                        WHERE 
                            r.StatusReserva IN (1) 
                            and  (p.Email IS NOT NULL and p.Email like '%@%') 
                            AND NOT exists(SELECT rc.IdReservasFront FROM ReservasRci rc Where rc.IDRESERVASFRONT = mh.IDRESERVASFRONT)
                            and  r.DATACHEGPREVISTA between :dataCheckInInicial and :dataCheckinFinal) b Where RowNum <= 50 ");

                        var dataInicial = checkInDate.Date.AddDays(-15);
                        var dataFinal = checkInDate.Date.AddDays(15);

                        var parameterInicial = new Parameter("dataCheckInInicial", dataInicial);
                        var parameterFinal = new Parameter("dataCheckinFinal", dataFinal);

                        resultado = (await _repositoryCm.FindBySql<ReservaInfo>(sbRange.ToString(), parameterInicial, parameterFinal)).AsList();
                    }

                    return resultado;
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing(SearchContratosTimeSharingModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetMeusContratosTimeSharing(SearchMeusContratosTimeSharingModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos(SearchReservaTsModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral(SearchReservasGeralModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci(SearchReservasRciModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetMinhasReservasGeralComConsumoPontos(SearchMinhasReservaTsModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral(SearchMinhasReservasGeralModel searchModel)
        {
            throw new NotImplementedException();
        }

        public bool? ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            //To-do implementar validão inadimplência e status CRC
            return true;
        }
    }
}
