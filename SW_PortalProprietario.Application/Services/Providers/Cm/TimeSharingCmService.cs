using AccessCenterDomain.AccessCenter.Fractional;
using CMDomain.Entities;
using CMDomain.Models.Pessoa;
using Dapper;
using EsolutionPortalDomain.ReservasApiModels.Hotel;
using FluentNHibernate.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NHibernate.Dialect.Schema;
using NHibernate.Linq.Functions;
using NHibernate.Util;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TimeSharing;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Entities.Core.TimeSharing;
using SW_Utils.Functions;
using System.Formats.Asn1;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using HospedeInputModel = SW_PortalProprietario.Application.Models.TimeSharing.HospedeInputModel;
using Parameter = SW_Utils.Auxiliar.Parameter;

namespace SW_PortalProprietario.Application.Services.Providers.Cm
{
    public class TimeSharingCmService : ITimeSharingProviderService
    {
        private readonly IRepositoryNHCm _repository;
        private readonly ILogger<TimeSharingCmService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;
        private readonly ICityService _cidadeService;
        private readonly IRepositoryNH _repositorySystem;
        private readonly IRegraPaxFreeService _regraPaxFreeService;
        private readonly ICommunicationProvider _communicationProvider;
        private const string CONTRATOS_CACHE_KEY = "contratosTimeSharingCache";
        private const string ASSOCIACAO_RCI_CACHE_KEY = "contratosAssociacaoRCICache";
        private readonly ICacheStore _cacheStore;

        public TimeSharingCmService(IRepositoryNHCm repository,
            ILogger<TimeSharingCmService> logger,
            IConfiguration configuration,
            IServiceBase serviceBase,
            ICommunicationProvider communicationProvider,
            ICacheStore cacheStore,
            IRepositoryNH repositorySystem,
            ICityService cidadeService,
            IRegraPaxFreeService regraPaxFreeService
            )
        {
            _logger = logger;
            _repository = repository;
            _configuration = configuration;
            _serviceBase = serviceBase;
            _communicationProvider = communicationProvider;
            _cacheStore = cacheStore;
            _repositorySystem = repositorySystem;
            _cidadeService = cidadeService;
            _regraPaxFreeService = regraPaxFreeService;
        }


        public async Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetContratosTimeSharing(SearchContratosTimeSharingModel searchModel)
        {
            var empresaCmId = _configuration.GetValue<int>("EmpresaCMId", 3);

            var parameters = new List<Parameter>();

            var sb = new StringBuilder(@$"SELECT
                                      VC.IDVENDAXCONTRATO AS IDVENDATS,
                                      VC.IDVENDAXCONTRATO,
                                      A.IDCLIENTE,
                                      P.NOME AS NOMECLIENTE,
                                      p.Email as EmailCliente,
                                      p.NumDocumento AS DocumentoCliente,
                                      P.IDPESSOA AS PESSOAPROVIDERID,
                                      COALESCE( PJ.NUMEROPROJETO,'-1') AS NUMEROPROJETO,
                                      cast( TO_CHAR( COALESCE( PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VC.NUMEROCONTRATO) ) as varchar (50)) AS PROJETOXCONTRATO,
                                      C.NOME AS TIPOCONTRATO,
                                      EP.NOMECIDADE || '-' || EP.NOMEESTADO AS CIDADE_ESTADO, 
                                      A.IDPROMAPRESEFET, PRO.NOME AS PROMOTOR_APRESENTACAO,
                                      CC.DATACANCELAMENTO,
                                      RC.DATAREVERSAO,
                                      VC.NUMEROCONTRATO,
                                      NVL(VC.FLGCANCELADO,'N') AS CANCELADO,
                                      NVL(VC.FLGREVERTIDO,'N') AS REVERTIDO,
                                      V.CODVENDA,
                                      AG.NOME AS SALAVENDAS,
                                      NVL(RC.DATAREVERSAO, V.DATAVENDA) AS DATAVENDA,
                                      C.NUMEROPONTOS - NVL(U.UTILIZACAO,0) + NVL(COMPRADOS.PONTOSCOMPRADOS,0) AS SALDOPONTOS,                  
                                      NVL(CC.FLGMIGRADO,NVL(RC.FLGMIGRADO,NVL(A.FLGMIGRADO,'N'))) AS FLGMIGRADO,
                                      CAST( DECODE(NVL(CC.FLGMIGRADO,NVL(RC.FLGMIGRADO,NVL(A.FLGMIGRADO,'N'))), 'S','Contratos migrados','Vendas normais') AS VARCHAR(18) ) AS TEXTOMIGRADO,
                                      TO_NUMBER(DECODE(NVL(VC.FLGCANCELADO,'N'),'N',
                                      DECODE(NVL(VC.FLGREVERTIDO,'N'),'N',1,0),0)) AS ATIVO,
                                      PAGTO.QUANT_PARC_ENTRADA AS QtdeParcelasEntrada,
                                      PAGTO.VALOR_ENTRADA AS ValorEntrada,
                                      PAGTO.QUANT_PARC_FINANC AS QtdeParcelasFinanciamento,
                                      PAGTO.VALOR_FINANC AS ValorFinanciado,
                                      VAL.TOTAL AS ValorTotalVenda,
                                      0.00 AS PERCENTTS, C.IDCONTRATOTS, LA.DESCRICAO AS LOCALPROSPECCAO, 
                                      DECODE(A.PERIODOATEND, 'M', 'Matutino', 'V', 'Vespertino', 'N', 'Noturno') AS PERIODOATEND,
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
                                       (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12) 
                                             WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                                             ELSE NVL(RC.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END) AS DATAVALIDADE,
                                       PAGTO.QTDE_PAGTO AS QtdeParcelasPagas
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
                                      AND H.IDPESSOA = {empresaCmId}
                                      ");


            if (!string.IsNullOrEmpty(searchModel.SalaVendas))
            {
                var agencias = (await _repository.FindBySql<SalaVendasModel>($@"SELECT 
                    P.NOME AS Nome,
                    ag.IdAgenciaTs AS Id
                    FROM 
                    AGENCIATS ag
                    INNER JOIN Pessoa p ON ag.IdAgenciaTs = p.IdPessoa
                    Where Lower(p.Nome) like '%{searchModel.SalaVendas.ToLower().TrimEnd()}%' ")).AsList();

                if (agencias != null && agencias.Any())
                {
                    sb.AppendLine($" and vc.IdAgenciaTs in ({string.Join(",", agencias.Select(b => b.Id).Distinct().ToList())}) ");
                }
            }

            if (!string.IsNullOrEmpty(searchModel.NomeCliente))
            {
                sb.AppendLine($" and Lower(p.Nome) like '%{searchModel.NomeCliente.ToLower().TrimEnd()}%' ");
            }

            if (!string.IsNullOrEmpty(searchModel.TipoContrato))
            {
                sb.AppendLine($" and Lower(C.NOME) like '%{searchModel.TipoContrato.ToLower().TrimEnd()}%' ");
            }

            if (!string.IsNullOrEmpty(searchModel.ProjetoXContrato))
            {
                sb.AppendLine($" and Lower(cast( TO_CHAR( COALESCE( PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VC.NUMEROCONTRATO) ) as varchar (50))) like '%{searchModel.ProjetoXContrato.ToLower().TrimEnd()}%' ");
            }

            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                if (searchModel.NumeroContrato.Split("-").Length == 2)
                {
                    
                    sb.AppendLine($" and Lower(cast( TO_CHAR( COALESCE( PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VC.NUMEROCONTRATO) ) as varchar (50))) like '%{searchModel.NumeroContrato.ToLower().TrimEnd()}%' ");
                }
                else
                {
                    sb.AppendLine($" and Lower(TO_CHAR(VC.NUMEROCONTRATO)) like '{searchModel.NumeroContrato.ToLower().TrimEnd()}%' ");
                }
            }

            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                if (searchModel.Status == "C")
                    searchModel.Status = "CANCELADO";
                else if (searchModel.Status == "E")
                    searchModel.Status = "EXPIRADO";
                else if (searchModel.Status == "R")
                    searchModel.Status = "REVERTIDO";
                else searchModel.Status = "ATIVO";

                    sb.AppendLine($@" and Lower(CASE WHEN VC.FLGCANCELADO = 'S' THEN 'CANCELADO'  
                    WHEN VC.FLGREVERTIDO = 'S' THEN 'REVERTIDO'  
                    WHEN ((PAR.DATASISTEMA > (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12)  
                                                   WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                                                   ELSE NVL(RC.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END))                                          
               AND (VC.IDSEMANAFIXAUH IS NULL)) THEN 'EXPIRADO' ELSE 'ATIVO' END) like '%{searchModel.Status.TrimEnd().ToLower()}%'");
            }

            if (searchModel.DataVendaInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataVendaInicial", searchModel.DataVendaInicial.GetValueOrDefault().Date));
                sb.AppendLine(" and NVL(RC.DATAREVERSAO, V.DATAVENDA) >= :dataVendaInicial ");
            }

            if (searchModel.DataVendaFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataVendaFinal", searchModel.DataVendaFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                sb.AppendLine(" and NVL(RC.DATAREVERSAO, V.DATAVENDA) <= :dataVendaFinal ");
            }

            if (searchModel.DataCancelamentoInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataCancelamentoInicial", searchModel.DataCancelamentoInicial.GetValueOrDefault().Date));
                sb.AppendLine(" and CC.DATACANCELAMENTO >= :dataCancelamentoInicial ");
            }

            if (searchModel.DataCancelamentoFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataCancelamentoFinal", searchModel.DataCancelamentoFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                sb.AppendLine(" and CC.DATACANCELAMENTO >= :dataCancelamentoFinal ");
            }

            if (searchModel.IdVendaTs.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and VC.IDVENDAXCONTRATO = {searchModel.IdVendaTs} ");
            }

            if (!string.IsNullOrEmpty(searchModel.NumDocumentoCliente))
            {
                sb.AppendLine($" and Replace(Replace(Replace(Replace(p.NumDocumento,'-',''),'.',''),'/',''),' ','') = '{Helper.ApenasNumeros(searchModel.NumDocumentoCliente)}' ");
            }

            var sql = sb.ToString();

            var totalRegistros = await _repository.CountTotalEntry(sql, parameters.ToArray());

            if (totalRegistros == 0)
                return (1, 1, new List<ContratoTimeSharingModel>());

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros) < 30 ? Convert.ToInt32(totalRegistros) : 30;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 &&
                searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > Convert.ToInt32(totalRegistros))
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros);

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
            if (totalPage < searchModel.NumeroDaPagina)
                searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);

            sb.AppendLine(" ORDER BY VC.IDVENDAXCONTRATO ");

            var result = (await _repository.FindBySql<ContratoTimeSharingModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), result);
                }

            }

            return (1, 1, result);
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContratoTimeSharingModel> contratos)?> GetMeusContratosTimeSharing(SearchMeusContratosTimeSharingModel searchModel)
        {
            var empresaCmId = _configuration.GetValue<int>("EmpresaCMId", 3);
            var loggedUser = await _repository.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            // Se IdCliente foi fornecido, verifica se o usuário é administrador
            var admAsUser = false;
            if (searchModel.IdCliente.HasValue)
            {
                if (!loggedUser.Value.isAdm)
                {
                    var clienteVinculado = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
                    if (clienteVinculado == null || int.Parse(clienteVinculado.PessoaProvider!) != searchModel.IdCliente)
                        throw new UnauthorizedAccessException("Apenas administradores podem visualizar reservas de outros clientes");

                }
                else admAsUser = true;
            }

            var pessoaVinculadaSistema = searchModel.IdCliente.GetValueOrDefault(0) > 0 ? 
                new PessoaSistemaXProviderModel() { PessoaProvider = searchModel.IdCliente.GetValueOrDefault().ToString()} : 
                await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
            if (pessoaVinculadaSistema == null)
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            if (string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) || !Helper.IsNumeric(pessoaVinculadaSistema.PessoaProvider))
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            if (searchModel.FormaSimplificada.GetValueOrDefault(false))
            {
                List<ContratoTimeSharingModel> contratosRetornar =
                    (await _repository.FindBySql<ContratoTimeSharingModel>($@"SELECT
                            ate.IdCLiente,
                            p.Nome AS NomeCliente,
                            p.NumDocumento AS DocumentoCliente,
                            COALESCE( PJ.NUMEROPROJETO,'-1') AS NUMEROPROJETO,
                            c.Nome AS TipoContrato,
                            vxc.IdVendaTs,
                            vxc.IdVendaXContrato,
                            pj.NUMEROPROJETO ||'-'|| TO_CHAR(TO_NUMBER(VXC.NUMEROCONTRATO)) AS NumeroContrato,
                            cast(TO_CHAR( COALESCE(PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VXC.NUMEROCONTRATO) ) as varchar (50)) AS PROJETOXCONTRATO,
                            NVL(VXC.FLGCANCELADO,'N') AS CANCELADO,
                            NVL(VXC.FLGREVERTIDO,'N') AS REVERTIDO,
                            TO_NUMBER(DECODE(NVL(VXC.FLGCANCELADO,'N'),'N',
                                      DECODE(NVL(VXC.FLGREVERTIDO,'N'),'N',1,0),0)) AS ATIVO,
                            CASE WHEN VXC.FLGCANCELADO = 'S' THEN 'CANCELADO'  
                                            WHEN VXC.FLGREVERTIDO = 'S' THEN 'REVERTIDO'  
                                            WHEN ((SYSDATE-1 > (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12)  
                                                                           WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                                                                           ELSE NVL(RC.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END))                                          
                                       AND (VXC.IDSEMANAFIXAUH IS NULL)) THEN 'EXPIRADO' ELSE 'ATIVO' END AS STATUS,
                            c.NumeroPontos as TotalPontos,
                            rc.IdRCI,
                            NVL(RC.DATAREVERSAO, V.DATAVENDA) AS DATAVENDA,
                            (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12) 
                                             WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                                             ELSE NVL(RC.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END) AS DATAVALIDADE
                            FROM 
                            cm.VendaXContratoTs vxc
                            INNER Join cm.AtendClienteTs ate ON vxc.IdAtendClienteTs = ate.IdAtendClienteTs
                            INNER JOIN cm.Pessoa p ON ate.IdCliente = p.IdPessoa
                            INNER JOIN cm.ProjetoTs pj ON vxc.IDPROJETOTS = pj.IdProjetoTs
                            INNER JOIN cm.ContratoTs c ON vxc.IdContratoTs = c.IdContratoTs
                            INNER JOIN cm.VENDATS V ON v.IDVENDATS = vxc.IDVENDATS 
                            LEFT OUTER JOIN cm.REVCONTRATOTS RC on VXC.IdVendaXContrato = RC.IDVENDAXCONTRNOVO
                            LEFT OUTER JOIN (
                            SELECT 
                             ap.IdPessoa,
                             ap.VALORCHAR AS IdRCI 
                            FROM 
                             Pessoaxatributo ap 
                            WHERE ap.idatributopessoa = 10 AND 
                             ap.VALORCHAR IS NOT null AND 
                             LENGTH(ap.VALORCHAR) > 1) rc on rc.IdPessoa = p.IdPessoa
                            WHERE
                            NVL(vxc.FLGREVERTIDO,'N')  = 'N' AND
                            NVL(vxc.FLGCANCELADO, 'N') = 'N' AND
                            p.IdPessoa = {pessoaVinculadaSistema.PessoaProvider}")).AsList();

                return (1, 1, contratosRetornar.AsList());
            }

            var parameters = new List<Parameter>();

            var sb = new StringBuilder(@$"SELECT
                                      VC.IDVENDATS,
                                      VC.IDVENDAXCONTRATO,
                                      A.IDCLIENTE,
                                      P.NOME AS NOMECLIENTE,
                                      p.NumDocumento AS DocumentoCliente,
                                      p.Email as EmailCliente,
                                      P.IDPESSOA AS PESSOAPROVIDERID,
                                      COALESCE( PJ.NUMEROPROJETO,'-1') AS NUMEROPROJETO,
                                      cast( TO_CHAR( COALESCE( PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VC.NUMEROCONTRATO) ) as varchar (50)) AS PROJETOXCONTRATO,
                                      C.NOME AS TIPOCONTRATO,
                                      EP.NOMECIDADE || '-' || EP.NOMEESTADO AS CIDADE_ESTADO, 
                                      A.IDPROMAPRESEFET, PRO.NOME AS PROMOTOR_APRESENTACAO,
                                      CC.DATACANCELAMENTO, 
                                      RC.DATAREVERSAO,
                                      cast( TO_CHAR( COALESCE( PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VC.NUMEROCONTRATO) ) as varchar (50)) AS NumeroContrato,
                                      NVL(VC.FLGCANCELADO,'N') AS CANCELADO,
                                      NVL(VC.FLGREVERTIDO,'N') AS REVERTIDO,
                                      V.CODVENDA,
                                      AG.NOME AS SALAVENDAS,
                                      NVL(RC.DATAREVERSAO, V.DATAVENDA) AS DATAVENDA,
                                      C.NUMEROPONTOS - NVL(U.UTILIZACAO,0) + NVL(COMPRADOS.PONTOSCOMPRADOS,0) AS SALDOPONTOS, 
                                      NVL(CC.FLGMIGRADO,NVL(RC.FLGMIGRADO,NVL(A.FLGMIGRADO,'N'))) AS FLGMIGRADO,
                                      CAST( DECODE(NVL(CC.FLGMIGRADO,NVL(RC.FLGMIGRADO,NVL(A.FLGMIGRADO,'N'))), 'S','Contratos migrados','Vendas normais') AS VARCHAR(18) ) AS TEXTOMIGRADO,
                                      TO_NUMBER(DECODE(NVL(VC.FLGCANCELADO,'N'),'N',
                                      DECODE(NVL(VC.FLGREVERTIDO,'N'),'N',1,0),0)) AS ATIVO,
                                      PAGTO.QUANT_PARC_ENTRADA AS QtdeParcelasEntrada,
                                      PAGTO.VALOR_ENTRADA AS ValorEntrada,
                                      PAGTO.QUANT_PARC_FINANC AS QtdeParcelasFinanciamento,
                                      PAGTO.VALOR_FINANC AS ValorFinanciado,
                                      VAL.TOTAL AS ValorTotalVenda,
                                      0.00 AS PERCENTTS, C.IDCONTRATOTS, LA.DESCRICAO AS LOCALPROSPECCAO, 
                                      DECODE(A.PERIODOATEND, 'M', 'Matutino', 'V', 'Vespertino', 'N', 'Noturno') AS PERIODOATEND,
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
                                       (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12) 
                                             WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                                             ELSE NVL(RC.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END) AS DATAVALIDADE,
                                       PAGTO.QTDE_PAGTO AS QtdeParcelasPagas,
                                       C.NUMEROPONTOS AS TOTALPONTOS,
                                       rc1.IdRCI
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
                                      (
                                        SELECT 
                                            ap.IdPessoa,
                                            ap.VALORCHAR AS IdRCI 
                                        FROM 
                                            Pessoaxatributo ap 
                                        WHERE ap.idatributopessoa = 10 AND 
                                        ap.VALORCHAR IS NOT null AND 
                                        LENGTH(ap.VALORCHAR) > 1) rc1,
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
                                      AND P.IDPESSOA            = rc1.IdPessoa(+)
                                      AND ((RC.IDREVCONTRATOTS IS NULL AND VAL.IDREVCONTRATOTS IS NULL) OR 
  		                                    (RC.IDREVCONTRATOTS = VAL.IDREVCONTRATOTS))
                                      AND ( (P.IDPESSOA         = EP.IDPESSOA(+) ) 
                                      AND (EP.IDENDERECO = P.IDENDRESIDENCIAL OR
                                            EP.IDENDERECO = P.IDENDCOMERCIAL   OR
                                            EP.IDENDERECO = P.IDENDCOBRANCA    OR
                                            EP.IDENDERECO = P.IDENDCORRESP     OR
                                            EP.IDENDERECO IS NULL
                                            ) )
                                      AND H.IDPESSOA = {empresaCmId}
                                      AND p.IDPESSOA = {(searchModel.IdCliente.HasValue ? searchModel.IdCliente.Value.ToString() : pessoaVinculadaSistema.PessoaProvider)}
                                      AND NVL(VC.FLGREVERTIDO,'N') = 'N' AND NVL(VC.FLGCANCELADO,'N') = 'N'
                                      ");


            if (!string.IsNullOrEmpty(searchModel.TipoContrato))
            {
                sb.AppendLine($" and Lower(C.NOME) like '%{searchModel.TipoContrato.ToLower().TrimEnd()}%' ");
            }

            if (!string.IsNullOrEmpty(searchModel.ProjetoXContrato))
            {
                sb.AppendLine($" and Lower(cast( TO_CHAR( COALESCE( PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VC.NUMEROCONTRATO) ) as varchar (50))) like '%{searchModel.ProjetoXContrato.ToLower().TrimEnd()}%' ");
            }

            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                sb.AppendLine($" and (Lower(TO_CHAR(VC.NUMEROCONTRATO)) like '{searchModel.NumeroContrato.ToLower().TrimEnd()}%' or Lower(cast( TO_CHAR( COALESCE( PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VC.NUMEROCONTRATO) ) as varchar (50))) like '%{searchModel.NumeroContrato.ToLower().TrimEnd()}%') ");
            }

            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                sb.AppendLine($@" and Lower(CASE WHEN VC.FLGCANCELADO = 'S' THEN 'CANCELADO'  
                    WHEN VC.FLGREVERTIDO = 'S' THEN 'REVERTIDO'  
                    WHEN ((PAR.DATASISTEMA > (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12)  
                                                   WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(RC.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                                                   ELSE NVL(RC.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END))                                          
               AND (VC.IDSEMANAFIXAUH IS NULL)) THEN 'EXPIRADO' ELSE 'ATIVO' END) like '%{searchModel.Status.ToLower().TrimEnd()}%'");
            }

            if (searchModel.DataVendaInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataVendaInicial", searchModel.DataVendaInicial.GetValueOrDefault().Date));
                sb.AppendLine(" and NVL(RC.DATAREVERSAO, V.DATAVENDA) >= :dataVendaInicial ");
            }

            if (searchModel.DataVendaFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataVendaFinal", searchModel.DataVendaFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                sb.AppendLine(" and NVL(RC.DATAREVERSAO, V.DATAVENDA) <= :dataVendaFinal ");
            }

            if (searchModel.DataCancelamentoInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataCancelamentoInicial", searchModel.DataCancelamentoInicial.GetValueOrDefault().Date));
                sb.AppendLine(" and CC.DATACANCELAMENTO >= :dataCancelamentoInicial ");
            }

            if (searchModel.DataCancelamentoFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("dataCancelamentoFinal", searchModel.DataCancelamentoFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                sb.AppendLine(" and CC.DATACANCELAMENTO >= :dataCancelamentoFinal ");
            }


            var sql = sb.ToString();

            var totalRegistros = await _repository.CountTotalEntry(sql, parameters.ToArray());

            if (totalRegistros == 0)
                return (1, 1, new List<ContratoTimeSharingModel>());

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros) < 30 ? Convert.ToInt32(totalRegistros) : 30;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 && searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > Convert.ToInt32(totalRegistros))
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros);

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            long totalPageValidation = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
            if (totalPageValidation < searchModel.NumeroDaPagina)
                searchModel.NumeroDaPagina = Convert.ToInt32(totalPageValidation);

            sb.AppendLine(" ORDER BY VC.IDVENDAXCONTRATO ");

            var result = (await _repository.FindBySql<ContratoTimeSharingModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            if (result.Any())
            {
                foreach (var item in result.Where(b=> b.IdVendaXContrato.GetValueOrDefault(0) > 0))
                {
                    var dadosUtilizacao = await DadosUtilizacaoContrato(item.IdVendaXContrato.GetValueOrDefault());
                    item.DadosUtilizacaoContrato = dadosUtilizacao;
                }

                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPageValidation), result);
                }

            }

            return (1, 1, result);
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetReservasGeralComConsumoPontos(SearchReservaTsModel searchModel)
        {
            var empresaCmId = _configuration.GetValue<int>("EmpresaCMId", 3);

            var contratos = await GetContratos();

            var parameters = new List<Parameter>();
            var txtPeriodoCheckin = "";
            var txtPeriodoCheckout = "";
            //var txtRmDataCheckinReserva = "";
            //var txtRmDataCheckoutReserva = "";

            if (searchModel.CheckinInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckinInicial = DateTime.Today.AddMonths(-1);
            }

            if (searchModel.CheckinFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckinFinal = DateTime.Today.AddMonths(1);
            }

            if (searchModel.CheckoutInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckoutInicial = DateTime.Today.AddMonths(-1);
            }

            if (searchModel.CheckoutFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckoutFinal = DateTime.Today.AddMonths(1);
            }

            if (searchModel.CheckinInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("chegadaInicial", searchModel.CheckinInicial.GetValueOrDefault().Date));
                txtPeriodoCheckin = " AND COALESCE(RF.DATACHEGADAREAL,RF.DATACHEGPREVISTA) >= :chegadaInicial ";
                //txtRmDataCheckinReserva = " AND RM.DATACHEGADA >= :chegadaInicial ";
            }

            if (searchModel.CheckinFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("chegadaFinal", searchModel.CheckinFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                txtPeriodoCheckin += " AND COALESCE(RF.DATACHEGADAREAL,RF.DATACHEGPREVISTA) <= :chegadaFinal ";
                //txtRmDataCheckinReserva += " AND RM.DATACHEGADA >= :chegadaFinal ";
            }


            if (searchModel.CheckoutInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("partidaInicial", searchModel.CheckoutInicial.GetValueOrDefault().Date));
                txtPeriodoCheckout = " AND COALESCE(RF.DATAPARTIDAREAL,RF.DATAPARTPREVISTA) >= :partidaInicial ";
                //txtRmDataCheckoutReserva = " AND RM.DATAPARTIDA >= :partidaInicial ";
            }

            if (searchModel.CheckoutFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("partidaFinal", searchModel.CheckoutFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                txtPeriodoCheckout += " AND COALESCE(RF.DATAPARTIDAREAL,RF.DATAPARTPREVISTA) <= :partidaFinal ";
                //txtRmDataCheckoutReserva += " AND RM.DATAPARTIDA <= :partidaFinal ";
            }


            var sb = new StringBuilder($@"WITH ReservasPontos AS (
                                            SELECT IDRESERVASFRONT, SUM(NVL(NUMEROPONTOS, 0)) AS QtdePontos, IDVENDAXCONTRATO, FLGMIGRADO  
                                            FROM LANCPONTOSTS
                                            WHERE IDTIPOLANCPONTOTS IN (1, 4)
                                            GROUP BY IDRESERVASFRONT, IDVENDAXCONTRATO, FLGMIGRADO
                                        ),
                                        UsuarioReserva AS (
                                            SELECT LP.IDRESERVASFRONT,
                                                   NVL(LP.IDUSUARIORESERVA, NVL(LP.IDUSUARIO, LP.IDUSUARIOLOGADO)) AS IDUSUARIO
                                            FROM LANCPONTOSTS LP
                                            JOIN (
                                                SELECT MIN(IDLANCPONTOSTS) AS IDLANCPONTOSTS
                                                FROM LANCPONTOSTS
                                                GROUP BY IDRESERVASFRONT
                                            ) IDS ON LP.IDLANCPONTOSTS = IDS.IDLANCPONTOSTS
                                        ),
                                        ReservasComTaxa AS (
                                            SELECT LP.IDRESERVASFRONT, SUM(L.VLRLANCAMENTO) AS VLRTAXA, LP.VLRTAXAISENTA
                                            FROM LANCAMENTOTS L
                                            JOIN LANCPONTOSTS LP ON L.IDLANCPONTOSTS = LP.IDLANCPONTOSTS
                                            WHERE L.IDTIPOLANCAMENTO = 5
                                              AND LP.IDTIPOLANCPONTOTS IN (1, 4)
                                              AND LP.IDRESERVASFRONT IS NOT NULL
                                            GROUP BY LP.IDRESERVASFRONT, LP.VLRTAXAISENTA
                                        )
                                       SELECT DISTINCT
                                        VTS.IDVENDATS,
                                        VXC.IDVENDAXCONTRATO,
                                        ACTS.IDCLIENTE,
                                        CR.NOME AS NOMECLIENTE,
                                        CR.EMAIL AS EmailCliente,
                                        CR.NUMDOCUMENTO AS DocumentoCliente,
                                        COALESCE( PJ.NUMEROPROJETO,'-1') AS NUMEROPROJETO,
                                        cast( TO_CHAR( COALESCE(PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VXC.NUMEROCONTRATO) ) as varchar (50)) AS PROJETOXCONTRATO,
                                        C.NOME AS TIPOCONTRATO,
                                        RF.NUMRESERVA,
                                        RF.IDRESERVASFRONT,
                                        CASE 
                                            WHEN RF.NUMVOO IS NULL OR RTRIM(RF.NUMVOO) = '' THEN 'Não'
                                            ELSE 'Sim'
                                        END AS NUMVOO,
                                        CASE 
                                            WHEN (
                                                SELECT MIN(LOCRESERVA) 
                                                FROM RESERVASFRONT 
                                                WHERE IDRESERVAMULTROOM = RF.IDRESERVASFRONT
                                                GROUP BY IDRESERVASFRONT
                                            ) > 0 
                                            THEN RF.LOCRESERVA || '/' || (
                                                SELECT LOCRESERVA 
                                                FROM RESERVASFRONT 
                                                WHERE IDRESERVAMULTROOM = RF.IDRESERVASFRONT
                                            )
                                            ELSE TO_CHAR(RF.LOCRESERVA)
                                        END AS LOCALIZADOR,
                                        NVL(RF.DATACHEGPREVISTA, RF.DATACHEGADAREAL) AS CHECKIN,
                                        NVL(RF.DATAPARTPREVISTA, RF.DATAPARTIDAREAL) AS CHECKOUT,
                                        ST.DESCRICAO AS STATUSRESERVA,
                                        RF.DATACANCELAMENTO,
                                        PEH.NOME AS HOTEL,
                                        TUH.DESCRICAO AS TIPOUH,
                                        LPAD(TUH.CODREDUZIDO, 20, ' ') AS CODTIPOUH,
                                        DECODE(RP.FLGMIGRADO,'N','Normal','S','Migrada') AS TIPORESERVA,
                                        TO_CHAR(VXC.NUMEROCONTRATO) AS NUMEROCONTRATO,
                                        US.NOMEUSUARIO AS CRIADAPOR,
                                        NVL(RP.Qtdepontos, 0) AS PONTORESERVA,
                                        NVL(RTX.VLRTAXA, 0) AS ValorTaxa,
                                        CASE 
                                            WHEN (NVL(RTX.VLRTAXA, 0) = 0 AND RTX.VLRTAXAISENTA IS NOT NULL) THEN 'Sim'
                                            ELSE 'Não'
                                        END AS TAXAISENTA,
                                        ( SELECT NVL(SUM(ORC.VALOR),0) FROM ORCAMENTORESERVA ORC WHERE ORC.IDRESERVASFRONT = RF.IDRESERVASFRONT ) VALORPENSAO,
                                        PH.NOME AS HOSPEDEPRINCIPAL,
                                        RF.Adultos,
                                        RF.Criancas1,
                                        RF.Criancas2
                                        FROM
                                        RESERVASTS RTS 
                                        INNER JOIN RESERVASFRONT RF ON RTS.IDRESERVASFRONT = RF.IDRESERVASFRONT
                                        INNER JOIN STATUSRESERVA ST ON ST.STATUSRESERVA = RF.STATUSRESERVA
                                        INNER JOIN MOVIMENTOHOSPEDES MH ON MH.IDRESERVASFRONT = RF.IDRESERVASFRONT AND MH.PRINCIPAL = 'S'
                                        INNER JOIN PESSOA PH ON MH.IDHOSPEDE = PH.IDPESSOA
                                        INNER JOIN PARAMTS PAR ON PAR.IDHOTEL = RF.IDHOTEL
                                        LEFT JOIN TIPOUH TUH ON TUH.IDTIPOUH = RF.TIPOUHTARIFA AND TUH.IDHOTEL = RF.IDHOTEL
                                        LEFT JOIN HOTEL H ON RF.IDHOTEL = H.IDHOTEL
                                        LEFT JOIN PESSOA PEH ON H.IDPESSOA = PEH.IDPESSOA
                                        LEFT JOIN ReservasPontos RP ON RP.IDRESERVASFRONT = RF.IDRESERVASFRONT
                                        LEFT JOIN VENDAXCONTRATOTS VXC ON VXC.IDVENDAXCONTRATO = RP.IDVENDAXCONTRATO
                                        LEFT JOIN VENDATS VTS ON VXC.IDVENDATS = VTS.IDVENDATS
                                        LEFT JOIN ATENDCLIENTETS ACTS ON VTS.IDATENDCLIENTETS = ACTS.IDATENDCLIENTETS
                                        LEFT JOIN CONTRATOTS C ON VXC.IDCONTRATOTS = C.IDCONTRATOTS
                                        LEFT JOIN PROJETOTS PJ ON PJ.IDPROJETOTS = VXC.IDPROJETOTS
                                        LEFT JOIN PESSOA CR ON CR.IDPESSOA = ACTS.IDCLIENTE
                                        LEFT JOIN ReservasComTaxa RTX ON RTX.IDRESERVASFRONT = RF.IDRESERVASFRONT
                                        LEFT JOIN UsuarioReserva UR ON UR.IDRESERVASFRONT = RF.IDRESERVASFRONT
                                        LEFT JOIN USUARIOSISTEMA US ON US.IDUSUARIO = UR.IDUSUARIO
                                    WHERE
 										H.IDPESSOA = 3
                                        AND (
                                            (RF.DATACHEGADAREAL IS NOT NULL AND RF.DATACHEGADAREAL BETWEEN :chegadaInicial AND :chegadaFinal)
                                            OR
                                            (RF.DATACHEGADAREAL IS NULL AND RF.DATACHEGPREVISTA BETWEEN :chegadaInicial AND :chegadaFinal)
                                        )
                                        AND (
                                            (RF.DATAPARTIDAREAL IS NOT NULL AND RF.DATAPARTIDAREAL BETWEEN :partidaInicial AND :partidaFinal)
                                            OR
                                            (RF.DATAPARTIDAREAL IS NULL AND RF.DATAPARTPREVISTA BETWEEN :partidaInicial AND :partidaFinal)
                                        )  ");

            if (!string.IsNullOrEmpty(searchModel.NomeCliente))
            {
                sb.AppendLine($" AND (LOWER(CR.NOME) LIKE '%{searchModel.NomeCliente.ToLower().TrimEnd()}%' OR LOWER(PH.NOME) LIKE '%{searchModel.NomeCliente.ToLower().TrimEnd()}%' )");
            }

            if (!string.IsNullOrEmpty(searchModel.Hotel))
            {
                sb.AppendLine($" AND LOWER(PEH.NOME) LIKE '%{searchModel.Hotel.ToLower().TrimEnd()}%'");
            }

            if (!string.IsNullOrEmpty(searchModel.NumDocumentoCliente))
            {
                sb.AppendLine($" AND exists(select dp.IdPessoa From DocPessoa dp Where Replace(Replace(Replace(dp.NumDocumento,'.',''),'-',''),'/','') like '{Helper.ApenasNumeros(searchModel.NumDocumentoCliente)}%' and dp.IdPessoa = CR.IDPESSOA ) ");
            }

            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                sb.AppendLine($" AND TO_CHAR(VXC.NUMEROCONTRATO) LIKE '%{searchModel.NumeroContrato.TrimEnd()}%' ");
            }

            if (searchModel.NumReserva.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" AND RF.NUMRESERVA = {searchModel.NumReserva} ");
            }

            if (!string.IsNullOrEmpty(searchModel.StatusReserva))
            {
                sb.AppendLine($" AND LOWER(ST.DESCRICAO) LIKE '%{searchModel.StatusReserva.ToLower().TrimEnd()}%' ");
            }

            #region Reserva Migrada não incluidas
            //sb.AppendLine($@")
            //                                UNION ALL
            //                                (
            //                                SELECT
            //                                    DISTINCT
            //                                    RM.NUMRESERVA AS NUMRESERVA,
            //                                    TO_CHAR(RM.LOCRESERVA) AS LOCALIZADOR, 'Não' AS NUMVOO,
            //                                    RM.DATACHEGADA AS CHECKIN,
            //                                    RM.DATAPARTIDA AS CHECKOUT,
            //                                    DECODE(SIGN(TO_NUMBER(RM.DATAPARTIDA-PH.DATASISTEMA)),1,DECODE(SIGN(TO_NUMBER(PH.DATASISTEMA-RM.DATACHEGADA)),1,'Check-In','Confirmada'),'Check-out') AS STATUSRESERVA,
            //                                    null AS DATACANCELAMENTO,
            //                                    H.NOME AS HOTEL,
            //                                    TUH.DESCRICAO AS TIPOUH,
            //                                    TUH.CODREDUZIDO AS CODTIPOUH,
            //                                    CL.NOME AS NOMECLIENTE,
            //                                    'Migrada' AS TIPORESERVA,
            //                                    DECODE(RCI.IDRESERVAMIGRADA,NULL,'Migrada','RCI-Migrada') AS TIPOLANCAMENTO,
            //                                    TO_CHAR(PJ.NUMEROPROJETO)||'-'||TO_CHAR(VXC.NUMEROCONTRATO) AS PROJETOXCONTRATO,
            //                                    TO_CHAR(VXC.NUMEROCONTRATO) AS NUMEROCONTRATO,
            //                                    US.NOMEUSUARIO AS CRIADAPOR,
            //                                    NVL(PR.VALOR,0) AS PONTORESERVA,
            //                                    NVL(LTX.VLRTAXA,0) AS TAXAMANUTENCAO,
            //                                    'Não' AS TAXAISENTA,
            //                                    CASE WHEN LS.IDLISTAESPERA IS NOT NULL THEN 'Sim' ELSE 'Não' END AS LISTAESPERA,
            //                                    V.IDVENDATS,
            //                                    DTTX.DATALANCAMENTO AS DATAPAGTAXA,
            //                                    ( SELECT NVL(SUM(ORC.VALOR),0) FROM ORCAMENTORESERVA ORC WHERE ORC.IDRESERVASFRONT = RM.IDRESERVAMIGRADA ) VALORPENSAO,
            //                                    ROUND(TO_NUMBER( DECODE( NVL(C.VALORPONTO,0),0, DECODE(NVL(C.VALORPERCPONTO,0),0,
            //                                                        DECODE( NVL(C.NUMEROPONTOS,0),0,0, NVL(VAL.TOTAL, VXC.VALORFINAL) / C.NUMEROPONTOS),
            //                                                        (NVL(VAL.TOTAL, VXC.VALORFINAL) * C.VALORPERCPONTO)/ 100),
            //                                                        NVL(C.VALORPONTO,0))),6) AS VALORPONTO,
            //                                    ROUND(PR.VALOR * TO_NUMBER((DECODE(NVL(C.VALORPONTO,0),0, DECODE(NVL(C.VALORPERCPONTO,0),0, 
            //                                    DECODE(NVL(C.NUMEROPONTOS,0),0,0, NVL(VAL.TOTAL, VXC.VALORFINAL) / C.NUMEROPONTOS),
            //                                    (NVL(VAL.TOTAL, VXC.VALORFINAL) * C.VALORPERCPONTO)/ 100), NVL(C.VALORPONTO,0)))),6) AS VALORPONTOS,
            //                                    ' ' AS MOTIVO_FORCAR_RESERVA,
            //                                    ' ' AS JUST_FORCAR_RESERVA,
            //                                    0 AS VLRCONTABPONTOS,
            //                                    0 AS VLRCONTABTAXA, RM.QTDEPAX AS ADULTOS, 0 AS CRIANCAS1, 0 AS CRIANCAS2, 0 AS QTDRESERVAS,
            //                                    ' ' AS FRACIONAMENTO
            //                                    FROM VENDAXCONTRATOTS VXC, RESERVAMIGRADATS RM, USUARIOSISTEMA US, ATENDCLIENTETS A, RESERVASBULKTS RB, PARAMHOTEL PH, PROJETOTS PJ, TIPOUH TUH, VENDATS V, PESSOA CL, PESSOA H,
            //                                        HOTEL HO, CONTRATOTS C, REVCONTRATOTS RV, LISTAESPERATS LS,
            //                                    (SELECT IDRESERVAMIGRADA, MIN(IDVENDAXCONTRATO) AS IDVENDAXCONTRATO,
            //                                            SUM(NVL(TO_NUMBER(DECODE(IDTIPOLANCPONTOTS,1,NUMEROPONTOS)),0)) AS NUMPONTOS,
            //                                            MAX(FLGMIGRADO) AS FLGMIGRADO, MIN(IDHOTEL) AS IDHOTEL, MAX(DATALANCAMENTO) AS DATALANCAMENTO
            //                                        FROM LANCPONTOSTS
            //                                    GROUP BY IDRESERVAMIGRADA) LP,
            //                                    (SELECT DISTINCT IDRESERVAMIGRADA,IDRESERVASRCI FROM RESERVASRCI) RCI,
            //                                    (SELECT IDRESERVAMIGRADA, SUM(NVL(NUMEROPONTOS,0)) AS VALOR
            //                                        FROM LANCPONTOSTS 
            //                                    WHERE IDTIPOLANCPONTOTS IN (1,4) 
            //                                    GROUP BY IDRESERVAMIGRADA) PR,
            //                                    (SELECT LP.IDRESERVAMIGRADA, SUM(L.VLRLANCAMENTO) AS VLRTAXA
            //                                        FROM LANCAMENTOTS L, LANCPONTOSTS LP
            //                                    WHERE L.IDLANCPONTOSTS   = LP.IDLANCPONTOSTS
            //                                        AND L.IDTIPOLANCAMENTO = 5
            //                                        AND LP.IDTIPOLANCPONTOTS IN (1,4)
            //                                        AND LP.IDRESERVAMIGRADA IS NOT NULL
            //                                    GROUP BY LP.IDRESERVAMIGRADA) LTX,
            //                                    (SELECT LP.IDRESERVAMIGRADA, MAX(L.DATALANCAMENTO) AS DATALANCAMENTO 
            //                                        FROM LANCAMENTOTS L, LANCPONTOSTS LP
            //                                    WHERE L.IDLANCPONTOSTS   = LP.IDLANCPONTOSTS
            //                                        AND L.IDTIPOLANCAMENTO = 6
            //                                        AND LP.IDTIPOLANCPONTOTS IN (1,4)
            //                                        AND LP.IDRESERVAMIGRADA IS NOT NULL
            //                                    GROUP BY LP.IDRESERVAMIGRADA) DTTX,
            //                                    (SELECT LANCPONTOSTS.IDRESERVAMIGRADA, NVL(LANCPONTOSTS.IDUSUARIORESERVA, NVL(LANCPONTOSTS.IDUSUARIO, LANCPONTOSTS.IDUSUARIOLOGADO)) AS IDUSUARIO
            //                                        FROM LANCPONTOSTS, (SELECT MIN(LP.IDLANCPONTOSTS) AS IDLANCPONTOSTS FROM LANCPONTOSTS LP GROUP BY LP.IDRESERVAMIGRADA) IDS
            //                                    WHERE LANCPONTOSTS.IDLANCPONTOSTS = IDS.IDLANCPONTOSTS) USUARIORES,   
            //                                    (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'D',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS UTILIZACAO
            //                                        FROM LANCPONTOSTS LP, RESERVASFRONT RF, RESERVAMIGRADATS RM
            //                                    WHERE NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS = 1)
            //                                        AND LP.IDRESERVASFRONT  = RF.IDRESERVASFRONT (+)
            //                                        AND LP.IDRESERVAMIGRADA = RM.IDRESERVAMIGRADA (+)
            //                                        AND LP.IDTIPOLANCPONTOTS <> 8
            //                                    GROUP BY IDVENDAXCONTRATO) U,
            //                                    (SELECT VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS) AS IDREVCONTRATOTS, ABS(SUM(L.VLRLANCAMENTO)) AS TOTAL
            //                                        FROM LANCAMENTOTS L, VENDATS V, VENDAXCONTRATOTS VC, CONTRATOTS C, AJUSTEFINANCTS AJ, REVCONTRATOTS R
            //                                    WHERE L.IDVENDATS      = V.IDVENDATS
            //                                        AND V.IDVENDATS      = VC.IDVENDATS
            //                                        AND VC.IDCONTRATOTS  = C.IDCONTRATOTS
            //                                        AND L.IDAJUSTEFINANCTS  = AJ.IDAJUSTEFINANCTS (+)
            //                                        AND AJ.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
            //                                        AND (L.IDTIPOLANCAMENTO IN (1,7) OR L.IDTIPODEBCRED = C.IDTIPODCJUROS OR L.IDTIPODEBCRED = C.IDTIPODCCONTRATO)
            //                                        AND ((VC.FLGCANCELADO = 'N' AND VC.FLGREVERTIDO = 'N'         AND L.IDMOTIVOESTORNO IS NULL    AND L.IDLANCESTORNO IS NULL)
            //                                        OR  (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NULL     AND L.IDCANCCONTRATOTS IS NULL)
            //                                        OR  (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NOT NULL AND L.IDCANCCONTRATOTS IS NULL   AND L.IDAJUSTEFINANCTS IS NOT NULL )
            //                                        OR  (VC.FLGCANCELADO = 'S' AND L.IDMOTIVOESTORNO IS NULL     AND L.IDCANCCONTRATOTS IS NULL ))
            //                                        AND L.IDVENDATS IS NOT NULL
            //                                    GROUP BY VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS)) VAL
            //                                WHERE RM.IDRESERVAMIGRADA     = RCI.IDRESERVAMIGRADA(+)  
            //                                    AND RM.IDRESERVAMIGRADA     = RB.IDRESERVASFRONT (+)
            //                                    AND RB.IDRESERVASFRONT      = LS.IDRESERVASFRONT(+)
            //                                    AND LP.IDRESERVAMIGRADA     = LTX.IDRESERVAMIGRADA(+)
            //                                    AND LP.IDRESERVAMIGRADA     = DTTX.IDRESERVAMIGRADA(+)
            //                                    AND VXC.IDVENDAXCONTRATO    = U.IDVENDAXCONTRATO (+)
            //                                    AND VXC.IDVENDAXCONTRATO    = VAL.IDVENDAXCONTRATO (+)
            //                                    AND VXC.IDVENDAXCONTRATO    = RV.IDVENDAXCONTRNOVO (+)
            //                                    AND A.IDATENDCLIENTETS      = V.IDATENDCLIENTETS
            //                                    AND VXC.IDCONTRATOTS        = C.IDCONTRATOTS
            //                                    AND V.IDVENDATS             = VXC.IDVENDATS
            //                                    AND PJ.IDPROJETOTS          = VXC.IDPROJETOTS
            //                                    AND VXC.IDVENDAXCONTRATO    = LP.IDVENDAXCONTRATO
            //                                    AND RM.IDHOTEL              = PH.IDHOTEL
            //                                    AND USUARIORES.IDUSUARIO    = US.IDUSUARIO
            //                                    AND RM.IDRESERVAMIGRADA     = USUARIORES.IDRESERVAMIGRADA
            //                                    AND PR.IDRESERVAMIGRADA     = LP.IDRESERVAMIGRADA
            //                                    AND RM.IDRESERVAMIGRADA     = LP.IDRESERVAMIGRADA
            //                                    AND H.IDPESSOA              = RM.IDHOTEL
            //                                    AND TUH.IDTIPOUH            = RM.IDTIPOUH
            //                                    AND TUH.IDHOTEL             = RM.IDHOTEL
            //                                    AND CL.IDPESSOA             = A.IDCLIENTE
            //                                    AND A.IDHOTEL               = HO.IDHOTEL
            //                                    AND ((RV.IDREVCONTRATOTS IS NULL AND VAL.IDREVCONTRATOTS IS NULL) OR (RV.IDREVCONTRATOTS = VAL.IDREVCONTRATOTS))
            //                                    AND HO.IDPESSOA             = {empresaCmId}
            //                                    AND LP.FLGMIGRADO           = 'S'    
            //                                    {txtRmDataCheckinReserva} {txtRmDataCheckoutReserva} "); 
            #endregion


            if (!string.IsNullOrEmpty(searchModel.NumDocumentoCliente))
            {
                sb.AppendLine($" AND exists(select dp.IdPessoa From DocPessoa dp Where Replace(Replace(Replace(dp.NumDocumento,'.',''),'-',''),'/','') like '{Helper.ApenasNumeros(searchModel.NumDocumentoCliente)}%' and dp.IdPessoa = CR.IDPESSOA ) ");
            }

            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                sb.AppendLine($" AND TO_CHAR(VXC.NUMEROCONTRATO) LIKE '%{searchModel.NumeroContrato.TrimEnd()}%' ");
            }


            var sql = sb.ToString();

            var totalRegistros = await _repository.CountTotalEntry(sql, parameters.ToArray());
            if (totalRegistros == 0)
                return (1, 1, new List<ReservaTsModel>());

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros) < 30 ? Convert.ToInt32(totalRegistros) : 30;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 && searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > Convert.ToInt32(totalRegistros))
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros);

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
            if (totalPage < searchModel.NumeroDaPagina)
                searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);

            sb.AppendLine(@" ORDER BY
                                RF.IDRESERVASFRONT ");

            var result = (await _repository.FindBySql<ReservaTsModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            if (result.Any())
            {
                foreach (var item in result.Where(a => a.PontoReserva.GetValueOrDefault(0) > 0))
                {
                    var baseContrato = contratos.Where(a => a.IdVendaXContrato == item.IdVendaXContrato);
                    if (baseContrato != null && baseContrato.Any())
                    {
                        var contratoBaseUtilizarCalculoPontos = baseContrato.Where(a => a.ValorFinal.GetValueOrDefault() > 0 && a.NumeroPontos.GetValueOrDefault() > 0);
                        if (contratoBaseUtilizarCalculoPontos != null && contratoBaseUtilizarCalculoPontos.Any())
                        {
                            var contratobase = contratoBaseUtilizarCalculoPontos.OrderByDescending(a => a.DataReversao.GetValueOrDefault(a.DataVenda.GetValueOrDefault())).FirstOrDefault();
                            if (contratobase != null)
                            {
                                item.ValorPonto = contratobase.ValorFinal.GetValueOrDefault() / contratobase.NumeroPontos.GetValueOrDefault();
                                item.ValorPontos = item.PontoReserva.GetValueOrDefault() * item.ValorPonto.GetValueOrDefault();
                            }

                        }
                    }
                }

                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), result);
                }

            }

            return (1, 1, result);

        }

        private async Task<List<ContratoTimeSharinCacheModel>> GetContratos()
        {
            var contratos = (await _cacheStore.GetAsync<List<ContratoTimeSharinCacheModel>>(CONTRATOS_CACHE_KEY, 0));
            if (contratos == null || !contratos.Any())
            {
                var contratosBd = (await _repository.FindBySql<ContratoTimeSharinCacheModel>(@$"SELECT 
                   c.idcontratots,
                   CASE 
   	                WHEN nvl(vc.FlgRevertido,'N')= 'S' THEN 'Revertido'
   	                WHEN nvl(vc.FlgCancelado,'N')= 'S' THEN 'Cancelado'
   	                ELSE 'Ativo' END AS StatusContrato,
   	                (CASE WHEN C.TIPOVALIDADE = 'A' THEN ADD_MONTHS(NVL(rev.DATAREVERSAO, V.DATAVENDA), C.VALIDADE * 12) 
                      WHEN C.TIPOVALIDADE = 'M' THEN ADD_MONTHS(NVL(Rev.DATAREVERSAO, V.DATAVENDA), C.VALIDADE) 
                      ELSE NVL(rev.DATAREVERSAO, V.DATAVENDA) + C.VALIDADE END) AS DATAVALIDADE,
                   rev.DataReversao, 
                   can.DataCancelamento,
                   aten.idcliente,
                   p.nome AS NomeCliente,
                   p.numdocumento AS DocumentoCliente,
                   p.Email AS EmailCliente,
                   vc.TaxaPrimeiraUtili,
                   vc.TaxaRci,
                   vc.NumeroContrato,
                   vc.FlgCancelado,
                   vc.FlgRevertido,
                   vc.idvendaxcontrato,
                   vc.idprojetots,
                   vc.idvendats,   
                   vc.ValorBase,
                   v.DataVenda,
                   vc.DataIntegraliza,
                   vc.ValorFinal, 
                   vc.idagenciats,
                   vc.idpromotor,
                   vc.dataintegraliza,
                   c.Nome AS NomeContrato,
                   c.validade,
                   c.tipovalidade,
                   c.NumeroPontos,
                   c.DescontoAnual,
                   c.IdHotel,
                   c.AnoInicial
                   FROM 
                   vendaxcontratots vc
                   INNER JOIN contratots c ON vc.idcontratots = c.idcontratots
                   INNER JOIN VendaTs v ON vc.idvendats = v.IdVendaTs
                   INNER JOIN AtendClienteTs aten ON v.idatendclientets = aten.idatendclientets
                   INNER JOIN pessoa p ON aten.idcliente = p.idpessoa
                   LEFT OUTER JOIN RevContratoTs rev ON rev.IDVENDAXCONTRNOVO = vc.IdVendaXContrato
                   LEFT OUTER JOIN CANCCONTRATOTS can ON can.IdVendaXContrato = vc.IdVendaXContrato
                   WHERE
                   1 = 1 ")).AsList();


                if (contratosBd != null && contratosBd.Any())
                {
                    contratos = contratosBd.AsList();
                    await _cacheStore.AddAsync(CONTRATOS_CACHE_KEY, contratosBd, DateTimeOffset.Now.AddMinutes(20));
                }
            }

            return contratos ?? new List<ContratoTimeSharinCacheModel>();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetReservasGeral(SearchReservasGeralModel searchModel)
        {
            var empresaCmId = _configuration.GetValue<int>("EmpresaCMId", 3);

            var parameters = new List<Parameter>();
            var txtPeriodo = "";
            var txtExibirTodosOsHospedes = !searchModel.ExibirTodosOsHospedes ? " AND M.IdHospede = (Select m1.IdHospede From MovimentoHospedes m1 Where m1.Principal = 'S' AND m1.IdHospede = m.IdHospede AND m1.IdReservasFront = m.IdReservasFront) " : "";

            if (searchModel.CheckinInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckinInicial = DateTime.Today.AddYears(-30);
            }

            if (searchModel.CheckinFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckinFinal = DateTime.Today.AddYears(10);
            }

            if (searchModel.CheckoutInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckoutInicial = DateTime.Today.AddYears(-30);
            }

            if (searchModel.CheckoutFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckoutFinal = DateTime.Today.AddYears(10);
            }


            if (searchModel.CheckinInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("chegadaInicial", searchModel.CheckinInicial.GetValueOrDefault().Date));
                txtPeriodo = " COALESCE(M.DATACHEGREAL,M.DATACHEGPREVISTA) >= :chegadaInicial ";
            }
            else
            {
                parameters.Add(new Parameter("chegadaFinal", searchModel.CheckinFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                if (!string.IsNullOrEmpty(txtPeriodo))
                    txtPeriodo += " AND ";

                txtPeriodo += " COALESCE(M.DATACHEGREAL,M.DATACHEGPREVISTA) <= :chegadaFinal ";
            }


            if (searchModel.CheckoutInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("partidaInicial", searchModel.CheckoutInicial.GetValueOrDefault().Date));
                if (!string.IsNullOrEmpty(txtPeriodo))
                    txtPeriodo += " AND ";
                txtPeriodo += " COALESCE(M.DATAPARTREAL,M.DATAPARTPREVISTA) >= :partidaInicial ";
            }
            else
            {
                parameters.Add(new Parameter("partidaFinal", searchModel.CheckoutFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                if (!string.IsNullOrEmpty(txtPeriodo))
                    txtPeriodo += " AND ";
                txtPeriodo += " COALESCE(M.DATAPARTREAL,M.DATAPARTPREVISTA) <= :partidaFinal ";
            }


            var sb = new StringBuilder(@$"SELECT
	                                    CASE
		                                    WHEN TAR.FLGCONFIDENCIAL = 'S' THEN 0
		                                    ELSE CASE
			                                    WHEN R.STATUSRESERVA IN (0, 1, 7, 8, 5, 6) THEN (
			                                    SELECT
				                                    VALOR
			                                    FROM
				                                    ORCAMENTORESERVA O1
			                                    WHERE
				                                    O1.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND DATA = (
				                                    SELECT
					                                    MIN(DATA)
				                                    FROM
					                                    ORCAMENTORESERVA OREO1
				                                    WHERE
					                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
			                                    WHEN R.STATUSRESERVA IN (3, 4) THEN (
			                                    SELECT
				                                    VALOR
			                                    FROM
				                                    ORCAMENTORESERVA O1
			                                    WHERE
				                                    O1.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND DATA = (
				                                    SELECT
					                                    MAX(DATA)
				                                    FROM
					                                    ORCAMENTORESERVA OREO1
				                                    WHERE
					                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
			                                    WHEN R.STATUSRESERVA = 2
			                                    AND R.DATAPARTPREVISTA > PH.DATASISTEMA THEN (
			                                    SELECT
				                                    VALOR
			                                    FROM
				                                    ORCAMENTORESERVA O1
			                                    WHERE
				                                    O1.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND DATA = PH.DATASISTEMA)
			                                    ELSE (
			                                    SELECT
				                                    VALOR
			                                    FROM
				                                    ORCAMENTORESERVA O1
			                                    WHERE
				                                    O1.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND DATA = (
				                                    SELECT
					                                    MAX(DATA)
				                                    FROM
					                                    ORCAMENTORESERVA OREO1
				                                    WHERE
					                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    END
	                                    END AS VLRDIARIA,
	                                    TAR.FLGCONFIDENCIAL,
	                                    CASE
		                                    WHEN RG.OBSERVACOES IS NULL THEN R.OBSERVACOES
		                                    ELSE CASE
			                                    WHEN R.OBSERVACOES IS NULL THEN RG.OBSERVACOES
			                                    ELSE 'Reserva Grupo: ' || (NVL(RG.OBSERVACOES, ' '))|| ' - Reserva Individual: ' ||(NVL(R.OBSERVACOES, ' '))
		                                    END
	                                    END AS OBSERVACOES,
                                        PRO.NUMEROCONTRATO,
	                                    PRO.IDPESSOA, 
	                                    PRO.NOME AS NOMECLIENTE,
	                                    PRO.NUMDOCUMENTO AS NumDocumentoCliente,
	                                    PRO.EMAIL AS EMAILCLIENTE,
	                                    PRO.FLGCANCELADO AS ContratoCancelado,
                                        R.CLIENTERESERVANTE,   
	                                    U.BLOCO,
	                                    DECODE(NVL(AGENDA.QTDAGENDA, 0), 0, 'N', 'S') AS FLGAGENDA,
	                                    DECODE(NVL(MENSAGEM.QTDMENSAGEM, 0), 0, 'N', 'S') AS FLGMENSAGEM,
	                                    H.IDHOSPEDE,
	                                    TO_NUMBER(R.NUMRESERVAGDS) AS NUMRESERVAGDS,
	                                    TO_NUMBER(R.NUMRESERVA) AS NUMRESERVA,
	                                    R.IDRESERVASFRONT,
                                        R.NUMRESERVA AS NUMERORESERVA,
	                                    M.DATACHEGREAL AS DTCHEGHOSPEDE,
	                                    H.OBSERVACAO AS OBSHOSPEDE,
	                                    R.OBSSENSIVEIS,
	                                    M.DATAPARTREAL AS DTSAIDAHOSPEDE,
	                                    STAT.DESCRICAO AS StatusReserva,
	                                    NVL(R.OBSERVACOES, ' ') AS OBSRESERVA,
	                                    TO_DATE(TO_CHAR(DECODE(R.STATUSRESERVA, 0 ,                                                                                          
        	                                             (NVL(R.HORACHEGADAREAL, R.HORACHEGPREVISTA)), 1,                                                                            
        			                                     (NVL(R.HORACHEGADAREAL, R.HORACHEGPREVISTA)),                                                                                  
        			                                     (NVL(M.HORACHEGREAL, M.HORACHEGPREVISTA)) ), 'DD/MM/YYYY HH24:MI:SS'), 'DD/MM/YYYY HH24:MI:SS') AS HORACHEGADA ,
	                                    TO_DATE(TO_CHAR(DECODE(R.STATUSRESERVA, 0 ,                                                                                           
        	                                             (NVL(R.HORAPARTIDAREAL, R.HORAPARTPREVISTA)) , 1,                                                                           
        			                                     (NVL(R.HORAPARTIDAREAL, R.HORAPARTPREVISTA)) ,                                                                                 
        			                                     (NVL(M.HORAPARTREAL, M.HORAPARTPREVISTA)) ), 'DD/MM/YYYY HH24:MI:SS'), 'DD/MM/YYYY HH24:MI:SS')AS HORAPARTIDA,
	                                    NVL(M.DATAPARTREAL, M.DATAPARTPREVISTA) AS CHECKOUT,
	                                    R.GARANTENOSHOW,
	                                    NVL(M.DATACHEGREAL, M.DATACHEGPREVISTA) AS CHECKIN,
	                                    R.CODUH,
	                                    R.LOCRESERVA,
	                                    R.CODREFERENCIA,
	                                    T.CODREDUZIDO || ' / ' || TRUH.CODREDUZIDO AS CODREDUZIDO ,
	                                    RG.OBSERVACOES AS OBSGRP,
	                                    R.IDHOTEL,
	                                    HT.NOME AS HOTEL,
	                                    NVL(H.CODTRATAMENTO, '')|| ' ' || H.NOME || ' '  || H.SOBRENOME || DECODE(M.INCOGNITO, 'N', '', ' (INC.)') AS NOMEHOSPEDE,
	                                    H.NOME || ' ' || H.SOBRENOME AS NOMEHOSPEDEORD,
	                                    RG.NOMEGRUPO,
	                                    M.DATACHEGPREVISTA,
	                                    M.DATAPARTPREVISTA,
	                                    R.ADULTOS ,
	                                    R.CRIANCAS1,
	                                    R.CRIANCAS2,
	                                    R.DATADEPOSITO,
	                                    R.DATACONFIRMACAO,
	                                    R.DATARESERVA,
	                                    TH.HOSPEDEVIP,
	                                    TH.DESCRICAO AS TIPOHOSPEDE,
	                                    R.DATACANCELAMENTO,
	                                    CASE
		                                    WHEN R.STATUSRESERVA IN (0, 1, 7, 8, 5, 6) THEN (
		                                    SELECT
			                                    SEG.DESCRICAO
		                                    FROM
			                                    SEGMENTO SEG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND SEG.IDHOTEL = R.IDHOTEL
			                                    AND SEG.CODSEGMENTO = OREO.CODSEGMENTO
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MIN(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    WHEN R.STATUSRESERVA IN (3, 4) THEN (
		                                    SELECT
			                                    SEG.DESCRICAO
		                                    FROM
			                                    SEGMENTO SEG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND SEG.IDHOTEL = R.IDHOTEL
			                                    AND SEG.CODSEGMENTO = OREO.CODSEGMENTO
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MAX(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    WHEN R.STATUSRESERVA = 2
		                                    AND R.DATAPARTPREVISTA > PH.DATASISTEMA THEN (
		                                    SELECT
			                                    SEG.DESCRICAO
		                                    FROM
			                                    SEGMENTO SEG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND SEG.IDHOTEL = R.IDHOTEL
			                                    AND SEG.CODSEGMENTO = OREO.CODSEGMENTO
			                                    AND OREO.DATA = PH.DATASISTEMA)
		                                    ELSE (
		                                    SELECT
			                                    SEG.DESCRICAO
		                                    FROM
			                                    SEGMENTO SEG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND SEG.IDHOTEL = R.IDHOTEL
			                                    AND SEG.CODSEGMENTO = OREO.CODSEGMENTO
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MAX(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
	                                    END AS SEGMENTO,
	                                    CASE
		                                    WHEN R.STATUSRESERVA IN (0, 1, 7, 8, 5, 6) THEN (
		                                    SELECT
			                                    ORG.DESCRICAO
		                                    FROM
			                                    ORIGEMRESERVA ORG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND ORG.IDORIGEM = OREO.IDORIGEM
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MIN(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    WHEN R.STATUSRESERVA IN (3, 4) THEN (
		                                    SELECT
			                                    ORG.DESCRICAO
		                                    FROM
			                                    ORIGEMRESERVA ORG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND ORG.IDORIGEM = OREO.IDORIGEM
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MAX(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    WHEN R.STATUSRESERVA = 2
		                                    AND R.DATAPARTPREVISTA > PH.DATASISTEMA THEN (
		                                    SELECT
			                                    ORG.DESCRICAO
		                                    FROM
			                                    ORIGEMRESERVA ORG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND ORG.IDORIGEM = OREO.IDORIGEM
			                                    AND OREO.DATA = PH.DATASISTEMA)
		                                    ELSE (
		                                    SELECT
			                                    ORG.DESCRICAO
		                                    FROM
			                                    ORIGEMRESERVA ORG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND ORG.IDORIGEM = OREO.IDORIGEM
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MAX(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
	                                    END AS ORIGEM,
	                                    TO_CHAR(R.ADULTOS)|| '/' || TO_CHAR(R.CRIANCAS1)|| '/' || TO_CHAR(R.CRIANCAS2) AS NHOSPEDES,
	                                    CLI.RAZAOSOCIAL AS RAZAOSOCIALCLIENTEHOTEL,
	                                    CLI.NOME AS CLIENTEHOTEL,
	                                    PO.NOME AS POSTO,
	                                    CONT.CODCONTRATO,
	                                    M.INCOGNITO,
	                                    M.SENHATELEFONIA,
	                                    TRUH.DESCRICAO AS TIPOUH,
	                                    R.OBSCMNET,
	                                    CASE
		                                    WHEN (
		                                    SELECT
			                                    PCH.DESCRICAO
		                                    FROM
			                                    PACOTEHOTEL PCH,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND PCH.IDPACOTE = OREO.IDPACOTE
			                                    AND PCH.IDHOTEL = OREO.IDHOTEL
			                                    AND PH.IDHOTEL = OREO.IDHOTEL
			                                    AND OREO.DATA = PH.DATASISTEMA) IS NOT NULL                                                                 
                                                 THEN (
		                                    SELECT
			                                    PCH.DESCRICAO
		                                    FROM
			                                    PACOTEHOTEL PCH,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND PCH.IDPACOTE = OREO.IDPACOTE
			                                    AND PCH.IDHOTEL = OREO.IDHOTEL
			                                    AND PH.IDHOTEL = OREO.IDHOTEL
			                                    AND OREO.DATA = PH.DATASISTEMA)
		                                    ELSE CASE
			                                    WHEN (R.DATACHEGPREVISTA > PH.DATASISTEMA
				                                    AND                                                                        
                                                            (
				                                    SELECT
					                                    PCH.DESCRICAO
				                                    FROM
					                                    PACOTEHOTEL PCH,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND PCH.IDPACOTE = OREO.IDPACOTE
					                                    AND PCH.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = R.DATACHEGPREVISTA) IS NOT NULL )                                                                 
                                                 THEN                                                                                                                            
                                                            (
			                                    SELECT
				                                    PCH.DESCRICAO
			                                    FROM
				                                    PACOTEHOTEL PCH,
				                                    ORCAMENTORESERVA OREO
			                                    WHERE
				                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND PCH.IDPACOTE = OREO.IDPACOTE
				                                    AND PCH.IDHOTEL = OREO.IDHOTEL
				                                    AND PH.IDHOTEL = OREO.IDHOTEL
				                                    AND OREO.DATA = R.DATACHEGPREVISTA)
			                                    ELSE CASE
				                                    WHEN R.STATUSRESERVA = 3
				                                    AND                                                                                   
                                                                 (
				                                    SELECT
					                                    PCH.DESCRICAO
				                                    FROM
					                                    PACOTEHOTEL PCH,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND PCH.IDPACOTE = OREO.IDPACOTE
					                                    AND PCH.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = (
					                                    SELECT
						                                    MAX(DATA)
					                                    FROM
						                                    ORCAMENTORESERVA OREO1
					                                    WHERE
						                                    OREO1.IDRESERVASFRONT = OREO.IDRESERVASFRONT )) IS NOT NULL                  
                                                                THEN (
				                                    SELECT
					                                    PCH.DESCRICAO
				                                    FROM
					                                    PACOTEHOTEL PCH,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND PCH.IDPACOTE = OREO.IDPACOTE
					                                    AND PCH.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = (
					                                    SELECT
						                                    MAX(DATA)
					                                    FROM
						                                    ORCAMENTORESERVA OREO1
					                                    WHERE
						                                    OREO1.IDRESERVASFRONT = OREO.IDRESERVASFRONT ))
				                                    ELSE PAC.DESCRICAO
			                                    END
		                                    END
	                                    END AS PACOTE,
	                                    CASE
		                                    WHEN (
		                                    SELECT
			                                    THO.DESCRICAO
		                                    FROM
			                                    TARIFAHOTEL THO,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND THO.IDTARIFA = OREO.IDTARIFA
			                                    AND THO.IDHOTEL = OREO.IDHOTEL
			                                    AND PH.IDHOTEL = OREO.IDHOTEL
			                                    AND OREO.DATA = PH.DATASISTEMA) IS NOT NULL                                                                            
                                                 THEN (
		                                    SELECT
			                                    THO.DESCRICAO
		                                    FROM
			                                    TARIFAHOTEL THO,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND THO.IDTARIFA = OREO.IDTARIFA
			                                    AND THO.IDHOTEL = OREO.IDHOTEL
			                                    AND PH.IDHOTEL = OREO.IDHOTEL
			                                    AND OREO.DATA = PH.DATASISTEMA)
		                                    ELSE CASE
			                                    WHEN (R.DATACHEGPREVISTA > PH.DATASISTEMA
				                                    AND                                                                              
                                                            (
				                                    SELECT
					                                    THO.DESCRICAO
				                                    FROM
					                                    TARIFAHOTEL THO,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND THO.IDTARIFA = OREO.IDTARIFA
					                                    AND THO.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = R.DATACHEGPREVISTA) IS NOT NULL )                                                                 
                                                 THEN                                                                                                                            
                                                            (
			                                    SELECT
				                                    THO.DESCRICAO
			                                    FROM
				                                    TARIFAHOTEL THO,
				                                    ORCAMENTORESERVA OREO
			                                    WHERE
				                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND THO.IDTARIFA = OREO.IDTARIFA
				                                    AND THO.IDHOTEL = OREO.IDHOTEL
				                                    AND PH.IDHOTEL = OREO.IDHOTEL
				                                    AND OREO.DATA = R.DATACHEGPREVISTA)
			                                    ELSE CASE
				                                    WHEN R.STATUSRESERVA = 3
				                                    AND                                                                                   
                                                                 (
				                                    SELECT
					                                    THO.DESCRICAO
				                                    FROM
					                                    TARIFAHOTEL THO,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND THO.IDTARIFA = OREO.IDTARIFA
					                                    AND THO.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = (
					                                    SELECT
						                                    MAX(DATA)
					                                    FROM
						                                    ORCAMENTORESERVA OREO1
					                                    WHERE
						                                    OREO1.IDRESERVASFRONT = OREO.IDRESERVASFRONT )) IS NOT NULL                  
                                                                THEN (
				                                    SELECT
					                                    THO.DESCRICAO
				                                    FROM
					                                    TARIFAHOTEL THO,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND THO.IDTARIFA = OREO.IDTARIFA
					                                    AND THO.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = (
					                                    SELECT
						                                    MAX(DATA)
					                                    FROM
						                                    ORCAMENTORESERVA OREO1
					                                    WHERE
						                                    OREO1.IDRESERVASFRONT = OREO.IDRESERVASFRONT ))
				                                    ELSE TAR.DESCRICAO
			                                    END
		                                    END
	                                    END AS TARIFA,
	                                    (
	                                    SELECT
		                                    TO_NUMBER(COUNT(*))
	                                    FROM
		                                    CONTASFRONT
	                                    WHERE
		                                    (IDRESERVASFRONT = R.IDRESERVASFRONT)
			                                    AND (IDHOSPEDE = M.IDHOSPEDE)
				                                    AND DATAENCREAL IS NULL) AS NAOENCERRADAS 
                                     ,
	                                    H.NOMESOCIAL,
	                                    LPAD(' ', 20, ' ') AS STATUSDARESERVA                                                                                      
                                     ,
	                                    FX.TIPOETARIO,
	                                    H.NOME || ' ' || H.SOBRENOME AS NOMEHOSPEDECOMPLETO
                                    FROM
	                                    STATUSRESERVA STAT,
	                                    UH U,
	                                    TARIFAHOTEL TAR,
	                                    PACOTEHOTEL PAC,
	                                    TIPOUH T,
	                                    TIPOUH TRUH,
	                                    TIPOHOSPEDE TH,
	                                    PARAMHOTEL PH,
	                                    CONTRCLIHOTEL CONT,
	                                    RESERVAGRUPO RG,
	                                    ROOMLISTVHF RL,
	                                    PESSOA PHOSP,
	                                    PESSOA CLI,
	                                    PESSOA HT,
	                                    PESSOA PO,
	                                    HOSPEDE H,
	                                    RESERVASFRONT R,
	                                    MOVIMENTOHOSPEDES M,
	                                    FAIXAETARIA FX,
	                                    (
	                                    SELECT
		                                    DISTINCT IDRESERVASFRONT
	                                    FROM
		                                    RESERVASTS) RESERVASTS,
	                                    (
	                                    SELECT
		                                    IDRESERVASFRONT,
		                                    SUM(1) AS QTDAGENDA
	                                    FROM
		                                    MENSAGEMCM
	                                    WHERE
		                                    LIDA = 0
		                                    AND NOMEREMETENTE IS NULL
		                                    AND IDRESERVASFRONT IS NOT NULL
	                                    GROUP BY
		                                    IDRESERVASFRONT) AGENDA,
	                                    (
	                                    SELECT
		                                    IDRESERVASFRONT,
		                                    IDDESTINATARIO,
		                                    SUM(1) AS QTDMENSAGEM
	                                    FROM
		                                    MENSAGEMCM
	                                    WHERE
		                                    IDRESERVASFRONT IS NOT NULL
		                                    AND IDDESTINATARIO IS NOT NULL
		                                    AND LIDA = 0
		                                    AND NOMEREMETENTE IS NOT NULL
	                                    GROUP BY
		                                    IDRESERVASFRONT,
		                                    IDDESTINATARIO) MENSAGEM,
	                                    (SELECT
		                                    DISTINCT
	                                        R.IDRESERVASFRONT,
	                                        TO_CHAR(PJ.NUMEROPROJETO)||'-'||TO_CHAR(VC.NUMEROCONTRATO) AS NUMEROCONTRATO,
		                                    PCL.IDPESSOA, 
		                                    PCL.NOME, 
		                                    PCL.NUMDOCUMENTO,
		                                    PCL.EMAIL,
		                                    VC.FLGCANCELADO
	                                      FROM 
	                                          LANCPONTOSTS LP, 
	                                          VENDAXCONTRATOTS VC,
                                              PROJETOTS PJ,
	                                          RESERVASFRONT R,
	                                          VENDATS V,
	                                          ATENDCLIENTETS A,
	                                          PESSOA PCL
	                                      WHERE
	                                       LP.IDRESERVASFRONT = R.IDRESERVASFRONT
	                                       AND LP.IDVENDAXCONTRATO  = VC.IDVENDAXCONTRATO
                                           AND VC.IDPROJETOTS = PJ.IDPROJETOTS
	                                       AND V.IDVENDATS = VC.IDVENDATS
	                                       AND V.IDATENDCLIENTETS = A.IDATENDCLIENTETS
	                                       AND A.IDCLIENTE = PCL.IDPESSOA
	                                     ) PRO
                                    WHERE
	                                ( 
                                      {txtPeriodo}      
                                    )
	                                AND (1900693 NOT IN (
	                                SELECT
		                                GU.IDUSUARIO
	                                FROM
		                                GRUPOUSU GU,
		                                GRPUSUACESSORES GR
	                                WHERE
		                                GR.IDHOTEL = R.IDHOTEL
		                                AND GU.IDGRUPO = GR.IDGRUPO)
	                                OR
                                               (1900693 IN (
	                                SELECT
		                                GU.IDUSUARIO
	                                FROM
		                                GRUPOUSU GU,
		                                GRUPOUSU GURES
	                                WHERE
		                                GURES.IDUSUARIO = R.USUARIO
		                                AND GU.IDGRUPO = GURES.IDGRUPO)))
	                                AND TH.IDTIPOHOSPEDE = M.IDTIPOHOSPEDE
	                                AND PHOSP.IDPESSOA = H.IDHOSPEDE
	                                AND R.IDHOTEL = HT.IDPESSOA
	                                AND R.IDHOTEL = T.IDHOTEL
	                                AND R.IDHOTEL = TRUH.IDHOTEL
	                                AND R.IDHOTEL = TH.IDHOTEL
	                                AND R.IDHOTEL = PH.IDHOTEL
	                                AND r.idtarifa = tar.idtarifa (+)
	                                AND R.TIPOUHESTADIA = T.IDTIPOUH
	                                AND R.TIPOUHTARIFA = TRUH.IDTIPOUH
	                                AND M.IDRESERVASFRONT = R.IDRESERVASFRONT
	                                AND M.IDHOSPEDE = H.IDHOSPEDE
	                                AND R.IDHOTEL = TAR.IDHOTEL (+)
	                                AND R.STATUSRESERVA = STAT.STATUSRESERVA
	                                AND R.IDRESERVASFRONT = RESERVASTS.IDRESERVASFRONT
	                                AND R.IDHOTEL = U.IDHOTEL (+)
	                                AND R.CODUH = U.CODUH (+)
	                                AND R.CLIENTERESERVANTE = CLI.IDPESSOA (+)
	                                AND R.CLIENTEHOSPEDE = PO.IDPESSOA (+)
	                                AND R.IDPACOTE = PAC.IDPACOTE (+)
	                                AND R.IDHOTEL = PAC.IDHOTEL (+)
	                                AND R.IDROOMLIST = RL.IDROOMLIST (+)
	                                AND RL.IDRESERVAGRUPO = RG.IDRESERVAGRUPO (+)
	                                AND R.IDHOTEL = CONT.IDHOTEL (+)
	                                AND R.CLIENTERESERVANTE = CONT.IDFORCLI (+)
	                                AND R.CONTRATOINICIAL = CONT.CODCONTRATO (+)
	                                AND R.IDRESERVASFRONT = AGENDA.IDRESERVASFRONT (+)
	                                AND M.IDRESERVASFRONT = MENSAGEM.IDRESERVASFRONT (+)
	                                AND M.IDHOSPEDE = MENSAGEM.IDDESTINATARIO (+)
	                                AND H.IDFAIXAETARIA = FX.IDFAIXAETARIA (+)
	                                AND PRO.IDRESERVASFRONT = R.IDRESERVASFRONT  
                                    {txtExibirTodosOsHospedes} ");

            if (!string.IsNullOrEmpty(searchModel.Hotel))
            {
                sb.AppendLine($" AND LOWER(HT.NOME) LIKE '%{searchModel.Hotel.ToLower().TrimEnd()}%'");
            }

            if (!string.IsNullOrEmpty(searchModel.NomeCliente))
            {
                sb.AppendLine($" AND (LOWER(PRO.NOME) LIKE '%{searchModel.NomeCliente.ToLower().TrimEnd()}%' OR LOWER(PHOSP.NOME) LIKE '%{searchModel.NomeCliente.ToLower().TrimEnd()}%')");
            }

            if (!string.IsNullOrEmpty(searchModel.NumDocumentoCliente))
            {
                sb.AppendLine($" AND exists(select dp.IdPessoa From DocPessoa dp Where Replace(Replace(Replace(dp.NumDocumento,'.',''),'-',''),'/','') like '{Helper.ApenasNumeros(searchModel.NumDocumentoCliente)}%' and dp.IdPessoa = PRO.IDPESSOA ) ");
            }

            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                sb.AppendLine($" AND TO_CHAR(PRO.NUMEROCONTRATO) LIKE '%{searchModel.NumeroContrato.TrimEnd()}%' ");
            }

            if (searchModel.NumReserva.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" AND R.NUMRESERVA = {searchModel.NumReserva} ");
            }

            if (!string.IsNullOrEmpty(searchModel.StatusReserva))
            {
                sb.AppendLine($" AND LOWER(STAT.DESCRICAO) LIKE '%{searchModel.StatusReserva.ToLower().TrimEnd()}%' ");
            }

            var sql = sb.ToString();

            var totalRegistros = await _repository.CountTotalEntry(sql, parameters.ToArray());
            if (totalRegistros == 0)
                return (1, 1, new List<ReservaGeralTsModel>());


            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros) < 30 ? Convert.ToInt32(totalRegistros) : 30;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 && searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > Convert.ToInt32(totalRegistros))
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros);

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;


            long totalPageValidation = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
            if (totalPageValidation < searchModel.NumeroDaPagina)
                searchModel.NumeroDaPagina = Convert.ToInt32(totalPageValidation);

            sb.AppendLine(@" ORDER BY
                                R.NUMRESERVA ");

            var result = (await _repository.FindBySql<ReservaGeralTsModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPageValidation), result);
                }

            }

            return (1, 1, result);

        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaTsModel> reservas)?> GetMinhasReservasGeralComConsumoPontos(SearchMinhasReservaTsModel searchModel)
        {
            var empresaCmId = _configuration.GetValue<int>("EmpresaCMId", 3);

            var parameters = new List<Parameter>();
            var txtPeriodoCheckin = "";
            var txtPeriodoCheckout = "";
            var txtRmDataCheckinReserva = "";
            var txtRmDataCheckoutReserva = "";

            if (searchModel.CheckinInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckinInicial = DateTime.Today.AddYears(-30);
            }

            if (searchModel.CheckinFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckinFinal = DateTime.Today.AddYears(10);
            }

            if (searchModel.CheckoutInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckoutInicial = DateTime.Today.AddYears(-30);
            }

            if (searchModel.CheckoutFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.CheckoutFinal = DateTime.Today.AddYears(10);
            }


            if (searchModel.CheckinInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("chegadaInicial", searchModel.CheckinInicial.GetValueOrDefault().Date));
                txtPeriodoCheckin = " AND COALESCE(RF.DATACHEGADAREAL,RF.DATACHEGPREVISTA) >= :chegadaInicial ";
                txtRmDataCheckinReserva = " AND RM.DATACHEGADA >= :chegadaInicial ";
            }

            if (searchModel.CheckinFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("chegadaFinal", searchModel.CheckinFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                txtPeriodoCheckin += " AND COALESCE(RF.DATACHEGADAREAL,RF.DATACHEGPREVISTA) <= :chegadaFinal ";
                txtRmDataCheckinReserva += " AND RM.DATACHEGADA >= :chegadaFinal ";
            }

            if (searchModel.CheckoutInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("partidaInicial", searchModel.CheckoutInicial.GetValueOrDefault().Date));
                txtPeriodoCheckout = " AND COALESCE(RF.DATAPARTIDAREAL,RF.DATAPARTPREVISTA) >= :partidaInicial ";
                txtRmDataCheckoutReserva = " AND RM.DATAPARTIDA >= :partidaInicial ";
            }

            if (searchModel.CheckoutFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("partidaFinal", searchModel.CheckoutFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                txtPeriodoCheckout += " AND COALESCE(RF.DATAPARTIDAREAL,RF.DATAPARTPREVISTA) <= :partidaFinal ";
                txtRmDataCheckoutReserva += " AND RM.DATAPARTIDA <= :partidaFinal ";
            }


            var loggedUser = await _repository.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            var pessoaVinculadaSistema = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
            if (pessoaVinculadaSistema == null)
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            if (string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) || !Helper.IsNumeric(pessoaVinculadaSistema.PessoaProvider))
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");


            var sb = new StringBuilder(@$"(SELECT
                                              DISTINCT
                                              RF.NUMRESERVA AS NUMRESERVA, 
                                              CASE WHEN (RF.NUMVOO IS NULL) OR (RTRIM(RF.NUMVOO) = '') THEN 'Não' ELSE 'Sim' END AS NUMVOO,
                                              CASE WHEN (SELECT MIN(LOCRESERVA) FROM RESERVASFRONT WHERE IDRESERVAMULTROOM = RF.IDRESERVASFRONT GROUP BY IDRESERVASFRONT) > 0 THEN
                                                RF.LOCRESERVA || '/' || (SELECT LOCRESERVA FROM RESERVASFRONT WHERE IDRESERVAMULTROOM = RF.IDRESERVASFRONT)
                                              ELSE
                                                TO_CHAR(RF.LOCRESERVA)
                                              END LOCALIZADOR,
                                              NVL(RF.DATACHEGPREVISTA, RF.DATACHEGADAREAL) AS CHECKIN,
                                              NVL(RF.DATAPARTPREVISTA, RF.DATAPARTIDAREAL) AS CHECKOUT,
                                              ST.DESCRICAO AS STATUS,
                                              RF.DATACANCELAMENTO,
                                              H.NOME AS HOTEL,
                                              TUH.DESCRICAO AS TIPOUH,
                                              CASE WHEN (SELECT COUNT(*) FROM RESERVASFRONT WHERE NUMRESERVAPRINC = RF.NUMRESERVA) > 1 THEN LPAD(TUH.CODREDUZIDO, 20, ' ') ELSE TUH.CODREDUZIDO END CODTIPOUH,
                                              CR.NOME AS NOMECLIENTE,
                                              DECODE(LP.FLGMIGRADO,'N','Normal','S','Migrada') AS TIPORESERVA,
                                              DECODE(LP.FLGMIGRADO,'S','Migrada', DECODE(RB.IDRESERVASFRONT, NULL, DECODE(RCI.IDRESERVASRCI, NULL, 'Normal', 'RCI'), 'BULK RCI')) AS TIPOLANCAMENTO,
                                              TO_CHAR(PJ.NUMEROPROJETO)||'-'||TO_CHAR(VXC.NUMEROCONTRATO) AS PROJETOXCONTRATO,
                                              TO_CHAR(VXC.NUMEROCONTRATO) AS NUMEROCONTRATO,
                                              US.NOMEUSUARIO AS CRIADAPOR,
                                              NVL(PR.VALOR,0) AS PONTORESERVA,
                                              NVL(LTX.VLRTAXA,0) AS TAXAMANUTENCAO,
                                              CASE WHEN ((NVL(LTX.VLRTAXA,0) = 0) AND (LTX.VLRTAXAISENTA IS NOT NULL)) THEN 'Sim' ELSE 'Não' END TAXAISENTA,
                                              CASE WHEN LS.IDLISTAESPERA IS NOT NULL THEN 'Sim' ELSE 'Não' END AS LISTAESPERA,
                                              V.IDVENDATS,
                                              DTTX.DATALANCAMENTO AS DATAPAGTAXA,
                                              ( SELECT NVL(SUM(ORC.VALOR),0) FROM ORCAMENTORESERVA ORC WHERE ORC.IDRESERVASFRONT = RF.IDRESERVASFRONT ) VALORPENSAO,
                                              ROUND(TO_NUMBER( DECODE( NVL(C.VALORPONTO,0),0, DECODE(NVL(C.VALORPERCPONTO,0),0,
                                                                   DECODE( NVL(C.NUMEROPONTOS,0),0,0, NVL(VAL.TOTAL, VXC.VALORFINAL) / C.NUMEROPONTOS),
                                                                   (NVL(VAL.TOTAL, VXC.VALORFINAL) * C.VALORPERCPONTO)/ 100),
                                                                   NVL(C.VALORPONTO,0))),6) AS VALORPONTO,
                                              ROUND(PR.VALOR * TO_NUMBER((DECODE(NVL(C.VALORPONTO,0),0, DECODE(NVL(C.VALORPERCPONTO,0),0,
                                              DECODE(NVL(C.NUMEROPONTOS,0),0,0, NVL(VAL.TOTAL, VXC.VALORFINAL) / C.NUMEROPONTOS),
                                              (NVL(VAL.TOTAL, VXC.VALORFINAL) * C.VALORPERCPONTO)/ 100), NVL(C.VALORPONTO,0)))),6) AS VALORPONTOS,
                                              MFR.DESCRICAO AS MOTIVO_FORCAR_RESERVA,
                                              MFR.JUSTIFORACONT AS JUST_FORCAR_RESERVA,
                                               (SELECT SUM(LF.VLRLANCAMENTO) AS TOTAL
                                                  FROM CONTASFRONT CF, LANCAMENTOSFRONT LF, TIPODEBCREDHOTEL T, CONTRATOTSXHOTEL CH, SERVICOSHOTEL SH, LANCPONTOSTS LP, VENDAXCONTRATOTS VC, RESERVASFRONT R
                                                 WHERE CF.IDCONTA           = LF.IDCONTA
                                                   AND LF.IDTIPODEBCRED     = T.IDTIPODEBCRED
                                                   AND CH.IDSERVICOUSOPONTO = SH.IDSERVICOHOTEL
                                                   AND SH.IDTIPODEBCRED     = T.IDTIPODEBCRED
                                                   AND CF.IDRESERVASFRONT   = LP.IDRESERVASFRONT
                                                   AND LP.IDVENDAXCONTRATO  = VC.IDVENDAXCONTRATO
                                                   AND CH.IDCONTRATOTS      = VC.IDCONTRATOTS
                                                   AND CH.IDHOTEL           = CF.IDHOTEL
                                                   AND LF.IDHOTEL           = T.IDHOTEL
                                                   AND LF.IDHOTEL           = CH.IDHOTEL
                                                   AND LF.IDHOTEL           = R.IDHOTEL
                                                   AND R.IDHOTEL            = RF.IDHOTEL
                                                   AND CF.IDRESERVASFRONT   = R.IDRESERVASFRONT
                                                   AND R.IDRESERVASFRONT    = RF.IDRESERVASFRONT
                                                   AND R.STATUSRESERVA      IN (2,3,4)
                                                 GROUP BY CF.IDRESERVASFRONT) AS VLRCONTABPONTOS,
                                               (SELECT SUM(LF.VLRLANCAMENTO) AS TOTAL
                                                  FROM CONTASFRONT CF, LANCAMENTOSFRONT LF, TIPODEBCREDHOTEL T, CONTRATOTSXHOTEL CH, TARIFAHOTEL TH, LANCPONTOSTS LP, VENDAXCONTRATOTS VC, RESERVASFRONT R
                                                 WHERE CF.IDCONTA           = LF.IDCONTA
                                                   AND LF.IDTIPODEBCRED     = T.IDTIPODEBCRED
                                                   AND CH.IDTARIFA          = TH.IDTARIFA
                                                   AND TH.IDTIPODCDIARIA    = T.IDTIPODEBCRED
                                                   AND CF.IDRESERVASFRONT   = LP.IDRESERVASFRONT
                                                   AND LP.IDVENDAXCONTRATO  = VC.IDVENDAXCONTRATO
                                                   AND CH.IDCONTRATOTS      = VC.IDCONTRATOTS
                                                   AND CH.IDHOTEL           = CF.IDHOTEL
                                                   AND LF.IDHOTEL           = T.IDHOTEL
                                                   AND LF.IDHOTEL           = CH.IDHOTEL
                                                   AND LF.IDHOTEL           = R.IDHOTEL
                                                   AND R.IDHOTEL            = RF.IDHOTEL
                                                   AND CF.IDRESERVASFRONT   = R.IDRESERVASFRONT
                                                   AND R.IDRESERVASFRONT    = RF.IDRESERVASFRONT
                                                   AND R.STATUSRESERVA     IN (2,3,4)
                                                 GROUP BY CF.IDRESERVASFRONT) AS VLRCONTABTAXA, RF.ADULTOS, RF.CRIANCAS1, RF.CRIANCAS2, (SELECT COUNT(*) FROM RESERVASFRONT WHERE NUMRESERVAPRINC = RF.NUMRESERVA) AS QTDRESERVAS,
                                                 CASE WHEN FR1.IDFRACIONAMENTOTS > 0 THEN 'Abertura' WHEN FR2.IDFRACIONAMENTOTS > 0 THEN 'Fechamento' ELSE ' ' END FRACIONAMENTO,
                                                 Nvl(RF.TipoDeUso,'UP') AS TipoDeUso
                                             FROM   
                                                VENDAXCONTRATOTS VXC, 
                                                MOVIMENTOHOSPEDES M,    
                                                USUARIOSISTEMA US, 
                                                STATUSRESERVA ST, 
                                                RESERVASFRONT RF, 
                                                RESERVASBULKTS RB, 
                                                LISTAESPERATS LS,
                                                PROJETOTS PJ, 
                                                HOSPEDE HP, 
                                                TIPOUH TUH, 
                                                PESSOA CR, 
                                                VENDATS V, 
                                                PESSOA H, 
                                                ATENDCLIENTETS A, 
                                                HOTEL HO, 
                                                CONTRATOTS C, 
                                                REVCONTRATOTS RV,
                                                FRACIONAMENTOTS FR1, 
                                                FRACIONAMENTOTS FR2,
                                                (SELECT DISTINCT IDRESERVASFRONT, IDRESERVASRCI  FROM RESERVASRCI) RCI,
                                                (SELECT IDRESERVASFRONT, MIN(IDVENDAXCONTRATO) AS IDVENDAXCONTRATO,
                                                      SUM(NVL(TO_NUMBER(DECODE(IDTIPOLANCPONTOTS,1,NUMEROPONTOS)),0)) AS NUMPONTOS,
                                                      MAX(FLGMIGRADO) AS FLGMIGRADO, MIN(IDHOTEL) AS IDHOTEL, MAX(DATALANCAMENTO) AS DATALANCAMENTO
                                                 FROM LANCPONTOSTS
                                                GROUP BY IDRESERVASFRONT) LP,
                                              (SELECT IDRESERVASFRONT, SUM(NVL(NUMEROPONTOS,0)) AS VALOR FROM LANCPONTOSTS WHERE IDTIPOLANCPONTOTS IN (1,4) GROUP BY IDRESERVASFRONT) PR,
                                              (SELECT LP.IDRESERVASFRONT, SUM(L.VLRLANCAMENTO) AS VLRTAXA, LP.VLRTAXAISENTA 
                                                 FROM LANCAMENTOTS L, LANCPONTOSTS LP 
                                                WHERE L.IDLANCPONTOSTS = LP.IDLANCPONTOSTS
                                                  AND L.IDTIPOLANCAMENTO = 5
                                                  AND LP.IDTIPOLANCPONTOTS IN (1,4)
                                                  AND LP.IDRESERVASFRONT IS NOT NULL
                                                GROUP BY LP.IDRESERVASFRONT, LP.VLRTAXAISENTA) LTX,
                                              (SELECT LP.IDRESERVASFRONT, MAX(L.DATALANCAMENTO) AS DATALANCAMENTO
                                                 FROM LANCAMENTOTS L, LANCPONTOSTS LP
                                                WHERE L.IDLANCPONTOSTS = LP.IDLANCPONTOSTS
                                                  AND L.IDTIPOLANCAMENTO = 6
                                                  AND LP.IDTIPOLANCPONTOTS IN (1,4)
                                                  AND LP.IDRESERVASFRONT IS NOT NULL
                                                GROUP BY LP.IDRESERVASFRONT) DTTX,
                                              (SELECT LANCPONTOSTS.IDRESERVASFRONT, NVL(LANCPONTOSTS.IDUSUARIORESERVA, NVL(LANCPONTOSTS.IDUSUARIO, LANCPONTOSTS.IDUSUARIOLOGADO)) AS IDUSUARIO
                                                 FROM LANCPONTOSTS, (SELECT MIN(LP.IDLANCPONTOSTS) AS IDLANCPONTOSTS FROM LANCPONTOSTS LP GROUP BY LP.IDRESERVASFRONT) IDS
                                                WHERE LANCPONTOSTS.IDLANCPONTOSTS = IDS.IDLANCPONTOSTS) USUARIORES,
                                              (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'D',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS UTILIZACAO
                                                 FROM LANCPONTOSTS LP, RESERVASFRONT RF, RESERVAMIGRADATS RM
                                                WHERE NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS = 1)
                                                  AND LP.IDRESERVASFRONT  = RF.IDRESERVASFRONT (+)
                                                  AND LP.IDRESERVAMIGRADA = RM.IDRESERVAMIGRADA (+)
                                                  AND LP.IDTIPOLANCPONTOTS <> 8
                                                GROUP BY IDVENDAXCONTRATO) U,
                                              (SELECT VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS) AS IDREVCONTRATOTS, ABS(SUM(L.VLRLANCAMENTO)) AS TOTAL
                                                 FROM LANCAMENTOTS L, VENDATS V, VENDAXCONTRATOTS VC, CONTRATOTS C, AJUSTEFINANCTS AJ, REVCONTRATOTS R
                                                WHERE L.IDVENDATS      = V.IDVENDATS
                                                  AND V.IDVENDATS      = VC.IDVENDATS
                                                  AND VC.IDCONTRATOTS  = C.IDCONTRATOTS
                                                  AND L.IDAJUSTEFINANCTS  = AJ.IDAJUSTEFINANCTS (+)
                                                  AND AJ.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                                                  AND (L.IDTIPOLANCAMENTO IN (1,7) OR L.IDTIPODEBCRED = C.IDTIPODCJUROS OR L.IDTIPODEBCRED = C.IDTIPODCCONTRATO)
                                                  AND ((VC.FLGCANCELADO = 'N' AND VC.FLGREVERTIDO = 'N'         AND L.IDMOTIVOESTORNO IS NULL    AND L.IDLANCESTORNO IS NULL)
                                                   OR  (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NULL     AND L.IDCANCCONTRATOTS IS NULL)
                                                   OR  (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NOT NULL AND L.IDCANCCONTRATOTS IS NULL   AND L.IDAJUSTEFINANCTS IS NOT NULL )
                                                   OR  (VC.FLGCANCELADO = 'S' AND L.IDMOTIVOESTORNO IS NULL     AND L.IDCANCCONTRATOTS IS NULL ))
                                                  AND L.IDVENDATS IS NOT NULL
                                                GROUP BY VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS)) VAL,
                                              (SELECT L.IDRESERVASFRONT, M.DESCRICAO, L.JUSTIFORACONT
                                                 FROM LANCPONTOSTS L, MOTIVOTS M
                                                WHERE L.IDMOTIVOFORACONT = M.IDMOTIVOTS
                                                  AND L.IDTIPOLANCPONTOTS = 1
                                                GROUP BY L.IDRESERVASFRONT, M.DESCRICAO, L.JUSTIFORACONT) MFR
                                             WHERE RF.IDRESERVASFRONT     = RB.IDRESERVASFRONT (+)
                                               AND RF.IDRESERVASFRONT     = RCI.IDRESERVASFRONT (+)
                                               AND RF.IDRESERVASFRONT     = LS.IDRESERVASFRONT(+)
                                               AND RF.IDRESERVASFRONT     = MFR.IDRESERVASFRONT(+)
                                               AND RF.IDRESERVASFRONT     = FR1.IDRESERVASFRONT1 (+)
                                               AND RF.IDRESERVASFRONT     = FR2.IDRESERVASFRONT2 (+)
                                               AND LP.IDRESERVASFRONT     = LTX.IDRESERVASFRONT(+)
                                               AND LP.IDRESERVASFRONT     = DTTX.IDRESERVASFRONT(+)
                                               AND VXC.IDVENDAXCONTRATO   = U.IDVENDAXCONTRATO (+)
                                               AND VXC.IDVENDAXCONTRATO   = VAL.IDVENDAXCONTRATO (+)
                                               AND VXC.IDVENDAXCONTRATO   = RV.IDVENDAXCONTRNOVO (+)
                                               AND RF.IDRESERVASFRONT     = USUARIORES.IDRESERVASFRONT
                                               AND VXC.IDCONTRATOTS       = C.IDCONTRATOTS
                                               AND USUARIORES.IDUSUARIO   = US.IDUSUARIO
                                               AND RF.IDHOTEL             = TUH.IDHOTEL
                                               AND RF.TIPOUHTARIFA        = TUH.IDTIPOUH
                                               AND RF.IDRESERVASFRONT     = LP.IDRESERVASFRONT
                                               AND VXC.IDVENDAXCONTRATO   = LP.IDVENDAXCONTRATO
                                               AND V.IDVENDATS            = VXC.IDVENDATS
                                               AND PJ.IDPROJETOTS         = VXC.IDPROJETOTS
                                               AND ST.STATUSRESERVA       = RF.STATUSRESERVA
                                               AND H.IDPESSOA             = RF.IDHOTEL
                                               AND V.IDATENDCLIENTETS     = A.IDATENDCLIENTETS
                                               AND A.IDCLIENTE            = CR.IDPESSOA
                                               AND CR.IDPESSOA = {pessoaVinculadaSistema.PessoaProvider}
                                               AND RF.IDHOTEL             = M.IDHOTEL
                                               AND RF.IDRESERVASFRONT     = M.IDRESERVASFRONT
                                               AND HP.IDHOSPEDE           = M.IDHOSPEDE
                                               AND PR.IDRESERVASFRONT     = LP.IDRESERVASFRONT
                                               AND A.IDHOTEL              = HO.IDHOTEL
                                               AND ((RV.IDREVCONTRATOTS IS NULL AND VAL.IDREVCONTRATOTS IS NULL) OR (RV.IDREVCONTRATOTS = VAL.IDREVCONTRATOTS))
                                               AND HO.IDPESSOA            = {empresaCmId}
                                               AND M.PRINCIPAL            = 'S'
                                               {txtPeriodoCheckin} {txtPeriodoCheckout} 
                                            ");


            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                sb.AppendLine($" AND TO_CHAR(VXC.NUMEROCONTRATO) LIKE '%{searchModel.NumeroContrato.TrimEnd()}%' ");
            }

            if (searchModel.NumReserva.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" AND RF.NUMRESERVA = {searchModel.NumReserva} ");
            }

            if (!string.IsNullOrEmpty(searchModel.StatusReserva))
            {
                sb.AppendLine($" AND LOWER(ST.DESCRICAO) LIKE '%{searchModel.StatusReserva.ToLower().TrimEnd()}%' ");
            }

            sb.AppendLine($@")
                                            UNION ALL
                                            (
                                            SELECT
                                                DISTINCT
                                                RM.NUMRESERVA AS NUMRESERVA,
                                                TO_CHAR(RM.LOCRESERVA) AS LOCALIZADOR, 'Não' AS NUMVOO,
                                                RM.DATACHEGADA AS CHECKIN,
                                                RM.DATAPARTIDA AS CHECKOUT,
                                                DECODE(SIGN(TO_NUMBER(RM.DATAPARTIDA-PH.DATASISTEMA)),1,DECODE(SIGN(TO_NUMBER(PH.DATASISTEMA-RM.DATACHEGADA)),1,'Check-In','Confirmada'),'Check-out') AS STATUS,
                                                null AS DATACANCELAMENTO,
                                                H.NOME AS HOTEL,
                                                TUH.DESCRICAO AS TIPOUH,
                                                TUH.CODREDUZIDO AS CODTIPOUH,
                                                CL.NOME AS NOMECLIENTE,
                                                'Migrada' AS TIPORESERVA,
                                                DECODE(RCI.IDRESERVAMIGRADA,NULL,'Migrada','RCI-Migrada') AS TIPOLANCAMENTO,
                                                TO_CHAR(PJ.NUMEROPROJETO)||'-'||TO_CHAR(VXC.NUMEROCONTRATO) AS PROJETOXCONTRATO,
                                                TO_CHAR(VXC.NUMEROCONTRATO) AS NUMEROCONTRATO,
                                                US.NOMEUSUARIO AS CRIADAPOR,
                                                NVL(PR.VALOR,0) AS PONTORESERVA,
                                                NVL(LTX.VLRTAXA,0) AS TAXAMANUTENCAO,
                                                'Não' AS TAXAISENTA,
                                                CASE WHEN LS.IDLISTAESPERA IS NOT NULL THEN 'Sim' ELSE 'Não' END AS LISTAESPERA,
                                                V.IDVENDATS,
                                                DTTX.DATALANCAMENTO AS DATAPAGTAXA,
                                                ( SELECT NVL(SUM(ORC.VALOR),0) FROM ORCAMENTORESERVA ORC WHERE ORC.IDRESERVASFRONT = RM.IDRESERVAMIGRADA ) VALORPENSAO,
                                                ROUND(TO_NUMBER( DECODE( NVL(C.VALORPONTO,0),0, DECODE(NVL(C.VALORPERCPONTO,0),0,
                                                                    DECODE( NVL(C.NUMEROPONTOS,0),0,0, NVL(VAL.TOTAL, VXC.VALORFINAL) / C.NUMEROPONTOS),
                                                                    (NVL(VAL.TOTAL, VXC.VALORFINAL) * C.VALORPERCPONTO)/ 100),
                                                                    NVL(C.VALORPONTO,0))),6) AS VALORPONTO,
                                                ROUND(PR.VALOR * TO_NUMBER((DECODE(NVL(C.VALORPONTO,0),0, DECODE(NVL(C.VALORPERCPONTO,0),0, 
                                                DECODE(NVL(C.NUMEROPONTOS,0),0,0, NVL(VAL.TOTAL, VXC.VALORFINAL) / C.NUMEROPONTOS),
                                                (NVL(VAL.TOTAL, VXC.VALORFINAL) * C.VALORPERCPONTO)/ 100), NVL(C.VALORPONTO,0)))),6) AS VALORPONTOS,
                                                ' ' AS MOTIVO_FORCAR_RESERVA,
                                                ' ' AS JUST_FORCAR_RESERVA,
                                                0 AS VLRCONTABPONTOS,
                                                0 AS VLRCONTABTAXA, RM.QTDEPAX AS ADULTOS, 0 AS CRIANCAS1, 0 AS CRIANCAS2, 0 AS QTDRESERVAS,
                                                ' ' AS FRACIONAMENTO
                                                FROM VENDAXCONTRATOTS VXC, RESERVAMIGRADATS RM, USUARIOSISTEMA US, ATENDCLIENTETS A, RESERVASBULKTS RB, PARAMHOTEL PH, PROJETOTS PJ, TIPOUH TUH, VENDATS V, PESSOA CL, PESSOA H,
                                                    HOTEL HO, CONTRATOTS C, REVCONTRATOTS RV, LISTAESPERATS LS,
                                                (SELECT IDRESERVAMIGRADA, MIN(IDVENDAXCONTRATO) AS IDVENDAXCONTRATO,
                                                        SUM(NVL(TO_NUMBER(DECODE(IDTIPOLANCPONTOTS,1,NUMEROPONTOS)),0)) AS NUMPONTOS,
                                                        MAX(FLGMIGRADO) AS FLGMIGRADO, MIN(IDHOTEL) AS IDHOTEL, MAX(DATALANCAMENTO) AS DATALANCAMENTO
                                                    FROM LANCPONTOSTS
                                                GROUP BY IDRESERVAMIGRADA) LP,
                                                (SELECT DISTINCT IDRESERVAMIGRADA,IDRESERVASRCI FROM RESERVASRCI) RCI,
                                                (SELECT IDRESERVAMIGRADA, SUM(NVL(NUMEROPONTOS,0)) AS VALOR
                                                    FROM LANCPONTOSTS 
                                                WHERE IDTIPOLANCPONTOTS IN (1,4) 
                                                GROUP BY IDRESERVAMIGRADA) PR,
                                                (SELECT LP.IDRESERVAMIGRADA, SUM(L.VLRLANCAMENTO) AS VLRTAXA
                                                    FROM LANCAMENTOTS L, LANCPONTOSTS LP
                                                WHERE L.IDLANCPONTOSTS   = LP.IDLANCPONTOSTS
                                                    AND L.IDTIPOLANCAMENTO = 5
                                                    AND LP.IDTIPOLANCPONTOTS IN (1,4)
                                                    AND LP.IDRESERVAMIGRADA IS NOT NULL
                                                GROUP BY LP.IDRESERVAMIGRADA) LTX,
                                                (SELECT LP.IDRESERVAMIGRADA, MAX(L.DATALANCAMENTO) AS DATALANCAMENTO 
                                                    FROM LANCAMENTOTS L, LANCPONTOSTS LP
                                                WHERE L.IDLANCPONTOSTS   = LP.IDLANCPONTOSTS
                                                    AND L.IDTIPOLANCAMENTO = 6
                                                    AND LP.IDTIPOLANCPONTOTS IN (1,4)
                                                    AND LP.IDRESERVAMIGRADA IS NOT NULL
                                                GROUP BY LP.IDRESERVAMIGRADA) DTTX,
                                                (SELECT LANCPONTOSTS.IDRESERVAMIGRADA, NVL(LANCPONTOSTS.IDUSUARIORESERVA, NVL(LANCPONTOSTS.IDUSUARIO, LANCPONTOSTS.IDUSUARIOLOGADO)) AS IDUSUARIO
                                                    FROM LANCPONTOSTS, (SELECT MIN(LP.IDLANCPONTOSTS) AS IDLANCPONTOSTS FROM LANCPONTOSTS LP GROUP BY LP.IDRESERVAMIGRADA) IDS
                                                WHERE LANCPONTOSTS.IDLANCPONTOSTS = IDS.IDLANCPONTOSTS) USUARIORES,   
                                                (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'D',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS UTILIZACAO
                                                    FROM LANCPONTOSTS LP, RESERVASFRONT RF, RESERVAMIGRADATS RM
                                                WHERE NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS = 1)
                                                    AND LP.IDRESERVASFRONT  = RF.IDRESERVASFRONT (+)
                                                    AND LP.IDRESERVAMIGRADA = RM.IDRESERVAMIGRADA (+)
                                                    AND LP.IDTIPOLANCPONTOTS <> 8
                                                GROUP BY IDVENDAXCONTRATO) U,
                                                (SELECT VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS) AS IDREVCONTRATOTS, ABS(SUM(L.VLRLANCAMENTO)) AS TOTAL
                                                    FROM LANCAMENTOTS L, VENDATS V, VENDAXCONTRATOTS VC, CONTRATOTS C, AJUSTEFINANCTS AJ, REVCONTRATOTS R
                                                WHERE L.IDVENDATS      = V.IDVENDATS
                                                    AND V.IDVENDATS      = VC.IDVENDATS
                                                    AND VC.IDCONTRATOTS  = C.IDCONTRATOTS
                                                    AND L.IDAJUSTEFINANCTS  = AJ.IDAJUSTEFINANCTS (+)
                                                    AND AJ.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                                                    AND (L.IDTIPOLANCAMENTO IN (1,7) OR L.IDTIPODEBCRED = C.IDTIPODCJUROS OR L.IDTIPODEBCRED = C.IDTIPODCCONTRATO)
                                                    AND ((VC.FLGCANCELADO = 'N' AND VC.FLGREVERTIDO = 'N'         AND L.IDMOTIVOESTORNO IS NULL    AND L.IDLANCESTORNO IS NULL)
                                                    OR  (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NULL     AND L.IDCANCCONTRATOTS IS NULL)
                                                    OR  (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NOT NULL AND L.IDCANCCONTRATOTS IS NULL   AND L.IDAJUSTEFINANCTS IS NOT NULL )
                                                    OR  (VC.FLGCANCELADO = 'S' AND L.IDMOTIVOESTORNO IS NULL     AND L.IDCANCCONTRATOTS IS NULL ))
                                                    AND L.IDVENDATS IS NOT NULL
                                                GROUP BY VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS)) VAL
                                            WHERE RM.IDRESERVAMIGRADA     = RCI.IDRESERVAMIGRADA(+)  
                                                AND RM.IDRESERVAMIGRADA     = RB.IDRESERVASFRONT (+)
                                                AND RB.IDRESERVASFRONT      = LS.IDRESERVASFRONT(+)
                                                AND LP.IDRESERVAMIGRADA     = LTX.IDRESERVAMIGRADA(+)
                                                AND LP.IDRESERVAMIGRADA     = DTTX.IDRESERVAMIGRADA(+)
                                                AND VXC.IDVENDAXCONTRATO    = U.IDVENDAXCONTRATO (+)
                                                AND VXC.IDVENDAXCONTRATO    = VAL.IDVENDAXCONTRATO (+)
                                                AND VXC.IDVENDAXCONTRATO    = RV.IDVENDAXCONTRNOVO (+)
                                                AND A.IDATENDCLIENTETS      = V.IDATENDCLIENTETS
                                                AND VXC.IDCONTRATOTS        = C.IDCONTRATOTS
                                                AND V.IDVENDATS             = VXC.IDVENDATS
                                                AND PJ.IDPROJETOTS          = VXC.IDPROJETOTS
                                                AND VXC.IDVENDAXCONTRATO    = LP.IDVENDAXCONTRATO
                                                AND RM.IDHOTEL              = PH.IDHOTEL
                                                AND USUARIORES.IDUSUARIO    = US.IDUSUARIO
                                                AND RM.IDRESERVAMIGRADA     = USUARIORES.IDRESERVAMIGRADA
                                                AND PR.IDRESERVAMIGRADA     = LP.IDRESERVAMIGRADA
                                                AND RM.IDRESERVAMIGRADA     = LP.IDRESERVAMIGRADA
                                                AND H.IDPESSOA              = RM.IDHOTEL
                                                AND TUH.IDTIPOUH            = RM.IDTIPOUH
                                                AND TUH.IDHOTEL             = RM.IDHOTEL
                                                AND CL.IDPESSOA             = A.IDCLIENTE
                                                AND CL.IDPESSOA = {pessoaVinculadaSistema.PessoaProvider}
                                                AND A.IDHOTEL               = HO.IDHOTEL
                                                AND ((RV.IDREVCONTRATOTS IS NULL AND VAL.IDREVCONTRATOTS IS NULL) OR (RV.IDREVCONTRATOTS = VAL.IDREVCONTRATOTS))
                                                AND HO.IDPESSOA             = {empresaCmId}
                                                AND LP.FLGMIGRADO           = 'S'    
                                                {txtRmDataCheckinReserva} {txtRmDataCheckoutReserva} ");


            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                sb.AppendLine($" AND TO_CHAR(VXC.NUMEROCONTRATO) LIKE '%{searchModel.NumeroContrato.TrimEnd()}%' ");
            }

            if (searchModel.NumReserva.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" AND RM.NUMRESERVA = {searchModel.NumReserva} ");
            }

            if (!string.IsNullOrEmpty(searchModel.StatusReserva))
            {
                sb.AppendLine($" AND LOWER(DECODE(SIGN(TO_NUMBER(RM.DATAPARTIDA-PH.DATASISTEMA)),1,DECODE(SIGN(TO_NUMBER(PH.DATASISTEMA-RM.DATACHEGADA)),1,'Check-In','Confirmada'),'Check-out')) LIKE '%{searchModel.StatusReserva.ToLower().TrimEnd()}%' ");
            }

            sb.AppendLine(")");

            var sql = sb.ToString();

            var totalRegistros = await _repository.CountTotalEntry(sql, parameters.ToArray());
            if (totalRegistros == 0)
                return (1, 1, new List<ReservaTsModel>());


            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros) < 30 ? Convert.ToInt32(totalRegistros) : 30;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 && searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > Convert.ToInt32(totalRegistros))
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros);

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            long totalPageValidation = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
            if (totalPageValidation < searchModel.NumeroDaPagina)
                searchModel.NumeroDaPagina = Convert.ToInt32(totalPageValidation);

            sb.AppendLine(@" ORDER BY
                                1 DESC");

            var result = (await _repository.FindBySql<ReservaTsModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPageValidation), result);
                }

            }

            return (1, 1, result);
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaGeralTsModel> reservas)?> GetMinhasReservasGeral(SearchMinhasReservasGeralModel searchModel)
        {
            var empresaCmId = _configuration.GetValue<int>("EmpresaCMId", 3);

            var parameters = new List<Parameter>();
            var txtPeriodo = " 1 = 1 ";
            var txtExibirTodosOsHospedes = !searchModel.ExibirTodosOsHospedes ? " AND M.IdHospede = (Select m1.IdHospede From MovimentoHospedes m1 Where m1.Principal = 'S' AND m1.IdHospede = m.IdHospede AND m1.IdReservasFront = m.IdReservasFront) " : "";

            if (!searchModel.CheckinInicial.HasValue && !searchModel.CheckoutInicial.HasValue)
            {
                if (searchModel.CheckinInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                {
                    searchModel.CheckinInicial = DateTime.Today.AddYears(-30);
                }

                if (searchModel.CheckinFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                {
                    searchModel.CheckinFinal = DateTime.Today.AddYears(10);
                }

                if (searchModel.CheckoutInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                {
                    searchModel.CheckoutInicial = DateTime.Today.AddYears(-30);
                }

                if (searchModel.CheckoutFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                {
                    searchModel.CheckoutFinal = DateTime.Today.AddYears(10);
                }
            }


            if (searchModel.CheckinInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("chegadaInicial", searchModel.CheckinInicial.GetValueOrDefault().Date));
                txtPeriodo += " AND COALESCE(M.DATACHEGREAL,M.DATACHEGPREVISTA) >= :chegadaInicial ";
            }

            if (searchModel.CheckinFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("chegadaFinal", searchModel.CheckinFinal.GetValueOrDefault().Date));
                txtPeriodo += " AND COALESCE(M.DATACHEGREAL,M.DATACHEGPREVISTA) <= :chegadaFinal ";
            }

            if (searchModel.CheckoutInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("partidaInicial", searchModel.CheckoutInicial.GetValueOrDefault().Date));
                if (!string.IsNullOrEmpty(txtPeriodo))
                    txtPeriodo += " AND COALESCE(M.DATAPARTREAL,M.DATAPARTPREVISTA) >= :partidaInicial ";
            }

            if (searchModel.CheckoutFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parameters.Add(new Parameter("partidaFinal", searchModel.CheckoutFinal.GetValueOrDefault().Date));
                if (!string.IsNullOrEmpty(txtPeriodo))
                    txtPeriodo += " AND COALESCE(M.DATAPARTREAL,M.DATAPARTPREVISTA) <= :partidaFinal ";
            }



            var loggedUser = await _repository.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            // Se IdCliente foi fornecido, verifica se o usuário é administrador
            bool admAsCLiente = false;
            if (searchModel.IdCliente.HasValue)
            {
                if (!loggedUser.Value.isAdm)
                {
                    var clienteVinculado = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
                    if (clienteVinculado == null || int.Parse(clienteVinculado.PessoaProvider!) != searchModel.IdCliente)
                        throw new UnauthorizedAccessException("Apenas administradores podem visualizar reservas de outros clientes");

                }
                else admAsCLiente = true;
            }

            var pessoaVinculadaSistema = admAsCLiente ? new PessoaSistemaXProviderModel() { PessoaProvider = searchModel.IdCliente.ToString()} : 
                await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
            if (pessoaVinculadaSistema == null)
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");


            if (string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) || !Helper.IsNumeric(pessoaVinculadaSistema.PessoaProvider))
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            var sb = new StringBuilder(@$"SELECT
	                                    CASE
		                                    WHEN TAR.FLGCONFIDENCIAL = 'S' THEN 0
		                                    ELSE CASE
			                                    WHEN R.STATUSRESERVA IN (0, 1, 7, 8, 5, 6) THEN (
			                                    SELECT
				                                    VALOR
			                                    FROM
				                                    ORCAMENTORESERVA O1
			                                    WHERE
				                                    O1.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND DATA = (
				                                    SELECT
					                                    MIN(DATA)
				                                    FROM
					                                    ORCAMENTORESERVA OREO1
				                                    WHERE
					                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
			                                    WHEN R.STATUSRESERVA IN (3, 4) THEN (
			                                    SELECT
				                                    VALOR
			                                    FROM
				                                    ORCAMENTORESERVA O1
			                                    WHERE
				                                    O1.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND DATA = (
				                                    SELECT
					                                    MAX(DATA)
				                                    FROM
					                                    ORCAMENTORESERVA OREO1
				                                    WHERE
					                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
			                                    WHEN R.STATUSRESERVA = 2
			                                    AND R.DATAPARTPREVISTA > PH.DATASISTEMA THEN (
			                                    SELECT
				                                    VALOR
			                                    FROM
				                                    ORCAMENTORESERVA O1
			                                    WHERE
				                                    O1.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND DATA = PH.DATASISTEMA)
			                                    ELSE (
			                                    SELECT
				                                    VALOR
			                                    FROM
				                                    ORCAMENTORESERVA O1
			                                    WHERE
				                                    O1.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND DATA = (
				                                    SELECT
					                                    MAX(DATA)
				                                    FROM
					                                    ORCAMENTORESERVA OREO1
				                                    WHERE
					                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    END
	                                    END AS VLRDIARIA,
	                                    TAR.FLGCONFIDENCIAL,
	                                    CASE
		                                    WHEN RG.OBSERVACOES IS NULL THEN R.OBSERVACOES
		                                    ELSE CASE
			                                    WHEN R.OBSERVACOES IS NULL THEN RG.OBSERVACOES
			                                    ELSE 'Reserva Grupo: ' || (NVL(RG.OBSERVACOES, ' '))|| ' - Reserva Individual: ' ||(NVL(R.OBSERVACOES, ' '))
		                                    END
	                                    END AS OBSERVACOES,
                                        PRO.PROJETOXCONTRATO AS NUMEROCONTRATO,
	                                    PRO.IDPESSOA, 
	                                    PRO.NOME AS NOMECLIENTE,
	                                    PRO.NUMDOCUMENTO AS NumDocumentoCliente,
	                                    PRO.EMAIL AS EMAILCLIENTE,
	                                    PRO.FLGCANCELADO AS ContratoCancelado,
                                        PRO.PROJETOXCONTRATO,
                                        PRO.PROJETOXCONTRATO AS PROJECTXCONTRACT,
                                        PRO.IdRCI,
	                                    U.BLOCO,
	                                    DECODE(NVL(AGENDA.QTDAGENDA, 0), 0, 'N', 'S') AS FLGAGENDA,
	                                    DECODE(NVL(MENSAGEM.QTDMENSAGEM, 0), 0, 'N', 'S') AS FLGMENSAGEM,
	                                    H.IDHOSPEDE,
	                                    TO_NUMBER(R.NUMRESERVAGDS) AS NUMRESERVAGDS,
	                                    TO_NUMBER(R.NUMRESERVA) AS NUMRESERVA,
	                                    R.IDRESERVASFRONT,
	                                    M.DATACHEGREAL AS DTCHEGHOSPEDE,
	                                    H.OBSERVACAO AS OBSHOSPEDE,
	                                    R.OBSSENSIVEIS,
	                                    M.DATAPARTREAL AS DTSAIDAHOSPEDE,
	                                    STAT.DESCRICAO AS StatusReserva,
	                                    NVL(R.OBSERVACOES, ' ') AS OBSRESERVA,
	                                    TO_DATE(TO_CHAR(DECODE(R.STATUSRESERVA, 0 ,                                                                                          
        	                                             (NVL(R.HORACHEGADAREAL, R.HORACHEGPREVISTA)), 1,                                                                            
        			                                     (NVL(R.HORACHEGADAREAL, R.HORACHEGPREVISTA)),                                                                                  
        			                                     (NVL(M.HORACHEGREAL, M.HORACHEGPREVISTA)) ), 'DD/MM/YYYY HH24:MI:SS'), 'DD/MM/YYYY HH24:MI:SS') AS HORACHEGADA ,
	                                    TO_DATE(TO_CHAR(DECODE(R.STATUSRESERVA, 0 ,                                                                                           
        	                                             (NVL(R.HORAPARTIDAREAL, R.HORAPARTPREVISTA)) , 1,                                                                           
        			                                     (NVL(R.HORAPARTIDAREAL, R.HORAPARTPREVISTA)) ,                                                                                 
        			                                     (NVL(M.HORAPARTREAL, M.HORAPARTPREVISTA)) ), 'DD/MM/YYYY HH24:MI:SS'), 'DD/MM/YYYY HH24:MI:SS')AS HORAPARTIDA,
	                                    NVL(M.DATAPARTREAL, M.DATAPARTPREVISTA) AS CHECKOUT,
	                                    R.GARANTENOSHOW,
	                                    NVL(M.DATACHEGREAL, M.DATACHEGPREVISTA) AS CHECKIN,
	                                    R.CODUH,
	                                    R.LOCRESERVA,
	                                    R.CODREFERENCIA,
	                                    T.CODREDUZIDO || ' / ' || TRUH.CODREDUZIDO AS CODREDUZIDO ,
	                                    RG.OBSERVACOES AS OBSGRP,
	                                    R.IDHOTEL,
	                                    HT.NOME AS HOTEL,
	                                    NVL(H.CODTRATAMENTO, '')|| ' ' || H.NOME || ' ' || H.SOBRENOME || DECODE(M.INCOGNITO, 'N', '', ' (INC.)') AS NOMEHOSPEDE,
	                                    H.NOME || ' ' || H.SOBRENOME AS NOMEHOSPEDEORD,
	                                    RG.NOMEGRUPO,
	                                    M.DATACHEGPREVISTA,
	                                    M.DATAPARTPREVISTA,
	                                    R.ADULTOS ,
	                                    R.CRIANCAS1,
	                                    R.CRIANCAS2,
	                                    R.DATADEPOSITO,
	                                    R.DATACONFIRMACAO,
	                                    R.DATARESERVA,
	                                    TH.HOSPEDEVIP,
	                                    TH.DESCRICAO AS TIPOHOSPEDE,
	                                    R.DATACANCELAMENTO,
                                        pont.QtdePontos as PontosDebitados,
	                                    CASE
		                                    WHEN R.STATUSRESERVA IN (0, 1, 7, 8, 5, 6) THEN (
		                                    SELECT
			                                    SEG.DESCRICAO
		                                    FROM
			                                    SEGMENTO SEG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND SEG.IDHOTEL = R.IDHOTEL
			                                    AND SEG.CODSEGMENTO = OREO.CODSEGMENTO
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MIN(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    WHEN R.STATUSRESERVA IN (3, 4) THEN (
		                                    SELECT
			                                    SEG.DESCRICAO
		                                    FROM
			                                    SEGMENTO SEG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND SEG.IDHOTEL = R.IDHOTEL
			                                    AND SEG.CODSEGMENTO = OREO.CODSEGMENTO
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MAX(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    WHEN R.STATUSRESERVA = 2
		                                    AND R.DATAPARTPREVISTA > PH.DATASISTEMA THEN (
		                                    SELECT
			                                    SEG.DESCRICAO
		                                    FROM
			                                    SEGMENTO SEG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND SEG.IDHOTEL = R.IDHOTEL
			                                    AND SEG.CODSEGMENTO = OREO.CODSEGMENTO
			                                    AND OREO.DATA = PH.DATASISTEMA)
		                                    ELSE (
		                                    SELECT
			                                    SEG.DESCRICAO
		                                    FROM
			                                    SEGMENTO SEG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND SEG.IDHOTEL = R.IDHOTEL
			                                    AND SEG.CODSEGMENTO = OREO.CODSEGMENTO
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MAX(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
	                                    END AS SEGMENTO,
	                                    CASE
		                                    WHEN R.STATUSRESERVA IN (0, 1, 7, 8, 5, 6) THEN (
		                                    SELECT
			                                    ORG.DESCRICAO
		                                    FROM
			                                    ORIGEMRESERVA ORG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND ORG.IDORIGEM = OREO.IDORIGEM
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MIN(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    WHEN R.STATUSRESERVA IN (3, 4) THEN (
		                                    SELECT
			                                    ORG.DESCRICAO
		                                    FROM
			                                    ORIGEMRESERVA ORG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND ORG.IDORIGEM = OREO.IDORIGEM
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MAX(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
		                                    WHEN R.STATUSRESERVA = 2
		                                    AND R.DATAPARTPREVISTA > PH.DATASISTEMA THEN (
		                                    SELECT
			                                    ORG.DESCRICAO
		                                    FROM
			                                    ORIGEMRESERVA ORG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND ORG.IDORIGEM = OREO.IDORIGEM
			                                    AND OREO.DATA = PH.DATASISTEMA)
		                                    ELSE (
		                                    SELECT
			                                    ORG.DESCRICAO
		                                    FROM
			                                    ORIGEMRESERVA ORG,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND ORG.IDORIGEM = OREO.IDORIGEM
			                                    AND OREO.DATA = (
			                                    SELECT
				                                    MAX(DATA)
			                                    FROM
				                                    ORCAMENTORESERVA OREO1
			                                    WHERE
				                                    OREO1.IDRESERVASFRONT = R.IDRESERVASFRONT))
	                                    END AS ORIGEM,
	                                    TO_CHAR(R.ADULTOS)|| '/' || TO_CHAR(R.CRIANCAS1)|| '/' || TO_CHAR(R.CRIANCAS2) AS NHOSPEDES,
	                                    CLI.RAZAOSOCIAL AS RAZAOSOCIALCLIENTEHOTEL,
	                                    CLI.NOME AS CLIENTEHOTEL,
	                                    PO.NOME AS POSTO,
	                                    CONT.CODCONTRATO,
	                                    M.INCOGNITO,
	                                    M.SENHATELEFONIA,
	                                    TRUH.DESCRICAO AS TIPOUH,
	                                    R.OBSCMNET,
	                                    CASE
		                                    WHEN (
		                                    SELECT
			                                    PCH.DESCRICAO
		                                    FROM
			                                    PACOTEHOTEL PCH,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND PCH.IDPACOTE = OREO.IDPACOTE
			                                    AND PCH.IDHOTEL = OREO.IDHOTEL
			                                    AND PH.IDHOTEL = OREO.IDHOTEL
			                                    AND OREO.DATA = PH.DATASISTEMA) IS NOT NULL                                                                 
                                                 THEN (
		                                    SELECT
			                                    PCH.DESCRICAO
		                                    FROM
			                                    PACOTEHOTEL PCH,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND PCH.IDPACOTE = OREO.IDPACOTE
			                                    AND PCH.IDHOTEL = OREO.IDHOTEL
			                                    AND PH.IDHOTEL = OREO.IDHOTEL
			                                    AND OREO.DATA = PH.DATASISTEMA)
		                                    ELSE CASE
			                                    WHEN (R.DATACHEGPREVISTA > PH.DATASISTEMA
				                                    AND                                                                        
                                                            (
				                                    SELECT
					                                    PCH.DESCRICAO
				                                    FROM
					                                    PACOTEHOTEL PCH,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND PCH.IDPACOTE = OREO.IDPACOTE
					                                    AND PCH.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = R.DATACHEGPREVISTA) IS NOT NULL )                                                                 
                                                 THEN                                                                                                                            
                                                            (
			                                    SELECT
				                                    PCH.DESCRICAO
			                                    FROM
				                                    PACOTEHOTEL PCH,
				                                    ORCAMENTORESERVA OREO
			                                    WHERE
				                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND PCH.IDPACOTE = OREO.IDPACOTE
				                                    AND PCH.IDHOTEL = OREO.IDHOTEL
				                                    AND PH.IDHOTEL = OREO.IDHOTEL
				                                    AND OREO.DATA = R.DATACHEGPREVISTA)
			                                    ELSE CASE
				                                    WHEN R.STATUSRESERVA = 3
				                                    AND                                                                                   
                                                                 (
				                                    SELECT
					                                    PCH.DESCRICAO
				                                    FROM
					                                    PACOTEHOTEL PCH,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND PCH.IDPACOTE = OREO.IDPACOTE
					                                    AND PCH.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = (
					                                    SELECT
						                                    MAX(DATA)
					                                    FROM
						                                    ORCAMENTORESERVA OREO1
					                                    WHERE
						                                    OREO1.IDRESERVASFRONT = OREO.IDRESERVASFRONT )) IS NOT NULL                  
                                                                THEN (
				                                    SELECT
					                                    PCH.DESCRICAO
				                                    FROM
					                                    PACOTEHOTEL PCH,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND PCH.IDPACOTE = OREO.IDPACOTE
					                                    AND PCH.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = (
					                                    SELECT
						                                    MAX(DATA)
					                                    FROM
						                                    ORCAMENTORESERVA OREO1
					                                    WHERE
						                                    OREO1.IDRESERVASFRONT = OREO.IDRESERVASFRONT ))
				                                    ELSE PAC.DESCRICAO
			                                    END
		                                    END
	                                    END AS PACOTE,
	                                    CASE
		                                    WHEN (
		                                    SELECT
			                                    THO.DESCRICAO
		                                    FROM
			                                    TARIFAHOTEL THO,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND THO.IDTARIFA = OREO.IDTARIFA
			                                    AND THO.IDHOTEL = OREO.IDHOTEL
			                                    AND PH.IDHOTEL = OREO.IDHOTEL
			                                    AND OREO.DATA = PH.DATASISTEMA) IS NOT NULL                                                                            
                                                 THEN (
		                                    SELECT
			                                    THO.DESCRICAO
		                                    FROM
			                                    TARIFAHOTEL THO,
			                                    ORCAMENTORESERVA OREO
		                                    WHERE
			                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
			                                    AND THO.IDTARIFA = OREO.IDTARIFA
			                                    AND THO.IDHOTEL = OREO.IDHOTEL
			                                    AND PH.IDHOTEL = OREO.IDHOTEL
			                                    AND OREO.DATA = PH.DATASISTEMA)
		                                    ELSE CASE
			                                    WHEN (R.DATACHEGPREVISTA > PH.DATASISTEMA
				                                    AND                                                                              
                                                            (
				                                    SELECT
					                                    THO.DESCRICAO
				                                    FROM
					                                    TARIFAHOTEL THO,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND THO.IDTARIFA = OREO.IDTARIFA
					                                    AND THO.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = R.DATACHEGPREVISTA) IS NOT NULL )                                                                 
                                                 THEN                                                                                                                            
                                                            (
			                                    SELECT
				                                    THO.DESCRICAO
			                                    FROM
				                                    TARIFAHOTEL THO,
				                                    ORCAMENTORESERVA OREO
			                                    WHERE
				                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
				                                    AND THO.IDTARIFA = OREO.IDTARIFA
				                                    AND THO.IDHOTEL = OREO.IDHOTEL
				                                    AND PH.IDHOTEL = OREO.IDHOTEL
				                                    AND OREO.DATA = R.DATACHEGPREVISTA)
			                                    ELSE CASE
				                                    WHEN R.STATUSRESERVA = 3
				                                    AND                                                                                   
                                                                 (
				                                    SELECT
					                                    THO.DESCRICAO
				                                    FROM
					                                    TARIFAHOTEL THO,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND THO.IDTARIFA = OREO.IDTARIFA
					                                    AND THO.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = (
					                                    SELECT
						                                    MAX(DATA)
					                                    FROM
						                                    ORCAMENTORESERVA OREO1
					                                    WHERE
						                                    OREO1.IDRESERVASFRONT = OREO.IDRESERVASFRONT )) IS NOT NULL                  
                                                                THEN (
				                                    SELECT
					                                    THO.DESCRICAO
				                                    FROM
					                                    TARIFAHOTEL THO,
					                                    ORCAMENTORESERVA OREO
				                                    WHERE
					                                    OREO.IDRESERVASFRONT = R.IDRESERVASFRONT
					                                    AND THO.IDTARIFA = OREO.IDTARIFA
					                                    AND THO.IDHOTEL = OREO.IDHOTEL
					                                    AND PH.IDHOTEL = OREO.IDHOTEL
					                                    AND OREO.DATA = (
					                                    SELECT
						                                    MAX(DATA)
					                                    FROM
						                                    ORCAMENTORESERVA OREO1
					                                    WHERE
						                                    OREO1.IDRESERVASFRONT = OREO.IDRESERVASFRONT ))
				                                    ELSE TAR.DESCRICAO
			                                    END
		                                    END
	                                    END AS TARIFA,
	                                    (
	                                    SELECT
		                                    TO_NUMBER(COUNT(*))
	                                    FROM
		                                    CONTASFRONT
	                                    WHERE
		                                    (IDRESERVASFRONT = R.IDRESERVASFRONT)
			                                    AND (IDHOSPEDE = M.IDHOSPEDE)
				                                    AND DATAENCREAL IS NULL) AS NAOENCERRADAS 
                                     ,
	                                    H.NOMESOCIAL,
	                                    LPAD(' ', 20, ' ') AS STATUSDARESERVA                                                                                      
                                     ,
	                                    FX.TIPOETARIO,
	                                    H.NOME || ' ' || H.SOBRENOME AS NOMEHOSPEDECOMPLETO,
                                     Decode(RCI.IDRESERVASRCI,NULL,'Não','Sim') as RCI,
                                     Case 
                                        when afrac.IdFracionamentoTs is not null then 'Inicio Fracionamento'
                                        when ffrac.IdFracionamentoTs is not null then 'Encerramento Fracionamento'
                                        else null end as TipoReserva
                                    FROM
	                                    STATUSRESERVA STAT,
	                                    UH U,
	                                    TARIFAHOTEL TAR,
	                                    PACOTEHOTEL PAC,
	                                    TIPOUH T,
	                                    TIPOUH TRUH,
	                                    TIPOHOSPEDE TH,
	                                    PARAMHOTEL PH,
	                                    CONTRCLIHOTEL CONT,
	                                    RESERVAGRUPO RG,
	                                    ROOMLISTVHF RL,
	                                    PESSOA PHOSP,
	                                    PESSOA CLI,
	                                    PESSOA HT,
	                                    PESSOA PO,
	                                    HOSPEDE H,
	                                    RESERVASFRONT R,
	                                    MOVIMENTOHOSPEDES M,
	                                    FAIXAETARIA FX,
	                                    (
	                                    SELECT
		                                    DISTINCT IDRESERVASFRONT
	                                    FROM
		                                    RESERVASTS) RESERVASTS,
	                                    (
	                                    SELECT
		                                    IDRESERVASFRONT,
		                                    SUM(1) AS QTDAGENDA
	                                    FROM
		                                    MENSAGEMCM
	                                    WHERE
		                                    LIDA = 0
		                                    AND NOMEREMETENTE IS NULL
		                                    AND IDRESERVASFRONT IS NOT NULL
	                                    GROUP BY
		                                    IDRESERVASFRONT) AGENDA,
	                                    (
	                                    SELECT
		                                    IDRESERVASFRONT,
		                                    IDDESTINATARIO,
		                                    SUM(1) AS QTDMENSAGEM
	                                    FROM
		                                    MENSAGEMCM
	                                    WHERE
		                                    IDRESERVASFRONT IS NOT NULL
		                                    AND IDDESTINATARIO IS NOT NULL
		                                    AND LIDA = 0
		                                    AND NOMEREMETENTE IS NOT NULL
	                                    GROUP BY
		                                    IDRESERVASFRONT,
		                                    IDDESTINATARIO) MENSAGEM,
	                                    (SELECT
		                                    DISTINCT
	                                        R.IDRESERVASFRONT,
	                                        VC.NUMEROCONTRATO,
                                            cast( TO_CHAR( COALESCE( PJ.NUMEROPROJETO,'-1' ) ) || '-' || TO_CHAR( TO_NUMBER(VC.NUMEROCONTRATO) ) as varchar (50)) AS PROJETOXCONTRATO,
		                                    PCL.IDPESSOA, 
		                                    PCL.NOME, 
		                                    PCL.NUMDOCUMENTO,
		                                    PCL.EMAIL,
		                                    VC.FLGCANCELADO,
                                            rc.IdRCI
	                                      FROM 
	                                          LANCPONTOSTS LP, 
	                                          VENDAXCONTRATOTS VC,
                                              ProjetoTs PJ,
	                                          RESERVASFRONT R,
	                                          VENDATS V,
	                                          ATENDCLIENTETS A,
	                                          PESSOA PCL,
                                              (
                                                SELECT 
                                                 ap.IdPessoa,
                                                 ap.VALORCHAR AS IdRCI
                                                FROM 
                                                 Pessoaxatributo ap 
                                                WHERE ap.idatributopessoa = 10 AND 
                                                 ap.VALORCHAR IS NOT null AND 
                                                 LENGTH(ap.VALORCHAR) > 1) rc
	                                      WHERE
	                                       LP.IDRESERVASFRONT = R.IDRESERVASFRONT
	                                       AND LP.IDVENDAXCONTRATO  = VC.IDVENDAXCONTRATO
	                                       AND V.IDVENDATS = VC.IDVENDATS
	                                       AND V.IDATENDCLIENTETS = A.IDATENDCLIENTETS
                                           AND VC.IDPROJETOTS = PJ.IdProjetoTs
	                                       AND A.IDCLIENTE = PCL.IDPESSOA
                                           AND A.IDCLIENTE = rc.IdPessoa(+)
	                                     ) PRO,
                                         (Select 
                                            rc.IdReservasFront, 
                                            rc.IdReservasRci 
                                         From 
                                            ReservasRci rc 
                                        ) RCI,
                                        (Select 
                                            fts.IdReservasFront1, 
                                            fts.IdFracionamentoTs 
                                         From 
                                            FracionamentoTs fts
                                        ) afrac,
                                        (Select 
                                            fts.IdReservasFront2, 
                                            fts.IdFracionamentoTs 
                                         From 
                                            FracionamentoTs fts
                                        ) ffrac,
                                        (SELECT 
                                            lpts.IdReservasFront, 
                                            rf.clientereservante,
                                            rf.clientehospede,
                                            Sum(Nvl(lpts.NumeroPontos,0)+Nvl(lpts.VLRPENSAO,0)) AS QtdePontos
                                            FROM 
                                            LancpontosTs lpts
                                            INNER JOIN ReservasFront rf ON lpts.IDRESERVASFRONT = rf.IDRESERVASFRONT
                                            INNER JOIN Pessoa p ON rf.CLIENTERESERVANTE = p.IdPessoa
                                            WHERE 
                                            rf.StatusReserva <> 6
                                            AND lpts.IDTIPOLANCPONTOTS = 1
                                            AND lpts.IDLANCPONTOSTS = (SELECT min(lpts1.IDLANCPONTOSTS) FROM LancPontosTs lpts1 WHERE lpts1.IdReservasFront = rf.IDRESERVASFRONT)
                                            GROUP BY lpts.IdReservasFront, rf.CLIENTERESERVANTE, rf.CLIENTEHOSPEDE 
                                        ) pont
                                    WHERE
	                                ( 
                                       {txtPeriodo}     
                                    )
	                                AND (1900693 NOT IN (
	                                SELECT
		                                GU.IDUSUARIO
	                                FROM
		                                GRUPOUSU GU,
		                                GRPUSUACESSORES GR
	                                WHERE
		                                GR.IDHOTEL = R.IDHOTEL
		                                AND GU.IDGRUPO = GR.IDGRUPO)
	                                OR
                                               (1900693 IN (
	                                SELECT
		                                GU.IDUSUARIO
	                                FROM
		                                GRUPOUSU GU,
		                                GRUPOUSU GURES
	                                WHERE
		                                GURES.IDUSUARIO = R.USUARIO
		                                AND GU.IDGRUPO = GURES.IDGRUPO)))
	                                AND TH.IDTIPOHOSPEDE = M.IDTIPOHOSPEDE
	                                AND PHOSP.IDPESSOA = H.IDHOSPEDE
	                                AND R.IDHOTEL = HT.IDPESSOA
	                                AND R.IDHOTEL = T.IDHOTEL
	                                AND R.IDHOTEL = TRUH.IDHOTEL
	                                AND R.IDHOTEL = TH.IDHOTEL
	                                AND R.IDHOTEL = PH.IDHOTEL
	                                AND r.idtarifa = tar.idtarifa (+)
	                                AND R.TIPOUHESTADIA = T.IDTIPOUH
	                                AND R.TIPOUHTARIFA = TRUH.IDTIPOUH
	                                AND M.IDRESERVASFRONT = R.IDRESERVASFRONT
	                                AND M.IDHOSPEDE = H.IDHOSPEDE
                                    AND M.PRINCIPAL = 'S'
	                                AND R.IDHOTEL = TAR.IDHOTEL (+)
	                                AND R.STATUSRESERVA = STAT.STATUSRESERVA
	                                AND R.IDRESERVASFRONT = RESERVASTS.IDRESERVASFRONT
	                                AND R.IDHOTEL = U.IDHOTEL (+)
	                                AND R.CODUH = U.CODUH (+)
	                                AND R.CLIENTERESERVANTE = CLI.IDPESSOA (+)
	                                AND R.CLIENTEHOSPEDE = PO.IDPESSOA (+)
	                                AND R.IDPACOTE = PAC.IDPACOTE (+)
	                                AND R.IDHOTEL = PAC.IDHOTEL (+)
	                                AND R.IDROOMLIST = RL.IDROOMLIST (+)
	                                AND RL.IDRESERVAGRUPO = RG.IDRESERVAGRUPO (+)
	                                AND R.IDHOTEL = CONT.IDHOTEL (+)
	                                AND R.CLIENTERESERVANTE = CONT.IDFORCLI (+)
	                                AND R.CONTRATOINICIAL = CONT.CODCONTRATO (+)
	                                AND R.IDRESERVASFRONT = AGENDA.IDRESERVASFRONT (+)
	                                AND M.IDRESERVASFRONT = MENSAGEM.IDRESERVASFRONT (+)
	                                AND M.IDHOSPEDE = MENSAGEM.IDDESTINATARIO (+)
	                                AND H.IDFAIXAETARIA = FX.IDFAIXAETARIA (+)
	                                AND PRO.IDRESERVASFRONT = R.IDRESERVASFRONT 
                                    AND STAT.StatusReserva <> 6
                                    AND R.IDRESERVASFRONT = RCI.IDRESERVASFRONT(+)
                                    AND R.IDRESERVASFRONT = pont.IdReservasFront(+)
                                    AND R.IDRESERVASFRONT = afrac.IdReservasFront1(+)
                                    AND R.IDRESERVASFRONT = ffrac.IdReservasFront2(+)");
            
            // Se IdCliente foi fornecido, usa ele; senão usa a pessoa vinculada ao usuário logado
            if (searchModel.IdCliente.HasValue)
            {
                sb.AppendLine($" AND PRO.IDPESSOA = {searchModel.IdCliente.Value} {txtExibirTodosOsHospedes}");
            }
            else
            {
                sb.AppendLine($" AND PRO.IDPESSOA = {pessoaVinculadaSistema.PessoaProvider} {txtExibirTodosOsHospedes}");
            }

            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                var numContrato = searchModel.NumeroContrato.Split("-").Length == 2 ? searchModel.NumeroContrato.Split("-")[1] : searchModel.NumeroContrato;
                sb.AppendLine($" AND TO_CHAR(PRO.NUMEROCONTRATO) LIKE '%{numContrato}%' ");
            }

            if (searchModel.NumReserva.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" AND R.NUMRESERVA = {searchModel.NumReserva} ");
            }

            if (!string.IsNullOrEmpty(searchModel.StatusReserva))
            {
                sb.AppendLine($" AND LOWER(STAT.DESCRICAO) LIKE '%{searchModel.StatusReserva.ToLower().TrimEnd()}%' ");
            }

            var sql = sb.ToString();


            var totalRegistros = await _repository.CountTotalEntry(sql, parameters.ToArray());

            // Usa IdCliente se fornecido, senão usa a pessoa vinculada ao usuário logado
            var clienteIdParaReservaTimeSharing = searchModel.IdCliente.HasValue
                ? searchModel.IdCliente.Value
                : Convert.ToInt32(pessoaVinculadaSistema.PessoaProvider);

            var reservaTimeSharing = parameters.Any(b => b.Name == "chegadaInicial") && parameters.Any(b => b.Name == "chegadaFinal") ? (await _repositorySystem.FindBySql<ReservaTimeSharing>(@$"Select 
                                                                                                * 
                                                                                               From 
                                                                                                ReservaTimeSharing 
                                                                                               Where 
                                                                                                ClienteReservante = {clienteIdParaReservaTimeSharing} and 
                                                                                                Upper(StatusCM) = 'PENDENTE' and 
                                                                                                IdReservasFront is null and 
                                                                                                TipoUtilizacao like 'RCI%INTER%' AND 
                                                                                                (Checkin between :chegadaInicial and :chegadaFinal) ",
                                                                                                 parameters.Where(a => a.Name == "chegadaInicial" || a.Name == "chegadaFinal").ToArray())).AsList() : new();


            if (totalRegistros == 0 && (reservaTimeSharing == null || !reservaTimeSharing.Any()))
                return (1, 1, new List<ReservaGeralTsModel>());

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros) < 30 ? Convert.ToInt32(totalRegistros) : 30;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 && searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > Convert.ToInt32(totalRegistros))
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros);

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            long totalPageValidation = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
            if (totalPageValidation < searchModel.NumeroDaPagina)
                searchModel.NumeroDaPagina = Convert.ToInt32(totalPageValidation);

            sb.AppendLine(@" ORDER BY
                                R.NUMRESERVA DESC ");

            var result = (await _repository.FindBySql<ReservaGeralTsModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parameters.ToArray())).AsList();

            
            foreach (var item in reservaTimeSharing)
            {
                result.Add(new ReservaGeralTsModel()
                {
                    NumeroContrato = item.NumeroContrato,
                    ProjetoXContrato = item.NumeroContrato,
                    NomeCliente = item.NomeCliente,
                    DataReserva = item.DataHoraCriacao,
                    Checkin = item.DataHoraCriacao,
                    Checkout = item.DataHoraCriacao,
                    PontosDebitados = item.PontosUtilizados,
                    StatusReserva = "Confirmada",
                    Origem = "RCI",
                    Rci = "Sim"
                });
            }

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPageValidation), result);
                }

            }

            return (1, 1, result);
        }

        public async Task<ReservaTimeSharingCMModel> Visualizar(string reservanumero)
        {
            throw new NotImplementedException();
        }

        public async Task<ReservaTimeSharingCMModel> VisualizarMinha(string reservanumero)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<PeriodoDisponivelResultModel>?> Disponibilidade(SearchDisponibilidadeModel searchModel)
        {
            if (string.IsNullOrEmpty(searchModel.NumeroContrato) || searchModel.IdVendaXContrato.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informado o número do contrato e o Id da venda x contrato para busca de disponibilidades.");

            if (searchModel.DataInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                throw new ArgumentException("Deve ser informada a data inicial e data final para busca de disponibiliades.");

            if (searchModel.DataFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                throw new ArgumentException("Deve ser informada a data inicial e data final para busca de disponibiliades.");

            if (searchModel.DataFinal.GetValueOrDefault().Date < searchModel.DataInicial.GetValueOrDefault().Date)
                throw new ArgumentException("Deve ser informada a data final maior ou igual a data inicial para busca de disponibiliades.");

            PeriodoDisponivelResultModel? baseSaldoPontos = await GetSaldo(searchModel);
            if (baseSaldoPontos == null || baseSaldoPontos.IdContratoTs.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Falha na busca de disponibilidade 'Contrato não encontrado'");

            var condicaoFinanceira = await PosicaoFinanceiraContrato(baseSaldoPontos.IdVendaTs.GetValueOrDefault(), baseSaldoPontos.SaldoPontos);

            var listRetorno = await GetDisponibilidade(searchModel, baseSaldoPontos, condicaoFinanceira,70);

            return listRetorno;
        }

        private async Task<DadosFinanceirosContrato?> PosicaoFinanceiraContrato(int idVendaTs, decimal? saldoPontos)
        {
            var resultCache = await _cacheStore.GetAsync<DadosFinanceirosContrato?>($"DadosFinanceirosContrato_{idVendaTs}", 1, _repositorySystem.CancellationToken);
            if (resultCache != null)
            {
                resultCache.SaldoPontos = saldoPontos;
                return resultCache;
            }

            var result = (await _repository.FindBySql<DadosFinanceirosContrato>(@$"Select b.* From (SELECT Nvl(RC.DATAREVERSAO,V.DataVenda) AS DataVenda,
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
                                        A.IDCLIENTE
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
                                      AND H.IDPESSOA = 3 AND VC.IDVENDATS = :idVendaTs ) b Where Lower(b.Status) = 'ativo' ",
                     new Parameter("idVendaTs", idVendaTs))).FirstOrDefault();

            //if (result != null && result?.SaldoInadimplente > 0.00m)
            //    throw new ArgumentException("Favor procure a Central de Atendimento ao Cliente '0001-PF'");

            if (result != null)
            {
                result.SaldoPontos = saldoPontos.GetValueOrDefault();

                result.BloqueioTsModel = (await _repository.FindBySql<BloqueioTsModel>($@"SELECT 
                B.FLGLIBERADO, 
                B.OBSERVACAO, 
                M.DESCRICAO, 
                B.DATABLOQUEIO, 
                U.NOMEUSUARIO AS USUARIOBLOQUEIO,        
                PJ.NUMEROPROJETO ||'-'||TO_CHAR(TO_NUMBER(VC.NUMEROCONTRATO)) AS NUMEROCONTRATO             
                FROM 
                   BLOQCLIENTETS B                                                                               
                   JOIN USUARIOSISTEMA U ON B.IDUSUARIO = U.IDUSUARIO                                                 
                   JOIN MOTIVOTS M ON B.IDMOTIVOTS = M.IDMOTIVOTS                                                     
                   LEFT JOIN VENDAXCONTRATOTS VC ON B.IDVENDAXCONTRATO = VC.IDVENDAXCONTRATO                          
                   LEFT JOIN PROJETOTS PJ ON VC.IDPROJETOTS = PJ.IDPROJETOTS                                          
                   LEFT JOIN AGENCIATS AG ON PJ.IDAGENCIATS = AG.IDAGENCIATS                                         
                WHERE B.FLGLIBERADO = 'N' AND 
                   B.IDCLIENTE = {result.IdCliente}
                   AND (VC.IDVENDAXCONTRATO IS NULL OR AG.IDPESSOA = 3)           
                   ORDER BY B.DATABLOQUEIO DESC")).FirstOrDefault();
            }

            if (result != null)
            {
                var debitoNaoUtilizacao = await GetDebitoPorNaoUtilzacaoPrevisto(result);
                result.DataPrevistaLancamentoNU = debitoNaoUtilizacao?.FlgGeraCredNUtil == "S" ? debitoNaoUtilizacao?.ValidadeCredito : null;
                result.ValorPrevistoDebitoNU = debitoNaoUtilizacao?.CreditoPontos;
                await _cacheStore.AddAsync($"DadosFinanceirosContrato_{idVendaTs}", result, DateTimeOffset.Now.AddMinutes(10), 1, _repositorySystem.CancellationToken);
            }

            return result;
        }

        private async Task<DebitoPorNaoUtlizacaoModel?> GetDebitoPorNaoUtilzacaoPrevisto(DadosFinanceirosContrato dadosFinanceiros)
        {
            var resultCache = await _cacheStore.GetAsync<DebitoPorNaoUtlizacaoModel?>($"DebitoPorNaoUtilizacao_{dadosFinanceiros.IdVendaXContrato}",1,_repositorySystem.CancellationToken);
            if (resultCache != null)
                return resultCache;

            var primeiroAniversario = dadosFinanceiros.DataVenda.GetValueOrDefault().AddYears(1);
            var dataPrimeiroDebitoPontos = primeiroAniversario.AddYears(1);
            DateTime? dataDebitoPontosAtual = null;
            if (dataPrimeiroDebitoPontos <= DateTime.Today.Date)
            {
                dataDebitoPontosAtual = dataPrimeiroDebitoPontos;
                while (dataDebitoPontosAtual <= DateTime.Today.Date)
                {
                    dataDebitoPontosAtual = dataDebitoPontosAtual.Value.AddYears(1);
                }
            }

            if (dataDebitoPontosAtual == null)
                return null;

            DateTime dataInicioApuracaoPeriodoDebito = dataDebitoPontosAtual.Value.AddYears(-1);


            var result = (await _repository.FindBySql<DebitoPorNaoUtlizacaoModel>(@$"SELECT NVL(R.DATAREVERSAO,V.DATAVENDA) AS DATAVENDA,
        
                                       :dataDebitoPrevisto as VALIDADECREDITO,      

                                       RFV.PONTOSRF, RMV.PONTOSRM, PONTOSV.PONTOSOUTROS,
                                       (NVL(RFV.PONTOSRF,0) + NVL(RMV.PONTOSRM,0) + NVL(PONTOSV.PONTOSOUTROS,0)) AS PONTOSUTILIZADOS,
               
                                       CASE WHEN (RF.IDVENDAXCONTRATO IS NULL) AND (RM.IDVENDAXCONTRATO IS NULL) AND (PONTOS.IDVENDAXCONTRATO IS NULL) THEN
                                         CASE WHEN (C.NUMEROPONTOS - NVL(U.UTILIZACAO,0)) > (C.DESCONTOANUAL - (NVL(RFV.PONTOSRF,0) + NVL(RMV.PONTOSRM,0) + NVL(PONTOSV.PONTOSOUTROS,0))) THEN
                                           C.DESCONTOANUAL - (NVL(RFV.PONTOSRF,0) + NVL(RMV.PONTOSRM,0) + NVL(PONTOSV.PONTOSOUTROS,0))
                                         ELSE
                                           (C.NUMEROPONTOS - NVL(U.UTILIZACAO,0))
                                         END
                                       ELSE
                                         CASE WHEN (C.UTILIZACAOMINIMAPONTOS > 0) AND ((C.NUMEROPONTOS - NVL(U.UTILIZACAO,0)) > (C.UTILIZACAOMINIMAPONTOS - (NVL(RF.PONTOSRF,0) + NVL(RM.PONTOSRM,0) + NVL(PONTOS.PONTOSOUTROS,0) + NVL(RFV.PONTOSRF,0) + NVL(RMV.PONTOSRM,0) + NVL(PONTOSV.PONTOSOUTROS,0)))) THEN
                                           C.UTILIZACAOMINIMAPONTOS - (NVL(RF.PONTOSRF,0) + NVL(RM.PONTOSRM,0) + NVL(PONTOS.PONTOSOUTROS,0) + NVL(RFV.PONTOSRF,0) + NVL(RMV.PONTOSRM,0) + NVL(PONTOSV.PONTOSOUTROS,0) )
                                         ELSE
                                           (C.NUMEROPONTOS - NVL(U.UTILIZACAO,0))
                                         END
                                       END AS CREDITOPONTOS,
              
                                       C.DESCONTOANUAL, C.TAXAANUAL,
                                       C.IDCONTRATOTS, VC.NUMEROCONTRATO,
                                       P.NOME AS NOMECLIENTE, C.NOME AS NOMECONTRATO,
                                       VC.IDVENDAXCONTRATO, C.IDTIPODCTAXA, C.FLGGERACREDNUTIL, C.ANOINICIAL
                                FROM   VENDAXCONTRATOTS VC, VENDATS V, REVCONTRATOTS R, CONTRATOTS C,
                                       ATENDCLIENTETS A, PESSOA P, PARAMTS PAR,
      
                                       (SELECT LP.IDVENDAXCONTRATO, SUM(LP.NUMEROPONTOS) AS PONTOSRF
                                         FROM   LANCPONTOSTS LP, RESERVASFRONT RF, 
                                                (SELECT IDRESERVASFRONT FROM RESERVASRCI GROUP BY IDRESERVASFRONT) RCI, 
                                                PARAMTS PAR, VENDAXCONTRATOTS VC, VENDATS V, REVCONTRATOTS R
                                         WHERE  LP.IDRESERVASFRONT = RF.IDRESERVASFRONT
                                           AND    RCI.IDRESERVASFRONT (+) = RF.IDRESERVASFRONT           
                                           AND    LP.IDVENDAXCONTRATO     = VC.IDVENDAXCONTRATO
                                           AND    VC.IDVENDATS            = V.IDVENDATS           
                                           AND    VC.IDVENDAXCONTRATO     = R.IDVENDAXCONTRNOVO (+)
                                           AND    LP.NUMEROPONTOS > 0   
                                           AND    (                        
                                                 (RCI.IDRESERVASFRONT IS NULL 
                                            AND   (RF.STATUSRESERVA IN (0,1,2,3,4,5) OR (RF.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS IN (4,5)))
                                            AND   :dataBaseInicialAnoAnterior <= RF.DATACHEGPREVISTA
                                            AND   :dataInicioApuracaoDebito >  RF.DATACHEGPREVISTA
                                            AND   ((:dataInicioApuracaoDebito <= LP.VALIDADECREDITO AND :dataProximoDebitoPontos >=  LP.VALIDADECREDITO) OR LP.VALIDADECREDITO IS NULL)  )
                                               OR
                                                 (RCI.IDRESERVASFRONT IS NOT NULL
                                            AND   (RF.STATUSRESERVA IN (0,1,2,3,4,5) OR (RF.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS IN (4,5)))
                                            AND   :dataBaseInicialAnoAnterior <= LP.DATALANCAMENTO
                                            AND   :dataInicioApuracaoDebito >  LP.DATALANCAMENTO
                                            AND   ((:dataInicioApuracaoDebito <= LP.VALIDADECREDITO AND :dataProximoDebitoPontos >=  LP.VALIDADECREDITO) OR LP.VALIDADECREDITO IS NULL)  ))
                                         GROUP BY LP.IDVENDAXCONTRATO) RF,

                                       (SELECT LP.IDVENDAXCONTRATO, SUM(LP.NUMEROPONTOS) AS PONTOSRM
                                         FROM   LANCPONTOSTS LP, RESERVAMIGRADATS RM, 
                                                (SELECT IDRESERVAMIGRADA FROM RESERVASRCI GROUP BY IDRESERVAMIGRADA) RCI, 
                                                PARAMTS PAR, VENDAXCONTRATOTS VC, VENDATS V, REVCONTRATOTS R
                                         WHERE  LP.IDRESERVAMIGRADA = RM.IDRESERVAMIGRADA
                                           AND    RCI.IDRESERVAMIGRADA (+) = RM.IDRESERVAMIGRADA
                                           AND    LP.IDVENDAXCONTRATO      = VC.IDVENDAXCONTRATO
                                           AND    VC.IDVENDATS             = V.IDVENDATS
                                           AND    VC.IDVENDAXCONTRATO      = R.IDVENDAXCONTRNOVO (+)
                                           AND    LP.NUMEROPONTOS > 0
                                           AND    (
                                                 (RCI.IDRESERVAMIGRADA IS NULL
                                           AND   :dataBaseInicialAnoAnterior <= RM.DATACHEGADA
                                           AND   :dataInicioApuracaoDebito >  RM.DATACHEGADA
                                           AND    LP.VALIDADECREDITO IS NULL)
                                              OR
                                                 (RCI.IDRESERVAMIGRADA IS NOT NULL
                                           AND   :dataBaseInicialAnoAnterior <= LP.DATALANCAMENTO
                                           AND   :dataInicioApuracaoDebito >  LP.DATALANCAMENTO
                                           AND    LP.VALIDADECREDITO IS NULL))
                                         GROUP BY LP.IDVENDAXCONTRATO) RM,

                                       (SELECT LP.IDVENDAXCONTRATO, SUM(LP.NUMEROPONTOS)  AS PONTOSOUTROS
                                         FROM   LANCPONTOSTS LP, MOTIVOTS M, PARAMTS PAR, VENDAXCONTRATOTS VC, VENDATS V, REVCONTRATOTS R
                                         WHERE  LP.IDRESERVAMIGRADA IS NULL
                                           AND  LP.IDVENDAXCONTRATO = VC.IDVENDAXCONTRATO
                                           AND  VC.IDVENDATS        = V.IDVENDATS
                                           AND  VC.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                                           AND  LP.IDRESERVASFRONT IS NULL
                                           AND  LP.NUMEROPONTOS > 0
                                           AND ((LP.IDTIPOLANCPONTOTS IN (2,4)
                                                AND   :dataBaseInicialAnoAnterior <  LP.DATALANCAMENTO
                                                AND   :dataInicioApuracaoDebito >= LP.DATALANCAMENTO)
                                              OR
                                                (LP.IDTIPOLANCPONTOTS NOT IN (2,4)
                                                AND   :dataBaseInicialAnoAnterior <= LP.DATALANCAMENTO
                                                AND   :dataInicioApuracaoDebito >  LP.DATALANCAMENTO))
                                           AND  LP.IDMOTIVO = M.IDMOTIVOTS 
                                           AND  M.FLGCONTAUTILIZACAO = 'S'
                                           AND  LP.VALIDADECREDITO IS NULL
                                           AND  LP.DEBITOCREDITO = 'D'     
                                         GROUP BY LP.IDVENDAXCONTRATO    
                                      ) PONTOS,      
      
                                       (SELECT LP.IDVENDAXCONTRATO, SUM(LP.NUMEROPONTOS) AS PONTOSRF
                                         FROM   LANCPONTOSTS LP, RESERVASFRONT RF, 
                                                (SELECT IDRESERVASFRONT FROM RESERVASRCI GROUP BY IDRESERVASFRONT) RCI, 
                                                PARAMTS PAR, VENDAXCONTRATOTS VC, VENDATS V, REVCONTRATOTS R
                                         WHERE  LP.IDRESERVASFRONT = RF.IDRESERVASFRONT
                                           AND    RCI.IDRESERVASFRONT (+) = RF.IDRESERVASFRONT
                                           AND    LP.IDVENDAXCONTRATO = VC.IDVENDAXCONTRATO
                                           AND    VC.IDVENDATS = V.IDVENDATS
                                           AND    VC.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                                           AND    LP.NUMEROPONTOS > 0           AND    (                        
                                                 (RCI.IDRESERVASFRONT IS NULL 
                                            AND   (RF.STATUSRESERVA IN (0,1,2,3,4,5) OR (RF.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS IN (4,5)))
                                            AND   :dataInicioApuracaoDebito <= RF.DATACHEGPREVISTA
                                            AND   :dataProximoDebitoPontos >  RF.DATACHEGPREVISTA
                                            )
                                               OR
                                                 (RCI.IDRESERVASFRONT IS NOT NULL
                                            AND   (RF.STATUSRESERVA IN (0,1,2,3,4,5) OR (RF.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS IN (4,5)))
                                            AND   :dataInicioApuracaoDebito <= LP.DATALANCAMENTO
                                            AND   :dataProximoDebitoPontos >  LP.DATALANCAMENTO
                                            ))
                                         GROUP BY LP.IDVENDAXCONTRATO) RFV, 
       
                                       (SELECT LP.IDVENDAXCONTRATO, SUM(LP.NUMEROPONTOS) AS PONTOSRM
                                         FROM   LANCPONTOSTS LP, RESERVAMIGRADATS RM, 
                                                (SELECT IDRESERVAMIGRADA FROM RESERVASRCI GROUP BY IDRESERVAMIGRADA) RCI, 
                                                PARAMTS PAR, VENDAXCONTRATOTS VC, VENDATS V, REVCONTRATOTS R
                                         WHERE  LP.IDRESERVAMIGRADA = RM.IDRESERVAMIGRADA
                                           AND    RCI.IDRESERVAMIGRADA (+) = RM.IDRESERVAMIGRADA
                                           AND    LP.IDVENDAXCONTRATO      = VC.IDVENDAXCONTRATO
                                           AND    VC.IDVENDATS             = V.IDVENDATS
                                           AND    VC.IDVENDAXCONTRATO      = R.IDVENDAXCONTRNOVO (+)
                                           AND    LP.NUMEROPONTOS > 0  
                                           AND    ((RCI.IDRESERVAMIGRADA IS NULL 
                                                 AND :dataInicioApuracaoDebito <= RM.DATACHEGADA
                                                 AND :dataProximoDebitoPontos >  RM.DATACHEGADA)
                                              OR
                                                 (RCI.IDRESERVAMIGRADA IS NOT NULL
                                                 AND :dataInicioApuracaoDebito <= LP.DATALANCAMENTO
                                                 AND :dataProximoDebitoPontos >  LP.DATALANCAMENTO))
                                         GROUP BY LP.IDVENDAXCONTRATO) RMV,
      
                                       (SELECT LP.IDVENDAXCONTRATO, SUM(LP.NUMEROPONTOS)  AS PONTOSOUTROS
                                         FROM   LANCPONTOSTS LP, MOTIVOTS M, PARAMTS PAR, VENDAXCONTRATOTS VC, VENDATS V, REVCONTRATOTS R
                                         WHERE  LP.IDRESERVAMIGRADA IS NULL
                                           AND  LP.IDVENDAXCONTRATO = VC.IDVENDAXCONTRATO
                                           AND  VC.IDVENDATS        = V.IDVENDATS
                                           AND  VC.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                                           AND  LP.NUMEROPONTOS > 0           
                                           AND  ((LP.IDTIPOLANCPONTOTS IN (2,4) 
                                                 AND :dataInicioApuracaoDebito <  LP.DATALANCAMENTO
                                                 AND :dataProximoDebitoPontos >= LP.DATALANCAMENTO)
                                               OR
                                                (LP.IDTIPOLANCPONTOTS NOT IN (2,4)
                                                 AND :dataInicioApuracaoDebito <= LP.DATALANCAMENTO
                                                 AND :dataProximoDebitoPontos >  LP.DATALANCAMENTO))           
                                           AND  LP.IDMOTIVO = M.IDMOTIVOTS
                                           AND  M.FLGCONTAUTILIZACAO = 'S'
                                           AND  LP.DEBITOCREDITO = 'D'    
                                         GROUP BY LP.IDVENDAXCONTRATO) PONTOSV,
         
                                       (SELECT LP.IDVENDAXCONTRATO, SUM(LP.NUMEROPONTOS)  AS PONTOS
                                         FROM   LANCPONTOSTS LP, PARAMTS PAR, VENDAXCONTRATOTS VC, VENDATS V, REVCONTRATOTS R
                                         WHERE  LP.IDVENDAXCONTRATO = VC.IDVENDAXCONTRATO
                                           AND  VC.IDVENDATS        = V.IDVENDATS
                                           AND  VC.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                                           AND  LP.NUMEROPONTOS > 0
                                           AND  LP.DEBITOCREDITO = 'D'
                                           AND  LP.VALIDADECREDITO IS NOT NULL
                                           AND  :dataBaseInicialAnoAnterior = LP.VALIDADECREDITO
                                         GROUP BY LP.IDVENDAXCONTRATO) PONTOSVEXTRA,          
      
                                       (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'D',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS UTILIZACAO
                                         FROM LANCPONTOSTS LP
                                         WHERE NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6)
                                         GROUP BY IDVENDAXCONTRATO) U
     
                                WHERE  VC.FLGREVERTIDO          = 'N'
                                AND    (VC.IDVENDAXCONTRATO     = :idVendaXContrato)
                                AND    VC.FLGCANCELADO          = 'N'
                                AND    VC.IDVENDAXCONTRATO      = RF.IDVENDAXCONTRATO (+)
                                AND    VC.IDVENDAXCONTRATO      = RM.IDVENDAXCONTRATO (+)
                                AND    VC.IDVENDAXCONTRATO      = PONTOS.IDVENDAXCONTRATO (+)
                                AND    VC.IDVENDAXCONTRATO      = RFV.IDVENDAXCONTRATO (+)
                                AND    VC.IDVENDAXCONTRATO      = RMV.IDVENDAXCONTRATO (+)
                                AND    VC.IDVENDAXCONTRATO      = PONTOSV.IDVENDAXCONTRATO (+)
                                AND    VC.IDVENDAXCONTRATO      = PONTOSVEXTRA.IDVENDAXCONTRATO (+)
                                AND    VC.IDVENDAXCONTRATO      = R.IDVENDAXCONTRNOVO (+)
                                AND    VC.IDVENDAXCONTRATO      = U.IDVENDAXCONTRATO (+)
                                AND    V.IDVENDATS              = VC.IDVENDATS
                                AND    C.IDCONTRATOTS           = VC.IDCONTRATOTS
                                AND    A.IDATENDCLIENTETS       = V.IDATENDCLIENTETS
                                AND    P.IDPESSOA               = A.IDCLIENTE
                                AND    (NVL(C.DESCONTOANUAL, 0) > 0 OR NVL(C.TAXAANUAL, 0) > 0)

                                AND   ( (  (RF.IDVENDAXCONTRATO IS NULL) AND (RM.IDVENDAXCONTRATO IS NULL) AND (PONTOS.IDVENDAXCONTRATO IS NULL) )
                                   OR ( ( NVL(RF.PONTOSRF,0) + NVL(RM.PONTOSRM,0) + NVL(PONTOS.PONTOSOUTROS,0) + NVL(RFV.PONTOSRF,0) + NVL(RMV.PONTOSRM,0) + NVL(PONTOSV.PONTOSOUTROS,0)  ) <= C.UTILIZACAOMINIMAPONTOS ) )

                                AND    NVL(U.UTILIZACAO,0) < C.NUMEROPONTOS
                                AND    NVL(PONTOSVEXTRA.PONTOS,0) < C.DESCONTOANUAL
                                AND    (NVL(RFV.PONTOSRF,0) + NVL(RMV.PONTOSRM,0) + NVL(PONTOSV.PONTOSOUTROS,0)) < C.DESCONTOANUAL
                                AND    ADD_MONTHS(NVL(R.DATAREVERSAO,V.DATAVENDA), 12) <= PAR.DATASISTEMA
                                 AND C.FLGGERACREDNUTIL = 'S'
                                AND    :dataDebitoPrevisto <= :dataAtualMais6Meses
                                AND    :dataAtualMais6Meses >= :dataInicioApuracaoDebito",
                     new Parameter("idVendaXContrato", dadosFinanceiros.IdVendaXContrato.GetValueOrDefault())
                     ,new Parameter("dataDebitoPrevisto", dataDebitoPontosAtual.GetValueOrDefault().AddDays(-1).Date)
                     , new Parameter("dataBaseInicialAnoAnterior", dataDebitoPontosAtual.GetValueOrDefault().AddYears(-2).Date)
                     , new Parameter("dataProximoDebitoPontos", dataDebitoPontosAtual.GetValueOrDefault().Date)
                     ,new Parameter("dataAtualMais6Meses", DateTime.Today.AddMonths(6).Date)
                     ,new Parameter("dataInicioApuracaoDebito", dataInicioApuracaoPeriodoDebito.Date)
                     )).FirstOrDefault();
            if (result != null)
                await _cacheStore.AddAsync($"DebitoPorNaoUtilizacao_{dadosFinanceiros.IdVendaXContrato}", result,DateTimeOffset.Now.AddMinutes(10),1,_repositorySystem.CancellationToken);

            return result;
        }

        private async Task<List<PeriodoDisponivelResultModel>?> GetDisponibilidade(SearchDisponibilidadeModel searchModel,
            PeriodoDisponivelResultModel baseSaldoPontos,
            DadosFinanceirosContrato? condicaoFinanceira,
            decimal? ocupacaoMaxima = 70)
        {
            if (string.IsNullOrEmpty(searchModel.NumeroContrato) || searchModel.IdVendaXContrato.GetValueOrDefault(0) <= 0)
                throw new ArgumentException("Falha na busca de disponibilidade 'Contrato não encontrado'");

            if (baseSaldoPontos.IdVendaTs.GetValueOrDefault(0) <= 0 && condicaoFinanceira?.IdVendaTs.GetValueOrDefault(0) <= 0)
                throw new ArgumentException("Não foi possível identificar o IdVendaTs");

            AtendClienteTs? atendClienteTs = await GetAtendimentoCliente(baseSaldoPontos.IdVendaXContrato.GetValueOrDefault(0));
            if (atendClienteTs == null || atendClienteTs.IdVendaXContrato.GetValueOrDefault(0) == 0)
                throw new ArgumentException($"Não foi localizado os dados da venda");

            if (searchModel.TipoDeBusca == "A")
            {
                var dataInicial = searchModel.DataInicial.GetValueOrDefault().AddDays(-7);
                if (dataInicial < DateTime.Today.AddDays(1))
                    dataInicial = DateTime.Today.AddDays(1);

                searchModel.DataInicial = dataInicial;
                searchModel.DataFinal = searchModel.DataFinal.GetValueOrDefault().AddDays(15);
            }

            ContratoTsModel? padraoContrato = await GetPadraoContrato(atendClienteTs);

            if (condicaoFinanceira?.DataVenda != null && padraoContrato != null && padraoContrato.Validade.GetValueOrDefault(0) > 0 && padraoContrato.TipoValidade == "A")
            {
                padraoContrato.DataVencimentoContrato = condicaoFinanceira.DataVenda.GetValueOrDefault().Date.AddYears(padraoContrato.Validade.GetValueOrDefault()).AddDays(-1);
            }

            var disponibilidadeRetorno = new List<PeriodoDisponivelResultModel>();
            var disponibilidadeTemp = new List<PeriodoDisponivelResultModel>();
            if (!searchModel.DataFinal.HasValue)
                searchModel.DataFinal = searchModel.DataInicial.GetValueOrDefault().AddDays(10);

            var hoteisVinculadosContrato = await GetHoteis(baseSaldoPontos.IdContratoTs.GetValueOrDefault(), searchModel.TipoDeBusca == "E" ? searchModel.HotelId : "-1");

            if (hoteisVinculadosContrato == null || !hoteisVinculadosContrato.Any())
                return new List<PeriodoDisponivelResultModel>();

            if (condicaoFinanceira == null)
                throw new ArgumentException("Falha na busca de disponibilidade 'Contrato não encontrado'");

            //if (condicaoFinanceira.SaldoInadimplente.GetValueOrDefault(0) > 0)
            //    throw new ArgumentException("Falha na busca de disponibilidade: 'PARC_INAD'");

            if (!string.IsNullOrWhiteSpace(condicaoFinanceira.Status) && !condicaoFinanceira.Status.Contains("ativo", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("Falha na busca de disponibilidade: 'Contrato inativo'");

            ParamTs paramTs = await GetParamHotel();

            List<FracionamentoTsModel> fracionamentos = await GetFracionamentosCorrentes(atendClienteTs!.IdCliente.GetValueOrDefault(), paramTs);

            if (fracionamentos != null && fracionamentos.Any())
                fracionamentos = fracionamentos.Where(b => DateTime.Today.Subtract(b.DataLancamento.GetValueOrDefault()).Days < 365).AsList();

            var apenasDomingoOuQuarta = new List<int>() { 12500, 25000, 50000, 100000 }.Any(b => b == padraoContrato.NumeroPontos);
            var apenasDomingoOuQuinta = padraoContrato!.NumeroPontos == 7;

            if (searchModel.IdReservasFront.GetValueOrDefault(0) > 0 && fracionamentos != null && fracionamentos.Any(b=> b.IdReservasFront1 == searchModel.IdReservasFront))
            {
                fracionamentos = fracionamentos.Where(a => a.IdReservasFront1 != searchModel.IdReservasFront).AsList();
            }

            List<DateTime> datasPossiveisCheckin = GetDatasPossiveis(searchModel, apenasDomingoOuQuarta, apenasDomingoOuQuinta, padraoContrato);

            await GetDisponibiliades(searchModel, baseSaldoPontos, condicaoFinanceira, disponibilidadeRetorno, hoteisVinculadosContrato,ocupacaoMaxima);

            AplicarRestricoesDeUsoComBaseContratoAndVincularFracionamentos(searchModel, ref disponibilidadeRetorno, fracionamentos, apenasDomingoOuQuinta, datasPossiveisCheckin);

            if (disponibilidadeRetorno != null && disponibilidadeRetorno.Any())
            {
                disponibilidadeRetorno = RemoverDuplicidades(disponibilidadeRetorno);

                if (disponibilidadeRetorno.Any() && 
                    condicaoFinanceira.DataPrevistaLancamentoNU.HasValue && condicaoFinanceira.ValorPrevistoDebitoNU.GetValueOrDefault(0) > 0)
                {
                    foreach (var item in disponibilidadeRetorno)
                    {
                        item.DataVenda = condicaoFinanceira.DataVenda;
                        item.ValidadeCredito = condicaoFinanceira.DataPrevistaLancamentoNU;
                        item.CreditoPontos = condicaoFinanceira.ValorPrevistoDebitoNU;
                    }
                }

                if (padraoContrato.DataVencimentoContrato.HasValue)
                {
                    foreach (var item in disponibilidadeRetorno)
                    {
                        item.VencimentoContrato = padraoContrato.DataVencimentoContrato;
                    }
                }
            }

            return disponibilidadeRetorno != null &&
                disponibilidadeRetorno.Any() ?
                disponibilidadeRetorno.OrderBy(a => a.Checkin)
                .ThenByDescending(a => a.Checkout.GetValueOrDefault()
                .Subtract(a.Checkout.GetValueOrDefault())).Where(a=> a.Checkout <= padraoContrato.DataVencimentoContrato.GetValueOrDefault(DateTime.MaxValue)).AsList()
                : disponibilidadeRetorno;
        }

        private async Task<ContratoTsModel?> GetPadraoContrato(AtendClienteTs atendClienteTs)
        {
            var padraoContrato = (await _repository.FindBySql<ContratoTsModel>(@$"Select 
                                                                                    c.* 
                                                                                  From 
                                                                                    ContratoTs c 
                                                                                  Where 
                                                                                    c.IdContratoTs = 
                                                                                    (Select vxc.IdContratoTs From VendaXContratoTs vxc Where vxc.IdVendaXContrato = {atendClienteTs.IdVendaXContrato})")).FirstOrDefault();
            if (padraoContrato == null)
                throw new ArgumentException($"Não foi possível encontrar o ContratoTs vinculado a venda: {atendClienteTs.IdVendaXContrato}");

            return padraoContrato;
        }

        private List<PeriodoDisponivelResultModel> RemoverDuplicidades(List<PeriodoDisponivelResultModel> disponibilidadeRetorno)
        {
            List<PeriodoDisponivelResultModel> periodosDistintosRetornar = new List<PeriodoDisponivelResultModel>();
            foreach (var item in disponibilidadeRetorno.GroupBy(a => new { a.Checkin, a.Checkout, a.HotelId }))
            {
                var fst = item.First();
                periodosDistintosRetornar.Add(fst);
            }

            return periodosDistintosRetornar.AsList();
        }

        private List<DateTime> GetDatasPossiveis(SearchDisponibilidadeModel searchModel, bool apenasDomingoOuQuarta, bool apenasDomingoOuQuinta, ContratoTsModel padraoContrato)
        {
            var datasPossiveisCheckin = new List<DateTime>();
            for (int i = 0; i < searchModel.DataFinal.GetValueOrDefault().Subtract(searchModel.DataInicial.GetValueOrDefault()).Days; i++)
            {
                if (apenasDomingoOuQuarta)
                {
                    var data = searchModel.DataInicial.GetValueOrDefault().Date.AddDays(i);
                    if (data.DayOfWeek == DayOfWeek.Sunday)
                        datasPossiveisCheckin.Add(data);
                    else if (data.DayOfWeek == DayOfWeek.Wednesday)
                        datasPossiveisCheckin.Add(data);
                }
                else if (apenasDomingoOuQuinta)
                {
                    var data = searchModel.DataInicial.GetValueOrDefault().Date.AddDays(i);
                    if (data.DayOfWeek == DayOfWeek.Sunday)
                        datasPossiveisCheckin.Add(data);
                    else if (data.DayOfWeek == DayOfWeek.Thursday)
                        datasPossiveisCheckin.Add(data);
                }
            }

            if (padraoContrato != null && padraoContrato.DataVencimentoContrato.HasValue)
            {
                datasPossiveisCheckin = datasPossiveisCheckin.Where(a => a <= padraoContrato.DataVencimentoContrato.Value).AsList();
            }
           
            return datasPossiveisCheckin.AsList();
        }

        private async Task GetDisponibiliades(SearchDisponibilidadeModel searchModel, 
            PeriodoDisponivelResultModel baseSaldoPontos, 
            DadosFinanceirosContrato condicaoFinanceira,
            List<PeriodoDisponivelResultModel> disponibilidadeRetorno, 
            List<HotelModel> hoteisVinculadosContrato, 
            decimal? ocupacaoMaxima = 70)
        {
            var periodosVinculados = await GetPeriodosVinculadosContrato(baseSaldoPontos!.IdContratoTs.GetValueOrDefault(),
                hoteisVinculadosContrato.Select(a => a.HotelId.GetValueOrDefault()).Distinct().AsList(),
                searchModel.DataInicial.GetValueOrDefault(),
                searchModel.DataFinal.GetValueOrDefault());

            if (periodosVinculados != null && periodosVinculados.Any())
            {
                var tarifarios = await GetTarifarios(
                    searchModel.DataInicial.GetValueOrDefault(),
                    searchModel.DataFinal.GetValueOrDefault(),
                    baseSaldoPontos!.IdContratoTs.GetValueOrDefault(),
                    hoteisVinculadosContrato.Select(a => a.HotelId.GetValueOrDefault()).Distinct().AsList(),
                    periodosVinculados.Select(a => a.IdTipoUh.GetValueOrDefault()).Distinct().AsList());


                var disponibilidades = await GetDisponibilidadeExecute(hoteisVinculadosContrato.Select(a => a.HotelId.GetValueOrDefault()).AsList(),
                    periodosVinculados.Select(a => a.IdTipoUh.GetValueOrDefault()).Distinct().AsList(),
                    searchModel.DataInicial.GetValueOrDefault(),
                    searchModel.DataFinal.GetValueOrDefault(),ocupacaoMaxima);

                if (disponibilidades != null && disponibilidades.Any())
                {
                    var tiposUhs = tarifarios.Select(a => new { a.IdHotel, a.IdTipoUh }).Distinct().AsList();
                    foreach (var itemGroupHotel in tiposUhs.GroupBy(b => b.IdHotel))
                    {
                        var periodosVinculadosAoHotel = periodosVinculados.Where(a => a.IdHotel == itemGroupHotel.Key).AsList();

                        foreach (var itemDisponibilidade in disponibilidades
                            .Where(a => a.IdHotel == itemGroupHotel.Key))
                        {
                            foreach (var itemTipo in itemDisponibilidade.DisponibilidadesDias
                                .Where(c => c.QtdeDisponivel > 0).GroupBy(b => b.IdTipoUh))
                            {

                                var tipoFst = itemTipo.First();
                                var hotel = hoteisVinculadosContrato.First(a => a.HotelId == itemGroupHotel.Key);

                                var disponibilidadesOrdenadas = itemTipo.OrderBy(d => d.Data).AsList();
                                List<PeriodoDisponivelResultModel>? periodosPossiveis = await IniciarPeriodos(baseSaldoPontos,
                                    tarifarios.Where(a => a.IdHotel == itemDisponibilidade.IdHotel && a.IdTipoUh == tipoFst.IdTipoUh).AsList(),
                                    itemTipo,
                                    tipoFst,
                                    hotel,
                                    disponibilidadesOrdenadas,
                                    searchModel.DataInicial.GetValueOrDefault(),
                                    searchModel.DataFinal.GetValueOrDefault(),
                                    condicaoFinanceira);

                                if (periodosPossiveis != null && periodosPossiveis.Any())
                                {
                                    foreach (var novoPeriodo in periodosPossiveis)
                                    {
                                        if (disponibilidadeRetorno != null && !disponibilidadeRetorno.Any(b =>
                                        b.Checkin == novoPeriodo.Checkin &&
                                        b.Checkout == novoPeriodo.Checkout &&
                                        b.TipoUhId == novoPeriodo.TipoUhId &&
                                        b.HotelId == novoPeriodo.HotelId))
                                            disponibilidadeRetorno.Add(novoPeriodo);
                                    }

                                }

                            }
                        }
                    }

                }

            }

        }

        private void AplicarRestricoesDeUsoComBaseContratoAndVincularFracionamentos(SearchDisponibilidadeModel searchModel, ref List<PeriodoDisponivelResultModel>? disponibilidadeRetorno, List<FracionamentoTsModel> fracionamentos, bool apenasDomingoOuQuinta, List<DateTime> datasPossiveisCheckin)
        {
            if (disponibilidadeRetorno == null || !disponibilidadeRetorno.Any()) return;

            bool restringerPeriodosPorFaltaDePonto = _configuration.GetValue<bool>("RestringirPeriodoPorFaltaDePontos", true);

            if (disponibilidadeRetorno != null && disponibilidadeRetorno.Any() && fracionamentos != null && fracionamentos.Any())
            {
                VincularFracionamentos(disponibilidadeRetorno, fracionamentos, apenasDomingoOuQuinta);
            }

            if (disponibilidadeRetorno != null && disponibilidadeRetorno.Any())
            {
                RemoverPeriodosComBaseDatasEPontosInsuficientes(searchModel, disponibilidadeRetorno, restringerPeriodosPorFaltaDePonto);

                if (datasPossiveisCheckin.Any())
                {
                    RemoverPeriodosEmDatasDeCheckinCheckoutForaPadrao(disponibilidadeRetorno, apenasDomingoOuQuinta);
                }
                else disponibilidadeRetorno = new List<PeriodoDisponivelResultModel>();
            }

            
        }

        private void VincularFracionamentos(List<PeriodoDisponivelResultModel> disponibilidadeRetorno, List<FracionamentoTsModel> fracionamentos, bool apenasDomingoOuQuinta)
        {
            foreach (var item in disponibilidadeRetorno)
            {
                if (apenasDomingoOuQuinta)
                {
                    if (item.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Thursday && item.Diarias == 3)
                    {
                        var fechamentoFracionamentoUtilizar = fracionamentos.FirstOrDefault(b =>
                        b.CheckinReservasFront1.GetValueOrDefault().DayOfWeek == DayOfWeek.Thursday &&
                        b.CheckoutReservasFront1.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday && b.HotelId == item.HotelId);
                        if (fechamentoFracionamentoUtilizar != null)
                        {
                            if (DateTime.Today.Subtract(fechamentoFracionamentoUtilizar.DataLancamento.GetValueOrDefault()).Days <= 365)
                            {
                                item.HotelIdAberturaFracionamento = fechamentoFracionamentoUtilizar.HotelId;
                                item.QtdePessoasAberturaFechamento = fechamentoFracionamentoUtilizar.QtdePessoas;
                                item.ReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.NumReserva1;
                                item.CheckinReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.CheckinReservasFront1.GetValueOrDefault().Date;
                                item.CheckoutReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.CheckoutReservasFront1.GetValueOrDefault().Date;
                                item.FechamentoFracionamentoPossivelId = fechamentoFracionamentoUtilizar.IdFracionamentoTs;
                            }
                        }
                    }
                    
                }
                else
                {
                    if (item.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday && item.Diarias == 3)
                    {
                        var fechamentoFracionamentoUtilizar = fracionamentos.FirstOrDefault(b =>
                        b.CheckinReservasFront1.GetValueOrDefault().DayOfWeek == DayOfWeek.Wednesday &&
                        b.CheckoutReservasFront1.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday && b.HotelId == item.HotelId);
                        if (fechamentoFracionamentoUtilizar != null)
                        {
                            if (DateTime.Today.Subtract(fechamentoFracionamentoUtilizar.DataLancamento.GetValueOrDefault()).Days <= 365)
                            {
                                item.HotelIdAberturaFracionamento = fechamentoFracionamentoUtilizar.HotelId;
                                item.QtdePessoasAberturaFechamento = fechamentoFracionamentoUtilizar.QtdePessoas;
                                item.ReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.NumReserva1;
                                item.CheckinReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.CheckinReservasFront1.GetValueOrDefault().Date;
                                item.CheckoutReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.CheckoutReservasFront1.GetValueOrDefault().Date;
                                item.FechamentoFracionamentoPossivelId = fechamentoFracionamentoUtilizar.IdFracionamentoTs;
                            }
                        }
                    }
                    else if (item.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Wednesday && item.Diarias == 4)
                    {
                        var fechamentoFracionamentoUtilizar = fracionamentos.FirstOrDefault(b =>
                        b.CheckinReservasFront1.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday &&
                        b.CheckoutReservasFront1.GetValueOrDefault().DayOfWeek == DayOfWeek.Wednesday && b.HotelId == item.HotelId);
                        if (fechamentoFracionamentoUtilizar != null)
                        {
                            if (DateTime.Today.Subtract(fechamentoFracionamentoUtilizar.DataLancamento.GetValueOrDefault()).Days <= 365)
                            {
                                item.HotelIdAberturaFracionamento = fechamentoFracionamentoUtilizar.HotelId;
                                item.QtdePessoasAberturaFechamento = fechamentoFracionamentoUtilizar.QtdePessoas;
                                item.ReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.NumReserva1;
                                item.CheckinReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.CheckinReservasFront1.GetValueOrDefault().Date;
                                item.CheckoutReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.CheckoutReservasFront1.GetValueOrDefault().Date;
                                item.FechamentoFracionamentoPossivelId = fechamentoFracionamentoUtilizar.IdFracionamentoTs;
                            }
                        }
                    }
                    else if (item.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Thursday && item.Diarias == 3 && item.HotelId == 1)
                    {
                        var fechamentoFracionamentoUtilizar = fracionamentos.FirstOrDefault(b =>
                        b.CheckinReservasFront1.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday &&
                        b.CheckoutReservasFront1.GetValueOrDefault().DayOfWeek == DayOfWeek.Wednesday && b.HotelId == item.HotelId);
                        if (fechamentoFracionamentoUtilizar != null)
                        {
                            if (DateTime.Today.Subtract(fechamentoFracionamentoUtilizar.DataLancamento.GetValueOrDefault()).Days <= 365)
                            {
                                item.HotelIdAberturaFracionamento = fechamentoFracionamentoUtilizar.HotelId;
                                item.QtdePessoasAberturaFechamento = fechamentoFracionamentoUtilizar.QtdePessoas;
                                item.ReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.NumReserva1;
                                item.CheckinReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.CheckinReservasFront1.GetValueOrDefault().Date;
                                item.CheckoutReservaAberturaFracionamento = fechamentoFracionamentoUtilizar.CheckoutReservasFront1.GetValueOrDefault().Date;
                                item.FechamentoFracionamentoPossivelId = fechamentoFracionamentoUtilizar.IdFracionamentoTs;
                            }
                        }
                    }

                }
                
            }
        }

        private void RemoverPeriodosComBaseDatasEPontosInsuficientes(SearchDisponibilidadeModel searchModel, List<PeriodoDisponivelResultModel> disponibilidadeRetorno, bool restringerPeriodosPorFaltaDePonto)
        {
            if (searchModel.TipoDeBusca == "E")
            {
                for (int i = disponibilidadeRetorno.Count - 1; i >= 0; i--)
                {
                    var dispAtual = disponibilidadeRetorno[i];
                    if (!string.IsNullOrEmpty(searchModel.HotelId) && int.Parse(searchModel.HotelId) > 0)
                    {
                        if (dispAtual.HotelId != int.Parse(searchModel.HotelId))
                            disponibilidadeRetorno.Remove(dispAtual);
                    }

                    //var dispAtual = disponibilidadeRetorno[i];
                    //if ((dispAtual.Checkin.GetValueOrDefault() != searchModel.DataInicial.GetValueOrDefault().Date ||
                    //    dispAtual.Checkout.GetValueOrDefault().Date != searchModel.DataFinal.GetValueOrDefault().Date) ||
                    //    (dispAtual.PontosIntegralDisp < dispAtual.PontosNecessario && restringerPeriodosPorFaltaDePonto && dispAtual.FechamentoFracionamentoPossivelId.GetValueOrDefault(0) == 0))
                    //{
                    //    disponibilidadeRetorno.Remove(dispAtual);
                    //}
                }
            }
            else if (restringerPeriodosPorFaltaDePonto)
            {
                for (int i = disponibilidadeRetorno.Count - 1; i >= 0; i--)
                {
                    var dispAtual = disponibilidadeRetorno[i];

                    if (dispAtual.PontosIntegralDisp < dispAtual.PontosNecessario && dispAtual.FechamentoFracionamentoPossivelId.GetValueOrDefault(0) == 0)
                    {
                        disponibilidadeRetorno.Remove(dispAtual);
                    }
                }
            }
        }

        private void RemoverPeriodosEmDatasDeCheckinCheckoutForaPadrao(List<PeriodoDisponivelResultModel> disponibilidadeRetorno, bool apenasDomingoOuQuinta)
        {
            for (int i = disponibilidadeRetorno.Count - 1; i >= 0; i--)
            {
                var dispAtual = disponibilidadeRetorno[i];
                if (apenasDomingoOuQuinta)
                {
                    if (dispAtual.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday && dispAtual.Diarias != 7)
                    {
                        disponibilidadeRetorno.Remove(dispAtual);
                        continue;
                    }
                    else if (dispAtual.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Thursday && dispAtual.Diarias != 3)
                    {
                        disponibilidadeRetorno.Remove(dispAtual);
                        continue;
                    }
                    else if (dispAtual.Checkin.GetValueOrDefault().DayOfWeek != DayOfWeek.Thursday && dispAtual.Checkin.GetValueOrDefault().DayOfWeek != DayOfWeek.Sunday)
                    {
                        disponibilidadeRetorno.Remove(dispAtual);
                        continue;
                    }
                }
                else
                {

                    if (dispAtual.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday && (dispAtual.Diarias != 7 && dispAtual.Diarias != 3))
                    {
                        disponibilidadeRetorno.Remove(dispAtual);
                        continue;
                    }
                    else if (dispAtual.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Wednesday && dispAtual.Diarias != 4)
                    {
                        disponibilidadeRetorno.Remove(dispAtual);
                        continue;
                    }
                    else if (dispAtual.Checkin.GetValueOrDefault().DayOfWeek != DayOfWeek.Wednesday && dispAtual.Checkin.GetValueOrDefault().DayOfWeek != DayOfWeek.Sunday)
                    {
                        disponibilidadeRetorno.Remove(dispAtual);
                        continue;
                    }

                }
            }
        }

        private async Task<ConfigContratoModel?> GetConfigContrato(PeriodoDisponivelResultModel baseSaldoPontos)
        {
            var configContrato = (await _repository.FindBySql<ConfigContratoModel>($@"SELECT 
                                    VALOR, 
                                    VALIDADE, 
                                    NUMEROPONTOS, 
                                    IDTIPODCCONTRATO, 
                                    IDTIPODCTAXA, 
                                    IDTIPODCJUROS,              
                                    IDTIPODCMULTACANC, 
                                    IDTIPODCDIFDIBALC, 
                                    PERCINTEGRALIZA, 
                                    PERCTXMANUTPRIUTI,                  
                                    PERCPONTOSPRIUTI, 
                                    PERCTXMANUTSEGUTI, 
                                    PERCPONTOSSEGUTI, 
                                    TIPOVALIDADE,                       
                                    IDTIPODCDEBNUTIL, 
                                    IDTIPODCCREDNUTIL, 
                                    ANOINICIAL, 
                                    VALORPONTO, 
                                    FLGCONTABDIFERIDA,            
                                    VALORPERCPONTO, 
                                    FLGPRIMUTILGRATUI, 
                                    UTILALTATEMP, 
                                    FLGUTILVLRPROP, 
                                    FLGDTANIVERSARIO,         
                                    FLGTIPOCONTRATO, 
                                    PAGANTESREGRAFREE, 
                                    HOSPEDESFREE, 
                                    FLGADULTOSFREE, 
                                    FLGCRIANCA1FREE,         
                                    FLGCRIANCA2FREE, 
                                    FLGIDOSOFREE, 
                                    ANOSFREE, 
                                    MAXPAXFREERESERVA, 
                                    FLGDESCPONTOSFREE,             
                                    FLGDESCTAXAFREE, 
                                    FLGUSAPAXFREE, 
                                    OBSPADRAORESERVA, 
                                    IDTIPODCTAXAPENSAO,                      
                                    CLIENTERESERVANTE, 
                                    CONTRATOINICIAL, 
                                    FLGUTILPONTOSPROP                               
                                    FROM   
                                    CONTRATOTS                                                                          
                                    WHERE  IDCONTRATOTS = {baseSaldoPontos.IdContratoTs.GetValueOrDefault()}")).FirstOrDefault();
            return configContrato;
        }

        private async Task<List<PeriodoDisponivelResultModel>?> IniciarPeriodos(PeriodoDisponivelResultModel? baseSaldoPontos,
            List<TarifarioResultModel> tarifarios, IGrouping<int, DisponibilidadeDia> itemTipo, DisponibilidadeDia tipoFst,
            HotelModel hotel, List<DisponibilidadeDia> disponibilidadesOrdenadas, DateTime dataInicialInformada, DateTime dataFinalInformada,
            DadosFinanceirosContrato dadosFinanceiroContrato)
        {
            List<PeriodoDisponivelResultModel> listRetorno = new List<PeriodoDisponivelResultModel>();

            if (tarifarios == null || tarifarios.Count == 0) return null;

            var datasInicioTarifario = tarifarios.Where(c => c.IdTipoUh == tipoFst.IdTipoUh).Select(a => new
            {
                a.DataInicial,
                a.DataFinal,
                a.MinimoDias,
                a.NumMaxPax,
                a.TaxaManutencao,
                a.IdContratoTs,
                a.NumeroPontos,
                a.IdHotel,
                a.IdTipoUh,
                a.IdContrTsXPontos,
                a.TipoPeriodo,
                a.TrgDtInclusao
            }).Distinct();

            
            if (datasInicioTarifario != null && datasInicioTarifario.Any())
            {
                var dataBase = dataInicialInformada;
                while (dataBase <= dataFinalInformada)
                {

                    foreach (var item in datasInicioTarifario)
                    {
                        var dataFinal = dataBase.AddDays(item.MinimoDias.GetValueOrDefault());

                        var temporada = (dataFinal.Month == 1 || dataFinal.Month == 7 || dataBase.Month == 1 || dataBase.Month == 7);

                        if ((item.MinimoDias.GetValueOrDefault() == 7 || temporada) && item.DataInicial <= dataBase.Date && 
                            item.DataFinal >= dataBase.Date && item.DataInicial <= dataFinal.Date && item.DataFinal >= dataFinal.Date)
                        {
                            // Começa um novo período.
                            PeriodoDisponivelResultModel? periodoAtual = new PeriodoDisponivelResultModel
                            {
                                Checkin = dataBase,
                                Checkout = dataBase.AddDays(temporada ? 7 : item.MinimoDias.GetValueOrDefault()),
                                TipoUhId = itemTipo.Key,
                                TipoApartamento = $"{tipoFst.CodigoTipoUh} - {tipoFst.NomeTipoApto}",
                                NomeHotel = hotel.HotelNome,
                                HotelId = hotel.HotelId,
                                CodTipoUh = tipoFst.CodigoTipoUh,
                                SaldoPontos = baseSaldoPontos?.SaldoPontos.GetValueOrDefault(0),
                                Capacidade = tipoFst.Capacidade,
                                IdContratoTs = baseSaldoPontos?.IdContratoTs,
                                PontosIntegralDisp = dadosFinanceiroContrato?.PontosIntegralizadosDisponiveis,
                                TipoPeriodo = item.TipoPeriodo,
                                IdVendaTs = dadosFinanceiroContrato?.IdVendaTs,
                                IdVendaXContrato = dadosFinanceiroContrato?.IdVendaXContrato,
                                NumeroContrato = dadosFinanceiroContrato?.NumeroContrato,
                            };

                            if (periodoAtual.Checkin < dataInicialInformada || periodoAtual.Checkout > dataFinalInformada) continue;

                            if (datasInicioTarifario.Any(c => periodoAtual.Checkin >= c.DataInicial && periodoAtual.Checkout <= c.DataFinal.GetValueOrDefault()))
                            {
                                var tarifariosConsiderar = datasInicioTarifario.Where(a => a.DataFinal.GetValueOrDefault().Date.Subtract(a.DataInicial.GetValueOrDefault().Date).Days <= 7 && 
                                a.DataInicial <= periodoAtual.Checkin && a.DataFinal >= periodoAtual.Checkout)
                                    .OrderByDescending(a=> a.TrgDtInclusao).ThenBy(c => c.NumMaxPax).AsList();

                                if (!tarifariosConsiderar.Any())
                                {
                                    tarifariosConsiderar = datasInicioTarifario.Where(a => a.DataInicial <= periodoAtual.Checkin && a.DataFinal >= periodoAtual.Checkout)
                                    .OrderByDescending(a => a.TrgDtInclusao).ThenBy(c => c.NumMaxPax).AsList();
                                }

                                if (tarifariosConsiderar != null && tarifariosConsiderar.Any())
                                {
                                    try
                                    {
                                        var tarifUtilizarMax =
                                            tarifariosConsiderar.Where(c => c.DataInicial.GetValueOrDefault().Date == periodoAtual.Checkin.GetValueOrDefault().Date &&
                                                c.DataFinal.GetValueOrDefault().Date == periodoAtual.Checkout.GetValueOrDefault().Date)?
                                                .OrderByDescending(b => b.NumeroPontos.GetValueOrDefault())
                                                .FirstOrDefault() ??
                                            tarifariosConsiderar.Where(c => c.DataInicial.GetValueOrDefault().Date <= periodoAtual.Checkin.GetValueOrDefault().Date &&
                                                c.DataFinal.GetValueOrDefault().Date >= periodoAtual.Checkout.GetValueOrDefault().Date)?
                                                .OrderByDescending(b => b.NumeroPontos.GetValueOrDefault())
                                                .FirstOrDefault();

                                        if (tarifUtilizarMax == null)
                                            continue;

                                        var tarifUtilizarMin =
                                            tarifariosConsiderar.Where(c => c.DataInicial.GetValueOrDefault().Date == periodoAtual.Checkin.GetValueOrDefault().Date &&
                                             c.DataFinal.GetValueOrDefault().Date == periodoAtual.Checkout.GetValueOrDefault().Date)?
                                            .OrderBy(b => b.NumeroPontos.GetValueOrDefault())
                                            .FirstOrDefault()
                                            ?? tarifariosConsiderar.Where(c => c.DataInicial.GetValueOrDefault().Date <= periodoAtual.Checkin.GetValueOrDefault().Date &&
                                             c.DataFinal.GetValueOrDefault().Date >= periodoAtual.Checkout.GetValueOrDefault().Date)?
                                            .OrderBy(b => b.NumeroPontos.GetValueOrDefault())
                                            .FirstOrDefault();

                                        if (tarifUtilizarMin == null)
                                            continue;

                                        periodoAtual.PontosNecessario = tarifUtilizarMax.NumeroPontos.GetValueOrDefault();
                                        if (tarifUtilizarMin.NumMaxPax != tarifUtilizarMax.NumMaxPax)
                                        {
                                            periodoAtual.PadraoTarifario = $"De 1 a {tarifUtilizarMin.NumMaxPax} pessoas: {tarifUtilizarMin.NumeroPontos:N0} pontos";
                                            periodoAtual.PadraoTarifario += $" ou de {tarifUtilizarMin.NumMaxPax + 1} a {tarifUtilizarMax.NumMaxPax} pessoas: {tarifUtilizarMax.NumeroPontos:N0} pontos";
                                            periodoAtual.Capacidade = tarifUtilizarMax.NumMaxPax;
                                            periodoAtual.IdContrTsXPontos1 = tarifUtilizarMin.IdContrTsXPontos;
                                            periodoAtual.CapacidadePontos1 = tarifUtilizarMin.NumMaxPax;
                                            periodoAtual.PontosParaCapacidade1 = tarifUtilizarMin.NumeroPontos.GetValueOrDefault(0);
                                            periodoAtual.IdContrTsXPontos2 = tarifUtilizarMax.IdContrTsXPontos;
                                            periodoAtual.CapacidadePontos2 = tarifUtilizarMax.NumMaxPax;
                                            periodoAtual.PontosParaCapacidade2 = tarifUtilizarMax.NumeroPontos.GetValueOrDefault(0);
                                            periodoAtual.TipoPeriodo = tarifUtilizarMax.TipoPeriodo;
                                        }
                                        else
                                        {
                                            periodoAtual.PadraoTarifario = $"De 1 a {tarifUtilizarMax.NumMaxPax} pessoas: {tarifUtilizarMax.NumeroPontos:N0} pontos";
                                            periodoAtual.Capacidade = tarifUtilizarMax.NumMaxPax;
                                            periodoAtual.IdContrTsXPontos1 = tarifUtilizarMax.IdContrTsXPontos;
                                            periodoAtual.CapacidadePontos1 = tarifUtilizarMax.NumMaxPax;
                                            periodoAtual.PontosParaCapacidade1 = tarifUtilizarMax.NumeroPontos.GetValueOrDefault(0);
                                            periodoAtual.TipoPeriodo = tarifUtilizarMax.TipoPeriodo;
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        _logger.LogError("Erro ao calcular pontos necessários.", err.Message);
                                    }
                                }

                                listRetorno.Add(periodoAtual);
                            }
                        }
                        else if ((item.MinimoDias.GetValueOrDefault() != 7 && !temporada) && item.DataInicial <= dataBase.Date && item.DataFinal >= dataBase.Date)
                        {
                            if (!string.IsNullOrEmpty(item.TipoPeriodo) && (item.TipoPeriodo.RemoveAccents().Contains("baixa", StringComparison.InvariantCultureIgnoreCase) ||
                                item.TipoPeriodo.RemoveAccents().Contains("media", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                for (int i = item.MinimoDias.GetValueOrDefault(); i <= 7; i++)
                                {

                                    var dataFinalMenosQue7Dias = dataBase.AddDays(i);

                                    if (item.DataInicial <= dataBase.Date &&
                                            item.DataFinal >= dataBase.Date && item.DataInicial <= dataFinalMenosQue7Dias.Date && item.DataFinal >= dataFinalMenosQue7Dias.Date)
                                    {
                                        // Começa um novo período.
                                        PeriodoDisponivelResultModel? novoPeriodo = new PeriodoDisponivelResultModel
                                        {
                                            Checkin = dataBase,
                                            Checkout = dataBase.AddDays(i),
                                            TipoUhId = itemTipo.Key,
                                            TipoApartamento = $"{tipoFst.CodigoTipoUh} - {tipoFst.NomeTipoApto}",
                                            NomeHotel = hotel.HotelNome,
                                            HotelId = hotel.HotelId,
                                            CodTipoUh = tipoFst.CodigoTipoUh,
                                            SaldoPontos = baseSaldoPontos?.SaldoPontos.GetValueOrDefault(0),
                                            Capacidade = tipoFst.Capacidade,
                                            IdContratoTs = baseSaldoPontos?.IdContratoTs,
                                            PontosIntegralDisp = dadosFinanceiroContrato?.PontosIntegralizadosDisponiveis,
                                            TipoPeriodo = item.TipoPeriodo,
                                            IdVendaTs = dadosFinanceiroContrato?.IdVendaTs,
                                            IdVendaXContrato = dadosFinanceiroContrato?.IdVendaXContrato,
                                            NumeroContrato = dadosFinanceiroContrato?.NumeroContrato
                                        };

                                        if (novoPeriodo.Checkin < dataInicialInformada || novoPeriodo.Checkout > dataFinalInformada) continue;
                                        if (datasInicioTarifario.Any(c => novoPeriodo.Checkin >= c.DataInicial && novoPeriodo.Checkout <= c.DataFinal.GetValueOrDefault()))
                                        {
                                            var tarifariosConsiderar = datasInicioTarifario.Where(a => a.DataFinal.GetValueOrDefault().Date.Subtract(a.DataInicial.GetValueOrDefault().Date).Days <= 7 &&
                                            a.DataInicial <= novoPeriodo.Checkin && a.DataFinal >= novoPeriodo.Checkout)
                                                .OrderByDescending(a => a.TrgDtInclusao).ThenBy(c => c.NumMaxPax).AsList();


                                            if (!tarifariosConsiderar.Any())
                                            {
                                                tarifariosConsiderar = datasInicioTarifario.Where(a => a.DataInicial.GetValueOrDefault().Date <= novoPeriodo.Checkin.GetValueOrDefault().Date && a.DataFinal.GetValueOrDefault().Date >= novoPeriodo.Checkout.GetValueOrDefault().Date)
                                                .OrderByDescending(a => a.TrgDtInclusao).ThenBy(c => c.NumMaxPax).AsList();
                                            }

                                            if (tarifariosConsiderar != null && tarifariosConsiderar.Any())
                                            {
                                                try
                                                {

                                                    var tarifMaxPontos =
                                                        tarifariosConsiderar.Where(c => c.DataInicial.GetValueOrDefault().Date == novoPeriodo.Checkin.GetValueOrDefault().Date &&
                                                    c.DataFinal.GetValueOrDefault().Date == novoPeriodo.Checkout.GetValueOrDefault().Date)?
                                                    .OrderByDescending(b => b.NumeroPontos.GetValueOrDefault())
                                                    .FirstOrDefault()
                                                        ?? tarifariosConsiderar.Where(c => c.DataInicial.GetValueOrDefault().Date <= novoPeriodo.Checkin.GetValueOrDefault().Date &&
                                                    c.DataFinal.GetValueOrDefault().Date >= novoPeriodo.Checkout.GetValueOrDefault().Date)?
                                                    .OrderByDescending(b => b.NumeroPontos.GetValueOrDefault())
                                                    .FirstOrDefault();

                                                    if (tarifMaxPontos == null)
                                                        continue;

                                                    var tarifMinPontos =
                                                        tarifariosConsiderar.Where(c => c.DataInicial.GetValueOrDefault().Date == novoPeriodo.Checkin.GetValueOrDefault().Date &&
                                                         c.DataFinal.GetValueOrDefault().Date == novoPeriodo.Checkout.GetValueOrDefault().Date)?
                                                        .OrderBy(b => b.NumeroPontos.GetValueOrDefault())
                                                        .FirstOrDefault() ??
                                                    tarifariosConsiderar.Where(c => c.DataInicial.GetValueOrDefault().Date <= novoPeriodo.Checkin.GetValueOrDefault().Date &&
                                                         c.DataFinal.GetValueOrDefault().Date >= novoPeriodo.Checkout.GetValueOrDefault().Date)?
                                                        .OrderBy(b => b.NumeroPontos.GetValueOrDefault())
                                                        .FirstOrDefault();

                                                    if (tarifMinPontos == null)
                                                        continue;

                                                    novoPeriodo.PontosNecessario = tarifMaxPontos.NumeroPontos.GetValueOrDefault();
                                                    if (tarifMinPontos.NumMaxPax != tarifMaxPontos.NumMaxPax)
                                                    {
                                                        novoPeriodo.PadraoTarifario = $"De 1 a {tarifMinPontos.NumMaxPax} pessoas: {tarifMinPontos.NumeroPontos:N0} pontos";
                                                        novoPeriodo.PadraoTarifario += $" ou de {tarifMinPontos.NumMaxPax + 1} a {tarifMaxPontos.NumMaxPax} pessoas: {tarifMaxPontos.NumeroPontos:N0} pontos";
                                                        novoPeriodo.Capacidade = tarifMaxPontos.NumMaxPax;
                                                        novoPeriodo.IdContrTsXPontos1 = tarifMinPontos.IdContrTsXPontos;
                                                        novoPeriodo.CapacidadePontos1 = tarifMinPontos.NumMaxPax;
                                                        novoPeriodo.PontosParaCapacidade1 = tarifMinPontos.NumeroPontos.GetValueOrDefault(0);
                                                        novoPeriodo.IdContrTsXPontos2 = tarifMaxPontos.IdContrTsXPontos;
                                                        novoPeriodo.CapacidadePontos2 = tarifMaxPontos.NumMaxPax;
                                                        novoPeriodo.PontosParaCapacidade2 = tarifMaxPontos.NumeroPontos.GetValueOrDefault(0);
                                                        novoPeriodo.TipoPeriodo = tarifMaxPontos.TipoPeriodo;

                                                    }
                                                    else
                                                    {
                                                        novoPeriodo.PadraoTarifario = $"De 1 a {tarifMaxPontos.NumMaxPax} pessoas: {tarifMaxPontos.NumeroPontos:N0} pontos";
                                                        novoPeriodo.Capacidade = tarifMaxPontos.NumMaxPax;
                                                        novoPeriodo.IdContrTsXPontos1 = tarifMaxPontos.IdContrTsXPontos;
                                                        novoPeriodo.CapacidadePontos1 = tarifMaxPontos.NumMaxPax;
                                                        novoPeriodo.PontosParaCapacidade1 = tarifMaxPontos.NumeroPontos.GetValueOrDefault(0);
                                                        novoPeriodo.TipoPeriodo = tarifMaxPontos.TipoPeriodo;
                                                    }
                                                }
                                                catch (Exception err)
                                                {
                                                    _logger.LogError("Erro ao calcular pontos necessários.", err.Message);
                                                }
                                            }

                                            listRetorno.Add(novoPeriodo);
                                        }
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(item.TipoPeriodo) && 
                                ((item.TipoPeriodo.RemoveAccents().Contains("alta", StringComparison.InvariantCultureIgnoreCase) || temporada) &&
                                dadosFinanceiroContrato != null && dadosFinanceiroContrato.NumeroPontos.GetValueOrDefault(0) == 7))
                            {

                                // Começa um novo período.
                                PeriodoDisponivelResultModel? periodoAtual = new PeriodoDisponivelResultModel
                                {
                                    Checkin = dataBase,
                                    Checkout = dataBase.AddDays(temporada ? 7 : item.MinimoDias.GetValueOrDefault()),
                                    TipoUhId = itemTipo.Key,
                                    TipoApartamento = $"{tipoFst.CodigoTipoUh} - {tipoFst.NomeTipoApto}",
                                    NomeHotel = hotel.HotelNome,
                                    HotelId = hotel.HotelId,
                                    CodTipoUh = tipoFst.CodigoTipoUh,
                                    SaldoPontos = baseSaldoPontos?.SaldoPontos.GetValueOrDefault(0),
                                    Capacidade = tipoFst.Capacidade,
                                    IdContratoTs = baseSaldoPontos?.IdContratoTs,
                                    PontosIntegralDisp = dadosFinanceiroContrato?.PontosIntegralizadosDisponiveis,
                                    TipoPeriodo = item.TipoPeriodo,
                                    IdVendaTs = dadosFinanceiroContrato?.IdVendaTs,
                                    IdVendaXContrato = dadosFinanceiroContrato?.IdVendaXContrato,
                                    NumeroContrato = dadosFinanceiroContrato?.NumeroContrato,
                                };


                                if (periodoAtual.Checkin < dataInicialInformada || periodoAtual.Checkout > dataFinalInformada) continue;

                                if (datasInicioTarifario.Any(c => periodoAtual.Checkin >= c.DataInicial && periodoAtual.Checkout <= c.DataFinal.GetValueOrDefault()))
                                {
                                    var tarifariosConsiderar = datasInicioTarifario.Where(a => a.DataFinal.GetValueOrDefault().Date.Subtract(a.DataInicial.GetValueOrDefault().Date).Days <= 7 &&
                                        a.DataInicial <= periodoAtual.Checkin && a.DataFinal >= periodoAtual.Checkout)
                                    .OrderByDescending(a => a.TrgDtInclusao).ThenBy(c => c.NumMaxPax).AsList();

                                    if (!tarifariosConsiderar.Any())
                                    {
                                        tarifariosConsiderar = datasInicioTarifario.Where(a => a.DataInicial <= periodoAtual.Checkin && a.DataFinal >= periodoAtual.Checkout)
                                        .OrderByDescending(a => a.TrgDtInclusao).ThenBy(c => c.NumMaxPax).AsList();
                                    }

                                    if (tarifariosConsiderar != null && tarifariosConsiderar.Any())
                                    {
                                        try
                                        {

                                            var tarifMaxPontos = tarifariosConsiderar
                                                .Where(b => b.DataInicial.GetValueOrDefault().Date == periodoAtual.Checkin.GetValueOrDefault().Date &&
                                                b.DataFinal.GetValueOrDefault().Date == periodoAtual.Checkout.GetValueOrDefault().Date)?
                                                .OrderByDescending(c => c.NumeroPontos)
                                                .FirstOrDefault() ??
                                                tarifariosConsiderar
                                                .Where(b => b.DataInicial.GetValueOrDefault().Date <= periodoAtual.Checkin.GetValueOrDefault().Date &&
                                                b.DataFinal.GetValueOrDefault().Date >= periodoAtual.Checkout.GetValueOrDefault().Date)?
                                                .OrderByDescending(c => c.NumeroPontos)
                                                .FirstOrDefault();


                                            if (tarifMaxPontos == null)
                                                continue;


                                            var tarifMinPontos = tarifariosConsiderar
                                                .Where(b => b.DataInicial.GetValueOrDefault().Date == periodoAtual.Checkin.GetValueOrDefault().Date &&
                                                b.DataFinal.GetValueOrDefault().Date == periodoAtual.Checkout.GetValueOrDefault().Date)?
                                                .OrderByDescending(c => c.NumeroPontos)
                                                .FirstOrDefault() ??
                                                tarifariosConsiderar
                                                .Where(b => b.DataInicial.GetValueOrDefault().Date <= periodoAtual.Checkin.GetValueOrDefault().Date &&
                                                b.DataFinal.GetValueOrDefault().Date >= periodoAtual.Checkout.GetValueOrDefault().Date)?
                                                .OrderByDescending(c => c.NumeroPontos)
                                                .FirstOrDefault();

                                            if (tarifMinPontos == null)
                                                continue;


                                            periodoAtual.PontosNecessario = tarifMaxPontos.NumeroPontos.GetValueOrDefault();
                                            if (tarifMinPontos.NumMaxPax != tarifMaxPontos.NumMaxPax)
                                            {
                                                periodoAtual.PadraoTarifario = $"De 1 a {tarifMinPontos.NumMaxPax} pessoas: {tarifMinPontos.NumeroPontos:N0} pontos";
                                                periodoAtual.PadraoTarifario += $" ou de {tarifMinPontos.NumMaxPax + 1} a {tarifMaxPontos.NumMaxPax} pessoas: {tarifMaxPontos.NumeroPontos:N0} pontos";
                                                periodoAtual.Capacidade = tarifMaxPontos.NumMaxPax;
                                                periodoAtual.IdContrTsXPontos1 = tarifMinPontos.IdContrTsXPontos;
                                                periodoAtual.CapacidadePontos1 = tarifMinPontos.NumMaxPax;
                                                periodoAtual.PontosParaCapacidade1 = tarifMinPontos.NumeroPontos.GetValueOrDefault(0);
                                                periodoAtual.IdContrTsXPontos2 = tarifMaxPontos.IdContrTsXPontos;
                                                periodoAtual.CapacidadePontos2 = tarifMaxPontos.NumMaxPax;
                                                periodoAtual.PontosParaCapacidade2 = tarifMaxPontos.NumeroPontos.GetValueOrDefault(0);
                                                periodoAtual.TipoPeriodo = tarifMaxPontos.TipoPeriodo;
                                            }
                                            else
                                            {
                                                periodoAtual.PadraoTarifario = $"De 1 a {tarifMaxPontos.NumMaxPax} pessoas: {tarifMaxPontos.NumeroPontos:N0} pontos";
                                                periodoAtual.Capacidade = tarifMaxPontos.NumMaxPax;
                                                periodoAtual.IdContrTsXPontos1 = tarifMaxPontos.IdContrTsXPontos;
                                                periodoAtual.CapacidadePontos1 = tarifMaxPontos.NumMaxPax;
                                                periodoAtual.PontosParaCapacidade1 = tarifMaxPontos.NumeroPontos.GetValueOrDefault(0);
                                                periodoAtual.TipoPeriodo = tarifMaxPontos.TipoPeriodo;
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            _logger.LogError("Erro ao calcular pontos necessários.", err.Message);
                                        }
                                    }

                                    listRetorno.Add(periodoAtual);
                                }
                            }
                        }

                    }
                    dataBase = dataBase.AddDays(1);

                }
            }

            await FiltrarPeriodosDisponiveis(listRetorno, disponibilidadesOrdenadas, dadosFinanceiroContrato!);

            return listRetorno;
        }

        private async Task FiltrarPeriodosDisponiveis(IList<PeriodoDisponivelResultModel> listRetorno, List<DisponibilidadeDia> disponibilidadesOrdenadas, DadosFinanceirosContrato dadosFinanceiroContrato)
        {
            foreach (var item in listRetorno.Reverse())
            {
                bool remover = false;
                var dataAtual = item.Checkin;
                while (dataAtual < item.Checkout)
                {
                    var existeDisponibilidade = disponibilidadesOrdenadas.FirstOrDefault(a => a.Data.Date == dataAtual.GetValueOrDefault().Date && a.QtdeDisponivel > 0 && a.IdTipoUh == item.TipoUhId);
                    if (existeDisponibilidade == null)
                    {
                        remover = true;
                        break;
                    }
                    dataAtual = dataAtual.GetValueOrDefault().Date.AddDays(1);
                }
                if (remover)
                    listRetorno.Remove(item);
            }



            await Task.CompletedTask;
        }

        private async Task<List<PeriodoVinculadoContratoModel>> GetPeriodosVinculadosContrato(int idContratoTs, List<int> hoteis, DateTime? dataInicial, DateTime? dataFinal)
        {
            var periodosPermitidos = (await _repository.FindBySql<PeriodoVinculadoContratoModel>($@"SELECT 
            CP.IDCONTRTSXPONTOS, 
            CP.IDHOTEL, 
            TD.FLGFERIADO, 
            TU.IDTIPOUH, 
            CP.FLGDESCONTO, 
            D.DATA, 
            TD.IDTEMPORADATS,
            DECODE(TEM.FLGTIPO, 'S','Super alta','A','Alta','M','Média','B','Baixa') AS TIPOPERIODO
              FROM CM.CONTRTSXPONTOS CP
              JOIN CM.TEMPORADATSXDATA TD ON TD.IDTEMPORADATS = CP.IDTEMPORADATS
              JOIN CM.TIPOUHXPONTOTS TU ON CP.IDCONTRTSXPONTOS = TU.IDCONTRTSXPONTOS
              JOIN CM.TEMPORADATS TEM ON TD.IDTEMPORADATS = TEM.IDTEMPORADATS
              JOIN CM.DATASIS D ON D.IDHOTEL = CP.IDHOTEL
             WHERE CP.IDCONTRATOTS = :idContratoTs
               AND CP.IDHOTEL      IN ({string.Join(",", hoteis)})
               AND ((:dataInicial BETWEEN TD.DATAINICIAL-40 AND TD.DATAFINAL+40 AND :dataFinal BETWEEN TD.DATAINICIAL-40 AND TD.DATAFINAL+40 AND TD.FLGFERIADO = 'N')
                OR (TD.DATAFINAL >= :dataInicial AND  TD.DATAINICIAL <= :dataFinal AND TD.FLGFERIADO  = 'S'))
               AND TD.FLGBLOQUEIAUSO  <> 'S'
               AND D.DATA BETWEEN TD.DATAINICIAL AND TD.DATAFINAL
             ORDER BY CP.IDHOTEL, TU.IDTIPOUH, D.DATA, CP.FLGDESCONTO DESC, TD.FLGFERIADO DESC"
             , new Parameter("idContratoTs", idContratoTs)
             , new Parameter("dataInicial", dataInicial!)
             , new Parameter("dataFinal", dataFinal!))).AsList();

            return periodosPermitidos;
        }

        private async Task<List<TarifarioResultModel>> GetTarifarios(DateTime dataInicial, DateTime dataFinal, int idContratoTs, List<int> hoteisIds, List<int> tiposAptosIds)
        {
            List<(DateTime dataIni, DateTime dataFim)> datas = GetDatasPesquisarTarifarios(dataInicial, dataFinal, DayOfWeek.Sunday);

            var tiposAptosDoHotel = (await _repository.FindBySql<TipoUhModel>($"Select t.IdTipoUh, t.IdHotel From TipoUh t Where t.IdHotel in ({string.Join(",",hoteisIds)})")).AsList();
            if (tiposAptosDoHotel != null && tiposAptosDoHotel.Any())
            {
                tiposAptosIds = tiposAptosDoHotel.Select(b => b.IdTipoUh.GetValueOrDefault()).Distinct().AsList();
            }

            List<TarifarioResultModel> tarifariosResult = new List<TarifarioResultModel>();

            foreach (var item in datas)
            {
                var tarifarios = (await _repository.FindBySql<TarifarioResultModel>($@"SELECT   
                    TD.DATAINICIAL, TD.DATAFINAL,CP.IDCONTRTSXPONTOS, CP.IDCONTRATOTS, CP.IDHOTEL, CP.IDTEMPORADATS, CP.NUMMINPAX, '               ' AS TIPOUH,
                             CP.NUMMAXPAX, CP.CRIANCAS1, CP.CRIANCAS2, TD.FLGFERIADO, TD.FLGBLOQUEIAUSO, CP.NUMERODIAS, CP.MINIMODIAS, TU.IDTIPOUH, TU.QUANT,
                             (SELECT SUM(QUANT) FROM TIPOUHXPONTOTS WHERE IDCONTRTSXPONTOS = CP.IDCONTRTSXPONTOS) QUANTAPTO, CP.FLGDESCONTO,
                             TO_NUMBER(DECODE(C.FLGUTILVLRPROP,'N',CP.TAXAMANUTENCAO, (CP.TAXAMANUTENCAO / CP.NUMERODIAS) * (TRUNC(TO_NUMBER(:dataFinal - :dataInicial))+1)  )) AS TAXAMANUTENCAO,
                             TO_NUMBER(DECODE(C.FLGUTILPONTOSPROP,'N',CP.NUMEROPONTOS, (CP.NUMEROPONTOS / CP.NUMERODIAS) * (TRUNC(TO_NUMBER(:dataFinal - :dataInicial))+1) )) AS NUMEROPONTOS,
                             CP.VLRREPHOTEL,
                             DECODE(TEM.FLGTIPO, 'S','Super alta','A','Alta','M','Média','B','Baixa') AS TIPOPERIODO,
                             C.FlgAdultosFree, C.FlgCrianca1Free, C.FlgCrianca2Free, C.AnosFree, C.PagantesRegraFree, TD.TRGDTINCLUSAO
                    FROM     CONTRTSXPONTOS CP, TEMPORADATSXDATA TD, CONTRATOTS C, TIPOUHXPONTOTS TU, TEMPORADATS TEM
                    WHERE    TD.IDTEMPORADATS    = CP.IDTEMPORADATS
                    AND      CP.IDCONTRATOTS     = C.IDCONTRATOTS
                    AND      CP.IDCONTRTSXPONTOS = TU.IDCONTRTSXPONTOS
                    AND      CP.IDCONTRATOTS     = :idContratoTs
                    AND      TD.IDTEMPORADATS = TEM.IDTEMPORADATS
                    AND     ((:dataInicial BETWEEN TD.DATAINICIAL-40 AND TD.DATAFINAL+40 AND :dataFinal BETWEEN TD.DATAINICIAL-40 AND TD.DATAFINAL+40 AND TD.FLGFERIADO = 'N')
                     OR      (TD.DATAFINAL >= :dataInicial AND  TD.DATAINICIAL <= :dataFinal AND TD.FLGFERIADO  = 'S'))
                    AND      TD.FLGBLOQUEIAUSO  <> 'S'
                    AND      TU.IDTIPOUH IN ({string.Join(",", tiposAptosIds)})
                    AND      CP.IDHOTEL IN ({string.Join(",", hoteisIds)})
                    AND      (SELECT SUM(QUANT) FROM TIPOUHXPONTOTS WHERE IDCONTRTSXPONTOS = CP.IDCONTRTSXPONTOS) = 1
                    ORDER BY TD.DATAFINAL DESC",
                                new Parameter("idContratoTs", idContratoTs),
                                new Parameter("dataInicial", item.dataIni.Date),
                                new Parameter("dataFinal", item.dataFim.Date))).AsList();

                foreach (var itemTarifario in tarifarios)
                {
                    tarifariosResult.Add(itemTarifario);
                }
            }

            foreach (var itemTarifario in tarifariosResult.OrderBy(c=> c.TrgDtInclusao))
            {

                var tipoTemporadaUtilizar = tarifariosResult
                    .FirstOrDefault(a => a.IdHotel == itemTarifario.IdHotel && a.TrgDtInclusao >= itemTarifario.TrgDtInclusao && a.DataInicial <= itemTarifario.DataInicial &&
                a.DataFinal >= itemTarifario.DataInicial && !string.IsNullOrEmpty(a.TipoPeriodo) &&
                !string.IsNullOrEmpty(a.FlgFeriado) && a.FlgFeriado.Equals("S",StringComparison.InvariantCultureIgnoreCase));

                if (tipoTemporadaUtilizar == null)
                    tipoTemporadaUtilizar = tarifariosResult.FirstOrDefault(a => a.IdHotel == itemTarifario.IdHotel && a.TrgDtInclusao >= itemTarifario.TrgDtInclusao && a.DataInicial <= itemTarifario.DataInicial &&
                    a.DataFinal >= itemTarifario.DataInicial && !string.IsNullOrEmpty(a.TipoPeriodo) &&
                    a.TipoPeriodo.Contains("Alta", StringComparison.InvariantCultureIgnoreCase));

                if (tipoTemporadaUtilizar == null)
                    tipoTemporadaUtilizar = tarifariosResult.FirstOrDefault(a => a.IdHotel == itemTarifario.IdHotel && a.TrgDtInclusao >= itemTarifario.TrgDtInclusao && a.DataInicial <= itemTarifario.DataInicial && a.DataFinal >= itemTarifario.DataFinal &&
                    !string.IsNullOrEmpty(a.TipoPeriodo) && (a.TipoPeriodo.Contains("Média", StringComparison.InvariantCultureIgnoreCase) ||
                    a.TipoPeriodo.Contains("Media", StringComparison.InvariantCultureIgnoreCase)));

                if (tipoTemporadaUtilizar == null)
                    tipoTemporadaUtilizar = tarifariosResult.FirstOrDefault(a => a.IdHotel == itemTarifario.IdHotel && a.TrgDtInclusao >= itemTarifario.TrgDtInclusao && a.DataInicial <= itemTarifario.DataInicial && a.DataFinal >= itemTarifario.DataFinal &&
                    !string.IsNullOrEmpty(a.TipoPeriodo) && (a.TipoPeriodo.Contains("Baixa", StringComparison.InvariantCultureIgnoreCase) ||
                    a.TipoPeriodo.Contains("Baixa", StringComparison.InvariantCultureIgnoreCase)));

                if (tipoTemporadaUtilizar != null)
                    itemTarifario.TipoPeriodo = tipoTemporadaUtilizar.TipoPeriodo ?? itemTarifario.TipoPeriodo;
            }

            return tarifariosResult;

        }

        private static List<(DateTime dataIni, DateTime dataFim)> GetDatasPesquisarTarifarios(DateTime dataInicial, DateTime dataFinal, DayOfWeek dayOfWeekPesquisar)
        {
            List<(DateTime dataIni, DateTime dataFim)> datas = new List<(DateTime dataIni, DateTime dataFim)>();
            if (dataInicial.DayOfWeek == dayOfWeekPesquisar)
            {
                datas.Add((dataInicial, dataInicial.AddDays(7)));
            }
            else
            {
                DateTime dataBase1 = dataInicial.Date;
                while (dataBase1.DayOfWeek != dayOfWeekPesquisar)
                {
                    dataBase1 = dataBase1.AddDays(-1);
                }
                datas.Add((dataBase1, dataBase1.AddDays(7)));
            }

            DateTime dataBase = datas.First().dataIni;
            DateTime dataFim = datas.First().dataFim;

            while (dataFim < dataFinal)
            {
                dataBase = dataFim;
                dataFim = dataBase.AddDays(7);
                datas.Add((dataBase, dataFim));
            }

            return datas;
        }

        private async Task<List<HotelModel>> GetHoteis(int idContratoTs, string? hotelId)
        {
            var result = (await _repository.FindBySql<HotelModel>($@"SELECT P.NOME as HotelNome, H.IDHOTEL as HotelId
              FROM PESSOA P, HOTEL H, CONTRATOTSXHOTEL CH, PARAMHOTEL PH, PARAMTS PTS, HOTEL HP, HOTELTS HTS, CONTRATOTS C
             WHERE P.IDPESSOA      = H.IDHOTEL
               AND H.IDHOTEL       = CH.IDHOTEL
               AND CH.IDCONTRATOTS = C.IDCONTRATOTS
               AND H.IDHOTEL       = PH.IDHOTEL (+)
               AND H.IDHOTEL       = HTS.IDHOTEL (+)
               AND PTS.IDHOTEL     = HP.IDHOTEL
               AND H.ATIVO         = 'S'
               AND CH.IDCONTRATOTS = :idContratoTs
               AND C.IDHOTEL       = 3
             GROUP BY P.NOME, H.IDHOTEL, NVL(PH.DATASISTEMA, PTS.DATASISTEMA), HTS.FLGTIPO, HTS.DIAINICIALSEMANA, H.OBSERVACAO
             ORDER BY P.NOME     ", new Parameter("idContratoTs", idContratoTs))).AsList();

            if (!string.IsNullOrEmpty(hotelId) && int.Parse(hotelId) > 0)
                result = result.Where(a => a.HotelId == int.Parse(hotelId)).AsList();

            return result.AsList();
        }


        public async Task<DadosFinanceirosContratoModel> DadosUtilizacaoContrato(int idVendaXContrato)
        {
            var sb = new StringBuilder(@$"SELECT
                 TO_DATE( TO_CHAR( NVL(R.DATAREVERSAO,V.DATAVENDA),'DD/MM/YYYY' ),'DD/MM/YYYY') DATAVENDA,
                 VC.IDVENDATS, VC.IDVENDAXCONTRATO, VC.FLGREVERTIDO, VC.FLGCANCELADO,
                 CASE WHEN NVL(VC.FLGREVERTIDO,'N') = 'N' THEN 'Não' ELSE 'Sim' END AS REVERTIDO,
                 CASE WHEN NVL(VC.FLGCANCELADO,'N') = 'N' THEN 'Não' ELSE 'Sim' END AS CANCELADO,
                 TO_DATE( TO_CHAR(R.DATAREVERSAO,'DD/MM/YYYY'),'DD/MM/YYYY') DATAREVERSAO,
                 TO_DATE( TO_CHAR(DECODE(VC.FLGREVERTIDO,'N',CC.DATACANCELAMENTO,R2.DATAREVERSAO),'DD/MM/YYYY'),'DD/MM/YYYY') DATACANCELAMENTO,
                 TO_CHAR( NVL(PJ.NUMEROPROJETO,'-1') ) || '-' || TO_CHAR(VC.NUMEROCONTRATO) AS NUMPROJETOCONTRATO,
                 TO_CHAR(VC.NUMEROCONTRATO) as NumeroContrato,
                 C.NOME AS NOMEPRODUTO,
                 P.IDPESSOA,
                 P.NOME AS NOMECLIENTE,
                 CASE WHEN U1.TIPOLANC = 'Débito' THEN U1.NUMEROPONTOS*-1 ELSE U1.NUMEROPONTOS END AS PONTOSBAIXADOSNAOPERACAO,
                 U1.TIPOLANC DEBCRED,
                 TO_DATE(TO_CHAR(U1.DATALANCAMENTO,'DD/MM/YYYY'),'DD/MM/YYYY') DATAOPERACAOLANCAMENTO,
                 U1.IDTIPOLANCPONTOTS,
                 DECODE(NVL(U1.STATUSRESERVA,'Não aplicável'),'Não aplicável', U1.TIPOLANCAMENTO, DECODE(U1.IDTIPOLANCPONTOTS,4,'Reserva',U1.TIPOLANCAMENTO)) DESCRICAOTIPOLANC,
                 U1.DESCRICAO MOTIVOLANCAMENTO,
                 U1.IDRESERVASFRONT, 
                 U1.HOTEL,
                 U1.IDRESERVAMIGRADA, 
                 U1.NUMRESERVAVHF, 
                 U1.LOCALIZADOR,
                 TO_DATE( U1.CHECKIN,'DD/MM/YYYY') CHECKIN,
                 TO_DATE( U1.CHECKOUT,'DD/MM/YYYY') CHECKOUT,
                 NVL(PJ.NUMEROPROJETO,'-1') AS NUMEROPROJETO,
                 NVL(VAL.TOTAL, VC.VALORFINAL) AS VALORCOMPRA,
                 Round(TO_NUMBER( DECODE( NVL(C.VALORPONTO,0),0, DECODE(NVL(C.VALORPERCPONTO,0),0,
                    DECODE( NVL(C.NUMEROPONTOS,0),0,0, NVL(VAL.TOTAL, VC.VALORFINAL) / C.NUMEROPONTOS),
                    (NVL(VAL.TOTAL, VC.VALORFINAL) * C.VALORPERCPONTO)/ 100),
                    NVL(C.VALORPONTO,0))),6) AS VALORPONTO,
                 Round(CASE WHEN U1.TIPOLANC = 'Débito' THEN U1.NUMEROPONTOS*-1 ELSE U1.NUMEROPONTOS END *
                  TO_NUMBER((DECODE(NVL(C.VALORPONTO,0),0, DECODE(NVL(C.VALORPERCPONTO,0),0,
                  DECODE(NVL(C.NUMEROPONTOS,0),0,0, NVL(VAL.TOTAL, VC.VALORFINAL) / C.NUMEROPONTOS),
                  (NVL(VAL.TOTAL, VC.VALORFINAL) * C.VALORPERCPONTO)/ 100), NVL(C.VALORPONTO,0)))),6) AS VALORUTILIZACAO,
                 Round(NVL(NVL(PONTOSANO.UTILIZACAO, U.UTILIZACAO),0),6) AS PONTOSBAIXADOSATUAL,
                 Round(CASE WHEN NVL(VC.FLGCANCELADO,'N') = 'N' THEN
                  C.NUMEROPONTOS - NVL(NVL(PONTOSANO.UTILIZACAO, U.UTILIZACAO),0) + NVL(COMPRADOS.PONTOSCOMPRADOS,0)
                  ELSE 0
                 END,6) AS SALDOPONTOSATUAL,
                 Round(CASE WHEN VC.FLGCANCELADO = 'N' THEN
                  ((C.NUMEROPONTOS - NVL(NVL(PONTOSANO.UTILIZACAO, U.UTILIZACAO),0)) * TO_NUMBER(DECODE(NVL(C.NUMEROPONTOS,0),0,0,NVL(VAL.TOTAL, VC.VALORFINAL) / C.NUMEROPONTOS)))
 	                 ELSE 0
                 END,6) AS VALORSALDOATUAL,
                 NVL(U1.STATUSRESERVA, 'Não aplicável') STATUSRESERVA,
                 NVL(U1.RCI, 'X') RCI,
                 NVL(U1.FRACIONAMENTO, 'Não aplicável') FRACIONAMENTO,
                 NVL(U1.STATUS_BOOK, 'Não aplicável') STATUS_BOOK,
                 TO_DATE( TO_CHAR(DECODE(U1.IDTIPOLANCPONTOTS,4,DECODE(C.FLGGERACREDNUTIL,'S',U1.VALIDADECREDITO,U1.DATALANCAMENTO),''),'DD/MM/YYYY'),'DD/MM/YYYY') AS VALIDADECREDITO,
                 C.DESCONTOANUAL,
                 C.VALIDADE,
                 DECODE(C.TIPOVALIDADE,'A','Ano(s)',DECODE(C.TIPOVALIDADE,'M','Meses','Dias')) AS TIPOVALIDADE,
                 TX.TAXAMANUTENCAO, TX.PAGTOTAXAMANUTENCAO, TX.TRANSFTAXAMANUTENCAO, TX.TAXAADMINISTRATIVA, TX.PAGTOTAXAADMINISTRATIVA,
                 TX.TAXAMANUTENCAO + TX.TAXAADMINISTRATIVA AS TOTALTAXA,
                 TX.PAGTOTAXAMANUTENCAO + TX.TRANSFTAXAMANUTENCAO + TX.PAGTOTAXAADMINISTRATIVA AS TOTALPAGTOTAXA,
                 U1.IDLANCPONTOSTS, TO_DATE(TO_CHAR(U1.TRGDTINCLUSAO, 'DD/MM/YYYY'), 'DD/MM/YYYY') AS DATALANCAMENTOREAL,
                 NVL(COMPRADOS.PONTOSCOMPRADOS, 0) AS PONTOSCOMPRADOS, CASE WHEN TO_DATE(
                              TO_CHAR(
                                  DECODE(U1.IDTIPOLANCPONTOTS, 4,
                                      DECODE(C.FLGGERACREDNUTIL,'S', U1.VALIDADECREDITO, U1.DATALANCAMENTO),
                                      NULL
                                  ), 'DD/MM/YYYY'
                              ), 'DD/MM/YYYY'
                          ) IS NOT NULL THEN
                -- Calcula a data inicial (24 meses atrás + 1 dia)
                TO_CHAR(
                    ADD_MONTHS(
                        TO_DATE(
                            TO_CHAR(
                                DECODE(U1.IDTIPOLANCPONTOTS, 4,
                                    DECODE(C.FLGGERACREDNUTIL,'S', U1.VALIDADECREDITO, U1.DATALANCAMENTO),
                                    NULL
                                ), 'DD/MM/YYYY'
                            ), 'DD/MM/YYYY'
                        ), -24
                    ) + 1, 'DD/MM/YYYY'
                ) || ' a ' ||
                -- Calcula a data final (12 meses atrás)
                TO_CHAR(
                    ADD_MONTHS(
                        TO_DATE(
                            TO_CHAR(
                                DECODE(U1.IDTIPOLANCPONTOTS, 4,
                                    DECODE(C.FLGGERACREDNUTIL,'S', U1.VALIDADECREDITO, U1.DATALANCAMENTO),
                                    NULL
                                ), 'DD/MM/YYYY'
                            ), 'DD/MM/YYYY'
                        ), -12
                    ), 'DD/MM/YYYY'
                )
            END AS DATADEBITOPERIODO
                 FROM
                 VENDAXCONTRATOTS VC,
                 VENDATS V,
                 ATENDCLIENTETS A,
                 PESSOA P,
                 PROJETOTS PJ,
                 CONTRATOTS C,
                 REVCONTRATOTS R,
                 CANCCONTRATOTS CC,
                 REVCONTRATOTS R2,
                 HOTEL H,
                 (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'D',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS UTILIZACAO
                  FROM LANCPONTOSTS LP, RESERVASFRONT RF, RESERVAMIGRADATS RM
                  WHERE NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS = 1)
                  AND LP.IDRESERVASFRONT = RF.IDRESERVASFRONT (+)
                  AND LP.IDRESERVAMIGRADA = RM.IDRESERVAMIGRADA (+)
                  AND LP.IDTIPOLANCPONTOTS <> 8
                  GROUP BY IDVENDAXCONTRATO) U,
                 (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'D', LPD.NUMEROPONTOS, -LPD.NUMEROPONTOS)),0) AS UTILIZACAO
                  FROM LANCPONTOSTS LP
                  JOIN PARAMTS PAR ON LP.IDHOTEL = PAR.IDHOTEL
                  JOIN LANCPONTOSDIATS LPD ON LP.IDLANCPONTOSTS = LPD.IDLANCPONTOSTS
                  WHERE PAR.FLGCALCULARSALDOPORANO = 'S'
                  AND LP.IDTIPOLANCPONTOTS <> 8
                  AND TO_CHAR(LPD.DATALANCAMENTO,'YYYY') = TO_CHAR(PAR.DATASISTEMA,'YYYY')
                  AND NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS = 1)
                  GROUP BY IDVENDAXCONTRATO
                  ) PONTOSANO,
                 (SELECT LP.IDVENDAXCONTRATO, NVL(SUM(DECODE(LP.DEBITOCREDITO,'C',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS PONTOSCOMPRADOS
                  FROM LANCPONTOSTS LP, RESERVASFRONT RF, RESERVAMIGRADATS RM
                  WHERE NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS = 1)
                  AND LP.IDRESERVASFRONT = RF.IDRESERVASFRONT (+)
                  AND LP.IDRESERVAMIGRADA = RM.IDRESERVAMIGRADA (+)
                  AND (SELECT SUM(VLRLANCAMENTO) AS SALDO FROM LANCAMENTOTS WHERE IDLANCPONTOSTS = LP.IDLANCPONTOSTS GROUP BY IDLANCPONTOSTS) = 0
                  AND LP.IDTIPOLANCPONTOTS = 8
                  GROUP BY IDVENDAXCONTRATO) COMPRADOS,
                 (SELECT LP.IDLANCPONTOSTS, NVL(SUM(DECODE(LP.DEBITOCREDITO,'D',LP.NUMEROPONTOS,-LP.NUMEROPONTOS)),0) AS UTILIZACAO,
                  SUM(DECODE(L.IDTIPOLANCAMENTO,5,L.VLRLANCAMENTO,0)) AS TAXAMANUTENCAO,
                  SUM(DECODE(L.IDTIPOLANCAMENTO,6,L.VLRLANCAMENTO,0)) AS PAGTOTAXAMANUTENCAO,
                  SUM(DECODE(L.IDTIPOLANCAMENTO,13,L.VLRLANCAMENTO,0)) AS TRANSFTAXAMANUTENCAO,
                  SUM(DECODE(L.IDTIPOLANCAMENTO,14,L.VLRLANCAMENTO,0)) AS TAXAADMINISTRATIVA,
                  SUM(DECODE(L.IDTIPOLANCAMENTO,15,L.VLRLANCAMENTO,0)) AS PAGTOTAXAADMINISTRATIVA
                  FROM LANCPONTOSTS LP, LANCAMENTOTS L
                  WHERE NOT EXISTS(SELECT R.IDRESERVASFRONT FROM RESERVASFRONT R WHERE R.IDRESERVASFRONT = LP.IDRESERVASFRONT AND R.STATUSRESERVA = 6 AND LP.IDTIPOLANCPONTOTS = 1)
                  AND LP.IDLANCPONTOSTS = L.IDLANCPONTOSTS (+)
                  GROUP BY LP.IDLANCPONTOSTS) TX,
                 (SELECT L.IDVENDAXCONTRATO, L.IDLANCPONTOSTS,
                  DECODE(L.DEBITOCREDITO,'D','Débito','C','Crédito') TIPOLANC,
                  L.NUMEROPONTOS,
                  L.DATALANCAMENTO,
                  L.IDTIPOLANCPONTOTS,
                  T.DESCRICAO TIPOLANCAMENTO,
                  M.DESCRICAO,
                  L.IDRESERVASFRONT,
                  L.IDRESERVAMIGRADA,
                  RM.LOCRESERVA LOCALIZADOR,
                  RM.NUMRESERVA NUMRESERVAVHF,
                  P.NOME HOTEL,
                  TO_CHAR(RM.DATACHEGADA,'DD/MM/YYYY') CHECKIN,
                  TO_CHAR(RM.DATAPARTIDA,'DD/MM/YYYY') CHECKOUT,
                  'Check-out' STATUSRESERVA,
                  DECODE(RCI.IDRESERVASRCI, NULL, 'N', 'S') RCI,
                  'Não aplicável' FRACIONAMENTO,
                  'Não aplicável' AS STATUS_BOOK,
                  L.VALIDADECREDITO,
                  L.TRGDTINCLUSAO
                  FROM LANCPONTOSTS L, TIPOLANCPONTOTS T, MOTIVOTS M, RESERVAMIGRADATS RM, PESSOA P, RESERVASRCI RCI
                  WHERE
                  RCI.IDRESERVAMIGRADA (+) = RM.IDRESERVAMIGRADA
                  AND L.IDRESERVAMIGRADA (+) = RM.IDRESERVAMIGRADA
                  AND RM.IDHOTEL (+)  = P.IDPESSOA
                  AND L.IDTIPOLANCPONTOTS = T.IDTIPOLANCPONTOTS
                  AND L.IDMOTIVO  = M.IDMOTIVOTS (+)
                  AND L.IDRESERVASFRONT IS NULL
                  AND L.FLGMIGRADO  = 'S'
                 UNION
                 (SELECT L.IDVENDAXCONTRATO, L.IDLANCPONTOSTS,
                  DECODE(L.DEBITOCREDITO,'D','Débito','C','Crédito') TIPOLANC,
                  TO_NUMBER(DECODE(L.DEBITOCREDITO, 'C',-L.NUMEROPONTOS,L.NUMEROPONTOS)) *
                  TO_NUMBER(DECODE(L.IDTIPOLANCPONTOTS, 1, DECODE(R.STATUSRESERVA, 6, 0, 1), 1)) AS NUMEROPONTOS,
                  L.DATALANCAMENTO,
                  L.IDTIPOLANCPONTOTS,
                  T.DESCRICAO TIPOLANCAMENTO,
                  M.DESCRICAO,
                  L.IDRESERVASFRONT,
                  L.IDRESERVAMIGRADA,
                  R.LOCRESERVA LOCALIZADOR,
                  R.NUMRESERVA NUMRESERVAVHF,
                  P.NOME HOTEL,
                  TO_CHAR(R.DATACHEGPREVISTA,'DD/MM/YYYY') CHECKIN,
                  TO_CHAR(R.DATAPARTPREVISTA,'DD/MM/YYYY') CHECKOUT,
                  S.DESCRICAO STATUSRESERVA,
                  DECODE(RCI.IDRESERVASRCI, NULL, 'N', 'S') RCI,
                  DECODE(FRAC1.IDFRACIONAMENTOTS, NULL, DECODE(FRAC2.IDFRACIONAMENTOTS, NULL, 'Não', 'Fechamento'), 'Início') FRACIONAMENTO,
                  'Não aplicável' AS STATUS_BOOK,
                  L.VALIDADECREDITO,
                  L.TRGDTINCLUSAO
                  FROM LANCPONTOSTS L, TIPOLANCPONTOTS T, MOTIVOTS M, RESERVASFRONT R, PESSOA P, STATUSRESERVA S, RESERVASRCI RCI,
                  FRACIONAMENTOTS FRAC1, FRACIONAMENTOTS FRAC2
                  WHERE L.IDTIPOLANCPONTOTS = T.IDTIPOLANCPONTOTS
                  AND L.IDRESERVASFRONT IS NOT NULL
                  AND L.IDRESERVAMIGRADA IS NULL
                  AND R.IDRESERVASFRONT (+) = L.IDRESERVASFRONT
                  AND L.IDMOTIVO   = M.IDMOTIVOTS (+)
                  AND R.IDHOTEL   = P.IDPESSOA (+)
                  AND R.IDRESERVASFRONT  = RCI.IDRESERVASFRONT (+)
                  AND R.IDRESERVASFRONT  = FRAC1.IDRESERVASFRONT1 (+)
                  AND R.IDRESERVASFRONT  = FRAC2.IDRESERVASFRONT2 (+)
                  AND R.STATUSRESERVA  = S.STATUSRESERVA )
                 UNION
                 (SELECT L.IDVENDAXCONTRATO, L.IDLANCPONTOSTS,
                  DECODE(L.DEBITOCREDITO,'D','Débito','C','Crédito') TIPOLANC,
                  L.NUMEROPONTOS,
                  L.DATALANCAMENTO,
                  L.IDTIPOLANCPONTOTS,
                  T.DESCRICAO TIPOLANCAMENTO,
                  M.DESCRICAO,
                  L.IDRESERVASFRONT,
                  L.IDRESERVAMIGRADA,
                  L.IDRESERVAMIGRADA LOCALIZADOR,
                  L.IDRESERVAMIGRADA NUMRESERVAVHF,
                  TO_CHAR(L.IDRESERVAMIGRADA) HOTEL,
                  TO_CHAR(L.DATALANCAMENTO,'DD/MM/YYYY') CHECKIN,
                  TO_CHAR(L.DATALANCAMENTO,'DD/MM/YYYY') CHECKOUT,
                  'Não aplicável' STATUSRESERVA,
                  'X' RCI,
                  'Não aplicável' FRACIONAMENTO,
                  'Não aplicável' AS STATUS_BOOK,
                  L.VALIDADECREDITO,
                  L.TRGDTINCLUSAO
                  FROM LANCPONTOSTS L, TIPOLANCPONTOTS T, MOTIVOTS M, RESERVASFRONT R, RESERVAMIGRADATS RM
                  WHERE L.IDTIPOLANCPONTOTS = T.IDTIPOLANCPONTOTS
                  AND L.IDMOTIVO  = M.IDMOTIVOTS (+)
                  AND L.IDRESERVASFRONT = R.IDRESERVASFRONT (+)
                  AND L.IDRESERVASFRONT = RM.IDRESERVAMIGRADA (+)
                  AND R.IDRESERVASFRONT IS NULL
                  AND L.IDRESERVAMIGRADA IS NULL
                  AND L.IDTIPOLANCPONTOTS <> 8
                  AND L.IDTIPOLANCPONTOTS != 1)
                  UNION
                 (SELECT L.IDVENDAXCONTRATO, L.IDLANCPONTOSTS,
                  DECODE(L.DEBITOCREDITO,'D','Débito','C','Crédito') TIPOLANC,
                  L.NUMEROPONTOS,
                  L.DATALANCAMENTO,
                  L.IDTIPOLANCPONTOTS,
                  T.DESCRICAO TIPOLANCAMENTO,
                  M.DESCRICAO,
                  L.IDRESERVASFRONT,
                  L.IDRESERVAMIGRADA,
                  L.IDRESERVAMIGRADA LOCALIZADOR,
                  L.IDRESERVAMIGRADA NUMRESERVAVHF,
                  TO_CHAR(L.IDRESERVAMIGRADA) HOTEL,
                  TO_CHAR(L.DATALANCAMENTO,'DD/MM/YYYY') CHECKIN,
                  TO_CHAR(L.DATALANCAMENTO,'DD/MM/YYYY') CHECKOUT,
                  BOOK.STATUS_BOOK AS STATUSRESERVA,
                  'X' RCI,
                  'Não aplicável' FRACIONAMENTO,
                  BOOK.STATUS_BOOK,
                  L.VALIDADECREDITO,
                  L.TRGDTINCLUSAO
                  FROM LANCPONTOSTS L
                  JOIN TIPOLANCPONTOTS T ON L.IDTIPOLANCPONTOTS = T.IDTIPOLANCPONTOTS
                  JOIN(SELECT C.IDCOMPRASBOOKOFERTASTS, C.IDLANCPONTOSTS, C.IDMOTIVOTS, C.DATACOMPRA, C.QUANTIDADE, ST.DESCRICAO AS STATUS_BOOK,
                   CASE WHEN C.IDSTATUS = 3 THEN 'Compra cancelada'
                   ELSE (SELECT CASE WHEN SUM(VLRLANCAMENTO) = 0 THEN 'Pago' ELSE ST.DESCRICAO END FROM LANCAMENTOTS WHERE IDLANCPONTOSTS = C.IDLANCPONTOSTS)
                   END AS STATUS
                  FROM COMPRASBOOKOFERTASTS C
                  JOIN STATUSBOOKOFERTASTS ST ON C.IDSTATUS = ST.IDSTATUSBOOKOFERTASTS
                  WHERE C.IDSTATUS IN (3, 4, 5)
                  ) BOOK ON L.IDLANCPONTOSTS = BOOK.IDLANCPONTOSTS
                  JOIN MOTIVOTS M ON BOOK.IDMOTIVOTS = M.IDMOTIVOTS
                 WHERE L.IDTIPOLANCPONTOTS IN (2, 8)
                  AND L.IDTIPOLANCPONTOTS != 1)
                  ) U1,
                 (SELECT VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS) AS IDREVCONTRATOTS, ABS(SUM(L.VLRLANCAMENTO)) AS TOTAL
                  FROM LANCAMENTOTS L, VENDATS V, VENDAXCONTRATOTS VC, CONTRATOTS C, AJUSTEFINANCTS AJ, REVCONTRATOTS R
                  WHERE L.IDVENDATS = V.IDVENDATS
                   AND V.IDVENDATS = VC.IDVENDATS
                   AND VC.IDCONTRATOTS = C.IDCONTRATOTS
                   AND L.IDAJUSTEFINANCTS = AJ.IDAJUSTEFINANCTS (+)
                   AND AJ.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                   AND (L.IDTIPOLANCAMENTO IN (1,7) OR L.IDTIPODEBCRED = C.IDTIPODCJUROS OR L.IDTIPODEBCRED = C.IDTIPODCCONTRATO)
                   AND ((VC.FLGCANCELADO = 'N' AND VC.FLGREVERTIDO = 'N'  AND L.IDMOTIVOESTORNO IS NULL AND L.IDLANCESTORNO IS NULL)
                   OR (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NULL AND L.IDCANCCONTRATOTS IS NULL)
                   OR (VC.FLGREVERTIDO = 'S' AND L.IDMOTIVOESTORNO IS NOT NULL AND L.IDCANCCONTRATOTS IS NULL AND L.IDAJUSTEFINANCTS IS NOT NULL )
                   OR (VC.FLGCANCELADO = 'S' AND L.IDMOTIVOESTORNO IS NULL AND L.IDCANCCONTRATOTS IS NULL ))
                   AND L.IDVENDATS IS NOT NULL
                  GROUP BY VC.IDVENDAXCONTRATO, NVL(L.IDREVCONTRATOTS, R.IDREVCONTRATOTS)) VAL
                 WHERE VC.IDVENDATS = V.IDVENDATS
                 AND A.IDATENDCLIENTETS = V.IDATENDCLIENTETS
                 AND P.IDPESSOA  = A.IDCLIENTE
                 AND C.IDCONTRATOTS = VC.IDCONTRATOTS
                 AND A.IDHOTEL  = H.IDHOTEL
                 AND VC.IDVENDAXCONTRATO = U1.IDVENDAXCONTRATO (+)
                 AND VC.IDVENDAXCONTRATO = PONTOSANO.IDVENDAXCONTRATO (+)
                 AND U1.IDLANCPONTOSTS = TX.IDLANCPONTOSTS (+)
                 AND VC.IDVENDAXCONTRATO = U.IDVENDAXCONTRATO (+)
                 AND VC.IDVENDAXCONTRATO = COMPRADOS.IDVENDAXCONTRATO (+)
                 AND VC.IDVENDAXCONTRATO = VAL.IDVENDAXCONTRATO (+)
                 AND VC.IDPROJETOTS = PJ.IDPROJETOTS (+)
                 AND VC.IDVENDAXCONTRATO = R.IDVENDAXCONTRNOVO (+)
                 AND VC.IDVENDAXCONTRATO = R2.IDVENDAXCONTRANT (+)
                 AND VC.IDVENDAXCONTRATO = CC.IDVENDAXCONTRATO (+)
                 AND ((R.IDREVCONTRATOTS IS NULL AND VAL.IDREVCONTRATOTS IS NULL) OR (R.IDREVCONTRATOTS = VAL.IDREVCONTRATOTS))
                 AND ((VC.PREVENDA = 'N') OR (VC.PREVENDA IS NULL))
                 AND H.IDPESSOA  = 3
                 AND VC.IDVENDAXCONTRATO = :idVendaXContrato
                 ORDER BY VC.IDVENDATS, VC.IDVENDAXCONTRATO, U1.DATALANCAMENTO");

            var parametros = new Parameter("idVendaXContrato", idVendaXContrato);

            var result = (await _repository.FindBySql<DadosFinanceiroContratoDto>(sb.ToString(), parametros)).AsList();

            DadosFinanceirosContratoModel? dadosRetorno = new DadosFinanceirosContratoModel();

            if (result != null)
            {
                foreach (var item in result.GroupBy(a=> a.IdVendaXContrato))
                {
                    var fst = item.First();
                    dadosRetorno.IdVendaXContrato = fst.IdVendaXContrato;
                    dadosRetorno.NumeroContrato = fst.NumProjetoContrato;
                    dadosRetorno.Produto = fst.NomeProduto;
                    dadosRetorno.ValorCompra = fst.ValorCompra;
                    dadosRetorno.ValorSaldoAtual = fst.ValorSaldoAtual;
                    dadosRetorno.ValorDoPonto = fst.ValorPonto;
                    dadosRetorno.PontosComprados = fst.PontosComprados;
                    dadosRetorno.PontosUtilizados = fst.PontosBaixadosAtual;
                    dadosRetorno.SaldoDePontos = fst.SaldoPontosAtual;
                    dadosRetorno.UtilizacoesItens = new List<UtilizacoesItensModel>();

                    foreach (var itemUtilizacao in item.GroupBy(c=> c.IdLancPontosTs))
                    {
                        foreach (var lancFst in itemUtilizacao.OrderBy(a=> a.IdLancPontosTs))
                        {
                            var cancelado = itemUtilizacao.Any(c => c.IdReservasFront == lancFst.IdReservasFront && c.IdTipoLancPontoTs == 5 && lancFst.IdTipoLancPontoTs == 1);
                            if (cancelado)
                            {
                                lancFst.PontosBaixadosNaOperacao = 0;
                            }

                            var utilizacaoItem = new UtilizacoesItensModel
                            {
                                DataOperacao = lancFst.DataOperacaoLancamento,
                                TipoLancamento = lancFst.DescricaoTipoLanc,
                                Motivo = lancFst.MotivoLancamento,
                                Tipo = lancFst.DescricaoTipoLanc,
                                ValorUtilizacao = lancFst.ValorUtilizacao,
                                Pontos = lancFst.PontosBaixadosNaOperacao,
                                Rci = lancFst.Rci,
                                Fracionamento = lancFst.Fracionamento,
                                Status = lancFst.StatusReserva,
                                Checkin = lancFst.Checkin,
                                Checkout = lancFst.Checkout,
                                DebitoCredito = lancFst.DebCred,
                                DataDebitoPeriodo = lancFst.DataDebitoPeriodo,
                                IdLancPontosTs = lancFst.IdLancPontosTs,
                                TotalTaxa = lancFst.TotalTaxa,
                                TotalPagtoTaxa = lancFst.TotalPagtoTaxa,
                                Reserva = lancFst.NumReservaVhf,
                                Hotel = lancFst.Hotel
                            };

                            if (utilizacaoItem.Pontos.GetValueOrDefault(0) != 0)
                                dadosRetorno.UtilizacoesItens.Add(utilizacaoItem);
                        }
                    }

                    var reservaTimeSharingTemp = (await _repositorySystem.FindBySql<ReservaTimeSharing>(@$"Select 
                                                                                                * 
                                                                                               From 
                                                                                                ReservaTimeSharing 
                                                                                               Where 
                                                                                                Upper(StatusCM) = 'PENDENTE' and 
                                                                                                IdReservasFront is null and 
                                                                                                TipoUtilizacao like 'RCI%INTER%' and 
                                                                                                ClienteReservante = {fst.IdPessoa} and 
                                                                                                NumeroContrato like '%{fst.NumProjetoContrato}%'")).AsList();
                    foreach (var utilizacaoTemp in reservaTimeSharingTemp)
                    {
                        var utilizacaoItem = new UtilizacoesItensModel
                        {
                            DataOperacao = utilizacaoTemp.DataHoraCriacao ?? utilizacaoTemp.DataHoraAlteracao,
                            TipoLancamento = "Reserva",
                            Tipo = "Reserva",
                            Pontos = Math.Abs(utilizacaoTemp.PontosUtilizados.GetValueOrDefault())*(-1),
                            Rci = "S",
                            Status = "Confirmada",
                            Checkin = utilizacaoTemp.Checkin,
                            Checkout = utilizacaoTemp.Checkout,
                            DebitoCredito = "Débito",
                            Reserva = "Não aplicável"
                        };

                        if (utilizacaoItem.Pontos.GetValueOrDefault(0) != 0)
                        {
                            dadosRetorno.UtilizacoesItens.Add(utilizacaoItem);
                            dadosRetorno.PontosUtilizados += Math.Abs(utilizacaoItem.Pontos.GetValueOrDefault());
                            dadosRetorno.SaldoDePontos-= Math.Abs(utilizacaoItem.Pontos.GetValueOrDefault());
                        }
                    }
                }
            }

            return dadosRetorno!;
        }

        private async Task<PeriodoDisponivelResultModel?> GetSaldo(SearchDisponibilidadeModel searchModel)
        {
            var numeroContratoUtilizar = !string.IsNullOrEmpty(searchModel.NumeroContrato) && searchModel.NumeroContrato.Contains('-') ? searchModel.NumeroContrato.Split('-')[1] : searchModel.NumeroContrato;

            var retornoBase = (await _repository.FindBySql<PeriodoDisponivelResultModel>(@$"SELECT VC.IdVendaTs,  VC.IDVENDAXCONTRATO,
                             P.NUMEROPROJETO ||'-'|| TO_CHAR(TO_NUMBER(VC.NUMEROCONTRATO)) AS NUMEROCONTRATO,
                             C.IDCONTRATOTS,
                             C.NOME,
                             C.NUMEROPONTOS - NVL(Sum( TO_NUMBER(DECODE(L.DEBITOCREDITO, 'C',-L.NUMEROPONTOS,L.NUMEROPONTOS)) *
                                                       TO_NUMBER(DECODE(L.IDTIPOLANCPONTOTS, 1, DECODE(R.STATUSRESERVA, 6, 0, 1), 1))), 0) AS SaldoPontos,
                             NVL(CRED.CREDITO, 0) AS CREDITO
                     FROM     CM.ATENDCLIENTETS  A, CM.VENDATS V, CM.VENDAXCONTRATOTS VC,
                             CM.CONTRATOTS C, CM.LANCPONTOSTS L, CM.PROJETOTS P, CM.RESERVASFRONT R, CM.AGENCIATS,
                             (SELECT VC.IDVENDAXCONTRATO, Sum(LP.NUMEROPONTOS) AS CREDITO
                              FROM   CM.ATENDCLIENTETS A, CM.VENDATS V, CM.VENDAXCONTRATOTS VC, CM.LANCPONTOSTS LP, CM.PARAMTS P
                              WHERE  
                              VC.IDVENDAXCONTRATO = :idVendaXContrato
                              AND    VC.NumeroContrato = :numeroContrato
                              AND    V.IDATENDCLIENTETS = A.IDATENDCLIENTETS
                              AND    VC.IDVENDATS = V.IDVENDATS
                              AND    LP.IDVENDAXCONTRATO   = VC.IDVENDAXCONTRATO
                              AND    P.IDHOTEL = A.IDHOTEL
                              AND    LP.VALIDADECREDITO   >= P.DATASISTEMA
                              AND    LP.DATALANCAMENTO    <= P.DATASISTEMA
                              AND    LP.VALIDADECREDITO    IS NOT NULL
                              AND    LP.DEBITOCREDITO      = 'D'
                              AND    LP.IDTIPOLANCPONTOTS  = 4
                              AND    LP.IDRESERVASFRONT    IS NULL
                              AND    LP.IDRESERVAMIGRADA   IS NULL
                              GROUP BY VC.IDVENDAXCONTRATO) CRED
                    WHERE    
                    V.IDATENDCLIENTETS       = A.IDATENDCLIENTETS
                    AND      VC.IDVENDATS             = V.IDVENDATS
                    AND      VC.IDVENDAXCONTRATO = :idVendaXContrato
                    AND      VC.NUMEROCONTRATO = :numeroContrato
                    AND      C.IDCONTRATOTS           = VC.IDCONTRATOTS
                    AND      L.IDRESERVASFRONT        = R.IDRESERVASFRONT (+)
                    AND      L.IDVENDAXCONTRATO (+)   = VC.IDVENDAXCONTRATO
                    AND      VC.IDVENDAXCONTRATO      = CRED.IDVENDAXCONTRATO(+)
                    AND      VC.IDAGENCIATS           = AGENCIATS.IDAGENCIATS
                    AND      VC.FLGCANCELADO          = 'N'
                    AND      VC.FLGREVERTIDO          = 'N'
                    AND      VC.IDPROJETOTS           = P.IDPROJETOTS
                    AND      ((VC.PREVENDA = 'N') OR (VC.PREVENDA IS NULL))
                    AND      AGENCIATS.IDPESSOA       = 3
                    GROUP BY VC.IDVENDATS, 
                             VC.IDVENDAXCONTRATO, 
                             VC.NUMEROCONTRATO, 
                             P.NUMEROPROJETO, 
                             C.IDCONTRATOTS, 
                             C.NOME, 
                             L.IDVENDAXCONTRATO, 
                             C.NUMEROPONTOS, 
                             CRED.CREDITO",
                new Parameter("idVendaXContrato", searchModel.IdVendaXContrato!),
                new Parameter("numeroContrato", numeroContratoUtilizar!))).FirstOrDefault();

            if (retornoBase != null && !string.IsNullOrEmpty(retornoBase.NumeroContrato))
            {
                var reservasTimeSharingTemp = (await _repositorySystem.FindBySql<ReservaTimeSharing>(@$"Select 
                                * 
                            From 
                                ReservaTimeSharing 
                            Where 
                                NumeroContrato = '{retornoBase.NumeroContrato}' and 
                                Upper(TipoUtilizacao) like '%RCI%INTERCAM%' and 
                                Upper(StatusCM) = 'PENDENTE' ")).AsList();

                if (reservasTimeSharingTemp != null && reservasTimeSharingTemp.Any())
                    retornoBase.SaldoPontos -= Math.Abs(reservasTimeSharingTemp.Sum(a => a.PontosUtilizados.GetValueOrDefault()));
            }

            return retornoBase;
        }

        private async Task<List<DisponibilidadeDoHotel>?> GetDisponibilidadeExecute(List<int> hoteisPesquisar, List<int> tiposAptosPesquisar, DateTime dataInicial, DateTime dataFinal, decimal? ocupacaoMaxima = 70)
        {
            if (hoteisPesquisar.Count <= 0 || tiposAptosPesquisar.Count <= 0) return null;

            List<DisponibilidadeDoHotel> listResult = new List<DisponibilidadeDoHotel>();

            var strDisponibilidadeFixa = "Select " +
                "IdHotel," +
                "IdTipoUh," +
                "DataIni as Checkin, " +
                "DataFim as Checkout, " +
                "QtdaDisp, " +
                "PercDisp, " +
                "FlgPerc " +
                "From " +
                "DISPTIPOUH " +
                "Where " +
                $"1 = 1 AND IdTipoUh in ({string.Join(",", tiposAptosPesquisar)}) AND " +
            " (DataIni between :dataInicial and :dataFinal) ";



            string hqlBloqueios1 = $@"select 
                                            DATA, 
                                            U.IDHOTEL, 
                                            U.IDTIPOUH, 
                                            SUM ( 1 ) AS QTDBLOQ
                                            from 
                                            VWUHPOOL U, 
                                            BLOQUEIOUH B, 
                                            TIPOUH T  
                                            WHERE 
                                            ( U.DATA BETWEEN :dataInicial AND :dataFinal )  AND  
                                            ( U.IDHOTEL in  ({string.Join(",", hoteisPesquisar)}) )  AND   
                                            ( T.FLGAPARECENADISP<>'N' )  AND   
                                            ( B.IDHOTEL=U.IDHOTEL )  AND   
                                            ( B.DATAINICIO<=U.DATA )  AND   
                                            ( B.DATAFIM>=U.DATA )  AND   
                                            ( U.CODUH=B.CODUH )  AND   
                                            ( U.IDHOTEL=B.IDHOTEL )  AND   
                                            ( T.IDTIPOUH=U.IDTIPOUH )  AND   
                                            ( T.IDHOTEL=U.IDHOTEL ) AND 
                                            ( T.idtipouh in ({string.Join(",", tiposAptosPesquisar)}))
                                            Group by 
                                            DATA, 
                                            U.IDHOTEL, 
                                            U.IDTIPOUH
                                             ORDER BY
                                             U.IDHOTEL, U.DATA, U.IDTIPOUH ";


            string tiposDeAtpto =
                    $@"Select 
                        D.IdHotel,
                        'MA' AS CODREDUZIDO,
                        D.IDTIPOUH,
                        Count(u.CODUH) as Qtde,
                        'MASTER' AS DESCRICAO,
                        D.QtdMaxPessoas as Capacidade
                        From
                        Uh u,
                        TipoUh D
                        Where
                        u.IdTipoUh = D.IdTipoUh and
                        u.uhpool = 'S' AND
                        u.FlgAtiva = 'S' and
                        Coalesce(u.FLGPOOLFLUT,'N') = 'N' and
                        D.IdHotel in ({string.Join(",", hoteisPesquisar)}) and
                        ( D.idtipouh in ({string.Join(",", tiposAptosPesquisar)})) and
                        D.QtdReal > 0
                        Group by D.QtdMaxPessoas, D.IDHOTEL, D.CODREDUZIDO,D.IDTIPOUH, D.DESCRICAO
                        Having Count(u.CODUH) > 0 
            
                        Union All
   
                        Select 
                        D.IdHotel,
                        'MA' AS CODREDUZIDO,
                        D.IDTIPOUH,
                        0 as Qtde,
                        'MASTER' AS DESCRICAO,
                        D.QtdMaxPessoas as Capacidade
                        From
                        Uh u,
                        TipoUh D
                        Where
                        u.IdTipoUh = D.IdTipoUh and
                        Coalesce(u.FLGPOOLFLUT,'N') = 'S' AND
                        u.FlgAtiva = 'S' and
                        D.QtdReal > 0 and
                        D.IdHotel in ({string.Join(",", hoteisPesquisar)}) AND
                        D.IdtipoUh in ({string.Join(",", tiposAptosPesquisar)})
                        Group by D.QtdMaxPessoas, D.IDHOTEL, D.CODREDUZIDO,D.IDTIPOUH, D.DESCRICAO";



            string consumoDisponibilidadePorReservas =
                $@"SELECT D.DATA, R.IDHOTEL, T.CODREDUZIDO, T.IDTIPOUH, T.QTDREAL, 
                    SUM(CAST((CASE WHEN R.STATUSRESERVA = 7 THEN 0 ELSE 1 END) AS FLOAT) ) AS TOTOCUPADA,
                        SUM (  CAST((CASE WHEN R.STATUSRESERVA = 7 THEN 1 ELSE 0 END) AS FLOAT ) ) AS TOTWAITLIST,
                        SUM (  CAST((CASE WHEN R.STATUSRESERVA = 0 THEN 1 ELSE 0 END) AS FLOAT ) ) AS TOTACONFIRMAR,
                        SUM (  CAST((CASE WHEN R.STATUSRESERVA = 1 THEN 1 ELSE 0 END) AS FLOAT ) ) AS TOTCONFIRMADA,
                        SUM (  CAST((CASE WHEN R.STATUSRESERVA = 7 THEN 0 ELSE(CASE WHEN SIGN(CAST(DATA - (CASE WHEN A.DATACUTOFF - 1 IS NULL THEN DATA ELSE A.DATACUTOFF - 1 END) AS FLOAT)) = 1
                    THEN 1 ELSE 0 END) END ) AS FLOAT ) ) AS QTDALLOTMENTS, SUM ( CASE WHEN R.TIPOUHESTADIA = A.IDTIPOUH THEN 1 ELSE 0 END ) AS QTDALLOTREAL  FROM
                        RESERVASFRONT R LEFT  OUTER JOIN ALLOTMENTXTIPOUH A ON(R.IDALLOTXTIPOUH = A.IDALLOTXTIPOUH),  TIPOUH T, DATASIS   D,   RESERVAREDUZ RD   WHERE
                        (D.DATA >= :dataInicial)   AND (D.DATA <= :dataFinal)   AND
                        (T.IdHotel in ({string.Join(",", hoteisPesquisar)}))
                        AND (R.POOLLISTA = 'N')   AND(R.IDROOMLIST IS NULL)   AND(RD.DATACHEGADA <= D.DATA)   AND
                        (RD.DATAPARTIDA > D.DATA)   AND(RD.IDHOTEL = D.IDHOTEL)   AND(R.IDRESERVASFRONT = RD.IDRESERVASFRONT)   AND
                        (T.IDTIPOUH = R.TIPOUHESTADIA)   AND(T.IDHOTEL = R.IDHOTEL)   AND(T.FLGAPARECENADISP <> 'N') AND 
                        (T.IDTIPOUH in ({string.Join(",", tiposAptosPesquisar)}))
                    GROUP BY   D.DATA,   R.IDHOTEL,   T.CODREDUZIDO,   T.IDTIPOUH, T.QTDREAL ORDER   BY R.IDHOTEL,  D.DATA ";


            var disponibilidadesFixas = (await _repository.FindBySql<DisponibilidadeFixaModel>(strDisponibilidadeFixa, new Parameter("dataInicial", dataInicial), new Parameter("dataFinal", dataFinal))).AsList();


            var retReservas =
                (await _repository.FindBySql<ConsumoDisponibilidadeReservas>(consumoDisponibilidadePorReservas, new Parameter("dataInicial", dataInicial), new Parameter("dataFinal", dataFinal))).AsList();



            string consumoDisponibilidadePorGrupos = $@"SELECT DATA, D.IDHOTEL, T.CODREDUZIDO, T.IDTIPOUH, T.QTDREAL, 
                SUM (  CAST (( CASE WHEN RG.STATUSRESERVA = 7 THEN 0 ELSE A.QTDUH END ) AS FLOAT ) ) AS TOTOCUPADA,  
                SUM (  CAST (( CASE WHEN RG.STATUSRESERVA = 7 THEN A.QTDUH ELSE 0 END ) AS FLOAT ) ) AS TOTWAITLIST,  
                SUM (  CAST (( CASE WHEN RG.STATUSRESERVA = 0 THEN A.QTDUH ELSE 0 END ) AS FLOAT ) ) AS TOTACONFIRMAR,  
                SUM (  CAST (( CASE WHEN RG.STATUSRESERVA = 1 THEN A.QTDUH ELSE 0 END ) AS FLOAT ) ) AS TOTCONFIRMADA,  
                T.CODREDUZIDO, T.IDTIPOUH  
                FROM
                    TIPOUH T, DATASIS D, ACOMODACAO A, RESERVAGRUPO RG  WHERE
                    ( D.DATA>= :dataInicial)  AND   ( D.DATA<= :dataFinal )  AND   
                    (T.IdHotel in ({string.Join(",", hoteisPesquisar)}))
                    AND ( RG.STATUSRESERVA<=2 OR RG.STATUSRESERVA=7 )  AND   ( A.IDHOTEL=D.IDHOTEL )  AND   
                    ( A.DATACHEGADA<=DATA )  AND   ( A.DATAPARTIDA>DATA )  AND   ( T.FLGAPARECENADISP<>'N' )  AND   ( RG.IDRESERVAGRUPO=A.IDRESERVAGRUPO )  AND   
                    ( T.IDTIPOUH=A.IDTIPOUH )  AND   ( T.IDHOTEL=A.IDHOTEL )  AND
                    (T.IDTIPOUH in ({string.Join(",", tiposAptosPesquisar)}))
                    GROUP BY
                    DATA, D.IDHOTEL, T.CODREDUZIDO, T.IDTIPOUH, T.QTDREAL   ORDER BY
                    D.IDHOTEL, DATA ";



            var retGrupos =
                (await _repository.FindBySql<ConsumoDisponibilidadeReservas>(consumoDisponibilidadePorGrupos, new Parameter("dataInicial", dataInicial), new Parameter("dataFinal", dataFinal))).AsList();

            var datas = new List<DateTime>();
            var dataAtual = dataInicial.Date;
            do
            {
                datas.Add(dataAtual);
                dataAtual = dataAtual.AddDays(1);

            } while (dataAtual.Date <= dataFinal.Date);


            var bloqueios = (await _repository.FindBySql<BloqueiosAptoModel>(hqlBloqueios1, new Parameter("dataInicial", dataInicial), new Parameter("dataFinal", dataFinal))).AsList();

            var retTiposApto = (await _repository.FindBySql<TipoUhModel>(tiposDeAtpto)).AsList();

            foreach (var item in retTiposApto.GroupBy(c => c.IdHotel))
            {
                var dispHotel = new DisponibilidadeDoHotel() { IdHotel = item.Key };
                foreach (var itemDisp in item.GroupBy(a => a.IdTipoUh))
                {
                    var itemDispFirst = itemDisp.First();
                    foreach (var dt in datas.OrderBy(a => a.Date))
                    {
                        var dispFixa = disponibilidadesFixas.FirstOrDefault(a => a.IdTipoUh.
                        Equals(itemDisp.Key) && a.Checkout <= dt.Date &&
                        a.Checkin >= dt.Date);

                        var dd = new DisponibilidadeDia()
                        {
                            Data = dt.Date,
                            IdTipoUh = itemDisp.Key.GetValueOrDefault(),
                            CodigoTipoUh = itemDispFirst.CodReduzido,
                            QtdeTotalUh = itemDispFirst.Qtde.GetValueOrDefault(),
                            NomeTipoApto = itemDispFirst.Descricao,
                            Capacidade = itemDispFirst.Capacidade > 4 ? 4 : itemDispFirst.Capacidade
                        };


                        var ocupacaoGrupo = retGrupos.FirstOrDefault(b =>
                            b.IdHotel.Equals(dispHotel.IdHotel) &&
                            b.Data == dd.Data &&
                            b.IdTipoUh.Equals(dd.IdTipoUh));
                        if (ocupacaoGrupo != null)
                        {
                            dd.QuantidadeUtilizada += ocupacaoGrupo.QuantidadeUtilizada.GetValueOrDefault();
                        }
                        else dd.QuantidadeUtilizada += 0;

                        var ocupacaoReservas = retReservas.FirstOrDefault(b =>
                                 b.IdHotel.Equals(dispHotel.IdHotel) &&
                                b.Data == dd.Data &&
                                b.IdTipoUh.Equals(dd.IdTipoUh));
                        if (ocupacaoReservas != null)
                        {
                            dd.QuantidadeUtilizada += ocupacaoReservas.QuantidadeUtilizada.GetValueOrDefault();
                        }

                        var qtdeBloqueadas = bloqueios.Where(b => b.IdHotel.Equals(dispHotel.IdHotel) &&
                            b.Data == dd.Data &&
                            b.IdTipoUh.Equals(dd.IdTipoUh)).AsList();

                        if (qtdeBloqueadas != null && qtdeBloqueadas.Any())
                        {
                            dd.QtdeBloqueadasManutencao += qtdeBloqueadas.Sum(b => b.QtdBloq.GetValueOrDefault());
                        }

                        if (dispFixa != null)
                        {
                            if (!string.IsNullOrEmpty(dispFixa.FlgPerc) && dispFixa.FlgPerc == "S")
                            {
                                if (dispFixa.PercDisp.GetValueOrDefault(0.00m) == 0.00m)
                                {
                                    dd.QtdeTotalUh = 0;
                                }
                                else
                                {
                                    dd.QtdeTotalUh = Convert.ToInt32(((itemDispFirst.Qtde.GetValueOrDefault(0) * dispFixa.PercDisp.GetValueOrDefault(0)) / 100));

                                }
                            }
                            else
                            {
                                dd.QtdeTotalUh = dispFixa.QtdaDisp.GetValueOrDefault();
                            }
                        }

                        // Calcular a ocupação percentual atual
                        // Ocupação = (Total - Disponível) / Total * 100
                        decimal ocupacaoAtual = 0;
                        if (dd.QtdeTotalUh > 0)
                        {
                            ocupacaoAtual = Convert.ToDecimal(((dd.QtdeTotalUh - dd.QtdeDisponivel) * 100) / dd.QtdeTotalUh);
                            dd.OcupacaoTipoApartamentoNoDia = ocupacaoAtual;
                        }

                        if (dd.QtdeDisponivel > 0)
                        {
                            dispHotel.DisponibilidadesDias.Add(dd);
                        }

                        dispHotel.TodosAptos.Add(dd);

                    }
                }

                listResult.Add(dispHotel);

            }

            if (listResult != null && listResult.Any() && ocupacaoMaxima.HasValue)
                await FiltrarApenasDatasComOcupacaoInferirOcupacaoMaximaDefinida(listResult, ocupacaoMaxima.GetValueOrDefault(70));

            return listResult;
        }

        private async Task FiltrarApenasDatasComOcupacaoInferirOcupacaoMaximaDefinida(List<DisponibilidadeDoHotel> listResult, decimal ocupacaoMaximaPermitida = 70)
        {
            foreach (var item in listResult.GroupBy(a => a.IdHotel))
            {
                var itemFst = item.First();
                foreach (var itemTipoApto in itemFst.TodosAptos.GroupBy(a=> a.Data.Date))
                {
                    var totalAptosNoDia = itemFst.TodosAptos.Where(a => a.Data.Date == itemTipoApto.Key).Sum(a => a.QtdeTotalUh);
                    var totalAptosDisponiveisNoDia = itemFst.TodosAptos.Where(a => a.Data.Date == itemTipoApto.Key).Sum(a => a.QtdeDisponivel);

                    if (totalAptosNoDia > 0)
                    {
                        var ocupacaoGeralAtual = Convert.ToDecimal(((totalAptosNoDia - totalAptosDisponiveisNoDia) * 100) / totalAptosNoDia);
                        foreach (var itemApto in itemTipoApto)
                        {
                            itemApto.OcupacaoGeralHotel = ocupacaoGeralAtual;
                        }

                        foreach (var itemDisponivel in itemFst.DisponibilidadesDias.Where(b=> b.Data.Date == itemTipoApto.Key.Date).Reverse())
                        {
                            itemDisponivel.OcupacaoGeralHotel = ocupacaoGeralAtual;
                            if (ocupacaoGeralAtual > ocupacaoMaximaPermitida)
                                itemFst.DisponibilidadesDias.Remove(itemDisponivel);

                        }

                    }
                }
            }

            await Task.CompletedTask;
        }

        public async Task<Int64> Save(InclusaoReservaInputModel model)
        {
            if (model.IdVendaXContrato.GetValueOrDefault(0) <= 0)
                throw new ArgumentException("Deve ser informado o IdVendaXContrato");

            if (model.IdHotel.GetValueOrDefault(0) == 0)
            {
                if (model.TipoUso != "I")
                    throw new ArgumentException("O IdHotel deve ser informado.");
                else model.IdHotel = 3;
            }

            ReservaTsModel? reservaCriada = null;

            try
            {
                _repository.BeginTransaction();
                _repositorySystem.BeginTransaction();


                AtendClienteTs? atendClienteTs = await GetAtendimentoCliente(model.IdVendaXContrato.GetValueOrDefault());
                if (atendClienteTs == null || atendClienteTs.IdVendaXContrato.GetValueOrDefault(0) == 0)
                    throw new ArgumentException($"Não foi localizado os dados da venda");

                VendaXContratoTs? vendaXContrato = await GetVendaXContrato(atendClienteTs);

                if (vendaXContrato == null)
                    throw new ArgumentException("Não foi localizado os dados da venda");

                PeriodoDisponivelResultModel? baseSaldoPontos = await GetSaldo(new SearchDisponibilidadeModel() { IdVendaXContrato = vendaXContrato.IdVendaXContrato, NumeroContrato = vendaXContrato.NumeroContrato.GetValueOrDefault().ToString() });
                if (baseSaldoPontos == null || baseSaldoPontos.IdContratoTs.GetValueOrDefault(0) == 0)
                    throw new ArgumentException("Falha na busca de disponibilidade 'Contrato não encontrado'");

                var condicaoFinanceira = await PosicaoFinanceiraContrato(baseSaldoPontos.IdVendaTs.GetValueOrDefault(), baseSaldoPontos.SaldoPontos);

                if (condicaoFinanceira != null && condicaoFinanceira.SaldoInadimplente.GetValueOrDefault(0) > 0)
                    throw new ArgumentException($"Existe pendência financeira no valor de: R$ {condicaoFinanceira.SaldoInadimplente:N2} favor procure a Central de Atendimento ao Cliente.");

                if (condicaoFinanceira != null && condicaoFinanceira.BloqueioTsModel != null)
                {
                    throw new ArgumentException($"Existe bloqueio ativo para esse contrato! Favor procure a Central de Atendimento ao Cliente");
                }

                AjustarQuantidadePaxsPorFaixaEtaria(model);

                DateTime? validadeContrato = null;
                DateTime? validadeCredito = null;

                FracionamentoValido(model);

                if (vendaXContrato.Validade.GetValueOrDefault(0) > 0)
                {
                    if (vendaXContrato.TipoValidade == "A")
                    {
                        validadeContrato = vendaXContrato.DataAniversario.GetValueOrDefault().AddYears(vendaXContrato.Validade.GetValueOrDefault(0)).AddDays(-1);
                        if (validadeContrato.GetValueOrDefault().Date < DateTime.Today.Date)
                            throw new ArgumentException("Contrato vencido");

                        validadeCredito = vendaXContrato.DataIntegraliza.GetValueOrDefault(vendaXContrato.DataAniversario.GetValueOrDefault()).AddYears(1);
                        if (validadeCredito > validadeContrato)
                            validadeCredito = validadeContrato;
                    }
                }

                var configContrato = await GetConfigContrato(baseSaldoPontos);
                if (model.IdFracionamentoTs.GetValueOrDefault(0) == 0)
                {
                    if (configContrato != null && configContrato.PercIntegraliza.GetValueOrDefault(0) > 0)
                    {
                        if (condicaoFinanceira != null && condicaoFinanceira.PercentualIntegralizacao.GetValueOrDefault(0) > 0)
                        {
                            var percIntegralizadoMinimo = configContrato.PercIntegraliza.GetValueOrDefault(20);
                            var percIntegralizadoAtual = condicaoFinanceira.PercentualIntegralizacao;

                            if (percIntegralizadoAtual < percIntegralizadoMinimo)
                                throw new ArgumentException($"Não é possível realizar a reserva, pois o percentual de integralização atual: {percIntegralizadoAtual:N2}% é inferior ao percentual requerido para realização de reservas: {percIntegralizadoMinimo:N2}%. Favor procurar a Central de Atendimento ao Cliente.");
                        }
                    }
                }

                ContratoTsModel? padraoContrato = await GetPadraoContrato(atendClienteTs);

                if (padraoContrato == null)
                    throw new ArgumentException($"Não foi possível encontrar o contrato vinculado ao IdVendaXContrato: {atendClienteTs.IdVendaXContrato}");

                if (padraoContrato.NumeroPontos == 7)
                {
                    if (model.TipoUso == "I")
                        throw new ArgumentException($"Não é possível realizar liberação de semana para a RCI - Intercambiadora para do tipo de contrato: {vendaXContrato.NumeroContrato} de {padraoContrato.NumeroPontos} pontos");
                    if (condicaoFinanceira != null && condicaoFinanceira.PercentualIntegralizacao.GetValueOrDefault(0) < 100)
                        throw new ArgumentException($"Não é possível realizar reserva para o contrato: {vendaXContrato.NumeroContrato} de {padraoContrato.NumeroPontos} antes de integralizar 100%");

                    if ((string.IsNullOrEmpty(model.TipoUso) ||
                       (!model.TipoUso.RemoveAccents().Contains("proprio", StringComparison.InvariantCultureIgnoreCase) &&
                       !model.TipoUso.Contains("up", StringComparison.InvariantCultureIgnoreCase))))
                            throw new ArgumentException($"O contrato: {atendClienteTs.NumeroContrato} só pode ser utilizado pelo titular");
                }

                if (string.IsNullOrEmpty(model.TipoUso) || model.TipoUso.Contains("prop", StringComparison.CurrentCultureIgnoreCase))
                    model.TipoUso = "UP";
                else if (model.TipoUso.Contains("conv", StringComparison.InvariantCultureIgnoreCase))
                    model.TipoUso = "UC";
                else if (model.TipoUso.Contains("int", StringComparison.InvariantCultureIgnoreCase))
                    model.TipoUso = "I";


                var pessoaProprietaria = (await _repository.FindBySql<UserRegisterInputModel>($@"SELECT
                    cp.IdPessoa AS PessoaId,
                    pes.Nome AS FullName,
                    pes.NumDocumento AS CpfCnpj,
                    tdp.NOMEDOCUMENTO AS TipoDocumentoClienteNome,
                    pes.Email,
                    pf.Sexo,
                    pf.DataNasc as DataNascimento
                    FROM
                    ContratoTs c
                    INNER JOIN Pessoa p ON c.IdPessoa = p.IdPessoa
                    INNER JOIN VENDAXCONTRATOTS vts ON c.IdContratoTs = c.IdContratoTs
                    INNER JOIN ProjetoTs pro ON vts.IdProjetoTs = pro.IDPROJETOTS
                    INNER JOIN VendaTs v ON v.IdVendaTs = vts.IdVendaTs AND vts.IDCONTRATOTS = c.IDCONTRATOTS
                    INNER JOIN ATENDCLIENTETS a ON vts.IDATENDCLIENTETS = a.IDATENDCLIENTETS
                    INNER JOIN CLIENTEPESS cp ON cp.IdPessoa = a.IDCLIENTE
                    INNER JOIN Pessoa pes ON cp.IdPessoa = pes.IdPessoa
                    LEFT JOIN PessoaFisica pf on pes.idPessoa = pf.IdPessoa
                    LEFT JOIN TipoDocPessoa tdp ON pes.IDDOCUMENTO = tdp.IDDOCUMENTO
                    WHERE
                    vts.FLGCANCELADO = 'N' AND
                    vts.FlgRevertido = 'N' AND 
                    pes.IdPessoa = {atendClienteTs.IdCliente}")).FirstOrDefault();


                if (!string.IsNullOrEmpty(model.TipoUso) &&
                    (model.TipoUso.RemoveAccents().Contains("proprio", StringComparison.InvariantCultureIgnoreCase) || 
                    model.TipoUso.Contains("up",StringComparison.InvariantCultureIgnoreCase)))
                {

                    if (pessoaProprietaria != null)
                    {
                        var principal = model.Hospedes.FirstOrDefault(a => a.Principal == "S");
                        if (principal == null)
                            throw new ArgumentException("Deve ser informado o hóspede principal");

                        if (principal.IdHospede.GetValueOrDefault() == int.Parse(pessoaProprietaria.PessoaId!) ||
                            principal.IdHospede.GetValueOrDefault(0) == 0)
                        {
                            principal.Cpf = pessoaProprietaria.CpfCnpj;
                            principal.Nome = pessoaProprietaria.FullName;
                            if (string.IsNullOrEmpty(principal.Email))
                                principal.Email = pessoaProprietaria.Email;
                            principal.Id = !string.IsNullOrEmpty(pessoaProprietaria.PessoaId) && Helper.IsNumeric(pessoaProprietaria.PessoaId) ? Int64.Parse(pessoaProprietaria.PessoaId) : null;
                            principal.IdHospede = pessoaProprietaria.PessoaId != null && Helper.IsNumeric(pessoaProprietaria.PessoaId) ? int.Parse(pessoaProprietaria.PessoaId) : null;
                            principal.Sexo = pessoaProprietaria.Sexo;
                            principal.DataNascimento = pessoaProprietaria.DataNascimento;
                        }

                    }
                    else throw new ArgumentException("Não foi possível encontrar os dados do proprietário");
                }

                if (model.Hospedes == null || !model.Hospedes.Any())
                {
                    model.Hospedes = new List<HospedeInputModel>()
                    {
                        new HospedeInputModel()
                        {
                            Nome = "RCI CONFIRMAR",
                            Id = 0,
                            IdHospede = 0,
                            Principal = "S"
                        },
                        new HospedeInputModel()
                        {
                            Nome = "ACT1 RCI CONFIRMAR",
                            Id = 0,
                            IdHospede = 0,
                            Principal = "N"
                        },
                        new HospedeInputModel()
                        {
                            Nome = "ACT2 RCI CONFIRMAR",
                            Id = 0,
                            IdHospede = 0,
                            Principal = "N"
                        },
                        new HospedeInputModel()
                        {
                            Nome = "ACT3 RCI CONFIRMAR",
                            Id = 0,
                            IdHospede = 0,
                            Principal = "N"
                        }
                    };

                    model.QuantidadeAdultos = 4;
                }

                if (model.Hospedes != null && model.Hospedes.Any())
                {
                    foreach (var item in model.Hospedes)
                    {
                        if (!string.IsNullOrEmpty(item.Documento))
                        {
                            if (Helper.IsCpf(item.Documento))
                                item.Cpf = item.Documento;
                        }

                        if (!item.Id.HasValue)
                            item.Id = 0;

                        if (!item.IdHospede.HasValue)
                            item.IdHospede = 0;

                        if (item.DataNascimento.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue || item.DataNascimento.GetValueOrDefault().Date >= DateTime.Today.AddDays(-5))
                            item.DataNascimento = DateTime.Today.AddYears(-20);

                        if (string.IsNullOrEmpty(item.Nome))
                        {
                            item.Nome = "CONFIRMAR CONFIRMAR";
                        }
                        else if (item.Nome.Split(' ').Count() == 1)
                        {
                            item.Nome += $" CONFIRMAR";
                        }

                        if (!string.IsNullOrEmpty(item.CidadeUf))
                        {
                            var cidades = (await _cidadeService.SearchCityOnProvider(new CidadeSearchModel() { Search = item.CidadeUf })).Value.cidades;

                            if (cidades != null && cidades.Any())
                            {
                                var fst = cidades.First();
                                item.CidadeId = fst.Id;
                                item.CodigoIbge = fst.CodigoIbge;
                                item.CidadeNome = fst.Nome;
                            }
                        }
                    }
                }

                AjustarQuantidadePaxsPorFaixaEtaria(model);

                IList<PeriodoDisponivelResultModel>? disponibilidades = await Disponibilidade(new SearchDisponibilidadeModel()
                {
                    DataInicial = model.Checkin,
                    DataFinal = model.Checkout,
                    HotelId = $"{model.IdHotel.GetValueOrDefault()}",
                    IdVendaXContrato = model.IdVendaXContrato,
                    NumeroContrato = model.NumeroContrato,
                    TipoDeBusca = "A"
                });

                if (disponibilidades == null || disponibilidades.Count == 0)
                    throw new ArgumentException($"Não foi localizada disponibilidade para o período de: {model.Checkin.GetValueOrDefault().Date:dd/MM/yyyy} até: {model.Checkout.GetValueOrDefault().Date:dd/MM/yyyy}");

                
                var qtdePax = model.QuantidadeAdultos.GetValueOrDefault(0) + model.QuantidadeCrianca1.GetValueOrDefault(0) + model.QuantidadeCrianca2.GetValueOrDefault(0);

                var disponibidadeAssociada = disponibilidades.Where(a => a.Checkin.GetValueOrDefault().Date == model.Checkin.GetValueOrDefault().Date &&
                a.Checkout.GetValueOrDefault().Date == model.Checkout.GetValueOrDefault().Date &&
                a.CapacidadePontos1 >= qtdePax && a.HotelId == model.IdHotel).FirstOrDefault() ??
                disponibilidades.Where(a => a.Checkin.GetValueOrDefault().Date == model.Checkin.GetValueOrDefault().Date &&
                a.Checkout.GetValueOrDefault().Date == model.Checkout.GetValueOrDefault().Date &&
                a.CapacidadePontos2 >= qtdePax && a.HotelId == model.IdHotel).FirstOrDefault();

                if ((disponibidadeAssociada == null || disponibidadeAssociada.HotelId != model.IdHotel) && model.TipoUso != "I" && !model.TipoUso.Contains("int",StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentException($"Não foi localizada disponibilidade para o período de: {model.Checkin.GetValueOrDefault().Date:dd/MM/yyyy} até: {model.Checkout.GetValueOrDefault().Date:dd/MM/yyyy}");


                model.IdHotel = disponibidadeAssociada != null ? disponibidadeAssociada.HotelId : model.IdHotel;
                decimal? pontosUtilizar = 0;
                int? contrTsXPontosUtilizar = null;
                if (model.TipoUso != "I" && !model.TipoUso.Contains("int",StringComparison.InvariantCultureIgnoreCase) && disponibidadeAssociada != null)
                {
                    contrTsXPontosUtilizar = disponibidadeAssociada.IdContrTsXPontos1;
                    if (disponibidadeAssociada.IdContrTsXPontos2 != null && qtdePax > disponibidadeAssociada.CapacidadePontos1)
                        contrTsXPontosUtilizar = disponibidadeAssociada.IdContrTsXPontos2;

                    // Usar método centralizado para calcular pontos (ÚNICO LUGAR onde pontos são calculados)
                    pontosUtilizar = CalcularPontosNecessarios(model, disponibidadeAssociada, model.IdHotel, qtdePax);

                    if (model.IdFracionamentoTs.GetValueOrDefault(disponibidadeAssociada.FechamentoFracionamentoPossivelId.GetValueOrDefault()) > 0)
                    {
                        var fracionamento = (await _repository.FindBySql<FracionamentoTs>($"Select f.* From FracionamentoTs f Where f.IdFracionamentoTs = {model.IdFracionamentoTs.GetValueOrDefault(disponibidadeAssociada.FechamentoFracionamentoPossivelId.GetValueOrDefault())} and f.IdReservasFront2 is null")).FirstOrDefault();
                        if (fracionamento == null)
                            throw new ArgumentException("Não foi localizado o fracionamento indicado");
                        else if (fracionamento.IdReservasFront2.GetValueOrDefault(0) > 0)
                            throw new ArgumentException($"O Fracionamento: {fracionamento.IdFracionamentoTs} já foi finaliado.");

                        pontosUtilizar = 0;
                    }
                }

                // Se for reserva RCI, usar pontos fixos configurados
                if (!string.IsNullOrEmpty(model.TipoUso) && (model.TipoUso == "I" || model.TipoUso.Contains("int",StringComparison.InvariantCultureIgnoreCase)))
                {
                    var parametroSistema = await _serviceBase.GetParametroSistema();
                    var pontosRci = parametroSistema?.PontosRci ?? 5629;
                    pontosUtilizar = pontosRci;

                    model.NumeroPontos = pontosUtilizar;
                }

                // Buscar regra tarifária vigente filtrando por hotel se informado
                var regraTarifaria = await _regraPaxFreeService.GetRegraVigente(disponibidadeAssociada?.HotelId);


                var cmUserId = _configuration.GetValue<int>("CMUserId", 1900693);

                await GravarLogByType(cmUserId, 769, "I");

                await GravarLogByType(cmUserId, 769, "O");

                ParamTs paramTs = await GetParamHotel(padraoContrato.IdHotel.GetValueOrDefault());

                var diasReservaAtual = model.Checkout.GetValueOrDefault().Date.Subtract(model.Checkin.GetValueOrDefault().Date).Days;

                if (paramTs.NumMaxPernoites.GetValueOrDefault(0) > 0 &&
                    diasReservaAtual > paramTs.NumMaxPernoites.GetValueOrDefault(0))
                {
                    throw new ArgumentException($"A reserva pode conter no máximo: {paramTs.NumMaxPernoites.GetValueOrDefault(0)} pernoites/diárias.");
                }

                await SetarParametrosReserva(model);
                model.TipoUhEstadia = disponibidadeAssociada?.TipoUhId;
                model.TipoUhTarifa = disponibidadeAssociada?.TipoUhId;
                model.IdTipoUh = disponibidadeAssociada?.TipoUhId;
                model.ClienteReservante = $"{atendClienteTs.IdCliente}";
                model.IdPessoaChave = atendClienteTs.IdCliente;

                // 🔥 Garantir que TipoUso seja definido antes do envio (valor padrão: "UP" - Uso Próprio)
                if (string.IsNullOrEmpty(model.TipoUso))
                {
                    model.TipoUso = model.TipoDeUso ?? "UP";
                    _logger.LogInformation("TipoUso não informado no model, definindo valor padrão: UP (Uso Próprio)");
                }

                if (string.IsNullOrEmpty(model.TipoDeUso))
                {
                    model.TipoDeUso = model.TipoUso ?? "UP";
                    _logger.LogInformation("TipoUso não informado no model, definindo valor padrão: UP (Uso Próprio)");
                }


                if (string.IsNullOrEmpty(model.TipoUso) && !string.IsNullOrEmpty(model.TipoDeUso))
                    model.TipoUso = model.TipoDeUso;
                else if (string.IsNullOrEmpty(model.TipoDeUso) && !string.IsNullOrEmpty(model.TipoUso))
                    model.TipoDeUso = model.TipoUso;

                var reservaModel = (InclusaoReservaInputDto)model;

                model.QtdePaxConsiderar = qtdePax;

                if (regraTarifaria != null && pontosUtilizar > 0 && (!model.TipoUso.Contains("inte",StringComparison.CurrentCultureIgnoreCase) && model.TipoUso != "I"))
                    CalcularRegraTarifaria(model, disponibilidades, qtdePax, ref disponibidadeAssociada, ref contrTsXPontosUtilizar, ref pontosUtilizar, regraTarifaria);

                if (pontosUtilizar != null && pontosUtilizar > condicaoFinanceira?.PontosIntegralizadosDisponiveis)
                    throw new ArgumentException($"Saldo de pontos integralizados disponíveis: {condicaoFinanceira.PontosIntegralizadosDisponiveis} é inferir aos {pontosUtilizar.Value}, necesários para a criação da reserva");


                var result = new ResultModel<Int64?>();

                ReservaTimeSharing? reservaTimeSharingHistorico = null;

                reservaModel.QuantidadeAdultos = model.QuantidadeAdultos.GetValueOrDefault();
                reservaModel.QuantidadeCrianca1 = model.QuantidadeCrianca1;
                reservaModel.QuantidadeCrianca2 = model.QuantidadeCrianca2;

                if (!string.IsNullOrEmpty(model.TipoUso) && model.TipoUso != "I" && !model.TipoUso.Contains("inte",StringComparison.InvariantCultureIgnoreCase))
                {
                    reservaTimeSharingHistorico = await SalvarVinculosHistoricosReservasViaPortal(model, pessoaProprietaria, vendaXContrato, atendClienteTs, reservaModel, reservaCriada, pontosUtilizar);
                    reservaModel.AgendamentoId = Math.Abs((reservaTimeSharingHistorico.Id)) * (-1);
                    reservaModel.LocReserva = Convert.ToInt32(reservaModel.AgendamentoId);
                    reservaCriada = await SalvarReservaNoCM(reservaModel);
                    reservaTimeSharingHistorico.IdReservasFront = reservaCriada?.IdReservasFront;
                    reservaTimeSharingHistorico.NumReserva = $"{reservaCriada?.NumReserva}";
                    reservaTimeSharingHistorico.StatusCM = "VINCULADA";
                    await _repositorySystem.Save(reservaTimeSharingHistorico);
                }
                else 
                    reservaTimeSharingHistorico = await SalvarVinculosHistoricosReservasViaPortal(model, pessoaProprietaria, vendaXContrato, atendClienteTs, reservaModel,reservaCriada, pontosUtilizar);

                await GravarVinculosTimeSharingComReservaELogs(model, atendClienteTs, vendaXContrato, validadeCredito, contrTsXPontosUtilizar, cmUserId, paramTs, diasReservaAtual, reservaTimeSharingHistorico, reservaCriada, pontosUtilizar.GetValueOrDefault());

                var commitReservaTransaction = await _repository.CommitAsync();
                if (!commitReservaTransaction.executed)
                    throw commitReservaTransaction.exception ?? throw new Exception("Falha na criação da reserva");

                await _repositorySystem.CommitAsync();

                return reservaCriada != null && reservaCriada.NumReserva.GetValueOrDefault(0) > 0 ? reservaCriada.NumReserva.GetValueOrDefault() : -1;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                _repositorySystem.Rollback();
                _logger.LogError(err, err.Message);

                if (reservaCriada != null)
                {
                    await CancelarReservaAPICM(new CancelarReservaTsModel() { ReservaId = reservaCriada.NumReserva });
                }

                throw err;
            }

        }

        private static void AjustarQuantidadePaxsPorFaixaEtaria(InclusaoReservaInputModel model)
        {
            model.QuantidadeAdultos = !string.IsNullOrEmpty(model.TipoUso) && !model.TipoUso.StartsWith("I") ? 0 : 1;
            model.QuantidadeCrianca1 = 0;
            model.QuantidadeCrianca2 = 0;

            foreach (var item in model.Hospedes)
            {
                if (item.DataNascimento.HasValue)
                {
                    var idade = Math.Round(DateTime.Today.Subtract(item.DataNascimento.GetValueOrDefault().Date).TotalDays / 365, 0);
                    if (idade >= 12)
                        model.QuantidadeAdultos = model.QuantidadeAdultos.GetValueOrDefault(0) + 1;
                    else if (idade <= 11)
                    {
                        if (idade < 6)
                            model.QuantidadeCrianca1 = model.QuantidadeCrianca1.GetValueOrDefault(0) + 1;
                        else model.QuantidadeCrianca2 = model.QuantidadeCrianca2.GetValueOrDefault(0) + 1;
                    }
                }
            }
        }

        /// <summary>
        /// MÉTODO CENTRALIZADO: Calcula os pontos necessários para uma reserva considerando regras tarifárias e capacidades
        /// Este é o ÚNICO lugar onde o cálculo de pontos deve ser feito
        /// </summary>
        /// <param name="model">Modelo da reserva com hóspedes e datas</param>
        /// <param name="disponibilidade">Disponibilidade selecionada com capacidades e pontos</param>
        /// <param name="hotelId">ID do hotel para buscar regras tarifárias</param>
        /// <param name="totalHospedes">Total de hóspedes (adultos + crianças)</param>
        /// <returns>Pontos necessários para a reserva</returns>
        private decimal CalcularPontosNecessarios(
            InclusaoReservaInputModel model, 
            PeriodoDisponivelResultModel disponibilidade, 
            int? hotelId, 
            int totalHospedes)
        {
            try
            {
                if (disponibilidade.CapacidadePontos2 > 0 && totalHospedes > disponibilidade.CapacidadePontos2)
                {
                    _logger.LogWarning(
                        "Total de hóspedes ({TotalHospedes}) excede a capacidade máxima ({CapacidadeMaxima}) para pontos no hotel {HotelId}. Usando pontos padrão.", 
                        totalHospedes, 
                        disponibilidade.CapacidadePontos2, 
                        hotelId);
                    throw new ArgumentException($"Total de hóspedes ({totalHospedes}) excede a capacidade máxima ({disponibilidade.CapacidadePontos2}) para pontos no hotel {hotelId}. Usando pontos padrão.");
                }

                // 1. Calcular quantos hóspedes pagam (aplicando regras tarifárias)
                int hospedesPagantes = CalcularHospedesPagantes(model, totalHospedes, hotelId);
                
                _logger.LogInformation(
                    "Cálculo de pontos - Hotel: {HotelId}, Total hóspedes: {TotalHospedes}, Hóspedes pagantes: {HospedesPagantes}", 
                    hotelId, 
                    totalHospedes, 
                    hospedesPagantes);

                // 2. Determinar pontos baseado na capacidade
                decimal pontosNecessarios;
                
                if (disponibilidade.CapacidadePontos1.HasValue && 
                    hospedesPagantes <= disponibilidade.CapacidadePontos1.Value)
                {
                    // Cabe na capacidade 1
                    pontosNecessarios = disponibilidade.PontosParaCapacidade1.GetValueOrDefault(0);
                    _logger.LogInformation("Usando capacidade 1: {Pontos} pontos", pontosNecessarios);
                }
                else if (disponibilidade.CapacidadePontos2.HasValue && 
                         hospedesPagantes <= disponibilidade.CapacidadePontos2.Value)
                {
                    // Cabe na capacidade 2
                    pontosNecessarios = disponibilidade.PontosParaCapacidade2.GetValueOrDefault(0);
                    _logger.LogInformation("Usando capacidade 2: {Pontos} pontos", pontosNecessarios);
                }
                else
                {
                    // Usar pontos necessário padrão
                    pontosNecessarios = disponibilidade.PontosNecessario.GetValueOrDefault(0);
                    _logger.LogInformation("Usando pontos padrão: {Pontos} pontos", pontosNecessarios);
                }

                return pontosNecessarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular pontos necessários. Usando valor padrão da disponibilidade.");
                return disponibilidade.PontosNecessario.GetValueOrDefault(0);
            }
        }

        /// <summary>
        /// Calcula quantos hóspedes pagam pontos baseado nas regras tarifárias vigentes
        /// </summary>
        /// <summary>
        /// Método público simplificado para calcular pontos necessários baseado em datas e quantidade de pessoas
        /// Este método pode ser consumido pelo frontend para cálculos em tempo real
        /// </summary>
        /// <param name="dataInicial">Data inicial (checkin)</param>
        /// <param name="dataFinal">Data final (checkout)</param>
        /// <param name="quantidadeAdultos">Quantidade de adultos</param>
        /// <param name="quantidadeCriancas1">Quantidade de crianças de 6 a 11 anos</param>
        /// <param name="quantidadeCriancas2">Quantidade de crianças de 0 a 5 anos</param>
        /// <param name="hotelId">ID do hotel</param>
        /// <param name="tipoUhId">ID do tipo de UH</param>
        /// <param name="idVendaXContrato">ID da venda x contrato</param>
        /// <param name="numeroContrato">Número do contrato</param>
        /// <param name="hospedes">Lista de hóspedes com datas de nascimento (opcional)</param>
        /// <returns>Pontos necessários para o período</returns>
        public async Task<decimal> CalcularPontosNecessariosSimplificado(
            DateTime dataInicial,
            DateTime dataFinal,
            int quantidadeAdultos,
            int quantidadeCriancas1,
            int quantidadeCriancas2,
            int hotelId,
            int tipoUhId,
            int idVendaXContrato,
            string numeroContrato,
            string? numReserva,
            List<HospedeInputModel>? hospedes = null)
        {
            try
            {
                // Validações básicas
                if (dataFinal <= dataInicial)
                    throw new ArgumentException("Data final deve ser maior que data inicial");

                if (idVendaXContrato <= 0 || string.IsNullOrEmpty(numeroContrato))
                    throw new ArgumentException("Contrato inválido");

                var vendaXContrato = (await _repository.FindBySql<VendaXContratoTs>($"Select v.* From VendaXContratoTs v Where v.IdVendaXContrato = {idVendaXContrato}")).FirstOrDefault();
                if (vendaXContrato == null)
                    throw new ArgumentException($"Não foi possível identificar a VendaXContrato: {idVendaXContrato}");

                if (hotelId == 0)
                    throw new ArgumentException($"Deve ser informado o HotelId");

                if (tipoUhId == 0)
                    throw new ArgumentException("Deve ser informado o TipoUhId");

                ReservaTsModel? reserva = !string.IsNullOrEmpty(numReserva) && int.Parse(numReserva) > 0 ?
                    (await _repository.FindBySql<ReservaTsModel>(@$"Select
                                                rf.DataChegPrevista as Checkin,
                                                rf.DataPartPrevista as Checkout,
                                                Nvl(rf.TipoDeUso,'UP') AS TipoDeUso,
                                                rf.*
                                            From 
                                                ReservasFront rf 
                                            Where 
                                                rf.NumReserva = {numReserva} and 
                                                rf.StatusReserva in (0,1,5,6) and 
                                                exists(Select rt.IdReservasFront From ReservasTs rt Where rt.IdReservasFront = rf.IdReservasFront) and 
                                                exists(Select lpt.IdReservasFront From LancPontosTs lpt Where lpt.IdReservasFront = rf.IdReservasFront and lpt.IdVendaXContrato = {idVendaXContrato})")).FirstOrDefault() : null;


                if (reserva == null)
                    throw new ArgumentException($"Não foi possível localizar reserva com o número informado: {numReserva}");


                if (!string.IsNullOrEmpty(numReserva) && int.Parse(numReserva) > 0 && reserva == null)
                    throw new ArgumentException($"Não foi encontrada a reserva informada: {numReserva}");

                var tarifarios = await GetTarifarios(
                dataInicial,
                dataFinal,
                vendaXContrato.IdContratoTs.GetValueOrDefault(),
                new List<int>() { hotelId },
                new List<int>() { tipoUhId });

                // Criar modelo temporário para cálculo
                var modelTemp = new InclusaoReservaInputModel
                {
                    QuantidadeAdultos = quantidadeAdultos,
                    QuantidadeCrianca1 = quantidadeCriancas1,
                    QuantidadeCrianca2 = quantidadeCriancas2,
                    Checkin = dataInicial,
                    Checkout = dataFinal,
                    IdHotel = hotelId,
                    IdTipoUh = tipoUhId,
                    IdVendaXContrato = vendaXContrato.IdVendaXContrato,
                    Hospedes = hospedes ?? new List<HospedeInputModel>()
                };

                // Se não foram informados hóspedes mas foram informadas quantidades,
                // criar hóspedes fictícios para o cálculo
                if (!modelTemp.Hospedes.Any() && (quantidadeAdultos > 0 || quantidadeCriancas1 > 0 || quantidadeCriancas2 > 0))
                {
                    // Adicionar adultos
                    for (int i = 0; i < quantidadeAdultos; i++)
                    {
                        modelTemp.Hospedes.Add(new HospedeInputModel
                        {
                            DataNascimento = DateTime.Today.AddYears(-25), // Adulto padrão
                            Principal = i == 0 ? "S" : "N"
                        });
                    }

                    // Adicionar crianças de 6 a 11 anos
                    for (int i = 0; i < quantidadeCriancas1; i++)
                    {
                        modelTemp.Hospedes.Add(new HospedeInputModel
                        {
                            DataNascimento = DateTime.Today.AddYears(-8), // Criança de 8 anos
                            Principal = "N"
                        });
                    }

                    // Adicionar crianças de 0 a 5 anos
                    for (int i = 0; i < quantidadeCriancas2; i++)
                    {
                        modelTemp.Hospedes.Add(new HospedeInputModel
                        {
                            DataNascimento = DateTime.Today.AddYears(-3), // Criança de 3 anos
                            Principal = "N"
                        });
                    }
                }

                int totalHospedes = quantidadeAdultos + quantidadeCriancas1 + quantidadeCriancas2;

                var qtdePagantes = CalcularHospedesPagantes(modelTemp, totalHospedes, reserva.IdHotel);

                var tipoUhUtilizar = reserva.TipoUhTarifa.GetValueOrDefault(reserva.TipoUhEstadia.GetValueOrDefault());
                if (tipoUhId > 0)
                    tipoUhUtilizar = tipoUhId;

                var tarifarioUtilizar = tarifarios.FirstOrDefault(a => a.DataInicial.GetValueOrDefault().Date <= dataInicial.Date && a.DataFinal.GetValueOrDefault().Date >= dataFinal.Date &&
                    a.IdTipoUh == tipoUhUtilizar && a.MinimoDias == dataFinal.Date.Subtract(dataInicial.Date).TotalDays && a.IdHotel == hotelId &&
                    a.NumMinPax.GetValueOrDefault() <= qtdePagantes && a.NumMaxPax.GetValueOrDefault() >= qtdePagantes) ?? 
                    tarifarios.FirstOrDefault(a => a.DataInicial.GetValueOrDefault().Date <= dataInicial.Date && a.DataFinal.GetValueOrDefault().Date >= dataFinal.Date &&
                    a.IdTipoUh == tipoUhUtilizar && a.IdHotel == hotelId);

                if (tarifarioUtilizar == null && hotelId == 3)
                {
                    tarifarioUtilizar = tarifarios.FirstOrDefault(a => a.DataInicial.GetValueOrDefault().Date <= dataInicial.Date && a.DataFinal.GetValueOrDefault().Date >= dataFinal.Date &&
                        a.MinimoDias == dataFinal.Date.Subtract(dataInicial.Date).TotalDays && a.IdHotel == hotelId &&
                        a.NumMinPax.GetValueOrDefault() <= qtdePagantes && a.NumMaxPax.GetValueOrDefault() >= qtdePagantes) ??
                        tarifarios.FirstOrDefault(a => a.DataInicial.GetValueOrDefault().Date <= dataInicial.Date && a.DataFinal.GetValueOrDefault().Date >= dataFinal.Date && a.IdHotel == hotelId);
                }

                if (tarifarioUtilizar == null)
                    throw new ArgumentException("Não foi possível localizar o tarifário para cálculo de pontos");

                
                var pontosRetono = tarifarioUtilizar.NumeroPontos.GetValueOrDefault(0) > 0 ? tarifarioUtilizar.NumeroPontos.GetValueOrDefault() : 0;

                if (pontosRetono == 0)
                    throw new ArgumentException("Não foi possível encontrar tarifário compatível com a nova configuração");

                return pontosRetono;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular pontos necessários simplificado");
                throw;
            }
        }

        private int CalcularHospedesPagantes(InclusaoReservaInputModel model, int totalHospedes, int? hotelId)
        {
            // Se não houver hóspedes ou hotel, todos pagam
            if (model.Hospedes == null || !model.Hospedes.Any() || !hotelId.HasValue)
                return totalHospedes;

            try
            {
                // Buscar regra tarifária vigente para o hotel
                var regraTarifaria = _regraPaxFreeService.GetRegraVigente(hotelId).GetAwaiter().GetResult();
                
                // Se não há regra tarifária, todos pagam
                if (regraTarifaria == null || regraTarifaria.Configuracoes == null || !regraTarifaria.Configuracoes.Any())
                    return totalHospedes;

                // Separar adultos e crianças
                int qtdAdultos = 0;
                var criancas = new List<(int idade, int indice)>();

                // Determinar data de referência (usar a primeira configuração como referência, ou RESERVA como padrão)
                var primeiraConfig = regraTarifaria.Configuracoes.FirstOrDefault();
                var tipoDataReferencia = primeiraConfig?.TipoDataReferencia ?? "RESERVA";
                DateTime dataReferencia = tipoDataReferencia == "CHECKIN" && model.Checkin.HasValue
                    ? model.Checkin.Value.Date
                    : DateTime.Today.Date;

                for (int i = 0; i < model.Hospedes.Count; i++)
                {
                    var hospede = model.Hospedes[i];
                    if (!hospede.DataNascimento.HasValue || hospede.DataNascimento.Value.Date >= dataReferencia.AddYears(-1))
                    {
                        // Sem data de nascimento ou data inválida, considera como adulto
                        qtdAdultos++;
                        continue;
                    }

                    var idade = (int)Math.Floor((dataReferencia - hospede.DataNascimento.Value.Date).TotalDays / 365.25);
                    
                    if (idade >= 12)
                    {
                        qtdAdultos++;
                    }
                    else
                    {
                        criancas.Add((idade, i));
                    }
                }

                // Ordenar configurações por quantidade de adultos (maior primeiro) para aplicar as mais específicas primeiro
                var configuracoes = regraTarifaria.Configuracoes
                    .Where(c => c.QuantidadeAdultos.HasValue && c.QuantidadePessoasFree.HasValue && c.IdadeMaximaAnos.HasValue)
                    .OrderByDescending(c => c.QuantidadeAdultos)
                    .ToList();

                if (!configuracoes.Any())
                    return totalHospedes;

                // Ordenar crianças por idade (menores primeiro) para aplicar regras corretamente
                var criancasOrdenadas = criancas.OrderBy(c => c.idade).ToList();
                
                // Aplicar regras tarifárias
                int criancasFree = 0;
                var criancasMarcadasComoFree = new HashSet<int>();

                foreach (var config in configuracoes)
                {
                    if (qtdAdultos < config.QuantidadeAdultos.Value)
                        continue;

                    // Quantas vezes esta regra se aplica
                    int vezesAplicavel = qtdAdultos / config.QuantidadeAdultos.Value;
                    
                    if (vezesAplicavel > 0)
                    {
                        // Quantas crianças podem ser free com esta regra
                        int pessoasFreeDisponiveis = vezesAplicavel * config.QuantidadePessoasFree.Value;
                        int pessoasFreeAplicadas = 0;

                        var tipoOperador = config.TipoOperadorIdade ?? "<=";

                        foreach (var crianca in criancasOrdenadas)
                        {
                            if (pessoasFreeAplicadas >= pessoasFreeDisponiveis)
                                break;

                            // Verificar se a criança se qualifica e ainda não foi marcada como free
                            bool qualifica = tipoOperador == ">=" 
                                ? crianca.idade >= config.IdadeMaximaAnos.Value
                                : crianca.idade <= config.IdadeMaximaAnos.Value;

                            if (!criancasMarcadasComoFree.Contains(crianca.indice) && qualifica)
                            {
                                criancasMarcadasComoFree.Add(crianca.indice);
                                pessoasFreeAplicadas++;
                                criancasFree++;
                            }
                        }
                    }
                }

                // Total de hóspedes que pagam = adultos (sempre pagam) + crianças que não são free
                int hospedesQuePagam = qtdAdultos + (criancas.Count - criancasFree);
                
                return hospedesQuePagam;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao calcular hóspedes pagantes. Usando valor total: {TotalHospedes}", totalHospedes);
                return totalHospedes;
            }
        }

        private void CalcularRegraTarifaria(InclusaoReservaInputModel model, IList<PeriodoDisponivelResultModel> disponibilidades, int qtdePax, ref PeriodoDisponivelResultModel disponibidadeAssociada, ref int? contrTsXPontosUtilizar, ref decimal? pontosUtilizar, RegraPaxFreeModel? regraTarifaria)
        {
            int qtdePaxParaTarifa = qtdePax;
            int qtdePessoasBaseAtual = qtdePax;
            var pessoasJaConsideradasFree = new List<HospedeInputModel>();
            var pessoasJaConsideradasNaRegra = new List<HospedeInputModel>();

            if (regraTarifaria != null && regraTarifaria.Configuracoes != null && regraTarifaria.Configuracoes.Any())
            {
                foreach (var item in regraTarifaria.Configuracoes.Where(c => c.QuantidadePessoasFree.GetValueOrDefault(0) > 0 && !string.IsNullOrEmpty(c.TipoOperadorIdade) && !string.IsNullOrEmpty(c.TipoDataReferencia)))
                {
                    var idadePessoaFree = item.IdadeMaximaAnos.GetValueOrDefault(0);
                    var tipoOperador = string.IsNullOrEmpty(item.TipoOperadorIdade) ? "<=" : item.TipoOperadorIdade;
                    var tipoDataReferencia = string.IsNullOrEmpty(item.TipoDataReferencia) ? "RESERVA" : item.TipoDataReferencia; // Valor padrão "RESERVA" para compatibilidade

                    // Determinar data de referência para cálculo da idade
                    DateTime dataReferencia = tipoDataReferencia == "CHECKIN" && model.Checkin.HasValue
                        ? model.Checkin.Value.Date
                        : DateTime.Today.Date;

                    IList<HospedeInputModel> pessoasFreeDentroFaixa = new List<HospedeInputModel>();
                    IList<HospedeInputModel> pessoasPagantesForaFaixa = new List<HospedeInputModel>();

                    if (model.Hospedes != null && model.Hospedes.Any())
                    {
                        if (tipoOperador.StartsWith(">"))
                        {
                            pessoasFreeDentroFaixa = model.Hospedes.Where(a => a.DataNascimento.HasValue &&
                                Math.Round(Convert.ToDecimal(dataReferencia.Subtract(a.DataNascimento.Value.Date).TotalDays / 365), 0) >= idadePessoaFree).AsList();

                            foreach (var itemHospede in model.Hospedes.Where(a => a.DataNascimento.HasValue))
                            {
                                var idade = dataReferencia.Date.Subtract(itemHospede.DataNascimento.GetValueOrDefault().Date).Days;
                                if (idade > 0)
                                    idade = (int)idade / 365;

                                if (itemHospede.DataNascimento.GetValueOrDefault().AddYears(idade).Subtract(dataReferencia.Date).TotalDays <= 1 && idade == idadePessoaFree - 1)
                                    idade = idade - 1;

                                if (idade >= idadePessoaFree)
                                    pessoasFreeDentroFaixa.Add(itemHospede);
                                else pessoasPagantesForaFaixa.Add(itemHospede);
                            }

                            foreach (var itemPessoaFreeFaixa in pessoasFreeDentroFaixa.Reverse())
                            {
                                if (pessoasJaConsideradasFree.Any())
                                {
                                    if (pessoasJaConsideradasFree.Contains(itemPessoaFreeFaixa))
                                    {
                                        pessoasFreeDentroFaixa.Remove(itemPessoaFreeFaixa);
                                    }
                                }
                                else pessoasJaConsideradasFree.Add(itemPessoaFreeFaixa);
                            }


                            foreach (var itemPessoaPagante in pessoasPagantesForaFaixa.Reverse())
                            {
                                if (pessoasJaConsideradasNaRegra.Any())
                                {
                                    if (pessoasJaConsideradasNaRegra.Contains(itemPessoaPagante))
                                    {
                                        pessoasPagantesForaFaixa.Remove(itemPessoaPagante);
                                    }
                                }
                                else pessoasJaConsideradasNaRegra.Add(itemPessoaPagante);
                            }
                        }
                        else
                        {
                            foreach (var itemHospede in model.Hospedes.Where(a => a.DataNascimento.HasValue))
                            {
                                var idade = dataReferencia.Date.Subtract(itemHospede.DataNascimento.GetValueOrDefault().Date).Days;
                                if (idade > 0)
                                    idade = (int)idade / 365;

                                if (itemHospede.DataNascimento.GetValueOrDefault().AddYears(idade).Subtract(dataReferencia.Date).TotalDays <= 1 && idade == idadePessoaFree + 1)
                                    idade = idade - 1;

                                if (idade <= idadePessoaFree)
                                    pessoasFreeDentroFaixa.Add(itemHospede);
                                else pessoasPagantesForaFaixa.Add(itemHospede);
                            }

                            foreach (var itemPessoaFreeFaixa in pessoasFreeDentroFaixa.Reverse())
                            {
                                if (pessoasJaConsideradasFree.Any())
                                {
                                    if (pessoasJaConsideradasFree.Contains(itemPessoaFreeFaixa))
                                    {
                                        pessoasFreeDentroFaixa.Remove(itemPessoaFreeFaixa);
                                    }
                                }
                                else pessoasJaConsideradasFree.Add(itemPessoaFreeFaixa);
                            }

                            foreach (var itemPessoaPagante in pessoasPagantesForaFaixa.Reverse())
                            {
                                if (pessoasJaConsideradasNaRegra.Any())
                                {
                                    if (pessoasJaConsideradasNaRegra.Contains(itemPessoaPagante))
                                    {
                                        pessoasPagantesForaFaixa.Remove(itemPessoaPagante);
                                    }
                                }
                                else pessoasJaConsideradasNaRegra.Add(itemPessoaPagante);
                            }
                        }
                    }

                    if (pessoasFreeDentroFaixa != null && pessoasFreeDentroFaixa.Any() && pessoasPagantesForaFaixa != null && pessoasPagantesForaFaixa.Any() &&
                        (pessoasPagantesForaFaixa.Count >= item.QuantidadeAdultos && pessoasPagantesForaFaixa.Count >= (pessoasFreeDentroFaixa.Count - item.QuantidadePessoasFree)))
                    {
                        var qtdePessoasFree = item.QuantidadePessoasFree.GetValueOrDefault();
                        qtdePessoasBaseAtual -= qtdePessoasFree;
                        model.QtdePaxConsiderar = qtdePessoasBaseAtual;
                        model.QtdePessoasFree = qtdePessoasFree;
                    }
                }


                var disponibilidadeAjustada = disponibilidades.Where(a => a.Checkin.GetValueOrDefault().Date == model.Checkin.GetValueOrDefault().Date &&
                    a.Checkout.GetValueOrDefault().Date == model.Checkout.GetValueOrDefault().Date &&
                    a.CapacidadePontos1 >= model.QtdePaxConsiderar && a.HotelId == model.IdHotel).FirstOrDefault() ??
                    disponibilidades.Where(a => a.Checkin.GetValueOrDefault().Date == model.Checkin.GetValueOrDefault().Date &&
                    a.Checkout.GetValueOrDefault().Date == model.Checkout.GetValueOrDefault().Date &&
                    a.CapacidadePontos2 >= model.QtdePaxConsiderar && a.HotelId == model.IdHotel).FirstOrDefault();

                if (disponibilidadeAjustada != null && disponibilidadeAjustada.HotelId == model.IdHotel)
                {
                    // Atualizar disponibilidade e pontos com base na quantidade de pax para tarifa
                    disponibidadeAssociada = disponibilidadeAjustada;
                    contrTsXPontosUtilizar = disponibilidadeAjustada.IdContrTsXPontos1;
                    if (disponibilidadeAjustada.IdContrTsXPontos2 != null && model.QtdePaxConsiderar > disponibilidadeAjustada.CapacidadePontos1)
                        contrTsXPontosUtilizar = disponibilidadeAjustada.IdContrTsXPontos2;

                    // Usar método centralizado para calcular pontos (ÚNICO LUGAR onde pontos são calculados)
                    pontosUtilizar = CalcularPontosNecessarios(model, disponibilidadeAjustada, model.IdHotel, qtdePax);
                }


            }
        }

        public async Task<ReservaTsModel?> AlterarReserva(InclusaoReservaInputModel model)
        {
            if (model.IdReservasFront.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informado o IdReservasFront para alteração da reserva");

            if (model.NumReserva.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informado o NumReserva para alteração da reserva");

            if (model.Reserva.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informado o Id da Reserva para alteração");

            AtendClienteTs? atendClienteTs = await GetAtendimentoCliente(model.IdVendaXContrato.GetValueOrDefault());
            if (atendClienteTs == null || atendClienteTs.IdVendaXContrato.GetValueOrDefault(0) == 0)
                throw new ArgumentException($"Não foi localizado os dados da venda");

            VendaXContratoTs? vendaXContrato = await GetVendaXContrato(atendClienteTs);

            ContratoTsModel? padraoContrato = await GetPadraoContrato(atendClienteTs);

            if (padraoContrato == null)
                throw new ArgumentException($"Não foi possível encontrar o contrato vinculado ao IdVendaXContrato: {atendClienteTs.IdVendaXContrato}");

            PeriodoDisponivelResultModel? baseSaldoPontos = await GetSaldo(new SearchDisponibilidadeModel() { IdVendaXContrato = vendaXContrato.IdVendaXContrato, NumeroContrato = vendaXContrato.NumeroContrato.GetValueOrDefault().ToString() });
            if (baseSaldoPontos == null || baseSaldoPontos.IdContratoTs.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Falha na busca de disponibilidade 'Contrato não encontrado'");

            var condicaoFinanceira = await PosicaoFinanceiraContrato(baseSaldoPontos.IdVendaTs.GetValueOrDefault(), baseSaldoPontos.SaldoPontos);

            if (condicaoFinanceira != null && condicaoFinanceira.SaldoInadimplente.GetValueOrDefault(0) > 0)
                throw new ArgumentException($"Existe pendência financeira no valor de: R$ {condicaoFinanceira.SaldoInadimplente:N2} favor procure a Central de Atendimento ao Cliente.");

            if (condicaoFinanceira != null && condicaoFinanceira.BloqueioTsModel != null)
            {
                throw new ArgumentException($"Existe bloqueio ativo para esse contrato! Favor procure a Central de Atendimento ao Cliente");
            }

            if (padraoContrato.NumeroPontos == 7)
            {
                if (model.TipoUso == "I")
                    throw new ArgumentException($"Não é possível realizar liberação de semana para a RCI - Intercambiadora para do tipo de contrato: {vendaXContrato.NumeroContrato} de {padraoContrato.NumeroPontos} pontos");
                if (condicaoFinanceira != null && condicaoFinanceira.PercentualIntegralizacao.GetValueOrDefault(0) < 100)
                    throw new ArgumentException($"Não é possível realizar reserva para o contrato: {vendaXContrato.NumeroContrato} de {padraoContrato.NumeroPontos} antes de integralizar 100%");

                if ((string.IsNullOrEmpty(model.TipoUso) ||
                   (!model.TipoUso.RemoveAccents().Contains("proprio", StringComparison.InvariantCultureIgnoreCase) &&
                   !model.TipoUso.Contains("up", StringComparison.InvariantCultureIgnoreCase))))
                    throw new ArgumentException($"O contrato: {atendClienteTs.NumeroContrato} só pode ser utilizado pelo titular");
            }

            if (string.IsNullOrEmpty(model.TipoUso))
                model.TipoUso = "UP";

            if (string.IsNullOrEmpty(model.TipoUso) || model.TipoUso.Contains("prop", StringComparison.CurrentCultureIgnoreCase))
                model.TipoUso = "UP";
            else if (model.TipoUso.Contains("conv", StringComparison.InvariantCultureIgnoreCase))
                model.TipoUso = "UC";
            else if (model.TipoUso.Contains("int", StringComparison.InvariantCultureIgnoreCase))
                model.TipoUso = "I";

            if (string.IsNullOrEmpty(model.TipoDeUso) && !string.IsNullOrEmpty(model.TipoUso))
                model.TipoDeUso = model.TipoUso;

            ReservaTsModel? reservaCriada = null;


            if (model.IdReservasFront.GetValueOrDefault(0) > 0)
            {
                reservaCriada = (await _repository.FindBySql<ReservaTsModel>($"Select Nvl(rf.TipoDeUso,'UP') AS TipoDeUso, rf.DataChegPrevista as Checkin, rf.DataPartPrevista as Checkout, rf.* From ReservasFront rf Where rf.IdReservasFront = {model.IdReservasFront.GetValueOrDefault()}")).FirstOrDefault();
                if (reservaCriada == null)
                    throw new ArgumentException("Reserva não encontrada");

                var lancPontosTs = (await _repository.FindByHql<LancPontosTs>($"From LancPontosTs Where IdReservasFront = {reservaCriada.IdReservasFront}")).FirstOrDefault();
                if (lancPontosTs == null)
                    throw new ArgumentException("Lançamento de pontos não encontrato");

                if (vendaXContrato == null || vendaXContrato.NumeroContrato.GetValueOrDefault(0) == 0 || vendaXContrato.FlgCancelado == "S" || vendaXContrato.FlgRevertido == "S")
                    throw new ArgumentException("Não foi posível um contrato ativo vinculado a reserva para alteração");

                if (model.IdHotel.GetValueOrDefault(0) == 0)
                {
                    if (model.TipoUso != "I")
                        throw new ArgumentException("Deve ser informado o IdHotel para alterar a reserva");
                    else model.IdHotel = reservaCriada.IdHotel;
                }

                model.IdVendaXContrato = lancPontosTs.IdVendaXContrato;
                model.NumeroContrato = vendaXContrato.NumeroContrato.ToString();

                if (model.TipoUso.Contains("UP", StringComparison.InvariantCultureIgnoreCase) || model.TipoUso.Contains("UC", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (reservaCriada.IdHotel == model.IdHotel)
                    {
                        model.TipoUhEstadia = reservaCriada.TipoUhEstadia;
                        model.TipoUhTarifa = reservaCriada.TipoUhEstadia;
                        model.Segmento = reservaCriada.CodSegmento;
                        model.IdTipoUh = reservaCriada.TipoUhEstadia.GetValueOrDefault();
                        model.IdHotel = reservaCriada.IdHotel.GetValueOrDefault();
                        model.Origem = reservaCriada.IdOrigem?.ToString();
                    }
                    else
                    {
                        await SetarParametrosReserva(model);
                    }
                }
            }
            else throw new ArgumentException("Não foi possível localizar a reserva para alteração");

            if (model.IdVendaXContrato.GetValueOrDefault(0) <= 0)
                throw new ArgumentException("Não foi encontrado o IdVendaXContrato");


            try
            {
                _repository.BeginTransaction();
                _repositorySystem.BeginTransaction();

                if (model.TipoUso.Contains("uc", StringComparison.InvariantCultureIgnoreCase) || model.TipoUso.Contains("up", StringComparison.InvariantCultureIgnoreCase))
                {
                    reservaCriada = await AlterarReservaExecute(model, reservaCriada);
                }
                else if (model.TipoUso == "I")
                {
                    if (reservaCriada != null && reservaCriada.IdReservasFront.GetValueOrDefault(0) > 0)
                    {
                        var reservaCancelada = await CancelarReservaAPICM(new CancelarReservaTsModel() { ReservaId = reservaCriada.NumReserva });
                        if (reservaCancelada.GetValueOrDefault() == false)
                            throw new ArgumentException("Não foi possível cancelar a reserva vinculada para liberação da semana para RCI - Intercambiadora");

                        await ReverterLancamentosTimeSharing(reservaCriada.IdReservasFront.GetValueOrDefault());
                    }

                    var parametroSistema = await _serviceBase.GetParametroSistema();
                    if (parametroSistema == null)
                        throw new ArgumentException("Parâmetros do sistema não encontrados");

                    if (parametroSistema.PontosRci.GetValueOrDefault(0) <= 0)
                        throw new ArgumentException("Parâmetro de pontos para RCI - Intercambiadora não configurado no sistema");

                    baseSaldoPontos = await GetSaldo(new SearchDisponibilidadeModel() { IdVendaXContrato = vendaXContrato.IdVendaXContrato, NumeroContrato = vendaXContrato.NumeroContrato.GetValueOrDefault().ToString() });
                    if (baseSaldoPontos == null || baseSaldoPontos.IdContratoTs.GetValueOrDefault(0) == 0)
                        throw new ArgumentException("Falha na busca de disponibilidade 'Contrato não encontrado'");

                    if (baseSaldoPontos.SaldoPontos.GetValueOrDefault(0) < parametroSistema.PontosRci.GetValueOrDefault(0))
                        throw new ArgumentException($"Saldo de pontos insuficiente para liberação de semana para RCI - Intercambiadora. Saldo atual: {baseSaldoPontos.SaldoPontos.GetValueOrDefault(0)} pontos");

                    var pessoaProprietaria = (await _repository.FindBySql<UserRegisterInputModel>($@"SELECT
                            cp.IdPessoa AS PessoaId,
                            pes.Nome AS FullName,
                            pes.NumDocumento AS CpfCnpj,
                            tdp.NOMEDOCUMENTO AS TipoDocumentoClienteNome,
                            pes.Email,
                            pf.Sexo,
                            pf.DataNasc as DataNascimento
                            FROM
                            ContratoTs c
                            INNER JOIN Pessoa p ON c.IdPessoa = p.IdPessoa
                            INNER JOIN VENDAXCONTRATOTS vts ON c.IdContratoTs = c.IdContratoTs
                            INNER JOIN ProjetoTs pro ON vts.IdProjetoTs = pro.IDPROJETOTS
                            INNER JOIN VendaTs v ON v.IdVendaTs = vts.IdVendaTs AND vts.IDCONTRATOTS = c.IDCONTRATOTS
                            INNER JOIN ATENDCLIENTETS a ON vts.IDATENDCLIENTETS = a.IDATENDCLIENTETS
                            INNER JOIN CLIENTEPESS cp ON cp.IdPessoa = a.IDCLIENTE
                            INNER JOIN Pessoa pes ON cp.IdPessoa = pes.IdPessoa
                            LEFT JOIN PessoaFisica pf on pes.idPessoa = pf.IdPessoa
                            LEFT JOIN TipoDocPessoa tdp ON pes.IDDOCUMENTO = tdp.IDDOCUMENTO
                            WHERE
                            vts.FLGCANCELADO = 'N' AND
                            vts.FlgRevertido = 'N' AND 
                            pes.IdPessoa = {atendClienteTs.IdCliente}")).FirstOrDefault();

                    //Salvo a pendência para vinculação com RCI
                    var reservaInputModel = new InclusaoReservaInputDto()
                    { };

                    var reservaTimeSharingHistorico = await SalvarVinculosHistoricosReservasViaPortal(model, pessoaProprietaria, vendaXContrato, atendClienteTs, reservaInputModel, null, parametroSistema.PontosRci);

                }
                else throw new ArgumentException("Não foi possível salvar a reserva! 'Tipo uso não informado'");

                var commitReservaTransaction = await _repository.CommitAsync();
                if (!commitReservaTransaction.executed)
                    throw commitReservaTransaction.exception ?? throw new Exception("Falha na criação da reserva");

                await _repositorySystem.CommitAsync();

                return reservaCriada;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                _repositorySystem.Rollback();
                _logger.LogError(err, err.Message);
                throw err;
            }
        }

        private async Task<ReservaTsModel?> AlterarReservaExecute(InclusaoReservaInputModel model, ReservaTsModel? reservaCriada, AtendClienteTs? atendClienteTsReceived = null)
        {
            if (reservaCriada != null)
            {
                model.NumReserva = reservaCriada.NumReserva;
                model.Reserva = reservaCriada.NumReserva;
                model.IdReservasFront = reservaCriada.IdReservasFront;
            }
            

            model.Id = model.Reserva;

            AtendClienteTs? atendClienteTs = atendClienteTsReceived ?? await GetAtendimentoCliente(model.IdVendaXContrato.GetValueOrDefault());
            if (atendClienteTs == null || atendClienteTs.IdVendaXContrato.GetValueOrDefault(0) == 0)
                throw new ArgumentException($"Não foi localizado os dados da venda");

            VendaXContratoTs? vendaXContrato = await GetVendaXContrato(atendClienteTs);

            if (vendaXContrato == null)
                throw new ArgumentException("Não foi localizado os dados da venda");

            PeriodoDisponivelResultModel? baseSaldoPontos = await GetSaldo(new SearchDisponibilidadeModel() { IdVendaXContrato = vendaXContrato.IdVendaXContrato, NumeroContrato = vendaXContrato.NumeroContrato.GetValueOrDefault().ToString() });
            if (baseSaldoPontos == null || baseSaldoPontos.IdContratoTs.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Falha na busca de disponibilidade não foi possível calcular a diferença de pontos");

            var condicaoFinanceira = await PosicaoFinanceiraContrato(baseSaldoPontos.IdVendaTs.GetValueOrDefault(), baseSaldoPontos.SaldoPontos);

            if (condicaoFinanceira != null && condicaoFinanceira.SaldoInadimplente.GetValueOrDefault(0) > 0)
                throw new ArgumentException($"Existe pendência financeira no valor de: R$ {condicaoFinanceira.SaldoInadimplente:N2} favor procure a Central de Atendimento ao Cliente.");

            if (condicaoFinanceira != null && condicaoFinanceira.BloqueioTsModel != null)
            {
                throw new ArgumentException($"Existe bloqueio ativo para esse contrato! Favor procure a Central de Atendimento ao Cliente.");
            }

            AjustarQuantidadePaxsPorFaixaEtaria(model);

            DateTime? validadeContrato = null;
            DateTime? validadeCredito = null;

            if (vendaXContrato.Validade.GetValueOrDefault(0) > 0)
            {
                if (vendaXContrato.TipoValidade == "A")
                {
                    validadeContrato = vendaXContrato.DataAniversario.GetValueOrDefault().AddYears(vendaXContrato.Validade.GetValueOrDefault(0)).AddDays(-1);
                    if (validadeContrato.GetValueOrDefault().Date < DateTime.Today.Date)
                        throw new ArgumentException("Contrato vencido");

                    validadeCredito = vendaXContrato.DataIntegraliza.GetValueOrDefault(vendaXContrato.DataAniversario.GetValueOrDefault()).AddYears(1);
                    if (validadeCredito > validadeContrato)
                        validadeCredito = validadeContrato;
                }
            }

            ContratoTsModel? padraoContrato = await GetPadraoContrato(atendClienteTs);

            if (padraoContrato == null)
                throw new ArgumentException($"Não foi possível encontrar o contrato vinculado ao IdVendaXContrato: {atendClienteTs.IdVendaXContrato}");


            if (padraoContrato.NumeroPontos == 7 && (string.IsNullOrEmpty(model.TipoUso) || 
                (!model.TipoUso.RemoveAccents().Contains("proprio", StringComparison.InvariantCultureIgnoreCase) && 
                !model.TipoUso.Contains("up",StringComparison.OrdinalIgnoreCase))))
                throw new ArgumentException($"O contrato: {atendClienteTs.NumeroContrato} só pode ser utilizado pelo titular");

            var pessoaProprietaria = (await _repository.FindBySql<UserRegisterInputModel>($@"SELECT
                            cp.IdPessoa AS PessoaId,
                            pes.Nome AS FullName,
                            pes.NumDocumento AS CpfCnpj,
                            tdp.NOMEDOCUMENTO AS TipoDocumentoClienteNome,
                            pes.Email,
                            pf.Sexo,
                            pf.DataNasc as DataNascimento
                            FROM
                            ContratoTs c
                            INNER JOIN Pessoa p ON c.IdPessoa = p.IdPessoa
                            INNER JOIN VENDAXCONTRATOTS vts ON c.IdContratoTs = c.IdContratoTs
                            INNER JOIN ProjetoTs pro ON vts.IdProjetoTs = pro.IDPROJETOTS
                            INNER JOIN VendaTs v ON v.IdVendaTs = vts.IdVendaTs AND vts.IDCONTRATOTS = c.IDCONTRATOTS
                            INNER JOIN ATENDCLIENTETS a ON vts.IDATENDCLIENTETS = a.IDATENDCLIENTETS
                            INNER JOIN CLIENTEPESS cp ON cp.IdPessoa = a.IDCLIENTE
                            INNER JOIN Pessoa pes ON cp.IdPessoa = pes.IdPessoa
                            LEFT JOIN PessoaFisica pf on pes.idPessoa = pf.IdPessoa
                            LEFT JOIN TipoDocPessoa tdp ON pes.IDDOCUMENTO = tdp.IDDOCUMENTO
                            WHERE
                            vts.FLGCANCELADO = 'N' AND
                            vts.FlgRevertido = 'N' AND 
                            pes.IdPessoa = {atendClienteTs.IdCliente}")).FirstOrDefault();

            if (!string.IsNullOrEmpty(model.TipoUso) &&
                (model.TipoUso.RemoveAccents().Contains("proprio", StringComparison.InvariantCultureIgnoreCase) ||
                model.TipoUso.RemoveAccents().Contains("UP", StringComparison.InvariantCultureIgnoreCase)))
            {
                if (pessoaProprietaria != null)
                {
                    var principal = model.Hospedes.FirstOrDefault(a => a.Principal == "S");
                    if (principal == null)
                        throw new ArgumentException("Deve ser informado o hóspede principal");

                    if (principal.IdHospede.GetValueOrDefault() == int.Parse(pessoaProprietaria.PessoaId!) || principal.IdHospede.GetValueOrDefault(0) == 0)
                    {
                        principal.Cpf = pessoaProprietaria.CpfCnpj;
                        principal.Nome = pessoaProprietaria.FullName;
                        if (string.IsNullOrEmpty(principal.Email))
                            principal.Email = pessoaProprietaria.Email;
                        principal.Id = !string.IsNullOrEmpty(pessoaProprietaria.PessoaId) && Helper.IsNumeric(pessoaProprietaria.PessoaId) ? Int64.Parse(pessoaProprietaria.PessoaId) : null;
                        principal.IdHospede = pessoaProprietaria.PessoaId != null && Helper.IsNumeric(pessoaProprietaria.PessoaId) ? int.Parse(pessoaProprietaria.PessoaId) : null;
                        principal.Sexo = pessoaProprietaria.Sexo;
                        principal.DataNascimento = pessoaProprietaria.DataNascimento;
                    }
                }

            }

            if (model.Hospedes != null && model.Hospedes.Any())
            {
                foreach (var item in model.Hospedes)
                {
                    if (!string.IsNullOrEmpty(item.Documento))
                    {
                        if (Helper.IsCpf(item.Documento))
                            item.Cpf = item.Documento;
                    }

                    if (item.DataNascimento.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue || item.DataNascimento.GetValueOrDefault().Date >= DateTime.Today.AddDays(-5))
                        item.DataNascimento = DateTime.Today.AddYears(20);

                    if (string.IsNullOrEmpty(item.Nome))
                    {
                        item.Nome = "ACONFIRMAR ACONFIRMAR";
                    }
                    else if (item.Nome.Split(' ').Count() == 1)
                    {
                        item.Nome += $" ACONFIRMAR";
                    }

                    if (!string.IsNullOrEmpty(item.CidadeUf))
                    {
                        var cidades = (await _cidadeService.SearchCityOnProvider(new CidadeSearchModel() { Search = item.CidadeUf })).Value.cidades;

                        if (cidades != null && cidades.Any())
                        {
                            var fst = cidades.First();
                            item.CidadeId = fst.Id;
                            item.CodigoIbge = fst.CodigoIbge;
                            item.CidadeNome = fst.Nome;
                        }
                    }
                }
            }

            var qtdePax = model.QuantidadeAdultos.GetValueOrDefault(0) + model.QuantidadeCrianca1.GetValueOrDefault(0) + model.QuantidadeCrianca2.GetValueOrDefault(0);

            var cmUserId = _configuration.GetValue<int>("CMUserId", 1900693);

            ParamTs paramTs = await GetParamHotel(padraoContrato.IdHotel.GetValueOrDefault());

            var diasReservaAtual = model.Checkout.GetValueOrDefault().Date.Subtract(model.Checkin.GetValueOrDefault().Date).Days;

            if (paramTs.NumMaxPernoites.GetValueOrDefault(0) > 0 &&
                diasReservaAtual > paramTs.NumMaxPernoites.GetValueOrDefault(0))
            {
                throw new ArgumentException($"A reserva pode conter no máximo: {paramTs.NumMaxPernoites.GetValueOrDefault(0)} pernoites/diárias.");
            }

            await SetarParametrosReserva(model);
            model.ClienteReservante = $"{atendClienteTs.IdCliente}";
            model.IdPessoaChave = atendClienteTs.IdCliente;

            model.QuantidadeAdultos = model.Hospedes != null && model.Hospedes.Any() ? model.Hospedes.Count() : 1;
            if (model.Hospedes != null && model.Hospedes.Any(b => b.DataNascimento.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue))
            {
                model.QuantidadeAdultos = model.Hospedes.Count(a => Math.Round(Convert.ToDecimal(DateTime.Today.Subtract(a.DataNascimento.GetValueOrDefault().Date).TotalDays / 365), 0) >= 12);
                model.QuantidadeCrianca1 = model.Hospedes.Count(a => Math.Round(Convert.ToDecimal(DateTime.Today.Subtract(a.DataNascimento.GetValueOrDefault().Date).TotalDays / 365), 0) >= 6 &&
                Math.Round(Convert.ToDecimal(DateTime.Today.Subtract(a.DataNascimento.GetValueOrDefault().Date).TotalDays / 365), 0) < 12);
                model.QuantidadeCrianca2 = model.Hospedes.Count(a => Math.Round(Convert.ToDecimal(DateTime.Today.Subtract(a.DataNascimento.GetValueOrDefault().Date).TotalDays / 365), 0) >= 0 &&
                Math.Round(Convert.ToDecimal(DateTime.Today.Subtract(a.DataNascimento.GetValueOrDefault().Date).TotalDays / 365), 0) < 6);
            }


            var reservaModel = (InclusaoReservaInputDto)model;

            // Buscar dados do usuário logado para preencher LoginPms e LoginSistemaVenda
            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId))
            {
                var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Where u.Id = {loggedUser.Value.userId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                if (usuario != null)
                {
                    reservaModel.LoginPms = usuario.LoginPms;
                    reservaModel.LoginSistemaVenda = usuario.LoginSistemaVenda;
                }
            }

            var result = new ResultModel<Int64?>();

            ReservaTimeSharing? reservaTimeSharingHistorico = null;
            if (reservaModel.IdReservasFront.GetValueOrDefault(0) > 0)
            {
                reservaTimeSharingHistorico = (await _repositorySystem.FindByHql<ReservaTimeSharing>($"From ReservaTimeSharing Where IdReservasFront = {reservaModel.IdReservasFront} and ClienteReservante = {reservaModel.ClienteReservante}")).FirstOrDefault();
                if (reservaTimeSharingHistorico == null)
                {
                    reservaTimeSharingHistorico = await SalvarVinculosHistoricosReservasViaPortal(model, pessoaProprietaria, vendaXContrato, atendClienteTs, reservaModel, reservaCriada, 0);
                }

                reservaModel.LocReserva = Math.Abs(reservaTimeSharingHistorico.Id)*(-1);
                reservaModel.AgendamentoId = reservaModel.LocReserva;
            }
            else
                reservaTimeSharingHistorico = await SalvarVinculosHistoricosReservasViaPortal(model, pessoaProprietaria, vendaXContrato, atendClienteTs, reservaModel, reservaCriada, 0);

            if (reservaModel.LocReserva.GetValueOrDefault(0) == 0 || reservaModel.AgendamentoId.GetValueOrDefault(0) == 0)
            {
                reservaModel.AgendamentoId = Math.Abs(reservaTimeSharingHistorico.Id) * (-1);
                reservaModel.LocReserva = Math.Abs(reservaTimeSharingHistorico.Id) * (-1);
                await _repositorySystem.Save(reservaTimeSharingHistorico);
            }

            reservaCriada = await SalvarReservaNoCM_AjutarPontos(model, reservaCriada, vendaXContrato, baseSaldoPontos, cmUserId, paramTs, atendClienteTs);

            if (reservaCriada != null && reservaCriada.IdReservasFront.GetValueOrDefault(0) > 0)
            {
                reservaTimeSharingHistorico.IdReservasFront = reservaCriada.IdReservasFront;
                await _repositorySystem.Save(reservaTimeSharingHistorico);
                await GravarResevasTs(cmUserId,reservaCriada);
            }
            
            return reservaCriada;
        }

        private async Task<ReservaTsModel?> SalvarReservaNoCM_AjutarPontos(InclusaoReservaInputModel model, 
            ReservaTsModel? reservaCriada, 
            VendaXContratoTs vendaXContrato, 
            PeriodoDisponivelResultModel baseSaldoPontos, 
            int cmUserId, 
            ParamTs paramTs,
            AtendClienteTs atendClienteTs)
        {
            if (model.IdReservasFront.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informado o Id da reserva para ser alterada.");

            // Verificar se a quantidade de pessoas mudou e recalcular pontos se necessário
            var reservaOriginal = (await _repository.FindBySql<ReservaTsModel>($"Select Nvl(rf.TipoDeUso,'UP') AS TipoDeUso, rf.LocReserva, rf.LocReserva as AgendamentoId, rf.DataChegPrevista as Checkin, rf.DataPartPrevista as Checkout, rf.* From ReservasFront rf Where rf.IdReservasFront = {model.IdReservasFront.GetValueOrDefault()} and rf.StatusReserva in (0,1,5,6)")).FirstOrDefault();
            if (reservaOriginal != null)
            {
                var lancPontosTs = (await _repository.FindByHql<LancPontosTs>($"From LancPontosTs Where IdReservasFront = {reservaOriginal.IdReservasFront}")).FirstOrDefault();
                if (lancPontosTs != null)
                {
                    // 1. Calcular pontos da configuração ORIGINAL usando o método simplificado
                    var pontosOriginais = await CalcularPontosNecessariosSimplificado(
                        reservaOriginal.Checkin.GetValueOrDefault(model.Checkin.GetValueOrDefault()),
                        reservaOriginal.Checkout.GetValueOrDefault(model.Checkout.GetValueOrDefault()),
                        reservaOriginal.Adultos ?? 1,
                        reservaOriginal.Criancas1 ?? 0,
                        reservaOriginal.Criancas2 ?? 0,
                        reservaOriginal.IdHotel.GetValueOrDefault(),
                        reservaOriginal.TipoUhTarifa.GetValueOrDefault(reservaOriginal.TipoUhTarifa.GetValueOrDefault()),
                        model.IdVendaXContrato.GetValueOrDefault(),
                        model.NumeroContrato,
                        $"{reservaOriginal.NumReserva}",
                        null
                    );

                    // 2. Calcular pontos da configuração NOVA usando o método simplificado
                    var pontosNovos = await CalcularPontosNecessariosSimplificado(
                        model.Checkin.GetValueOrDefault(),
                        model.Checkout.GetValueOrDefault(),
                        model.QuantidadeAdultos.GetValueOrDefault(1),
                        model.QuantidadeCrianca1.GetValueOrDefault(0),
                        model.QuantidadeCrianca2.GetValueOrDefault(0),
                        model.IdHotel.GetValueOrDefault(),
                        model.IdTipoUh.GetValueOrDefault(),
                        model.IdVendaXContrato.GetValueOrDefault(),
                        model.NumeroContrato,
                        $"{reservaOriginal.NumReserva}",
                        model.Hospedes
                    );

                    // 3. Calcular diferença
                    var diferencaPontos = pontosNovos - pontosOriginais;

                    _logger.LogInformation(
                        "Recálculo de pontos - Original: {PontosOriginais}, Novo: {PontosNovos}, Diferença: {Diferenca}",
                        pontosOriginais,
                        pontosNovos,
                        diferencaPontos
                    );

                    // 4. Verificar se saldo é suficiente para cobrir a diferença
                    if (diferencaPontos > 0)
                    {
                        var saldoAtual = baseSaldoPontos.SaldoPontos.GetValueOrDefault(0);

                        if (saldoAtual < diferencaPontos)
                            throw new ArgumentException(
                                $"Saldo de pontos insuficiente para a alteração. " +
                                $"Pontos originais: {pontosOriginais:N0}, " +
                                $"Pontos necessários: {pontosNovos:N0}, " +
                                $"Diferença a pagar: {diferencaPontos:N0}. " +
                                $"Saldo disponível: {saldoAtual:N0}."
                            );
                    }


                    if (diferencaPontos != 0)
                    {
                        // 5. Estornar lançamento anterior
                        await EfetuarLancamentoPontosTsCancelamento(lancPontosTs, paramTs, cmUserId);
                    }

                    // 6. Atualizar modelo com pontos necessários
                    model.NumeroPontos = pontosNovos;

                    _logger.LogInformation(
                        "Pontos recalculados com sucesso. Reserva: {NumReserva}, " +
                        "Pax Original: {PaxOriginal}, Pax Nova: {PaxNova}, " +
                        "Pontos Originais: {PontosOriginais}, Pontos Novos: {PontosNovos}",
                        reservaOriginal.NumReserva,
                        reservaOriginal.Adultos.GetValueOrDefault().ToString() + "/" + reservaOriginal.Criancas1.GetValueOrDefault().ToString() + "/" + reservaOriginal.Criancas2.GetValueOrDefault().ToString(),
                        model.QuantidadeAdultos.GetValueOrDefault().ToString() + "/" + model.QuantidadeCrianca1.GetValueOrDefault().ToString() + "/" + model.QuantidadeCrianca2.GetValueOrDefault().ToString(),
                        pontosOriginais,
                        pontosNovos
                    );

                    var fracionamentoVinculado = 
                        (await _repository.FindByHql<FracionamentoTs>("From FracionamentoTs f Where (f.IdReservasFront1 = :reservaOriginalId or f.IdReservasFront2 = :reservaOriginalId)", new Parameter("reservaOriginalId", reservaOriginal!.IdReservasFront.GetValueOrDefault()))).FirstOrDefault();

                    if (fracionamentoVinculado != null && reservaOriginal != null)
                    {
                        var qtdeDiasReservaAtual = model.Checkout.GetValueOrDefault().Subtract(model.Checkin.GetValueOrDefault()).TotalDays;
                        var qtdeDiasReservaReservaAnterior = reservaOriginal.Checkout.GetValueOrDefault().Subtract(reservaOriginal.Checkin.GetValueOrDefault()).TotalDays;

                        if (qtdeDiasReservaAtual != 7)
                        {
                            if (qtdeDiasReservaAtual != qtdeDiasReservaReservaAnterior)
                                throw new ArgumentException($"Não é possível alterar uma reserva de fracionamento de: {qtdeDiasReservaReservaAnterior} para {qtdeDiasReservaAtual} dias.");

                            if (reservaOriginal.Checkin.GetValueOrDefault().DayOfWeek != model.Checkin.GetValueOrDefault().DayOfWeek)
                                throw new ArgumentException($"Não é possível alterar uma reserva de fracionamento iniciando no dia: {reservaOriginal.Checkin.GetValueOrDefault():ddd} para início no dia {model.Checkin.GetValueOrDefault():ddd}.");

                            if (reservaOriginal.Checkout.GetValueOrDefault().DayOfWeek != model.Checkout.GetValueOrDefault().DayOfWeek)
                                throw new ArgumentException($"Não é possível alterar uma reserva de fracionamento encerrando no dia: {reservaOriginal.Checkout.GetValueOrDefault():ddd} para encerramento no dia {model.Checkout.GetValueOrDefault():ddd}.");

                            if (fracionamentoVinculado.IdReservasFront2.GetValueOrDefault(0) > 0 && fracionamentoVinculado.IdReservasFront2.GetValueOrDefault() != reservaOriginal.IdReservasFront)
                                throw new ArgumentException("Não é possível alterar uma reserva vinculada a um fracionamento já encerrado.");
                        }
                    }
                    else if (model.Checkout.GetValueOrDefault().Subtract(model.Checkin.GetValueOrDefault()).TotalDays != 7)
                    {
                        FracionamentoValido(model);
                    }

                    if (reservaOriginal != null && model.IdHotel != reservaOriginal.IdHotel)
                    {

                        bool? cancelada = null;
                        try
                        {
                            cancelada = await CancelarReservaExecute(new CancelarReservaTsModel()
                            {
                                ReservaId = reservaOriginal.NumReserva
                            }, cancelada);
                        }
                        catch (Exception err)
                        {
                            cancelada = await CancelarReservaExecute(new CancelarReservaTsModel()
                            {
                                ReservaId = reservaOriginal.NumReserva,
                            }, cancelada);
                        }

                        if (cancelada.GetValueOrDefault())
                        {
                            await _repository.ExecuteSqlCommand($"Update ReservasFront Set LocReserva = null Where IdReservasFront = {reservaOriginal.IdReservasFront} ");

                            model.Reserva = null;
                            model.Id = null;
                        }
                    }

                    var reservaTimeShaging = (await _repositorySystem.FindBySql<ReservaTimeSharing>($"Select t.* From ReservaTimeSharing t Where t.IdReservasFront = {reservaOriginal!.IdReservasFront.GetValueOrDefault()} Order by t.Id Desc")).FirstOrDefault();
                    if (reservaTimeShaging == null)
                    {
                        var pessoaProprietaria = (await _repository.FindBySql<UserRegisterInputModel>($"Select p.IdPessoa as PessoaId, p.Nome as FullName From Pessoa p Where p.IdPessoa = {atendClienteTs.IdCliente}")).FirstOrDefault();
                        if (pessoaProprietaria == null)
                            throw new ArgumentException("Não foi possível localizar os dados do proprietário para salvar o histórico da reserva.");

                        reservaTimeShaging = await SalvarVinculosHistoricosReservasViaPortal(model, pessoaProprietaria, vendaXContrato, atendClienteTs, (InclusaoReservaInputDto)model, reservaCriada, reservaOriginal.IdReservasFront.GetValueOrDefault());
                    }

                    if (reservaTimeShaging != null)
                    {
                        model.AgendamentoId = Math.Abs((reservaTimeShaging.Id)) * (-1);
                        model.LocReserva = Convert.ToInt32(model.AgendamentoId);
                    }
                    else
                    {
                        model.AgendamentoId = Math.Abs((reservaOriginal.LocReserva.GetValueOrDefault(0))) * (-1);
                        model.LocReserva = Convert.ToInt32(model.AgendamentoId);
                    }

                    reservaCriada = await SalvarReservaNoCM((InclusaoReservaInputDto)model);

                    if (reservaCriada != null)
                    {
                        if (model.Checkout.GetValueOrDefault().Subtract(model.Checkin.GetValueOrDefault()).TotalDays != 7)
                        {
                            if (fracionamentoVinculado != null)
                            {
                                fracionamentoVinculado.IdReservasFront1 = reservaCriada.IdReservasFront;
                                fracionamentoVinculado.Inclusao = false;
                                await _repository.Save(fracionamentoVinculado);
                            }
                            else await GravarAberturaFracionamento(cmUserId, atendClienteTs, reservaCriada, reservaTimeShaging!);
                        }
                        else if (fracionamentoVinculado != null)
                        {
                            fracionamentoVinculado = (await _repository.FindByHql<FracionamentoTs>("From FracionamentoTs f Where f.IdFracionamentoTs = :id",new Parameter("id", fracionamentoVinculado.IdFracionamentoTs.GetValueOrDefault()))).FirstOrDefault();
                            if (fracionamentoVinculado != null)
                                await _repository.Remove(fracionamentoVinculado);
                        }
                    }

                    // Se pontos foram recalculados, criar novo lançamento
                    if (reservaCriada != null && model.NumeroPontos.HasValue && model.NumeroPontos.Value > 0)
                    {
                        var lancPontosTsAtual = (await _repository.FindByHql<LancPontosTs>($"From LancPontosTs Where IdReservasFront = {reservaCriada.IdReservasFront}")).FirstOrDefault();
                        if (lancPontosTsAtual == null && reservaOriginal != null)
                        {
                            // Criar novo lançamento com pontos recalculados
                            var novoLancPontos = await EfetuarLancamentoPontosTs(
                                reservaCriada,
                                paramTs,
                                model,
                                cmUserId,
                                baseSaldoPontos.IdContratoTs,
                                baseSaldoPontos.ValidadeCredito,
                                model.NumeroPontos.Value,
                                "S"
                            );

                            // Gravar lançamento financeiro vinculado aos pontos
                            if (vendaXContrato != null)
                            {
                                await EfetuarLancamentoCriacaoReservaTs(vendaXContrato, novoLancPontos, reservaCriada, paramTs, model, cmUserId, baseSaldoPontos.IdContratoTs, baseSaldoPontos.ValidadeCredito);
                            }
                        }
                    }
                }

            }

            return reservaCriada;
        }

        private static void FracionamentoValido(InclusaoReservaInputModel model)
        {
            if (model.Checkout.GetValueOrDefault().Subtract(model.Checkin.GetValueOrDefault()).TotalDays == 7) return;
            
            if (model.IdHotel == 1)
            {
                if (model.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Thursday && model.Checkout.GetValueOrDefault().Subtract(model.Checkin.GetValueOrDefault()).TotalDays != 3)
                    throw new ArgumentException("Fracionamentos do hotel escolhido, iniciando na quinta-feira deve encerrar no domingo.");

            }

            if (model.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday && model.Checkout.GetValueOrDefault().Subtract(model.Checkin.GetValueOrDefault()).TotalDays != 3)
                throw new ArgumentException("Fracionamentos do hotel escolhido, iniciando no dominto deve encerrar na quarta-feira.");

            if (model.Checkin.GetValueOrDefault().DayOfWeek == DayOfWeek.Wednesday && model.Checkout.GetValueOrDefault().Subtract(model.Checkin.GetValueOrDefault()).TotalDays != 4)
                throw new ArgumentException("Fracionamentos do hotel escolhido, iniciando na quarta-feira deve encerrar no domingo.");
        }

        private async Task<bool?> CancelarReservaAPICM(CancelarReservaTsModel model)
        {
            var baseUrl = _configuration.GetValue<string>("ReservasCMApiConfig:BaseUrl");
            var liberarPoolUrl = _configuration.GetValue<string>("ReservasCMApiConfig:CancelarReserva");
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
                    return true;
                }
                else
                {
                    return false;
                }

            }

        }

        private async Task<ReservaTimeSharing> SalvarVinculosHistoricosReservasViaPortal(InclusaoReservaInputModel model, 
            UserRegisterInputModel? pessoaProprietaria, 
            VendaXContratoTs vendaXContrato, 
            AtendClienteTs atendClienteTs, 
            InclusaoReservaInputDto reservaModel, 
            ReservaTsModel? reservaCriada, 
            decimal? pontosUtilizar)
        {
            if (string.IsNullOrEmpty(model.TipoUso) && 
                !string.IsNullOrEmpty(model.TipoDeUso))
                model.TipoUso = model.TipoDeUso;
            else if (!string.IsNullOrEmpty(model.TipoUso) && 
                string.IsNullOrEmpty(model.TipoDeUso))
                model.TipoDeUso = model.TipoUso;

            var reservaTimeSharingHistorico = new ReservaTimeSharing()
            {
                IdTipoUh = model.TipoUhEstadia,
                ClienteReservante = atendClienteTs.IdCliente,
                PontosUtilizados = model.NumeroPontos ?? pontosUtilizar,
                FracionamentoIdFinalizado = model.IdFracionamentoTs,
                NomeCliente = pessoaProprietaria?.FullName,
                Checkin = model.Checkin,
                Checkout = model.Checkout,
                Adultos = model.QuantidadeAdultos,
                Criancas1 = model.QuantidadeCrianca1,
                Criancas2 = model.QuantidadeCrianca2,
                IdVendaXContrato = atendClienteTs.IdVendaXContrato,
                NumeroContrato = $"{vendaXContrato.NumeroProjeto}-{vendaXContrato.NumeroContrato}" ?? atendClienteTs.NumeroContrato,
                TipoUtilizacao = model.TipoUtilizacao ?? model.TipoUso,
                IdReservasFront = reservaCriada != null ? reservaCriada.IdReservasFront : null,
            };

            if (reservaTimeSharingHistorico != null && !string.IsNullOrEmpty(reservaTimeSharingHistorico.TipoUtilizacao) && 
                (reservaTimeSharingHistorico.TipoUtilizacao.Contains("rci", StringComparison.InvariantCultureIgnoreCase) || 
                reservaTimeSharingHistorico.TipoUtilizacao.Contains("intercambiadora", StringComparison.InvariantCultureIgnoreCase) ||
                reservaTimeSharingHistorico.TipoUtilizacao.Equals("I",StringComparison.InvariantCultureIgnoreCase)))
            {
                reservaTimeSharingHistorico.TipoUtilizacao = "RCI - INTERCAMBIADORA";
                reservaTimeSharingHistorico.StatusCM = "Pendente";
            }


            await _repositorySystem.Save(reservaTimeSharingHistorico);
            reservaModel.LocReserva = reservaTimeSharingHistorico!.Id;
            reservaModel.AgendamentoId = reservaTimeSharingHistorico.Id;
            return reservaTimeSharingHistorico;
        }

        private async Task GravarVinculoReservaRciReservaJaExistente(InclusaoReservaInputModel model, 
            AtendClienteTs atendClienteTs, 
            VendaXContratoTs vendaXContrato, 
            DateTime? validadeCredito, 
            int? contrTsXPontosUtilizar, 
            int cmUserId, 
            ParamTs paramTs, 
            int diasReservaAtual, 
            ReservaTimeSharing reservaTimeSharingHistorico, 
            ReservaTsModel? reservaCriada,
            decimal pontosUtilizados)
        {
            if (reservaCriada != null)
            {
                await GravarLogs(cmUserId, atendClienteTs, paramTs);

                if (model.IdVendaXContrato.GetValueOrDefault(0) == 0)
                    model.IdVendaXContrato = vendaXContrato.IdVendaXContrato;

                LancPontosTs lancPontosTs = await EfetuarLancamentoPontosTs(reservaCriada, paramTs, model, cmUserId, contrTsXPontosUtilizar, validadeCredito,pontosUtilizados,flgAssociada: "S");

                LancamentoTs lancamentoTs = await EfetuarLancamentoCriacaoReservaTs(vendaXContrato, lancPontosTs, reservaCriada, paramTs, model, cmUserId, contrTsXPontosUtilizar, validadeCredito);

                await GravarResevasTs(cmUserId, reservaCriada);

                var reservasRci = new ReservasRci()
                {
                    IdReservasFront = reservaCriada.IdReservasFront,
                    TrgDtInclusao = DateTime.Now,
                    TrgUserInclusao = $"CM{cmUserId}",
                    Inclusao = true
                };

                await _repository.Save(reservasRci);

                reservaTimeSharingHistorico.IdReservasFront = reservaCriada.IdReservasFront;
                await _repositorySystem.Save(reservaTimeSharingHistorico);
            }
        }


        private async Task GravarVinculosTimeSharingComReservaELogs(InclusaoReservaInputModel model,
            AtendClienteTs atendClienteTs,
            VendaXContratoTs vendaXContrato,
            DateTime? validadeCredito,
            int? contrTsXPontosUtilizar,
            int cmUserId,
            ParamTs paramTs,
            int diasReservaAtual,
            ReservaTimeSharing reservaTimeSharingHistorico,
            ReservaTsModel? reservaCriada,
            decimal pontosUtilizados)
        {
            if (reservaCriada != null)
            {
                await GravarLogs(cmUserId, atendClienteTs, paramTs);

                await AjustarFracionamentoReserva(model, cmUserId, atendClienteTs, paramTs, diasReservaAtual, reservaCriada, reservaTimeSharingHistorico);

                LancPontosTs lancPontosTs = await EfetuarLancamentoPontosTs(reservaCriada, paramTs, model, cmUserId, contrTsXPontosUtilizar, validadeCredito, pontosUtilizados,flgAssociada:"S");

                LancamentoTs lancamentoTs = await EfetuarLancamentoCriacaoReservaTs(vendaXContrato, lancPontosTs, reservaCriada, paramTs, model, cmUserId, contrTsXPontosUtilizar, validadeCredito);

                await GravarResevasTs(cmUserId, reservaCriada);

                reservaTimeSharingHistorico.IdReservasFront = reservaCriada.IdReservasFront;
                await _repositorySystem.Save(reservaTimeSharingHistorico);
            }
        }


        private async Task<ReservaTsModel?> SalvarReservaNoCM(InclusaoReservaInputDto reservaModel)
        {
            // 🔥 Garantir que TipoUso seja enviado (valor padrão: "UP" - Uso Próprio)
            if (string.IsNullOrEmpty(reservaModel.TipoUso))
            {
                reservaModel.TipoUso = "UP";
                _logger.LogInformation("TipoUso não informado, definindo valor padrão: UP (Uso Próprio)");
            }

            if (string.IsNullOrEmpty(reservaModel.TipoUso) && !string.IsNullOrEmpty(reservaModel.TipoDeUso))
                reservaModel.TipoUso = reservaModel.TipoDeUso;
            else if (string.IsNullOrEmpty(reservaModel.TipoDeUso) && !string.IsNullOrEmpty(reservaModel.TipoUso))
                reservaModel.TipoDeUso = reservaModel.TipoUso;

            // Buscar dados do usuário logado para preencher LoginPms e LoginSistemaVenda se não foram preenchidos
            if (string.IsNullOrEmpty(reservaModel.LoginPms) || string.IsNullOrEmpty(reservaModel.LoginSistemaVenda))
            {
                var loggedUser = await _repositorySystem.GetLoggedUser();
                if (loggedUser.HasValue && !string.IsNullOrEmpty(loggedUser.Value.userId))
                {
                    var usuario = (await _repositorySystem.FindByHql<Domain.Entities.Core.Sistema.Usuario>($"From Usuario u Where u.Id = {loggedUser.Value.userId} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0")).FirstOrDefault();
                    if (usuario != null)
                    {
                        if (string.IsNullOrEmpty(reservaModel.LoginPms))
                            reservaModel.LoginPms = usuario.LoginPms;
                        if (string.IsNullOrEmpty(reservaModel.LoginSistemaVenda))
                            reservaModel.LoginSistemaVenda = usuario.LoginSistemaVenda;
                    }
                }
            }

            _logger.LogInformation("📤 Enviando reserva para API Java - TipoUso: {TipoUso}, NumReserva: {NumReserva}, LoginPms: {LoginPms}, LoginSistemaVenda: {LoginSistemaVenda}", 
                reservaModel.TipoUso, reservaModel.Reserva, reservaModel.LoginPms, reservaModel.LoginSistemaVenda);

            if (reservaModel.Id == reservaModel.IdReservasFront && reservaModel.NumReserva.GetValueOrDefault(0) > 0)
                reservaModel.Id = reservaModel.NumReserva;


            await AjustarObservacoesReservaMvc(reservaModel);

            var baseUrl = _configuration.GetValue<string>("ReservasCMApiConfig:BaseUrl");
            var liberarPoolUrl = _configuration.GetValue<string>("ReservasCMApiConfig:CriarAlterarReserva");
            var fullUrl = baseUrl + liberarPoolUrl;
            var token = await _serviceBase.getToken();
            ReservaTsModel? reservaCriada = null;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(fullUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                HttpResponseMessage responseResult = await client.PostAsJsonAsync(fullUrl, reservaModel);

                string resultMessage = await responseResult.Content.ReadAsStringAsync();

                _logger.LogInformation(resultMessage);

                if (responseResult.IsSuccessStatusCode)
                {
                    var resultReservaCriada = System.Text.Json.JsonSerializer.Deserialize<ResultModel<ReservaTsModel>>(resultMessage, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (resultReservaCriada != null && resultReservaCriada.Data != null)
                    {
                        reservaCriada = resultReservaCriada.Data;
                    }
                }
                else
                {
                    throw new ArgumentException($"{resultMessage}");
                }

            }

            if (reservaCriada != null)
            { 
                _logger.LogInformation("✅ Reserva criada/alterada com sucesso - IdReservasFront: {IdReservasFront}, NumReserva: {NumReserva}", reservaCriada.IdReservasFront, reservaCriada.NumReserva);
            }

            return reservaCriada;
        }

        private async Task AjustarObservacoesReservaMvc(InclusaoReservaInputDto reservaModel)
        {
            if (reservaModel.ClienteReservante != null)
            {
                var pessoa = (await _repository.FindBySql<PessoaModel>($"Select p.IdPessoa, p.Nome From Pessoa p Where p.IdPessoa = {reservaModel.ClienteReservante}")).FirstOrDefault();
                if (pessoa != null && !string.IsNullOrEmpty(pessoa.Nome))
                {
                    var sb = new StringBuilder($@"RESERVA MVC OBSERVAÇÃO\r\n
                    CLIENTE MABU VACATION CLUB ({pessoa.Nome!.ToUpper()})*CAFÉ* 20% DESCONTO NO\r\n
                    ALMOÇO E JANTAR DO RESTAURANTE PRINCIPAL*TIROLESA 10% EXTRAS DIRETO\r\n
                    NO HOTEL TERÁ TODOS OS BENEFÍCIOS MABU VACATION CLUB 30% DE DESCONTO\r\n
                    NA ENTRADA DO BLUE PARK");

                    reservaModel.Observacao = sb.ToString();
                }
            }
        }

        private async Task SetarParametrosReserva(InclusaoReservaInputModel model)
        {
            if (string.IsNullOrEmpty(model.Segmento))
            {
                var segmentosReserva = _configuration.GetValue<string>("ReservasCMApiConfig:SegmentoReserva");
                if (segmentosReserva != null && !string.IsNullOrEmpty(segmentosReserva))
                {
                    var segmentosDoHotel = segmentosReserva.Contains("|") ? segmentosReserva.Split('|').Where(b => b.StartsWith($"HotelId:{model.IdHotel}")).AsList() : new List<string>();
                    foreach (var item in segmentosDoHotel)
                    {
                        var configSegmentoPorTipo = item.Contains(";") ? item.Split(';').FirstOrDefault(b => b.Contains("pontos", StringComparison.InvariantCultureIgnoreCase)) : "";
                        if (!string.IsNullOrEmpty(configSegmentoPorTipo) && configSegmentoPorTipo.Contains(":"))
                        {
                            model.Segmento = configSegmentoPorTipo.Split(':')[1];
                            break;
                        }
                    }

                }
            }

            var tiposDeHospedes = _configuration.GetValue<string>("ReservasCMApiConfig:TipoHospede");
            if (tiposDeHospedes != null && !string.IsNullOrEmpty(tiposDeHospedes))
            {
                var tipoHospede = tiposDeHospedes.Split('|').Where(b => b.StartsWith($"HotelId:{model.IdHotel}")).AsList();
                foreach (var item in tipoHospede)
                {
                    var tipoHospedeUtilizar = item.Contains(";") ? item.Split(';')[1] : "";
                    if (!string.IsNullOrEmpty(tipoHospedeUtilizar))
                    {
                        foreach (var itemHospede in model.Hospedes)
                        {
                            itemHospede.TipoHospede = tipoHospedeUtilizar.Contains(":") ? tipoHospedeUtilizar.Split(':')[1] : "";
                            itemHospede.DataCheckin = model.Checkin;
                            itemHospede.DataCheckout = model.Checkout;
                            itemHospede.IdTipoHospede = itemHospede.TipoHospede;
                        }
                        break;
                    }
                }

            }

            var tarifasHoteis = _configuration.GetValue<string>("ReservasCMApiConfig:TarifaHotel");
            if (tarifasHoteis != null && !string.IsNullOrEmpty(tarifasHoteis))
            {
                var tarifaHotel = tarifasHoteis.Split('|').Where(b => b.StartsWith($"HotelId:{model.IdHotel}")).FirstOrDefault();
                if (tarifaHotel != null && tarifaHotel.Contains(";") && Helper.IsNumeric(tarifaHotel.Split(';')[1]))
                    model.TipoTarifa = int.Parse(tarifaHotel.Split(';')[1]);
            }

            //"TarifaHotel":"HotelId:3;109|HotelId:1;1519"
            model.IdVeiculo = _configuration.GetValue<string>("ReservasCMApiConfig:VeiculoComunicacao");
            model.Origem = string.IsNullOrEmpty(model.Origem) ? _configuration.GetValue<string>("ReservasCMApiConfig:OrigemReserva") : model.Origem;
            model.MeioComunicacao = _configuration.GetValue<string>("ReservasCMApiConfig:MeioComunicacao");

            await Task.CompletedTask;
        }

        private async Task GravarResevasTs(int cmUserId, ReservaTsModel reservaCriada)
        {
            var reservasTsExistente = (await _repository.FindByHql<ReservasTs>($"From ReservasTs Where IdReservasFront = {reservaCriada.IdReservasFront}")).FirstOrDefault();
            bool inclusao = reservasTsExistente == null;
            //ReservaTs
            var reservaTs = reservasTsExistente ?? new ReservasTs()
            {
                IdReservasFront = reservaCriada.IdReservasFront,
                TrgDtInclusao = DateTime.Now,
                TrgUserInclusao = $"CM{cmUserId}"
            };

            reservaTs.IdReservasFront = reservaCriada.IdReservasFront;
            reservaTs.Inclusao = inclusao;

            await _repository.Save(reservaTs);
        }

        private async Task<VendaXContratoTs?> GetVendaXContrato(AtendClienteTs atendClienteTs)
        {
            return (await _repository.FindBySql<VendaXContratoTs>($@"SELECT 
                                    A.IDCLIENTE, 
                                    vc.IdVendaXContrato, 
                                    vc.IdContratoTs, 
                                    NVL(RC.DATAREVERSAO, V.DATAVENDA) AS DATA, 
                                    PJ.NUMEROPROJETO, VC.NUMEROCONTRATO,
                                    VC.FLGREVERTIDO, VC.FLGCANCELADO,
                                    TO_DATE(TO_CHAR(DECODE(C.FLGDTANIVERSARIO, 0, NVL(RC.DATAREVERSAO, V.DATAVENDA), NVL(VC.DATAINTEGRALIZA, P.DATASISTEMA)),'DD/MM/YYYY'),'DD/MM/YYYY') AS DATAANIVERSARIO,
                                    C.PERCTXMANUTPRIUTI, C.PERCPONTOSPRIUTI, C.PERCTXMANUTSEGUTI, C.PERCPONTOSSEGUTI, C.FLGUTILVLRPROP, C.FLGUTILPONTOSPROP, C.FLGSALDOINSUFICIENTE, C.FLGUTILALTATEMPANOCOMPRA,
                                    C.Validade, C.TipoValidade, C.IdTipoDcTaxa
                                    FROM ATENDCLIENTETS A, VENDATS V, VENDAXCONTRATOTS VC, PROJETOTS PJ, REVCONTRATOTS RC, CONTRATOTS C, PARAMTS P
                                    WHERE A.IDATENDCLIENTETS       = V.IDATENDCLIENTETS
                                       AND V.IDVENDATS              = VC.IDVENDATS
                                       AND VC.IDCONTRATOTS          = C.IDCONTRATOTS
                                       AND RC.IDVENDAXCONTRNOVO (+) = VC.IDVENDAXCONTRATO
                                       AND VC.IDPROJETOTS           = PJ.IDPROJETOTS
                                       AND VC.IDVENDAXCONTRATO      = {atendClienteTs!.IdVendaXContrato}
                                       AND A.IDHOTEL                = P.IDHOTEL")).FirstOrDefault();
        }

        private async Task<LancamentoTs> EfetuarLancamentoCriacaoReservaTs(VendaXContratoTs vendaXContrato, LancPontosTs lancPontosTs, ReservaTsModel reservaCriada, ParamTs paramTs, InclusaoReservaInputModel model, int cmUserId, int? contrTsXPontosUtilizar, DateTime? validadeCredito)
        {
            var lancamentoTs = new LancamentoTs()
            {
                Inclusao = true,
                IdLancPontosTs = lancPontosTs.IdLancPontosTs,
                IdTipoDebCred = vendaXContrato.IdTipoDcTaxa,
                IdHotel = paramTs.IdHotel.GetValueOrDefault(3),
                VlrLancamento = 0,
                VlrAPagar = 0,
                IdTipoLancamento = 5,
                DataLancamento = paramTs.DataSistema,
                DataPagamento = paramTs.DataSistema,
                Documento = " ",
                IdUsuario = cmUserId,
                ValidadeCredito = new DateTime(1899, 12, 30),
                FlgTaxaAdm = " "
            };

            await _repository.Save(lancamentoTs);

            return lancamentoTs;

        }

        private async Task<LancPontosTs> EfetuarLancamentoPontosTs(ReservaTsModel reservaCriada, 
            ParamTs paramTs, 
            InclusaoReservaInputModel model, 
            int cmUserId, 
            int? contrTsXPontosUtilizar, 
            DateTime? validadeCredito, 
            decimal pontosUtilizados,
            string flgAssociada = "N")
        {
            var lancPontosTs = new LancPontosTs()
            {
                Inclusao = true,
                IdTipoLancPontoTs = 1,
                IdVendaXContrato = model.IdVendaXContrato ?? reservaCriada.IdVendaXContrato.GetValueOrDefault(0),
                NumeroPontos = model.IdFracionamentoTs.GetValueOrDefault(0) > 0 ? 0 : model.NumeroPontos.GetValueOrDefault(pontosUtilizados),
                IdReservasFront = reservaCriada.IdReservasFront,
                IdHotel = paramTs.IdHotel.GetValueOrDefault(3),
                DebitoCredito = "D",
                IdUsuario = cmUserId,
                IdUsuarioLogado = cmUserId,
                IdUsuarioReserva = cmUserId,
                FlgMigrado = "N",
                FlgVlrManual = "N",
                IdContrXPontoCobrado = contrTsXPontosUtilizar != null ? contrTsXPontosUtilizar.GetValueOrDefault(0) : null,
                DataLancamento = paramTs.DataSistema,
                TrgDtInclusao = DateTime.Now,
                TrgUserInclusao = $"CM{cmUserId}",
                ValidadeCredito = validadeCredito,
                FlgAssociada = flgAssociada

            };

            if (reservaCriada != null && reservaCriada.IdReservasFront.GetValueOrDefault(0) > 0)
            {
                var tipoUso = (model.TipoUso ?? model.TipoDeUso) ?? "UP";
                await _repository.ExecuteSqlCommand($"Update ReservasFront Set TipoDeUso = '{tipoUso}' Where IdReservasFront = {reservaCriada.IdReservasFront.GetValueOrDefault(0)}");
            }

            await _repository.Save(lancPontosTs);

            return lancPontosTs;
        }

        private async Task<LancPontosTs?> EfetuarLancamentoPontosTsCancelamento(LancPontosTs lancPontosTsBase, ParamTs paramTs, int cmUserId)
        {
            if (lancPontosTsBase.FlgAssociada != "S")
            {
                var lancPontosTs = new LancPontosTs()
                {
                    Inclusao = true,
                    IdTipoLancPontoTs = 5,
                    IdVendaXContrato = lancPontosTsBase.IdVendaXContrato,
                    NumeroPontos = 0m,
                    IdReservasFront = lancPontosTsBase.IdReservasFront,
                    IdHotel = lancPontosTsBase.IdHotel,
                    DebitoCredito = "D",
                    IdUsuario = cmUserId,
                    IdUsuarioLogado = cmUserId,
                    IdUsuarioReserva = cmUserId,
                    FlgMigrado = "N",
                    FlgVlrManual = "N",
                    FlgAssociada = lancPontosTsBase.FlgAssociada,
                    IdContrXPontoCobrado = lancPontosTsBase.IdContrXPontoCobrado,
                    DataLancamento = paramTs.DataSistema,
                    TrgDtInclusao = DateTime.Now,
                    TrgUserInclusao = $"CM{cmUserId}",
                    ValidadeCredito = lancPontosTsBase.ValidadeCredito

                };

                await _repository.Save(lancPontosTs);
            }
            else
            {
                if (lancPontosTsBase == null)
                    throw new ArgumentException("Não foi possível reverter os lançamentos de pontos.");

                var lancamentoTs = (await _repository.FindByHql<LancamentoTs>($"From LancamentoTs Where IdLancPontosTs = {lancPontosTsBase.IdLancPontosTs}")).AsList();
                if (lancamentoTs != null && lancamentoTs.Any())
                {
                    _repository.RemoveRange(lancamentoTs);
                    await _repository.Remove(lancPontosTsBase);
                }
            }

            return null;
        }

        private async Task<ParamTs> GetParamHotel(int idHotel = 3)
        {
            var paramTs = (await _repository.FindBySql<ParamTs>($"Select p.* From ParamTs p Where p.IdHotel = {idHotel}")).FirstOrDefault();
            if (paramTs == null)
            {
                paramTs = (await _repository.FindBySql<ParamTs>($"Select p.* From ParamTs p Where p.IdHotel = 3 ")).FirstOrDefault();
                if (paramTs == null)
                throw new ArgumentException("Falha na criação de reserva: ParamTs");
            }

            return paramTs;
        }

        private async Task<AtendClienteTs?> GetAtendimentoCliente(int idVendaXContrato)
        {
            var atendClienteTs = (await _repository.FindBySql<AtendClienteTs>(@$"SELECT
                    vxc.IdVendaXContrato,
                    vxc.IdVendaTs,
                    v.IdAtendClienteTs,
                    aten.IdCliente,
                    vxc.NumeroContrato
                    FROM
                    VendaXContratoTs vxc
                    INNER JOIN VendaTs v ON vxc.IdVendaTs = v.IdVendaTs
                    INNER JOIN AtendClienteTs aten ON v.IdAtendClienteTs = aten.IdAtendClienteTs
                    WHERE
                    vxc.IdVendaXContrato = {idVendaXContrato}")).FirstOrDefault();

            if (atendClienteTs == null)
                throw new ArgumentException($"Não foi possível encontrar o cliente vinculado com a venda informada: {idVendaXContrato}");
            return atendClienteTs;
        }

        private async Task GravarLogs(int cmUserId, AtendClienteTs? atendClienteTs, ParamTs? paramTs)
        {
            var tipoLogTs = (await _repository.FindBySql<TipoLogTs>("Select * From TipoLogTs Where Lower(Descricao) = 'pós-venda'")).FirstOrDefault() ?? new TipoLogTs() { IdTipoLogTs = 2 };
            if (tipoLogTs != null)
            {
                var logTs = new LogTs()
                {
                    IdUsuario = cmUserId,
                    IdTipoLogTs = tipoLogTs.IdTipoLogTs,
                    DataSistema = paramTs?.DataSistema ?? DateTime.Today,
                    DataHora = DateTime.Now,
                    Chave = atendClienteTs?.IdCliente,
                    Inclusao = true
                };

                await _repository.Save(logTs);
            }
        }

        private async Task AjustarFracionamentoReserva(InclusaoReservaInputModel model, int cmUserId, AtendClienteTs? atendClienteTs, ParamTs paramTs, int diasReservaAtual, ReservaTsModel reservaCriada, ReservaTimeSharing reservaTimeSharingHistorico)
        {
            if (atendClienteTs == null)
                throw new ArgumentException("Deve ser infomado o parâmetro atendClienteTs");

            List<FracionamentoTsModel> fracionamentos = await GetFracionamentosCorrentes(atendClienteTs!.IdCliente.GetValueOrDefault(), paramTs);
            if (fracionamentos != null && fracionamentos.Any())
                fracionamentos = fracionamentos.Where(b => DateTime.Today.Subtract(b.DataLancamento.GetValueOrDefault()).Days <= 365).AsList();

            if (diasReservaAtual != 7)
            {
                if (model.IdFracionamentoTs.GetValueOrDefault(0) > 0)
                {
                    var fracionamentoEmAberto =  fracionamentos != null && fracionamentos.Any() ? fracionamentos.FirstOrDefault(a => a.IdFracionamentoTs == model.IdFracionamentoTs) : null;
                    if (fracionamentoEmAberto == null)
                        throw new ArgumentException($"Não foi encontrado o fracionamento com o Id informado: {model.IdFracionamentoTs}");

                    var qtdeDiasReservaUtilizadas = fracionamentoEmAberto.CheckoutReservasFront1.GetValueOrDefault()
                        .Date.Subtract(fracionamentoEmAberto.CheckinReservasFront1.GetValueOrDefault().Date).Days;

                    if ((qtdeDiasReservaUtilizadas + diasReservaAtual) > 7)
                    {
                        throw new ArgumentException($"A quantidade máxima para o fechamento do fracionamento é de: {(7 - qtdeDiasReservaUtilizadas)} pernoites/diárias.");
                    }

                    var aberturaFracionamento = (await _repository.FindByHql<FracionamentoTs>($"From FracionamentoTs Where IdFracionamentoTs = {fracionamentoEmAberto.IdFracionamentoTs}")).FirstOrDefault();
                    if (aberturaFracionamento != null)
                    {
                        aberturaFracionamento.IdReservasFront2 = reservaCriada.IdReservasFront.GetValueOrDefault(reservaCriada.Id.GetValueOrDefault());
                        aberturaFracionamento.Inclusao = false;
                        await _repository.Save(aberturaFracionamento);

                        reservaTimeSharingHistorico.FracionamentoIdFinalizado = aberturaFracionamento.IdFracionamentoTs;
                        reservaTimeSharingHistorico.PontosUtilizados = 0;
                        await _repositorySystem.Save(reservaTimeSharingHistorico);
                    }
                }
                else
                {
                    await GravarAberturaFracionamento(cmUserId, atendClienteTs, reservaCriada, reservaTimeSharingHistorico);

                }
            }
        }

        private async Task GravarAberturaFracionamento(int cmUserId, AtendClienteTs atendClienteTs, ReservaTsModel reservaCriada, ReservaTimeSharing reservaTimeSharingHistorico)
        {
            var novoFracinamento = new FracionamentoTs()
            {
                IdReservasFront1 = reservaCriada.IdReservasFront.GetValueOrDefault(reservaCriada.Id.GetValueOrDefault()),
                TrgDtInclusao = DateTime.Now,
                TrgUserInclusao = $"CM{cmUserId}",
                IdVendaXContrato = atendClienteTs!.IdVendaXContrato.GetValueOrDefault(),
                Inclusao = true
            };

            await _repository.Save(novoFracinamento);

            reservaTimeSharingHistorico.FracionamentoIdCriado = novoFracinamento.IdFracionamentoTs;
            await _repositorySystem.Save(reservaTimeSharingHistorico);
        }

        private async Task<List<FracionamentoTsModel>> GetFracionamentosCorrentes(int idCliente, ParamTs paramTs)
        {
            return (await _repository.FindBySql<FracionamentoTsModel>(@$"(SELECT 
                         VC.IDVENDAXCONTRATO, 
                         F.IdFracionamentoTs, 
                         R.IdReservasFront AS IdReservasFront1,
                         NVL(R.DataChegadaReal,R.DataChegPrevista) AS CheckinReservasFront1,
                         NVL(R.DataPartidaReal,R.DataPartPrevista) AS CheckoutReservasFront1,
                         R.StatusReserva as StatusReservasFront1,
                         Nvl(R.Adultos,0)+Nvl(R.CRIANCAS1,0)+Nvl(R.CRIANCAS2,0) AS QtdePessoas,
                         R.IdHotel,
                         R.NumReserva as NumReserva1,
                         A.IdCliente,
                         F.TrgDtInclusao as DataLancamento,
                         R.IDHOTEL as HotelId
                        FROM  
                        FRACIONAMENTOTS F, 
                        RESERVASFRONT R, 
                        VENDAXCONTRATOTS VC, 
                        CONTRATOTS C, 
                        ATENDCLIENTETS A
                        WHERE  F.IDRESERVASFRONT1 = R.IDRESERVASFRONT
                        AND    VC.IDATENDCLIENTETS = A.IDATENDCLIENTETS
                        AND    F.IDRESERVASFRONT2 IS NULL
                        AND    R.STATUSRESERVA    <> 6
                        AND    NVL(C.PERCPONTOSSEGUTI,0) = 0
                        AND    F.IDVENDAXCONTRATO = VC.IDVENDAXCONTRATO
                        AND    VC.IDCONTRATOTS    = C.IDCONTRATOTS
                        AND    (R.DATACHEGPREVISTA + NVL(C.NUMMAXDIASFECFRAC,0)) >= :dataSistema
                        AND    A.IDCLIENTE        = {idCliente} )
                        UNION
                        (SELECT 
                         VC.IDVENDAXCONTRATO, 
                         F.IdFracionamentoTs, 
                         R.IdReservasFront AS IdReservasFront1,
                         NVL(R.DataChegadaReal,R.DataChegPrevista) AS CheckinReservasFront1,
                         NVL(R.DataPartidaReal,R.DataPartPrevista) AS CheckoutReservasFront1,
                         R.StatusReserva as StatusReservasFront1,
                         Nvl(R.Adultos,0)+Nvl(R.CRIANCAS1,0)+Nvl(R.CRIANCAS2,0) AS QtdePessoas,
                         R.IdHotel,
                         R.NumReserva as NumReserva1,
                         A.IdCliente,
                         F.TrgDtInclusao as DataLancamento,
                         R.IDHOTEL as HotelId
                        FROM   FRACIONAMENTOTS F, RESERVASFRONT R, VENDAXCONTRATOTS VC, CONTRATOTS C, ATENDCLIENTETS A 
                        WHERE  F.IDRESERVASFRONT2 = R.IDRESERVASFRONT
                        AND    F.IDVENDAXCONTRATO = VC.IDVENDAXCONTRATO
                        AND    VC.IDATENDCLIENTETS = A.IDATENDCLIENTETS
                        AND    VC.IDCONTRATOTS    = C.IDCONTRATOTS
                        AND    R.STATUSRESERVA    = 6
                        AND    NVL(C.PERCPONTOSSEGUTI,0) = 0
                        AND    (R.DATACHEGPREVISTA + NVL(C.NUMMAXDIASFECFRAC,0)) >= :dataSistema
                        AND    (R.DATACHEGPREVISTA - R.DATACANCELAMENTO) >= C.NUMMINDIASCANCRES
                        AND    A.IDCLIENTE        = {idCliente})", new Parameter("dataSistema", paramTs!.DataSistema.GetValueOrDefault().Date))).AsList();
        }

        private async Task GravarLogByType(int cmUserId, int moduloId, string logTipo)
        {
            var logAcessoSis = new LogAcessoSis()
            {
                IdUsuario = cmUserId,
                IdModulo = moduloId,
                FlgOperacao = logTipo,
                Inclusao = true
            };

            await _repository.Save(logAcessoSis);
        }

        public async Task<bool?> CancelarReserva(CancelarReservaTsModel model)
        {
            if (model.ReservaTimesharingId.GetValueOrDefault(0) == 0 &&
                model.ReservaId.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informada a reserva que deseja ser cancelada.");

            _repositorySystem.BeginTransaction();
            _repository.BeginTransaction();
            bool? cancelada = false;
            
            try
            {
                cancelada = await CancelarReservaExecute(model, cancelada);

                var commitResult = await _repositorySystem.CommitAsync();
                if (commitResult.executed)
                {
                    var commitResultPortal = await _repository.CommitAsync();
                }


            }
            catch (Exception err)
            {
                _repository.Rollback();
                _repositorySystem.Rollback();
                throw new ArgumentException(err.Message);
            }

            return cancelada;
        }

        private async Task<bool?> CancelarReservaExecute(CancelarReservaTsModel model, bool? cancelada)
        {
            if (model.ReservaId.GetValueOrDefault(0) > 0)
            {
                var reserva = (await _repository.FindBySql<ReservaTsModel>($"Select Nvl(rf.TipoDeUso,'UP') AS TipoDeUso, Nvl(rf.DataChegPrevista,rf.DataChegadaReal) as DataCheckin, rf.* From ReservasFront rf Where rf.NumReserva = {model.ReservaId}")).FirstOrDefault();
                if (reserva == null)
                    throw new ArgumentException("Reserva não encontrada");
                if (reserva.DataCheckin.GetValueOrDefault().Date.Subtract(DateTime.Today.Date).Days < 30)
                    throw new ArgumentException("A reserva não pode ser cancelada, está fora do prazo de cancelamento: (30)");

                var fracionamentoTs = (await _repository.FindByHql<FracionamentoTs>($"From FracionamentoTs fr Where fr.IdReservasFront1 = {reserva.IdReservasFront} or fr.IdReservasFront2 = {reserva.IdReservasFront}")).FirstOrDefault();
                if (fracionamentoTs != null)
                {
                    if (reserva.IdReservasFront == fracionamentoTs.IdReservasFront1.GetValueOrDefault() && fracionamentoTs.IdReservasFront2.GetValueOrDefault(0) > 0)
                        throw new ArgumentException("A reserva de abertura de fracionamento não pode ser cancelada, quando já possuir reserva de fechamento vinculada.");
                }

                cancelada = await CancelarReservaAPICM(model);
                if (cancelada.GetValueOrDefault(false))
                {
                    await ReverterLancamentosTimeSharing(reserva.IdReservasFront);
                }
            }
            else
            {
                var reservaTimeSharing = (await _repositorySystem.FindByHql<ReservaTimeSharing>($"From ReservaTimeSharing Where Id = {model.ReservaTimesharingId}")).FirstOrDefault();
                if (reservaTimeSharing == null)
                    throw new ArgumentException("Reserva timesharing não encontrada");

                if (reservaTimeSharing.IdReservasFront.GetValueOrDefault(0) > 0 || !string.IsNullOrEmpty(reservaTimeSharing.NumReserva))
                {
                    var reserva = reservaTimeSharing.IdReservasFront.GetValueOrDefault(0) == 0 ? (await _repository.FindBySql<ReservaTsModel>(@$"Select 
                                    Nvl(rf.TipoDeUso,'UP') AS TipoDeUso,    
                                    rf.* 
                                    From 
                                        ReservasFront rf 
                                    Where 
                                        r.NumReserva = {reservaTimeSharing.NumReserva} and 
                                        Exists(Select rc.IdReservasFront From ReservasRci rc Where rc.IdReservasFront = rf.IdReservasFront) ")).FirstOrDefault() : null;

                    if (reserva != null && reservaTimeSharing.IdReservasFront.GetValueOrDefault(0) == 0)
                        reservaTimeSharing.IdReservasFront = reserva.IdReservasFront;

                    await ReverterLancamentosTimeSharing(reservaTimeSharing.IdReservasFront);

                }

                if (reservaTimeSharing != null && (string.IsNullOrEmpty(reservaTimeSharing.StatusCM) ||
                    (!reservaTimeSharing.StatusCM.Contains("Cancelada", StringComparison.CurrentCultureIgnoreCase))))
                {
                    reservaTimeSharing.StatusCM = "Cancelada";
                    reservaTimeSharing.NumReserva = null;
                    reservaTimeSharing.IdReservasFront = null;
                    reservaTimeSharing.MotivoCancelamentoInfUsu = model.MotivoCancelamentoInfUsu ?? model.ObservacaoCancelamento;
                    reservaTimeSharing.ClienteNotificadoCancelamento = model.NotificarCliente.GetValueOrDefault(false) ? Domain.Enumns.EnumSimNao.Sim : Domain.Enumns.EnumSimNao.Não;
                    await _repositorySystem.Save(reservaTimeSharing);

                    cancelada = true;
                }
            }

            return cancelada;
        }

        public async Task<ParametroSistemaViewModel?> GetParametroSistema()
        {
            return await _serviceBase.GetParametroSistema();
        }

        private async Task ReverterLancamentosTimeSharing(int? idReservasFront)
        {
            if (idReservasFront.GetValueOrDefault(0) > 0)
            {
                ParamTs paramTs = await GetParamHotel(3);
                var cmUserId = _configuration.GetValue<int>("CMUserId", 1900693);

                var lancPontosTs = (await _repository.FindByHql<LancPontosTs>($"From LancPontosTs lp Where lp.IdReservasFront = {idReservasFront.GetValueOrDefault()}")).AsList();
                if (lancPontosTs.Any(b=> b.IdTipoLancPontoTs == 1 && !lancPontosTs.Any(c=> c.IdTipoLancPontoTs == 5)))
                {
                    LancPontosTs lancPontosTsCancelamento = await EfetuarLancamentoPontosTsCancelamento(lancPontosTs.First(a=> a.IdTipoLancPontoTs == 1), paramTs, cmUserId);
                }

                var fracionamentoTs = (await _repository.FindByHql<FracionamentoTs>($"From FracionamentoTs fr Where fr.IdReservasFront1 = {idReservasFront} or fr.IdReservasFront2 = {idReservasFront}")).FirstOrDefault();
                if (fracionamentoTs != null)
                {
                    if (fracionamentoTs.IdReservasFront2.GetValueOrDefault(0) > 0)
                        throw new ArgumentException("A reserva de abertura de fracionamento não pode ser cancelada, quando já possuir reserva de fechamento vinculada.");

                    
                    await _repository.Remove(fracionamentoTs);
                }

                var reservaTs = (await _repository.FindByHql<ReservasTs>($"From ReservasTs fr Where fr.IdReservasFront = {idReservasFront}")).FirstOrDefault();
                if (reservaTs != null)
                {
                    await _repository.Remove(reservaTs);
                }

                var reservaRci = (await _repository.FindByHql<ReservasRci>($"From ReservasRci fr Where fr.IdReservasFront = {idReservasFront}")).FirstOrDefault();
                if (reservaRci != null)
                {
                    await _repository.Remove(reservaRci);
                }

            }
        }

        public async Task<ReservaTimeSharingCMModel?> Editar(long numreserva)
        {
            try
            {

                var reservasFront = (await _repository.FindBySql<ReservaTimeSharingCMModel>(@"Select 
                    rf.IdHotel,
                    rf.IdReservasFront as Id,
                    rf.IdReservasFront,
                    ph.Nome as NomeHotel,
                    ori.IdOrigem AS IdOrigemReserva,
                    ori.DESCRICAO AS OrigemReserva,
                    sr.Descricao AS StatusReserva,
                    us.IDUSUARIO AS Usuario,
                    us.NOMEUSUARIO,
                    cp.IDPESSOA AS ClienteReservante,
                    cppe.Nome AS ClienteReservanteNome,
                    th.IDTIPOUH AS TipoUhEstadia,
                    th.IDTIPOUH AS IdTipoUh,
                    'MA - MASTER' AS NomeTipoUhEstadia,
                    rf.RESERVANTE AS Reservante,
                    rf.TELRESERVANTE AS TelefoneReservante,
                    rf.EMAILRESERVANTE,
                    rf.CODUH,
                    rf.IDTARIFA,
                    tar.DESCRICAO AS NomeTarifa,
                    seg.DESCRICAO AS SegmentoReserva,
                    m.DESCRICAO AS MeioComunicacao,
                    rf.CONTRATOINICIAL,
                    rf.CONTRATOFINAL,
                    CAST(rf.CONTRATOINICIAL AS VARCHAR2(50)) AS NumeroContrato,
                    lp.IdVendaXContrato,
                    lp.NumeroContrato,
                    rf.DATACHEGPREVISTA AS DataChegadaPrevista,
                    rf.DATACHEGADAREAL,
                    rf.DATAPARTPREVISTA AS DataPartidaPrevista,
                    rf.DATAPARTIDAREAL,
                    rf.ADULTOS,
                    rf.CRIANCAS1,
                    rf.CRIANCAS2,
                    ABS(lp.PONTOS) AS PontosDebitados,
                    rf.CODPENSAO,
                    rf.CODPENSAO AS Pensao,
                    rf.DataReserva,
                    rf.OBSERVACOES,
                    rf.DATACONFIRMACAO,
                    rf.DATACANCELAMENTO,
                    rf.DOCUMENTO,
                    rf.NUMRESERVA,
                    rf.DATACANCELAMENTO,
                    rf.OBSCANCELAMENTO,
                    rf.DATAREATIVACAO,
                    rf.FLGDIARIAFIXA,
                    rf.VLRDIARIAMANUAL,
                    rf.TrgDtInclusao,
                    rf.TrgUserInclusao,
                    Nvl(rf.TipoDeUso,'UP') AS TipoDeUso
                    From 
                    ReservasFront rf
                    Inner Join Hotel h on rf.IdHotel = h.IdHotel
                    Inner Join Pessoa ph on h.IdPessoa = ph.IdPessoa
                    INNER JOIN StatusReserva sr ON rf.STATUSRESERVA = sr.STATUSRESERVA
                    LEFT JOIN UsuarioSistema us ON rf.USUARIO = us.IDUSUARIO 
                    LEFT JOIN OrigemReserva ori ON rf.IDORIGEM = ori.IDORIGEM
                    LEFT JOIN ClientePess cp ON cp.IDPESSOA = rf.CLIENTERESERVANTE 
                    LEFT JOIN Pessoa cppe ON cp.IDPESSOA = cppe.IDPESSOA 
                    LEFT JOIN TipoUh th ON rf.TIPOUHESTADIA = th.IDTIPOUH AND th.IDHOTEL = rf.IDHOTEL
                    LEFT JOIN TARIFAHOTEL tar ON rf.IDTARIFA = tar.IDTARIFA AND rf.IDHOTEL = tar.IDHOTEL
                    LEFT JOIN Segmento seg ON rf.CODSEGMENTO = seg.CODSEGMENTO AND rf.IDHOTEL = seg.IDHOTEL 
                    LEFT JOIN MEIOSCOMUNICACAO m ON rf.IDMEIOCOMUNICACAO = m.IDMEIOCOMUNICACAO 
                    LEFT JOIN (
                        SELECT lp.IdReservasFront, 
                        lp.NumeroPontos AS Pontos,
                        lp.IdVendaXContrato,
                        v.NumeroContrato
                        FROM LancPontosTs lp
                        Inner Join VendaXContratoTs v on lp.IdVendaXContrato = v.IdVendaXContrato
                        WHERE IdTipoLancPontoTs = 1
                    ) lp ON lp.IdReservasFront = rf.IdReservasFront
                    LEFT JOIN ReservasTs rt ON rt.IdReservasFront = rf.IdReservasFront
                    WHERE
                    exists(Select rt2.IdReservasFront From ReservasTs rt2 Where rt2.IdReservasFront = rf.IdReservasFront)
                    and rf.NumReserva =:numReserva", new Parameter("numReserva", numreserva))).FirstOrDefault();

                if (reservasFront != null)
                {
                    await GetHospedes(reservasFront);
                }

                return reservasFront;

            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                throw err;

            }
        }

        private async Task GetHospedes(ReservaTimeSharingCMModel? reservasFront)
        {
            if (reservasFront == null || reservasFront.IdReservasFront.GetValueOrDefault(0) == 0) return;

            reservasFront.Hospedes = new List<HospedeInputModel>();

            var hospedes = (await _repository.FindBySql<HospedeInputModel>(@"SELECT 
                    mh.IDRESERVASFRONT,
                    h.IdHospede AS Id,
                    h.IdHospede,
                    h.IDHOSPEDE AS ClienteId,
                    mh.PRINCIPAL,
                    th.Descricao,
                    p.NOME,
                    h.DATANASCIMENTO,
                    p.IDDOCUMENTO,
                    tdp.NOMEDOCUMENTO AS TipoDocumento,
                    p.NumDocumento as Documento,
                    p.EMAIL,
                    pf.SEXO,
                    Nvl(mh.DATACHEGPREVISTA,mh.DATACHEGREAL) AS CHECKIN,
                    Nvl(mh.DATAPARTPREVISTA,mh.DATAPARTREAL) AS DataCheckout
                    FROM 
                    MovimentoHospedes mh
                    INNER JOIN Hospede h ON mh.IdHospede = h.IdHospede
                    INNER JOIN Pessoa p ON h.IDHOSPEDE = p.IDPESSOA
                    LEFT JOIN PessoaFisica pf ON p.IdPessoa = pf.IdPessoa
                    LEFT JOIN TipoHospede th ON mh.IDTIPOHOSPEDE = th.IDTIPOHOSPEDE AND mh.IDHOTEL = th.IDHOTEL 
                    LEFT JOIN TipoDocPessoa tdp ON p.IDDOCUMENTO = tdp.IDDOCUMENTO
                    WHERE 
                    mh.IDRESERVASFRONT  = :idReservasFront", new Parameter("idReservasFront", reservasFront.IdReservasFront.GetValueOrDefault()))).AsList();

            reservasFront.Hospedes = hospedes.AsList();


            await PopularOutrosDados(reservasFront.Hospedes);
        }

        private async Task PopularOutrosDados(List<HospedeInputModel> hospedes)
        {
            var cidadesSistemaCache = await _cacheStore.GetAsync<List<CidadeModel>>("CidadesSistema", 0, _repositorySystem.CancellationToken);
            var cidadesLegadoCache = await _cacheStore.GetAsync<List<CidadeModel>>("CidadesLegado", 0, _repository.CancellationToken);
            var tiposDocumentos = (await _repositorySystem.FindByHql<TipoDocumentoPessoa>("From TipoDocumentoPessoa")).AsList();

            var cidadesSistema = cidadesSistemaCache != null && cidadesSistemaCache.Any() ?
                cidadesSistemaCache : new List<CidadeModel>();

            if (!cidadesSistema.Any())
            {
                cidadesSistema = (await _repositorySystem.FindBySql<CidadeModel>($@"select 
                c.Id,
                c.nome,
                e.Sigla as EstadoSigla,
                e.Nome as EstadoNome,
                c.codigoibge
                from
                Cidade c 
                inner join Estado e on c.Estado = e.Id ")).AsList();

                await _cacheStore.AddAsync("CidadesSistema", cidadesSistema, DateTimeOffset.Now.AddMinutes(10), 0, _repositorySystem.CancellationToken);


            }

            var cidadesLegado = cidadesLegadoCache != null && cidadesLegadoCache.Any() ?
                cidadesLegadoCache : new List<CidadeModel>();

            if (!cidadesLegado.Any())
            {

                cidadesLegado = (await _repository.FindBySql<CidadeModel>($@"Select
                        c.IdCidades AS Id,
                        c.Nome,
                        c.CodMunicipio AS CodigoIbge,
                        e.CodEstado as EstadoSigla,
                        e.NomeEstado as EstadoNome
                        FROM 
                        Cidades c
                        INNER JOIN Estado e ON c.CodEstado = e.CodEstado AND c.IdPais = e.IdPais ")).AsList();


                await _cacheStore.AddAsync("CidadesLegado", cidadesLegado, DateTimeOffset.Now.AddMinutes(10), 0, _repositorySystem.CancellationToken);

            }

            if (!hospedes.Any()) return;

            foreach (var hospedeItem in hospedes.Where(a => !string.IsNullOrEmpty(a.Documento) && !string.IsNullOrEmpty(a.TipoDocumento)))
            {
                var tipoDocumentoSystem = tiposDocumentos.FirstOrDefault(a => !string.IsNullOrEmpty(a.Nome) &&
                !string.IsNullOrEmpty(hospedeItem.TipoDocumento) && a.Nome.RemoveAccents().Equals(hospedeItem.TipoDocumento));

                if (tipoDocumentoSystem != null)
                    hospedeItem.TipoDocumentoId = tipoDocumentoSystem.Id;
            }

            var endPess = (await _repository.FindBySql<EndPess>(@$"Select 
            ep.IdEndereco,
            ep.IdPessoa,
            ep.IdCidades,
            ep.Logradouro,
            ep.Numero,
            ep.Complemento,
            ep.Bairro,
            ep.Cep,
            ep.TipoEndereco,
            ep.Nome,
            FlgTipoEnd
            From 
            EndPess ep 
            Where 
            ep.IdPessoa in ({string.Join(",", hospedes.Select(a => a.IdHospede))})")).AsList();

            List<TelEndPess> telendPess = new List<TelEndPess>();


            if (endPess != null && endPess.Any())
            {
                telendPess = (await _repository.FindBySql<TelEndPess>(@$"Select 
                    tp.IdTelefone,
                    tp.IdEndereco,
                    tp.Ddi,
                    tp.Ddd,
                    tp.Numero,
                    tp.Tipo
                    From 
                    TelEndPess tp 
                    Where 
                    tp.IdEndereco in ({string.Join(",", endPess.Select(a => a.IdEndereco))})")).AsList();
            }

            if (endPess != null && endPess.Any())
            {
                foreach (var item in endPess.GroupBy(a => a.IdCidades))
                {
                    var fst = item.First();
                    var cidadeLegado = cidadesLegado.FirstOrDefault(a => a.Id == fst.IdCidades);
                    if (cidadeLegado != null && !string.IsNullOrEmpty(cidadeLegado.Nome) && !string.IsNullOrEmpty(cidadeLegado.EstadoSigla))
                    {
                        var cidadeSystem = cidadesSistema.FirstOrDefault(a => !string.IsNullOrEmpty(a.Nome) && a.EstadoSigla != null &&
                            !string.IsNullOrEmpty(a.EstadoSigla) &&
                            (a.Nome.RemoveAccents().Contains(cidadeLegado.Nome, StringComparison.CurrentCultureIgnoreCase) &&
                            a.EstadoSigla.RemoveAccents() == cidadeLegado.EstadoSigla) ||
                            (!string.IsNullOrEmpty(cidadeLegado.CodigoIbge) && !string.IsNullOrEmpty(a.CodigoIbge) &&
                            cidadeLegado.CodigoIbge == a.CodigoIbge));

                        if (cidadeSystem != null)
                        {
                            foreach (var endereco in item)
                            {
                                endereco.IdCidades = cidadeSystem.Id;
                                endereco.CodigoIbge = cidadeSystem.CodigoIbge;
                                endereco.CidadeNome = cidadeSystem.Nome;
                                endereco.EstadoSigla = cidadeSystem.EstadoSigla;
                                endereco.CidadeUf = $"{cidadeSystem.Nome}/{cidadeSystem.EstadoSigla}";
                            }
                        }
                        else
                        {
                            foreach (var endereco in item)
                            {
                                endereco.IdCidades = null;
                            }
                        }

                    }

                }

                foreach (var hospedeItem in hospedes)
                {
                    var endereco = endPess.FirstOrDefault(a => a.IdPessoa == hospedeItem.IdHospede && a.IdCidades.GetValueOrDefault(0) > 0) ??
                        endPess.FirstOrDefault(a => a.IdPessoa == hospedeItem.IdHospede);

                    var telefone = endereco != null ? telendPess.FirstOrDefault(a => a.IdEndereco == endereco.IdEndereco) : null;

                    if (endereco != null && endereco.IdCidades.GetValueOrDefault(0) > 0)
                    {
                        hospedeItem.CidadeId = endereco.IdCidades.GetValueOrDefault();
                        hospedeItem.Bairro = endereco.Bairro;
                        hospedeItem.Logradouro = endereco.Logradouro;
                        hospedeItem.Numero = endereco.Numero;
                        hospedeItem.CEP = endereco.Cep;
                        hospedeItem.CidadeNome = endereco.CidadeNome;
                        hospedeItem.SiglaEstado = endereco.EstadoSigla;
                        hospedeItem.Estrangeiro = 0;
                        endereco.CidadeUf = $"{endereco.CidadeNome}/{endereco.EstadoSigla}";

                    }

                    if (telefone != null)
                    {
                        hospedeItem.DDI = telefone.Ddi;
                        hospedeItem.DDD = telefone.Ddd;
                        hospedeItem.Numero = telefone.Numero;
                        hospedeItem.Estrangeiro = 0;
                    }

                }

            }


        }

        public async Task<List<HotelModel>> HoteisVinculados()
        {
            var parametros = await _serviceBase.GetParametroSistema();
            if (parametros == null || string.IsNullOrEmpty(parametros.ExibirFinanceirosDasEmpresaIds)) return new List<HotelModel>();

            var hoteisRetornar = await _repository.FindBySql<HotelModel>(@$"SELECT p.Nome AS HotelNome,
                h.IdHotel as HotelId
                FROM Hotel h
                INNER JOIN Pessoa p ON h.IdPessoa = p.IdPessoa
                WHERE h.IdPessoa IN ({parametros.ExibirFinanceirosDasEmpresaIds})
                ORDER BY p.Nome");

            return hoteisRetornar.AsList();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ReservaRciModel> reservas)?> GetReservasRci(SearchReservasRciModel searchModel)
        {

            var contratosRci = await _cacheStore.GetAsync<List<ContratoAssociacaoRCIModel>>(ASSOCIACAO_RCI_CACHE_KEY, 0, _repositorySystem.CancellationToken);
            if (contratosRci == null || !contratosRci.Any())
            {
                contratosRci = 
                    (await _repository.FindBySql<ContratoAssociacaoRCIModel>(@$"SELECT
                        ap.IdPessoa AS IdCliente,
                        p.Nome AS NomeCliente,
                        ap.VALORCHAR AS IdRCI 
                        FROM 
                        Pessoaxatributo ap 
                        INNER JOIN Pessoa p ON ap.IdPessoa = p.IdPessoa
                        WHERE 
                        ap.idatributopessoa = 10 AND 
                        ap.VALORCHAR IS NOT NULL AND LENGTH(ap.VALORCHAR) > 1 ")).AsList();

                await _cacheStore.AddAsync(ASSOCIACAO_RCI_CACHE_KEY, contratosRci, DateTimeOffset.Now.AddMinutes(10), 0, _repositorySystem.CancellationToken);
            }

            var parameters = new List<Parameter>();
            var query = new StringBuilder($@"Select 
                                                rc.DataHoraCriacao as TrgDtInclusao,
                                                rc.Id as IdReservasRci,
                                                rc.NumeroContrato,
                                                rc.NumReserva as NumeroReserva,
                                                rc.StatusCM,
                                                p.Nome as UsuarioAlteracao,
                                                rc.DataHoraAlteracao as DataAlteracao,
                                                rc.NomeCliente,
                                                Coalesce(rc.ClienteNotificadoCancelamento,0) as ClienteNotificadoCancelamento,
                                                rc.ClienteReservante,
                                                Coalesce(p.EmailPreferencial, p.EmailAlternativo) as EmailCliente
                                             From
                                                ReservaTimeSharing rc
                                                Left Outer Join Usuario u on Coalesce(rc.UsuarioVinculacao, rc.UsuarioAlteracao) = u.Id
                                                Left Outer Join Pessoa p on u.Pessoa = p.Id
                                             Where 
                                                Upper(rc.TipoUtilizacao) like '%RCI%INTERCAMBIADORA%' ");

            if (!string.IsNullOrEmpty(searchModel.StatusCM))
            {
                query.AppendLine($" AND Lower(rc.StatusCM) = :statusVinculacao");
                parameters.Add(new Parameter("statusVinculacao", searchModel.StatusCM.ToLower()));
            }

            if (!string.IsNullOrEmpty(searchModel.NumeroContrato))
            {
                query.AppendLine($" AND rc.NumeroContrato like :numeroContrato");
                parameters.Add(new Parameter("numeroContrato", "%" + searchModel.NumeroContrato + "%"));
            }

            if (!string.IsNullOrEmpty(searchModel.NomeCliente))
            {
                query.AppendLine($" AND (Lower(rc.NomeCliente) like :nomeCliente or Lower(p.Nome) like :nomeCliente) ");
                parameters.Add(new Parameter("nomeCliente", "%" + searchModel.NomeCliente.ToLower() + "%"));
            }

            if (searchModel.DataCriacaoInicial.HasValue)
            {
                query.AppendLine($" AND rc.DataHoraCriacao >= :dataCriacaoInicial");
                parameters.Add(new Parameter("dataCriacaoInicial", searchModel.DataCriacaoInicial.Value));
            }

            if (searchModel.DataCriacaoFinal.HasValue)
            {
                query.AppendLine($" AND rc.DataHoraCriacao <= :dataCriacaoFinal");
                parameters.Add(new Parameter("dataCriacaoFinal", searchModel.DataCriacaoFinal.Value));
            }

            query.AppendLine(" ORDER BY rc.Id ");

            var reservas = await _repositorySystem.FindBySql<ReservaRciModel>(query.ToString(),searchModel.QuantidadeRegistrosRetornar,searchModel.NumeroDaPagina, parameters.ToArray());

            foreach (var item in reservas)
            {
                var codigoRci = contratosRci!.FirstOrDefault(a => a.IdCliente == item.ClienteReservante);
                if (codigoRci != null && !string.IsNullOrEmpty(codigoRci.IdRCI))
                {
                    item.IdRCI = codigoRci.IdRCI;
                }
            }

            return (searchModel.NumeroDaPagina,searchModel.QuantidadeRegistrosRetornar, reservas.AsList());

        }

        public async Task<DadosImpressaoVoucherResultModel?> GetDadosImpressaoVoucher(long numReserva)
        {
            SearchReservasGeralModel searchModel = new SearchReservasGeralModel
            {
                NumReserva = checked((int)numReserva),
                ExibirTodosOsHospedes = true,
                QuantidadeRegistrosRetornar = 1
            };

            var result = await GetReservasGeral(searchModel);
            if (result == null || !result.Value.reservas.Any()) 
            {
                throw new ArgumentException($"Reserva {numReserva} não foi encontrada para obter os dados do voucher!");            
            }

            var reserva = result.Value.reservas.First();

            // Buscar todos os hóspedes da reserva
            var hospedesParams = new List<Parameter>
            {
                new Parameter("idReservasFront", reserva.IdReservasFront.Value)
            };

            var hospedesSql = @"SELECT 
                                    H.NOME || ' ' || H.SOBRENOME AS NOME,
                                    PH.NUMDOCUMENTO AS DOCUMENTO,
                                    CASE WHEN M.PRINCIPAL = 'S' THEN 1 ELSE 0 END AS PRINCIPAL,
                                    CASE WHEN PRO.IDPESSOA = PH.IDPESSOA THEN 1 ELSE 0 END AS PROPRIETARIO,
                                    R.ADULTOS, R.CRIANCAS1, R.CRIANCAS2
                                FROM MOVIMENTOHOSPEDES M
                                INNER JOIN RESERVASFRONT R ON R.IDRESERVASFRONT = M.IDRESERVASFRONT
                                INNER JOIN HOSPEDE H ON H.IDHOSPEDE = M.IDHOSPEDE
                                INNER JOIN PESSOA PH ON PH.IDPESSOA = H.IDHOSPEDE
                                LEFT JOIN (SELECT DISTINCT R.IDRESERVASFRONT, A.IDCLIENTE AS IDPESSOA
                                FROM LANCPONTOSTS LP, VENDAXCONTRATOTS VC, RESERVASFRONT R, VENDATS V, ATENDCLIENTETS A
                                WHERE LP.IDRESERVASFRONT = R.IDRESERVASFRONT
                                AND LP.IDVENDAXCONTRATO = VC.IDVENDAXCONTRATO
                                AND V.IDVENDATS = VC.IDVENDATS
                                AND V.IDATENDCLIENTETS = A.IDATENDCLIENTETS) PRO ON PRO.IDRESERVASFRONT = M.IDRESERVASFRONT
                                WHERE M.IDRESERVASFRONT = :idReservasFront
                                ORDER BY CASE WHEN M.PRINCIPAL = 'S' THEN 0 ELSE 1 END, H.NOME";

            var hospedesResult = await _repository.FindBySql<HospedeVoucherTemp>(hospedesSql, hospedesParams.ToArray());

            var hospedes = new List<VoucherHospedeModel>();
            foreach (var h in hospedesResult)
            {
                hospedes.Add(new VoucherHospedeModel
                {
                    Nome = h.NOME,
                    Documento = h.DOCUMENTO,
                    Principal = h.PRINCIPAL.HasValue && h.PRINCIPAL.Value == 1,
                    Proprietario = h.PROPRIETARIO.HasValue && h.PROPRIETARIO.Value == 1
                });
            }

            // Buscar documento do hóspede principal
            var hospedePrincipal = hospedes.FirstOrDefault(h => h.Principal);
            var hospedePrincipalDocumento = hospedePrincipal?.Documento;

            var qtdePorFaixa = "";
            if (reserva.Adultos.HasValue)
                qtdePorFaixa += reserva.Adultos.GetValueOrDefault(0) > 1 ? $"{reserva.Adultos} Adulto(s); " : $"{reserva.Adultos} Adulto; ";
            if (reserva.Criancas1.GetValueOrDefault(0) > 0 || reserva.Criancas2.GetValueOrDefault(0) > 0)
                qtdePorFaixa += (reserva.Criancas1.GetValueOrDefault(0) + reserva.Criancas2.GetValueOrDefault(0) > 1) ? 
                    $"{reserva.Criancas1.GetValueOrDefault(0)+reserva.Criancas2.GetValueOrDefault(0)} Criança(s) " :
                    $"{reserva.Criancas1.GetValueOrDefault(0) + reserva.Criancas2.GetValueOrDefault(0)} Criança ";
            

            // Montar o retorno
            DadosImpressaoVoucherResultModel retorno = new DadosImpressaoVoucherResultModel
            {
                AgendamentoId = reserva.IdReservasFront,
                NumeroReserva = reserva.NumReserva?.ToString(),
                NomeCliente = reserva.NomeCliente,
                DocumentoCliente = reserva.NumDocumentoCliente,
                NomeHotel = reserva.Hotel,
                HospedePrincipal = reserva.NomeHospede,
                HospedePrincipalNome = reserva.NomeHospede,
                HospedePrincipalDocumento = hospedePrincipalDocumento,
                TipoUtilizacao = reserva.TipoHospede,
                TipoDisponibilizacao = reserva.Origem,
                TipoUso = reserva.Segmento,
                Contrato = reserva.NumeroContrato,
                Observacao = reserva.Observacoes,
                Observacoes = reserva.Observacoes,
                DataChegada = reserva.Checkin?.ToString("dd/MM/yyyy"),
                HoraChegada = reserva.Checkin.HasValue ? reserva.Checkin.Value.ToString("HH:mm") : "15h00",
                DataPartida = reserva.Checkout?.ToString("dd/MM/yyyy"),
                HoraPartida = reserva.Checkout.HasValue ? reserva.Checkout.Value.ToString("HH:mm") : "12h00",
                Acomodacao = reserva.TipoUh,
                TipoApartamento = reserva.TipoUh,
                QuantidadePax = reserva.Adultos.HasValue && reserva.Criancas1.HasValue && reserva.Criancas2.HasValue
                    ? (reserva.Adultos.Value + reserva.Criancas1.Value + reserva.Criancas2.Value).ToString()
                    : reserva.Adultos?.ToString(),
                Hospedes = hospedes,
                QuantidadePaxPorFaixaEtaria = qtdePorFaixa
            };

            return retorno;
        }

        public async Task<bool> VincularReservaRCI(VincularReservaRciModel vincularModel)
        {
            var loggedUser = await _repositorySystem.GetLoggedUser();

            _repository.BeginTransaction();
            try
            {
                var reservaTimeSharing = (await _repositorySystem.FindByHql<ReservaTimeSharing>($"From ReservaTimeSharing Where Id = {vincularModel.IdReservaTimeSharing}")).FirstOrDefault();
                if (reservaTimeSharing == null)
                    throw new ArgumentException("Reserva Time Sharing não encontrada para vinculação.");

                var reservasFront = (await _repository.FindBySql<ReservaTsModel>(@$"Select 
                            Nvl(rf.TipoDeUso,'UP') AS TipoDeUso,
                            rf.*,
                            To_char(Coalesce(rf.DataChegadaReal, rf.DataChegPrevista),'dd/MM/yyyy') as Checkin,
                            To_char(Coalesce(rf.DataPartidaReal, rf.DataPartPrevista),'dd/MM/yyyy') as Checkout,
                            Coalesce(rf.DataChegadaReal, rf.DataChegPrevista) as DataCheckin,
                            Coalesce(rf.DataPartidaReal, rf.DataPartPrevista) as DataCheckout,
                            rf.NumReserva,
                            rf.IdRoomList,
                            rg.NomeGrupo
                            From 
                             ReservasFront rf
                             Left Join RoomListVhf r on rf.IdRoomList = r.IdRoomList
                             Left Join ReservaGrupo rg on r.IdReservaGrupo = rg.IdReservaGrupo
                           Where 
                             rf.NumReserva = {vincularModel.NumReserva}")).FirstOrDefault();

                if (reservasFront == null)
                    throw new ArgumentException("Reserva não encontrada para vinculação.");

                if (reservasFront.IdRoomList.GetValueOrDefault(0) > 0 && !reservasFront.NomeGrupo!.Contains("RCI",StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentException($"A reserva: {reservasFront.NumReserva} está vinculada ao grupo/Evento: '{reservasFront.NomeGrupo}' e não pode ser vinculada com uma utilização RCI.");

                var reservaJaVinculada = (await _repository.FindByHql<ReservasTs>("From ReservasRci rc Where rc.IdReservasFront = :idReservasFront",
                    new Parameter("idReservasFront",reservasFront!.IdReservasFront.GetValueOrDefault()))).FirstOrDefault();

                if (reservaJaVinculada == null)
                {
                    var reservaBaixandoPontos = (await _repository.FindBySql<LancPontosTs>("Select lp.* From LancPontosTs lp Where lp.IdReservasFront = :idReservasFront",
                        new Parameter("idReservasFront",reservaTimeSharing!.IdReservasFront.GetValueOrDefault()))).FirstOrDefault();

                    if (reservaBaixandoPontos != null)
                            throw new ArgumentException("Não é possível vincular a reserva, pois a reserva informada já está vinculada a baixa de pontos.");

                    var parametroSistema = await GetParametroSistema();
                    if (parametroSistema == null)
                        throw new ArgumentException("Parâmetros do sistema não foram encontrados.");

                    var vendaXContratoModel = 
                        (await _repository.FindBySql<VendaXContratoTs>($@"Select 
                                                                            vc.* 
                                                                          From 
                                                                            VendaXContratoTs vc 
                                                                          Where 
                                                                            vc.IdVendaXContrato = {reservaTimeSharing.IdVendaXContrato.GetValueOrDefault(0)}")).FirstOrDefault();

                    if (vendaXContratoModel == null)
                        throw new ArgumentException($"Não foi localizado os dados da venda x contrato para o IdVendaXContrato: {reservaTimeSharing.IdVendaXContrato.GetValueOrDefault(0)}");

                    AtendClienteTs? atendClienteTs = await GetAtendimentoCliente(vendaXContratoModel!.IdVendaXContrato.GetValueOrDefault());
                    if (atendClienteTs == null || atendClienteTs.IdVendaXContrato.GetValueOrDefault(0) == 0)
                        throw new ArgumentException($"Não foi localizado os dados da venda");

                    VendaXContratoTs? vendaXContrato = await GetVendaXContrato(atendClienteTs);
                    if (vendaXContrato == null || vendaXContrato.IdVendaXContrato.GetValueOrDefault(0) == 0)
                        throw new ArgumentException($"Não foi localizado os dados da venda x contrato para o IdVendaXContrato: {atendClienteTs.IdVendaXContrato.GetValueOrDefault(0)}");

                    ContratoTsModel? padraoContrato = await GetPadraoContrato(atendClienteTs);
                    if (padraoContrato == null || padraoContrato.IdHotel.GetValueOrDefault(0) == 0)
                        throw new ArgumentException("Não foi possível localizar o padrão de contrato para vinculação da reserva RCI.");

                    if (reservasFront.IdHotel.GetValueOrDefault(0) != padraoContrato.IdHotel.GetValueOrDefault(3))
                        throw new ArgumentException($"A reserva informada não pertence ao hotel: {padraoContrato.IdHotel.GetValueOrDefault(3)}");

                    var paramTs = await GetParamHotel(padraoContrato.IdHotel.GetValueOrDefault(3));
                    if (paramTs == null)
                        throw new ArgumentException("Parâmetros do hotel não foram encontrados para vinculação da reserva RCI.");

                    var cmUserId = _configuration.GetValue<int>("CMUserId", 1900693);

                    await GravarVinculoReservaRciReservaJaExistente(new InclusaoReservaInputModel()
                    {
                        IdHotel = reservasFront.IdHotel.GetValueOrDefault(3),
                        QuantidadeAdultos = reservasFront.Adultos,
                        QuantidadeCrianca1 = reservasFront.Criancas1,
                        QuantidadeCrianca2 = reservasFront.Criancas2,
                        Checkin = reservasFront.DataCheckin,
                        Checkout = reservasFront.DataCheckout,
                        Reserva = reservasFront.NumReserva
                    }, 
                    atendClienteTs, 
                    vendaXContrato!, 
                    null, 
                    null, 
                    cmUserId, 
                    paramTs, 
                    7, 
                    reservaTimeSharing, 
                    reservasFront, 
                    reservaTimeSharing.PontosUtilizados.GetValueOrDefault(parametroSistema!.PontosRci.GetValueOrDefault(5629)));
                }


                reservaTimeSharing.NumReserva = vincularModel.NumReserva;
                reservaTimeSharing.StatusCM = "VINCULADA";
                reservaTimeSharing.IdReservasFront = reservasFront.IdReservasFront;
                reservaTimeSharing.DataHoraAlteracao = DateTime.Now;
                reservaTimeSharing.UsuarioVinculacao = loggedUser != null && !string.IsNullOrEmpty(loggedUser.Value.userId) ? int.Parse(loggedUser.GetValueOrDefault().userId) : null;
                reservaTimeSharing.NumReserva = $"{reservasFront.NumReserva}";
                await _repositorySystem.Save(reservaTimeSharing);

                var resultCommit = await _repository.CommitAsync();
                if (!resultCommit.executed)
                    throw resultCommit.exception ?? new Exception("Não foi possível concluir a operação de vinculação da reserva RCI.");    

                return true;
            }
            catch (Exception err)
            {
                _repository.Rollback();
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        public async Task<IList<PeriodoDisponivelResultModel>?> DisponibilidadeParaTroca(SearchDisponibilidadeParaTrocaModel searchModel)
        {
            // Busca disponibilidade similar ao método Disponibilidade, mas considerando a reserva atual
            if (searchModel.ReservaId.GetValueOrDefault(0) <= 0)
                throw new ArgumentException("ReservaId é obrigatório");

            // Busca a reserva atual para obter informações do contrato
            var reserva = (await _repository.FindBySql<ReservaTsModel>($"Select Nvl(rf.TipoDeUso,'UP') AS TipoDeUso, rf.DataChegPrevista as Checkin, rf.DataPartPrevista as Checkout, rf.* From ReservasFront rf Where rf.IdReservasFront = {searchModel.ReservaId.GetValueOrDefault()}")).FirstOrDefault();
            if (reserva == null)
                throw new ArgumentException("Reserva não encontrada");

            var lancPontosTs = (await _repository.FindByHql<LancPontosTs>($"From LancPontosTs Where IdReservasFront = {reserva.IdReservasFront}")).FirstOrDefault();
            if (lancPontosTs == null)
                throw new ArgumentException("Lançamento de pontos não encontrado");

            // Usa o SearchDisponibilidadeModel existente para buscar disponibilidade
            var disponibilidadeModel = new SearchDisponibilidadeModel
            {
                NumeroContrato = searchModel.NumeroContrato,
                IdVendaXContrato = lancPontosTs.IdVendaXContrato ?? searchModel.IdVendaXContrato,
                HotelId = searchModel.HotelId,
                DataInicial = searchModel.DataInicial,
                DataFinal = searchModel.DataFinal,
                TipoDeBusca = searchModel.TipoDeBusca ?? "E",
                IdReservasFront = reserva != null && reserva.IdReservasFront.GetValueOrDefault(0) > 0 ? reserva.IdReservasFront.GetValueOrDefault() : searchModel.ReservaId,
                NumReserva = reserva != null && reserva.NumReserva.GetValueOrDefault(0) > 0 ? reserva.NumReserva.ToString() : null
            };

            // Buscar disponibilidade usando método existente
            IList<PeriodoDisponivelResultModel> periodosDisponiveis = (await Disponibilidade(disponibilidadeModel)).AsList();

            if (periodosDisponiveis == null || !periodosDisponiveis.Any())
                return periodosDisponiveis;

            // 🔢 AJUSTE: Recalcular PontosNecessario usando quantidade de pessoas da reserva atual
            // Se o frontend enviou as quantidades, usar essas. Senão, buscar da reserva atual
            int qtdAdultos = searchModel.QuantidadeAdultos ?? reserva.Adultos ?? 2;
            int qtdCriancas1 = searchModel.QuantidadeCriancas1 ?? reserva.Criancas1 ?? 0;
            int qtdCriancas2 = searchModel.QuantidadeCriancas2 ?? reserva.Criancas2 ?? 0;

            _logger.LogInformation("📊 Recalculando pontos para troca de período - Adultos: {Adultos}, Crianças6-11: {Criancas1}, Crianças0-5: {Criancas2}",
                qtdAdultos, qtdCriancas1, qtdCriancas2);

            if (reserva != null)
            {
                foreach (var periodo in periodosDisponiveis.Reverse())
                {
                    if ((reserva.Checkin.GetValueOrDefault().Date == periodo.Checkin.GetValueOrDefault().Date && 
                        reserva.Checkout.GetValueOrDefault().Date == periodo.Checkout.GetValueOrDefault().Date) || 
                        (periodo.HotelId == 1 && (reserva.Adultos.GetValueOrDefault(0)+reserva.Criancas1.GetValueOrDefault(0)+reserva.Criancas2.GetValueOrDefault(0)) > periodo.Capacidade.GetValueOrDefault(2)))
                        periodosDisponiveis.Remove(periodo);
                }

            }

            // Recalcular pontos para cada período disponível usando a quantidade correta de pessoas
            foreach (var periodo in periodosDisponiveis)
            {
                try
                {
                    if (!periodo.Checkin.HasValue || !periodo.Checkout.HasValue)
                        continue;

                    // Calcular pontos usando a quantidade de pessoas da reserva atual
                    var idVendaXContratoValor = lancPontosTs.IdVendaXContrato ?? searchModel.IdVendaXContrato ?? 0;
                    var pontosCalculados = await CalcularPontosNecessariosSimplificado(
                        periodo.Checkin.Value,
                        periodo.Checkout.Value,
                        qtdAdultos,
                        qtdCriancas1,
                        qtdCriancas2,
                        periodo.HotelId ?? 3,
                        periodo.TipoUhId.GetValueOrDefault(),
                        idVendaXContratoValor,
                        searchModel.NumeroContrato ?? "",
                        numReserva: reserva.NumReserva.GetValueOrDefault().ToString(),
                        null // hospedes
                    );

                    // Atualizar com o valor correto
                    periodo.PontosNecessario = pontosCalculados;
                    
                    // 🔥 AJUSTE: Atualizar PadraoTarifario para refletir o cálculo real aplicado
                    // Mostra a quantidade de pessoas usada no cálculo para evitar confusão
                    int totalPessoas = qtdAdultos + qtdCriancas1 + qtdCriancas2;
                    
                    // Construir mensagem detalhada do padrão tarifário aplicado
                    string descricaoPessoas = "";
                    if (qtdAdultos > 0 && (qtdCriancas1 > 0 || qtdCriancas2 > 0))
                    {
                        descricaoPessoas = $"{qtdAdultos} adulto{(qtdAdultos > 1 ? "s" : "")}";
                        if (qtdCriancas1 > 0)
                            descricaoPessoas += $", {qtdCriancas1} criança{(qtdCriancas1 > 1 ? "s" : "")} (6-11 anos)";
                        if (qtdCriancas2 > 0)
                            descricaoPessoas += $", {qtdCriancas2} criança{(qtdCriancas2 > 1 ? "s" : "")} (0-5 anos)";
                    }
                    else if (qtdAdultos > 0)
                    {
                        descricaoPessoas = $"{qtdAdultos} adulto{(qtdAdultos > 1 ? "s" : "")}";
                    }
                    else
                    {
                        descricaoPessoas = $"{totalPessoas} pessoa{(totalPessoas > 1 ? "s" : "")}";
                    }
                    
                    periodo.PadraoTarifario = $"{descricaoPessoas}: {pontosCalculados:N0} pontos";
                    
                    _logger.LogInformation("✅ Período {Checkin} - {Checkout}: {Pontos} pontos (para {Qtd} pessoas - {Descricao}) - Padrão atualizado",
                        periodo.Checkin.Value.ToString("dd/MM/yyyy"),
                        periodo.Checkout.Value.ToString("dd/MM/yyyy"),
                        periodo.PontosNecessario,
                        totalPessoas,
                        descricaoPessoas);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Erro ao recalcular pontos para período {Checkin} - {Checkout}. Mantendo valor original.",
                        periodo.Checkin?.ToString("dd/MM/yyyy"),
                        periodo.Checkout?.ToString("dd/MM/yyyy"));
                }
            }

            return periodosDisponiveis;
        }

        public async Task<TrocaPeriodoResponseModel> TrocarPeriodo(TrocaPeriodoRequestModel model)
        {

            try
            {
                _repository.BeginTransaction();
                _repositorySystem.BeginTransaction();

                // 1. Buscar reserva atual
                var reserva = (await _repository.FindBySql<ReservaTsModel>($"Select Nvl(rf.TipoDeUso,'UP') AS TipoDeUso, rf.DataChegPrevista as Checkin, rf.DataPartPrevista as Chekout, rf.* From ReservasFront rf Where rf.IdReservasFront = {model.ReservaId}")).FirstOrDefault();
                if (reserva == null)
                    throw new ArgumentException("Reserva não encontrada");

                // 2. Buscar lançamento de pontos atual
                var lancPontosTs = (await _repository.FindByHql<LancPontosTs>($"From LancPontosTs Where IdReservasFront = {reserva.IdReservasFront}")).FirstOrDefault();
                if (lancPontosTs == null)
                    throw new ArgumentException("Lançamento de pontos não encontrado");

                var pontosDebitadosAtual = lancPontosTs.NumeroPontos.GetValueOrDefault(0);

                // 3. Buscar saldo atual de pontos
                var disponibilidade = await GetSaldo(new SearchDisponibilidadeModel() 
                { 
                    IdVendaXContrato = model.IdVendaXContrato, 
                    NumeroContrato = model.NumeroContrato 
                });

                if (disponibilidade == null)
                    throw new ArgumentException("Não foi possível buscar saldo de pontos");

                var saldoAtual = disponibilidade.SaldoPontos.GetValueOrDefault(0);

                // 4. Calcular pontos necessários para o novo período
                var disponibilidadeNovoPeriodo = await Disponibilidade(new SearchDisponibilidadeModel
                {
                    IdVendaXContrato = model.IdVendaXContrato,
                    NumeroContrato = model.NumeroContrato,
                    HotelId = model.HotelId,
                    DataInicial = model.NovoCheckin,
                    DataFinal = model.NovoCheckout,
                    TipoDeBusca = model.TipoDeBusca ?? "E"
                });

                if (disponibilidadeNovoPeriodo == null || !disponibilidadeNovoPeriodo.Any())
                    throw new ArgumentException("Período não disponível para troca");

                var periodoDisponivel = disponibilidadeNovoPeriodo.FirstOrDefault();
                if (periodoDisponivel == null)
                    throw new ArgumentException("Período não encontrado na disponibilidade");

                // Obter quantidade de hóspedes da reserva original
                int totalHospedes = reserva.Adultos.GetValueOrDefault(0) + 
                                   reserva.Criancas1.GetValueOrDefault(0) + 
                                   reserva.Criancas2.GetValueOrDefault(0);

                var reservaTimeSharingHistorico = (await _repositorySystem.FindByHql<ReservaTimeSharing>($"From ReservaTimeSharing Where IdReservasFront = {reserva.IdReservasFront}")).FirstOrDefault();
                var tipoUsoAtual = reservaTimeSharingHistorico?.TipoUtilizacao ?? "UP";

                // Criar modelo temporário simples para cálculo (sem detalhes de hóspedes)
                // Nota: Para troca de período, mantém-se a mesma quantidade/composição de hóspedes
                var inclusaoReservaModel = new InclusaoReservaInputModel
                {
                    Reserva = reserva.NumReserva,
                    NumeroContrato = model.NumeroContrato,
                    IdVendaXContrato = model.IdVendaXContrato,
                    IdHotel = !string.IsNullOrEmpty(model.HotelId) ? int.Parse(model.HotelId) : 0,
                    Checkin = model.NovoCheckin,
                    Checkout = model.NovoCheckout,
                    TipoUhEstadia = periodoDisponivel.TipoUhId,
                    TipoUhTarifa = periodoDisponivel.TipoUhId,
                    IdTipoUh = periodoDisponivel.TipoUhId,
                    QuantidadeAdultos = reserva.Adultos ?? 1,
                    QuantidadeCrianca1 = reserva.Criancas1 ?? 0,
                    QuantidadeCrianca2 = reserva.Criancas2 ?? 0,
                    TipoUso = tipoUsoAtual,
                    TipoUtilizacao = tipoUsoAtual
                };

                if (inclusaoReservaModel.IdHotel.GetValueOrDefault(0) <= 0)
                    throw new ArgumentException("Não foi possível salvar a alteração da reserva no novo período");

                // Usar método centralizado para calcular pontos (ÚNICO LUGAR onde pontos são calculados)
                var pontosNecessariosNovo = CalcularPontosNecessarios(inclusaoReservaModel, periodoDisponivel, inclusaoReservaModel.IdHotel.GetValueOrDefault(0), totalHospedes);

                // 5. Calcular saldo atualizado (devolver pontos da reserva atual)
                var saldoAtualizado = saldoAtual + pontosDebitadosAtual;

                // 6. Verificar se saldo é suficiente
                if (saldoAtualizado < pontosNecessariosNovo)
                    throw new ArgumentException($"Saldo de pontos insuficiente. Pontos necessários: {pontosNecessariosNovo}. Saldo disponível: {saldoAtualizado}.");

                // Buscar hóspedes atuais da reserva para manter
                var hospedesAtuais = await GetHospedesReserva(Convert.ToInt64(reserva.IdReservasFront.GetValueOrDefault()));
                inclusaoReservaModel.Hospedes = hospedesAtuais;
                
                // 8. Ajustar débito de pontos
                // Primeiro, estornar o lançamento de pontos anterior
                var paramTs = await GetParamHotel(inclusaoReservaModel.IdHotel.GetValueOrDefault());
                var cmUserId = _configuration.GetValue<int>("CMUserId", 1900693);

                AtendClienteTs? atendClienteTs = await GetAtendimentoCliente(model.IdVendaXContrato);
                if (atendClienteTs == null || atendClienteTs.IdVendaXContrato.GetValueOrDefault(0) == 0)
                    throw new ArgumentException($"Não foi localizado os dados da venda");

                var reservaAlterada = await AlterarReservaExecute(inclusaoReservaModel, reserva,atendClienteTs);
                if (reservaAlterada == null)
                    throw new ArgumentException("Falha ao alterar período na API do VHF");


                var diferencaPontos = pontosNecessariosNovo - pontosDebitadosAtual;

                await _repository.CommitAsync();
                await _repositorySystem.CommitAsync();

                // Buscar saldo atualizado após ajuste
                var saldoAtualizadoFinal = await GetSaldo(new SearchDisponibilidadeModel()
                {
                    IdVendaXContrato = model.IdVendaXContrato,
                    NumeroContrato = model.NumeroContrato
                });

                return new TrocaPeriodoResponseModel
                {
                    ReservaId = model.ReservaId,
                    NovoCheckin = model.NovoCheckin,
                    NovoCheckout = model.NovoCheckout,
                    PontosDebitados = pontosNecessariosNovo,
                    PontosDevolvidos = pontosDebitadosAtual,
                    PontosAdicionais = diferencaPontos > 0 ? diferencaPontos : 0,
                    SaldoPontosAtual = saldoAtualizadoFinal?.SaldoPontos.GetValueOrDefault(0) ?? 0
                };
            }
            catch (Exception err)
            {
                _repository.Rollback();
                _repositorySystem.Rollback();
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        public async Task<TrocaTipoUsoResponseModel> TrocarTipoUso(TrocaTipoUsoRequestModel model)
        {
            try
            {
                _repository.BeginTransaction();
                _repositorySystem.BeginTransaction();

                // 1. Buscar reserva atual
                var reserva = (await _repository.FindBySql<ReservaTsModel>($"Select Nvl(rf.TipoDeUso,'UP') AS TipoDeUso, rf.* From ReservasFront rf Where rf.IdReservasFront = {model.ReservaId}")).FirstOrDefault();
                if (reserva == null)
                    throw new ArgumentException("Reserva não encontrada");

                // 2. Alterar tipo de uso na reserva
                var inclusaoReservaModel = new InclusaoReservaInputModel
                {
                    Reserva = reserva.NumReserva,
                    IdReservasFront = reserva.IdReservasFront,
                    NumReserva = reserva.NumReserva,
                    NumeroContrato = model.NumeroContrato,
                    IdVendaXContrato = model.IdVendaXContrato,
                    IdHotel = reserva.IdHotel,
                    Checkin = reserva.DataCheckin,
                    Checkout = reserva.DataCheckout,
                    TipoUhEstadia = reserva.TipoUhEstadia,
                    TipoUhTarifa = reserva.TipoUhTarifa ?? reserva.TipoUhEstadia,
                    QuantidadeAdultos = reserva.Adultos ?? 1,
                    QuantidadeCrianca1 = reserva.Criancas1 ?? 0,
                    QuantidadeCrianca2 = reserva.Criancas2 ?? 0,
                    TipoUso = model.NovoTipoUso
                };

                // Buscar hóspedes atuais
                var hospedesAtuais = await GetHospedesReserva(Convert.ToInt64(reserva.IdReservasFront));
                inclusaoReservaModel.Hospedes = hospedesAtuais;

                var reservaAlterada = await SalvarReservaNoCM((InclusaoReservaInputDto)inclusaoReservaModel);
                if (reservaAlterada == null)
                    throw new ArgumentException("Falha ao alterar tipo de uso na API do VHF");

                await _repository.CommitAsync();
                await _repositorySystem.CommitAsync();

                return new TrocaTipoUsoResponseModel
                {
                    ReservaId = model.ReservaId,
                    NovoTipoUso = model.NovoTipoUso
                };
            }
            catch (Exception err)
            {
                _repository.Rollback();
                _repositorySystem.Rollback();
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        private async Task<List<HospedeInputModel>> GetHospedesReserva(long idReservasFront)
        {
            // Usar a mesma lógica do método GetHospedes existente
            var reservaModel = new ReservaTimeSharingCMModel
            {
                IdReservasFront = (int)idReservasFront
            };
            
            await GetHospedes(reservaModel);
            
            return reservaModel.Hospedes ?? new List<HospedeInputModel>();
        }

        public async Task<CalcularPontosResponseModel> CalcularPontosNecessarios(CalcularPontosRequestModel request)
        {
            try
            {
                if (request.NumReserva.GetValueOrDefault(0) > 0)
                {
                    var reservasFront = (await _repository.FindBySql<ReservaTsModel>("Select Nvl(rf.TipoDeUso,'UP') AS TipoDeUso, rf.DataChegPrevista as Checkin, rf.DataPartPrevista as Checkoutm, rf.* From ReservasFront rf Where rf.NumReserva = :numReserva ", new Parameter("numReserva", request.NumReserva.GetValueOrDefault()))).FirstOrDefault();
                    if (reservasFront == null)
                        throw new ArgumentException($"Reserva {request.NumReserva} não encontrada");

                    request.HotelId = reservasFront.IdHotel.GetValueOrDefault();
                }


                // Buscar disponibilidade para obter informações complementares
                var disponibilidade = await Disponibilidade(new SearchDisponibilidadeModel
                {
                    DataInicial = request.DataInicial,
                    DataFinal = request.DataFinal,
                    HotelId = request.HotelId.ToString(),
                    IdVendaXContrato = request.IdVendaXContrato,
                    NumeroContrato = request.NumeroContrato,
                    TipoDeBusca = "E"
                });

                var periodoDisponivel = disponibilidade?.FirstOrDefault();

                if (periodoDisponivel == null)
                    throw new ArgumentException("Não foi possível encontrar disponibilidade para o período informado");


                // Chamar serviço para calcular pontos
                var pontosNecessarios = await CalcularPontosNecessariosSimplificado(
                    request.DataInicial,
                    request.DataFinal,
                    request.QuantidadeAdultos,
                    request.QuantidadeCriancas1,
                    request.QuantidadeCriancas2,
                    request.HotelId,
                    periodoDisponivel.TipoUhId.GetValueOrDefault(),
                    request.IdVendaXContrato,
                    request.NumeroContrato,
                    request.NumReserva.GetValueOrDefault(0).ToString()
                );

                // Montar response
                var response = new CalcularPontosResponseModel
                {
                    PontosNecessarios = pontosNecessarios,
                    DataInicial = request.DataInicial,
                    DataFinal = request.DataFinal,
                    Diarias = (request.DataFinal - request.DataInicial).Days,
                    TotalHospedes = request.QuantidadeAdultos + request.QuantidadeCriancas1 + request.QuantidadeCriancas2,
                    QuantidadeAdultos = request.QuantidadeAdultos,
                    QuantidadeCriancas1 = request.QuantidadeCriancas1,
                    QuantidadeCriancas2 = request.QuantidadeCriancas2,
                    HotelId = request.HotelId,
                    NomeHotel = periodoDisponivel?.NomeHotel,
                    TipoApartamento = periodoDisponivel?.TipoApartamento,
                    PadraoTarifario = periodoDisponivel?.PadraoTarifario,
                    NumeroContrato = request.NumeroContrato
                };

                return response;
            }
            catch (Exception err)
            {
                _logger.LogError(err, err.Message);
                throw;
            }
        }

        public DadosContratoModel? GetContrato(DadosImpressaoVoucherResultModel dadosReserva, List<DadosContratoModel> contratos)
        {
            var contrato = contratos.FirstOrDefault(a=> !string.IsNullOrEmpty(a.ProjetoXContrato) && !string.IsNullOrEmpty(dadosReserva.Contrato) && a.ProjetoXContrato == dadosReserva.Contrato);
            if (contrato != null)
            {
                dadosReserva.Contrato = contrato.ProjetoXContrato;
                dadosReserva.NomeCliente = contrato.PessoaTitular1Nome;
            }
            return contrato;
        }
    }
}
