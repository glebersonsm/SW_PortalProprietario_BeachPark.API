using AccessCenterDomain.AccessCenter;
using AccessCenterDomain.AccessCenter.Fractional;
using AccessCenterDomain.AccessCenter.Model;
using Dapper;
using EsolutionPortalDomain.Enums;
using EsolutionPortalDomain.Portal;
using EsolutionPortalDomain.ReservasApiModels;
using EsolutionPortalDomain.ReservasApiModels.Condominio;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using FluentNHibernate.Conventions;
using FluentNHibernate.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Dialect.Schema;
using NHibernate.Util;
using PuppeteerSharp;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using ZXing;
using ZXing.OneD;

namespace SW_PortalProprietario.Application.Services.Providers.Hybrid
{
    public class EmpreendimentoHybridService : IEmpreendimentoHybridProviderService
    {
        private const string CACHE_CLIENTES_INADIMPLENTES_KEY = "ClientesInadimplentesMP_";
        private const string CACHE_CONTRATOSSCP = "ContratosSCP";
        private const string PREFIXO_TRANSACOES_FINANCEIRAS = "PORTALPROPESOL_";
        public string PrefixoTransacaoFinanceira => PREFIXO_TRANSACOES_FINANCEIRAS;

        private readonly ILogger<EmpreendimentoHybridService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRepositoryNHAccessCenter _repositoryNHAccessCenter;
        private readonly IRepositoryNH _repositorySystem;
        private readonly IRepositoryNHEsolPortal _repositoryPortal;
        private readonly IFinanceiroHybridProviderService _financeiroService;
        private readonly IServiceBase _serviceBase;
        private readonly IEmailService _emailService;
        private readonly ICacheStore _cacheStore;

        // CM Dependencies
        private readonly ICommunicationProvider _communicationProvider;
        private readonly IRepositoryNHCm _repositoryCM;

        private string CommunicationProviderName => "EsolutionProvider";
        public EmpreendimentoHybridService(ILogger<EmpreendimentoHybridService> logger,
            IConfiguration configuration,
            IRepositoryNHAccessCenter repositoryNHAccessCenter,
            IRepositoryNH repositorySystem,
            IServiceBase serviceBase,
            IEmailService emailService,
            IRepositoryNHEsolPortal repositoryPortal,
            IFinanceiroHybridProviderService financeiroService,
            ICacheStore cache,
            ICommunicationProvider communicationProvider,
            IRepositoryNHCm repositoryCM)
        {
            _logger = logger;
            _configuration = configuration;
            _repositoryNHAccessCenter = repositoryNHAccessCenter;
            _repositorySystem = repositorySystem;
            _serviceBase = serviceBase;
            _emailService = emailService;
            _repositoryPortal = repositoryPortal;
            _financeiroService = financeiroService;
            _cacheStore = cache;
            _communicationProvider = communicationProvider;
            _repositoryCM = repositoryCM;
        }

        public string ProviderName
        {
            get
            {
                return CommunicationProviderName;
            }
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_Esol(SearchImovelModel searchModel)
        {
            var empreendimentoId = _configuration.GetValue("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado");

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();

            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema!.ExibirFinanceirosDasEmpresaIds) ? $" and e.Id in (Select e1.Id From Empreendimento e1 Inner Join Filial f1 on e1.Filial = f1.Id and f1.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})) " : $" and e.Id in ({empreendimentoId}) ";

            List<Parameter> parameters = new List<Parameter>();

            var sb = new StringBuilder(@$"SELECT
                                            i.Id,
                                            i.Numero AS ImovelNumero,
                                            i.DataHoraCriacao AS DataCriacao,
                                            e.Id AS EmpreendimentoId,
                                            e.Nome AS EmpreendimentoNome,
                                            ib.Codigo AS BlocoCodigo,
                                            ib.Nome AS BlocoNome,
                                            ia.Codigo AS ImovelAndarCodigo,
                                            ia.Nome AS ImovelAndarNome,
                                            ti.Codigo AS TipoImovelCodigo,
                                            ti.Nome AS TipoImovelNome,
                                            COALESCE(est.Vendida, 0) AS QtdeVendida,
                                            COALESCE(est.Disponivel, 0) AS QtdeDisponivel,
                                            COALESCE(est.Bloqueada, 0) AS QtdeBloqueada
                                        FROM
                                            Imovel i
                                            INNER JOIN ImovelBloco ib ON i.ImovelBloco = ib.Id
                                            INNER JOIN ImovelAndar ia ON i.ImovelAndar = ia.Id
                                            INNER JOIN TipoImovel ti ON i.TipoImovel = ti.Id
                                            INNER JOIN Empreendimento e ON i.Empreendimento = e.Id
                                            LEFT JOIN (
                                            SELECT
                                                c.Imovel,
                                                SUM(CASE WHEN c.Status = 'V' THEN 1 ELSE 0 END) AS Vendida,
                                                SUM(CASE WHEN c.Status = 'D' AND c.BLOQUEADO = 'N' THEN 1 ELSE 0 END) AS Disponivel,
                                                SUM(CASE WHEN c.Status = 'D' AND c.BLOQUEADO = 'S' THEN 1 ELSE 0 END) AS Bloqueada
                                            FROM Cota c
                                            GROUP BY c.Imovel
                                            ) est ON est.Imovel = i.Id
                                        WHERE 1 = 1
                                            {filtroEmpresa}");

            if (!string.IsNullOrEmpty(searchModel.NumeroImovel))
                sb.AppendLine($" and i.Numero = '{searchModel.NumeroImovel.TrimEnd()}' ");

            if (!string.IsNullOrEmpty(searchModel.CodigoBloco))
                sb.AppendLine($" and ib.Codigo =  '{searchModel.CodigoBloco.TrimEnd()}' ");

            if (!string.IsNullOrEmpty(searchModel.CodigoBloco))
                sb.AppendLine($" and Lower(ib.Codigo) = '{searchModel.CodigoBloco.ToLower().TrimStart().TrimEnd()}' ");

            var sql = sb.ToString();

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 15;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            var totalRegistros = await _repositoryNHAccessCenter.CountTotalEntry(sql, session: null, parameters.ToArray());

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine(" Order by i.Id ");

            var result = (await _repositoryNHAccessCenter.FindBySql<ImovelSimplificadoModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), result);
                }

            }

            return (1, 1, result);
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_Esol(SearchMyContractsModel searchModel)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            var dadosCache = await _cacheStore.GetAsync<List<ContratoSCPModel>>($"{CACHE_CONTRATOSSCP}");

