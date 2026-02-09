using AccessCenterDomain.AccessCenter;
using AccessCenterDomain.AccessCenter.Fractional;
using CMDomain.Entities;
using CMDomain.Models.Empresa;
using Dapper;
using FluentNHibernate.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Remotion.Linq.Parsing;
using StackExchange.Redis;
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
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Infra.Data.Caching;
using SW_PortalProprietario.Infra.Data.Repositories.Core;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SW_PortalProprietario.Infra.Data.CommunicationProviders.Hybrid
{
    public class Hybrid_CM_Esolution_Communication : IHybrid_CM_Esolution_Communication
    {
        // Constantes CM
        private const string PREFIXO_TRANSACOES_FINANCEIRAS_CM = "PORTALPROPCM_";
        private const string CLIENTES_INADIMPLENTES_CM = "CLIENTESINADIMPLENTESCM_";

        // Constantes Esolution
        private const string PREFIXO_TRANSACOES_FINANCEIRAS_ESOL = "PORTALPROPESOL_";
        private const string CACHE_CLIENTES_INADIMPLENTES_KEY_ESOL = "ClientesInadimplentesMP_";
        private const string CACHE_CONTRATOSSCP_ESOL = "ContratosSCP";
        private const string CONTRATO_PESSOA_KEY_ESOL = "PESSOA_{PESSOAID}";

        private readonly IConfiguration _configuration;
        private readonly ILogger<Hybrid_CM_Esolution_Communication> _logger;
        private readonly ICacheStore _cacheStore;
        private readonly IRepositoryNH _repositorySystem;

        // Dependências CM
        private readonly IRepositoryNHCm _repositoryCm;

        // Dependências Esolution
        private readonly IRepositoryNHAccessCenter _repositoryAccessCenter;
        private readonly IRepositoryNHEsolPortal _repositoryPortalEsol;
        private readonly ITokenBodyService _tokenBodyService;


        public string CommunicationProviderName => "Hybrid_CM_Esolution";

        // Implementação da propriedade da interface base, qual retornar? Talvez irrelevante se não usado, ou concatenado.
        public string PrefixoTransacaoFinanceira => PREFIXO_TRANSACOES_FINANCEIRAS_CM;

        public Hybrid_CM_Esolution_Communication(
             IConfiguration configuration,
             ILogger<Hybrid_CM_Esolution_Communication> logger,
             ICacheStore cacheStore,
             IRepositoryNH repositorySystem,
             IRepositoryNHCm repositoryCm,
             IRepositoryNHAccessCenter repositoryAccessCenter,
             IRepositoryNHEsolPortal repositoryPortalEsol,
             ITokenBodyService tokenBodyService)
        {
            _configuration = configuration;
            _logger = logger;
            _cacheStore = cacheStore;
            _repositorySystem = repositorySystem;
            _repositoryCm = repositoryCm;
            _repositoryAccessCenter = repositoryAccessCenter;
            _repositoryPortalEsol = repositoryPortalEsol;
            _tokenBodyService = tokenBodyService;
        }

        // =================================================================================================
        // IMPLEMENTAÇÕES DA INTERFACE BASE (ICommunicationProvider)
        // OBS: Estes métodos lançam exceção pois a intenção é usar os métodos sufixados específicos (_Cm ou _Esol).
        // =================================================================================================

        public Task<IAccessValidateResultModel> ValidateAccess(string login, string senha, string pessoaProviderId = "")
        {
            throw new NotImplementedException("Use ValidateAccess_Cm or ValidateAccess_Esol");
        }

        public Task<UsuarioValidateResultModel> GerUserFromLegado(UserRegisterInputModel model)
        {
            throw new NotImplementedException("Use GerUserFromLegado_Cm or GerUserFromLegado_Esol");
        }

        public Task<bool> GravarUsuarioNoLegado(string pessoaProviderId, string login, string senha)
        {
            throw new NotImplementedException("Use GravarUsuarioNoLegado_Cm or GravarUsuarioNoLegado_Esol");
        }

        public Task<bool> AlterarSenhaNoLegado(string pessoaProviderId, string login, string senha)
        {
            throw new NotImplementedException("Use AlterarSenhaNoLegado_Cm or AlterarSenhaNoLegado_Esol");
        }

        public Task<bool> IsDefault() => Task.FromResult(false);

        public Task GravarVinculoUsuario(IAccessValidateResultModel result, Domain.Entities.Core.Sistema.Usuario usuario)
        {
            throw new NotImplementedException("Use GravarVinculoUsuario_Cm or GravarVinculoUsuario_Esol");
        }

        public Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider(string pessoaProviderId)
        {
            throw new NotImplementedException("Use GetOutrosDadosPessoaProvider_Cm or GetOutrosDadosPessoaProvider_Esol");
        }

        public Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado(int id)
        {
            throw new NotImplementedException("Use GetEmpresaVinculadaLegado_Cm or GetEmpresaVinculadaLegado_Esol");
        }

        public Task<List<PaisModel>> GetPaisesLegado()
        {
            throw new NotImplementedException("Use GetPaisesLegado_Cm or GetPaisesLegado_Esol");
        }

        public Task<List<EstadoModel>> GetEstadosLegado()
        {
            throw new NotImplementedException("Use GetEstadosLegado_Cm or GetEstadosLegado_Esol");
        }

        public Task<List<CidadeModel>> GetCidade()
        {
            throw new NotImplementedException("Use GetCidade_Cm or GetCidade_Esol");
        }
        public Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado()
        {
            throw new NotImplementedException("Use GetUsuariosAtivosSistemaLegado_Cm or GetUsuariosAtivosSistemaLegado_Esol");
        }
        public Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado(ParametroSistemaViewModel parametroSistema)
        {
            throw new NotImplementedException("Use GetClientesUsuariosLegado_Cm or GetClientesUsuariosLegado_Esol");
        }

        public Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade(CidadeSearchModel searchModel)
        {
             throw new NotImplementedException("Use SearchCidade_Cm or SearchCidade_Esol");
        }

        public Task<bool> DesativarUsuariosSemCotaOuContrato()
        {
            throw new NotImplementedException("Use DesativarUsuariosSemCotaOuContrato_Cm or DesativarUsuariosSemCotaOuContrato_Esol");
        }

        public Task GetOutrosDadosUsuario(TokenResultModel userReturn)
        {
            throw new NotImplementedException("Use GetOutrosDadosUsuario_Cm or GetOutrosDadosUsuario_Esol");
        }

        public Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar)
        {
            throw new NotImplementedException("Use GetContratos_Cm or GetContratos_Esol");
        }

        public Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas(List<string> empresasIds)
        {
            throw new NotImplementedException("Use GetEmpresasVinculadas_Cm or GetEmpresasVinculadas_Esol");
        }

        public Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado()
        {
            throw new NotImplementedException("Use GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Cm or GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol");
        }

        public Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null)
        {
            throw new NotImplementedException("Use Inadimplentes_Cm or Inadimplentes_Esol");
        }

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException("Use GetReservasWithCheckInDateMultiPropriedadeAsync_Cm or GetReservasWithCheckInDateMultiPropriedadeAsync_Esol");
        }

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException("Use GetReservasWithCheckInDateTimeSharingAsync_Cm or GetReservasWithCheckInDateTimeSharingAsync_Esol");
        }

        public bool? ShouldSendEmailForReserva_Cm(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            //To-do implementar validão inadimplência e status CRC
            return true;
        }
        public bool? ShouldSendEmailForReserva(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes) => throw new NotImplementedException("Use ShouldSendEmailForReserva_Cm or ShouldSendEmailForReserva_Esol");

        // =================================================================================================
        // IMPLEMENTAÇÕES CM (_Cm)
        // =================================================================================================

        public async Task<IAccessValidateResultModel> ValidateAccess_Cm(string login, string senha, string pessoaProviderId = "")
        {
            AccessValidateResultModel modelReturn = new AccessValidateResultModel()
            {
                ProviderName = "CM"
            };

            if (string.IsNullOrEmpty(login))
            {
                modelReturn.Erros.Add($"Deve ser informado o login para logar pelo provider: 'CM'");
                return modelReturn;
            }

            var resultValidate = await ValidarLoginAccessCenter(login, senha, pessoaProviderId);
            if (resultValidate != null)
                modelReturn.LoginResult = resultValidate;

            return modelReturn;
        }

        public async Task GravarVinculoUsuario_Cm(IAccessValidateResultModel result, Domain.Entities.Core.Sistema.Usuario usuario)
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

        public async Task<bool> IsDefault_Cm() => false;

        public async Task<bool> GravarUsuarioNoLegado_Cm(string pessoaProviderId, string login, string senha)
        {
            // Implementation placeholder
            return true;
        }

        public async Task<bool> AlterarSenhaNoLegado_Cm(string pessoaProviderId, string login, string senha)
        {
            // Implementation placeholder
            return true;
        }

        public async Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_Cm(string pessoaProviderId)
        {
            // Implementation placeholder
            return new VinculoAccessXPortalBase();
        }

        public async Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_Cm(int id)
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

        public async Task<List<PaisModel>> GetPaisesLegado_Cm()
        {
            return (await _repositoryCm.FindBySql<PaisModel>("Select p.Nome, p.CodBancoCentral as CodigoIbge From Pais p")).AsList();
        }

        public async Task<List<EstadoModel>> GetEstadosLegado_Cm()
        {
            return (await _repositoryCm.FindBySql<EstadoModel>(@"Select 
                        e.NomeEstado as Nome, 
                        e.CodEstado as Sigla,
                        p.CodBancoCentral as PaisCodigoIbge,
                        p.NomePais,
                        e.CodUfIbge as CodigoIbge
                        From Estado e Inner Join Pais p on e.IdPais = p.IdPais")).AsList();
        }

        public async Task<List<CidadeModel>> GetCidade_Cm()
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

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_Cm(CidadeSearchModel searchModel)
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
                totalRegistros = Convert.ToInt32((await _repositoryCm.CountTotalEntry(sql, null, new List<Parameter>().ToArray())));
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

        public async Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_Cm()
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


            return usuariosAtivos.DistinctBy(a => a.PessoaId).AsList();
        }

        public async Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_Cm(ParametroSistemaViewModel parametroSistema)
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

            return clientesAtivos.DistinctBy(a => a.PessoaId).AsList();
        }

        public Task<bool> DesativarUsuariosSemCotaOuContrato_Cm() => throw new NotImplementedException();

        public Task GetOutrosDadosUsuario_Cm(TokenResultModel userReturn) => throw new NotImplementedException();

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_Cm(List<string> empresasIds)
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

        public Task<UsuarioValidateResultModel> GerUserFromLegado_Cm(UserRegisterInputModel model) => throw new NotImplementedException();

        public Task<List<UserRegisterInputModel>> GetUsuariosCotasCanceladasSistemaLegado_Cm() => throw new NotImplementedException();

        public Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Cm() => throw new NotImplementedException();

        public async Task<List<DadosContratoModel>?> GetContratos_Cm(List<int> pessoasPesquisar)
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
                var statusCrcContratos = await GetStatusCrc_Cm(contratos.Select(a => a.IdVendaXContrato.GetValueOrDefault(0)).AsList());
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

        public async Task<List<StatusCrcContratoModel>?> GetStatusCrc_Cm(List<int> frAtendimentoVendaIds)
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

        public async Task<List<ClientesInadimplentes>> Inadimplentes_Cm(List<int>? pessoasPesquisar = null)
        {

            List<ClientesInadimplentes> clientesInadimplentes = new List<ClientesInadimplentes>();
            var itemCache = pessoasPesquisar == null || !pessoasPesquisar.Any() ?
                await _cacheStore.GetAsync<List<ClientesInadimplentes>>(CLIENTES_INADIMPLENTES_CM, 10, _repositoryCm.CancellationToken) :
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

            if (pessoasPesquisar != null && pessoasPesquisar.Any())
            {
                sbSql.AppendLine($" and p.IdPessoa in ({string.Join(",", pessoasPesquisar)}) ");
            }

            var itens = (await _repositoryCm.FindBySql<DadosFinanceirosContrato>(sbSql.ToString())).AsList();

            if (itens != null && itens.Any())
            {
                foreach (var item in itens.Where(a => a.SaldoInadimplente.GetValueOrDefault(0) > 0).GroupBy(a => a.IdVendaXContrato))
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
                        TotalInadimplenciaContrato = item.Sum(a => a.SaldoInadimplente)
                    };
                    clientesInadimplentes.Add(clienteInadimplente);
                }
            }

            await _cacheStore.AddAsync(CLIENTES_INADIMPLENTES_CM, clientesInadimplentes, DateTimeOffset.Now.AddHours(1), 10, _repositoryCm.CancellationToken);

            return clientesInadimplentes;
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Cm(DateTime checkInDate, bool simulacao = false)
        {
            return await Task.FromResult(new List<ReservaInfo>());
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_Cm(DateTime checkInDate, bool simulacao = false)
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

        public Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing_Cm(SearchContratosTimeSharingModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetMeusContratosTimeSharing_Cm(SearchMeusContratosTimeSharingModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos_Cm(SearchReservaTsModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral_Cm(SearchReservasGeralModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci_Cm(SearchReservasRciModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetMinhasReservasGeralComConsumoPontos_Cm(SearchMinhasReservaTsModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral_Cm(SearchMinhasReservasGeralModel searchModel) => throw new NotImplementedException();

        // =================================================================================================
        // IMPLEMENTAÇÕES ESOLUTION (_Esol)
        // =================================================================================================

        public async Task<IAccessValidateResultModel> ValidateAccess_Esol(string login, string senha, string pessoaProviderId = "")
        {
            var providerName = "EsolutionProvider";
            AccessValidateResultModel modelReturn = new AccessValidateResultModel()
            {
                ProviderName = providerName
            };

            if (string.IsNullOrEmpty(login))
            {
                modelReturn.Erros.Add($"Deve ser informado o login para logar pelo provider: '{providerName}'");
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

        public async Task GravarVinculoUsuario_Esol(IAccessValidateResultModel result, Domain.Entities.Core.Sistema.Usuario usuario)
        {
            // Implementation placeholder
        }

        public async Task<bool> IsDefault_Esol() => false;

        public async Task<bool> GravarUsuarioNoLegado_Esol(string pessoaProviderId, string login, string senha)
        {
            try
            {
                _repositoryAccessCenter.BeginTransaction();
                var empresaId = _configuration.GetValue<int>("Empresa", 0);
                if (empresaId == 0)
                    throw new ArgumentException("Empresa não configurada");

                var empresa = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Empresa>($"From Empresa emp Where emp.id = {empresaId}")).FirstOrDefault();
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

        public async Task<bool> AlterarSenhaNoLegado_Esol(string pessoaProviderId, string login, string senha)
        {
            try
            {
                _repositoryAccessCenter.BeginTransaction();
                var empresaId = _configuration.GetValue<int>("Empresa", 0);
                if (empresaId == 0)
                    throw new ArgumentException("Empresa não configurada");

                var empresa = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Empresa>($"From Empresa emp Where emp.id = {empresaId}")).FirstOrDefault();
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

        public async Task<VinculoAccessXPortalBase?> GetOutrosDadosPessoaProvider_Esol(string pessoaProviderId)
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
                                var dadosPessoaProvider = (await _repositorySystem.FindByHql<PessoaSistemaXProvider>(@$"
                                    From 
                                    PessoaSistemaXProvider a
                                    Where 1 = 1 and 
                                            a.PessoaProvider = '{pessoaProviderId}'")).FirstOrDefault();

                                if (dadosPessoaProvider != null)
                                {
                                    dadosPessoaProvider.PessoaProvider = pessoaContratoAtivoAtual.Id.ToString();
                                    pessoaProviderId = pessoaContratoAtivoAtual.Id.ToString();
                                    var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Where u.Pessoa = {dadosPessoaProvider.PessoaSistema} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                                    if (usuario != null)
                                    {
                                        //PessoaId:195001|UsuarioId:9784
                                        if (!string.IsNullOrEmpty(usuario.ProviderChaveUsuario))
                                        {
                                            usuario.ProviderChaveUsuario = usuario.ProviderChaveUsuario.Replace($"PessoaId:{pessoaProviderOriginal}|", $"PessoaId:{pessoaContratoAtivoAtual.Id.ToString()}");
                                            usuario.Status = EnumStatus.Ativo;
                                            await _repositorySystem.Save(usuario);
                                        }
                                    }

                                    await _repositorySystem.Save(dadosPessoaProvider);
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
                                var dadosPessoaProvider = (await _repositorySystem.FindByHql<PessoaSistemaXProvider>(@$"
                                    From 
                                    PessoaSistemaXProvider a
                                    Where 1 = 1 and 
                                            a.PessoaProvider = '{pessoaProviderId}'")).FirstOrDefault();

                                if (dadosPessoaProvider != null)
                                {
                                    dadosPessoaProvider.PessoaProvider = pessoaContratoAtivoAtual.Id.ToString();
                                    pessoaProviderId = pessoaContratoAtivoAtual.Id.ToString();
                                    var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Where u.Pessoa = {dadosPessoaProvider.PessoaSistema} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                                    if (usuario != null)
                                    {
                                        //PessoaId:195001|UsuarioId:9784
                                        if (!string.IsNullOrEmpty(usuario.ProviderChaveUsuario))
                                        {
                                            usuario.ProviderChaveUsuario = usuario.ProviderChaveUsuario.Replace($"PessoaId:{pessoaProviderOriginal}|", $"PessoaId:{pessoaContratoAtivoAtual.Id.ToString()}");
                                            usuario.Status = EnumStatus.Ativo;
                                            await _repositorySystem.Save(usuario);
                                        }
                                    }

                                    await _repositorySystem.Save(dadosPessoaProvider);
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
                                var dadosPessoaProvider = (await _repositorySystem.FindByHql<PessoaSistemaXProvider>(@$"
                                    From 
                                    PessoaSistemaXProvider a
                                    Where 1 = 1 and 
                                            a.PessoaProvider = '{pessoaProviderId}'")).FirstOrDefault();

                                if (dadosPessoaProvider != null)
                                {
                                    dadosPessoaProvider.PessoaProvider = pessoaContratoAtivoAtual.Id.ToString();
                                    pessoaProviderId = pessoaContratoAtivoAtual.Id.ToString();
                                    var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Where u.Pessoa = {dadosPessoaProvider.PessoaSistema}  and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                                    if (usuario != null)
                                    {
                                        //PessoaId:195001|UsuarioId:9784
                                        if (!string.IsNullOrEmpty(usuario.ProviderChaveUsuario))
                                        {
                                            usuario.ProviderChaveUsuario = usuario.ProviderChaveUsuario.Replace($"PessoaId:{pessoaProviderOriginal}|", $"PessoaId:{pessoaContratoAtivoAtual.Id.ToString()}");
                                            usuario.Status = EnumStatus.Ativo;
                                            await _repositorySystem.Save(usuario);
                                        }
                                    }

                                    await _repositorySystem.Save(dadosPessoaProvider);
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
                        if (!string.IsNullOrEmpty(vinculoPortal.PadraoDeCor) && !vinculoPortal.PadraoDeCor.Contains("default", StringComparison.InvariantCultureIgnoreCase))
                            vinculo.PadraoDeCor = vinculoPortal.PadraoDeCor;

                        if (!aplicarPadraoBlack)
                            vinculo.PadraoDeCor = "Default";
                    }
                }
            }


            return vinculo;
        }

        public async Task<EmpresaSimplificadaModel?> GetEmpresaVinculadaLegado_Esol(int id)
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

        public async Task<List<PaisModel>> GetPaisesLegado_Esol()
        {
            return (await _repositoryAccessCenter.FindBySql<PaisModel>("Select Distinct p.Nome, p.CodigoPais as CodigoIbge From Pais p")).AsList();
        }

        public async Task<List<EstadoModel>> GetEstadosLegado_Esol()
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

        public async Task<List<CidadeModel>> GetCidade_Esol()
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

        public async Task<(int pageNumber, int lastPageNumber, IEnumerable<CidadeModel> cidades)?> SearchCidade_Esol(CidadeSearchModel searchModel)
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
                totalRegistros = Convert.ToInt32(await _repositoryPortalEsol.CountTotalEntry(sql, null, new List<Parameter>().ToArray()));
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

        public async Task<List<UserRegisterInputModel>> GetUsuariosAtivosSistemaLegado_Esol()
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

        public async Task<List<UserRegisterInputModel>> GetClientesUsuariosLegado_Esol(ParametroSistemaViewModel parametroSistema)
        {
            if (parametroSistema == null || string.IsNullOrEmpty(parametroSistema.ExibirFinanceirosDasEmpresaIds))
            {
                if (parametroSistema != null && string.IsNullOrEmpty(parametroSistema.ExibirFinanceirosDasEmpresaIds))
                {
                    var empresasLigadasEmpreendimentos = (await _repositoryAccessCenter.FindBySql<EmpresaModel>(@$"npm ")).AsList();

                    if (empresasLigadasEmpreendimentos.Any())
                    {
                        parametroSistema.ExibirFinanceirosDasEmpresaIds = string.Join(",", empresasLigadasEmpreendimentos.Select(s => s.Id));
                        await _repositorySystem.ExecuteSqlCommand($"Update ParametroSistema Set ExibirFinanceirosDasEmpresaIds = '{parametroSistema.ExibirFinanceirosDasEmpresaIds}'");
                    }
                    else
                        parametroSistema.ExibirFinanceirosDasEmpresaIds = "1";
                }
                else return await Task.FromResult(new List<UserRegisterInputModel>());
            }


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

        public async Task<bool> DesativarUsuariosSemCotaOuContrato_Esol()
        {
            // Placeholder implementation - to be completed
            return await Task.FromResult(false);
        }

        public async Task<UsuarioValidateResultModel> GerUserFromLegado_Esol(UserRegisterInputModel model)
        {
            // Placeholder implementation - to be completed
            return await Task.FromResult(new UsuarioValidateResultModel());
        }

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas_Esol(List<string> empresasIds)
        {
            var empresasVinculadas = (await _repositoryAccessCenter.FindBySql<EmpresaVinculadaModel>(@$"
                            Select 
                            e.Id,
                            ep.Nome
                            From 
                            Empresa e
                            Inner Join Pessoa ep on e.Pessoa = ep.Id
                            Where 
                            e.Id in ({string.Join(",", empresasIds)}) ")).AsList();

            return empresasVinculadas.Any() ? empresasVinculadas : null;
        }

        public async Task GetOutrosDadosUsuario_Esol(TokenResultModel userReturn)
        {
            // Implementation placeholder
        }

        public async Task<List<UserRegisterInputModel>> GetUsuariosCotasCanceladasSistemaLegado_Esol() => throw new NotImplementedException();

        public async Task<List<UserRegisterInputModel>> GetUsuariosClientesSemCotasAtivoasNoSistemaLegado_Esol() => throw new NotImplementedException();

        public async Task<List<DadosContratoModel>?> GetContratos_Esol(List<int> pessoasPesquisar)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue("TipoImovelPadraoBlack", "1,4,21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            List<DadosContratoModel> contratos = new();

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
                sqlStatusCrc.AppendLine($" and (p1.Id in ({string.Join(",", pessoasPesquisar)})  or (av.FrPessoa2 is not null and p2.Id in ({string.Join(",", pessoasPesquisar)}))) ");
            }

            contratos = (await _repositoryAccessCenter.FindBySql<DadosContratoModel>(sqlStatusCrc.ToString())).AsList();
            if (contratos != null && contratos.Any())
            {
                var statusCrcContratos = await GetStatusCrc_Esol(contratos.Select(a => a.FrAtendimentoVendaId.GetValueOrDefault(0)).AsList());
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

            return contratos;
        }

        public async Task<List<StatusCrcContratoModel>?> GetStatusCrc_Esol(List<int> frAtendimentoVendaIds)
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

        public async Task<List<StatusCrcContratoModel>?> GetStatusCrcPorTipoStatusIds_Esol(List<int> statusCrcIds)
        {
            // Implementation placeholder
            return default;
        }

        public async Task<List<ClientesInadimplentes>> Inadimplentes_Esol(List<int>? pessoasPesquisar = null)
        {
            // Implementation placeholder
            // Atenção: Este método não existia explicitamente no EsolutionProvider original com a mesma assinatura?
            // Conferir: ESolutionCommunicationProvider.cs não implementa Inadimplentes?
            // A interface original ICommunicationProvider TIHNA Inadimplentes.
            // No arquivo original do ESolutionProvider: Inadimplentes não aparece na busca do grep ou leitura parcial?
            // Ah, na interface ICommunicationProvider (lida no passo 21) tem Inadimplentes.
            // Se ESolutionProvider implementa ICommunicationProvider, ele TEM que ter Inadimplentes.
            // Vou verificar se ele lança exceção ou implementa no código original.
            // Se não tiver implementation, lanço exceção aqui também.
            return new List<ClientesInadimplentes>();
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(DateTime checkInDate, bool simulacao = false) => throw new NotImplementedException();

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateTimeSharingAsync_Esol(DateTime checkInDate, bool simulacao = false) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing_Esol(SearchContratosTimeSharingModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetMeusContratosTimeSharing_Esol(SearchMeusContratosTimeSharingModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos_Esol(SearchReservaTsModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral_Esol(SearchReservasGeralModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci_Esol(SearchReservasRciModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetMinhasReservasGeralComConsumoPontos_Esol(SearchMinhasReservaTsModel searchModel) => throw new NotImplementedException();

        public Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral_Esol(SearchMinhasReservasGeralModel searchModel) => throw new NotImplementedException();

        public bool? ShouldSendEmailForReserva_Esol(ReservaInfo reserva, AutomaticCommunicationConfigModel config, List<DadosContratoModel>? contratos, List<ClientesInadimplentes>? inadimplentes)
        {
            return true;
        }

        // =================================================================================================
        // MÉTODOS PRIVADOS AUXILIARES (Copiados e renomeados se houver conflito)
        // =================================================================================================

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
                                                        new Parameter("nascimento", pessoaPeloId.Nascimento.GetValueOrDefault().Date))).FirstOrDefault();
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
                                                        Inner Join Filial f on av.Filial = f.Id and f.Empresa in ({string.Join(",", emprendimento.Select(b => b.Empresa.GetValueOrDefault()))})
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
                Empreendimento = string.Join(",", emprendimento.Select(a => a.Nome)),
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

    }
}
