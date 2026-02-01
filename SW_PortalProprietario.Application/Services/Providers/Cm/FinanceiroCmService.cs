using AccessCenterDomain.AccessCenter;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Providers.Cm
{
    public class FinanceiroCmService : IFinanceiroProviderService
    {

        private const string PREFIXO_TRANSACOES_FINANCEIRAS = "PORTALPROPCM_";
        private readonly ICommunicationProvider _communicationProvider;
        private readonly ILogger<FinanceiroCmService> _logger;
        private readonly IRepositoryNHCm _repositoryNHCm;
        private readonly IConfiguration _configuration;
        private readonly IServiceBase _serviceBase;

        public FinanceiroCmService(ICommunicationProvider communicationProvider,
            ILogger<FinanceiroCmService> logger,
            IRepositoryNHCm repositoryNHCm,
            IConfiguration configuration,
            IServiceBase serviceBase)
        {
            _logger = logger;
            _communicationProvider = communicationProvider;
            _repositoryNHCm = repositoryNHCm;
            _configuration = configuration;
            _serviceBase = serviceBase;
        }

        public string ProviderName => _communicationProvider.CommunicationProviderName;

        public string PrefixoTransacaoFinanceira => PREFIXO_TRANSACOES_FINANCEIRAS;

        public Task<BaixaResultModel> AlterarTipoContaReceberPagasEmCartao(PaymentCardTokenized item, IStatelessSession? session)
        {
            throw new NotImplementedException();
        }

        public Task<BaixaResultModel> BaixarValoresPagosEmPix(PaymentPix item, IStatelessSession? session)
        {
            throw new NotImplementedException();
        }

        public Task<BoletoModel> DownloadBoleto(DownloadBoleto model)
        {
            throw new NotImplementedException();
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario(SearchContasPendentesUsuarioLogado searchModel)
        {
            var parametros = new List<Parameter>();

            var empresaCmId = _configuration.GetValue<int>("EmpresaCMId", 3);

            var loggedUser = await _repositoryNHCm.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            var pessoaVinculadaSistema = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), _communicationProvider.CommunicationProviderName);
            if (pessoaVinculadaSistema == null)
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            if (string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) || !Helper.IsNumeric(pessoaVinculadaSistema.PessoaProvider))
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {_communicationProvider.CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            var txtFiltrosAdicionais = $" AND PRO.IdPessoa = {pessoaVinculadaSistema.PessoaProvider} ";

            if (searchModel.VencimentoInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parametros.Add(new Parameter("vencimentoInicial", searchModel.VencimentoInicial.GetValueOrDefault().Date));
                txtFiltrosAdicionais += " AND COALESCE(D.DATAPROGRAMADA,D.DATAVENCTO) >= :vencimentoInicial ";
            }

            if (searchModel.VencimentoFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parametros.Add(new Parameter("vencimentoFinal", searchModel.VencimentoFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                txtFiltrosAdicionais += " AND COALESCE(D.DATAPROGRAMADA,D.DATAVENCTO) <= :vencimentoFinal ";
            }

            if (searchModel.Status == "B")
            {
                txtFiltrosAdicionais += " AND Exists(Select ld.CodDocumento From LanctoDocum ld Where ld.Operacao = 5 and ld.CodDocumento = l.CodDocumento) ";
            }
            else if (searchModel.Status == "P")
            {
                txtFiltrosAdicionais += " AND NOT Exists(Select ld.CodDocumento From LanctoDocum ld Where ld.Operacao = 5 and ld.CodDocumento = l.CodDocumento) and SaldoCar.Saldo != 0.00 ";
            }
            else if (searchModel.Status == "V")
            {
                txtFiltrosAdicionais += " AND Not Exists(Select ld.CodDocumento From LanctoDocum ld Where ld.Operacao = 5 and ld.CodDocumento = l.CodDocumento) and SaldoCar.Saldo != 0.00 and " +
                    " COALESCE(D.DATAPROGRAMADA,D.DATAVENCTO) <= :dataAtual ";
                parametros.Add(new Parameter("dataAtual", DateTime.Today.Date));
            }

            txtFiltrosAdicionais += " AND NOT EXISTS(Select ld.CodDocumento From LanctoDocum ld Where ld.Operacao = 4 and ld.CodAlterador = 521 and ld.CodDocumento = l.CodDocumento) ";


            var sb = new StringBuilder(@$"SELECT 
                                    PRO.EMAIL,
                                    L.CodDocumento as Id,
                                    L.TRGDTINCLUSAO AS DataHoraCriacao,
                                    PRO.IdPessoa as PessoaId,
                                    PRO.IdPessoa as PessoaProviderId,
                                    PRO.NOME as NomePessoa,
                                    PRO.NUMDOCUMENTO AS DOCUMENTOCLIENTE,
                                    L.DataLancamento as DataCriacao,
                                    COALESCE(D.DATAPROGRAMADA,D.DATAVENCTO) AS Vencimento,
                                    TDRP.CODTIPDOC AS CODIGOTIPOCONTA,
                                    TDRP.DESCRICAO AS NomeTipoConta,
                                    D.NUMDIGCODBARRAS AS LINHADIGITAVELBOLETO,
                                    D.NOSSONUMERO AS NOSSONUMEROBOLETO,
                                    PE.NUMDOCUMENTO AS EMPREENDIMENTOCNPJ,
                                    PE.NOME AS EMPREENDIMENTONOME,
                                    PE.IDPESSOA AS PESSOAEMPREENDIMENTOID,
                                    PE.IDPESSOA AS EMPRESAID,
                                    PE.NOME AS EMPRESANOME,
                                    PRO.NUMEROCONTRATO AS NumeroImovel,
                                    PRO.NUMEROCONTRATO AS Contrato, 
                                    CASE WHEN SaldoCar.Saldo = 0 then COALESCE(D.DATAVENCTO, L.DATAPAGAMENTO) ELSE NULL end AS DATAHORABAIXA,
                                    COALESCE(D.DATAVENCTO, L.DATAPAGAMENTO) AS DATAPROCESSAMENTO,
                                    CASE
                                       WHEN SUBSTR(L.COMPLDOCUMENTO,1,1) = 'S' THEN 'Sinal: '         || SUBSTR(L.COMPLDOCUMENTO,2,LENGTH(L.COMPLDOCUMENTO)-1)
                                               WHEN SUBSTR(L.COMPLDOCUMENTO,1,1) = 'E' THEN 'Entrada: '       || SUBSTR(L.COMPLDOCUMENTO,2,LENGTH(L.COMPLDOCUMENTO)-1)
                                               WHEN SUBSTR(L.COMPLDOCUMENTO,1,1) = 'I' THEN 'Intermediária: ' || SUBSTR(L.COMPLDOCUMENTO,2,LENGTH(L.COMPLDOCUMENTO)-1)
                                               WHEN SUBSTR(L.COMPLDOCUMENTO,1,1) = 'P' THEN 'Parcela: '       || SUBSTR(L.COMPLDOCUMENTO,2,LENGTH(L.COMPLDOCUMENTO)-1)
                                            END AS OBSERVACAO,
                                            (LD.VALOR * Decode(ld.DebCre,'D',-1,1)) AS ValorOriginal,
                                            SALDOCAR.Saldo,
                                            (LD.VALOR * Decode(ld.DebCre,'D',-1,1)) AS Valor,
                                            SALDOCAR.VALORBAIXADO
                                            FROM
                                            LANCAMENTOTS L
                                            INNER JOIN DOCUMENTO D ON L.CODDOCUMENTO = D.CODDOCUMENTO
                                            INNER JOIN LANCTODOCUM LD ON D.CODDOCUMENTO = LD.CODDOCUMENTO
                                            INNER JOIN TIPODOCRECPAG TDRP ON D.CODTIPDOC = TDRP.CODTIPDOC
                                            INNER JOIN PESSOA PE ON D.IDPESSOA = PE.IDPESSOA AND PE.IDPESSOA = 3
                                            INNER JOIN PARAMTS P ON L.IDHOTEL = P.IDHOTEL
                                            INNER JOIN (
                                            SELECT
                                            ld3.CodDocumento, 
                                            Sum(ld3.Valor * Decode(ld3.DebCre,'D',-1,1)) AS Saldo,
                                            Sum(CASE WHEN Ld3.OPERACAO IN (4,5) THEN (ld3.Valor * Decode(ld3.DebCre,'D',-1,1)) ELSE 0 end) AS ValorBaixado,
                                            Max(ld3.DataLancto) AS DataUltimaAlteracao
                                            From LanctoDocum ld3 
                                            WHERE exists(SELECT 
                                                            lt.CodDocumento 
                                                         FROM 
                                                            LancamentoTs lt 
                                                         WHERE 
                                                            lt.CodDocumento = ld3.CodDocumento AND 
                                                            ((LT.FLGREMOVIDO IS NULL) OR (LT.FLGREMOVIDO IS NOT NULL AND (LT.IDLANCESTORNO IS NULL AND LT.IDMOTIVOESTORNO IS NULL) )))
                                                        GROUP BY ld3.CodDocumento) SALDOCAR ON SALDOCAR.CODDOCUMENTO = D.CODDOCUMENTO
                                            INNER JOIN (SELECT
                                                DISTINCT
                                                V.IDVENDATS,
                                                VC.IDVENDAXCONTRATO,
                                                VC.NUMEROCONTRATO,
                                                PCL.IDPESSOA, 
                                                PCL.NOME, 
                                                PCL.NUMDOCUMENTO,
                                                PCL.EMAIL,
                                                VC.FLGCANCELADO
                                              FROM 
                                                  VENDAXCONTRATOTS VC, 
                                                  VENDATS V,
                                                  ATENDCLIENTETS A,
                                                  PESSOA PCL
                                              WHERE
                                               V.IDVENDATS = VC.IDVENDATS
                                               AND V.IDATENDCLIENTETS = A.IDATENDCLIENTETS
                                               AND A.IDCLIENTE = PCL.IDPESSOA
                                              ) PRO ON PRO.IDVENDATS = L.IDVENDATS
                                                AND LD.NUMLANCTO = (SELECT MIN(LD1.NUMLANCTO) FROM LANCTODOCUM LD1 WHERE LD1.CODDOCUMENTO = D.CODDOCUMENTO)
                                                AND D.IDPESSOA = 3 AND d.RecPag = 'R'
                                            AND ((L.FLGREMOVIDO IS NULL) OR (L.FLGREMOVIDO IS NOT NULL AND (L.IDLANCESTORNO IS NULL AND L.IDMOTIVOESTORNO IS NULL) ))
                                        {txtFiltrosAdicionais} ");

            var sql = sb.ToString();

            var totalRegistros = await _repositoryNHCm.CountTotalEntry(sql, parametros.ToArray());
            if (totalRegistros == 0)
                return (1, 1, new List<ContaPendenteModel>());

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros) < 30 ? Convert.ToInt32(totalRegistros) : 30;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 && searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > Convert.ToInt32(totalRegistros))
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros);

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
            totalRegistros <= (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine("ORDER BY L.IDLANCAMENTOTS");

            var result = (await _repositoryNHCm.FindBySql<ContaPendenteModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parametros.ToArray())).AsList();

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    foreach (var item in result)
                    {
                        if (item.Saldo.GetValueOrDefault(0) != 0.00m || item.DataHoraBaixa is null)
                            item.StatusParcela = "Em aberto";
                        else item.StatusParcela = "Paga";

                        item.Valor = item.Valor * (-1);
                        item.Saldo = item.Saldo * (-1);
                        item.ValorAtualizado = item.Saldo;
                    }

                    long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), result);
                }

            }

            return (1, 1, result);
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral(SearchContasPendentesGeral searchModel)
        {
            var parametros = new List<Parameter>();

            var parematrosSistema = await _serviceBase.GetParametroSistema();

            if (parematrosSistema == null)
                throw new ArgumentException("Falha na busca das contas: PARAMEMPNFOUND");

            var empresasIds = "";

            if (searchModel.EmpresaId.GetValueOrDefault(0) > 0)
                empresasIds = $"{searchModel.EmpresaId}";
            else empresasIds = parematrosSistema.ExibirFinanceirosDasEmpresaIds;

            if (string.IsNullOrEmpty(empresasIds))
                throw new ArgumentException("Não foi possível identificar as empresas para exibição do financeiro.");

            var txtFiltrosAdicionais = "";

            if (searchModel.VencimentoInicial.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parametros.Add(new Parameter("vencimentoInicial", searchModel.VencimentoInicial.GetValueOrDefault().Date));
                txtFiltrosAdicionais = " AND COALESCE(D.DATAPROGRAMADA,D.DATAVENCTO) >= :vencimentoInicial ";
            }

            if (searchModel.VencimentoFinal.GetValueOrDefault(DateTime.MinValue) != DateTime.MinValue)
            {
                parametros.Add(new Parameter("vencimentoFinal", searchModel.VencimentoFinal.GetValueOrDefault().Date.AddDays(1).AddMicroseconds(-1)));
                txtFiltrosAdicionais += " AND COALESCE(D.DATAPROGRAMADA,D.DATAVENCTO) <= :vencimentoFinal ";
            }

            if (!string.IsNullOrEmpty(searchModel.PessoaNome))
            {
                txtFiltrosAdicionais += $" AND LOWER(PRO.Nome) like '{searchModel.PessoaNome.ToLower().TrimEnd()}%' ";
            }

            var sb = new StringBuilder(@$"SELECT 
                                        PRO.EMAIL,
                                        L.CodDocumento as Id,
                                        L.TRGDTINCLUSAO AS DataHoraCriacao,
                                        PRO.IdPessoa as PessoaId,
                                        PRO.IdPessoa as PessoaProviderId,
                                        PRO.NOME as NomePessoa,
                                        L.DataLancamento as DataCriacao,
                                        COALESCE(D.DATAPROGRAMADA,D.DATAVENCTO) AS Vencimento,
                                        TDRP.CODTIPDOC AS CODIGOTIPOCONTA,
                                        TDRP.DESCRICAO AS NomeTipoConta,
                                        D.NUMDIGCODBARRAS AS LINHADIGITAVELBOLETO,
                                        D.NOSSONUMERO AS NOSSONUMEROBOLETO,
                                        PE.NUMDOCUMENTO AS EMPREENDIMENTOCNPJ,
                                        PE.NOME AS EMPRESANOME,
                                        PE.IDPESSOA AS EMPRESAID,
                                        PRO.NUMEROCONTRATO AS NumeroImovel,
                                        PRO.NUMEROCONTRATO AS Contrato, 
                                        (PRO.NUMEROPROJETO || '-' || TO_CHAR(PRO.NUMEROCONTRATO)) AS Contrato, 
                                        PRO.NUMDOCUMENTO AS DOCUMENTOCLIENTE,
                                        COALESCE(D.DATAVENCTO, L.DATAPAGAMENTO) AS DATAVENCTO,
                                        CASE
                                           WHEN SUBSTR(L.COMPLDOCUMENTO,1,1) = 'S' THEN 'Sinal: '         || SUBSTR(L.COMPLDOCUMENTO,2,LENGTH(L.COMPLDOCUMENTO)-1)
                                           WHEN SUBSTR(L.COMPLDOCUMENTO,1,1) = 'E' THEN 'Entrada: '       || SUBSTR(L.COMPLDOCUMENTO,2,LENGTH(L.COMPLDOCUMENTO)-1)
                                           WHEN SUBSTR(L.COMPLDOCUMENTO,1,1) = 'I' THEN 'Intermediária: ' || SUBSTR(L.COMPLDOCUMENTO,2,LENGTH(L.COMPLDOCUMENTO)-1)
                                           WHEN SUBSTR(L.COMPLDOCUMENTO,1,1) = 'P' THEN 'Parcela: '       || SUBSTR(L.COMPLDOCUMENTO,2,LENGTH(L.COMPLDOCUMENTO)-1)
                                        END AS OBSERVACAO,
                                        (LD.VALOR * Decode(ld.DebCre,'D',-1,1)) AS ValorOriginal,
                                        SALDOCAR.Saldo,
                                        SALDOCAR.Saldo AS Valor
                                        FROM
                                        LANCAMENTOTS L
                                        INNER JOIN DOCUMENTO D ON L.CODDOCUMENTO = D.CODDOCUMENTO
                                        INNER JOIN LANCTODOCUM LD ON D.CODDOCUMENTO = LD.CODDOCUMENTO
                                        INNER JOIN TIPODOCRECPAG TDRP ON D.CODTIPDOC = TDRP.CODTIPDOC
                                        INNER JOIN PESSOA PE ON D.IDPESSOA = PE.IDPESSOA AND PE.IDPESSOA in ({empresasIds})
                                        INNER JOIN PARAMTS P ON L.IDHOTEL = P.IDHOTEL
                                        INNER JOIN (
                                        SELECT
                                        ld3.CodDocumento, 
                                        Sum(ld3.Valor * Decode(ld3.DebCre,'D',-1,1)) AS Saldo
                                        From LanctoDocum ld3 
                                        WHERE exists(SELECT 
				                                        lt.CodDocumento 
			                                         FROM 
			 	                                        LancamentoTs lt 
			                                         WHERE 
			 	                                        lt.CodDocumento = ld3.CodDocumento AND 
			 	                                        ((LT.FLGREMOVIDO IS NULL) OR (LT.FLGREMOVIDO IS NOT NULL AND (LT.IDLANCESTORNO IS NULL AND LT.IDMOTIVOESTORNO IS NULL) )))
			                                        GROUP BY ld3.CodDocumento) SALDOCAR ON SALDOCAR.CODDOCUMENTO = D.CODDOCUMENTO
                                        INNER JOIN (SELECT
                                            DISTINCT
                                            V.IDVENDATS,
                                            VC.IDVENDAXCONTRATO,
                                            VC.NUMEROCONTRATO,
                                            PCL.IDPESSOA, 
                                            PCL.NOME, 
                                            PCL.NUMDOCUMENTO,
                                            PCL.EMAIL,
                                            VC.FLGCANCELADO,
                                            PRJ.NUMEROPROJETO
                                          FROM 
                                              VENDAXCONTRATOTS VC, 
                                              VENDATS V,
                                              ATENDCLIENTETS A,
                                              PESSOA PCL,
                                              PROJETOTS PRJ
                                          WHERE
                                           V.IDVENDATS = VC.IDVENDATS
                                           AND V.IDATENDCLIENTETS = A.IDATENDCLIENTETS
                                           AND A.IDCLIENTE = PCL.IDPESSOA
                                           AND VC.IDPROJETOTS = PRJ.IDPROJETOTS
                                          ) PRO ON PRO.IDVENDATS = L.IDVENDATS
                                            AND LD.NUMLANCTO = (SELECT MIN(LD1.NUMLANCTO) FROM LANCTODOCUM LD1 WHERE LD1.CODDOCUMENTO = D.CODDOCUMENTO)
                                            AND D.IDPESSOA = 3 AND d.RecPag = 'R'
                                            AND ((L.FLGREMOVIDO IS NULL) OR (L.FLGREMOVIDO IS NOT NULL AND (L.IDLANCESTORNO IS NULL AND L.IDMOTIVOESTORNO IS NULL) ))
                                            AND SALDOCAR.Saldo <> 0 
                                            AND D.IDPESSOA in ({empresasIds}) {txtFiltrosAdicionais} ");

            var sql = sb.ToString();

            var totalRegistros = await _repositoryNHCm.CountTotalEntry(sql, parametros.ToArray());
            if (totalRegistros == 0)
                return (1, 1, new List<ContaPendenteModel>());

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros) < 30 ? Convert.ToInt32(totalRegistros) : 30;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0 && searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > Convert.ToInt32(totalRegistros))
                searchModel.QuantidadeRegistrosRetornar = Convert.ToInt32(totalRegistros);

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            sb.AppendLine("ORDER BY L.IDLANCAMENTOTS");

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros <= (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            var result = (await _repositoryNHCm.FindBySql<ContaPendenteModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(0), parametros.ToArray())).AsList();

            if (result.Any())
            {
                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    foreach (var item in result)
                    {
                        if (item.Saldo.GetValueOrDefault(0) != 0.00m)
                            item.StatusParcela = "Em aberto";
                        else item.StatusParcela = "Paga";

                        item.Valor = item.Valor * (-1);
                        item.Saldo = item.Saldo * (-1);
                        item.ValorAtualizado = item.Saldo;
                    }

                    long totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), result);
                }

            }

            return (1, 1, result);
        }

        public Task<List<ClienteContaBancariaViewModel>> GetContasBancarias(int pessoaId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartao(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContaPendenteModel>> GetContasPorIds(List<int> itensToPay)
        {
            throw new NotImplementedException();
        }

        public Task<List<CotaPeriodoModel>> GetCotaPeriodo(int pessoaId, DateTime? dataInicial, DateTime? dataFinal)
        {
            throw new NotImplementedException();
        }

        public Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa(int pessoaProviderId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ClienteContaBancariaViewModel>> GetMinhasContasBancarias()
        {
            throw new NotImplementedException();
        }

        public Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail(bool pool, bool naoPool)
        {
            throw new NotImplementedException();
        }

        public Task<int> SalvarContaBancaria(ClienteContaBancariaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<ClienteContaBancaria?> SalvarContaBancariaInterna(ClienteContaBancariaInputModel model, ParametroSistemaViewModel? parametroSistema = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> SalvarMinhaContaBancaria(ClienteContaBancariaInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VoltarParaTiposOriginais(PaymentCardTokenized item, IStatelessSession? session)
        {
            throw new NotImplementedException();
        }
    }
}