            var contratosScps = dadosCache ?? (await _repositorySystem.FindBySql<ContratoSCPModel>($@"Select 
                c.Id,
                c.CotaPortalId,
                c.CotaAccessCenterId,
                c.PessoaLegadoId,
                c.UhCondominioId 
                From 
                ContratoVinculoScpEsol c ")).AsList();

            if (dadosCache == null && contratosScps.Any())
            {
                await _cacheStore.AddAsync($"{CACHE_CONTRATOSSCP}", contratosScps, DateTimeOffset.Now.AddMinutes(1));
            }

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();

            var empreendimentoId = _configuration.GetValue("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado");

            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema!.ExibirFinanceirosDasEmpresaIds) ? $" and e.Id in (Select e1.Id From Empreendimento e1 Inner Join Filial f1 on e1.Filial = f1.Id and f1.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})) " : $" and e.Id in ({empreendimentoId}) ";

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 20;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            var pessoasVinculadaSistema = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), CommunicationProviderName);
            if (pessoasVinculadaSistema == null)
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            if (pessoasVinculadaSistema.Any(a=> string.IsNullOrEmpty(a.PessoaProvider) || !Helper.IsNumeric(a.PessoaProvider)))
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            List<int> pessoasPesquiar = pessoasVinculadaSistema.Select(b=> Convert.ToInt32(b.PessoaProvider)).AsList();

            await GetOutrasPessoasVinculadas(pessoasVinculadaSistema.First(), pessoasPesquiar);

            List<Parameter> parameters = new List<Parameter>();

            var sb = new StringBuilder(@$"Select
                                            pes.Id as PessoaProviderId,
                                            i.Numero as ImovelNumero,
                                            C.DataAquisicao,
                                            e.Id as EmpreendimentoId,
                                            'MY MABU' as EmpreendimentoNome,
                                            ib.Codigo as BlocoCodigo,
                                            ib.Nome as BlocoNome,
                                            ia.Codigo as ImovelAndarCodigo,
                                            ib.Nome as ImovelAndarNome,
                                            ti.Codigo as TipoImovelCodigo,
                                            ti.Nome as TipoImovelNome,
                                            c.Id as CotaId,
                                            gc.Codigo as GrupoCotaCodigo,
                                            gc.Nome as GrupoCotaNome,
                                            gctc.Codigo as CodigoFracao,
                                            gctc.Nome as NomeFracao,
                                            cli.Id as ClienteId,
                                            cli.Codigo as ClienteCodigo,
                                            pes.Nome as NomeCliente,
                                            Case when pes.Tipo = 'F' then pes.CPF else pes.CNPJ end as CpfCnpj,
                                            pemp.CNPJ as EmpreendimentoCnpj,
                                            av.Codigo as NumeroContrato,
                                            pes.Email,
                                            tc.Nome as TipoCotaNome,
                                            tc.QuantidadeSemana,
                                            av.IdIntercambiadora,
                                            Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                                            From
                                            Cota c
                                            Inner Join Cliente cli on c.Proprietario = cli.Id
                                            Inner Join Pessoa pes on cli.Pessoa = pes.Id
                                            Inner Join GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                                            Inner Join TipoCota tc on gctc.TipoCota = tc.Id
                                            Inner Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                            Inner Join Imovel i on c.Imovel = i.Id
                                            Inner Join ImovelBloco ib on i.ImovelBloco = ib.Id
                                            Inner Join ImovelAndar ia on i.ImovelAndar = ia.Id
                                            Inner Join TipoImovel ti on i.TipoImovel = ti.Id
                                            Inner Join Empreendimento e on i.Empreendimento = e.Id
                                            Inner Join Filial f on e.Filial = f.Id
                                            Inner Join Empresa emp on f.Empresa = emp.Id
                                            Inner Join Pessoa pemp on emp.Pessoa = pemp.Id
                                            Inner Join FrAtendimentoVenda av on c.Id = av.Cota and av.Status = 'A'
                                            Left join TipoImovel ti on i.TipoImovel = ti.Id
                                            Where 1 = 1
                                            {filtroEmpresa} and pes.Id in ({string.Join(",", pessoasPesquiar)}) ");

            var sql = sb.ToString();

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 15;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            var totalRegistros = await _repositoryNHAccessCenter.CountTotalEntry(sql, session: null, parameters.ToArray());

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine(" Order by c.Id ");

            var result = (await _repositoryNHAccessCenter.FindBySql<ProprietarioSimplificadoModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                    var itensRetornar = (searchModel.NumeroDaPagina.GetValueOrDefault(1), totalPage, result);
                    foreach (var item in result)
                    {
                        item.PossuiContratoSCP = contratosScps.Any(b => b.CotaAccessCenterId == item.CotaId);
                        if (!aplicarPadraoBlack)
                            item.PadraoDeCor = "Default";
                    }

                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), result);
                }

            }

            var retorno = (1, 1, result);

            return retorno;
        }

        //private async Task GetOutrasPessoasVinculadas(PessoaSistemaXProviderModel pessoaVinculadaSistema, List<int> pessoasPesquiar)
        //{
        //    var dadosPessoa = !string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) ?
        //        (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = {pessoaVinculadaSistema.PessoaProvider}")).FirstOrDefault() : null;


        //    var outrasPessoasPesquisar = new List<AccessCenterDomain.AccessCenter.Pessoa>();

        //    if (dadosPessoa != null)
        //    {
        //        if (dadosPessoa.CPF.GetValueOrDefault(0) > 0)
        //        {
        //            outrasPessoasPesquisar = (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.CPF = {dadosPessoa.CPF.GetValueOrDefault()}")).AsList();
        //        }
        //        else if (!string.IsNullOrEmpty(dadosPessoa.eMail))
        //        {
        //            outrasPessoasPesquisar = (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where Lower(p.eMail) = '{dadosPessoa.eMail.ToLower()}'")).AsList();
        //        }
        //    }

        //    List<FrPessoa> todasFrPessoasVinculadas = new List<FrPessoa>();

        //    if (!outrasPessoasPesquisar.Any())
        //    {
        //        var frPessoa = (await _repositoryNHAccessCenter.FindBySql<FrPessoa>($"Select fr.* From FrPessoa fr Where fr.Pessoa = {Convert.ToInt32(pessoaVinculadaSistema.PessoaProvider)}")).FirstOrDefault();
        //        if (frPessoa != null)
        //            todasFrPessoasVinculadas.Add(frPessoa);
        //    }
        //    else 
        //    {
        //        todasFrPessoasVinculadas = (await _repositoryNHAccessCenter.FindBySql<FrPessoa>($"Select fr.* From FrPessoa fr Where fr.Pessoa in ({string.Join(",",outrasPessoasPesquisar.Select(b=> b.Id.GetValueOrDefault()))})")).AsList();

        //    }


        //    if (todasFrPessoasVinculadas != null && todasFrPessoasVinculadas.Any())
        //    {
        //        var contratosAtivosVinculadosPessoa =
        //            (await _repositoryNHAccessCenter.FindBySql<FrAtendimentoVenda>(@$"Select 
        //                                                                                av.* 
        //                                                                              From 
        //                                                                                  FrAtendimentoVenda av 
        //                                                                                  Inner Join FrPessoa fp on av.FrPessoa1 = fp.Id 
        //                                                                              Where 
        //                                                                                  av.Status = 'A' and 
        //                                                                                  fp.Id in ({string.Join(",",todasFrPessoasVinculadas.Select(b=> b.Id.GetValueOrDefault()))}) ")).AsList();

        //        if (contratosAtivosVinculadosPessoa != null && contratosAtivosVinculadosPessoa.Any())
        //        {
        //            var outrasPessoasVinculadas = (await _repositoryNHAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select
        //                                                                                               p.*
        //                                                                                             From 
        //                                                                                               FrAtendimentoVendaContaRec avcr 
        //                                                                                               Inner Join ContaReceber cr on avcr.ContaReceber = cr.Id 
        //                                                                                               Inner Join Cliente cli on cr.Cliente = cli.Id
        //                                                                                               Inner Join Pessoa p on cli.Pessoa = p.Id
        //                                                                                             Where 
        //                                                                                               avcr.FrAtendimentoVenda in ({string.Join(",", contratosAtivosVinculadosPessoa.Select(a => a.Id.GetValueOrDefault()))})")).AsList();

        //            if (outrasPessoasVinculadas != null && outrasPessoasVinculadas.Any())
        //            {
        //                pessoasPesquiar.AddRange(outrasPessoasVinculadas.Select(b => b.Id.GetValueOrDefault()).Distinct().AsList());
        //            }
        //        }
        //    }
        //}

        public async Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_Esol(SearchProprietarioModel searchModel)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            var dadosCache = await _cacheStore.GetAsync<List<ContratoSCPModel>>($"{CACHE_CONTRATOSSCP}");

            var contratosScps = (await _repositorySystem.FindBySql<ContratoSCPModel>($@"Select 
                c.Id,
                c.CotaPortalId,
                c.CotaAccessCenterId,
                c.PessoaLegadoId,
                c.UhCondominioId 
                From 
                ContratoVinculoScpEsol c 
                Order by c.Id desc ")).AsList();

            if (dadosCache == null && contratosScps.Any())
            {
                await _cacheStore.AddAsync($"{CACHE_CONTRATOSSCP}", contratosScps, DateTimeOffset.Now.AddMinutes(1));
            }

            var statusAssinaturaContrato = "";
            if (!string.IsNullOrEmpty(searchModel.StatusAssinaturaContratoSCP) && contratosScps != null && contratosScps.Any())
            {
                var sbitens = Helper.Sublists<int>(contratosScps.Select(a => a.PessoaLegadoId.GetValueOrDefault()), 1000);

                if (searchModel.StatusAssinaturaContratoSCP.StartsWith("S", StringComparison.InvariantCultureIgnoreCase))
                {
                    statusAssinaturaContrato = @$" and ( ";
                    var idx = 0;
                    foreach (var item in sbitens)
                    {
                        if (idx > 0)
                        {
                            statusAssinaturaContrato += $" or pes.Id in ({string.Join(",", item)}) ";
                        }
                        else
                        {
                            statusAssinaturaContrato += $" pes.Id in ({string.Join(",", item)}) ";
                        }
                        idx++;
                    }

                    statusAssinaturaContrato += ") ";
                }
                else if (searchModel.StatusAssinaturaContratoSCP.StartsWith("N", StringComparison.InvariantCultureIgnoreCase))
                {
                    statusAssinaturaContrato = @$" and ( ";
                    var idx = 0;
                    foreach (var item in sbitens)
                    {
                        if (idx > 0)
                        {
                            statusAssinaturaContrato += $" and pes.Id not in ({string.Join(",", item)}) ";
                        }
                        else
                        {
                            statusAssinaturaContrato += $" pes.Id not in ({string.Join(",", item)}) ";
                        }
                        idx++;
                    }

                    statusAssinaturaContrato += ") ";
                }
            }



            var empresaCondominioPortal = _configuration.GetValue<int>("EmpresaCondominioPortal", 15);
            if (empresaCondominioPortal == -1)
                throw new Exception("Deve ser configurado o valor para 'EmpresaCondominioPortal' no arquivo appsettings");

            var empreendimentoId = _configuration.GetValue("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado");

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();

            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema!.ExibirFinanceirosDasEmpresaIds) ? $" and e.Id in (Select e1.Id From Empreendimento e1 Inner Join Filial f1 on e1.Filial = f1.Id and f1.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})) " : $" and e.Id in ( {empreendimentoId} ) ";

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 20;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            List<Parameter> parameters = new List<Parameter>();

            var sb = new StringBuilder(@$"Select
                                            pes.Id as PessoaProviderId,
                                            i.Numero as ImovelNumero,
                                            C.DataAquisicao,
                                            e.Id as EmpreendimentoId,
                                            e.Nome as EmpreendimentoNome,
                                            ib.Codigo as BlocoCodigo,
                                            ib.Nome as BlocoNome,
                                            ia.Codigo as ImovelAndarCodigo,
                                            ib.Nome as ImovelAndarNome,
                                            ti.Codigo as TipoImovelCodigo,
                                            ti.Nome as TipoImovelNome,
                                            c.Id as CotaId,
                                            gc.Codigo as GrupoCotaCodigo,
                                            gc.Nome as GrupoCotaNome,
                                            gctc.Codigo as CodigoFracao,
                                            gctc.Nome as NomeFracao,
                                            cli.Id as ClienteId,
                                            cli.Codigo as ClienteCodigo,
                                            pes.Nome as NomeCliente,
                                            Case when pes.Tipo = 'F' then pes.CPF else pes.CNPJ end as CpfCnpjCliente,
                                            pes.Email,
                                            pemp.CNPJ as EmpreendimentoCnpj,
                                            av.Codigo as NumeroContrato,
                                            tc.Nome as TipoCotaNome,
                                            tc.QuantidadeSemana,
                                            av.IdIntercambiadora,
                                            Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                                            From
                                            Cota c
                                            Inner Join Cliente cli on c.Proprietario = cli.Id
                                            Inner Join Pessoa pes on cli.Pessoa = pes.Id
                                            Inner Join GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                                            Inner JOin TipoCota tc on gctc.TipoCota = tc.Id
                                            Inner Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                            Inner Join Imovel i on c.Imovel = i.Id
                                            Inner Join ImovelBloco ib on i.ImovelBloco = ib.Id
                                            Inner Join ImovelAndar ia on i.ImovelAndar = ia.Id
                                            Inner Join TipoImovel ti on i.TipoImovel = ti.Id
                                            Inner Join Empreendimento e on i.Empreendimento = e.Id
                                            Inner Join Filial f on e.Filial = f.Id
                                            Inner Join Empresa emp on f.Empresa = emp.Id
                                            Inner Join Pessoa pemp on emp.Pessoa = pemp.Id
                                            Inner Join FrAtendimentoVenda av on av.Cota = c.Id and av.Status = 'A'
                                            Where 1 = 1
                                            {filtroEmpresa} {statusAssinaturaContrato} ");

            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(pes.NomePesquisa) like '%{searchModel.Nome.TrimEnd().ToLower().RemoveAccents()}%' ");

            if (!string.IsNullOrEmpty(searchModel.NumeroUnidade))
                sb.AppendLine($" and i.Numero =  '{searchModel.NumeroUnidade.TrimEnd()}' ");

            if (!string.IsNullOrEmpty(searchModel.FracaoCota))
                sb.AppendLine($" and Lower(gctc.Codigo) like  '%{searchModel.FracaoCota.TrimEnd().ToLower()}%' ");

            if (searchModel.PessoaProviderId.GetValueOrDefault(0) > 0)
                sb.AppendLine($" and pes.Id = {searchModel.PessoaProviderId.GetValueOrDefault()} ");

            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                sb.AppendLine($" and Lower(av.Codigo) like '%{searchModel.NumeroContrato.ToLower().TrimEnd()}%'");
            }

            if (searchModel.DataAquisicaoInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                sb.AppendLine($" and c.DataAquisicao >= :dataInicialAquisicao ");
                parameters.Add(new Parameter("dataInicialAquisicao", searchModel.DataAquisicaoInicial.GetValueOrDefault().Date));
            }

            if (searchModel.DataAquisicaoFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                sb.AppendLine($" and c.DataAquisicao <= :dataFinalAquisicao ");
                parameters.Add(new Parameter("dataFinalAquisicao", searchModel.DataAquisicaoFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
            }

            if (searchModel.EmpresaId.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and emp.Id = :empresaId ");
                parameters.Add(new Parameter("empresaId", searchModel.EmpresaId.GetValueOrDefault()));
            }

            if (!string.IsNullOrEmpty(searchModel.DocumentoCliente))
            {
                var apenasNumeros = Helper.ApenasNumeros(searchModel.DocumentoCliente).TrimStart('0');
                if (!string.IsNullOrEmpty(apenasNumeros))
                {
                    if (_repositoryNHAccessCenter.DataBaseType == SW_Utils.Enum.EnumDataBaseType.Oracle)
                    {
                        sb.AppendLine($" and (to_char(pes.CPF) like  '%{apenasNumeros}%' or to_char(pes.CNPJ) like '%{apenasNumeros}%') ");
                    }
                    else
                    {
                        sb.AppendLine($" and (Cast(pes.CPF as varchar) like '%{apenasNumeros}%' or Cast(pes.CNPJ as varchar) like '%{apenasNumeros}%') ");
                    }
                }
                else
                {
                    if (_repositoryNHAccessCenter.DataBaseType == SW_Utils.Enum.EnumDataBaseType.Oracle)
                    {
                        sb.AppendLine($" and (to_char(pes.CPF) like  '%{searchModel.DocumentoCliente}%' or to_char(pes.CNPJ) like '%{searchModel.DocumentoCliente}%') ");
                    }
                    else
                    {
                        sb.AppendLine($" and (Cast(pes.CPF as varchar) like '%{searchModel.DocumentoCliente}%' or Cast(pes.CNPJ as varchar) like '%{searchModel.DocumentoCliente}%') ");
                    }
                }
            }

            var sql = sb.ToString();

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 15;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            var totalRegistros = await _repositoryNHAccessCenter.CountTotalEntry(sql, session: null, parameters.ToArray());

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine(" Order by c.Id ");

            var result = (await _repositoryNHAccessCenter.FindBySql<ProprietarioSimplificadoModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                    foreach (var item in result)
                    {
                        item.PossuiContratoSCP = contratosScps.Any(b => b.CotaAccessCenterId == item.CotaId);
                        if (!aplicarPadraoBlack)
                            item.PadraoDeCor = "Default";
                    }

                    if (!string.IsNullOrEmpty(searchModel.StatusAssinaturaContratoSCP))
                    {
                        if (searchModel.StatusAssinaturaContratoSCP.StartsWith("S", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = result.Where(a => a.PossuiContratoSCP.GetValueOrDefault(false) == true).AsList();
                        }
                        else if (searchModel.StatusAssinaturaContratoSCP.StartsWith("N", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = result.Where(a => a.PossuiContratoSCP.GetValueOrDefault(false) == false).AsList();
                        }
                    }


                    var itensRetornar = (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), result);


                    return itensRetornar;
                }

            }

            var retorno = (1, 1, result);

            return retorno;
        }

        public async Task<bool> GerarCodigoVerificacaoLiberacaoPool_Esol(int agendamentoId)
        {
            throw new ArgumentException();
            //var loggedUser = await _repositorySystem.GetLoggedUser();
            //var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {loggedUser.Value.userId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ")).FirstOrDefault();
            //if (usuario == null || usuario.Pessoa == null)
            //    throw new FileNotFoundException("Não foi possível identificar o usuário logado para envio do código de confirmação para liberação da cota para POOL");

            //var empresa = (await _repositorySystem.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e Where 1 = 1 Order by e.Id desc")).FirstOrDefault();

            //if (empresa == null)
            //    throw new FileNotFoundException("Não foi possível identificar a empresa logada no sistema.");

            //var emailCliente = "";

            //if (string.IsNullOrEmpty(usuario.Pessoa.EmailPreferencial) &&
            //    string.IsNullOrEmpty(usuario.Pessoa.EmailAlternativo))
            //{
            //    var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(usuario.Id, CommunicationProviderName);
            //    if (pessoaProvider != null && !string.IsNullOrEmpty(pessoaProvider.PessoaProvider))
            //    {
            //        var pessoaSistemaLegado = (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = {pessoaProvider.PessoaProvider}")).FirstOrDefault();
            //        if (pessoaSistemaLegado != null && !string.IsNullOrEmpty(pessoaSistemaLegado.eMail) && pessoaSistemaLegado.eMail.Contains("@"))
            //            emailCliente = pessoaSistemaLegado.eMail.Split(';')[0];

            //    }
            //}
            //else
            //{
            //    if (!string.IsNullOrEmpty(usuario.Pessoa.EmailPreferencial))
            //        emailCliente = usuario.Pessoa.EmailPreferencial.Split(';')[0];
            //    else if (!string.IsNullOrEmpty(usuario.Pessoa.EmailAlternativo))
            //        emailCliente = usuario.Pessoa.EmailAlternativo.Split(';')[0];
            //}

            //if (!string.IsNullOrEmpty(emailCliente) && emailCliente.Contains("@"))
            //{
            //    try
            //    {
            //        _repositorySystem.BeginTransaction();

            //        var agendamento = (await _repositoryPortal.FindByHql<PeriodoCotaDisponibilidade>($"From PeriodoCotaDisponibilidade pcd Where pcd.Id = {agendamentoId}")).FirstOrDefault();
            //        if (agendamento == null)
            //            throw new FileNotFoundException($"Não foi foi encontrado o agendamento com o Id: {agendamentoId}");

            //        if (agendamento.DataInicial.GetValueOrDefault().Date <= DateTime.Today)
            //            throw new FileNotFoundException($"A data de Check-in: {agendamento.DataInicial.GetValueOrDefault().Date:dd/MM/yyyy} do agendamento Id: {agendamentoId} não permite liberação para o POOL");

            //        var codigoDeConfirmacao = $"LS{Helper.GenerateRandomCode(6)}";

            //        var emailsPermitidos = _configuration.GetValue<string>("DestinatarioEmailPermitido");
            //        var enviarEmailApenasParaDestinatariosPermitidos = _configuration.GetValue<bool>("EnviarEmailApenasParaDestinatariosPermitidos", true);


            //        if (enviarEmailApenasParaDestinatariosPermitidos)
            //        {
            //            if (string.IsNullOrEmpty(emailsPermitidos) || string.IsNullOrEmpty(emailCliente) ||
            //                                !emailsPermitidos.Contains(emailCliente, StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                emailCliente = "glebersonsm@gmail.com";
            //            }
            //        }

            //        var emailResult = await _emailService.SaveInternal(new EmailInputInternalModel()
            //        {
            //            UsuarioCriacao = usuario.Id,
            //            Assunto = "Código de validação para liberação de semana para o POOL de hospedagem",
            //            EmpresaId = empresa.Id,
            //            Destinatario = emailCliente,
            //            ConteudoEmail = @$"<div>Olá, {usuario.Pessoa?.Nome}!</div>
            //            <div>
            //            Favor utilize o código: <b>{codigoDeConfirmacao}</b> válido até: <b>{DateTime.Now.AddMinutes(10).AddSeconds(-5):dd/MM/yyyy HH:mm:ss}</b>
            //            <div>
            //                Para efetivação da liberação do seu agendamento de semana com Id: <b>{agendamentoId}</b>
            //            </div>
            //            <div>
            //                Ref. ao período de hospedagem com Check-in em: <b>{agendamento.DataInicial:dd/MM/yyyy}</b> e Check-out em: <b>{agendamento.DataFinal.GetValueOrDefault().AddDays(1):dd/MM/yyyy}</b> 
            //                <div>para o POOL de hospedagem!</div>
            //            </div>"
            //        });

            //        var confirmacaoLiberacaoPool = new ConfirmacaoLiberacaoPool()
            //        {
            //            UsuarioCriacao = usuario.Id,
            //            DataHoraCriacao = DateTime.Now,
            //            CodigoEnviadoAoCliente = codigoDeConfirmacao,
            //            Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresa.Id },
            //            Email = emailResult != null && emailResult.Id > 0 ? new Email() { Id = emailResult.Id.GetValueOrDefault() } : null,
            //            AgendamentoId = agendamentoId
            //        };

            //        await _repositorySystem.Save(confirmacaoLiberacaoPool);

            //        var commitResult = await _repositorySystem.CommitAsync();
            //        if (!commitResult.executed)
            //            throw commitResult.exception ?? new Exception("Erro na operação.");
            //    }
            //    catch (Exception err)
            //    {
            //        _repositorySystem.Rollback();
            //        _logger.LogError(err, err.Message);
            //        throw;
            //    }

            //    return true;
            //}
            //else throw new ArgumentException("Não foi possível identificar o email do usuário logado. (Consultamos no cadastro de usuário e também no sistema legado");

        }

        public async Task<bool> ValidarCodigo_Esol(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true)
        {
            try
            {
                if (controlarTransacao.GetValueOrDefault(false) == true)
                    _repositorySystem.BeginTransaction();

                if (string.IsNullOrEmpty(codigoVerificacao))
                    throw new ArgumentException("Deve ser informado o código de verificação");

                var loggedUser = await _repositorySystem.GetLoggedUser();
                var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {loggedUser.Value.userId}and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                if (usuario == null || usuario.Pessoa == null)
                    throw new FileNotFoundException("Não foi possível identificar o usuário logado para envio do código de confirmação para liberação da cota para POOL");

                var empresa = (await _repositorySystem.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e Where 1 = 1 Order by e.Id desc")).FirstOrDefault();

                if (empresa == null)
                    throw new FileNotFoundException("Não foi possível identificar a empresa logada no sistema.");


                var lastCodigoVerificacaoGerado =
                    (await _repositorySystem.FindByHql<ConfirmacaoLiberacaoPool>(@$"From 
                        ConfirmacaoLiberacaoPool clp
                        Inner Join Fetch clp.Empresa emp
                    Where 
                        clp.UsuarioCriacao = {usuario.Id} and 
                        emp.Id = {empresa.Id} and 
                        clp.AgendamentoId = {agendamentoId} and
                        ((clp.LiberacaoConfirmada != {(int)EnumSimNao.Sim} and clp.DataConfirmacao is null) or (clp.DataConfirmacao >= :ultimos10minutos))
                    Order by clp.Id Desc", session: null, new Parameter("ultimos10minutos", DateTime.Now.AddMinutes(-10)))).FirstOrDefault();


                if (lastCodigoVerificacaoGerado == null || string.IsNullOrEmpty(lastCodigoVerificacaoGerado.CodigoEnviadoAoCliente))
                    throw new FileNotFoundException("Não foi possível validar o código informado, favor gere um novo código");

                if (lastCodigoVerificacaoGerado.CodigoEnviadoAoCliente.TrimEnd().TrimStart().Equals(codigoVerificacao.TrimEnd().TrimStart()))
                {
                    if (lastCodigoVerificacaoGerado.LiberacaoConfirmada.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)
                    {
                        lastCodigoVerificacaoGerado.LiberacaoConfirmada = Domain.Enumns.EnumSimNao.Sim;
                        lastCodigoVerificacaoGerado.UsuarioAlteracao = usuario.Id;
                        lastCodigoVerificacaoGerado.DataConfirmacao = DateTime.Now;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(lastCodigoVerificacaoGerado.Tentativas))
                        lastCodigoVerificacaoGerado.Tentativas += "|";

                    lastCodigoVerificacaoGerado.Tentativas += $"{codigoVerificacao} em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                }

                await _repositorySystem.Save(lastCodigoVerificacaoGerado);

                if (controlarTransacao.GetValueOrDefault(false) == true)
                {
                    var resultCommit = await _repositorySystem.CommitAsync();
                    if (!resultCommit.executed)
                        throw resultCommit.exception ?? new Exception("Erro na operação");
                }

                return lastCodigoVerificacaoGerado.LiberacaoConfirmada.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Sim;

            }
            catch (Exception err)
            {
                if (controlarTransacao.GetValueOrDefault(false) == true)
                    _repositorySystem.Rollback();
                _logger.LogError(err, err.Message);
            }

            return false;
        }

        public async Task<ResultModel<int>?> SalvarReservaEmAgendamento_Esol(CriacaoReservaAgendamentoInputModel modelReserva)
        {

            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (!loggedUser.Value.isAdm)
            {
                var usuarioProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId));
                if (usuarioProvider != null)
                {
                    var result = await Inadimplente(usuarioProvider);
                    if (result != null)
                        throw new ArgumentException("Não foi possível realizar a operação: PF");
                }
            }


            var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
            var criarReservaAgendamentoUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:CriarReservaAgendamentoUrl");
            var fullUrl = baseUrl + criarReservaAgendamentoUrl;
            var token = await _serviceBase.getToken();

            if (modelReserva.Hospedes != null && modelReserva.Hospedes.Any())
            {
                foreach (var item in modelReserva.Hospedes.Where(b => !string.IsNullOrEmpty(b.CPF)))
                {
                    if (!Helper.IsCpf(item.CPF!))
                        throw new ArgumentException($"O CPF: '{item.CPF}' não é válido");
                }
            }

            var modelEnvio = (CriacaoReservaAgendamentoModel)modelReserva;

            if (modelEnvio.TipoTarifacao != EsolutionPortalDomain.Enums.EnumTipoTarifacao.DiaDia &&
                modelEnvio.TipoTarifacao != EsolutionPortalDomain.Enums.EnumTipoTarifacao.Tarifario &&
                modelEnvio.TipoTarifacao != EsolutionPortalDomain.Enums.EnumTipoTarifacao.Fixa)
                modelEnvio.TipoTarifacao = EsolutionPortalDomain.Enums.EnumTipoTarifacao.DiaDia;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(fullUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, modelEnvio);

                string resultMessage = await responseResult.Content.ReadAsStringAsync();

                _logger.LogInformation(resultMessage);
                if (responseResult.StatusCode == HttpStatusCode.OK || responseResult.StatusCode == HttpStatusCode.Created)
                {
                    var resultModel = System.Text.Json.JsonSerializer.Deserialize<ResultModel<int>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (resultModel != null)
                    {
                        resultModel.Success = true;
                        resultModel.Status = (int)HttpStatusCode.OK;
                    }
                    return resultModel;
                }
                else
                {
                    var resultModel = System.Text.Json.JsonSerializer.Deserialize<ResultModel<ReservaModel>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    var result = new ResultModel<int>(-1)
                    {
                        Errors = new List<string>() { resultModel.Message },
                        Success = resultModel.Success,
                        Status = resultModel.Status
                    };

                    return result;
                }

            }
        }

        public async Task<ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>?> ConsultarAgendamentosGerais_Esol(ReservasMultiPropriedadeSearchModel model)
        {
            var dadosCache = await _cacheStore.GetAsync<List<ContratoSCPModel>>($"{CACHE_CONTRATOSSCP}");

            if ((model.ApenasInadimplentes == true || model.StatusCrcIds != null && model.StatusCrcIds.Any()) && model.QuantidadeRegistrosRetornar < 100000)
            {
                model.QuantidadeRegistrosRetornar = 100000;
                model.NumeroDaPagina = 1;
            }

            var inadimplentes = model.ApenasInadimplentes.GetValueOrDefault(false) ? await Inadimplentes_Esol() : new();

            var frAtendimentoStatusCrcModels = model.StatusCrcIds != null && model.StatusCrcIds.Any() ? await GetStatusCrcPorTipoStatusIds(model.StatusCrcIds) : new List<StatusCrcContratoModel>();

            List<string>? NumeroApartamentos = new List<string>();
            List<string>? NomesCotas = new List<string>();
            if (model.StatusCrcIds != null && model.StatusCrcIds.Any())
            {
                if (frAtendimentoStatusCrcModels != null && frAtendimentoStatusCrcModels.Any())
                {
                    foreach (var item in frAtendimentoStatusCrcModels.Where(a => !string.IsNullOrEmpty(a.ImovelNumero)).GroupBy(c => c.ImovelNumero))
                    {
                        if (!string.IsNullOrEmpty(item.Key))
                        {
                            if (!NumeroApartamentos.Contains(item.Key!))
                                NumeroApartamentos.Add(item.Key!);
                        }
                    }

                    foreach (var item in frAtendimentoStatusCrcModels.Where(b => !string.IsNullOrEmpty(b.GrupoCotaTipoCotaNome)).GroupBy(c => c.GrupoCotaTipoCotaNome))
                    {
                        if (!string.IsNullOrEmpty(item.Key))
                        {
                            if (!NomesCotas.Contains(item.Key!))
                                NomesCotas.Add(item.Key!);
                        }
                    }
                }
                else return new ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>()
                {
                    Data = new(),
                    Success = true,
                    LastPageNumber = 1,
                    PageNumber = 1
                };
            }

            if (NumeroApartamentos != null && NumeroApartamentos.Any())
                model.NumeroApartamentos = NumeroApartamentos;

            if (NomesCotas != null && NomesCotas.Any())
                model.NomeCotas = NomesCotas;

            var contratosScps = dadosCache ?? (await _repositorySystem.FindBySql<ContratoSCPModel>($@"Select 
                c.Id,
                c.CotaPortalId,
                c.CotaAccessCenterId,
                c.PessoaLegadoId,
                c.UhCondominioId 
                From 
                ContratoVinculoScpEsol c ")).AsList();

            if (dadosCache == null && contratosScps.Any())
            {
                await _cacheStore.AddAsync($"{CACHE_CONTRATOSSCP}", contratosScps, DateTimeOffset.Now.AddMinutes(1));
            }

            _logger.LogInformation($"{DateTime.Now} - Buscando reservas da API");
            var result = new ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>();
            try
            {
                result = await ConsultarDadosApiReserva(model, result);

            }
            catch (HttpRequestException err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.InternalServerError;
                }
            }

            var retorno = FiltrarRetorno(model, inadimplentes, frAtendimentoStatusCrcModels, contratosScps, result);

            return retorno;
        }

        private async Task<ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>?> ConsultarDadosApiReserva(ReservasMultiPropriedadeSearchModel model, ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>? result)
        {
            var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
            var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:ConsultaReservasAgendamentoUrl");
            var fullUrl = $"{baseUrl}{consultarReservaUrl}";//?{model.ToQueryString()}";
            var token = await _serviceBase.getToken();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(fullUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, model);

                string resultMessage = await responseResult.Content.ReadAsStringAsync();

                if (responseResult.IsSuccessStatusCode)
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (result != null)
                        result.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    if (result != null)
                    {
                        result.Status = (int)HttpStatusCode.NotFound;
                        result.Errors = new List<string>() { $"Erro: {responseResult}" };

                    }
                }
            }

            return result;
        }

        private ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>? FiltrarRetorno(ReservasMultiPropriedadeSearchModel model,
            List<ClientesInadimplentes> inadimplentes,
            List<StatusCrcContratoModel>? frAtendimentoStatusCrcModels,
            List<ContratoSCPModel> contratosScps,
            ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>? result)
        {
            if (result != null &&
                            result.Data != null &&
                            result.Data.Any())
            {

                List<int> pessoasInadimplentes = new List<int>();
                if (model.ApenasInadimplentes.GetValueOrDefault(false) || (model.StatusCrcIds != null && model.StatusCrcIds.Any()))
                {

                    IList<Models.Empreendimento.SemanaModel> itensRetorno = result.Data.AsList();
                    if (model.ApenasInadimplentes.GetValueOrDefault(false))
                    {
                        foreach (var item in itensRetorno.Reverse())
                        {
                            if ((!string.IsNullOrEmpty(item.PessoaTitular1CPF) && item.PessoaTitular1CPF.Length > 5) ||
                                (!string.IsNullOrEmpty(item.PessoaTitualar1CNPJ) && item.PessoaTitualar1CNPJ.Length > 5))
                            {
                                var cpfCnpj = Int64.Parse(Helper.ApenasNumeros(item.PessoaTitualar1CNPJ));
                                var ehInadimplente = inadimplentes.FirstOrDefault(b => b.CpfCnpj.HasValue &&
                                cpfCnpj == b.CpfCnpj);

                                if (ehInadimplente != null)
                                {
                                    if (model.NaoConsiderarParcelasCondominio.GetValueOrDefault(false) &&
                                        ehInadimplente.TotalInadimplenciaContrato.GetValueOrDefault(0) == 0)
                                    {
                                        itensRetorno.Remove(item);
                                    }
                                    else if (model.NaoConsiderarParcelasContrato.GetValueOrDefault(false) &&
                                        ehInadimplente.TotalInadimplenciaCondominio.GetValueOrDefault(0) == 0)
                                    {
                                        itensRetorno.Remove(item);
                                    }

                                }
                                else itensRetorno.Remove(item);
                            }
                            else if (!string.IsNullOrEmpty(item.NomeProprietario))
                            {
                                var ehInadimplente = inadimplentes.FirstOrDefault(b => !string.IsNullOrEmpty(b.Nome) &&
                                Helper.RemoveAccents(b.Nome).Contains(Helper.RemoveAccents(item.NomeProprietario), StringComparison.InvariantCultureIgnoreCase));

                                if (ehInadimplente != null)
                                {
                                    if (string.IsNullOrEmpty(item.Detalhes))
                                    {
                                        item.Detalhes = $"Pendências=> Contrato: {ehInadimplente.TotalInadimplenciaContrato.GetValueOrDefault():N2} - Taxas: {ehInadimplente.TotalInadimplenciaCondominio.GetValueOrDefault():N2}";
                                    }
                                    else
                                    {
                                        item.Detalhes += $"|Pendências=> Contrato: {ehInadimplente.TotalInadimplenciaContrato.GetValueOrDefault():N2} - Taxas: {ehInadimplente.TotalInadimplenciaCondominio.GetValueOrDefault():N2}";
                                    }

                                    if (model.NaoConsiderarParcelasCondominio.GetValueOrDefault(false) &&
                                        ehInadimplente.TotalInadimplenciaContrato.GetValueOrDefault(0) == 0)
                                    {
                                        itensRetorno.Remove(item);
                                    }
                                    else if (model.NaoConsiderarParcelasContrato.GetValueOrDefault(false) &&
                                        ehInadimplente.TotalInadimplenciaCondominio.GetValueOrDefault(0) == 0)
                                    {
                                        itensRetorno.Remove(item);
                                    }
                                }
                                else itensRetorno.Remove(item);
                                continue;
                            }
                        }
                    }

                    if (model.StatusCrcIds != null && model.StatusCrcIds.Any())
                    {
                        if (frAtendimentoStatusCrcModels != null && frAtendimentoStatusCrcModels.Any() && itensRetorno.Any())
                        {
                            foreach (var item in itensRetorno.Reverse())
                            {
                                var statusCrcAtual = frAtendimentoStatusCrcModels.FirstOrDefault(b => !string.IsNullOrEmpty(b.NomeTitular) && !string.IsNullOrEmpty(item.NomeProprietario) &&
                                !string.IsNullOrEmpty(b.GrupoCotaTipoCotaNome) && !string.IsNullOrEmpty(item.CotaNome) &&
                                !string.IsNullOrEmpty(b.ImovelNumero) && !string.IsNullOrEmpty(item.UhCondominioNumero) &&
                                b.GrupoCotaTipoCotaNome.Contains(item.CotaNome, StringComparison.CurrentCultureIgnoreCase) &&
                                b.ImovelNumero.Contains(item.UhCondominioNumero) && b.NomeTitular.RemoveAccents().Contains(item.NomeProprietario.RemoveAccents(), StringComparison.OrdinalIgnoreCase));
                                if (statusCrcAtual == null)
                                    itensRetorno.Remove(item);
                                else
                                {
                                    if (string.IsNullOrEmpty(item.Detalhes))
                                    {
                                        item.Detalhes = $"StatusCRC: {statusCrcAtual.CodigoStatus}-{statusCrcAtual.NomeStatus}";
                                    }
                                    else item.Detalhes += $"|StatusCRC: {statusCrcAtual.CodigoStatus}-{statusCrcAtual.NomeStatus}";
                                }
                            }
                        }
                        else
                        {
                            result.Data = new List<Models.Empreendimento.SemanaModel>();
                            return result;
                        }
                    }

                    result.Data = itensRetorno.AsList();
                    if (itensRetorno.Count() == 0) return result;
                }


                if (contratosScps != null && contratosScps.Any())
                {
                    foreach (var item in result.Data)
                    {
                        item.PossuiContratoSCP = contratosScps.Any(b => b.CotaPortalId == item.CotaId && b.UhCondominioId == item.UhCondominioId);
                    }
                }
            }

            return result;
        }

        public async Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos_Esol(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue("TipoImovelPadraoBlack", "1,4,21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);
            var dadosCache = await _cacheStore.GetAsync<List<ContratoSCPModel>>($"{CACHE_CONTRATOSSCP}");

            var contratosScps = dadosCache ?? (await _repositorySystem.FindBySql<ContratoSCPModel>($@"Select 
                c.Id,
                c.CotaPortalId,
                c.CotaAccessCenterId,
                c.PessoaLegadoId,
                c.UhCondominioId 
                From 
                ContratoVinculoScpEsol c ")).AsList();

            if (dadosCache == null && contratosScps.Any())
            {
                await _cacheStore.AddAsync($"{CACHE_CONTRATOSSCP}", contratosScps, DateTimeOffset.Now.AddMinutes(1));
            }

            CotaModel? cota = null;
            if (model.CotaAcId.GetValueOrDefault(0) > 0)
            {
                cota = (await _repositoryNHAccessCenter.FindBySql<CotaModel>(@$"Select 
                            c.Id as CotaId, 
                            gctc.Nome as GrupoCotaTipoCotaNome, 
                            i.Numero as ImovelNumero,
                            av.IdIntercambiadora,
                            c.Status,
                            Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                            From 
                            Cota c 
                            Inner Join Imovel i on c.Imovel = i.Id
                            Inner Join GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                            Left Join FrAtendimentoVenda av on c.Id = av.Cota and av.Status = 'A'
                            Left Join TipoImovel ti on i.TipoImovel = ti.Id
                            Where c.Id = {model.CotaAcId}")).FirstOrDefault();

                if (cota != null)
                {
                    model.ImovelNumero = cota.ImovelNumero;
                    model.CotaNome = cota.GrupoCotaTipoCotaNome;
                    if (!aplicarPadraoBlack)
                        cota.PadraoDeCor = "Default";
                }
                else return new ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>();
            }

            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Erro: Não foi possível identificar o usuário logado.");

            List<DadosContratoModel> contratosDoCliente = new List<DadosContratoModel>();

            if (!string.IsNullOrEmpty(model.AgendamentoId))
            {
                var periodoCotaDisponibilidade = (await _repositoryPortal.FindBySql<PeriodoCotaDisponibilidade>($"Select pcd.* From PeriodoCotaDisponibilidade pcd Where pcd.Id = {model.AgendamentoId}")).FirstOrDefault();
                if (periodoCotaDisponibilidade == null)
                    throw new ArgumentException($"Não foi encontrado o agendamento com o Id informado: {model.AgendamentoId}");

                model.Ano = $"{periodoCotaDisponibilidade.DataInicial.GetValueOrDefault().Year}";
            }

            if (!loggedUser.Value.isAdm)
            {

                var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId));
                if (pessoaProvider == null)
                    throw new ArgumentException("Erro: Não foi possível identificar o usuário logado.");

                var outrasPessoas = await GetOutrasPessoasVinculadas(pessoaProvider.First(), pessoaProvider.Where(c=> !string.IsNullOrEmpty(c.PessoaProvider)).Select(b=> int.Parse(b.PessoaProvider)).AsList());

                contratosDoCliente = await _serviceBase.GetContratos(outrasPessoas != null && outrasPessoas.Any() ? outrasPessoas : new List<int>() { int.Parse(pessoaProvider.First().PessoaProvider!) }) ?? new List<DadosContratoModel>();
                if (contratosDoCliente != null && contratosDoCliente.Any(b => !b.DataCancelamento.HasValue && b.Status == "A" && b.CotaStatus == "V" && b.Cota == model.CotaAcId))
                {
                    var fstNaoCancelada = contratosDoCliente.FirstOrDefault(b => !b.DataCancelamento.HasValue && b.Status == "A" && b.CotaStatus == "V" && b.Cota == model.CotaAcId);
                    if (fstNaoCancelada != null)
                    {
                        model.CotaNome = fstNaoCancelada.GrupoCotaTipoCotaNome;
                        model.CotaAcId = fstNaoCancelada.Cota.GetValueOrDefault();
                        model.ImovelNumero = fstNaoCancelada.NumeroImovel;
                        model.DataAquisicaoContrato = fstNaoCancelada.DataVenda.GetValueOrDefault();
                        model.IdIntercambiadora = fstNaoCancelada.IdIntercambiadora;
                        if (!string.IsNullOrEmpty(fstNaoCancelada.PadraoDeCor))
                            model.PadraoDeCor = fstNaoCancelada.PadraoDeCor;

                        if (!aplicarPadraoBlack)
                            model.PadraoDeCor = "Default";
                    }
                    else
                    {
                        return new ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>()
                        {
                            Data = new List<Models.Empreendimento.SemanaModel>() { new Models.Empreendimento.SemanaModel() { CotaNome = "Nenhuma cota encontrada" } },
                            Errors = new List<string>() { "Nenhuma cota foi encontrada" },
                            LastPageNumber = 1,
                            PageNumber = 1,
                            Message = "Nenhuma cota foi encontrada"
                        };
                    }
                }
                else
                {
                    return new ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>()
                    {
                        Data = new List<Models.Empreendimento.SemanaModel>() { new Models.Empreendimento.SemanaModel() { CotaNome = "Nenhuma cota encontrada" } },
                        Errors = new List<string>() { "Nenhuma cota foi encontrada" },
                        LastPageNumber = 1,
                        PageNumber = 1,
                        Message = "Nenhuma cota foi encontrada"
                    };
                }

            }

            if (string.IsNullOrEmpty(model.Ano))
                model.Ano = $"{DateTime.Today.Year:yyyy}";


            _logger.LogInformation($"{DateTime.Now} - Buscando reservas da API");
            var result = new ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>();
            try
            {

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:ConsultaMinhasReservasAgendamentoUrl");
                var fullUrl = $"{baseUrl}{consultarReservaUrl}?{model.ToQueryString()}";

                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.GetAsync(fullUrl);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    if (responseResult.IsSuccessStatusCode)
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<ResultWithPaginationModel<List<Models.Empreendimento.SemanaModel>>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (result?.Data != null && result.Data.Any())
                        {
                            foreach (var item in result.Data.Cast<Models.Empreendimento.SemanaModel>().GroupBy(b => b.CotaId))
                            {
                                var fstCota = item.First();
                                var contratoVinculado =
                                    contratosDoCliente.FirstOrDefault(a => !string.IsNullOrEmpty(a.GrupoCotaTipoCotaNome) &&
                                    !string.IsNullOrEmpty(fstCota.CotaNome) &&
                                    !string.IsNullOrEmpty(fstCota.CotaNome) &&
                                    fstCota.CotaNome.Contains(a.GrupoCotaTipoCotaNome, StringComparison.InvariantCultureIgnoreCase));

                                if (contratoVinculado != null)
                                {
                                    foreach (var itemSemana in item)
                                    {
                                        itemSemana.IdIntercambiadora = contratoVinculado.IdIntercambiadora;
                                        itemSemana.PessoaTitular1Tipo = contratoVinculado.PessoaTitular1Tipo;
                                        itemSemana.PessoaTitular1CPF = contratoVinculado.PessoaTitular1CPF;
                                        itemSemana.PessoaTitualar1CNPJ = contratoVinculado.PessoaTitualar1CNPJ;
                                        if (!string.IsNullOrEmpty(contratoVinculado.PadraoDeCor))
                                            itemSemana.PadraoDeCor = contratoVinculado.PadraoDeCor;

                                        if (!aplicarPadraoBlack)
                                            itemSemana.PadraoDeCor = "Default";
                                    }
                                }
                            }
                        }
                        if (result == null) return default;

                        result!.Status = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        result.Status = (int)HttpStatusCode.NotFound;
                        result.Errors = new List<string>() { $"Erro: {responseResult}" };

                    }
                }

            }
            catch (HttpRequestException err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.InternalServerError;
                }
            }

            if (result != null && result.Data != null && result.Data.Any() && contratosScps != null && contratosScps.Any())
            {
                foreach (var item in result.Data)
                {
                    item.PossuiContratoSCP = contratosScps.Any(b => b.CotaPortalId == item.CotaId && b.UhCondominioId == item.UhCondominioId);
                    if (!aplicarPadraoBlack)
                        item.PadraoDeCor = "Default";
                }
            }

            return result;
        }


        private async Task<List<int>> GetOutrasPessoasVinculadas(PessoaSistemaXProviderModel pessoaVinculadaSistema, List<int> pessoasPesquiar)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            var dadosPessoa = !string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) ?
                (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = {pessoaVinculadaSistema.PessoaProvider}")).FirstOrDefault() : null;


            var outrasPessoasPesquisar = new List<AccessCenterDomain.AccessCenter.Pessoa>();

            if (dadosPessoa != null)
            {
                if (dadosPessoa.CPF.GetValueOrDefault(0) > 0)
                {
                    outrasPessoasPesquisar = (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.CPF = {dadosPessoa.CPF.GetValueOrDefault()}")).AsList();
                }
                else if (!string.IsNullOrEmpty(dadosPessoa.eMail))
                {
                    outrasPessoasPesquisar = (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where Lower(p.eMail) = '{dadosPessoa.eMail.ToLower()}'")).AsList();
                }
            }

            List<FrPessoa> todasFrPessoasVinculadas = new List<FrPessoa>();

            if (!outrasPessoasPesquisar.Any())
            {
                var frPessoa = (await _repositoryNHAccessCenter.FindBySql<FrPessoa>($"Select fr.* From FrPessoa fr Where fr.Pessoa = {Convert.ToInt32(pessoaVinculadaSistema.PessoaProvider)}")).FirstOrDefault();
                if (frPessoa != null)
                    todasFrPessoasVinculadas.Add(frPessoa);
            }
            else
            {
                todasFrPessoasVinculadas = (await _repositoryNHAccessCenter.FindBySql<FrPessoa>($"Select fr.* From FrPessoa fr Where fr.Pessoa in ({string.Join(",", outrasPessoasPesquisar.Select(b => b.Id.GetValueOrDefault()))})")).AsList();

            }


            if (todasFrPessoasVinculadas != null && todasFrPessoasVinculadas.Any())
            {
                var contratosAtivosVinculadosPessoa =
                    (await _repositoryNHAccessCenter.FindBySql<FrAtendimentoVenda>(@$"Select 
                                                                                av.*,
                                                                                Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                                                                              From 
                                                                                  FrAtendimentoVenda av 
                                                                                  Inner Join FrPessoa fp on av.FrPessoa1 = fp.Id 
                                                                                  Left Join Cota c on av.Cota = c.Id
                                                                                  Left Join Imovel i on c.Imovel = i.Id
                                                                                  Left Join TipoImovel ti on i.TipoImovel = ti.Id
                                                                              Where 
                                                                                  av.Status = 'A' and 
                                                                                  fp.Id in ({string.Join(",", todasFrPessoasVinculadas.Select(b => b.Id.GetValueOrDefault()))}) ")).AsList();

                if (contratosAtivosVinculadosPessoa != null && contratosAtivosVinculadosPessoa.Any())
                {
                    var outrasPessoasVinculadas = (await _repositoryNHAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select
                                                                                               p.*
                                                                                             From 
                                                                                               FrAtendimentoVendaContaRec avcr 
                                                                                               Inner Join ContaReceber cr on avcr.ContaReceber = cr.Id 
                                                                                               Inner Join Cliente cli on cr.Cliente = cli.Id
                                                                                               Inner Join Pessoa p on cli.Pessoa = p.Id
                                                                                             Where 
                                                                                               avcr.FrAtendimentoVenda in ({string.Join(",", contratosAtivosVinculadosPessoa.Select(a => a.Id.GetValueOrDefault()))})")).AsList();

                    foreach (var item in contratosAtivosVinculadosPessoa)
                    {
                        if (!aplicarPadraoBlack)
                            item.PadraoDeCor = "Default";
                    }

                    if (outrasPessoasVinculadas != null && outrasPessoasVinculadas.Any())
                    {
                        pessoasPesquiar.AddRange(outrasPessoasVinculadas.Select(b => b.Id.GetValueOrDefault()).Distinct().AsList());
                    }
                }
            }

            return pessoasPesquiar;
        }

        public async Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId_Esol(string agendamento)
        {
            if (string.IsNullOrEmpty(agendamento))
                throw new ArgumentException("O agendamentoId deve ser informado.");

            _logger.LogInformation($"{DateTime.Now} - Buscando reservas da API");
            var result = new ResultModel<List<ReservaModel>>();
            try
            {

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:ConsultaReservaByAgendamentoIdUrl");
                if (!string.IsNullOrEmpty(consultarReservaUrl))
                {
                    var fullUrl = $"{baseUrl}{consultarReservaUrl}{agendamento}";
                    var token = await _serviceBase.getToken();

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(fullUrl);
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("accept", "application/json");
                        client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                        HttpResponseMessage responseResult = await client.GetAsync(fullUrl);

                        string resultMessage = await responseResult.Content.ReadAsStringAsync();

                        if (responseResult.IsSuccessStatusCode)
                        {
                            result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<List<ReservaModel>>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                            if (result != null)
                                result.Status = (int)HttpStatusCode.OK;
                        }
                        else
                        {
                            result.Status = (int)HttpStatusCode.NotFound;
                            result.Errors = new List<string>() { $"Erro: {responseResult}" };

                        }
                    }
                }
                else throw new ArgumentException($"Não foi encontrada a configuração de url: 'ConsultaReservaByAgendamentoIdUrl'");

            }
            catch (HttpRequestException err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.InternalServerError;
                }
            }

            return result;
        }

        public async Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId_Esol(string agendamento)
        {
            if (string.IsNullOrEmpty(agendamento))
                throw new ArgumentException("O agendamentoId deve ser informado.");

            _logger.LogInformation($"{DateTime.Now} - Buscando reservas da API");
            var result = new ResultModel<List<ReservaModel>>();

            try
            {

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:ConsultaMinhasReservaByAgendamentoIdUrl");
                var fullUrl = $"{baseUrl}{consultarReservaUrl}{agendamento}";
                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.GetAsync(fullUrl);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    if (responseResult.IsSuccessStatusCode)
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<List<ReservaModel>>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (result != null)
                            result.Status = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        result.Status = (int)HttpStatusCode.NotFound;
                        result.Errors = new List<string>() { $"Erro: {responseResult}" };

                    }
                }

            }
            catch (HttpRequestException err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.InternalServerError;
                }
            }

            return result;
        }

        public async Task<ResultModel<bool>?> CancelarReservaAgendamento_Esol(CancelamentoReservaAgendamentoModel model)
        {
            var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
            var criarReservaAgendamentoUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:CancelarReservaAgendamentoUrl");
            var fullUrl = baseUrl + criarReservaAgendamentoUrl;
            var token = await _serviceBase.getToken();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(fullUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, model);

                string resultMessage = await responseResult.Content.ReadAsStringAsync();

                _logger.LogInformation(resultMessage);
                var resultModel = System.Text.Json.JsonSerializer.Deserialize<ResultModel<bool>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                return resultModel;
            }
        }

        public async Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento_Esol(CancelamentoReservaAgendamentoModel model)
        {
            var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
            var criarReservaAgendamentoUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:CancelarMinhaReservaAgendamentoUrl");
            var fullUrl = baseUrl + criarReservaAgendamentoUrl;
            var token = await _serviceBase.getToken();


            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(fullUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, model);

                string resultMessage = await responseResult.Content.ReadAsStringAsync();

                _logger.LogInformation(resultMessage);
                var resultModel = System.Text.Json.JsonSerializer.Deserialize<ResultModel<bool>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                return resultModel;
            }
        }

        public async Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva_Esol(int id)
        {
            if (id == 0)
                throw new ArgumentException("O id da reserva deve ser informado.");

            _logger.LogInformation($"{DateTime.Now} - Buscando reserva da API");
            var result = new ResultModel<ReservaForEditModel>();

            try
            {

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:EditarMinhaReservaAgendamentoUrl");
                var fullUrl = $"{baseUrl}{consultarReservaUrl}{id}";
                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.GetAsync(fullUrl);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    if (responseResult.IsSuccessStatusCode)
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<ReservaForEditModel>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (result != null)
                        {
                            result.Status = (int)HttpStatusCode.OK;
                            result.Success = true;
                        }

                    }
                    else
                    {
                        result.Status = (int)HttpStatusCode.NotFound;
                        result.Errors = new List<string>() { $"Erro: {responseResult}" };
                    }
                }

            }
            catch (HttpRequestException err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.InternalServerError;
                }
            }

            return result;
        }

        public async Task<ResultModel<ReservaForEditModel>?> EditarReserva_Esol(int id)
        {
            if (id == 0)
                throw new ArgumentException("O id da reserva deve ser informado.");

            _logger.LogInformation($"{DateTime.Now} - Buscando reserva da API");
            var result = new ResultModel<ReservaForEditModel>();

            try
            {

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:EditarReservaAgendamentoUrl");
                var fullUrl = $"{baseUrl}{consultarReservaUrl}{id}";
                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.GetAsync(fullUrl);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    if (responseResult.IsSuccessStatusCode)
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<ReservaForEditModel>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (result != null)
                            result.Status = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        result.Status = (int)HttpStatusCode.NotFound;
                        result.Errors = new List<string>() { $"Erro: {responseResult}" };

                    }
                }

            }
            catch (HttpRequestException err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.InternalServerError;
                }
            }

            return result;
        }

        public async Task<Models.ResultModel<List<InventarioModel>>?> ConsultarInventarios_Esol(InventarioSearchModel searchModel)
        {
            if (string.IsNullOrEmpty(searchModel.NoPool))
                searchModel.NoPool = "Todos";

            if (searchModel.Agendamentoid.GetValueOrDefault(0) <= 0)
                throw new ArgumentException("Deve ser informado o AgendamentoId");

            _logger.LogInformation($"{DateTime.Now} - Buscando inventários na API");
            var result = new Models.ResultModel<List<InventarioModel>>();

            try
            {

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:ConsultarInventarios");
                var fullUrl = $"{baseUrl}{consultarReservaUrl}?{searchModel.ToQueryString()}";
                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.GetAsync(fullUrl);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    if (responseResult.IsSuccessStatusCode)
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<Models.ResultModel<List<InventarioModel>>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (result != null)
                            result.Status = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        result.Status = (int)HttpStatusCode.NotFound;
                        result.Errors = new List<string>() { $"Erro: {responseResult}" };

                    }
                }

            }
            catch (HttpRequestException err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.InternalServerError;
                    result.Message = err.Message;
                }
            }

            return result;
        }

        public async Task<Models.ResultModel<bool>?> RetirarSemanaPool_Esol(AgendamentoInventarioModel modelAgendamentoPool)
        {
            var result = new Models.ResultModel<bool>();

            try
            {
                _repositorySystem.BeginTransaction();

                var loggedUser = await _repositorySystem.GetLoggedUser();
                if (string.IsNullOrEmpty(loggedUser.Value.userId))
                    throw new FileNotFoundException("Não foi possível identificar o usuário logado.");

                var empresa = (await _repositorySystem.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e Where 1 = 1 Order by e.Id desc")).FirstOrDefault();

                if (empresa == null)
                    throw new FileNotFoundException("Não foi possível identificar a empresa logada no sistema.");

                if (modelAgendamentoPool.AgendamentoId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o AgendamentoId");

                if (modelAgendamentoPool.InventarioId.GetValueOrDefault(0) == 0)
                {

                    Models.ResultModel<List<InventarioModel>>? inventariosVinculadosAoAgendamentoNoPool = await ConsultarInventarios_Esol(new InventarioSearchModel()
                    {
                        Agendamentoid = modelAgendamentoPool.AgendamentoId,
                        NoPool = "N"
                    });

                    if (inventariosVinculadosAoAgendamentoNoPool == null ||
                        !inventariosVinculadosAoAgendamentoNoPool.Success ||
                        inventariosVinculadosAoAgendamentoNoPool.Data == null ||
                        !inventariosVinculadosAoAgendamentoNoPool.Data.Any() ||
                        inventariosVinculadosAoAgendamentoNoPool.Data.Count() > 1)
                        throw new FileNotFoundException($"Não foi possível identificar o inventário/Pool compatível para utilização no agendamento Id: {modelAgendamentoPool.AgendamentoId.GetValueOrDefault()}");

                    modelAgendamentoPool.InventarioId = inventariosVinculadosAoAgendamentoNoPool.Data.First().Id;

                    InventarioModel? inventarioUtilizar = null;

                    if (modelAgendamentoPool.InventarioId.GetValueOrDefault(0) > 0)
                    {
                        inventarioUtilizar = inventariosVinculadosAoAgendamentoNoPool.Data.First(a => a.Id == modelAgendamentoPool.InventarioId);
                        if (inventarioUtilizar == null)
                            throw new ArgumentException($"Não foi possível encontrar o inventário Pool para utilização no agendamento Id: {modelAgendamentoPool.InventarioId.GetValueOrDefault()}");

                        var agendamento = (await _repositoryPortal.FindByHql<PeriodoCotaDisponibilidade>($"From PeriodoCotaDisponibilidade Where Id = {modelAgendamentoPool.AgendamentoId.GetValueOrDefault()}")).FirstOrDefault();

                        if (agendamento != null && !loggedUser.Value.isAdm && inventarioUtilizar?.DiasMinimoInicioAgendamentoRemover.GetValueOrDefault(0) > -1)
                        {
                            if (agendamento.DataInicial.GetValueOrDefault().Date.Subtract(DateTime.Today).Days <
                                inventarioUtilizar.DiasMinimoInicioAgendamentoRemover.GetValueOrDefault())
                                throw new ArgumentException($"Só é possível remover período do POOL com no mímino: {inventarioUtilizar.DiasMinimoInicioAgendamentoRemover.GetValueOrDefault()} dias de antecedência.");
                        }
                    }

                    if (modelAgendamentoPool.InventarioId.GetValueOrDefault(0) == 0)
                        throw new ArgumentException("Deve ser informado o InventárioId");
                }

                if (!loggedUser.Value.isAdm)
                {
                    var usuarioProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId));
                    if (usuarioProvider != null)
                    {
                        var resultInadimplente = await Inadimplente(usuarioProvider);
                        if (resultInadimplente != null)
                            throw new ArgumentException("Não foi possível realizar a operação: PF");
                    }
                }

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var retirarSemanaPoolUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:RetirarSemanaPool");
                var fullUrl = baseUrl + retirarSemanaPoolUrl;
                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, modelAgendamentoPool);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    _logger.LogInformation(resultMessage);

                    if (responseResult.IsSuccessStatusCode)
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<Models.ResultModel<bool>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (result != null)
                        {
                            result.Status = (int)HttpStatusCode.OK;
                            result.Success = true;
                        }
                    }
                    else
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<Models.ResultModel<bool>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (result != null)
                        {
                            result.Status = (int)HttpStatusCode.NotFound;
                            result.Success = false;
                        }
                    }

                }

                if (modelAgendamentoPool.AcaoInterna.GetValueOrDefault(false) == false)
                {

                    var historicoRetiraPool = new HistoricoRetiradaPool()
                    {
                        UsuarioCriacao = Convert.ToInt32(loggedUser.Value.userId),
                        DataHoraCriacao = DateTime.Now,
                        Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresa.Id },
                        AgendamentoId = modelAgendamentoPool.AgendamentoId
                    };

                    await _repositorySystem.Save(historicoRetiraPool);
                }

                var resultCommit = await _repositorySystem.CommitAsync();
            }
            catch (Exception err)
            {
                _repositorySystem.Rollback();
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Message = err.Message;
                    result.Errors = new List<string>() { err.Message };
                }
            }

            return result;
        }

        public async Task<ResultModel<bool>?> LiberarSemanaPool_Esol(LiberacaoAgendamentoInputModel modelAgendamentoPool)
        {

            throw new NotImplementedException();
            //var result = new ResultModel<bool>();

            //try
            //{
            //    _repositorySystem.BeginTransaction();

            //    var loggedUser = await _repositorySystem.GetLoggedUser();
            //    if (string.IsNullOrEmpty(loggedUser.Value.userId))
            //        throw new FileNotFoundException("Não foi possível identificar o usuário logado.");

            //    var empresa = (await _repositorySystem.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e Where 1 = 1 Order by e.Id desc")).FirstOrDefault();

            //    if (empresa == null)
            //        throw new FileNotFoundException("Não foi possível identificar a empresa logada no sistema.");

            //    if (modelAgendamentoPool.AgendamentoId.GetValueOrDefault(0) == 0)
            //        throw new ArgumentException("Deve ser informado o AgendamentoId");

            //    if (modelAgendamentoPool.InventarioId.GetValueOrDefault(0) == 0)
            //        throw new ArgumentException("Deve ser informado o InventárioId");

            //    var agendamento = (await _repositoryPortal.FindByHql<PeriodoCotaDisponibilidade>($"From PeriodoCotaDisponibilidade Where Id = {modelAgendamentoPool.AgendamentoId.GetValueOrDefault()}")).FirstOrDefault();
            //    if (agendamento == null)
            //        throw new FileNotFoundException($"Não foi foi encontrado o agendamento com o Id: {modelAgendamentoPool.AgendamentoId}");

            //    if (agendamento.DataInicial.GetValueOrDefault().Date <= DateTime.Today)
            //        throw new FileNotFoundException($"A data de Check-in: {agendamento.DataInicial.GetValueOrDefault().Date:dd/MM/yyyy} do agendamento Id: {modelAgendamentoPool.AgendamentoId} não permite liberação para o POOL");


            //    if (!loggedUser.Value.isAdm)
            //    {
            //        var usuarioProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId));
            //        if (usuarioProvider != null)
            //        {
            //            if (agendamento != null)
            //            {
            //                var cotaAtual = await GetCotaAccessCenterPelosDadosPortal(new GetHtmlValuesModel() { PeriodoCotaDisponibilidadeId = agendamento.Id, UhCondominioId = agendamento.UhCondominio, CotaOrContratoId = agendamento.Cota });
            //                if (cotaAtual != null)
            //                {
            //                    if (!string.IsNullOrEmpty(usuarioProvider.PessoaProvider))
            //                    {
            //                        var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(usuarioProvider.PessoaProvider!) });
            //                        if (propCache != null && propCache.Any(b => b.frAtendimentoStatusCrcModels.Any(b => cotaAtual.FrAtendimentoVenda.GetValueOrDefault() == b.FrAtendimentoVendaId.GetValueOrDefault() &&
            //                        (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && b.AtendimentoStatusCrcStatus == "A")))
            //                        {
            //                            throw new ArgumentException("Não foi possível localizar as disponibilidades, motivo 0001BL");
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }


            //    var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
            //    var liberarPoolUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:LiberarSemanaPool");
            //    var fullUrl = baseUrl + liberarPoolUrl;
            //    var token = await _serviceBase.getToken();

            //    using (HttpClient client = new HttpClient())
            //    {
            //        client.BaseAddress = new Uri(fullUrl);
            //        client.DefaultRequestHeaders.Clear();
            //        client.DefaultRequestHeaders.Add("accept", "application/json");
            //        client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
            //        HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, modelAgendamentoPool);

            //        string resultMessage = await responseResult.Content.ReadAsStringAsync();

            //        _logger.LogInformation(resultMessage);

            //        if (responseResult.IsSuccessStatusCode)
            //        {
            //            result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<bool>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            //            if (result != null)
            //                result.Status = (int)HttpStatusCode.OK;
            //        }
            //        else
            //        {
            //            result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<bool>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            //            if (result != null)
            //            {
            //                result.Status = (int)HttpStatusCode.NotFound;
            //                result.Success = false;
            //            }
            //        }

            //    }

            //    //Ajustar formato conta, conta digito, agencia e agencia digito
            //    if (!string.IsNullOrEmpty(modelAgendamentoPool.ContaNumero))
            //    {
            //        modelAgendamentoPool.ContaNumero = modelAgendamentoPool.ContaNumero.TrimEnd().TrimStart();
            //        if (modelAgendamentoPool.ContaNumero.Contains(" "))
            //        {
            //            var conta = modelAgendamentoPool.ContaNumero.Split(" ")[0];
            //            var contaDigito = "0";
            //            if (modelAgendamentoPool.ContaNumero.Split(" ").Length > 1)
            //            {
            //                contaDigito = modelAgendamentoPool.ContaNumero.Split(" ")[1];
            //            }
            //            modelAgendamentoPool.ContaNumero = conta;
            //            modelAgendamentoPool.ContaDigito = contaDigito;
            //        }
            //        else if (modelAgendamentoPool.ContaNumero.Contains("-"))
            //        {
            //            var conta = modelAgendamentoPool.ContaNumero.Split("-")[0];
            //            var contaDigito = "0";
            //            if (modelAgendamentoPool.ContaNumero.Split("-").Length > 1)
            //            {
            //                contaDigito = modelAgendamentoPool.ContaNumero.Split("-")[1];
            //            }
            //            modelAgendamentoPool.ContaNumero = conta;
            //            modelAgendamentoPool.ContaDigito = contaDigito;
            //        }
            //    }

            //    if (!string.IsNullOrEmpty(modelAgendamentoPool.Agencia))
            //    {
            //        modelAgendamentoPool.Agencia = modelAgendamentoPool.Agencia.TrimEnd().TrimStart();
            //        if (modelAgendamentoPool.Agencia.Contains(" "))
            //        {
            //            var agencia = modelAgendamentoPool.Agencia.Split(" ")[0];
            //            var agenciaDigito = "0";
            //            if (modelAgendamentoPool.Agencia.Split(" ").Length > 1)
            //            {
            //                agenciaDigito = modelAgendamentoPool.Agencia.Split(" ")[1];
            //            }
            //            modelAgendamentoPool.Agencia = agencia;
            //            modelAgendamentoPool.AgenciaDigito = agenciaDigito;
            //        }
            //        else if (modelAgendamentoPool.Agencia.Contains("-"))
            //        {
            //            var agencia = modelAgendamentoPool.Agencia.Split("-")[0];
            //            var agenciaDigito = "0";
            //            if (modelAgendamentoPool.Agencia.Split("-").Length > 1)
            //            {
            //                agenciaDigito = modelAgendamentoPool.Agencia.Split("-")[1];
            //            }
            //            modelAgendamentoPool.Agencia = agencia;
            //            modelAgendamentoPool.AgenciaDigito = agenciaDigito;
            //        }
            //    }


            //    var confirmacaoLiberacaoPool = new ConfirmacaoLiberacaoPool()
            //    {
            //        UsuarioCriacao = Convert.ToInt32(loggedUser.Value.userId),
            //        DataHoraCriacao = DateTime.Now,
            //        Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresa.Id },
            //        AgendamentoId = modelAgendamentoPool.AgendamentoId,
            //        LiberacaoConfirmada = Domain.Enumns.EnumSimNao.Sim,
            //        LiberacaoDiretaPeloCliente = Domain.Enumns.EnumSimNao.Não,
            //        Banco = modelAgendamentoPool.CodigoBanco,
            //        Conta = modelAgendamentoPool.ContaNumero,
            //        ContaDigito = modelAgendamentoPool.ContaDigito,
            //        Agencia = modelAgendamentoPool.Agencia,
            //        AgenciaDigito = modelAgendamentoPool.AgenciaDigito,
            //        ChavePix = modelAgendamentoPool.ChavePix,
            //        Tipo = modelAgendamentoPool.Variacao,
            //        Variacao = modelAgendamentoPool.Variacao,
            //        TipoConta = modelAgendamentoPool.Variacao,
            //        Preferencial = modelAgendamentoPool.Preferencial.GetValueOrDefault(false) ? "S" : "N",
            //        TipoChavePix = modelAgendamentoPool.TipoChavePix,
            //        IdCidade = $"{modelAgendamentoPool.IdCidade}"
            //    };

            //    await _repositorySystem.Save(confirmacaoLiberacaoPool);

            //    var resultCommit = await _repositorySystem.CommitAsync();

            //}
            //catch (Exception err)
            //{
            //    _repositorySystem.Rollback();
            //    _logger.LogError(err, err.Message);
            //    if (result != null)
            //    {
            //        result.Message = err.Message;
            //        result.Errors = new List<string>() { err.Message };
            //        result.Success = false;
            //    }
            //}
            //return result;

        }

        public async Task<ResultModel<bool>?> LiberarMinhaSemanaPool_Esol(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
        {
            throw new NotImplementedException();
            //bool liberacaoEfetuada = false;

            //ResultModel<bool> retornoVinculo =
            //new ResultModel<bool>(false)
            //{
            //    Success = false,
            //    Errors = new List<string>() { "Falha na liberação do agendamento para o POOL" },
            //    Message = "Falha na liberação do agendamento para o POOL"
            //};

            //var agendamento = (await _repositoryPortal.FindByHql<PeriodoCotaDisponibilidade>($"From PeriodoCotaDisponibilidade Where Id = {modelAgendamentoPool.AgendamentoId.GetValueOrDefault()}")).FirstOrDefault();
            //if (agendamento == null)
            //    throw new FileNotFoundException($"Não foi foi encontrado o agendamento com o Id: {modelAgendamentoPool.AgendamentoId}");

            //var emitirEspanhol = false;

            //var loggedUserValue = await _repositorySystem.GetLoggedUser();
            //if (!loggedUserValue.Value.isAdm)
            //{
            //    var usuarioProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUserValue.Value.userId));
            //    if (usuarioProvider != null)
            //    {
            //        if (agendamento != null)
            //        {
            //            var cotaAtual = await GetCotaAccessCenterPelosDadosPortal(new GetHtmlValuesModel() { PeriodoCotaDisponibilidadeId = agendamento.Id, UhCondominioId = agendamento.UhCondominio, CotaOrContratoId = agendamento.Cota });
            //            if (cotaAtual != null)
            //            {
            //                if (!string.IsNullOrEmpty(usuarioProvider.PessoaProvider))
            //                {
            //                    var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(usuarioProvider.PessoaProvider!) });
            //                    if (propCache != null && propCache.Any(b => b.frAtendimentoStatusCrcModels.Any(b => cotaAtual.FrAtendimentoVenda.GetValueOrDefault() == b.FrAtendimentoVendaId.GetValueOrDefault() &&
            //                    (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && b.AtendimentoStatusCrcStatus == "A")))
            //                    {
            //                        throw new ArgumentException("Não foi possível localizar as disponibilidades, motivo 0001BL");
            //                    }
            //                }
            //            }
            //        }

            //        var pessoa = (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = {usuarioProvider.PessoaProvider}")).FirstOrDefault();
            //        if (pessoa != null && pessoa.Estrangeiro == "S")
            //        {
            //            emitirEspanhol = true;
            //        }

            //        var resultInadimplente = await Inadimplente(usuarioProvider);
            //        if (resultInadimplente != null)
            //            throw new ArgumentException("Não foi possível realizar a operação: PF");

            //    }

            //}

            //if (!string.IsNullOrEmpty(modelAgendamentoPool.Agencia) && (string.IsNullOrEmpty(modelAgendamentoPool.CodigoBanco) || modelAgendamentoPool.CodigoBanco == "000"))
            //    throw new ArgumentException("Não foi possível encontrar o banco de código 000");

            //string? pathGeracao, htmlDocumentPath;
            //ValidarPathArquivosLiberacaoPool(out pathGeracao, out htmlDocumentPath, emitirEspanhol);



            //try
            //{
            //    _repositoryNHAccessCenter.BeginTransaction();
            //    _repositorySystem.BeginTransaction();

            //    (PeriodoCotaDisponibilidade? periodoCotaDisponibilidade, ParametroSistemaViewModel? parametros,
            //        (string userId, string providerKeyUser, string companyId, bool isAdm)? loggedUser,
            //        Domain.Entities.Core.Sistema.Usuario? usuario,
            //        Domain.Entities.Core.Framework.Empresa? empresa) =
            //        await ValidarLiberacaoSemanaPool(modelAgendamentoPool);

            //    ClienteContaBancaria? clienteContaBancaria = null;
            //    List<PessoaSistemaXProviderModel>? pessoaProviderUtilizar = null;
            //    if (!string.IsNullOrEmpty(modelAgendamentoPool.ContaNumero) || !string.IsNullOrEmpty(modelAgendamentoPool.ChavePix))
            //    {
            //        (ClienteContaBancaria? contaBancariaResult, List<PessoaSistemaXProviderModel>? pessoaProvider) = await CadastrarContaBancaria(modelAgendamentoPool, parametros, usuario);
            //        if (contaBancariaResult != null)
            //            clienteContaBancaria = contaBancariaResult;

            //    }

            //    (liberacaoEfetuada, retornoVinculo) = await LiberarSemanaParaPoolExecute(modelAgendamentoPool, liberacaoEfetuada, retornoVinculo);

            //    if (empresa == null)
            //    {
            //        empresa = (await _repositorySystem.FindBySql<Domain.Entities.Core.Framework.Empresa>("Select e.* From Empresa e Order by e.Id Desc")).FirstOrDefault();
            //        if (empresa == null)
            //            throw new ArgumentException("Não foi localizada a empresa");
            //    }

            //    var commitResult = await _repositoryNHAccessCenter.CommitAsync();
            //    if (!commitResult.executed)
            //        throw commitResult.exception ?? new Exception("Não foi possível realizar a operação");

            //    await GravarConfirmacaoLiberacaoParaPool(modelAgendamentoPool, pathGeracao, htmlDocumentPath, periodoCotaDisponibilidade, empresa, clienteContaBancaria, pessoaProviderUtilizar);

            //    var resultCommit = await _repositorySystem.CommitAsync();

            //    return new ResultModel<bool>()
            //    {
            //        Data = true,
            //        Message = "Liberação efetuada com sucesso",
            //        Success = true,
            //        Errors = new List<string>()
            //    };

            //}
            //catch (Exception err)
            //{
            //    _repositoryNHAccessCenter.Rollback();
            //    _repositorySystem.Rollback();
            //    if (liberacaoEfetuada)
            //    {
            //        var result = await RetirarSemanaPool_Esol(new AgendamentoInventarioModel()
            //        {
            //            AgendamentoId = modelAgendamentoPool.AgendamentoId,
            //            AcaoInterna = true
            //        });
            //    }

            //    if (retornoVinculo != null)
            //    {
            //        retornoVinculo.Message = err.Message;
            //        retornoVinculo.Errors = new List<string>() { err.Message };
            //        retornoVinculo.Success = false;
            //    }
            //    throw;
            //}

        }

        private async Task<(bool liberacaoEfetuada, ResultModel<bool> retornoVinculo)> LiberarSemanaParaPoolExecute(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool, bool liberacaoEfetuada, ResultModel<bool> retornoVinculo)
        {
            var result = new ResultModel<bool>();

            var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
            var liberarPoolUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:LiberarSemanaPool");
            var fullUrl = baseUrl + liberarPoolUrl;
            var token = await _serviceBase.getToken();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(fullUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, modelAgendamentoPool);

                string resultMessage = await responseResult.Content.ReadAsStringAsync();

                _logger.LogInformation(resultMessage);

                if (responseResult.IsSuccessStatusCode)
                {
                    retornoVinculo = System.Text.Json.JsonSerializer.Deserialize<ResultModel<bool>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (retornoVinculo != null && retornoVinculo!.Success)
                    {
                        retornoVinculo.Status = (int)HttpStatusCode.OK;
                        retornoVinculo.Success = true;
                        retornoVinculo.Data = true;
                        liberacaoEfetuada = true;
                    }
                }
                else
                {
                    retornoVinculo = System.Text.Json.JsonSerializer.Deserialize<ResultModel<bool>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (retornoVinculo != null)
                    {
                        retornoVinculo.Status = (int)HttpStatusCode.NotFound;
                        retornoVinculo.Success = false;
                    }
                }

            }

            return (liberacaoEfetuada, retornoVinculo);
        }

        private async Task GravarConfirmacaoLiberacaoParaPool(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool,
            string? pathGeracao,
            string htmlDocumentPath,
            PeriodoCotaDisponibilidade? periodoCotaDisponibilidade,
            Domain.Entities.Core.Framework.Empresa empresa,
            ClienteContaBancaria? contaBancariaResult,
            List<PessoaSistemaXProviderModel>? pessoaProvider)
        {
            throw new NotImplementedException();
            //var loggedUser = await _repositorySystem.GetLoggedUser();
            //if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.userId))
            //    throw new ArgumentException("Falha na gravação da liberação para POOL");

            //var pesProvider = pessoaProvider ?? await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId));
            //if (pesProvider == null)
            //    throw new ArgumentException("Falha na gravação da liberação para POOL");

            ////Ajustar formato conta, conta digito, agencia e agencia digito
            //if (!string.IsNullOrEmpty(modelAgendamentoPool.ContaNumero))
            //{
            //    modelAgendamentoPool.ContaNumero = modelAgendamentoPool.ContaNumero.TrimEnd().TrimStart();
            //    if (modelAgendamentoPool.ContaNumero.Contains(" "))
            //    {
            //        var conta = modelAgendamentoPool.ContaNumero.Split(" ")[0];
            //        var contaDigito = "0";
            //        if (modelAgendamentoPool.ContaNumero.Split(" ").Length > 1)
            //        {
            //            contaDigito = modelAgendamentoPool.ContaNumero.Split(" ")[1];
            //        }
            //        modelAgendamentoPool.ContaNumero = conta;
            //        modelAgendamentoPool.ContaDigito = contaDigito;
            //    }
            //    else if (modelAgendamentoPool.ContaNumero.Contains("-"))
            //    {
            //        var conta = modelAgendamentoPool.ContaNumero.Split("-")[0];
            //        var contaDigito = "0";
            //        if (modelAgendamentoPool.ContaNumero.Split("-").Length > 1)
            //        {
            //            contaDigito = modelAgendamentoPool.ContaNumero.Split("-")[1];
            //        }
            //        modelAgendamentoPool.ContaNumero = conta;
            //        modelAgendamentoPool.ContaDigito = contaDigito;
            //    }
            //}

            //if (!string.IsNullOrEmpty(modelAgendamentoPool.Agencia))
            //{
            //    modelAgendamentoPool.Agencia = modelAgendamentoPool.Agencia.TrimEnd().TrimStart();
            //    if (modelAgendamentoPool.Agencia.Contains(" "))
            //    {
            //        var agencia = modelAgendamentoPool.Agencia.Split(" ")[0];
            //        var agenciaDigito = "0";
            //        if (modelAgendamentoPool.Agencia.Split(" ").Length > 1)
            //        {
            //            agenciaDigito = modelAgendamentoPool.Agencia.Split(" ")[1];
            //        }
            //        modelAgendamentoPool.Agencia = agencia;
            //        modelAgendamentoPool.AgenciaDigito = agenciaDigito;
            //    }
            //    else if (modelAgendamentoPool.Agencia.Contains("-"))
            //    {
            //        var agencia = modelAgendamentoPool.Agencia.Split("-")[0];
            //        var agenciaDigito = "0";
            //        if (modelAgendamentoPool.Agencia.Split("-").Length > 1)
            //        {
            //            agenciaDigito = modelAgendamentoPool.Agencia.Split("-")[1];
            //        }
            //        modelAgendamentoPool.Agencia = agencia;
            //        modelAgendamentoPool.AgenciaDigito = agenciaDigito;
            //    }
            //}

            //bool emitirEmEspanhol = false;
            //var pessoa = (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = {pesProvider.PessoaProvider}")).FirstOrDefault();
            //if (pessoa != null && pessoa.Estrangeiro == "S")
            //{
            //    emitirEmEspanhol = true;
            //}

            //ConfirmacaoLiberacaoPool? confirmacao = null;

            //confirmacao =
            //    (await _repositorySystem.FindByHql<ConfirmacaoLiberacaoPool>(@$"From 
            //                                                        ConfirmacaoLiberacaoPool c 
            //                                                    Where 
            //                                                        c.AgendamentoId = {modelAgendamentoPool.AgendamentoId} 
            //                                                        and c.UsuarioCriacao = {Convert.ToInt32(loggedUser.Value.userId)}")).FirstOrDefault();
            //if (confirmacao == null)
            //{
            //    confirmacao = new ConfirmacaoLiberacaoPool()
            //    {
            //        UsuarioCriacao = Convert.ToInt32(loggedUser.Value.userId),
            //        DataHoraCriacao = DateTime.Now,
            //        Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresa.Id },
            //        AgendamentoId = modelAgendamentoPool.AgendamentoId,
            //        LiberacaoConfirmada = Domain.Enumns.EnumSimNao.Sim,
            //        CodigoEnviadoAoCliente = modelAgendamentoPool.CodigoVerificacao,
            //        DataConfirmacao = DateTime.Now,
            //        LiberacaoDiretaPeloCliente = Domain.Enumns.EnumSimNao.Sim,
            //        Banco = modelAgendamentoPool.CodigoBanco,
            //        Agencia = modelAgendamentoPool.Agencia,
            //        AgenciaDigito = modelAgendamentoPool.AgenciaDigito,
            //        Conta = modelAgendamentoPool.ContaNumero,
            //        ContaDigito = modelAgendamentoPool.ContaDigito,
            //        ChavePix = modelAgendamentoPool.ChavePix,
            //        Tipo = modelAgendamentoPool.Variacao,
            //        Variacao = modelAgendamentoPool.Variacao,
            //        TipoConta = modelAgendamentoPool.Variacao,
            //        Preferencial = modelAgendamentoPool.Preferencial.GetValueOrDefault(false) ? "N" : "S",
            //        TipoChavePix = modelAgendamentoPool.TipoChavePix,
            //        IdCidade = $"{modelAgendamentoPool.IdCidade.GetValueOrDefault(0)}"
            //    };

            //    await _repositorySystem.Save(confirmacao);
            //}

            //var cotaAccessCenter = await GetCotaAccessCenterPelosDadosPortal(new GetHtmlValuesModel()
            //{
            //    CotaOrContratoId = periodoCotaDisponibilidade.Cota,
            //    UhCondominioId = periodoCotaDisponibilidade.UhCondominio,
            //    PeriodoCotaDisponibilidadeId = periodoCotaDisponibilidade.Id
            //});

            //if (cotaAccessCenter == null)
            //    throw new ArgumentException("Não foi encontrada a cota vinculada.");


            //var contratoExistente = (await _repositorySystem.FindByHql<ContratoVinculoSCPEsol>(@$"From 
            //            ContratoVinculoSCPEsol con 
            //            Inner Join Fetch con.Empresa emp
            //        Where 
            //            emp.Id = {empresa.Id} and 
            //            con.PessoaLegadoId in ({string.Join(",", pesProvider.Select(p => p.PessoaProvider))}) and 
            //            con.CotaPortalId = {periodoCotaDisponibilidade.Cota} and 
            //            con.UhCondominioId = {periodoCotaDisponibilidade.UhCondominio} and 
            //            con.CotaAccessCenterId = {cotaAccessCenter.CotaId} Order by con.Id Desc")).AsList();


            //if (contratoExistente == null || !contratoExistente.Any())
            //{
            //    contratoExistente = new List<ContratoVinculoSCPEsol>() { new ContratoVinculoSCPEsol()
            //    {
            //        CodigoVerificacao = modelAgendamentoPool.CodigoVerificacao,
            //        Empresa = empresa,
            //        CotaAccessCenterId = cotaAccessCenter.CotaId,
            //        CotaPortalId = periodoCotaDisponibilidade.Cota,
            //        UhCondominioId = periodoCotaDisponibilidade.UhCondominio,
            //        PessoaLegadoId = int.Parse(pesProvider.First().PessoaProvider),
            //        Idioma = emitirEmEspanhol ? 2 : 0,
            //    } };
            //}

            //if (contratoExistente != null)
            //{
            //    await GerarContratoExecute(confirmacao, contaBancariaResult, contratoExistente, empresa.Id, emitirEmEspanhol || contratoExistente.Idioma == 2);
            //}
        }

        private async Task GerarContratoExecute(ConfirmacaoLiberacaoPool confirmacaoLiberacaoCotaPool,
            ClienteContaBancaria? contaBancariaResult,
            ContratoVinculoSCPEsol contrato,
            int empresaId, bool espanhol = false)
        {
            string? pathGeracao, htmlDocumentPath;
            ValidarPathArquivosLiberacaoPool(out pathGeracao, out htmlDocumentPath, espanhol);

            if (string.IsNullOrEmpty(pathGeracao))
                return;

            string htmlContent = File.ReadAllText(htmlDocumentPath);

            if (contrato.Idioma.GetValueOrDefault(-1) == -1)
            {
                contrato.Idioma = espanhol ? 2 : 0;
                await _repositorySystem.Save(contrato);
            }

            var dadosPreenchimentoContrato = await GetKeyValueListFromContratoSCP_Esol(new GetHtmlValuesModel()
            {
                CotaOrContratoId = contrato.CotaPortalId,
                UhCondominioId = contrato.UhCondominioId,
                PeriodoCotaDisponibilidadeId = confirmacaoLiberacaoCotaPool.AgendamentoId
            }, confirmacaoLiberacaoCotaPool?.CodigoEnviadoAoCliente ?? contrato.CodigoVerificacao ?? "LS",
              confirmacaoLiberacaoCotaPool!.DataConfirmacao.GetValueOrDefault(confirmacaoLiberacaoCotaPool.DataHoraCriacao.GetValueOrDefault(DateTime.Today)), espanhol);

            string dadosBancarios = "";
            if (contaBancariaResult != null)
            {
                var dadosBancariosResult = await DadosBancarios(contaBancariaResult.Id.GetValueOrDefault());
                if (dadosBancariosResult != null && !string.IsNullOrEmpty(dadosBancariosResult.NomeNormalizado))
                    dadosBancarios = dadosBancariosResult.NomeNormalizado;

                var dadosBancariosTemp = dadosPreenchimentoContrato.FirstOrDefault(a => !string.IsNullOrEmpty(a.Key) && a.Key.Equals("[DADOSBANCARIOS]", StringComparison.CurrentCultureIgnoreCase));
                if (dadosBancariosTemp != null)
                {
                    dadosBancariosTemp.Value = dadosBancarios;
                }
                else
                {
                    dadosPreenchimentoContrato.Add(new KeyValueModel()
                    {
                        Key = "[DADOSBANCARIOS]",
                        Value = $"{dadosBancarios}"
                    });
                }
            }

            htmlContent = await AplicarSubstituicoes(htmlContent, dadosPreenchimentoContrato);

            var launchOptions = new LaunchOptions
            {
                Headless = true
            };

            // Inicializar o PuppeteerSharp
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            // Carregar o conteúdo HTML na página
            await page.SetContentAsync(htmlContent);

            var pdfDocumentPath = Path.Combine(pathGeracao, $"{contrato.CotaAccessCenterId}_{contrato.UhCondominioId}_{contrato.CotaPortalId}.pdf");

            if (File.Exists(pdfDocumentPath))
            {
                File.Delete(pdfDocumentPath);
            }

            // Gerar o PDF
            await page.PdfAsync(pdfDocumentPath);

            contrato.Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = empresaId };
            contrato.DadosQualificacaoCliente = System.Text.Json.JsonSerializer.Serialize(dadosPreenchimentoContrato);
            contrato.DocumentoFull = htmlContent;
            contrato.PdfPath = pdfDocumentPath;

            await _repositorySystem.Save(contrato);

        }


        private async Task<(PeriodoCotaDisponibilidade? periodoCotaDisponibilidade,
            ParametroSistemaViewModel? parametros,
            (string userId, string providerKeyUser, string companyId, bool isAdm)? loggedUser,
            Domain.Entities.Core.Sistema.Usuario? usuario,
            Domain.Entities.Core.Framework.Empresa? empresa)> ValidarLiberacaoSemanaPool(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
        {
            throw new NotImplementedException();
            //ResultModel<List<InventarioModel>>? inventariosVinculadosAoAgendamentoNoPool = await ConsultarInventarios_Esol(new InventarioSearchModel()
            //{
            //    Agendamentoid = modelAgendamentoPool.AgendamentoId,
            //    NoPool = "S"
            //});

            //if (inventariosVinculadosAoAgendamentoNoPool == null ||
            //    !inventariosVinculadosAoAgendamentoNoPool.Success ||
            //    inventariosVinculadosAoAgendamentoNoPool.Data == null ||
            //    !inventariosVinculadosAoAgendamentoNoPool.Data.Any() ||
            //    inventariosVinculadosAoAgendamentoNoPool.Data.Count() > 1)
            //    throw new FileNotFoundException($"Não foi possível identificar o inventário/Pool compatível para utilização no agendamento Id: {modelAgendamentoPool.AgendamentoId.GetValueOrDefault()}");

            //modelAgendamentoPool.InventarioId = inventariosVinculadosAoAgendamentoNoPool.Data.First().Id;

            //InventarioModel? inventarioUtilizar = null;

            //if (modelAgendamentoPool.InventarioId.GetValueOrDefault(0) > 0)
            //{
            //    inventarioUtilizar = inventariosVinculadosAoAgendamentoNoPool.Data.First(a => a.Id == modelAgendamentoPool.InventarioId);
            //    if (inventarioUtilizar == null)
            //        throw new ArgumentException($"Não foi possível encontrar o inventário Pool para utilização no agendamento Id: {modelAgendamentoPool.InventarioId.GetValueOrDefault()}");
            //}

            //var periodoCotaDisponibilidade = (await _repositoryPortal.FindByHql<PeriodoCotaDisponibilidade>($"From PeriodoCotaDisponibilidade p Where p.Id = {modelAgendamentoPool.AgendamentoId}")).FirstOrDefault();
            //if (periodoCotaDisponibilidade == null)
            //    throw new ArgumentException($"Não foi encontrado agendamento com Id: {modelAgendamentoPool.AgendamentoId}");

            //var parametros = await _repositorySystem.GetParametroSistemaViewModel();
            //if (parametros == null || string.IsNullOrEmpty(parametros.ExibirFinanceirosDasEmpresaIds))
            //    throw new FileNotFoundException("Não foi encontrado a configuração das empresas vinculadas nos parâmetros do sistema.");

            //if (modelAgendamentoPool.AgendamentoId.GetValueOrDefault(0) == 0)
            //    throw new ArgumentException("Deve ser informado o AgendamentoId");

            //if (modelAgendamentoPool.InventarioId.GetValueOrDefault(0) == 0)
            //    throw new ArgumentException("Deve ser informado o InventárioId");


            //var loggedUser = await _repositorySystem.GetLoggedUser();
            //var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {loggedUser.Value.userId}and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
            //if (usuario == null || usuario.Pessoa == null)
            //    throw new FileNotFoundException("Não foi possível identificar o usuário logado para validar o código de confirmação para liberação da cota para POOL");

            //var contratoAssinadoAnteriormente = false;

            //if (loggedUser != null && !loggedUser.Value.isAdm)
            //{
            //    var usuarioProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId));
            //    if (!string.IsNullOrEmpty(usuarioProvider?.PessoaProvider) && int.TryParse(usuarioProvider.PessoaProvider, out _))
            //    {
            //        var hasContract = (await _repositorySystem.FindBySql<ContratoSCPModel>($"Select a.Id From ContratoVinculoSCPEsol a Where a.PessoaLegadoId = {usuarioProvider.PessoaProvider}")).FirstOrDefault();
            //        if (hasContract == null)
            //        {
            //            if (string.IsNullOrEmpty(modelAgendamentoPool.CodigoVerificacao))
            //                throw new ArgumentException("Deve ser informado o código de verificação recebido no seu eMail de cadastro");
            //        }
            //        else contratoAssinadoAnteriormente = true;
            //    }

            //    if (inventarioUtilizar?.DiasMinimoInicioAgendamentoAdicionar.GetValueOrDefault(0) > -1)
            //    {
            //        if (periodoCotaDisponibilidade.DataInicial.GetValueOrDefault().Date.Subtract(DateTime.Today).Days <
            //            inventarioUtilizar.DiasMinimoInicioAgendamentoAdicionar.GetValueOrDefault())
            //            throw new ArgumentException($"Só é possível liberar período para o POOL com no mímino: {inventarioUtilizar.DiasMinimoInicioAgendamentoAdicionar.GetValueOrDefault()} dias de antecedência.");
            //    }

            //}


            //var empresa = (await _repositorySystem.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e Where 1 = 1 Order by e.Id desc")).FirstOrDefault();

            //if (empresa == null)
            //    throw new FileNotFoundException("Não foi possível identificar a empresa logada no sistema.");
            //if (!contratoAssinadoAnteriormente)
            //{
            //    bool codigoValido = await ValidarCodigo_Esol(modelAgendamentoPool.AgendamentoId.GetValueOrDefault(), modelAgendamentoPool.CodigoVerificacao, false);
            //    if (!codigoValido)
            //        throw new ArgumentException("O código de verificação informado não é válido");
            //}

            //return (periodoCotaDisponibilidade, parametros, loggedUser, usuario, empresa);
        }

        private async Task<(ClienteContaBancaria? contaBancariaResult, List<PessoaSistemaXProviderModel>? pessoaProvider)> CadastrarContaBancaria(LiberacaoMeuAgendamentoInputModel model, 
            ParametroSistemaViewModel parametros, 
            Domain.Entities.Core.Sistema.Usuario usuario)
        {
            ClienteContaBancaria? contaBancariaResult =
                                model.ClienteContaBancariaId.GetValueOrDefault(0) > 0 ?
                                (await _repositoryNHAccessCenter.FindByHql<ClienteContaBancaria>($"From ClienteContaBancaria ccb Where ccb.Id = {model.ClienteContaBancariaId.GetValueOrDefault(0)}")).FirstOrDefault() : null;

            var pessoasProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(usuario.Id, ProviderName);
            if (pessoasProvider == null || !pessoasProvider.Any())
                throw new FileNotFoundException("Não foi possível identificar a pessoa no sistema legado");

            if (contaBancariaResult == null)
            {

                var clientesIds = (await _repositoryNHAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Cliente>(@$"Select cli.Id, cli.Empresa 
                                                From 
                                                    Cliente cli 
                                                Where 
                                                    cli.Pessoa in ({string.Join(",", pessoasProvider.Select(b=> b .PessoaProvider))}) and 
                                                    cli.Empresa in ({parametros.ExibirFinanceirosDasEmpresaIds})")).AsList();

                if (clientesIds == null || clientesIds.Count == 0)
                    throw new ArgumentException($"Não foi possível identificar o cliente vinculado ao agendamento Id: {model.AgendamentoId}");


                //Ajustar formato conta, conta digito, agencia e agencia digito
                if (!string.IsNullOrEmpty(model.ContaNumero))
                {
                    model.ContaNumero = model.ContaNumero.TrimEnd().TrimStart();
                    if (model.ContaNumero.Contains(" "))
                    {
                        var conta = model.ContaNumero.Split(" ")[0];
                        var contaDigito = "0";
                        if (model.ContaNumero.Split(" ").Length > 1)
                        {
                            contaDigito = model.ContaNumero.Split(" ")[1];
                        }
                        model.ContaNumero = conta;
                        model.ContaDigito = contaDigito;
                    }
                    else if (model.ContaNumero.Contains("-"))
                    {
                        var conta = model.ContaNumero.Split("-")[0];
                        var contaDigito = "0";
                        if (model.ContaNumero.Split("-").Length > 1)
                        {
                            contaDigito = model.ContaNumero.Split("-")[1];
                        }
                        model.ContaNumero = conta;
                        model.ContaDigito = contaDigito;
                    }
                }

                if (!string.IsNullOrEmpty(model.Agencia))
                {
                    model.Agencia = model.Agencia.TrimEnd().TrimStart();
                    if (model.Agencia.Contains(" "))
                    {
                        var agencia = model.Agencia.Split(" ")[0];
                        var agenciaDigito = "0";
                        if (model.Agencia.Split(" ").Length > 1)
                        {
                            agenciaDigito = model.Agencia.Split(" ")[1];
                        }
                        model.Agencia = agencia;
                        model.AgenciaDigito = agenciaDigito;
                    }
                    else if (model.Agencia.Contains("-"))
                    {
                        var agencia = model.Agencia.Split("-")[0];
                        var agenciaDigito = "0";
                        if (model.Agencia.Split("-").Length > 1)
                        {
                            agenciaDigito = model.Agencia.Split("-")[1];
                        }
                        model.Agencia = agencia;
                        model.AgenciaDigito = agenciaDigito;
                    }
                }

                contaBancariaResult = await _financeiroService.SalvarContaBancariaInterna(new Models.Financeiro.ClienteContaBancariaInputModel()
                {
                    Id = model.ClienteContaBancariaId,
                    ClienteId = clientesIds.First().Id,
                    ClientesIds = clientesIds.Select(b => b.Id.GetValueOrDefault()).AsList(),
                    CodigoBanco = model.CodigoBanco,
                    Agencia = model.Agencia,
                    AgenciaDigito = model.AgenciaDigito,
                    ContaNumero = model.ContaNumero ?? null,
                    Variacao = !string.IsNullOrEmpty(model.Variacao) && model.Variacao.StartsWith("p", StringComparison.CurrentCultureIgnoreCase) ? "P" : "C",
                    ContaDigito = model.ContaDigito,
                    Preferencial = model.Preferencial,
                    IdCidade = model.IdCidade,
                    TipoChavePix = model.TipoChavePix,
                    ChavePix = model.ChavePix,
                    Status = model.Status
                });
            }

            return (contaBancariaResult, pessoasProvider);
        }

        private void ValidarPathArquivosLiberacaoPool(out string? pathGeracao, out string htmlDocumentPath, bool espanhol = false)
        {
            var pathModelosContratos = _configuration.GetValue<string>($"CertidoesConfig:ModelosCertidoesPath");
            if (string.IsNullOrEmpty(pathModelosContratos))
                throw new FileNotFoundException("Não foi encontrada path dos modelos de contratos, necessária para liberação da semana para POOL");


            pathGeracao = _configuration.GetValue<string>($"CertidoesConfig:GeracaoPdfContratoPath");
            if (string.IsNullOrEmpty(pathGeracao))
                throw new FileNotFoundException("Não foi encontrada a path para gravação do contrato, necessária para liberação da semana para POOL");

            var modelosContrato = _configuration.GetValue<string>($"CertidoesConfig:ContratoSCPPorEmpresa");
            if (string.IsNullOrEmpty(modelosContrato))
                throw new FileNotFoundException("Não foi encontrado o modelo de contrato, necessário para liberação da semana para o POOL");

            if (espanhol)
            {
                modelosContrato = _configuration.GetValue<string>($"CertidoesConfig:ContratoSCPEspanhol");
                if (string.IsNullOrEmpty(modelosContrato))
                    throw new FileNotFoundException($"Não foi encontrado o modelo de contrato em espanhol, necessário para liberação da semana para o POOL");
            }

            htmlDocumentPath = "";
            if (!Directory.Exists(pathGeracao))
                Directory.CreateDirectory(pathGeracao);

            if (string.IsNullOrEmpty(modelosContrato))
                throw new FileNotFoundException("Não foi encontrado o modelo de contrato, necessário para liberação da semana para POOL");

            if (modelosContrato.Contains("|"))
            {
                foreach (var item in modelosContrato.Split('|'))
                {
                    var itemValidar = item.Contains(":") ? item.Split(':')[1] : item;
                    if (!File.Exists(Path.Combine(pathModelosContratos, itemValidar)))
                    {
                        throw new FileNotFoundException($"Não foi encontrado o modelo de contrato: '{itemValidar}'");
                    }
                    htmlDocumentPath = Path.Combine(pathModelosContratos, itemValidar);
                }
            }
            else
            {
                var modeloTestar = modelosContrato.Contains(":") ? modelosContrato.Split(':')[1] : modelosContrato;

                if (!File.Exists(Path.Combine(pathModelosContratos, modeloTestar)))
                {
                    throw new FileNotFoundException($"Não foi encontrado o modelo de contrato em espanhol, necessário para liberação da semana para o POOL: '{modeloTestar}'");
                }
                htmlDocumentPath = Path.Combine(pathModelosContratos, modeloTestar);
            }
        }

        private async Task<ClienteContaBancariaViewModel?> DadosBancarios(int clienteContaBancariaId)
        {
            StringBuilder sb = new StringBuilder(@$"Select 
                                                    ccb.Id, 
                                                    ccb.Agencia,
                                                    ccb.AgenciaDigito, 
                                                    ccb.Conta as ContaNumero, 
                                                    ccb.ContaDigito, 
                                                    b.Codigo as CodigoBanco, 
                                                    b.Nome as NomeBanco,
                                                    Case when ccb.Preferencial = 'N' Then 'Não' else 'Sim' end as Preferencial,
                                                    ccb.Cidade as IdCidade,
                                                    c.Nome as NomeCidade,
                                                    e.Uf as SiglaEstadoCidade,
                                                    ccb.Cliente as IdFornecedor,
                                                    ccb.TipoChavePix,
                                                    ccb.Status,
                                                    Case 
                                                    When ccb.TipoChavePix = 'F' then 'CPF'
                                                    When ccb.TipoChavePix = 'J' then 'CNPJ'
                                                    When ccb.TipoChavePix = 'E' then 'eMail'
                                                    When ccb.TipoChavePix = 'T' then 'Número de telefone'
                                                    When ccb.TipoChavePix = 'A' then 'Aleatória' end as DescricaoTipoChavePix,
                                                    ccb.ChavePix,
                                                    ccb.Tipo
                                                    From 
                                                    ClienteContaBancaria ccb 
                                                    Left Join Cliente cli on ccb.Cliente = cli.Id
                                                    Left Join Banco b on ccb.Banco = b.Id
                                                    Left Join Cidade c on ccb.Cidade = c.Id
                                                    Left Join Estado e on c.Estado = e.Id
                                                    Where ccb.Id = {clienteContaBancariaId} ");



            var clienteContaBancaria = (await _repositoryNHAccessCenter.FindBySql<ClienteContaBancariaViewModel>(sb.ToString())).AsList();
            foreach (var item in clienteContaBancaria)
            {
                if (item.Tipo == "P" && !string.IsNullOrEmpty(item.ChavePix))
                {
                    item.NomeNormalizado = $"Pix {item.DescricaoTipoChavePix} ({item.ChavePix})";
                }
                else if (!string.IsNullOrEmpty(item.NomeBanco))
                {
                    item.NomeNormalizado = $"Banco: {item.NomeBanco}({item.CodigoBanco}) AG: {item.Agencia}-{(!string.IsNullOrEmpty(item.AgenciaDigito) ? item.AgenciaDigito : "0")} C/C: {item.ContaNumero}";
                    if (!string.IsNullOrEmpty(item.ContaDigito))
                        item.NomeNormalizado += $"-{item.ContaDigito}";

                    if (!string.IsNullOrEmpty(item.ChavePix))
                    {
                        item.NomeNormalizado += $" Pix {item.DescricaoTipoChavePix} ({item.ChavePix})";
                    }
                }
            }

            return clienteContaBancaria.FirstOrDefault();
        }

        private async Task<string> AplicarSubstituicoes(string htmlContent, List<KeyValueModel> variaveis)
        {
            var htmlAjustado = htmlContent;
            if (variaveis == null || !variaveis.Any())
                return await Task.FromResult(htmlAjustado);

            foreach (var item in variaveis)
            {
                htmlAjustado = htmlAjustado.Replace($"{item.Key}", item.Value, StringComparison.OrdinalIgnoreCase);
            }

            return await Task.FromResult(htmlAjustado);
        }

        public async Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_Esol(int agendamentoId)
        {
            if (agendamentoId <= 0)
                throw new ArgumentException("Deve ser informado o agendamento Id");

            var isAdm = _repositorySystem.IsAdm;

            var sb = new StringBuilder(@$"select
                Concat('LP-',Cast(c.Id as varchar)) as OperacaoId,
                c.AgendamentoId,
                'Liberação para POOL' as TipoOperacao,
                u.Login LoginUsuario,
                p.nome as NomeUsuario,
                c.datahoracriacao as DataOperacao,
                c.dataconfirmacao as DataConfirmacao,
                case when c.liberacaoconfirmada = 1 then 'Liberação confirmada com sucesso' else 'Liberação não efetuada' end as Historico,
                c.tentativas 
                from
                confirmacaoliberacaopool c 
                inner join usuario u on c.UsuarioCriacao = u.Id
                inner join pessoa p on u.pessoa = p.id 
                Where 1 = 1 and c.AgendamentoId = {agendamentoId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ");

            if (isAdm)
            {
                sb.AppendLine(@$"
                Union all
                select
                Concat('RP-',Cast(c.Id as varchar)) as OperacaoId,
                c.AgendamentoId,
                'Retirada do Pool' as TipoOperacao,
                u.Login LoginUsuario,
                p.nome as NomeUsuario,
                c.datahoracriacao as DataOperacao,
                c.datahoracriacao as DataConfirmacao,
                'Agendamento retirado do Pool com sucesso' as Historico,
                '' as tentativas 
                from
                HistoricoRetiradaPool c 
                inner join usuario u on c.UsuarioCriacao = u.Id
                inner join pessoa p on u.pessoa = p.id 
                Where 1 = 1 and c.AgendamentoId = {agendamentoId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ");

                sb.AppendLine($@"
                Union all 
                select 
                Concat('TP-',cast(c.Id as varchar)) as OperacaoId,
                c.NovoAgendamentoId as AgendamentoId,
                'Troca de período' as TipoOperacao,
                u.Login as LoginUsuario,
                p.Nome as NomeUsuario,
                c.DataHoraCriacao as DataOperacao,
                c.DataHoraCriacao as DataConfirmacao,
                c.Descricao as Historico,
                '' Tentativas
                from
                historicotrocadesemana c
                inner join Usuario u on c.UsuarioCriacao = u.id
                inner join Pessoa p on u.Pessoa = p.id
                where
                c.AgendamentoAnteriorId is not null and
                c.novoagendamentoid = {agendamentoId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ");

                sb.AppendLine($@"
                Union all 
                select 
                Concat('IP-',cast(c.Id as varchar)) as OperacaoId,
                c.NovoAgendamentoId as AgendamentoId,
                'Inclusão de período' as TipoOperacao,
                u.Login as LoginUsuario,
                p.Nome as NomeUsuario,
                c.DataHoraCriacao as DataOperacao,
                c.DataHoraCriacao as DataConfirmacao,
                c.Descricao as Historico,
                '' Tentativas
                from
                historicotrocadesemana c
                inner join Usuario u on c.UsuarioCriacao = u.id
                inner join Pessoa p on u.Pessoa = p.id
                where
                c.AgendamentoAnteriorId is null and
                c.novoagendamentoid = {agendamentoId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ");

            }

            var result = (await _repositorySystem.FindBySql<AgendamentoHistoryModel>(sb.ToString())).AsList();
            if (result.Any())
            {
                return new ResultModel<List<AgendamentoHistoryModel>>(result.OrderBy(b => b.DataConfirmacao.GetValueOrDefault(b.DataOperacao.GetValueOrDefault())).AsList())
                {
                    Success = true,
                    Status = (int)HttpStatusCode.OK,
                    Errors = new List<string>(),
                };
            }
            else
            {
                return new ResultModel<List<AgendamentoHistoryModel>>(result)
                {
                    Success = true,
                    Status = (int)HttpStatusCode.NotFound,
                    Errors = new List<string>(),
                };
            }
        }

        public async Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_Esol(DispobilidadeSearchModel searchModel)
        {
            var result = new ResultModel<List<SemanaDisponibilidadeModel>>();

            try
            {
                if (searchModel.Agendamentoid.GetValueOrDefault(0) <= 0 && searchModel.CotaAccessCenterId.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Deve ser informado o AgendamentoId ou a CotaAccessCenterId");

                var loggedUser = await _repositorySystem.GetLoggedUser();
                if (loggedUser.Value.isAdm && searchModel.CotaAccessCenterId.GetValueOrDefault(0) == 0 && searchModel.Agendamentoid.GetValueOrDefault(0) > 0)
                {
                    var agendamento = (await _repositoryPortal.FindBySql<PeriodoCotaDisponibilidade>($"Select * From PeriodoCotaDisponibilidade Where Id = {searchModel.Agendamentoid} ")).FirstOrDefault();
                    if (agendamento != null && agendamento.Cota.GetValueOrDefault(0) > 0)
                    {
                        var cotaAc = await GetCotaAccessCenterPelosDadosPortal(new GetHtmlValuesModel() { PeriodoCotaDisponibilidadeId = agendamento.Id, UhCondominioId = agendamento.UhCondominio, CotaOrContratoId = agendamento.Cota });
                        if (cotaAc != null)
                        {
                            searchModel.CotaAccessCenterId = cotaAc.CotaId;
                            searchModel.AdmVisaoDeCliente = true;
                        }
                    }
                }

                if (loggedUser.Value.isAdm)
                {
                    var cotaAc = searchModel.CotaAccessCenterId > 0 ? (await _repositoryNHAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Cota>($"Select c.* From Cota c Where c.Id = {searchModel.CotaAccessCenterId}")).FirstOrDefault() : null;
                    if (cotaAc == null || cotaAc.Proprietario == null)
                        throw new ArgumentException($"Não foi possível encontrar a cota Id: {searchModel.CotaAccessCenterId}");

                    var clienteVinculadoCota = (await _repositoryNHAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Cliente>($"Select c.* From Cliente c Where c.Id = {cotaAc.Proprietario.GetValueOrDefault()}")).FirstOrDefault();
                    if (clienteVinculadoCota == null || clienteVinculadoCota.Pessoa.GetValueOrDefault(0) <= 0)
                        throw new ArgumentException($"Não foi possível encontrar a cota Id: {searchModel.CotaAccessCenterId}");

                    searchModel.AdmVisaoDeCliente = true;
                    searchModel.PessoaLegadoId = clienteVinculadoCota.Pessoa;
                }


                var pessoaVinculadaSistema = searchModel.AdmVisaoDeCliente.GetValueOrDefault(false) == false ?
                    await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), CommunicationProviderName) : null;
                if (pessoaVinculadaSistema == null && searchModel.AdmVisaoDeCliente.GetValueOrDefault(false) == false)
                    throw new ArgumentException($"Não foi encontrada pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");


                if (searchModel.Agendamentoid.GetValueOrDefault(0) <= 0)
                {
                    CotaAccessCenterModel? cotaPortalUtilizar = await GetCotaPortalOuCotaAccessCenter(searchModel.CotaAccessCenterId);
                    if (cotaPortalUtilizar != null)
                    {
                        searchModel.CotaPortalId = cotaPortalUtilizar.CotaId;
                        searchModel.CotaAccessCenterId = searchModel.CotaAccessCenterId;
                        searchModel.UhCondominioId = cotaPortalUtilizar.UhCondominio.GetValueOrDefault(0);
                        searchModel.CotaPortalNome = cotaPortalUtilizar.CotaNome;
                        searchModel.CotaPortalCodigo = cotaPortalUtilizar.CotaCodigo;
                        searchModel.GrupoCotaPortalNome = cotaPortalUtilizar.GrupoCotaNome;
                        searchModel.CotaProprietarioId = cotaPortalUtilizar.CotaProprietarioId;
                        searchModel.NumeroImovel = cotaPortalUtilizar.NumeroImovel;
                        searchModel.EmpresaAcId = cotaPortalUtilizar.EmpresaAcId;
                        searchModel.EmpresaPortalId = cotaPortalUtilizar.EmpresaPortalId;


                        var empresaCondominioPortalId = _configuration.GetValue<string>("EmpresaCondominioPortalId", "1,15");

                        if (searchModel.CotaPortalId.GetValueOrDefault(0) == 0)
                            throw new ArgumentException($"Não foi possível encontrar a eSolution Cota Portal vinculada a Cota AccessCenter Id: {searchModel.CotaAccessCenterId}");

                        var cotaPortalUtilizarObjeto = (await _repositoryPortal.FindBySql<CotaAccessCenterModel>(@$"Select 
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
                                    uc.Numero AS NumeroUhCondominio
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
                                    c.Id = {searchModel.CotaPortalId} and 
                                    cp.Id = {searchModel.CotaProprietarioId} and 
                                    uc.Id = {searchModel.UhCondominioId} and 
                                    (uc.Numero = '{searchModel.NumeroImovel}' or u.Numero = '{searchModel.NumeroImovel}')")).FirstOrDefault();

                        if (cotaPortalUtilizarObjeto == null)
                            throw new ArgumentException($"Não foi encontrada a Cota com Id: {searchModel.CotaPortalId} no Portal eSolution");
                    }
                    else throw new ArgumentException($"Não foi encontrada a Cota no eSolution Portal, vinculada a conta da AccessCenter Id: {searchModel.CotaAccessCenterId}");

                }

                _logger.LogInformation($"{DateTime.Now} - Buscando disponibilidades compatíveis");


                try
                {

                    var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                    var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:ConsultarDisponibilidadeCompativel");
                    var fullUrl = $"{baseUrl}{consultarReservaUrl}?{searchModel.ToQueryString()}";
                    var token = await _serviceBase.getToken();

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(fullUrl);
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("accept", "application/json");
                        client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                        HttpResponseMessage responseResult = await client.GetAsync(fullUrl);

                        string resultMessage = await responseResult.Content.ReadAsStringAsync();

                        if (responseResult.IsSuccessStatusCode)
                        {
                            result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<List<SemanaDisponibilidadeModel>>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                            if (result != null)
                                result.Status = (int)HttpStatusCode.OK;
                        }
                        else
                        {
                            result.Status = (int)HttpStatusCode.NotFound;
                            result.Errors = new List<string>() { $"Erro: {responseResult}" };

                        }
                    }

                }
                catch (HttpRequestException err)
                {
                    throw err;
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                if (result != null)
                {
                    result.Errors.Add($"Erro: {err.Message}");
                    result.Status = (int)HttpStatusCode.BadRequest;
                    result.Message = err.Message;
                }
            }

            return result;
        }

        private async Task<DadosContratoAccessCenterModel?> GetDadosContrato(int? cotaAcId, int? cotaPortalId, int? uhCondominioId)
        {

            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            var empresaCondominioPortalId = _configuration.GetValue<string>("EmpresaCondominioPortalId", "1,15");

            if (cotaAcId.GetValueOrDefault(0) == 0 && (cotaPortalId.GetValueOrDefault(0) == 0 || uhCondominioId.GetValueOrDefault(0) == 0))
                throw new ArgumentException("Deve ser informada a cotaAcId ou (cotaPortalId e uhCondominioId)");

            if (cotaAcId.GetValueOrDefault(0) > 0)
            {
                var cotaAccessCenter =
                    (await _repositoryNHAccessCenter.FindBySql<DadosContratoAccessCenterModel>($@"
                select
                c.Id as CotaId,
                i.Numero as ImovelNumero,
                frp.Nome as Produto,
                av.Codigo as NumeroContrato,
                gctc.Nome as CotaNome,
                gctc.Codigo as CotaCodigo,
                emp.Id as EmpreendimentoId,
                'MY MABU' as EmpreendimentoNome,
                'AC' as TipoRetorno,
                p.Nome as Titular1Nome,
                p1.Nome as Titular2Nome,
                ib.Nome as ImovelBloco,
                ia.Nome as ImovelAndar,
                av.IdIntercambiadora,
                Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                From
                FrAtendimentoVenda av
                Left Join Cota c on av.Cota = c.Id
                Left Join FrPessoa fp1 on av.FrPessoa1 = fp1.Id
                Left Join Pessoa p on fp1.Pessoa = p.Id
                Left Join FrPessoa fp2 on av.FrPessoa2 = fp2.Id
                Left Join Pessoa p1 on fp2.Pessoa = p1.Id
                Inner Join Imovel i on c.Imovel = i.Id
                Inner Join ImovelAndar ia on i.ImovelAndar = ia.Id
                Inner Join ImovelBloco ib on i.ImovelBloco = ib.Id
                Inner join Empreendimento emp on i.Empreendimento = emp.Id 
                Inner Join GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                Inner Join GrupoCota gc on gctc.GrupoCota = gc.Id
                Inner Join TipoCota tc on gctc.TipoCota = tc.Id
                Left Join FrProduto frp on av.FrProduto = frp.Id
                Left Join TipoImovel ti on i.TipoImovel = ti.Id
                where
                c.Id = {cotaAcId}")).FirstOrDefault();

                if (cotaAccessCenter != null && !aplicarPadraoBlack)
                    cotaAccessCenter.PadraoDeCor = "Default";

                return cotaAccessCenter;
            }
            else if (cotaPortalId.GetValueOrDefault(0) > 0 && uhCondominioId.GetValueOrDefault(0) > 0)
            {

                var cotaAccessCenter = await GetCotaAccessCenterPelosDadosPortal(new GetHtmlValuesModel()
                {
                    CotaOrContratoId = cotaPortalId,
                    UhCondominioId = uhCondominioId,
                    PeriodoCotaDisponibilidadeId = null
                });

                if (cotaAccessCenter == null)
                    throw new ArgumentException($"Não foi possível localizar a cota portal Id: {cotaPortalId}");

                var dadosRetorno =
                    (await _repositoryNHAccessCenter.FindBySql<DadosContratoAccessCenterModel>($@"
                    select
                    c.Id as CotaId,
                    i.Numero as ImovelNumero,
                    frp.Nome as Produto,
                    av.Codigo as NumeroContrato,
                    gctc.Nome as CotaNome,
                    gctc.Codigo as CotaCodigo,
                    emp.Id as EmpreendimentoId,
                    'MY MABU' as EmpreendimentoNome,
                    'AC' as TipoRetorno,
                    p.Nome as Titular1Nome,
                    p1.Nome as Titular2Nome,
                    ib.Nome as ImovelBloco,
                    ia.Nome as ImovelAndar,
                    av.IdIntercambiadora,
                    Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
                    From
                    FrAtendimentoVenda av
                    Inner Join Cota c on av.Cota = c.Id
                    Inner Join GrupoCotaTipoCota gctc on c.GrupoCotaTipoCota = gctc.Id
                    Inner Join GrupoCota gc on gctc.GrupoCota = gc.Id
                    Inner Join TipoCota tc on gctc.TipoCota = tc.Id
                    Left Join FrPessoa fp1 on av.FrPessoa1 = fp1.Id
                    Left Join Pessoa p on fp1.Pessoa = p.Id
                    Left Join FrPessoa fp2 on av.FrPessoa2 = fp2.Id
                    Left Join Pessoa p1 on fp2.Pessoa = p1.Id
                    Left Join Imovel i on c.Imovel = i.Id
                    Left Join TipoImovel ti on i.TipoImovel = ti.Id
                    Left Join ImovelAndar ia on i.ImovelAndar = ia.Id
                    Left Join ImovelBloco ib on i.ImovelBloco = ib.Id
                    Left join Empreendimento emp on i.Empreendimento = emp.Id 
                    Left Join FrProduto frp on av.FrProduto = frp.Id
                    where
                    c.Id = {cotaAccessCenter.CotaId}")).FirstOrDefault();

                if (dadosRetorno != null && !aplicarPadraoBlack)
                    dadosRetorno.PadraoDeCor = "Default";

                return dadosRetorno;

            }

            return default;

        }

        private async Task<CotaAccessCenterModel?> GetCotaPortalOuCotaAccessCenter(int? cotaId, bool apenasCotaAccessCenter = false)
        {
            var empresaCondominioPortalId = _configuration.GetValue<string>("EmpresaCondominioPortalId", "1,15");
            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            if (cotaId.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informada a CotaId");

            var cotaAccessCenter = (await _repositoryNHAccessCenter.FindBySql<CotaAccessCenterModel>($@"select
                                                                                            c.Id as CotaId,
                                                                                            c.Id as CotaAcId,
                                                                                            i.Id as Imovel,
                                                                                            ia.Codigo as AndarCodigo,
                                                                                            ia.Nome as AndarNome,
                                                                                            ib.Codigo as CodigoBloco,
                                                                                            ib.Nome as NomeBloco,
                                                                                            i.Numero as NumeroImovel,
                                                                                            i.Tag as TagImovel,
                                                                                            c.Tag as TagCota,
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
                                                                                            clip.Id as PessoaProviderId,
                                                                                            COALESCE(emp.Empresa,cli.Empresa) as EmpresaAcId,
                                                                                            av.IdIntercambiadora,
                                                                                            av.Codigo as NumeroContrato,
                                                                                            Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
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
                                                                                            Left Join FrAtendimentoVenda av on av.Cota = c.Id and av.Status = 'A'
                                                                                            Left Join TipoImovel ti on i.TipoImovel = ti.Id
                                                                                            where
                                                                                            c.Id = {cotaId}")).FirstOrDefault();

            if (cotaAccessCenter == null)
            {
                return cotaAccessCenter;
            }
            else
            {
                if (apenasCotaAccessCenter)
                {
                    await GetDadosCompletosCotaAccessCenter(cotaAccessCenter);
                    if (!aplicarPadraoBlack && cotaAccessCenter != null)
                        cotaAccessCenter.PadraoDeCor = "Default";
                    return cotaAccessCenter;
                }

                var cotasPortal = (await _repositoryPortal.FindBySql<CotaAccessCenterModel>(@$"Select 
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
                                    uc.Numero as NumeroUhCondominio,
                                    gc.Empresa as EmpresaPortalId
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
                                    u.Numero = '{cotaAccessCenter.NumeroImovel!.TrimEnd()}'")).AsList();

                if (!cotasPortal.Any())
                    throw new FileNotFoundException($"Não foi encontrada nenhuma cota no eSolution Portal");

                var cotaPortalUtilizar = cotasPortal.FirstOrDefault(a => (Helper.RemoveAccents(a.CotaNome!)
                .Equals(Helper.RemoveAccents(cotaAccessCenter.GrupoCotaTipoCotaNome!), StringComparison.CurrentCultureIgnoreCase) &&
                    !string.IsNullOrEmpty(a.GrupoCotaNome) && a.GrupoCotaNome.Contains(cotaAccessCenter.GrupoCotaNome!, StringComparison.InvariantCultureIgnoreCase) &&
                    a.NumeroImovel == cotaAccessCenter.NumeroImovel));

                if (cotaPortalUtilizar == null && cotaAccessCenter != null && cotaAccessCenter.GrupoCotaNome!.RemoveAccents().Contains("UNICA", StringComparison.CurrentCultureIgnoreCase))
                {
                    cotaPortalUtilizar = cotasPortal.FirstOrDefault(a => Helper.RemoveAccents(a.CotaNome!).Equals(Helper.RemoveAccents(cotaAccessCenter.GrupoCotaTipoCotaNome!), StringComparison.CurrentCultureIgnoreCase) &&
                    a.NumeroImovel == cotaAccessCenter.NumeroImovel);
                }

                if (cotaPortalUtilizar != null)
                {
                    if (cotaPortalUtilizar != null && !cotaPortalUtilizar.ProprietarioNome!.Split(' ')[0].Contains(cotaAccessCenter!.ProprietarioNome!.Split(' ')[0], StringComparison.InvariantCultureIgnoreCase) &&
                        cotaAccessCenter.NumeroImovel != cotaPortalUtilizar.NumeroImovel)
                        throw new FileNotFoundException($"Não foi possível localizar a Cota no Portal eSolution vinculada a cota Id: {cotaAccessCenter.CotaId}");


                    cotaPortalUtilizar!.EmpresaAcId = cotaAccessCenter?.EmpresaAcId ?? null;
                    cotaPortalUtilizar!.EmpresaPortalId = cotaPortalUtilizar.EmpresaPortalId;
                    cotaPortalUtilizar!.IdIntercambiadora = cotaPortalUtilizar.IdIntercambiadora;
                    cotaPortalUtilizar.CotaAcId = cotaAccessCenter?.CotaAcId ?? 0;

                    if (!string.IsNullOrEmpty(cotaAccessCenter?.PadraoDeCor))
                        cotaPortalUtilizar!.PadraoDeCor = cotaAccessCenter?.PadraoDeCor;

                    if (!aplicarPadraoBlack && cotaPortalUtilizar != null)
                        cotaPortalUtilizar.PadraoDeCor = "Default";

                    return cotaPortalUtilizar;
                }
            }

            return default;

        }


        private async Task<CotaAccessCenterModel?> GetCotaAccessCenterPelosDadosPortal(GetHtmlValuesModel dadosCota)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            //if (dadosCota.CotaOrContratoId.GetValueOrDefault(0) == 0)
            //    throw new ArgumentException("Erro: Deve ser informado o valor para CotaOrContratoId");

            if (dadosCota.UhCondominioId.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Erro: Deve ser informado o valor para UhCondominioId");


            var empresaCondominioPortalId = _configuration.GetValue<string>("EmpresaCondominioPortalId", "1,15");
            var empreendimentosAccessCenter = _configuration.GetValue<string>("EmpreendimentoId", "1,21");

            var cotaPortal = (await _repositoryPortal.FindBySql<CotaAccessCenterModel>(@$"Select 
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
                                    uc.Numero as NumeroUhCondominio,
                                    th.Capacidade
                                    From 
                                    Cota c
                                    Inner Join CotaProprietario cp on cp.Cota = c.Id and cp.DataHoraExclusao is null and cp.UsuarioExclusao is null
                                    Inner Join UhCondominio uc on cp.UhCondominio = uc.Id
                                    Inner Join Uh u on uc.Id = u.UhCondominio
                                    Inner Join TipoUh th on u.TipoUh = th.Id
                                    Inner Join GrupoCotas gc on c.GrupoCotas = gc.Id
                                    Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null and pro.UsuarioExclusao IS null
                                    Inner Join Cliente cli on pro.Cliente = cli.Id
                                    Inner Join Pessoa clip on cli.Pessoa = clip.Id
                                    Where 
                                    gc.Empresa in ({empresaCondominioPortalId}) and 
                                    uc.Id = {dadosCota.UhCondominioId} and 
                                    c.Id = {dadosCota.CotaOrContratoId} ")).FirstOrDefault();


            if (cotaPortal == null)
                throw new ArgumentException("Não foi possível identificar a cota eSolution Portal");



            var cotasAccessCenter = (await _repositoryNHAccessCenter.FindBySql<CotaAccessCenterModel>($@"select
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
                                                                                            clip.Id as PessoaProviderId,
                                                                                            av.IdIntercambiadora,
                                                                                            av.Codigo as NumeroContrato,
                                                                                            Case when ti.Id in ({tipoImovelPadraoBlack}) then 'Black' else 'Default' end as PadraoDeCor
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
                                                                                            Left Join FrAtendimentoVenda av on av.Cota = c.Id and av.Status = 'A'
                                                                                            Left Outer Join TipoImovel ti on i.TipoImovel = ti.Id
                                                                                            where
                                                                                            (Lower(gctc.Nome) = '{cotaPortal.CotaNome!.ToLower()}' or 
                                                                                            Lower(gctc.Codigo) = '{cotaPortal.CotaCodigo!.ToLower()}')
                                                                                            and i.Numero = '{cotaPortal.NumeroImovel}' 
                                                                                            and emp.Id in ({empreendimentosAccessCenter})")).AsList();

            if (cotasAccessCenter == null || !cotasAccessCenter.Any())
            {
                throw new ArgumentException($"Erro: Não foi possível localizar a cota na AccessCenter vinculada a cota portal nome: {cotaPortal.CotaNome} do imóvel número: {cotaPortal.ImovelNumero} ");
            }

            var cotaAccessCenter = cotasAccessCenter.FirstOrDefault(a => a.ProprietarioNome!.RemoveAccents().TrimStart().Split(' ')[0].Contains(cotaPortal.ProprietarioNome!.RemoveAccents().TrimStart().Split(' ')[0], StringComparison.CurrentCultureIgnoreCase));

            if (cotaAccessCenter == null && cotasAccessCenter.Count() == 1)
                cotaAccessCenter = cotasAccessCenter.FirstOrDefault();

            if (cotaAccessCenter == null)
                throw new ArgumentException($"Erro: Não foi possível localizar a cota na AccessCenter vinculada a cota portal nome: {cotaPortal.CotaNome} do imóvel número: {cotaPortal.ImovelNumero} ");

            cotaAccessCenter!.Capacidade = cotaPortal.Capacidade;

            if (!aplicarPadraoBlack && cotaAccessCenter != null)
                cotaAccessCenter.PadraoDeCor = "Default";

            await GetDadosCompletosCotaAccessCenter(cotaAccessCenter);
            return cotaAccessCenter;
        }

        private async Task GetDadosCompletosCotaAccessCenter(CotaAccessCenterModel cotaAccessCenter)
        {
            var docRegistros = (await _repositoryNHAccessCenter.FindByHql<DocumentoRegistro>($@"From 
                                                                    DocumentoRegistro dr 
                                                                Where 
                                                                    dr.Pessoa = {cotaAccessCenter.PessoaProviderId} and 
                                                                    dr.DocumentoAlfanumerico is not null")).AsList();

            if (docRegistros.Any(b => b.TipoDocumentoRegistro == 3))
            {
                cotaAccessCenter.ProprietarioRG = docRegistros.First(a => a.TipoDocumentoRegistro == 3 && !string.IsNullOrEmpty(a.DocumentoAlfanumerico)).DocumentoAlfanumerico;
            }

            if (docRegistros.Any(b => (b.TipoDocumentoRegistro == 1 || b.TipoDocumentoRegistro == 2) && !string.IsNullOrEmpty(b.DocumentoAlfanumerico)))
            {
                cotaAccessCenter.ProprietarioCPF_CNPJ = docRegistros.First(a => (a.TipoDocumentoRegistro == 2 || a.TipoDocumentoRegistro == 1) && !string.IsNullOrEmpty(a.DocumentoAlfanumerico)).DocumentoAlfanumerico;
            }

            var enderecosPessoa = (await _repositoryNHAccessCenter.FindBySql<PessoaEnderecoModel>($@"
                                                                Select
                                                                    pe.Pessoa as PessoaId,
                                                                    c.Nome as CidadeNome,
                                                                    e.UF as EstadoSigla,
                                                                    e.Nome as EstadoNome,
                                                                    pe.Numero,
                                                                    pe.Logradouro,
                                                                    pe.Bairro,
                                                                    pe.Cep,
                                                                    pe.Complemento,
                                                                    Case when pe.Preferencial = 'S' then 1 else 0 end as Preferencial
                                                                From 
                                                                    PessoaEndereco pe
                                                                    Inner Join Cidade c on pe.Cidade = c.Id
                                                                    Inner Join Estado e on c.Estado = e.Id
                                                                Where
                                                                    pe.Pessoa = {cotaAccessCenter.PessoaProviderId} ")).AsList();

            if (enderecosPessoa.Any())
            {
                var enderecoUtilizar = enderecosPessoa.FirstOrDefault(a => a.Preferencial == Domain.Enumns.EnumSimNao.Sim) ??
                    enderecosPessoa.FirstOrDefault();

                if (enderecoUtilizar != null)
                {
                    cotaAccessCenter.LogradouroSocio = enderecoUtilizar.Logradouro;
                    cotaAccessCenter.BairroSocio = enderecoUtilizar.Bairro;
                    cotaAccessCenter.CidadeSocio = enderecoUtilizar.CidadeNome;
                    cotaAccessCenter.UfCidadeSocio = enderecoUtilizar.EstadoSigla;
                    cotaAccessCenter.CepEnderecoSocio = enderecoUtilizar.Cep;
                }
            }

            var telefonesPessoa = (await _repositoryNHAccessCenter.FindBySql<PessoaTelefoneModel>($@"
                                                                Select
                                                                   pt.Id,
                                                                   tt.Id as TipoTelefoneId,
                                                                   tt.Nome as TipoTelefoneNome,
                                                                   pt.Numero,
                                                                   Case when pt.Preferencial = 'S' then 1 else 0 end as Preferencial                                                                  
                                                                From 
                                                                    PessoaTelefone pt
                                                                    Inner Join TipoTelefone tt on pt.TipoTelefone = tt.Id
                                                                Where 
                                                                    pt.Pessoa = {cotaAccessCenter.PessoaProviderId} and 
                                                                    pt.Numero is not null and pt.Numero <> '0'")).AsList();


            var telefoneCelular = telefonesPessoa.FirstOrDefault(a => !string.IsNullOrEmpty(a.TipoTelefoneNome) && a.TipoTelefoneNome.Contains("celu", StringComparison.InvariantCultureIgnoreCase));
            if (telefoneCelular != null)
            {
                cotaAccessCenter.TelefoneCelular = telefoneCelular.Numero;
            }

            var telefoneFixo =
                telefonesPessoa.FirstOrDefault(a => !string.IsNullOrEmpty(a.TipoTelefoneNome) && a.TipoTelefoneNome.Contains("resid", StringComparison.InvariantCultureIgnoreCase)) ??
                telefonesPessoa.FirstOrDefault(a => !string.IsNullOrEmpty(a.TipoTelefoneNome) && !a.TipoTelefoneNome.Contains("celu", StringComparison.InvariantCultureIgnoreCase));

            if (telefoneFixo != null)
            {
                cotaAccessCenter.TelefoneFixo = telefoneFixo.Numero;
            }


        }

        public async Task<ResultModel<int>?> TrocarSemana_Esol(TrocaSemanaInputModel model)
        {
            ResultModel<int> retornoVinculo =
            new ResultModel<int>(-1)
            {
                Success = false,
                Errors = new List<string>() { "Falha na troca de semana" },
                Message = "Falha na troca de semana"
            };

            if (string.IsNullOrEmpty(model.TipoUso) && !string.IsNullOrEmpty(model.TipoUtilizacao))
            {
                model.TipoUso = model.TipoUtilizacao;
            }
            else if (!string.IsNullOrEmpty(model.TipoUso) && string.IsNullOrEmpty(model.TipoUtilizacao))
            {
                model.TipoUtilizacao = model.TipoUso;
            }

            try
            {
                _repositorySystem.BeginTransaction();

                if (!string.IsNullOrEmpty(model.Variacao) && model.Variacao == "-1")
                    model.Variacao = "Corrente";

                var loggedUser = await _repositorySystem.GetLoggedUser();
                var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {loggedUser.Value.userId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                if (usuario == null || usuario.Pessoa == null)
                    throw new FileNotFoundException("Não foi possível identificar o usuário logado para envio do código de confirmação para liberação da cota para POOL");

                var empresa = (await _repositorySystem.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e Where 1 = 1 Order by e.Id desc")).FirstOrDefault();

                if (empresa == null)
                    throw new FileNotFoundException("Não foi possível identificar a empresa logada no sistema.");

                var agendamento = (await _repositoryPortal.FindByHql<PeriodoCotaDisponibilidade>($"From PeriodoCotaDisponibilidade Where Id = {model.AgendamentoId.GetValueOrDefault()}")).FirstOrDefault();
                if (agendamento == null)
                    throw new FileNotFoundException($"Não foi foi encontrado o agendamento com o Id: {model.AgendamentoId}");


                CotaAccessCenterModel? cotaPortalUtilizar = await GetCotaPortalOuCotaAccessCenter(model.CotaAccessCenterId, false);
                if (cotaPortalUtilizar != null)
                {
                    model.IdIntercambiadora = cotaPortalUtilizar.IdIntercambiadora;
                }
                else throw new ArgumentException($"Erro: Não foi possível identificar a Cota no eSolution Portal, vinculada a Cota AccessCenter Id: {agendamento.Cota} informada.");


                var modelInput = new IncluirSemanaInputModel();

                if (cotaPortalUtilizar != null)
                {
                    modelInput.CotaId = cotaPortalUtilizar.CotaId;
                    modelInput.UhCondominioId = cotaPortalUtilizar.UhCondominio.GetValueOrDefault(0);
                    modelInput.CotaPortalNome = cotaPortalUtilizar.CotaNome;
                    modelInput.CotaPortalCodigo = cotaPortalUtilizar.CotaCodigo;
                    modelInput.GrupoCotaPortalNome = cotaPortalUtilizar.GrupoCotaNome;
                    modelInput.CotaProprietarioId = cotaPortalUtilizar.CotaProprietarioId;
                    modelInput.NumeroImovel = cotaPortalUtilizar.NumeroImovel;
                    modelInput.EmpresaPortalId = cotaPortalUtilizar.EmpresaPortalId;
                    modelInput.EmpresaAcId = cotaPortalUtilizar.EmpresaAcId;
                    modelInput.IdIntercambiadora = cotaPortalUtilizar.IdIntercambiadora;
                    modelInput.AdmAsUser = loggedUser.Value.isAdm;
                    modelInput.CotaAcId = cotaPortalUtilizar.CotaAcId;
                    modelInput.CotaAccessCenterId = cotaPortalUtilizar.CotaAcId;
                }
                else throw new ArgumentException($"Erro: Não foi possível identificar a Cota no eSolution Portal, vinculada a Cota AccessCenter Id: {modelInput.CotaAcId} informada.");

                await VerificarInadimplencia(modelInput, cotaPortalUtilizar, loggedUser, usuario);

                var result = new ResultModel<int?>();

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var liberarPoolUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:TrocarSemana");
                var fullUrl = baseUrl + liberarPoolUrl;
                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, model);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    _logger.LogInformation(resultMessage);

                    if (responseResult.IsSuccessStatusCode)
                    {
                        retornoVinculo = System.Text.Json.JsonSerializer.Deserialize<ResultModel<int>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (retornoVinculo != null)
                        {
                            retornoVinculo.Status = (int)HttpStatusCode.OK;
                            retornoVinculo.Success = true;
                            retornoVinculo.Data = retornoVinculo.Data;
                        }
                    }
                    else
                    {
                        retornoVinculo = System.Text.Json.JsonSerializer.Deserialize<ResultModel<int>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (retornoVinculo != null)
                        {
                            retornoVinculo.Status = (int)HttpStatusCode.NotFound;
                            retornoVinculo.Success = false;
                        }
                    }

                }

                if (retornoVinculo != null && retornoVinculo.Data > 0 && retornoVinculo.Data != model.AgendamentoId.GetValueOrDefault())
                {
                    var agendamentos = (await _repositoryPortal.FindBySql<SemanaDisponibilidadeModel>(@$"Select
                        sd.PeriodoCotaDisponibilidade as Id,
                        s.Id as SemanaId,
                        ts.Nome as TipoSemanaNome,
                        s.DataInicial as SemanaDataInicial,
                        s.DataFinal as SemanaDataFinal
                        From
                        TipoSemana ts
                        Inner Join Semana s on s.TipoSemana = ts.Id
                        Inner Join SemanaDisponibilidade sd on sd.Semana = s.Id
                        Where
                        sd.PeriodoCotaDisponibilidade in ({model.AgendamentoId.GetValueOrDefault()},{retornoVinculo.Data})")).AsList();

                    if (agendamentos.Count() == 2)
                    {

                        var agendamentoAnterior = agendamentos.FirstOrDefault(a => a.Id == model.AgendamentoId.GetValueOrDefault());
                        var novoAgendamento = agendamentos.FirstOrDefault(a => a.Id == retornoVinculo.Data);
                        var descricaoCompleta = $"Agendamento anterior: {agendamentoAnterior?.Id} SemanaId: {agendamentoAnterior?.SemanaId} Inicio: {agendamentoAnterior?.SemanaDataInicial:dd/MM/yyyy} Final: {agendamentoAnterior?.SemanaDataFinal:dd/MM/yyyy} ";
                        descricaoCompleta += $"{Environment.NewLine}Novo agendamento: {novoAgendamento?.Id} SemanaId: {novoAgendamento?.SemanaId} Inicio: {novoAgendamento?.SemanaDataInicial:dd/MM/yyyy} Final: {novoAgendamento?.SemanaDataFinal:dd/MM/yyyy} ";

                        var historicoTroca = new HistoricoTrocaDeSemana()
                        {
                            AgendamentoAnteriorId = model.AgendamentoId.GetValueOrDefault(),
                            NovoAgendamentoId = retornoVinculo?.Data,
                            Descricao = descricaoCompleta,
                            DataHoraCriacao = DateTime.Now,
                            UsuarioCriacao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null,
                            Empresa = empresa
                        };

                        await _repositorySystem.Save(historicoTroca);
                    }
                }

                var commitResult = await _repositorySystem.CommitAsync();
                if (!commitResult.executed)
                    throw commitResult.exception ?? new Exception("Não foi possível realizar a operação");

                return retornoVinculo;

            }
            catch (Exception err)
            {
                _repositorySystem.Rollback();
                if (retornoVinculo != null)
                {
                    retornoVinculo.Message = err.Message;
                    retornoVinculo.Errors = new List<string>() { err.Message };
                    retornoVinculo.Success = false;
                }
                throw;
            }
        }

        private async Task<List<ClientesInadimplentes>?> Inadimplente(List<PessoaSistemaXProviderModel> usuariosProvider)
        {
            //if (Debugger.IsAttached) return null;
            List<ClientesInadimplentes> pessoasComPendenciaFinanceiras = new List<ClientesInadimplentes>();

            foreach (var item in usuariosProvider)
            {
                var clienteInadimplenteRetorno = await _cacheStore.GetAsync<ClientesInadimplentes>($"ClienteInadimplenteId_{item.PessoaProvider}", 2, _repositorySystem.CancellationToken);
                if (clienteInadimplenteRetorno != null)
                {
                    pessoasComPendenciaFinanceiras.Add(clienteInadimplenteRetorno);
                    break;
                }

                var tiposContasReceberConsiderar = _configuration.GetValue<string>("TiposContasReceberIdsConsiderarNoPortal");
                var strQueryAdicional = "";
                if (!string.IsNullOrEmpty(tiposContasReceberConsiderar))
                    strQueryAdicional += $" and tcr.Id in ({tiposContasReceberConsiderar}) ";

                var tiposContaReceberConsiderarBaixados = _configuration.GetValue<string>("TiposContasReceberIdsConsiderarBaixados");
                if (!string.IsNullOrEmpty(tiposContaReceberConsiderarBaixados))
                    strQueryAdicional += $" and tcr.Id not in ({tiposContaReceberConsiderarBaixados}) ";

                var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();

                if (parametrosSistema != null && !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds))
                    strQueryAdicional += $" AND cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds}) ";


                var pendenciasFinanceiras =
                                            (await _repositoryNHAccessCenter.FindBySql<ClientesInadimplentes>($@"Select 
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
                                        cli.Pessoa = {item.PessoaProvider} and 
                                        crp.Vencimento <= :dataCorte and 
                                        crp.Status = 'P' and 
                                        crp.SaldoPendente > 0 {strQueryAdicional} 
                                    Group by cli.Pessoa, pes.Nome, Case when pes.Tipo = 'F' then pes.cpf else pes.Cnpj end, pes.Email",
                                            new Parameter("dataCorte", DateTime.Today.AddDays(-5).Date))).FirstOrDefault();


                if (pendenciasFinanceiras != null && pendenciasFinanceiras.TotalInadimplencia.GetValueOrDefault(0) > 0)
                {
                    await _cacheStore.AddAsync($"ClienteInadimplenteId_{item.PessoaProvider}", pendenciasFinanceiras, DateTimeOffset.Now.AddMinutes(5), 2, _repositorySystem.CancellationToken);

                    pessoasComPendenciaFinanceiras.Add(pendenciasFinanceiras);
                    break;
                }
            }

            return pessoasComPendenciaFinanceiras;
        }

        public async Task<List<ClientesInadimplentes>> Inadimplentes_Esol(List<int>? pessoasPesquisar = null)
        {
            var tiposContasReceberConsiderar = _configuration.GetValue<string>("TiposContasReceberIdsConsiderarNoPortal");
            var strQueryAdicional = "";
            if (!string.IsNullOrEmpty(tiposContasReceberConsiderar))
                strQueryAdicional += $" and tcr.Id in ({tiposContasReceberConsiderar}) ";

            var tiposContaReceberConsiderarBaixados = _configuration.GetValue<string>("TiposContasReceberIdsConsiderarBaixados");
            if (!string.IsNullOrEmpty(tiposContaReceberConsiderarBaixados))
                strQueryAdicional += $" and tcr.Id not in ({tiposContaReceberConsiderarBaixados}) ";

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();

            if (parametrosSistema != null && !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds))
                strQueryAdicional += $" AND cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds}) ";

            List<ClientesInadimplentes> pessoasComPendenciaFinanceiras = new List<ClientesInadimplentes>();
            if (pessoasPesquisar != null && pessoasPesquisar.Any())
            {
                var pessoasSubList = Helper.Sublists<int>(pessoasPesquisar, 1000);

                foreach (var item in pessoasSubList)
                {
                    var pendenciasFinanceiras =
                                        (await _repositoryNHAccessCenter.FindBySql<ClientesInadimplentes>($@"Select 
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

                var itens = await _cacheStore.GetAsync<List<ClientesInadimplentes>>(CACHE_CLIENTES_INADIMPLENTES_KEY, 10, _repositorySystem.CancellationToken);
                if (itens != null && itens.Any()) return itens;

                var pendenciasFinanceiras =
                                        (await _repositoryNHAccessCenter.FindBySql<ClientesInadimplentes>($@"Select 
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

                await _cacheStore.AddAsync(CACHE_CLIENTES_INADIMPLENTES_KEY, pessoasComPendenciaFinanceiras, DateTimeOffset.Now.AddMinutes(10), 10, _repositorySystem.CancellationToken);
            }

            return pessoasComPendenciaFinanceiras;
        }


        public async Task<ResultModel<int>?> IncluirSemana_Esol(IncluirSemanaInputModel model)
        {

            CotaAccessCenterModel? cotaPortalUtilizar = await GetCotaPortalOuCotaAccessCenter(model.CotaId ?? model.CotaAcId);
            if (cotaPortalUtilizar != null)
            {
                model.CotaId = cotaPortalUtilizar.CotaId;
                model.UhCondominioId = cotaPortalUtilizar.UhCondominio.GetValueOrDefault(0);
                model.CotaPortalNome = cotaPortalUtilizar.CotaNome;
                model.CotaPortalCodigo = cotaPortalUtilizar.CotaCodigo;
                model.GrupoCotaPortalNome = cotaPortalUtilizar.GrupoCotaNome;
                model.CotaProprietarioId = cotaPortalUtilizar.CotaProprietarioId;
                model.NumeroImovel = cotaPortalUtilizar.NumeroImovel;
                model.EmpresaPortalId = cotaPortalUtilizar.EmpresaPortalId;
                model.EmpresaAcId = cotaPortalUtilizar.EmpresaAcId;
                model.IdIntercambiadora = cotaPortalUtilizar.IdIntercambiadora;
            }
            else throw new ArgumentException($"Erro: Não foi possível identificar a Cota no eSolution Portal, vinculada a Cota AccessCenter Id: {model.CotaId} informada.");

            if (string.IsNullOrEmpty(model.TipoUso) && !string.IsNullOrEmpty(model.TipoUtilizacao))
            {
                model.TipoUso = model.TipoUtilizacao;
            }
            else if (!string.IsNullOrEmpty(model.TipoUso) && string.IsNullOrEmpty(model.TipoUtilizacao))
            {
                model.TipoUtilizacao = model.TipoUso;
            }

            ResultModel<int> retornoVinculo =
            new ResultModel<int>(-1)
            {
                Success = false,
                Errors = new List<string>() { "Falha na inclusão de agendamento" },
                Message = "Falha na inclusão de agendamento"
            };


            try
            {
                _repositorySystem.BeginTransaction();

                var loggedUser = await _repositorySystem.GetLoggedUser();
                var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {loggedUser.Value.userId}and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                if (usuario == null || usuario.Pessoa == null)
                    throw new FileNotFoundException("Não foi possível identificar o usuário logado para envio do código de confirmação para liberação da cota para POOL");

                await VerificarInadimplencia(model, cotaPortalUtilizar, loggedUser, usuario);

                var empresa = (await _repositorySystem.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e Where 1 = 1 Order by e.Id desc")).FirstOrDefault();
                if (empresa == null)
                    throw new FileNotFoundException("Não foi possível identificar a empresa logada no sistema.");


                var result = new ResultModel<int?>();

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var liberarPoolUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:IncluirSemana");
                var fullUrl = baseUrl + liberarPoolUrl;
                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, model);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    _logger.LogInformation(resultMessage);

                    if (responseResult.IsSuccessStatusCode)
                    {
                        retornoVinculo = System.Text.Json.JsonSerializer.Deserialize<ResultModel<int>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (retornoVinculo != null)
                        {
                            retornoVinculo.Status = (int)HttpStatusCode.OK;
                            retornoVinculo.Success = true;
                            retornoVinculo.Data = retornoVinculo.Data;
                        }
                    }
                    else
                    {
                        retornoVinculo = System.Text.Json.JsonSerializer.Deserialize<ResultModel<int>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (retornoVinculo != null)
                        {
                            retornoVinculo.Status = (int)HttpStatusCode.NotFound;
                            retornoVinculo.Success = false;
                        }
                    }

                }

                if (retornoVinculo != null && retornoVinculo.Data > 0)
                {
                    var agendamentos = (await _repositoryPortal.FindBySql<SemanaDisponibilidadeModel>(@$"Select
                        sd.PeriodoCotaDisponibilidade as Id,
                        s.Id as SemanaId,
                        ts.Nome as TipoSemanaNome,
                        s.DataInicial as SemanaDataInicial,
                        s.DataFinal as SemanaDataFinal
                        From
                        TipoSemana ts
                        Inner Join Semana s on s.TipoSemana = ts.Id
                        Inner Join SemanaDisponibilidade sd on sd.Semana = s.Id
                        Where
                        sd.PeriodoCotaDisponibilidade in ({retornoVinculo.Data})")).AsList();

                    if (agendamentos.Count() == 1)
                    {

                        var novoAgendamento = agendamentos.FirstOrDefault(a => a.Id == retornoVinculo.Data);
                        var descricaoCompleta = $"Inclusão de novo agendamento: {novoAgendamento?.Id} SemanaId: {novoAgendamento?.SemanaId} Inicio: {novoAgendamento?.SemanaDataInicial:dd/MM/yyyy} Final: {novoAgendamento?.SemanaDataFinal:dd/MM/yyyy} ";

                        var historicoTroca = new HistoricoTrocaDeSemana()
                        {
                            AgendamentoAnteriorId = null,
                            NovoAgendamentoId = retornoVinculo?.Data,
                            Descricao = descricaoCompleta,
                            DataHoraCriacao = DateTime.Now,
                            UsuarioCriacao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null,
                            Empresa = empresa
                        };

                        await _repositorySystem.Save(historicoTroca);
                    }
                }

                var commitResult = await _repositorySystem.CommitAsync();
                if (!commitResult.executed)
                    throw commitResult.exception ?? new Exception("Não foi possível realizar a operação");

                return retornoVinculo;

            }
            catch (Exception err)
            {
                _repositorySystem.Rollback();
                if (retornoVinculo != null)
                {
                    retornoVinculo.Message = err.Message;
                    retornoVinculo.Errors = new List<string>() { err.Message };
                    retornoVinculo.Success = false;
                }
                throw;
            }
        }

        private async Task VerificarInadimplencia(IncluirSemanaInputModel model, CotaAccessCenterModel cotaPortalUtilizar,
            (string userId, string providerKeyUser, string companyId, bool isAdm)? loggedUser, Domain.Entities.Core.Sistema.Usuario usuario)
        {
            var usuarioProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(usuario.Id);

            if (!loggedUser.Value.isAdm)
            {
                if (usuarioProvider != null && usuarioProvider.Any())
                {
                    var propCache = await _serviceBase.GetContratos(usuarioProvider.Select(b=> int.Parse(b.PessoaProvider!)).AsList());
                    if (!string.IsNullOrEmpty(cotaPortalUtilizar.CotaNome))
                    {
                        if (propCache != null && propCache.Any(b => !string.IsNullOrEmpty(b.GrupoCotaTipoCotaNome) && b.GrupoCotaTipoCotaNome!.RemoveAccents() == cotaPortalUtilizar.CotaNome.RemoveAccents() && b.frAtendimentoStatusCrcModels.Any(b => (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && b.AtendimentoStatusCrcStatus == "A")))
                        {
                            throw new ArgumentException("Não foi possível realizar a operação favor procure a Central de Atendimento aos Clientes.");
                        }
                    }
                    else
                    {
                        if (propCache != null && propCache.Any(b => b.frAtendimentoStatusCrcModels.Any(b => (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && b.AtendimentoStatusCrcStatus == "A")))
                        {
                            throw new ArgumentException("Não foi possível realizar a operação favor procure a Central de Atendimento aos Clientes.");
                        }
                    }

                    var resultInadimplente = await Inadimplente(usuarioProvider);
                    if (resultInadimplente != null)
                        throw new ArgumentException("Não foi possível realizar a operação favor procure a Central de Atendimento aos Clientes.");

                }
            }
            else if (model.AdmAsUser.GetValueOrDefault(false) && model.CotaAcId.GetValueOrDefault(0) > 0)
            {
                if (_configuration.GetValue<bool>("ImpedirReservarComInadimplenciaViaVisaoAdm", false))
                {
                    var cotaAcModel = await GetCotaPortalOuCotaAccessCenter(model.CotaAcId, true);
                    if (cotaAcModel != null)
                    {
                        var resultInadimplente = await Inadimplente(new List<PessoaSistemaXProviderModel>() { new PessoaSistemaXProviderModel() { PessoaProvider = cotaAcModel.PessoaProviderId.ToString() } });
                        if (resultInadimplente != null)
                            throw new ArgumentException("Não foi possível realizar a operação favor procure a Central de Atendimento aos Clientes.");

                    }
                }
            }
        }

        public async Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_Esol(GetHtmlValuesModel model, string codigoEnviadoAoCliente, DateTime? dataAssinatura, bool espanhol = false)
        {

            try
            {
                CotaAccessCenterModel? cotaPortalUtilizar = await GetCotaAccessCenterPelosDadosPortal(model);

                if (cotaPortalUtilizar == null)
                    throw new ArgumentException("Erro: Não foi possível encontrar a cota vinculada, necessária para preenchimento automático do contrato.");


                var dadosAssinatura = !string.IsNullOrEmpty(codigoEnviadoAoCliente) && codigoEnviadoAoCliente.TrimStart().StartsWith("LS", StringComparison.InvariantCultureIgnoreCase) ? $"CÓDIGO RECEBIDO NO EMAIL E CONFIRMADO PELO CLIENTE: {codigoEnviadoAoCliente}" : "";
                if (espanhol)
                    dadosAssinatura = !string.IsNullOrEmpty(codigoEnviadoAoCliente) && codigoEnviadoAoCliente.TrimStart().StartsWith("LS", StringComparison.InvariantCultureIgnoreCase) ? $"CÓDIGO RECIBIDO EN CORREO ELECTRÓNICO Y CONFIRMADO POR EL CLIENTE: {codigoEnviadoAoCliente}" : "";

                List<KeyValueModel> listResult = new List<KeyValueModel>()
                {

                    new KeyValueModel
                    {
                        Key = "[NOME_SOCIO]",
                        Value = $"{cotaPortalUtilizar.ProprietarioNome}"
                    },
                    new KeyValueModel
                    {
                        Key = "[RG_SOCIO]",
                        Value = $"{cotaPortalUtilizar.ProprietarioRG}"
                    },
                    new KeyValueModel
                    {
                        Key = "[CPF_CNPJ_SOCIO]",
                        Value = $"{cotaPortalUtilizar.ProprietarioCPF_CNPJ ?? cotaPortalUtilizar.CnpjProprietario ?? cotaPortalUtilizar.CpfProprietario}"
                    },
                    new KeyValueModel
                    {
                        Key = "[ENDERECO_SOCIO]",
                        Value = $"{cotaPortalUtilizar.LogradouroSocio}"
                    },
                    new KeyValueModel
                    {
                        Key = "[BAIRRO_SOCIO]",
                        Value = $"{cotaPortalUtilizar.BairroSocio}"
                    },
                    new KeyValueModel
                    {
                        Key = "[CIDADE_SOCIO]",
                        Value = $"{cotaPortalUtilizar.CidadeSocio}"
                    },
                    new KeyValueModel
                    {
                        Key = "[UF_SOCIO]",
                        Value = $"{cotaPortalUtilizar.UfCidadeSocio}"
                    },
                    new KeyValueModel
                    {
                        Key = "[CEP_SOCIO]",
                        Value = $"{cotaPortalUtilizar.CepEnderecoSocio}"
                    },
                    new KeyValueModel
                    {
                        Key = "[TELEFONE_FIXO_SOCIO]",
                        Value = $"{cotaPortalUtilizar.TelefoneFixo}"
                    },
                    new KeyValueModel
                    {
                        Key = "[CELULAR_SOCIO]",
                        Value = $"{cotaPortalUtilizar.TelefoneCelular}"
                    },
                    new KeyValueModel
                    {
                        Key = "[EMAIL_SOCIO]",
                        Value = $"{cotaPortalUtilizar.EmailProprietario}"
                    },
                    new KeyValueModel
                    {
                        Key = "[NOME_REPRESENTANTE_LEGAL]",
                        Value = ""
                    },
                    new KeyValueModel
                    {
                        Key = "[UNIDADE_AUTONOMA]",
                        Value = $"{cotaPortalUtilizar.NumeroImovel}"
                    },
                    new KeyValueModel
                    {
                        Key = "[MATRICULA_UNIDADE]",
                        Value = $"{cotaPortalUtilizar.NumeroContrato}"
                    },
                    new KeyValueModel
                    {
                        Key = "[CAPACIDADE_OCUPACIONAL]",
                        Value = $"{cotaPortalUtilizar.Capacidade}"
                    },
                    new KeyValueModel
                    {
                        Key = "[DADOSBANCARIOS]",
                        Value = $"{cotaPortalUtilizar.DadosBancariosRecebimentoRendimentos}"
                    },
                    new KeyValueModel
                    {
                        Key = "[DIA]",
                        Value = $"{dataAssinatura.GetValueOrDefault(DateTime.Today).Day.ToString().PadLeft(2,'0')}"
                    },
                    new KeyValueModel
                    {
                        Key = "[MES]",
                        Value = $"{dataAssinatura.GetValueOrDefault(DateTime.Today).ToString("MMMM")}"
                    },
                    new KeyValueModel
                    {
                        Key = "[ANO]",
                        Value = $"{dataAssinatura.GetValueOrDefault(DateTime.Today).ToString("yyyy")}"
                    },
                    new KeyValueModel
                    {
                        Key = "[DADOS_ASSINATURA]",
                        Value = dadosAssinatura
                    }

                };


                return listResult;
            }
            catch (Exception err)
            {
                throw new ArgumentException(err.Message);
            }

        }

        public async Task<DownloadContratoResultModel?> DownloadContratoSCP_Esol(int cotaId)
        {
            throw new NotImplementedException();
            //try
            //{
            //    _repositorySystem.BeginTransaction();

            //    var loggedUser = await _repositorySystem.GetLoggedUser();
            //    if (loggedUser == null)
            //        throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            //    ContratoVinculoSCPEsol? contrato = null;

            //    if (!loggedUser.Value.isAdm)
            //    {

            //        var pessoaVinculadaSistema = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), CommunicationProviderName);
            //        if (pessoaVinculadaSistema == null)
            //            throw new ArgumentException($"Não foi encontrada pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            //        if (string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) || !Helper.IsNumeric(pessoaVinculadaSistema.PessoaProvider))
            //            throw new ArgumentException($"Não foi encontrada pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");


            //        contrato = (await _repositorySystem.FindByHql<ContratoVinculoSCPEsol>(@$"From 
            //                            ContratoVinculoSCPEsol c 
            //                            Inner Join Fetch c.Empresa e 
            //                        Where 
            //                            c.CotaAccessCenterId = {cotaId} and
            //                            c.PessoaLegadoId = {pessoaVinculadaSistema.PessoaProvider}
            //                        Order by c.Id desc ")).FirstOrDefault();


            //    }
            //    else
            //    {
            //        contrato = (await _repositorySystem.FindByHql<ContratoVinculoSCPEsol>(@$"From 
            //                            ContratoVinculoSCPEsol c 
            //                            Inner Join Fetch c.Empresa e 
            //                        Where 
            //                            c.CotaAccessCenterId = {cotaId}
            //                        Order by c.Id desc ")).FirstOrDefault();

            //    }

            //    if (contrato == null)
            //        throw new ArgumentException($"Não foi encontrado contrato SCP vinculado a Cota AccessCenterCotaId: {cotaId}");

            //    var clienteContaBancarias = (await _repositoryNHAccessCenter.FindByHql<ClienteContaBancaria>(@$"
            //                                                                        From 
            //                                                                            ClienteContaBancaria ccb
            //                                                                        Where 
            //                                                                            Exists(Select cli.Id From Cliente cli Where cli.Pessoa = {contrato.PessoaLegadoId} and cli.Id = ccb.Cliente)")).AsList();

            //    bool emitirEmEspanhol = false;
            //    var pessoa = (await _repositoryNHAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = {contrato.PessoaLegadoId}")).FirstOrDefault();
            //    if (pessoa != null && pessoa.Estrangeiro == "S")
            //    {
            //        emitirEmEspanhol = true;
            //    }

            //    if (emitirEmEspanhol)
            //    {
            //        if (!string.IsNullOrEmpty(contrato.DocumentoFull) && (!contrato.DocumentoFull.Contains("INSTRUMENTO PRIVADO PARA LA CONSTITUCIÓN") ||
            //            !contrato.DocumentoFull.Contains("CÓDIGO RECIBIDO EN CORREO ELECTRÓNICO Y CONFIRMADO POR EL CLIENTE:")))
            //        {

            //            var confirmacaoLiberacaoPool =
            //                (await _repositorySystem.FindByHql<ConfirmacaoLiberacaoPool>($@"From 
            //                                                            ConfirmacaoLiberacaoPool c 
            //                                                            Inner Join Fetch c.Empresa e 
            //                                                        Where 
            //                                                            c.CodigoEnviadoAoCliente = '{contrato.CodigoVerificacao}' and 
            //                                                            c.LiberacaoConfirmada = 1 and 
            //                                                            (c.LiberacaoDiretaPeloCliente = 'Sim' or c.LiberacaoDiretaPeloCliente  = '1')")).FirstOrDefault();

            //            if (confirmacaoLiberacaoPool == null)
            //                throw new ArgumentException($"Não foi possível encontrar a confirmação de liberação para o POOL relacionada ao contrato: {contrato.Id}");


            //            var contaBancariaUtilizar = clienteContaBancarias.LastOrDefault(a => a.Preferencial == "S") ??
            //                clienteContaBancarias.LastOrDefault();


            //            await GerarContratoExecute(confirmacaoLiberacaoPool, contaBancariaUtilizar, contrato, contrato.Empresa!.Id!, emitirEmEspanhol);

            //            if (!string.IsNullOrEmpty(contrato.DocumentoFull))
            //            {
            //                var pathGeracao = _configuration.GetValue<string>($"CertidoesConfig:GeracaoPdfContratoPath");
            //                if (string.IsNullOrEmpty(pathGeracao))
            //                    throw new FileNotFoundException("Não foi encontrada a path para gravação  do contrato, necessário para liberação da semana para POOL");

            //                if (!Directory.Exists(pathGeracao))
            //                    Directory.CreateDirectory(pathGeracao);

            //                string fileName = $"{contrato.CotaAccessCenterId}_{contrato.CotaPortalId}_{contrato.UhCondominioId}.pdf";
            //                var fullPath = Path.Combine(pathGeracao, fileName);
            //                if (File.Exists(fullPath))
            //                    File.Delete(fullPath);

            //                var launchOptions = new LaunchOptions
            //                {
            //                    Headless = true
            //                };

            //                await new BrowserFetcher().DownloadAsync();
            //                using var browser = await Puppeteer.LaunchAsync(launchOptions);
            //                using var page = await browser.NewPageAsync();

            //                await page.SetContentAsync(contrato.DocumentoFull);

            //                await page.PdfAsync(fullPath);

            //                contrato.PdfPath = fullPath;

            //                await _repositorySystem.Save(contrato);

            //                await _repositorySystem.CommitAsync();

            //                return new DownloadContratoResultModel()
            //                {
            //                    Path = contrato.PdfPath
            //                };

            //            }
            //        }
            //        else
            //        {
            //            if (!string.IsNullOrEmpty(contrato.PdfPath) &&
            //                string.IsNullOrEmpty(contrato.DocumentoFull))
            //            {
            //                if (File.Exists(contrato.PdfPath))
            //                {
            //                    return new DownloadContratoResultModel()
            //                    {
            //                        Path = contrato.PdfPath
            //                    };
            //                }
            //            }

            //            string pathItemAnterior = contrato != null && !string.IsNullOrEmpty(contrato?.PdfPath) ? contrato.PdfPath : "";


            //            if (!string.IsNullOrEmpty(contrato?.DocumentoFull))
            //            {
            //                var pathGeracao = _configuration.GetValue<string>($"CertidoesConfig:GeracaoPdfContratoPath");
            //                if (string.IsNullOrEmpty(pathGeracao))
            //                    throw new FileNotFoundException("Não foi encontrada a path para gravação do contrato, necessária para liberação da semana para POOL");



            //                if (!Directory.Exists(pathGeracao))
            //                    Directory.CreateDirectory(pathGeracao);

            //                string fileName = $"{contrato.CotaAccessCenterId}_{contrato.CotaPortalId}_{contrato.UhCondominioId}.pdf";
            //                var fullPath = Path.Combine(pathGeracao, fileName);
            //                if (File.Exists(fullPath))
            //                    File.Delete(fullPath);

            //                var launchOptions = new LaunchOptions
            //                {
            //                    Headless = true // Define se o navegador será exibido ou não
            //                };

            //                // Inicializar o PuppeteerSharp
            //                await new BrowserFetcher().DownloadAsync();
            //                using var browser = await Puppeteer.LaunchAsync(launchOptions);
            //                using var page = await browser.NewPageAsync();

            //                // Carregar o conteúdo HTML na página
            //                await page.SetContentAsync(contrato.DocumentoFull);

            //                // Gerar o PDF
            //                await page.PdfAsync(fullPath);

            //                contrato.PdfPath = fullPath;

            //                await _repositorySystem.Save(contrato);

            //                await _repositorySystem.CommitAsync();

            //                //if (!string.IsNullOrEmpty(pathItemAnterior))
            //                //{
            //                //    if (File.Exists(pathItemAnterior))
            //                //    {
            //                //        try
            //                //        {
            //                //            File.Delete(pathItemAnterior);
            //                //        }
            //                //        catch
            //                //        {

            //                //        }
            //                //    }
            //                //}

            //                return new DownloadContratoResultModel()
            //                {
            //                    Path = contrato.PdfPath
            //                };

            //            }
            //        }
            //    }
            //    else
            //    {

            //        if ((clienteContaBancarias != null &&
            //            clienteContaBancarias.Any() &&
            //            (contrato.DataHoraCriacao <= new DateTime(2025, 7, 18) ||
            //            (!string.IsNullOrEmpty(contrato.DocumentoFull) &&
            //            (contrato.DocumentoFull.Contains("<td>[DADOSBANCARIOS]</td>", StringComparison.InvariantCultureIgnoreCase) ||
            //            !contrato.DocumentoFull.Contains("<td>Banco:", StringComparison.InvariantCultureIgnoreCase))))) ||
            //            (!string.IsNullOrEmpty(contrato.DocumentoFull) && !contrato.DocumentoFull.Contains("CÓDIGO RECEBIDO NO EMAIL E CONFIRMADO PELO CLIENTE:")))
            //        {

            //            var confirmacaoLiberacaoPool =
            //                (await _repositorySystem.FindByHql<ConfirmacaoLiberacaoPool>($@"From 
            //                                                            ConfirmacaoLiberacaoPool c 
            //                                                            Inner Join Fetch c.Empresa e 
            //                                                        Where 
            //                                                            c.CodigoEnviadoAoCliente = '{contrato.CodigoVerificacao}' and 
            //                                                            c.LiberacaoConfirmada = 1 and 
            //                                                            (c.LiberacaoDiretaPeloCliente = 'Sim' or c.LiberacaoDiretaPeloCliente  = '1')")).FirstOrDefault();

            //            if (confirmacaoLiberacaoPool == null)
            //                throw new ArgumentException($"Não foi possível encontrar a confirmação de liberação para o POOL relacionada ao contrato: {contrato.Id}");


            //            var contaBancariaUtilizar = clienteContaBancarias.LastOrDefault(a => a.Preferencial == "S") ??
            //                clienteContaBancarias.LastOrDefault();


            //            await GerarContratoExecute(confirmacaoLiberacaoPool, contaBancariaUtilizar, contrato, contrato.Empresa!.Id!, false);

            //            if (!string.IsNullOrEmpty(contrato.DocumentoFull))
            //            {
            //                var pathGeracao = _configuration.GetValue<string>($"CertidoesConfig:GeracaoPdfContratoPath");
            //                if (string.IsNullOrEmpty(pathGeracao))
            //                    throw new FileNotFoundException("Não foi encontrada a path para geração do contrato, necessária para liberação da semana para POOL");

            //                if (!Directory.Exists(pathGeracao))
            //                    Directory.CreateDirectory(pathGeracao);

            //                string fileName = $"{contrato.CotaAccessCenterId}_{contrato.CotaPortalId}_{contrato.UhCondominioId}.pdf";
            //                var fullPath = Path.Combine(pathGeracao, fileName);
            //                if (File.Exists(fullPath))
            //                    File.Delete(fullPath);

            //                var launchOptions = new LaunchOptions
            //                {
            //                    Headless = true
            //                };

            //                await new BrowserFetcher().DownloadAsync();
            //                using var browser = await Puppeteer.LaunchAsync(launchOptions);
            //                using var page = await browser.NewPageAsync();

            //                await page.SetContentAsync(contrato.DocumentoFull);

            //                await page.PdfAsync(fullPath);

            //                contrato.PdfPath = fullPath;

            //                await _repositorySystem.Save(contrato);

            //                await _repositorySystem.CommitAsync();

            //                return new DownloadContratoResultModel()
            //                {
            //                    Path = contrato.PdfPath
            //                };

            //            }
            //        }
            //        else
            //        {
            //            if (!string.IsNullOrEmpty(contrato.PdfPath) &&
            //                string.IsNullOrEmpty(contrato.DocumentoFull))
            //            {
            //                if (File.Exists(contrato.PdfPath))
            //                {
            //                    return new DownloadContratoResultModel()
            //                    {
            //                        Path = contrato.PdfPath
            //                    };
            //                }
            //            }

            //            string pathItemAnterior = contrato != null && !string.IsNullOrEmpty(contrato?.PdfPath) ? contrato.PdfPath : "";


            //            if (!string.IsNullOrEmpty(contrato?.DocumentoFull))
            //            {
            //                var pathGeracao = _configuration.GetValue<string>($"CertidoesConfig:GeracaoPdfContratoPath");
            //                if (string.IsNullOrEmpty(pathGeracao))
            //                    throw new FileNotFoundException("Não foi encontrada a path para geração do contrato, necessária para liberação da semana para POOL");



            //                if (!Directory.Exists(pathGeracao))
            //                    Directory.CreateDirectory(pathGeracao);

            //                string fileName = $"{contrato.CotaAccessCenterId}_{contrato.CotaPortalId}_{contrato.UhCondominioId}.pdf";
            //                var fullPath = Path.Combine(pathGeracao, fileName);
            //                if (File.Exists(fullPath))
            //                    File.Delete(fullPath);

            //                var launchOptions = new LaunchOptions
            //                {
            //                    Headless = true // Define se o navegador será exibido ou não
            //                };

            //                // Inicializar o PuppeteerSharp
            //                await new BrowserFetcher().DownloadAsync();
            //                using var browser = await Puppeteer.LaunchAsync(launchOptions);
            //                using var page = await browser.NewPageAsync();

            //                // Carregar o conteúdo HTML na página
            //                await page.SetContentAsync(contrato.DocumentoFull);

            //                // Gerar o PDF
            //                await page.PdfAsync(fullPath);

            //                contrato.PdfPath = fullPath;

            //                await _repositorySystem.Save(contrato);

            //                await _repositorySystem.CommitAsync();

            //                //if (!string.IsNullOrEmpty(pathItemAnterior))
            //                //{
            //                //    if (File.Exists(pathItemAnterior))
            //                //    {
            //                //        try
            //                //        {
            //                //            File.Delete(pathItemAnterior);
            //                //        }
            //                //        catch
            //                //        {

            //                //        }
            //                //    }
            //                //}

            //                return new DownloadContratoResultModel()
            //                {
            //                    Path = contrato.PdfPath
            //                };

            //            }
            //        }
            //    }

            //}
            //catch (Exception)
            //{
            //    _repositorySystem.Rollback();
            //    throw;
            //}

            //return default;
        }

        #region Forma via API
        //public async Task<ResultModel<DadosImpressaoVoucherResultModel>?> GetDadosImpressaoVoucher(string agendamentoId)
        //{
        //if (string.IsNullOrEmpty(agendamentoId))
        //    throw new ArgumentException("O agendamentoId deve ser informado.");

        //var loggedUser = await _repositorySystem.GetLoggedUser();
        //if (loggedUser != null && !loggedUser.Value.isAdm)
        //{
        //    var usuarioProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(int.Parse(loggedUser.Value.userId));
        //    if (usuarioProvider != null)
        //    {
        //        var resultInadimplente = await Inadimplente(usuarioProvider);
        //        if (resultInadimplente != null)
        //            throw new ArgumentException("Não foi possível baixar o voucher da reserva, favor procure a Central de Atendimento - (PF)");
        //    }
        //}

        //_logger.LogInformation($"{DateTime.Now} - Buscando reservas da API");
        //var result = new ResultModel<DadosImpressaoVoucherResultModel>();
        //try
        //{

        //    var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
        //    var consultarReservaUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:GetDadosImpressaoVoucherUrl");
        //    if (!string.IsNullOrEmpty(consultarReservaUrl))
        //    {
        //        var fullUrl = $"{baseUrl}{consultarReservaUrl}{agendamentoId}";
        //        var token = await _serviceBase.getToken();

        //        using (HttpClient client = new HttpClient())
        //        {
        //            client.BaseAddress = new Uri(fullUrl);
        //            client.DefaultRequestHeaders.Clear();
        //            client.DefaultRequestHeaders.Add("accept", "application/json");
        //            client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
        //            HttpResponseMessage responseResult = await client.GetAsync(fullUrl);

        //            string resultMessage = await responseResult.Content.ReadAsStringAsync();

        //            if (responseResult.IsSuccessStatusCode)
        //            {
        //                result = System.Text.Json.JsonSerializer.Deserialize<ResultModel<DadosImpressaoVoucherResultModel>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        //                if (result != null)
        //                    result.Status = (int)HttpStatusCode.OK;
        //            }
        //            else
        //            {
        //                result.Status = (int)HttpStatusCode.NotFound;
        //                result.Errors = new List<string>() { $"Erro: {responseResult}" };

        //            }
        //        }
        //    }
        //    else throw new ArgumentException($"Não foi encontrada a configuração de url: 'GetDadosImpressaoVoucherUrl'");

        //}
        //catch (HttpRequestException err)
        //{
        //    _logger.LogError(err, err.Message);
        //    if (result != null)
        //    {
        //        result.Errors.Add($"Erro: {err.Message}");
        //        result.Status = (int)HttpStatusCode.InternalServerError;
        //    }
        //}
        //catch (Exception err)
        //{
        //    _logger.LogError(err, err.Message);
        //    if (result != null)
        //    {
        //        result.Errors.Add($"Erro: {err.Message}");
        //        result.Status = (int)HttpStatusCode.InternalServerError;
        //    }
        //}

        //if (result != null && (result.Errors == null || !result.Errors.Any()))
        //{
        //    var resultItem = result.Data;
        //    if (resultItem != null)
        //    {
        //        var dadosCota = await GetDadosContrato(null, resultItem.CotaPortalId.GetValueOrDefault(), resultItem.UhCondominioId.GetValueOrDefault());
        //        if (dadosCota != null)
        //            result.Data!.Contrato = dadosCota.NumeroContrato;
        //    }
        //}

        //return result;
        //} 
        #endregion


        public async Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_Esol(string agendamentoId)
        {

            var sb = new StringBuilder(@$"Select
                                            Cast(r.Id as varchar) as NumeroReserva,
                                            r.Id as NumReserva,
                                            r.PeriodoCotaDisponibilidade  as AgendamentoId,
                                            procp.Nome as Cliente,
                                            pc.Nome as HospedePrincipal,
                                            pcd.TipoDisponibilizacao,
                                            Case 
                                            when pcd.TipoDisponibilizacao = 'U' then 'Uso Próprio'
                                            when pcd.TipoDisponibilizacao = 'C' then 'Uso Convidado'
                                            when pcd.TipoDisponibilizacao = 'P' then 'Pool'
                                            when pcd.TipoDisponibilizacao = 'I' then 'Uso/Intercambiadora'
                                            else pcd.TipoDisponibilizacao end as TipoUso,
                                            Format(Coalesce(r.Checkin,r.CheckinPrevisao),'dd/MM/yyyy') as DataChegada,
                                            Case 
	                                            when Format(Coalesce(Coalesce(r.Checkin,r.CheckinPrevisao),h.HorarioLimiteCheckin),'HH:mm') != '00:00' 
	                                            then Format(Coalesce(Coalesce(r.Checkin,r.CheckinPrevisao),h.HorarioLimiteCheckin),'HH:mm') else '15:00' end 
                                            as HoraChegada,
                                            Format(Coalesce(r.Checkout,r.CheckoutPrevisao),'dd/MM/yyyy') as DataPartida,
                                            Case 
	                                            when Format(Coalesce(Coalesce(r.Checkout,r.CheckoutPrevisao),h.HorarioLimiteCheckout),'HH:mm') != '00:00' 
	                                            then Format(Coalesce(Coalesce(r.Checkout,r.CheckoutPrevisao),h.HorarioLimiteCheckout),'HH:mm') else '10:00' end
                                            as HoraPartida,
                                            (Coalesce(r.QuantidadeAdulto,0)+Coalesce(r.QuantidadeCrianca1,0)+Coalesce(r.QuantidadeCrianca2,0)) as QuantidadePaxPorFaixaEtaria,
                                            tuh.Nome as TipoApartamento,
                                            Case 
                                            When r.TipoUtilizacao = 'U' or pcd.TipoDisponibilizacao = 'U' then 'UP'
                                            When r.TipoUtilizacao = 'C' or pcd.TipoDisponibilizacao = 'C' then 'UC'
                                            When r.TipoUtilizacao = 'I' or pcd.TipoDisponibilizacao = 'I' then 'I'
                                            end as TipoUtilizacao,
                                            pcd.UhCondominio as UhCondominioId,
                                            cp.Cota as CotaPortalId,
                                            h.Nome as NomeHotel,
                                            Coalesce(r.QuantidadeAdulto,0) as QuantidadeAdulto,
                                            Coalesce(r.QuantidadeCrianca1,0) as QuantidadeCrianca1,
                                            Coalesce(r.QuantidadeCrianca2,0) as QuantidadeCrianca2,
                                            ct.Nome as CotaNome,
                                            uc.Numero as UhCondominioNumero
                                            From 
                                            Reserva r
                                            Inner Join PeriodoCotaDisponibilidade pcd on r.PeriodoCotaDisponibilidade = pcd.Id
                                            Left Outer Join ReservaCliente rc on rc.Reserva = r.Id and rc.Principal = 'S'
                                            Left Outer Join Cliente rcc on rc.Cliente = rcc.Id
                                            Left Outer Join Pessoa pc on rcc.Pessoa = pc.Id
                                            Left Outer Join TipoUh tuh on r.TipoUh = tuh.Id
                                            Left Outer Join Hotel h on r.Hotel = h.Id
                                            Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota and cp.UsuarioExclusao is null and cp.DataHoraExclusao is null
                                            Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null and pro.UsuarioExclusao is null
                                            Inner Join Cliente proca on pro.Cliente = proca.Id
                                            Inner Join Pessoa procp on proca.Pessoa = procp.Id
                                            Left Outer Join TipoHospede th on r.TipoHospede = th.Id
                                            Inner Join UhCondominio uc on uc.Id = cp.UhCondominio
                                            Inner Join Cota ct on ct.Id = cp.Cota
                                            Where 
                                            pro.Id = (Select Max(pro1.Id) From Proprietario pro1 Where pro1.CotaProprietario = cp.Id and pro1.Cliente = proca.Id  and pro1.DataHoraExclusao is null and pro1.UsuarioExclusao is null)
                                            and pcd.Id = {agendamentoId} 
                                            and r.Status <> 'CL' ");


            var dados = (await _repositoryPortal.FindBySql<DadosImpressaoVoucherResultModel>(sb.ToString())).FirstOrDefault();
            if (dados != null)
            {
                dados.QuantidadePax += " PAX";
                if (dados.QuantidadeAdulto.GetValueOrDefault(0) > 0)
                    dados.QuantidadePaxPorFaixaEtaria = dados.QuantidadeAdulto.GetValueOrDefault(0) > 1 ? $"{dados.QuantidadeAdulto.GetValueOrDefault(0)} adultos;" :
                         $"{dados.QuantidadeAdulto.GetValueOrDefault(0)} adulto;";

                if ((dados.QuantidadeCrianca1.GetValueOrDefault(0) + dados.QuantidadeCrianca2.GetValueOrDefault(0)) > 0)
                {
                    dados.QuantidadePaxPorFaixaEtaria += (dados.QuantidadeCrianca1.GetValueOrDefault(0) + dados.QuantidadeCrianca2.GetValueOrDefault(0)) > 1 ? $"{(dados.QuantidadeCrianca1.GetValueOrDefault(0) + dados.QuantidadeCrianca2.GetValueOrDefault(0))} crianças;" :
                        $"{(dados.QuantidadeCrianca1.GetValueOrDefault(0) + dados.QuantidadeCrianca2.GetValueOrDefault(0))} criança;";
                }

                if (!string.IsNullOrEmpty(dados.TipoUso) && !dados.TipoUso.Contains("convidado", StringComparison.InvariantCultureIgnoreCase))
                {
                    dados.Observacao = "INCLUÍDO ACESSO AO BLUE PARK";
                }
                else dados.Observacao = "NÃO ESTÁ INCLUÍDO O ACESSO AO BLUE PARK";

                dados.Acomodacao = "MASTER";

                var hospedes = (await _repositoryPortal.FindBySql<VoucherHospedeModel>($@"Select
                                                                                pc.Nome,
                                                                                pc.Cpf as Documento
                                                                                From 
                                                                                ReservaCliente rc
                                                                                Inner Join Cliente rccl on rc.Cliente = rccl.Id
                                                                                Inner Join Pessoa pc on rccl.Pessoa = pc.Id
                                                                                Left Outer Join Reserva r on rc.Reserva = r.Id
                                                                                Left Outer Join PeriodoCotaDisponibilidade pcd on r.PeriodoCotaDisponibilidade = pcd.Id
                                                                                Inner Join CotaProprietario cp on cp.UhCondominio = pcd.UhCondominio and cp.Cota = pcd.Cota and cp.UsuarioExclusao is null and cp.DataHoraExclusao is null
                                                                                Inner Join Proprietario pro on pro.CotaProprietario = cp.Id and pro.DataHoraExclusao is null and pro.UsuarioExclusao is null
                                                                                Inner Join Cliente proca on pro.Cliente = proca.Id
                                                                                Inner Join Pessoa procp on proca.Pessoa = procp.Id
                                                                                Where 
                                                                                rc.Reserva = {dados.NumeroReserva}")).AsList();

                dados.Hospedes = hospedes ?? new List<VoucherHospedeModel>();
            }

            return dados;
        }

        public async Task<ResultModel<int>?> TrocarTipoUso_Esol(TrocaSemanaInputModel model)
        {
            ResultModel<int> retornoVinculo =
            new ResultModel<int>(-1)
            {
                Success = false,
                Errors = new List<string>() { "Falha na troca de semana" },
                Message = "Falha na troca de semana"
            };

            if (model.TrocaDeTipoDeUso != true)
                throw new ArgumentException("O modelo enviado não é para troca de tipo de uso.");

            if (string.IsNullOrEmpty(model.TipoUso) && !string.IsNullOrEmpty(model.TipoUtilizacao))
            {
                model.TipoUso = model.TipoUtilizacao;
            }
            else if (!string.IsNullOrEmpty(model.TipoUso) && string.IsNullOrEmpty(model.TipoUtilizacao))
            {
                model.TipoUtilizacao = model.TipoUso;
            }

            try
            {
                _repositorySystem.BeginTransaction();

                if (!string.IsNullOrEmpty(model.Variacao) && model.Variacao == "-1")
                    model.Variacao = "Corrente";

                var loggedUser = await _repositorySystem.GetLoggedUser();
                var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Inner Join Fetch u.Pessoa p Where u.Id = {loggedUser.Value.userId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                if (usuario == null || usuario.Pessoa == null)
                    throw new FileNotFoundException("Não foi possível identificar o usuário logado para envio do código de confirmação para liberação da cota para POOL");

                var empresa = (await _repositorySystem.FindByHql<Domain.Entities.Core.Framework.Empresa>("From Empresa e Where 1 = 1 Order by e.Id desc")).FirstOrDefault();

                if (empresa == null)
                    throw new FileNotFoundException("Não foi possível identificar a empresa logada no sistema.");

                var agendamento = (await _repositoryPortal.FindByHql<PeriodoCotaDisponibilidade>($"From PeriodoCotaDisponibilidade Where Id = {model.AgendamentoId.GetValueOrDefault()}")).FirstOrDefault();
                if (agendamento == null)
                    throw new FileNotFoundException($"Não foi foi encontrado o agendamento com o Id: {model.AgendamentoId}");


                CotaAccessCenterModel? cotaPortalUtilizar = await GetCotaPortalOuCotaAccessCenter(model.CotaAccessCenterId, false);
                if (cotaPortalUtilizar != null)
                {
                    model.IdIntercambiadora = cotaPortalUtilizar.IdIntercambiadora;
                }
                else throw new ArgumentException($"Erro: Não foi possível identificar a Cota no eSolution Portal, vinculada a Cota AccessCenter Id: {agendamento.Cota} informada.");


                var modelInput = new IncluirSemanaInputModel();

                if (cotaPortalUtilizar != null)
                {
                    modelInput.CotaId = cotaPortalUtilizar.CotaId;
                    modelInput.UhCondominioId = cotaPortalUtilizar.UhCondominio.GetValueOrDefault(0);
                    modelInput.CotaPortalNome = cotaPortalUtilizar.CotaNome;
                    modelInput.CotaPortalCodigo = cotaPortalUtilizar.CotaCodigo;
                    modelInput.GrupoCotaPortalNome = cotaPortalUtilizar.GrupoCotaNome;
                    modelInput.CotaProprietarioId = cotaPortalUtilizar.CotaProprietarioId;
                    modelInput.NumeroImovel = cotaPortalUtilizar.NumeroImovel;
                    modelInput.EmpresaPortalId = cotaPortalUtilizar.EmpresaPortalId;
                    modelInput.EmpresaAcId = cotaPortalUtilizar.EmpresaAcId;
                    modelInput.IdIntercambiadora = cotaPortalUtilizar.IdIntercambiadora;
                    modelInput.CotaAccessCenterId = cotaPortalUtilizar.CotaAcId;
                    modelInput.CotaAcId = cotaPortalUtilizar.CotaAcId;
                    modelInput.AdmAsUser = loggedUser.Value.isAdm;
                }
                else throw new ArgumentException($"Erro: Não foi possível identificar a Cota no eSolution Portal, vinculada a Cota AccessCenter Id: {modelInput.CotaId} informada.");


                await VerificarInadimplencia(modelInput, cotaPortalUtilizar, loggedUser, usuario);


                var result = new ResultModel<int?>();

                var baseUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:BaseUrl");
                var liberarPoolUrl = _configuration.GetValue<string>("ReservasEsolutionApiConfig:TrocarSemana");
                var fullUrl = baseUrl + liberarPoolUrl;
                var token = await _serviceBase.getToken();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(fullUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("accept", "application/json");
                    client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                    HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, model);

                    string resultMessage = await responseResult.Content.ReadAsStringAsync();

                    _logger.LogInformation(resultMessage);

                    if (responseResult.IsSuccessStatusCode)
                    {
                        retornoVinculo = System.Text.Json.JsonSerializer.Deserialize<ResultModel<int>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (retornoVinculo != null)
                        {
                            retornoVinculo.Status = (int)HttpStatusCode.OK;
                            retornoVinculo.Success = true;
                            retornoVinculo.Data = retornoVinculo.Data;
                        }
                    }
                    else
                    {
                        retornoVinculo = System.Text.Json.JsonSerializer.Deserialize<ResultModel<int>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (retornoVinculo != null)
                        {
                            retornoVinculo.Status = (int)HttpStatusCode.NotFound;
                            retornoVinculo.Success = false;
                        }
                    }

                }

                if (retornoVinculo != null && retornoVinculo.Data > 0 && retornoVinculo.Data != model.AgendamentoId.GetValueOrDefault())
                {
                    var agendamentos = (await _repositoryPortal.FindBySql<SemanaDisponibilidadeModel>(@$"Select
                        sd.PeriodoCotaDisponibilidade as Id,
                        s.Id as SemanaId,
                        ts.Nome as TipoSemanaNome,
                        s.DataInicial as SemanaDataInicial,
                        s.DataFinal as SemanaDataFinal
                        From
                        TipoSemana ts
                        Inner Join Semana s on s.Semana = ts.Id
                        Inner Join SemanaDisponibilidade sd on sd.Semana = s.Id
                        Where
                        sd.PeriodoCotaDisponibilidade in ({model.AgendamentoId.GetValueOrDefault()},{retornoVinculo.Data})")).AsList();

                    if (agendamentos.Count() == 2)
                    {

                        var agendamentoAnterior = agendamentos.FirstOrDefault(a => a.Id == model.AgendamentoId.GetValueOrDefault());
                        var novoAgendamento = agendamentos.FirstOrDefault(a => a.Id == retornoVinculo.Data);
                        var descricaoCompleta = $"Agendamento anterior: {agendamentoAnterior?.Id} SemanaId: {agendamentoAnterior?.SemanaId} Inicio: {agendamentoAnterior?.SemanaDataInicial:dd/MM/yyyy} Final: {agendamentoAnterior?.SemanaDataFinal:dd/MM/yyyy} ";
                        descricaoCompleta += $"{Environment.NewLine}Novo agendamento: {novoAgendamento?.Id} SemanaId: {novoAgendamento?.SemanaId} Inicio: {novoAgendamento?.SemanaDataInicial:dd/MM/yyyy} Final: {novoAgendamento?.SemanaDataFinal:dd/MM/yyyy} ";

                        var historicoTroca = new HistoricoTrocaDeSemana()
                        {
                            AgendamentoAnteriorId = model.AgendamentoId.GetValueOrDefault(),
                            NovoAgendamentoId = retornoVinculo?.Data,
                            Descricao = descricaoCompleta,
                            DataHoraCriacao = DateTime.Now,
                            UsuarioCriacao = loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId) ? Convert.ToInt32(loggedUser.Value.userId) : null,
                            Empresa = empresa
                        };

                        await _repositorySystem.Save(historicoTroca);
                    }
                }

                var commitResult = await _repositorySystem.CommitAsync();
                if (!commitResult.executed)
                    throw commitResult.exception ?? new Exception("Não foi possível realizar a operação");

                return retornoVinculo;

            }
            catch (Exception err)
            {
                _repositorySystem.Rollback();
                if (retornoVinculo != null)
                {
                    retornoVinculo.Message = err.Message;
                    retornoVinculo.Errors = new List<string>() { err.Message };
                    retornoVinculo.Success = false;
                }
                throw;
            }
        }

        public async Task<List<Models.Empreendimento.StatusCrcModel>?> ConsultarStatusCrc_Esol()
        {
            var statusCache = await _cacheStore.GetAsync<List<Models.Empreendimento.StatusCrcModel>>("StatusCrc_", 2, _repositoryNHAccessCenter.CancellationToken);
            if (statusCache != null && statusCache.Any())
                return statusCache.Where(b => Helper.IsNumeric(b.Codigo!)).OrderBy(a => int.Parse(a.Codigo!)).AsList();

            var statusRetorno = (await _repositoryNHAccessCenter.FindBySql<Models.Empreendimento.StatusCrcModel>(@$"Select
                        s.Id,
                        s.Codigo,
                        s.Nome
                        From
                        frstatuscrc s
                        Where 1 = 1")).AsList();

            if (statusRetorno != null && statusRetorno.Any())
                await _cacheStore.AddAsync("StatusCrc_", statusRetorno, DateTimeOffset.Now.AddHours(1), 2, _repositoryNHAccessCenter.CancellationToken);

            return statusRetorno != null && statusRetorno.Any() ? statusRetorno.Where(b => Helper.IsNumeric(b.Codigo!)).OrderBy(a => int.Parse(a.Codigo!)).AsList() : default;

        }

        private async Task<List<StatusCrcContratoModel>?> GetStatusCrcPorTipoStatusIds(List<int> statusCrcIds)
        {

            List<StatusCrcContratoModel>? status = new();
            if (!statusCrcIds.Any()) return default;

            var statusCache = await _cacheStore.GetAsync<List<StatusCrcContratoModel>>($"FrAtendimentoStatusCrcModel_{string.Join("_", statusCrcIds.OrderBy(b => b))}", 2, _repositoryNHAccessCenter.CancellationToken);
            if (statusCache != null && statusCache.Any())
                return statusCache;

            var sqlStatusCrc = new StringBuilder(@$"
                                            SELECT
                                            avcrc.DataHoraCriacao AS AtendimentoStatusCrcData,
                                            avcrc.Id AS AtendimentoStatusCrcId,
                                            avcrc.Status AS AtendimentoStatusCrcStatus,
                                            av.Id AS FrAtendimentoVendaId,
                                            st.Id as FrStatusCrcId,
                                            st.Codigo AS CodigoStatus,
                                            st.Nome AS NomeStatus,
                                            st.BloquearEmissaoBoletos,
                                            st.Status AS FrCrcStatus,
                                            st.BloquearUtilizacaoCota,
                                            st.BloquearCobrancaPagRec,
                                            p1.Nome as NomeTitular,
                                            Case when p1.Tipo = 'F' then p1.CPF else p1.CNPJ end as Cpf_Cnpj_Titular,
                                            p2.Nome as NomeCoCessionario,
                                            Case when p2.Tipo = 'F' then p2.CPF else p2.CNPJ end as Cpf_Cnpj_CoCessionario,
                                            i.Numero as ImovelNumero,
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
                                            st.Id in ({string.Join(",", statusCrcIds)})");

            var result = (await _repositoryNHAccessCenter.FindBySql<StatusCrcContratoModel>(sqlStatusCrc.ToString())).AsList();

            if (result != null && result.Any())
            {
                await _cacheStore.AddAsync($"FrAtendimentoStatusCrcModel_{string.Join("_", statusCrcIds.OrderBy(b => b))}", result, DateTimeOffset.Now.AddMinutes(10), 2, _repositoryNHAccessCenter.CancellationToken);
            }

            return result;
        }

        public async Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(DateTime checkInDate, bool simulacao = false)
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
                    return (await _repositoryPortal.FindBySql<ReservaInfo>(sb.ToString(), parameter)).AsList();
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
                    var resultado = (await _repositoryPortal.FindBySql<ReservaInfo>(sb.ToString(), parameter)).AsList();

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

                        resultado = (await _repositoryPortal.FindBySql<ReservaInfo>(sbRange.ToString(), parameterInicial, parameterFinal)).AsList();
                    }

                    return resultado;
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public async Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_Esol(int ano, int? uhCondominioId = null, int? cotaPortalId = null)
        {
            List<Parameter> parameters = new List<Parameter>();


            var sb = new StringBuilder($@"Select
                                          Sum(tcts.Quantidade) as QtdeSemanasDireitoUso,
                                          Coalesce(util.QtdeReservas,0) as QtdeReservas,
                                          uc.Id as UhCondominio,
                                          uc.Numero as UhCondominioNumero,
                                          c.Id as CotaId,
                                          c.Nome as CotaNome,
                                          c.PrioridadeAgendamento,
                                          cp.TipoContrato,
                                          pcli.Id as PessoaClienteId,
                                          pcli.Nome as PessoaClienteNome,
                                          pri.DataInicial as DataInicialAgendamento,
                                          pri.DataFinal as DataFinalAgendamento
                                          from 
                                          CotaProprietario cp
                                          inner join Cota c on cp.Cota = c.Id
                                          Inner join UhCondominio uc on cp.UhCondominio = uc.Id
                                          inner join TipoContrato tc on cp.TipoContrato = tc.Id and tc.UsuarioExclusao is null and tc.DataHoraExclusao is null
                                          inner join TipoContratoTipoSemana tcts on tc.Id = tcts.TipoContrato And tcts.DataHoraExclusao is null And tcts.UsuarioExclusao is null
                                          inner join TipoSemana ts on tcts.TipoSemana = ts.Id And ts.DataHoraExclusao is null And ts.UsuarioExclusao is null
                                          inner join Proprietario pr on cp.Id = pr.CotaProprietario and pr.DataHoraExclusao is null and pr.UsuarioExclusao is null
                                          inner join Cliente cli on pr.Cliente = cli.Id
                                          inner join Pessoa pcli on cli.Pessoa = pcli.Id
                                          Left Outer Join (Select  
					                                        pa.Id as PrioridadeAgendamento, 
					                                        paa.Ano,
					                                        paa.DataInicial, 
				                                            paa.DataFinal 
				                                           from 
					                                        PrioridadeAgendamento pa
					                                        Inner Join PrioridadeAgendamentoAno paa on paa.PrioridadeAgendamento = pa.id and paa.DataHoraExclusao is null and paa.UsuarioExclusao is null
					                                        ) pri on pri.PrioridadeAgendamento = c.PrioridadeAgendamento and pri.ano = tcts.Ano
                                          LEFT Outer Join (
                                          Select 
	                                        Year(pcd.dataInicial) as Ano,
	                                        pcd.Cota,
	                                        pcd.UhCondominio,
	                                        COUNT(1) as QtdeReservas
                                          From
                                            PeriodoCotaDisponibilidade pcd
                                          Where 
                                            pcd.DataHoraExclusao is null and
	                                        pcd.UsuarioExclusao is null
                                          Group by 
  	                                        Year(pcd.dataInicial),
	                                        pcd.Cota,
	                                        pcd.UhCondominio
                                          ) util on util.Cota = c.Id And util.UhCondominio = uc.Id and util.Ano = tcts.Ano
                                          where
                                          cp.DataHoraExclusao Is null 
                                          and cp.UsuarioExclusao is null 
                                          and tcts.Ano = :ano ");


            if (ano == 0)
                ano = DateTime.Now.Year;

            parameters.Add(new Parameter("ano", ano));

            if (uhCondominioId.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine(" and uc.Id = :uhCondominioId ");
                parameters.Add(new Parameter("uhCondominioId", uhCondominioId.GetValueOrDefault()));
            }

            if (cotaPortalId.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine(" and c.Id = :cotaPortalId ");
                parameters.Add(new Parameter("cotaPortalId", cotaPortalId.GetValueOrDefault()));
            }


            sb.AppendLine(@"group by 
                                          uc.Id,
                                          uc.Numero,
                                          c.Id,
                                          c.Nome,
                                          c.PrioridadeAgendamento,
                                          cp.TipoContrato,
                                          pcli.Id,
                                          pcli.Nome,
                                          pri.DataInicial,
                                          pri.DataFinal,
                                          Coalesce(util.QtdeReservas,0) ");

            if (uhCondominioId.GetValueOrDefault(0) == 0 && cotaPortalId.GetValueOrDefault(0) == 0)
            {
                var resultCache = await _cacheStore.GetAsync<List<PosicaoAgendamentoViewModel>>($"PosicaoAgendamentoAno_{ano}", 2, _repositoryPortal.CancellationToken);
                if (resultCache != null && resultCache.Any())
                    return resultCache;
            }


            var resultado = (await _repositoryPortal.FindBySql<PosicaoAgendamentoViewModel>(sb.ToString(),null, parameters.ToArray())).AsList();

            if (resultado != null && resultado.Any())
            {
                resultado = resultado.Where(a => a.QtdeReservas < a.QtdeSemanasDireitoUso).AsList();
            }

            if (resultado != null && resultado.Any() && uhCondominioId.GetValueOrDefault(0) == 0 && cotaPortalId.GetValueOrDefault(0) == 0)
            {
                await _cacheStore.AddAsync($"PosicaoAgendamentoAno_{ano}", resultado, DateTimeOffset.Now.AddMinutes(2), 2, _repositoryPortal.CancellationToken);
            }

            return resultado ?? new List<PosicaoAgendamentoViewModel>();

        }

        public DadosContratoModel? GetContrato_Esol(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
        {
            var contrato = contratos.FirstOrDefault(c => c.GrupoCotaTipoCotaNome == dadosReserva.CotaNome && c.NumeroImovel == dadosReserva.UhCondominioNumero);
            if (contrato != null)
            {
                dadosReserva.Contrato = contrato.NumeroContrato;
                dadosReserva.NomeCliente = contrato.PessoaTitular1Nome;
            }
            return contrato;
        }

        #region CM Methods
        public Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis_CM(SearchImovelModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios_CM(SearchProprietarioModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts_CM(SearchMyContractsModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<int>?> SalvarReservaEmAgendamento_CM(CriacaoReservaAgendamentoInputModel modelReserva)
        {
            throw new NotImplementedException();
        }

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais_CM(ReservasMultiPropriedadeSearchModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos_CM(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId_CM(string agendamento)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId_CM(string agendamento)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> CancelarReservaAgendamento_CM(CancelamentoReservaAgendamentoModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento_CM(CancelamentoReservaAgendamentoModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva_CM(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<ReservaForEditModel>?> EditarReserva_CM(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios_CM(InventarioSearchModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> RetirarSemanaPool_CM(AgendamentoInventarioModel modelAgendamentoPool)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> LiberarSemanaPool_CM(LiberacaoAgendamentoInputModel modelAgendamentoPool)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<bool>?> LiberarMinhaSemanaPool_CM(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GerarCodigoVerificacaoLiberacaoPool_CM(int agendamentoId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidarCodigo_CM(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos_CM(int agendamentoId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel_CM(DispobilidadeSearchModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<int>?> TrocarSemana_CM(TrocaSemanaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<int>?> TrocarTipoUso_CM(TrocaSemanaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel<int>?> IncluirSemana_CM(IncluirSemanaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP_CM(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false)
        {
            throw new NotImplementedException();
        }

        public Task<DownloadContratoResultModel?> DownloadContratoSCP_CM(int cotaId)
        {
            throw new NotImplementedException();
        }

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher_CM(string agendamentoId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<StatusCrcModel>?> ConsultarStatusCrc_CM()
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

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync_CM(DateTime checkInDate, bool simulacao = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<ClientesInadimplentes>> Inadimplentes_CM(List<int>? pessoasPesquisar = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync_CM(int ano, int? uhCondominioId = null, int? cotaPortalId = null)
        {
            throw new NotImplementedException();
        }

        public DadosContratoModel? GetContrato_CM(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
        {
            var contrato = contratos.FirstOrDefault();
            if (contrato != null)
            {
                dadosReserva.Contrato = contrato.NumeroContrato;
                dadosReserva.NomeCliente = contrato.PessoaTitular1Nome;
            }
            return contrato;
        }
        #endregion

        #region Default Methods (delegates to Esol)
        public Task<(int pageNumber, int lastPageNumber, List<ImovelSimplificadoModel> imoveis)?> GetImoveis(SearchImovelModel searchModel)
            => GetImoveis_Esol(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> proprietarios)?> GetProprietarios(SearchProprietarioModel searchModel)
            => GetProprietarios_Esol(searchModel);

        public Task<(int pageNumber, int lastPageNumber, List<ProprietarioSimplificadoModel> contratos)?> GetMyContracts(SearchMyContractsModel searchModel)
            => GetMyContracts_Esol(searchModel);

        public Task<ResultModel<int>?> SalvarReservaEmAgendamento(CriacaoReservaAgendamentoInputModel modelReserva)
            => SalvarReservaEmAgendamento_Esol(modelReserva);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarAgendamentosGerais(ReservasMultiPropriedadeSearchModel model)
            => ConsultarAgendamentosGerais_Esol(model);

        public Task<ResultWithPaginationModel<List<SemanaModel>>?> ConsultarMeusAgendamentos(PeriodoCotaDisponibilidadeUsuarioSearchModel model)
            => ConsultarMeusAgendamentos_Esol(model);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarReservaByAgendamentoId(string agendamento)
            => ConsultarReservaByAgendamentoId_Esol(agendamento);

        public Task<ResultModel<List<ReservaModel>>?> ConsultarMinhasReservaByAgendamentoId(string agendamento)
            => ConsultarMinhasReservaByAgendamentoId_Esol(agendamento);

        public Task<ResultModel<bool>?> CancelarReservaAgendamento(CancelamentoReservaAgendamentoModel model)
            => CancelarReservaAgendamento_Esol(model);

        public Task<ResultModel<bool>?> CancelarMinhaReservaAgendamento(CancelamentoReservaAgendamentoModel model)
            => CancelarMinhaReservaAgendamento_Esol(model);

        public Task<ResultModel<ReservaForEditModel>?> EditarMinhaReserva(int id)
            => EditarMinhaReserva_Esol(id);

        public Task<ResultModel<ReservaForEditModel>?> EditarReserva(int id)
            => EditarReserva_Esol(id);

        public Task<ResultModel<List<InventarioModel>>?> ConsultarInventarios(InventarioSearchModel searchModel)
            => ConsultarInventarios_Esol(searchModel);

        public Task<ResultModel<bool>?> RetirarSemanaPool(AgendamentoInventarioModel modelAgendamentoPool)
            => RetirarSemanaPool_Esol(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarSemanaPool(LiberacaoAgendamentoInputModel modelAgendamentoPool)
            => LiberarSemanaPool_Esol(modelAgendamentoPool);

        public Task<ResultModel<bool>?> LiberarMinhaSemanaPool(LiberacaoMeuAgendamentoInputModel modelAgendamentoPool)
            => LiberarMinhaSemanaPool_Esol(modelAgendamentoPool);

        public Task<bool> GerarCodigoVerificacaoLiberacaoPool(int agendamentoId)
            => GerarCodigoVerificacaoLiberacaoPool_Esol(agendamentoId);

        public Task<bool> ValidarCodigo(int agendamentoId, string codigoVerificacao, bool? controlarTransacao = true)
            => ValidarCodigo_Esol(agendamentoId, codigoVerificacao, controlarTransacao);

        public Task<ResultModel<List<AgendamentoHistoryModel>>?> ConsultarHistoricos(int agendamentoId)
            => ConsultarHistoricos_Esol(agendamentoId);

        public Task<ResultModel<List<SemanaDisponibilidadeModel>>?> ConsultarDisponibilidadeCompativel(DispobilidadeSearchModel searchModel)
            => ConsultarDisponibilidadeCompativel_Esol(searchModel);

        public Task<ResultModel<int>?> TrocarSemana(TrocaSemanaInputModel model)
            => TrocarSemana_Esol(model);

        public Task<ResultModel<int>?> TrocarTipoUso(TrocaSemanaInputModel model)
            => TrocarTipoUso_Esol(model);

        public Task<ResultModel<int>?> IncluirSemana(IncluirSemanaInputModel model)
            => IncluirSemana_Esol(model);

        public Task<List<KeyValueModel>> GetKeyValueListFromContratoSCP(GetHtmlValuesModel model, string codigoVerificacao, DateTime? dataAssinatura, bool espanhol = false)
            => GetKeyValueListFromContratoSCP_Esol(model, codigoVerificacao, dataAssinatura, espanhol);

        public Task<DownloadContratoResultModel?> DownloadContratoSCP(int cotaId)
            => DownloadContratoSCP_Esol(cotaId);

        public Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(string agendamentoId)
            => GetDadosImpressaoVoucher_Esol(agendamentoId);

        public Task<List<StatusCrcModel>?> ConsultarStatusCrc()
            => ConsultarStatusCrc_Esol();

        public Task<List<ReservaInfo>> GetReservasWithCheckInDateMultiPropriedadeAsync(DateTime checkInDate, bool simulacao = false)
            => GetReservasWithCheckInDateMultiPropriedadeAsync_Esol(checkInDate, simulacao);

        public Task<List<ClientesInadimplentes>> Inadimplentes(List<int>? pessoasPesquisar = null)
            => Inadimplentes_Esol(pessoasPesquisar);

        public Task<List<PosicaoAgendamentoViewModel>> GetPosicaoAgendamentoAnoAsync(int ano, int? uhCondominioId = null, int? cotaPortalId = null)
            => GetPosicaoAgendamentoAnoAsync_Esol(ano, uhCondominioId, cotaPortalId);

        public DadosContratoModel? GetContrato(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
            => GetContrato_Esol(dadosReserva, contratos);
        #endregion
    }
}
