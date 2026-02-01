using AccessCenterDomain.AccessCenter;
using AccessCenterDomain.AccessCenter.Fractional;
using CMDomain.Models.Financeiro;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq.Functions;
using PuppeteerSharp;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Application.Models.Proprietario;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras;
using SW_PortalProprietario.Application.Models.TransacoesFinanceiras.Boleto;
using SW_PortalProprietario.Application.Models.UsuarioFinanceiro;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using SW_Utils.Auxiliar;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Helper = SW_Utils.Functions.Helper;

namespace SW_PortalProprietario.Application.Services.Providers.Esolution
{
    public class FinanceiroEsolutionService : IFinanceiroProviderService
    {
        private const string PREFIXO_TRANSACOES_FINANCEIRAS = "PORTALPROPESOL_";
        private readonly ILogger<FinanceiroEsolutionService> _logger;
        private readonly IRepositoryNHAccessCenter _repositoryAccessCenter;
        private readonly IRepositoryNH _repositorySystem;
        private readonly IServiceBase _serviceBase;
        private readonly IConfiguration _configuration;
        private readonly string idsTiposContasReceberConsiderar = "";
        private readonly string idsTiposBaixasConsiderar = "";
        private readonly string idsTiposContasReceberConsiderarBaixado = "";
        private readonly BrokerModel? _brokerModel;

        public string PrefixoTransacaoFinanceira => PREFIXO_TRANSACOES_FINANCEIRAS;
        private string CommunicationProviderName => "EsolutionProvider";

        public FinanceiroEsolutionService(ILogger<FinanceiroEsolutionService> logger,
            IConfiguration configuration,
            IRepositoryNHAccessCenter repositoryAccessCenter,
            IServiceBase serviceBase,
            IRepositoryNH repositorySystem,
            IOptions<BrokerModel> brokerConfig)
        {
            _logger = logger;
            _configuration = configuration;
            _repositoryAccessCenter = repositoryAccessCenter;
            _serviceBase = serviceBase;
            _repositorySystem = repositorySystem;
            idsTiposContasReceberConsiderar = _configuration.GetValue<string>("TiposContasReceberIdsConsiderarNoPortal", "70,363,6,9,86,87,7,12,100,101,102,112,114,115,116,117,5,8,18,20,15,21,307,463,892,898,304,422,645,923,1082,1113,183,382,1112,1114,1117,1118,1119,363,622,623,624,802,891,703,922,983,1022,822,893,896,897,10,113,282,308,202,283,483,805,13,1173,1176,1177,1178,1179,1168,1169,1174,1302,1162,1322,1167,1163,1165,1166,1170,1171,1172,1175,1164,1262,1383,1362,1423,1402,1422,63,64,65,11,16,46,66,17,19,28,72,73,75,83,84,89,62,104,118,121,47,48,49,882,883,887,305,1105,1109,1110,1106,943,627,1107,1108,885,888,45,71,85,90,120,74,14,889,1002,119,91,1384");
            idsTiposContasReceberConsiderarBaixado = _configuration.GetValue<string>("TiposContasReceberIdsConsiderarBaixados", "6,7,8,9,10,12,13,15,18,20,21,31,33,34,38,39,11,14,16,17,19,22,23,24,26,27,28,302,304,305,306,343,442,1042");
            idsTiposBaixasConsiderar = _configuration.GetValue<string>("TiposBaixaConsiderarPortal", "21,44,144,164,224,225,244,265");
            _brokerModel = brokerConfig.Value;
        }

        public string ProviderName
        {
            get
            {
                return CommunicationProviderName;
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

        public async Task<PessoaParaTransacaoBrokerModel?> GetDadosPessoa(int pessoaProviderId)
        {
            if (pessoaProviderId == 0)
                throw new ArgumentException("O parâmetro pessoaId deve ser enviado e maior que zero");

            var pessoaRetornar = (await _repositoryAccessCenter.FindBySql<PessoaParaTransacaoBrokerModel>($@"
            Select
                p.Id as PessoaId,
                p.Nascimento as DataNascimento,               
                p.Nome,
                p.Email,
                Case When p.Tipo = 'F' then 0 else 1 end as TipoPessoa,
                p.Cpf,
                p.Cnpj,
                p.Sexo,
                cid.Nome as CidadeNome,
                e.UF as EstadoSigla,
                e.Nome as EstadoNome,
                pe.Numero,
                pe.Logradouro,
                pe.Bairro,
                pe.Complemento,
                pe.Cep,
                pt.Numero as NumeroTelefone,
                tt.Nome as TipoTelefone,
                Case when Lower(pa.Nome) like 'BRA%' then 'BR' else 'EX' end as SiglaPais
            From
               Pessoa p
               Left Join PessoaEndereco pe on pe.Id = p.PessoaEnderecoPreferencial
               Left Join Cidade cid on pe.Cidade = cid.Id
               Left Join Estado e on cid.Estado = e.Id
               Left Join Pais pa on e.Pais = pa.Id
               Left Join PessoaTelefone pt on pt.Id = p.PessoaTelefonePreferencial
               Left Join TipoTelefone tt on pt.TipoTelefone = tt.Id
            Where
               p.Id = {pessoaProviderId}")).FirstOrDefault();

            return pessoaRetornar;
        }

        private async Task<PessoaParaTransacaoBrokerModel?> GetDadosEmpresa(int pessoaEmpresaId)
        {

            var pessoaRetornar = (await _repositoryAccessCenter.FindBySql<PessoaParaTransacaoBrokerModel>($@"
            Select
                p.Id as PessoaId,
                p.Nascimento as DataNascimento,               
                p.Nome,
                p.Email,
                Case When p.Tipo = 'F' then 0 else 1 end as TipoPessoa,
                p.Cpf,
                p.Cnpj,
                p.Sexo,
                cid.Nome as CidadeNome,
                e.UF as EstadoSigla,
                e.Nome as EstadoNome,
                pe.Numero,
                pe.Logradouro,
                pe.Bairro,
                pe.Complemento,
                pe.Cep,
                pt.Numero as NumeroTelefone,
                tt.Nome as TipoTelefone,
                Case when Lower(pa.Nome) like 'BRA%' then 'BR' else 'EX' end as SiglaPais
            From
               Pessoa p
               Left Join PessoaEndereco pe on pe.Id = p.PessoaEnderecoPreferencial
               Left Join Cidade cid on pe.Cidade = cid.Id
               Left Join Estado e on cid.Estado = e.Id
               Left Join Pais pa on e.Pais = pa.Id
               Left Join PessoaTelefone pt on pt.Id = p.PessoaTelefonePreferencial
               Left Join TipoTelefone tt on pt.TipoTelefone = tt.Id
            Where
               p.Id = {pessoaEmpresaId}")).FirstOrDefault();

            return pessoaRetornar;
        }

        private async Task<List<ContaPendenteModel>> GetContasGeralParaPagamento(List<int> itensToPay, string empreendimentoId, string pessoaProviderId)
        {
            var contasVinculadas = (await _repositorySystem.FindBySql<PaymentItemModel>(@$"Select 
                        pcti.* 
                    From 
                        PaymentCardTokenizedItem pcti 
                        Inner Join PaymentCardTokenized pct on pcti.PaymentCardTokenized = pct.Id
                    Where 
                        Lower(pct.Status) like '%captured%'")).AsList();

            List<int> itensJaPagos = contasVinculadas.Select(b => b.ItemId.GetValueOrDefault(0)).Distinct().AsList();

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();


            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds) ? $" and cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})"
                : $" and emp.Id in ({empreendimentoId}) ";


            var sb = new StringBuilder(@$"Select
                                        crp.Id,
                                        Coalesce(crp.DataHoraCriacao,crp.DataHoraAlteracao) as DataHoraCriacao,
                                        Case when crbp.Status <> 'C' then crbp.Id else null end as BoletoId,
                                        p.Id as PessoaProviderId,
                                        Case when crbp.Status <> 'C' then Round(Coalesce(crbp.ValorBoleto,crp.Valor),2) else crp.Valor end as Valor,
                                        Case 
                                            when tcr.Id in ({idsTiposContasReceberConsiderarBaixado}) then 'Paga'
                                            when crp.Status = 'P' then 'Em aberto' 
                                            else 'Paga' end as StatusParcela,
                                        crp.Vencimento as Vencimento,
                                        tcr.Codigo as CodigoTipoConta,
                                        tcr.Nome as NomeTipoConta,
                                        Case when crbp.Status <> 'C' then crbp.LinhaDigitavel else null end as LinhaDigitavelBoleto,
                                        cr.Observacao,
                                        p.Nome as NomePessoa,
                                        pemp.CNPJ as EmpreendimentoCnpj,
                                        'MY MABU' as EmpreendimentoNome,
                                        pemp.Id as PessoaEmpreendimentoId,
                                        i.Numero as NumeroImovel,
                                        gctc.Codigo as FracaoCota,
                                        ib.Codigo as BlocoCodigo,
                                        crbp.LimitePagamentoTransmitido,
                                        crbp.ComLimitePagamentoTra,
                                        crbp.ComLimitePagamento,
                                        crbp.ValorJuroDiario,
                                        crbp.PercentualJuroDiario,
                                        crbp.PercentualJuroMensal,
                                        crbp.ValorJuroMensal,
                                        crbp.PercentualMulta,
                                        crp.PercentualJuroDiario as PercentualJuroDiarioCar,
                                        crp.PercentualMulta as PercentualMultaCar, 
                                        (Select Max(av.Codigo) From FrAtendimentoVendaContaRec avcr Inner Join FrAtendimentoVenda av on avcr.FrAtendimentoVenda = av.Id Where avcr.ContaReceber = cr.Id) as Contrato,
                                        tcr.TaxaJuroMensalProcessamento,
                                        tcr.TaxaMultaMensalProcessamento,
                                        e.Id as EmpresaId,
                                        pemp.Nome as EmpresaNome,
                                        Nvl((Select Max(crpa.Data) From ContaReceberParcelaAlteracao crpa Where crpa.TipoContaReceber = tcr.Id and crpa.ContaReceberParcela = crp.Id),crp.DataHoraBaixa) as DataProcessamento,
                                        Nvl((Select Max(crpav.Data) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'J' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),crp.Vencimento) as DataBaseAplicacaoJurosMultas,
                                        Case when Nvl((Select Max(crpav.Id) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'M' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),0) > 1 then 'N' else 'S' end as PodeAplicarMulta,
                                        crp.DataHoraBaixa
                                        From 
                                            ContaReceberParcela crp
                                            Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id
                                            Inner Join ContaReceber cr on crp.ContaReceber = cr.Id
                                            Left Outer Join Cota co on cr.Cota = co.Id
                                            Left Outer Join GrupoCotaTipoCota gctc on co.GrupoCotaTipoCota = gctc.Id
                                            Left Outer Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                            Left Outer Join Imovel i on co.Imovel = i.Id
                                            Left Outer Join ImovelBloco ib on i.ImovelBloco = ib.Id
                                            Left Outer Join Empreendimento emp on i.Empreendimento = emp.Id
                                            Left Outer Join Filial f on emp.Filial = f.Id
                                            Left Outer Join Empresa e on f.Empresa = e.Id
                                            Left Outer Join Pessoa pemp on e.Pessoa = pemp.Id
                                            Left Outer Join
                                            (
                                            select 
	                                            crpb.ContaReceberParcela,
	                                            crb.*
	                                        from
	                                            ContaReceberParcelaBoleto crpb
                                                Inner Join ContaReceberBoleto crb on crpb.ContaReceberBoleto = crb.Id
                                                Inner Join ContaFinVariConCob cfcc on crb.ContaFinVariConCob = cfcc.Id
                                                Inner Join ContaFinanceiraVariacao cfv on cfcc.ContaFinanceiraVariacao = cfv.Id
                                                Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id
                                                Inner Join Banco b on cf.Banco = b.Id
                                            Where
                                                crpb.ContaReceberBoleto = (select Max(crpb1.ContaReceberBoleto) From ContaReceberParcelaBoleto crpb1 Where crpb1.ContaReceberParcela = crpb.ContaReceberParcela)
                                            ) crbp on crp.Id = crbp.ContaReceberParcela
                                            Inner Join Cliente cli on cr.Cliente = cli.Id
                                            Inner Join Pessoa p on cli.Pessoa = p.Id
                                        Where 
                                        crp.Status <> 'B' and
                                        tcr.Id in ({idsTiposContasReceberConsiderar}) and
                                        tcr.Id not in ({idsTiposContasReceberConsiderarBaixado}) and
                                        crp.SaldoPendente > 0 and
                                        crp.Id in ({string.Join(",", itensToPay)})
                                        {filtroEmpresa}
                                        and p.Id = {pessoaProviderId} 
                                        and exists(Select co.Proprietario From Cota co INNER JOIN cliente ccli ON co.PROPRIETARIO  = ccli.id Where ccli.PESSOA = p.Id) ");


            if (itensJaPagos.Any())
            {
                if (itensJaPagos.Count <= 1000)
                {
                    sb.AppendLine($" AND crp.Id not in ({string.Join(",", itensJaPagos)}) ");
                }
            }

            IList<ContaPendenteModel> itensEncontrados = (await _repositoryAccessCenter.FindBySql<ContaPendenteModel>(sb.ToString())).AsList();

            if (itensJaPagos.Any() && itensJaPagos.Count > 1000)
            {
                foreach (var item in itensEncontrados.Reverse())
                {
                    if (itensJaPagos.Any(b => b == item.Id))
                        itensEncontrados.Remove(item);
                }
            }

            await AtualizarValores(itensEncontrados.AsList());

            return itensEncontrados.AsList();
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoGeral(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            if (getContasParaPagamentoEmCartaoModel.PessoaId.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informado o parâmetro PessoaId para localizar contas para pagamento em cartão");

            var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaPessoaSistema($"{getContasParaPagamentoEmCartaoModel.PessoaId.GetValueOrDefault()}", CommunicationProviderName);

            if (getContasParaPagamentoEmCartaoModel.ValorTotal.GetValueOrDefault(0.00m) <= 0.00m)
                throw new ArgumentException("Deve ser informado o Valor total para pagamento em cartão");

            if (getContasParaPagamentoEmCartaoModel.ItensToPay == null || !getContasParaPagamentoEmCartaoModel.ItensToPay.Any())
                throw new ArgumentException("Deve ser informada pelo menos uma id de conta para pagamento em cartão");

            if (getContasParaPagamentoEmCartaoModel.CardTokenizedId.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informado o parâmetro CardTokenizedId");

            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            var itensEncontrados = await GetContasGeralParaPagamento(getContasParaPagamentoEmCartaoModel.ItensToPay, empreendimentoId, $"{CommunicationProviderName}");

            if (itensEncontrados.Count() != getContasParaPagamentoEmCartaoModel.ItensToPay.Count())
                throw new ArgumentException($"A quantidade de contas encontradas: {itensEncontrados.Count()} é diferente da quantidade esperada: {getContasParaPagamentoEmCartaoModel.ItensToPay.Count()}");

            if (Math.Round(itensEncontrados.Sum(a => Math.Round(a.ValorAtualizado.GetValueOrDefault(), 2)), 2) != Math.Round(getContasParaPagamentoEmCartaoModel.ValorTotal.GetValueOrDefault(), 2))
                throw new ArgumentException($"O valor total das contas encontradas: {Math.Round(itensEncontrados.Sum(a => Math.Round(a.ValorAtualizado.GetValueOrDefault(), 2)), 2):N2} é diferente do valor total esperado: {Math.Round(getContasParaPagamentoEmCartaoModel.ValorTotal.GetValueOrDefault(), 2):N2}");

            return itensEncontrados;
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteDoUsuario(SearchContasPendentesUsuarioLogado searchModel)
        {
            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();

            if (searchModel.VencimentoFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
            {
                searchModel.VencimentoFinal = DateTime.Today.AddDays(parametrosSistema!.QtdeMaximaDiasContasAVencer.GetValueOrDefault(1));
            }

            if (searchModel.VencimentoInicial.GetValueOrDefault().Date < DateTime.Today.AddYears(-100))
                searchModel.VencimentoInicial = DateTime.Today.AddYears(-100);

            if (searchModel.VencimentoInicial.GetValueOrDefault() > searchModel.VencimentoFinal.GetValueOrDefault())
                throw new ArgumentException("A data de vencimento inicial, seve ser inferir a data de vencimento final");

            if (parametrosSistema != null && parametrosSistema.ExibirContasVencidas.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)
                searchModel.VencimentoInicial = DateTime.Today;

            var loggedUser = await _repositorySystem.GetLoggedUser();
            if (loggedUser == null)
                throw new ArgumentException("Não foi possível identificar o usuário logado no sistema");

            var pessoaVinculadaSistema = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), CommunicationProviderName);
            if (pessoaVinculadaSistema == null)
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            if (string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) || !Helper.IsNumeric(pessoaVinculadaSistema.PessoaProvider))
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds) ? $" and cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})"
                : $" and emp.Id in ({empreendimentoId}) ";

            List<Parameter> parameters = new List<Parameter>()
            {
                new Parameter("vencimentoInicial",searchModel.VencimentoInicial.GetValueOrDefault()),
                new Parameter("vencimentoFinal",searchModel.VencimentoFinal.GetValueOrDefault())
            };

            var contasVinculadas = (await _repositorySystem.FindBySql<PaymentItemModel>(@$"Select 
                        pcti.* 
                    From 
                        PaymentCardTokenizedItem pcti 
                        Inner Join PaymentCardTokenized pct on pcti.PaymentCardTokenized = pct.Id
                    Where 
                        Lower(pct.Status) like '%captured%'")).AsList();

            List<int> itensJaPagos = contasVinculadas.Select(b => b.ItemId.GetValueOrDefault(0)).Distinct().AsList();

            List<int> pessoasPesquiar = new List<int>() { Convert.ToInt32(pessoaVinculadaSistema.PessoaProvider) };

            await GetOutrasPessoasVinculadas(pessoaVinculadaSistema, pessoasPesquiar);


            var sb = new StringBuilder(@$"Select
                                        crp.Id,
                                        Coalesce(crp.DataHoraCriacao,crp.DataHoraAlteracao) as DataHoraCriacao,
                                        Case when crbp.Status <> 'C' then crbp.Id else null end as BoletoId,
                                        p.Id as PessoaProviderId,
                                        Case 
                                            when crp.Status = 'P' and crbp.Status <> 'C' then Round(NVL(crbp.ValorBoleto,crp.Valor),2) 
                                            when crp.Status = 'B' then crp.Valor else Round(crp.Valor,2) end as Valor,
                                        Case when crp.ValorBaixado > crp.Valor and crp.Status = 'B' then crp.ValorBaixado else crp.Valor end as ValorAtualizado,
                                        Case 
                                            when tcr.Id in ({idsTiposContasReceberConsiderarBaixado}) then 'Paga'
                                            when crp.Status = 'P' then 'Em aberto' 
                                            else 'Paga' end as StatusParcela,
                                        crp.Vencimento as Vencimento,
                                        tcr.Codigo as CodigoTipoConta,
                                        tcr.Nome as NomeTipoConta,
                                        Case when crbp.Status <> 'C' then crbp.LinhaDigitavel else null end as LinhaDigitavelBoleto,
                                        cr.Observacao,
                                        p.Nome as NomePessoa,
                                        pemp.CNPJ as EmpreendimentoCnpj,
                                        'MY MABU' as EmpreendimentoNome,
                                        pemp.Id as PessoaEmpreendimentoId,
                                        i.Numero as NumeroImovel,
                                        gctc.Codigo as FracaoCota,
                                        ib.Codigo as BlocoCodigo,
                                        crbp.LimitePagamentoTransmitido,
                                        crbp.ComLimitePagamentoTra,
                                        crbp.ComLimitePagamento,
                                        crbp.ValorJuroDiario,
                                        crbp.PercentualJuroDiario,
                                        crbp.PercentualJuroMensal,
                                        crbp.ValorJuroMensal,
                                        crbp.PercentualMulta,
                                        crp.PercentualJuroDiario as PercentualJuroDiarioCar,
                                        crp.PercentualMulta as PercentualMultaCar, 
                                        tcr.TaxaJuroMensalProcessamento,
                                        tcr.TaxaMultaMensalProcessamento,
                                        cremp.Id as EmpresaId,
                                        empes.Nome as EmpresaNome,
                                        Nvl((Select Max(crpa.Data) From ContaReceberParcelaAlteracao crpa Where crpa.TipoContaReceber = tcr.Id and crpa.ContaReceberParcela = crp.Id and (Nvl(crp.CartaoCreditoRecorrenteStatus,'P') = 'A' or crp.DocumentoFinanceira is not null)),crp.Vencimento) as DataProcessamento,
                                        nvl((Select Max(av.Codigo) From FrAtendimentoVendaContaRec avcr Inner Join FrAtendimentoVenda av on avcr.FrAtendimentoVenda = av.Id Where avcr.ContaReceber = cr.Id),cr.Documento) as Contrato,
                                        Nvl((Select Max(crpav.Data) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'J' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),crp.Vencimento) as DataBaseAplicacaoJurosMultas,
                                        Case when Nvl((Select Max(crpav.Id) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'M' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),0) > 1 then 'N' else 'S' end as PodeAplicarMulta,
                                        crp.DataHoraBaixa
                                        From 
                                            ContaReceberParcela crp
                                            Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id
                                            Inner Join ContaReceber cr on crp.ContaReceber = cr.Id
                                            Inner Join Empresa cremp on cr.Empresa = cremp.Id
                                            Inner join Pessoa empes on cremp.Pessoa = empes.Id
                                            Inner Join Cliente cli on cr.Cliente = cli.Id
                                            Inner Join Pessoa p on cli.Pessoa = p.Id
                                            Left Outer Join Cota co on cr.Cota = co.Id
                                            Left Outer Join GrupoCotaTipoCota gctc on co.GrupoCotaTipoCota = gctc.Id
                                            Left Outer Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                            Left Outer Join Imovel i on co.Imovel = i.Id
                                            Left Outer Join ImovelBloco ib on i.ImovelBloco = ib.Id
                                            Left Outer Join Empreendimento emp on i.Empreendimento = emp.Id
                                            Left Outer Join Filial f on emp.Filial = f.Id
                                            Left Outer Join Empresa e on f.Empresa = e.Id
                                            Left Outer JOin Pessoa pemp on e.Pessoa = pemp.Id
                                            Left Outer Join
                                            (
                                            select 
	                                            crpb.ContaReceberParcela,
	                                            crb.*
	                                        from
	                                            ContaReceberParcelaBoleto crpb
                                                Inner Join ContaReceberBoleto crb on crpb.ContaReceberBoleto = crb.Id
                                                Inner Join ContaFinVariConCob cfcc on crb.ContaFinVariConCob = cfcc.Id
                                                Inner Join ContaFinanceiraVariacao cfv on cfcc.ContaFinanceiraVariacao = cfv.Id
                                                Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id
                                                Inner Join Banco b on cf.Banco = b.Id
                                            Where
                                                crpb.ContaReceberBoleto = (select Max(crpb1.ContaReceberBoleto) From ContaReceberParcelaBoleto crpb1 Where crpb1.ContaReceberParcela = crpb.ContaReceberParcela)
                                            ) crbp on crp.Id = crbp.ContaReceberParcela
                                        Where 
                                        (crp.Vencimento  >= :vencimentoInicial and 
                                        crp.Vencimento  <= :vencimentoFinal) and 
                                        tcr.Id in ({idsTiposContasReceberConsiderar})
                                        {filtroEmpresa}
                                        and (crp.Status <> 'B' or Exists(Select 
                                                                            crpb.ContaReceberParcela 
                                                                         From 
                                                                            ContaReceberParcelaBaixa crpb 
                                                                            Inner Join AgrupamConRecParcBai ag on crpb.AgrupamConRecParcBai = ag.Id 
                                                                            Inner Join TipoBaixa tb on ag.TipoBaixa = tb.Id
                                                                         Where 
                                                                            tb.Id in ({idsTiposBaixasConsiderar}) and
                                                                            crpb.ContaReceberParcela = crp.Id))
                                        and p.Id in ({string.Join(",",pessoasPesquiar)}) 
                                        and (exists(SELECT fp.Pessoa FROM FrAtendimentoVenda av INNER JOIN FrPessoa fp ON av.FrPessoa1 = fp.Id Where fp.Pessoa = p.Id AND av.Status = 'A')
                                         OR exists(SELECT fp.Pessoa FROM FrAtendimentoVenda av INNER JOIN FrPessoa fp ON av.FrPessoa2 = fp.Id Where fp.Pessoa = p.Id AND av.Status = 'A')
                                         )");

            if (itensJaPagos.Any())
            {
                if (itensJaPagos.Count <= 1000)
                {
                    sb.AppendLine($" AND crp.Id not in ({string.Join(",", itensJaPagos)}) ");
                }
            }

            var parametroSistema = await _serviceBase.GetParametroSistema();

            if (parametroSistema != null)
            {
                if (parametroSistema.ExibirContasVencidas == Domain.Enumns.EnumSimNao.Não)
                {
                    sb.AppendLine($" and crp.Vencimento >= :dataAtual");
                    parameters.Add(new Parameter("dataAtual", DateTime.Today));
                }

                if (parametroSistema.QtdeMaximaDiasContasAVencer.GetValueOrDefault(0) > 0)
                {
                    sb.AppendLine($" and crp.Vencimento <= :dataLimite");
                    parameters.Add(new Parameter("dataLimite", DateTime.Today.AddDays(parametroSistema.QtdeMaximaDiasContasAVencer.GetValueOrDefault())));
                }
            }

            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                if (searchModel.Status.Equals("P", StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AppendLine($" and (crp.SaldoPendente > 0 and crp.Status = 'P' and tcr.Id not in ({idsTiposContasReceberConsiderarBaixado})) ");
                }
                else if (searchModel.Status.Equals("V", StringComparison.InvariantCultureIgnoreCase))
                {
                    //Pendentes e vencidas
                    sb.AppendLine($" and (crp.SaldoPendente > 0 and crp.Status = 'P' and crp.Vencimento <= :today and tcr.Id not in ({idsTiposContasReceberConsiderarBaixado})) ");
                    parameters.Add(new Parameter("today", DateTime.Today));
                }
                else if (searchModel.Status.Equals("B", StringComparison.InvariantCultureIgnoreCase))
                {
                    //Baixadas
                    sb.AppendLine($" and ((crp.SaldoPendente = 0 or crp.Status = 'B') or (tcr.Id in ({idsTiposContasReceberConsiderarBaixado}))) ");
                }

            }


            var sql = sb.ToString();
            Int64 totalRegistros = 0;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 15;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
            {
                totalRegistros = await _repositoryAccessCenter.CountTotalEntry(sql, parameters.ToArray());
            }

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            sb.AppendLine(" Order by crp.Vencimento ");

            IList<ContaPendenteModel> lisResult = new List<ContaPendenteModel>();

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                lisResult = (await _repositoryAccessCenter.FindBySql<ContaPendenteModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(1), parameters.ToArray())).AsList();
            else
                lisResult = (await _repositoryAccessCenter.FindBySql<ContaPendenteModel>(sb.ToString(), parameters.ToArray())).AsList();

            if (!loggedUser.Value.isAdm && lisResult.Any())
            {
                if (!string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider))
                {
                    var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(pessoaVinculadaSistema.PessoaProvider!) });
                    if (propCache != null && propCache.Any())
                    {
                        foreach (var itens in lisResult.GroupBy(a => a.Contrato))
                        {
                            var itemFst = itens.First();
                            var itemVinculadoAoContrato = propCache.FirstOrDefault(b => 
                                ((!string.IsNullOrEmpty(b.GrupoCotaTipoCotaCodigo) && !string.IsNullOrEmpty(itemFst.FracaoCota) &&
                                b.GrupoCotaTipoCotaCodigo!.Contains(itemFst.FracaoCota)) || 
                                (!string.IsNullOrEmpty(b.NumeroContrato) && !string.IsNullOrEmpty(itemFst.Contrato) && b.NumeroContrato.Contains(itemFst.Contrato))) && 
                                b.frAtendimentoStatusCrcModels.Any(b => (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && 
                            b.AtendimentoStatusCrcStatus == "A"));

                            if (itemVinculadoAoContrato != null && itemVinculadoAoContrato.frAtendimentoStatusCrcModels.Any(b => (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && b.AtendimentoStatusCrcStatus == "A"))
                            {
                                foreach (var itemParcela in itens)
                                {
                                    itemParcela.StatusCrcBloqueiaPagamento = "S";
                                }
                            }
                        }
                    }
                    
                }
            }


            if (lisResult.Any())
            {
                if (itensJaPagos.Any() && itensJaPagos.Count > 1000)
                {
                    foreach (var item in lisResult.Reverse())
                    {
                        if (itensJaPagos.Any(b => b == item.Id))
                            lisResult.Remove(item);
                    }
                }

                await AtualizarValores(lisResult.AsList());

                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    Int64 totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), lisResult.AsList());
                }
            }

            return (1, 1, lisResult.AsList());

        }

        private async Task GetOutrasPessoasVinculadas(PessoaSistemaXProviderModel pessoaVinculadaSistema, List<int> pessoasPesquiar)
        {
            var tipoImovelPadraoBlack = _configuration.GetValue<string>("TipoImovelPadraoBlack", "1, 4, 21");
            var aplicarPadraoBlack = _configuration.GetValue<bool>("AplicarPadraoBlack", false);

            var dadosPessoa = !string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider) ?
                (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.Id = {pessoaVinculadaSistema.PessoaProvider}")).FirstOrDefault() : null;


            var outrasPessoasPesquisar = new List<AccessCenterDomain.AccessCenter.Pessoa>();

            if (dadosPessoa != null)
            {
                if (dadosPessoa.CPF.GetValueOrDefault(0) > 0)
                {
                    outrasPessoasPesquisar = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where p.CPF = {dadosPessoa.CPF.GetValueOrDefault()}")).AsList();
                }
                else if (!string.IsNullOrEmpty(dadosPessoa.eMail))
                {
                    outrasPessoasPesquisar = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Pessoa>($"From Pessoa p Where Lower(p.eMail) = '{dadosPessoa.eMail.ToLower()}'")).AsList();
                }
            }

            List<FrPessoa> todasFrPessoasVinculadas = new List<FrPessoa>();

            if (!outrasPessoasPesquisar.Any())
            {
                var frPessoa = (await _repositoryAccessCenter.FindBySql<FrPessoa>($"Select fr.* From FrPessoa fr Where fr.Pessoa = {Convert.ToInt32(pessoaVinculadaSistema.PessoaProvider)}")).FirstOrDefault();
                if (frPessoa != null)
                    todasFrPessoasVinculadas.Add(frPessoa);
            }
            else
            {
                todasFrPessoasVinculadas = (await _repositoryAccessCenter.FindBySql<FrPessoa>($"Select fr.* From FrPessoa fr Where fr.Pessoa in ({string.Join(",", outrasPessoasPesquisar.Select(b => b.Id.GetValueOrDefault()))})")).AsList();

            }


            if (todasFrPessoasVinculadas != null && todasFrPessoasVinculadas.Any())
            {
                var contratosAtivosVinculadosPessoa =
                    (await _repositoryAccessCenter.FindBySql<FrAtendimentoVenda>(@$"Select 
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
                    if (!aplicarPadraoBlack)
                    {
                        foreach (var item in contratosAtivosVinculadosPessoa)
                        {
                            item.PadraoDeCor = "Default";
                        }
                    }

                    var outrasPessoasVinculadas = (await _repositoryAccessCenter.FindBySql<AccessCenterDomain.AccessCenter.Pessoa>($@"Select
                                                                                                       p.*
                                                                                                     From 
                                                                                                       FrAtendimentoVendaContaRec avcr 
                                                                                                       Inner Join ContaReceber cr on avcr.ContaReceber = cr.Id 
                                                                                                       Inner Join Cliente cli on cr.Cliente = cli.Id
                                                                                                       Inner Join Pessoa p on cli.Pessoa = p.Id
                                                                                                     Where 
                                                                                                       avcr.FrAtendimentoVenda in ({string.Join(",", contratosAtivosVinculadosPessoa.Select(a => a.Id.GetValueOrDefault()))})")).AsList();

                    if (outrasPessoasVinculadas != null && outrasPessoasVinculadas.Any())
                    {
                        pessoasPesquiar.AddRange(outrasPessoasVinculadas.Select(b => b.Id.GetValueOrDefault()).Distinct().AsList());
                    }
                }
            }
        }

        private async Task AtualizarValores(List<ContaPendenteModel> lisResult)
        {
            
            var qtdeDiasLimparCodigoBarrasBoleto = _configuration.GetValue<int>("LimparCodigoParrasBoletoApos", 60);

            foreach (var item in lisResult.Where(b=> !string.IsNullOrEmpty(b.StatusParcela) && b.StatusParcela.Contains("abert",StringComparison.InvariantCultureIgnoreCase)))
            {
                var dataAplicacaoJuros = item.DataBaseAplicacaoJurosMultas.GetValueOrDefault(item.Vencimento.GetValueOrDefault());
                item.DataBaseAplicacaoJurosMultas = dataAplicacaoJuros;
                var podeAplicarMulta = item.PodeAplicarMulta == "S";
                item.PercentualJuroMensal = item.TaxaJuroMensalProcessamento.GetValueOrDefault(item.PercentualJuroMensal.GetValueOrDefault());
                if (item.PercentualJuroMensal.GetValueOrDefault(0) == 0 && item.PercentualJuroDiarioCar.GetValueOrDefault(0) > 0)
                    item.PercentualJuroMensal = item.PercentualJuroDiario * 30;

                if (podeAplicarMulta)
                    item.PercentualMulta = item.TaxaMultaMensalProcessamento.GetValueOrDefault(item.PercentualMultaCar.GetValueOrDefault());
                else item.PercentualMulta = 0.00m;
            }

            foreach (var item in lisResult.Where(b => (!string.IsNullOrEmpty(b.StatusParcela) && b.StatusParcela.Contains("abert", StringComparison.InvariantCultureIgnoreCase)) && 
            b.DataBaseAplicacaoJurosMultas.GetValueOrDefault() < DateTime.Today.Date &&
                (b.PercentualJuroDiario.GetValueOrDefault(0) > 0 || b.PercentualMulta.GetValueOrDefault(0) > 0)))
            {
                var totalDiasJuros = (int)((item.DataHoraBaixa.HasValue ? item.DataHoraBaixa.GetValueOrDefault().Date : DateTime.Today) - item.DataBaseAplicacaoJurosMultas.GetValueOrDefault()).TotalDays;

                if (!string.IsNullOrEmpty(item.LinhaDigitavelBoleto) && totalDiasJuros >= qtdeDiasLimparCodigoBarrasBoleto)
                    item.LinhaDigitavelBoleto = null;

                #region Apenas dias úteis comentado
                //var diasFinalSemana = 0;

                //if (item.Vencimento.GetValueOrDefault().DayOfWeek == DayOfWeek.Saturday)
                //    diasFinalSemana += 2;

                //if (item.Vencimento.GetValueOrDefault().DayOfWeek == DayOfWeek.Sunday)
                //    diasFinalSemana += 1;

                //qteDias -= diasFinalSemana; 
                #endregion

                if (totalDiasJuros > 0)
                {
                    var valorJuro = item.PercentualJuroMensal.GetValueOrDefault(0) > 0 && item.Valor.GetValueOrDefault(0) > 0 ? decimal.Round(item.PercentualJuroMensal.GetValueOrDefault() / 30 * totalDiasJuros / 100 * item.Valor.GetValueOrDefault(), 2) : 0;
                    var valorMulta = item.PercentualMulta.GetValueOrDefault(0) > 0 && item.Valor.GetValueOrDefault(0) > 0 ? decimal.Round((item.PercentualMulta.GetValueOrDefault() / 100) * item.Valor.GetValueOrDefault(), 2) : 0;
                    item.ValorAtualizado = valorJuro + item.Valor + valorMulta;
                }
                else item.ValorAtualizado = item.Valor;
            }


            foreach (var item in lisResult)
            {
                if (item.ValorAtualizado.GetValueOrDefault(0) == 0 && item.Valor > 0)
                    item.ValorAtualizado = item.Valor;
            }
            await Task.CompletedTask;
        }

        private async Task<DateTime?> GerarDataUltimoJurosAplicado(int contaReceberParcelaId)
        {

            var dataUltimoJuro = (await _repositoryAccessCenter.FindBySql<ContaReceberParcelaAltVal>(@$"Select 
                                                                                                            crpav.* 
                                                                                                        From 
                                                                                                            ContaReceberParcelaAltVal crpav 
                                                                                                            Inner Join AlteradorValor av on crpav.AlteradorValor = av.Id 
                                                                                                        Where 
                                                                                                            av.Categoria = 'J' and 
                                                                                                            av.AlteradorValorAplicacao = 'R' and
                                                                                                            crpav.ContaReceberParcela = {contaReceberParcelaId}
                                                                                                        Order by crpav.Id desc")).FirstOrDefault();

            return dataUltimoJuro?.Data ?? null;
        }

        private async Task<bool> PodeAplicarMulta(int contaReceberParcelaId)
        {
            var dataUltimaMultaAplicada = (await _repositoryAccessCenter.FindBySql<ContaReceberParcelaAltVal>(@$"Select 
                                                                                                            crpav.* 
                                                                                                        From 
                                                                                                            ContaReceberParcelaAltVal crpav 
                                                                                                            Inner Join AlteradorValor av on crpav.AlteradorValor = av.Id 
                                                                                                        Where 
                                                                                                            av.Categoria = 'M' and 
                                                                                                            av.AlteradorValorAplicacao = 'R' and
                                                                                                            crpav.ContaReceberParcela = {contaReceberParcelaId} and
                                                                                                            crpav.Estornado = 'N'
                                                                                                        Order by crpav.Id desc")).FirstOrDefault();

            return dataUltimaMultaAplicada == null;
           
        }

        public async Task<(int pageNumber, int lastPageNumber, List<ContaPendenteModel> contasPendentes)?> GetContaPendenteGeral(SearchContasPendentesGeral searchModel)
        {
            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();

            if (parametrosSistema == null)
                throw new FileNotFoundException("Não foi encontrado os parâmetros do sistema");

            if (searchModel.VencimentoFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                searchModel.VencimentoFinal = DateTime.Today.AddDays(parametrosSistema != null ? parametrosSistema.QtdeMaximaDiasContasAVencer.GetValueOrDefault(1000) : 1000);

            if (searchModel.VencimentoInicial.GetValueOrDefault().Date < DateTime.Today.AddYears(-100))
                searchModel.VencimentoInicial = DateTime.Today.AddYears(-100);

            if (searchModel.VencimentoInicial.GetValueOrDefault() > searchModel.VencimentoFinal.GetValueOrDefault())
                throw new ArgumentException("A data de vencimento inicial, seve ser inferir a data de vencimento final");

            if (parametrosSistema != null && parametrosSistema.ExibirContasVencidas.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)
                searchModel.VencimentoInicial = DateTime.Today;

            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds) ? $" and cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})"
                : $" and emp.Id in ( {empreendimentoId} ) ";


            if (searchModel.EmpresaId.GetValueOrDefault(0) > 0)
                filtroEmpresa = $" and cr.Empresa = {searchModel.EmpresaId} ";


            var contasVinculadas = (await _repositorySystem.FindBySql<PaymentItemModel>(@$"Select 
                        pcti.* 
                    From 
                        PaymentCardTokenizedItem pcti 
                        Inner Join PaymentCardTokenized pct on pcti.PaymentCardTokenized = pct.Id
                    Where 
                        Lower(pct.Status) like '%captured%'")).AsList();

            List<int> itensJaPagos = contasVinculadas.Select(b => b.ItemId.GetValueOrDefault(0)).Distinct().AsList();

            var contasVinculadasPix = (await _repositorySystem.FindBySql<PaymentItemModel>(@$"Select 
                        pcti.* 
                    From 
                        PaymentPixItem pcti 
                        Inner Join PaymentPix pct on pcti.PaymentPix = pct.Id
                    Where 
                        Lower(pct.Status) like '%captured%'")).AsList();

            if (contasVinculadasPix != null && contasVinculadasPix.Any())
                itensJaPagos.AddRange(contasVinculadasPix.Select(b => Convert.ToInt32(b.ItemId)).Distinct().AsList());

            List<Parameter> parameters = new List<Parameter>()
            {
                new Parameter("empreendimentoId", empreendimentoId),
                new Parameter("vencimentoInicial", searchModel.VencimentoInicial.GetValueOrDefault()),
                new Parameter("vencimentoFinal", searchModel.VencimentoFinal.GetValueOrDefault())
            };

            var sb = new StringBuilder(@$"Select
                                        crp.Id,
                                        Coalesce(crp.DataHoraCriacao,crp.DataHoraAlteracao) as DataHoraCriacao,
                                        Case when crbp.Status <> 'C' then crbp.Id else null end as BoletoId,
                                        p.Id as PessoaProviderId,
                                        Case 
                                            when crp.Status = 'P' and crbp.Status <> 'C' then Round(NVL(crbp.ValorBoleto,crp.Valor),2) 
                                            when crp.Status = 'B' then crp.Valor else Round(crp.Valor,2) end as Valor,
                                        Case when crp.ValorBaixado > crp.Valor and crp.Status = 'B' then crp.ValorBaixado else crp.Valor end as ValorAtualizado,
                                        Case 
                                            when tcr.Id in ({idsTiposContasReceberConsiderarBaixado}) then 'Paga'
                                            when crp.Status = 'P' then 'Em aberto' 
                                            else 'Paga' end as StatusParcela,
                                        crp.Vencimento as Vencimento,
                                        tcr.Codigo as CodigoTipoConta,
                                        tcr.Nome as NomeTipoConta,
                                        Case when crbp.Status <> 'C' then crbp.LinhaDigitavel else null end as LinhaDigitavelBoleto,
                                        cr.Observacao,
                                        p.Nome as NomePessoa,
                                        pemp.CNPJ as EmpreendimentoCnpj,
                                        'MY MABU' as EmpreendimentoNome,
                                        pemp.Id as PessoaEmpreendimentoId,
                                        i.Numero as NumeroImovel,
                                        gctc.Codigo as FracaoCota,
                                        ib.Codigo as BlocoCodigo,
                                        crbp.LimitePagamentoTransmitido,
                                        crbp.ComLimitePagamentoTra,
                                        crbp.ComLimitePagamento,
                                        crbp.ValorJuroDiario,
                                        crbp.PercentualJuroDiario,
                                        crbp.PercentualJuroMensal,
                                        crbp.ValorJuroMensal,
                                        crbp.PercentualMulta,
                                        crp.PercentualJuroDiario as PercentualJuroDiarioCar,
                                        crp.PercentualMulta as PercentualMultaCar, 
                                        tcr.TaxaJuroMensalProcessamento,
                                        tcr.TaxaMultaMensalProcessamento,
                                        cremp.Id as EmpresaId,
                                        empes.Nome as EmpresaNome,
                                        Nvl((Select Max(av.Codigo) From FrAtendimentoVendaContaRec avcr Inner Join FrAtendimentoVenda av on avcr.FrAtendimentoVenda = av.Id Where avcr.ContaReceber = cr.Id),cr.Documento) as Contrato,
                                        Nvl((Select Max(crpav.Data) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'J' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),crp.Vencimento) as DataBaseAplicacaoJurosMultas,
                                        Case when Nvl((Select Max(crpav.Id) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'M' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),0) > 1 then 'N' else 'S' end as PodeAplicarMulta,
                                        crp.DataHoraBaixa,
                                        Nvl((Select Max(crpa.Data) From ContaReceberParcelaAlteracao crpa Where crpa.TipoContaReceber = tcr.Id and crpa.ContaReceberParcela = crp.Id and (Nvl(crp.CartaoCreditoRecorrenteStatus,'P') = 'A' or crp.DocumentoFinanceira is not null)),crp.Vencimento) as DataProcessamento
                                        From 
                                            ContaReceberParcela crp
                                            Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id 
                                            Inner Join ContaReceber cr on crp.ContaReceber = cr.Id
                                            Inner Join Empresa cremp on cr.Empresa = cremp.Id
                                            Inner join Pessoa empes on cremp.Pessoa = empes.Id
                                            Left Outer Join Cota co on cr.Cota = co.Id
                                            Left Outer Join GrupoCotaTipoCota gctc on co.GrupoCotaTipoCota = gctc.Id
                                            Left Outer Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                            Left Outer Join Imovel i on co.Imovel = i.Id
                                            Left Outer Join ImovelBloco ib on i.ImovelBloco = ib.Id
                                            Left Outer Join Empreendimento emp on i.Empreendimento = emp.Id
                                            Left Outer Join Filial f on emp.Filial = f.Id
                                            Left Outer Join Empresa e on f.Empresa = e.Id
                                            Left Outer Join Pessoa pemp on e.Pessoa = pemp.Id
                                            Left Outer Join
                                            (
                                            select 
	                                            crpb.ContaReceberParcela,
	                                            crb.*
	                                        from
	                                            ContaReceberParcelaBoleto crpb
                                                Inner Join ContaReceberBoleto crb on crpb.ContaReceberBoleto = crb.Id
                                                Inner Join ContaFinVariConCob cfcc on crb.ContaFinVariConCob = cfcc.Id
                                                Inner Join ContaFinanceiraVariacao cfv on cfcc.ContaFinanceiraVariacao = cfv.Id
                                                Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id
                                                Inner Join Banco b on cf.Banco = b.Id
                                            Where
                                              crpb.ContaReceberBoleto = (select Max(crpb1.ContaReceberBoleto) From ContaReceberParcelaBoleto crpb1 Where crpb1.ContaReceberParcela = crpb.ContaReceberParcela)
                                            ) crbp on crp.Id = crbp.ContaReceberParcela
                                            Inner Join Cliente cli on cr.Cliente = cli.Id
                                            Inner Join Pessoa p on cli.Pessoa = p.Id
                                        Where 
                                        tcr.Id in ({idsTiposContasReceberConsiderar}) 
                                        and (crp.Status <> 'B' or Exists(Select 
                                                                            crpb.ContaReceberParcela 
                                                                         From 
                                                                            ContaReceberParcelaBaixa crpb 
                                                                            Inner Join AgrupamConRecParcBai ag on crpb.AgrupamConRecParcBai = ag.Id 
                                                                            Inner Join TipoBaixa tb on ag.TipoBaixa = tb.Id
                                                                         Where 
                                                                            tb.Id in ({idsTiposBaixasConsiderar}) and
                                                                            crpb.ContaReceberParcela = crp.Id))
                                        and (crp.Vencimento >= :vencimentoInicial and crp.Vencimento <= :vencimentoFinal) 
                                        {filtroEmpresa}
                                        and (exists(SELECT fp.Pessoa FROM FrAtendimentoVenda av INNER JOIN FrPessoa fp ON av.FrPessoa1 = fp.Id Where fp.Pessoa = p.Id AND av.Status = 'A')
                                         OR exists(SELECT fp.Pessoa FROM FrAtendimentoVenda av INNER JOIN FrPessoa fp ON av.FrPessoa2 = fp.Id Where fp.Pessoa = p.Id AND av.Status = 'A')
                                         )");

            if (!string.IsNullOrEmpty(searchModel.PessoaNome))
            {
                sb.AppendLine($" and Lower(p.NomePesquisa) like '%{Helper.RemoveAccents(searchModel.PessoaNome.ToLower().TrimEnd().TrimStart(), new List<string>())}%' ");
            }

            if (itensJaPagos.Any())
            {
                sb.AppendLine($" AND crp.Id not in ({string.Join(",", itensJaPagos.OrderByDescending(a => a).Take(1000))}) ");
            }

            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                if (searchModel.Status.Equals("P", StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AppendLine($" and (crp.SaldoPendente > 0 and crp.Status = 'P' and tcr.Id not in ({idsTiposContasReceberConsiderarBaixado})) ");
                }
                else if (searchModel.Status.Equals("V", StringComparison.InvariantCultureIgnoreCase))
                {
                    //Pendentes e vencidas
                    sb.AppendLine($" and (crp.SaldoPendente > 0 and crp.Status = 'P' and crp.Vencimento <= :today and tcr.Id not in ({idsTiposContasReceberConsiderarBaixado}) )");
                    parameters.Add(new Parameter("today", DateTime.Today));
                }
                else if (searchModel.Status.Equals("B", StringComparison.InvariantCultureIgnoreCase))
                {
                    //Baixadas
                    sb.AppendLine($" and ((crp.SaldoPendente = 0 or crp.Status = 'B' ) or tcr.Id in ({idsTiposContasReceberConsiderarBaixado})) ");
                }

            }
            
            //sb.AppendLine($" and rownum <= {_configuration.GetValue<int>("QtdMaximaExibicaoFinanceiros",1000)} ");


            sb.AppendLine(" Order by crp.Vencimento ");

            var sql = sb.ToString();
            Int64 totalRegistros = 0;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) == 0)
                searchModel.QuantidadeRegistrosRetornar = 15;

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0)
                searchModel.NumeroDaPagina = 1;

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
            {
                totalRegistros = await _repositoryAccessCenter.CountTotalEntry(sql, parameters.ToArray());
            }

            if (searchModel.NumeroDaPagina.GetValueOrDefault(0) == 0 ||
                totalRegistros < (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault() * searchModel.NumeroDaPagina.GetValueOrDefault()) - searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1))
            {
                long totalPage = SW_Utils.Functions.Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(100), totalRegistros);
                if (totalPage < searchModel.NumeroDaPagina)
                    searchModel.NumeroDaPagina = Convert.ToInt32(totalPage);
            }

            IList<ContaPendenteModel> lisResult = new List<ContaPendenteModel>();

            if (totalRegistros == 0)
                return (1, 1, lisResult.AsList());

            if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                lisResult = (await _repositoryAccessCenter.FindBySql<ContaPendenteModel>(sb.ToString(), searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(1), searchModel.NumeroDaPagina.GetValueOrDefault(1), parameters.ToArray())).AsList();
            else
                lisResult = (await _repositoryAccessCenter.FindBySql<ContaPendenteModel>(sb.ToString(), parameters.ToArray())).AsList();


            if (lisResult.Any())
            {
                if (itensJaPagos.Any() && itensJaPagos.Count > 1000)
                {
                    foreach (var item in lisResult.Reverse())
                    {
                        if (itensJaPagos.Any(b => b == item.Id))
                            lisResult.Remove(item);
                    }
                }

                await AtualizarValores(lisResult.AsList());

                if (searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(0) > 0)
                {
                    Int64 totalPage = Helper.TotalPaginas(searchModel.QuantidadeRegistrosRetornar.GetValueOrDefault(), totalRegistros);
                    return (searchModel.NumeroDaPagina.GetValueOrDefault(1), Convert.ToInt32(totalPage), lisResult.AsList());
                }

            }

            return (1, 1, lisResult.AsList());

        }

        public async Task<List<ContaPendenteModel>> GetContasPorIds(List<int> itensToPay)
        {
            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();


            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds) ? $" and cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})"
                : $" and emp.Id in ({empreendimentoId}) ";

            var sb = new StringBuilder(@$"Select
                                        crp.Id,
                                        Coalesce(crp.DataHoraCriacao,crp.DataHoraAlteracao) as DataHoraCriacao,
                                        Case when crbp.Status <> 'C' then crbp.Id else null end as BoletoId,
                                        p.Id as PessoaProviderId,
                                        Case when crbp.Status <> 'C' then Round(Coalesce(crbp.ValorBoleto,crp.Valor),2) else crp.Valor end as Valor,
                                        Case 
                                            when tcr.Id in ({idsTiposContasReceberConsiderarBaixado}) then 'Paga'
                                            when crp.Status = 'P' then 'Em aberto' 
                                            else 'Paga' end as StatusParcela,
                                        crp.Vencimento as Vencimento,
                                        tcr.Codigo as CodigoTipoConta,
                                        tcr.Nome as NomeTipoConta,
                                        Case when crbp.Status <> 'C' then crbp.LinhaDigitavel else null end as LinhaDigitavelBoleto,
                                        cr.Observacao,
                                        p.Nome as NomePessoa,
                                        pemp.CNPJ as EmpreendimentoCnpj,
                                        'MY MABU' as EmpreendimentoNome,
                                        pemp.Id as PessoaEmpreendimentoId,
                                        i.Numero as NumeroImovel,
                                        gctc.Codigo as FracaoCota,
                                        ib.Codigo as BlocoCodigo,
                                        crbp.LimitePagamentoTransmitido,
                                        crbp.ComLimitePagamentoTra,
                                        crbp.ComLimitePagamento,
                                        crbp.ValorJuroDiario,
                                        crbp.PercentualJuroDiario,
                                        crbp.PercentualJuroMensal,
                                        crbp.ValorJuroMensal,
                                        crbp.PercentualMulta,
                                        crp.PercentualJuroDiario as PercentualJuroDiarioCar,
                                        crp.PercentualMulta as PercentualMultaCar, 
                                        cremp.Id as EmpresaId,
                                        empes.Nome as EmpresaNome,
                                        tcr.TaxaJuroMensalProcessamento,
                                        tcr.TaxaMultaMensalProcessamento,
                                        (Select Max(av.Codigo) From FrAtendimentoVendaContaRec avcr Inner Join FrAtendimentoVenda av on avcr.FrAtendimentoVenda = av.Id Where avcr.ContaReceber = cr.Id) as Contrato
                                        From 
                                            ContaReceberParcela crp
                                            Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id
                                            Inner Join ContaReceber cr on crp.ContaReceber = cr.Id
                                            Inner Join Empresa cremp on cr.Empresa = cremp.Id
                                            Inner Join Pessoa empes on cremp.Pessoa = empes.Id
                                            Left Outer Join Cota co on cr.Cota = co.Id
                                            Left Outer Join GrupoCotaTipoCota gctc on co.GrupoCotaTipoCota = gctc.Id
                                            Left Outer Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                            Left Outer Join Imovel i on co.Imovel = i.Id
                                            Left Outer Join ImovelBloco ib on i.ImovelBloco = ib.Id
                                            Left Outer Join Empreendimento emp on i.Empreendimento = emp.Id
                                            Left Outer Join Filial f on emp.Filial = f.Id
                                            Left Outer Join Empresa e on f.Empresa = e.Id
                                            Left Outer Join Pessoa pemp on e.Pessoa = pemp.Id
                                            Left Outer Join
                                            (
                                            select 
	                                            crpb.ContaReceberParcela,
	                                            crb.*
	                                        from
	                                            ContaReceberParcelaBoleto crpb
                                                Inner Join ContaReceberBoleto crb on crpb.ContaReceberBoleto = crb.Id
                                                Inner Join ContaFinVariConCob cfcc on crb.ContaFinVariConCob = cfcc.Id
                                                Inner Join ContaFinanceiraVariacao cfv on cfcc.ContaFinanceiraVariacao = cfv.Id
                                                Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id
                                                Inner Join Banco b on cf.Banco = b.Id
	                                        Where
                                                crpb.ContaReceberBoleto = (select Max(crpb1.ContaReceberBoleto) From ContaReceberParcelaBoleto crpb1 Where crpb1.ContaReceberParcela = crpb.ContaReceberParcela)
                                            ) crbp on crp.Id = crbp.ContaReceberParcela
                                            Inner Join Cliente cli on cr.Cliente = cli.Id
                                            Inner Join Pessoa p on cli.Pessoa = p.Id
                                        Where 
                                        crp.Id in ({string.Join(",", itensToPay)}) and
                                        tcr.Id in ({idsTiposContasReceberConsiderar})
                                        {filtroEmpresa}
                                        and exists(Select co.Proprietario From Cota co INNER JOIN cliente ccli ON co.PROPRIETARIO  = ccli.id Where ccli.PESSOA = p.Id) ");

            var itensEncontrados = (await _repositoryAccessCenter.FindBySql<ContaPendenteModel>(sb.ToString())).AsList();
            return itensEncontrados;
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixGeral(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            if (getContasParaPagamentoEmPixModel.PessoaId.GetValueOrDefault(0) == 0)
                throw new ArgumentException("Deve ser informado o parâmetro PessoaId para localizar contas para pagamento em PIX");

            if (getContasParaPagamentoEmPixModel.ValorTotal.GetValueOrDefault(0.00m) <= 0.00m)
                throw new ArgumentException("Deve ser informado o Valor total para pagamento em PIX");

            if (!getContasParaPagamentoEmPixModel.ItensToPay.Any())
                throw new ArgumentException("Deve ser informada pelo menos uma id de conta para pagamento em PIX");


            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            var itensEncontrados = await GetContasGeralParaPagamento(getContasParaPagamentoEmPixModel.ItensToPay, empreendimentoId, $"{CommunicationProviderName}");

            if (itensEncontrados.Count() != getContasParaPagamentoEmPixModel.ItensToPay.Count())
                throw new ArgumentException($"A quantidade de contas encontradas: {itensEncontrados.Count()} é diferente da quantidade esperada: {getContasParaPagamentoEmPixModel.ItensToPay.Count()}");

            if (Math.Abs(Math.Round(itensEncontrados.Sum(a => Math.Round(a.Valor.GetValueOrDefault(), 2)), 2) - Math.Round(getContasParaPagamentoEmPixModel.ValorTotal.GetValueOrDefault(), 2)) != 0.00m)
                throw new ArgumentException($"O valor total das contas encontradas: {Math.Round(itensEncontrados.Sum(a => Math.Round(a.Valor.GetValueOrDefault(), 2)), 2):N2} é diferente do valor total esperado: {Math.Round(getContasParaPagamentoEmPixModel.ValorTotal.GetValueOrDefault(), 2):N2}");


            return itensEncontrados;
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmCartaoDoUsuario(DoTransactionCardInputModel getContasParaPagamentoEmCartaoModel)
        {
            var loggedUser = await _repositorySystem.GetLoggedUser();

            if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentNullException("Não foi possível identificar o usuário para comunicação com o eSolution!");


            var pessoaProvider = await _serviceBase.GetPessoaProviderVinculadaPessoaSistema($"{getContasParaPagamentoEmCartaoModel.PessoaId.GetValueOrDefault()}", CommunicationProviderName);
            if (pessoaProvider == null)
                throw new ArgumentNullException($"Não foi possível encontrar a pessoa do provider: {CommunicationProviderName} vinculada a pessoa: {getContasParaPagamentoEmCartaoModel.PessoaId.GetValueOrDefault()}");


            if (getContasParaPagamentoEmCartaoModel.ItensToPay == null || !getContasParaPagamentoEmCartaoModel.ItensToPay.Any())
                throw new ArgumentException($"Deve ser informado pelo menos uma conta a ser paga no array ItensToPay");

            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            //PessoaId:3567|UsuarioId:256

            var itensEncontrados = await GetContasDoUsuarioParaPagamento(getContasParaPagamentoEmCartaoModel.ItensToPay, empreendimentoId, pessoaProvider?.PessoaProvider ?? "");

            if (itensEncontrados.Count() != getContasParaPagamentoEmCartaoModel.ItensToPay.Count())
                throw new ArgumentException($"A quantidade de contas encontradas: {itensEncontrados.Count()} é diferente da quantidade esperada: {getContasParaPagamentoEmCartaoModel.ItensToPay.Count()}");

            if (Math.Round(itensEncontrados.Sum(a => Math.Round(a.ValorAtualizado.GetValueOrDefault(), 2)), 2) != Math.Round(getContasParaPagamentoEmCartaoModel.ValorTotal.GetValueOrDefault(), 2))
                throw new ArgumentException($"O valor total das contas encontradas: {Math.Round(itensEncontrados.Sum(a => Math.Round(a.Valor.GetValueOrDefault(), 2)), 2):N2} é diferente do valor total esperado: {Math.Round(getContasParaPagamentoEmCartaoModel.ValorTotal.GetValueOrDefault(), 2):N2}");

            return itensEncontrados;

        }

        private async Task<List<ContaPendenteModel>> GetContasDoUsuarioParaPagamento(List<int> itensToPay, string empreendimentoId, string pessoaProviderId)
        {
            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();
            if (parametrosSistema == null)
                throw new FileNotFoundException("Não foi encontrado os parâmetros do sistema.");

            if (string.IsNullOrEmpty(pessoaProviderId))
                throw new ArgumentException("Deve ser informado o parâmetro pessoaProviderId para localizar contas pendentes do usuário logado");

            var loggedUser = await _repositoryAccessCenter.GetLoggedUser();

            var pessoaVinculadaSistema = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), CommunicationProviderName);
            if (pessoaVinculadaSistema == null)
                throw new ArgumentException($"Não foi encontrada pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");


            if (!loggedUser.Value.isAdm)
            {

                if (!string.IsNullOrEmpty(pessoaVinculadaSistema.PessoaProvider))
                {
                    var propCache = await _serviceBase.GetContratos(new List<int>() { int.Parse(pessoaVinculadaSistema.PessoaProvider!) });
                    if (propCache != null && propCache.Any(b => b.frAtendimentoStatusCrcModels.Any(b => (b.BloquearCobrancaPagRec == "S" || b.BloqueaRemissaoBoletos == "S") && b.AtendimentoStatusCrcStatus == "A")))
                    {
                        throw new ArgumentException("Não foi possível localiza as contas pendentes, motivo 0001BL");
                    }
                }
            }


            var contasVinculadas = (await _repositorySystem.FindBySql<PaymentItemModel>(@$"Select 
                        pcti.* 
                    From 
                        PaymentCardTokenizedItem pcti 
                        Inner Join PaymentCardTokenized pct on pcti.PaymentCardTokenized = pct.Id
                    Where 
                        Lower(pct.Status) like '%captured%'")).AsList();

            List<int> itensJaPagos = contasVinculadas.Select(b => b.ItemId.GetValueOrDefault(0)).Distinct().AsList();


            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds) ? $" and cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})"
                : $" and emp.Id in ({empreendimentoId}) ";

            List<Parameter> parameters = new List<Parameter>();

            var sb = new StringBuilder(@$"Select
                                        crp.Id,
                                        Coalesce(crp.DataHoraCriacao,crp.DataHoraAlteracao) as DataHoraCriacao,
                                        Case when crbp.Status <> 'C' then crbp.Id else null end as BoletoId,
                                        p.Id as PessoaProviderId,
                                        Case when crbp.Status <> 'C' then Round(Coalesce(crbp.ValorBoleto,crp.Valor),2) else crp.Valor end as Valor,
                                        Case 
                                            when tcr.Id in ({idsTiposContasReceberConsiderarBaixado}) then 'Paga'
                                            when crp.Status = 'P' then 'Em aberto' 
                                            else 'Paga' end as StatusParcela,
                                        crp.Vencimento as Vencimento,
                                        tcr.Codigo as CodigoTipoConta,
                                        tcr.Nome as NomeTipoConta,
                                        Case when crbp.Status <> 'C' then crbp.LinhaDigitavel else null end as LinhaDigitavelBoleto,
                                        cr.Observacao,
                                        p.Nome as NomePessoa,
                                        pemp.CNPJ as EmpreendimentoCnpj,
                                        'MY MABU' as EmpreendimentoNome,
                                        pemp.Id as PessoaEmpreendimentoId,
                                        i.Numero as NumeroImovel,
                                        gctc.Codigo as FracaoCota,
                                        ib.Codigo as BlocoCodigo,
                                        crbp.LimitePagamentoTransmitido,
                                        crbp.ComLimitePagamentoTra,
                                        crbp.ComLimitePagamento,
                                        crbp.ValorJuroDiario,
                                        crbp.PercentualJuroDiario,
                                        crbp.PercentualJuroMensal,
                                        crbp.ValorJuroMensal,
                                        crbp.PercentualMulta,
                                        crp.PercentualJuroDiario as PercentualJuroDiarioCar,
                                        crp.PercentualMulta as PercentualMultaCar, 
                                        tcr.TaxaJuroMensalProcessamento,
                                        tcr.TaxaMultaMensalProcessamento,
                                        cremp.Id as EmpresaId,
                                        pemp.Nome as EmpresaNome,
                                        tcr.TaxaJuroMensalProcessamento,
                                        tcr.TaxaMultaMensalProcessamento,
                                        (Select Max(av.Codigo) From FrAtendimentoVendaContaRec avcr Inner Join FrAtendimentoVenda av on avcr.FrAtendimentoVenda = av.Id Where avcr.ContaReceber = cr.Id) as Contrato,
                                        Nvl((Select Max(crpav.Data) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'J' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),crp.Vencimento) as DataBaseAplicacaoJurosMultas,
                                        Case when Nvl((Select Max(crpav.Id) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'M' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),0) > 1 then 'N' else 'S' end as PodeAplicarMulta
                                        From 
                                            ContaReceberParcela crp
                                            Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id
                                            Inner Join ContaReceber cr on crp.ContaReceber = cr.Id
                                            Inner Join Empresa cremp on cr.Empresa = cremp.Id
                                            Left Outer Join Cota co on cr.Cota = co.Id
                                            Left Outer Join GrupoCotaTipoCota gctc on co.GrupoCotaTipoCota = gctc.Id
                                            Left Outer Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                            Left Outer Join Imovel i on co.Imovel = i.Id
                                            LEFT OUTER JOIN ImovelBloco ib ON i.ImovelBloco = ib.Id
                                            Left Outer Join Empreendimento emp on i.Empreendimento = emp.Id
                                            Left Outer Join Filial f on emp.Filial = f.Id
                                            Left Outer Join Empresa e on f.Empresa = e.Id
                                            Left Outer Join Pessoa pemp on e.Pessoa = pemp.Id
                                            Left Outer Join
                                            (
                                            select 
	                                            crpb.ContaReceberParcela,
	                                            crb.*
	                                        from
	                                            ContaReceberParcelaBoleto crpb
                                                Inner Join ContaReceberBoleto crb on crpb.ContaReceberBoleto = crb.Id
                                                Inner Join ContaFinVariConCob cfcc on crb.ContaFinVariConCob = cfcc.Id
                                                Inner Join ContaFinanceiraVariacao cfv on cfcc.ContaFinanceiraVariacao = cfv.Id
                                                Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id
                                                Inner Join Banco b on cf.Banco = b.Id
                                            Where
                                                crpb.ContaReceberBoleto = (select Max(crpb1.ContaReceberBoleto) From ContaReceberParcelaBoleto crpb1 Where crpb1.ContaReceberParcela = crpb.ContaReceberParcela)
                                            ) crbp on crp.Id = crbp.ContaReceberParcela
                                            Inner Join Cliente cli on cr.Cliente = cli.Id
                                            Inner Join Pessoa p on cli.Pessoa = p.Id
                                        Where 
                                        crp.Status <> 'B' and
                                        tcr.Id in ({idsTiposContasReceberConsiderar}) and
                                        tcr.Id not in ({idsTiposContasReceberConsiderarBaixado})
                                        and crp.SaldoPendente > 0 and
                                        crp.Id in ({string.Join(",", itensToPay)})
                                        {filtroEmpresa}
                                        and p.Id = {pessoaProviderId} 
                                        and exists(Select co.Proprietario From Cota co INNER JOIN cliente ccli ON co.PROPRIETARIO  = ccli.id Where ccli.PESSOA = p.Id) ");

            if (itensJaPagos.Any())
            {
                if (itensJaPagos.Count <= 1000)
                {
                    sb.AppendLine($" AND crp.Id not in ({string.Join(",", itensJaPagos)}) ");
                }
            }

            IList<ContaPendenteModel> itensEncontrados = (await _repositoryAccessCenter.FindBySql<ContaPendenteModel>(sb.ToString(), parameters.ToArray())).AsList();

            if (itensJaPagos.Any() && itensJaPagos.Count > 1000)
            {
                foreach (var item in itensEncontrados.Reverse())
                {
                    if (itensJaPagos.Any(b => b == item.Id))
                        itensEncontrados.Remove(item);
                }
            }

            await AtualizarValores(itensEncontrados.AsList());

            return itensEncontrados.AsList();
        }

        public async Task<List<ContaPendenteModel>> GetContasParaPagamentoEmPixDoUsuario(DoTransactionPixInputModel getContasParaPagamentoEmPixModel)
        {
            var loggedUser = await _repositorySystem.GetLoggedUser();

            if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentNullException("Não foi possível identificar o usuário para comunicação com o eSolution!");

            var dadosVinculacaoProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), CommunicationProviderName);
            if (dadosVinculacaoProvider == null)
                throw new ArgumentNullException($"Não foi possível encontrar a pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            if (string.IsNullOrEmpty(dadosVinculacaoProvider.PessoaProvider) || !Helper.IsNumeric(dadosVinculacaoProvider.PessoaProvider))
                throw new ArgumentException("Não foi encontrada a pessoa vinculada ao usuário logado");


            if (getContasParaPagamentoEmPixModel.ItensToPay == null || !getContasParaPagamentoEmPixModel.ItensToPay.Any())
                throw new ArgumentException($"Deve ser informado pelo menos uma conta a ser paga no array ItensToPay");


            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            //PessoaId:3567|UsuarioId:256

            var itensEncontrados = await GetContasDoUsuarioParaPagamento(getContasParaPagamentoEmPixModel.ItensToPay, empreendimentoId, dadosVinculacaoProvider.PessoaProvider);

            if (itensEncontrados.Count() != getContasParaPagamentoEmPixModel.ItensToPay.Count())
                throw new ArgumentException($"A quantidade de contas encontradas: {itensEncontrados.Count()} é diferente da quantidade esperada: {getContasParaPagamentoEmPixModel.ItensToPay.Count()}");

            var totalEncontrado = Math.Round(itensEncontrados.Sum(a => Math.Round(a.ValorAtualizado.GetValueOrDefault(), 2)), 2);

            if (Math.Abs(totalEncontrado - Math.Round(getContasParaPagamentoEmPixModel.ValorTotal.GetValueOrDefault(), 2)) != 0.00m)
                throw new ArgumentException($"O valor total das contas encontradas: {Math.Round(itensEncontrados.Sum(a => Math.Round(a.ValorAtualizado.GetValueOrDefault(), 2)), 2):N2} é diferente do valor total esperado: {Math.Round(getContasParaPagamentoEmPixModel.ValorTotal.GetValueOrDefault(), 2):N2}");

            return itensEncontrados;
        }

        public async Task<List<CotaPeriodoModel>> GetCotaPeriodo(int pessoaId, DateTime? dataInicial, DateTime? dataFinal)
        {
            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            if (pessoaId == 0)
                throw new ArgumentException("Deve ser informada a PessoaId para pesquisa de Cotas");

            List<Parameter> parameters = new List<Parameter>();

            var sb = new StringBuilder(@$"SELECT
                                        cp.Id,
                                        cp.DataHoraCriacao,
                                        cp.UsuarioCriacao,
                                        cp.Cota,
                                        i.Numero AS NumeroImovel,
                                        gctc.Codigo AS Fracao,
                                        cli.Id AS CodigoProprietario,
                                        pcli.Nome AS NomeProprietario,
                                        cp.DataInicial,
                                        cp.DataFinal,
                                        cp.OpcaoUso,
                                        cp.Pool,
                                        po.Codigo AS CodigoPool,
                                        po.Nome AS NomePool
                                        FROM
                                        CotaPeriodo cp
                                        INNER JOIN Pool po ON cp.Pool = po.Id
                                        INNER JOIN Cota c ON cp.Cota = c.Id
                                        INNER JOIN Imovel i ON c.Imovel = i.Id
                                        INNER JOIN GrupoCotaTipoCota gctc ON c.GrupoCotaTipoCota = gctc.Id
                                        INNER JOIN Empreendimento emp ON i.Empreendimento = emp.Id
                                        INNER JOIN Cliente cli ON c.Proprietario = cli.Id
                                        INNER JOIN Pessoa pcli ON cli.Pessoa = pcli.Id
                                        pcli.Id = {pessoaId} and emp.Id in ({empreendimentoId}) ");

            if (dataInicial.HasValue)
            {
                sb.AppendLine($" and cp.DataInicial >= :dataInicial");
                parameters.Add(new Parameter("dataInicial", dataInicial.Value));
            }

            if (dataFinal.HasValue)
            {
                sb.AppendLine($" and cp.DataFinal <= :dataFina");
                parameters.Add(new Parameter("dataFinal", dataFinal.Value));
            }


            return (await _repositoryAccessCenter.FindBySql<CotaPeriodoModel>(sb.ToString(), parameters.ToArray())).AsList();
        }

        public async Task<List<CotaPeriodoModel>> ProprietarioNoPoolHoje(int pessoaId)
        {
            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            if (pessoaId == 0)
                throw new ArgumentException("Deve ser informada a PessoaId para pesquisa de Cotas");

            List<Parameter> parameters = new List<Parameter>();
            parameters.Add(new Parameter("dataAtual", DateTime.Today));

            var sb = new StringBuilder(@$"SELECT
                                        cp.Id,
                                        cli.Id as Proprietario,
                                        cp.DataHoraCriacao,
                                        cp.UsuarioCriacao,
                                        cp.Cota,
                                        i.Numero AS NumeroImovel,
                                        gctc.Codigo AS Fracao,
                                        cli.Id AS CodigoProprietario,
                                        pcli.Nome AS NomeProprietario,
                                        cp.DataInicial,
                                        cp.DataFinal,
                                        cp.OpcaoUso,
                                        cp.Pool,
                                        po.Codigo AS CodigoPool,
                                        po.Nome AS NomePool
                                        FROM
                                        CotaPeriodo cp
                                        INNER JOIN Pool po ON cp.Pool = po.Id
                                        INNER JOIN Cota c ON cp.Cota = c.Id
                                        INNER JOIN Imovel i ON c.Imovel = i.Id
                                        INNER JOIN GrupoCotaTipoCota gctc ON c.GrupoCotaTipoCota = gctc.Id
                                        INNER JOIN Empreendimento emp ON i.Empreendimento = emp.Id
                                        INNER JOIN Cliente cli ON c.Proprietario = cli.Id
                                        INNER JOIN Pessoa pcli ON cli.Pessoa = pcli.Id
                                        Where
                                        pcli.Id = {pessoaId} and 
                                        @dataAtual between cp.DataInicial and cp.DataFinal and cp.OpcaoUso = 'P' and emp.Id in ({empreendimentoId}) ");


            return (await _repositoryAccessCenter.FindBySql<CotaPeriodoModel>(sb.ToString(), parameters.ToArray())).AsList();
        }

        public async Task<List<CotaPeriodoModel>> GetProprietariosParaEnvioEmail(bool pool, bool naoPool)
        {
            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            List<CotaPeriodoModel> listReturn = new List<CotaPeriodoModel>();
            List<Parameter> parameters = new List<Parameter>();
            parameters.Add(new Parameter("dataAtual", DateTime.Today.AddMonths(-5)));

            if (pool)
            {
                var sb = new StringBuilder(@$"SELECT
                                        cp.Id,
                                        cli.Id as Proprietario,
                                        cp.DataHoraCriacao,
                                        cp.UsuarioCriacao,
                                        cp.Cota,
                                        i.Numero AS NumeroImovel,
                                        gctc.Codigo AS Fracao,
                                        cli.Id AS CodigoProprietario,
                                        pcli.Nome AS NomeProprietario,
                                        cp.DataInicial,
                                        cp.DataFinal,
                                        cp.OpcaoUso,
                                        cp.Pool,
                                        po.Codigo AS CodigoPool,
                                        po.Nome AS NomePool,
                                        1 as NoPoolHoje,
                                        pcli.Email
                                        FROM
                                        CotaPeriodo cp
                                        INNER JOIN Pool po ON cp.Pool = po.Id
                                        INNER JOIN Cota c ON cp.Cota = c.Id
                                        INNER JOIN Imovel i ON c.Imovel = i.Id
                                        INNER JOIN GrupoCotaTipoCota gctc ON c.GrupoCotaTipoCota = gctc.Id
                                        INNER JOIN Empreendimento emp ON i.Empreendimento = emp.Id
                                        INNER JOIN Cliente cli ON c.Proprietario = cli.Id
                                        INNER JOIN Pessoa pcli ON cli.Pessoa = pcli.Id
                                        Where
                                        @dataAtual between cp.DataInicial and cp.DataFinal and cp.OpcaoUso = 'P' and emp.Id in ({empreendimentoId}) and 
                                        pcli.Email is not null and pcli.Email like '%@%'");


                listReturn.AddRange((await _repositoryAccessCenter.FindBySql<CotaPeriodoModel>(sb.ToString(), parameters.ToArray())).AsList());
            }

            if (naoPool)
            {
                var sb = new StringBuilder(@$"SELECT
                                        cp.Id,
                                        cli.Id as Proprietario,
                                        cp.DataHoraCriacao,
                                        cp.UsuarioCriacao,
                                        cp.Cota,
                                        i.Numero AS NumeroImovel,
                                        gctc.Codigo AS Fracao,
                                        cli.Id AS CodigoProprietario,
                                        pcli.Nome AS NomeProprietario,
                                        cp.DataInicial,
                                        cp.DataFinal,
                                        cp.OpcaoUso,
                                        cp.Pool,
                                        po.Codigo AS CodigoPool,
                                        po.Nome AS NomePool,
                                        0 as NoPoolHoje,
                                        pcli.Email
                                        FROM
                                        CotaPeriodo cp
                                        INNER JOIN Pool po ON cp.Pool = po.Id
                                        INNER JOIN Cota c ON cp.Cota = c.Id
                                        INNER JOIN Imovel i ON c.Imovel = i.Id
                                        INNER JOIN GrupoCotaTipoCota gctc ON c.GrupoCotaTipoCota = gctc.Id
                                        INNER JOIN Empreendimento emp ON i.Empreendimento = emp.Id
                                        INNER JOIN Cliente cli ON c.Proprietario = cli.Id
                                        INNER JOIN Pessoa pcli ON cli.Pessoa = pcli.Id
                                        Where
                                        @dataAtual between cp.DataInicial and cp.DataFinal and cp.OpcaoUso = 'U' and emp.Id in ({empreendimentoId}) and 
                                        pcli.Email is not null and pcli.Email like '%@%'");


                listReturn.AddRange((await _repositoryAccessCenter.FindBySql<CotaPeriodoModel>(sb.ToString(), parameters.ToArray())).AsList());
            }

            return listReturn;
        }

        public async Task<BoletoModel> DownloadBoleto(DownloadBoleto model)
        {
            if (!_repositorySystem.IsAdm)
            {
                var systemConfiguration = await _repositorySystem.GetParametroSistemaViewModel();
                if (systemConfiguration != null && systemConfiguration.HabilitarBaixarBoleto.GetValueOrDefault(Domain.Enumns.EnumSimNao.Não) == Domain.Enumns.EnumSimNao.Não)
                    throw new ArgumentException("O sistema está configurado para não permitir a baixa de boletos.");
            }

            var path = _configuration.GetValue<string>("PathGeracaoBoletos");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Não foi informado o caminho de gravação dos códigos de barras dos boletos 'GeracaoCodigoBarrasPath' no appsettings.config");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!File.Exists(Path.Combine(path, "Boleto.html")))
                throw new ArgumentException($"Não foi encontrado o arquivo base para emissão de boletos 'Boleto.html' no diretório: '{path}'");

            if (string.IsNullOrEmpty(model.LinhaDigitavelBoleto))
                throw new ArgumentException("Não foi informada a linha digitável do boleto");

            //Pego os dados para geração do boleto
            var contaPendenteVinculadaAoBoleto = await ContaPendenteDownloaBoleto(model.LinhaDigitavelBoleto);
            if (contaPendenteVinculadaAoBoleto == null)
                throw new FileNotFoundException("Não foi encontrado boleto com a linha digitável informada");

            var cliente = await GetDadosPessoa(contaPendenteVinculadaAoBoleto.PessoaProviderId.GetValueOrDefault());
            if (cliente == null)
                throw new FileNotFoundException($"Não foi encontrada a pessoa com o Id: {contaPendenteVinculadaAoBoleto.PessoaProviderId} - Sacado");

            if (cliente.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Fisica && !Helper.IsCpf(cliente.Cpf))
                throw new ArgumentException($"A pessoa: '{cliente.Nome}' não possui CPF ou o CPF informado é inválido - Sacado");

            if (cliente.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Juridica && !Helper.IsCnpj(cliente.Cnpj))
                throw new ArgumentException($"A pessoa: '{cliente.Nome}' não possui CNPJ ou o CNPJ informado é inválido - Sacado");

            var empresa = await GetDadosEmpresa(contaPendenteVinculadaAoBoleto.PessoaEmpreendimentoId.GetValueOrDefault());
            if (empresa == null)
                throw new FileNotFoundException($"Não foi encontrada a pessoa Id: {contaPendenteVinculadaAoBoleto.PessoaEmpreendimentoId} - 'Cedente'");

            if (empresa.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Fisica && !Helper.IsCpf(empresa.Cpf))
                throw new ArgumentException($"A pessoa: '{empresa.Nome}' não possui CPF ou o CPF informado é inválido - Cedente");

            if (empresa.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Juridica && !Helper.IsCnpj(empresa.Cnpj))
                throw new ArgumentException($"A pessoa: '{empresa.Nome}' não possui CNPJ ou o CNPJ informado é inválido - Cedente");

            var sacado = new BoletoSacadoModel()
            {
                Nome = cliente.Nome,
                CPFCNPJ = cliente.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Fisica ?
                Helper.FormatarCPF(Convert.ToInt64(Helper.ApenasNumeros(cliente.Cpf))) :
                Helper.FormatarCNPJ(Convert.ToInt64(Helper.ApenasNumeros(cliente.Cnpj))),
                Endereco = new BoletoEnderecoModel()
                {
                    LogradouroEndereco = cliente.Logradouro,
                    Bairro = cliente.Bairro,
                    Cidade = cliente.CidadeNome,
                    UF = cliente.EstadoSigla,
                    CEP = cliente.Cep
                }
            };

            var cpfCnpjCedente = empresa.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Fisica ?
                Helper.FormatarCPF(Convert.ToInt64(Helper.ApenasNumeros(empresa.Cpf))) :
                Helper.FormatarCNPJ(Convert.ToInt64(Helper.ApenasNumeros(empresa.Cnpj)));

            if (!string.IsNullOrEmpty(contaPendenteVinculadaAoBoleto.CnpjCedente) && !string.IsNullOrEmpty(contaPendenteVinculadaAoBoleto.Cedente))
                cpfCnpjCedente = Helper.FormatarCNPJ(Convert.ToInt64(Helper.ApenasNumeros(contaPendenteVinculadaAoBoleto.CnpjCedente)));

            var cedente = new BoletoCedenteModel()
            {
                Nome = !string.IsNullOrEmpty(contaPendenteVinculadaAoBoleto.Cedente) ? contaPendenteVinculadaAoBoleto.Cedente : empresa.Nome,
                CPFCNPJ = cpfCnpjCedente,
                Endereco = new BoletoEnderecoModel()
                {
                    LogradouroEndereco = !string.IsNullOrEmpty(contaPendenteVinculadaAoBoleto.EnderecoCedente) ? contaPendenteVinculadaAoBoleto.EnderecoCedente : empresa.Logradouro,
                    Bairro = empresa.Bairro,
                    Cidade = empresa.CidadeNome,
                    UF = empresa.EstadoSigla,
                    CEP = empresa.Cep
                }
                //,
                //ContaBancaria = new BoletoContaBancariaModel()
                //{
                //    Agencia = contaPendenteVinculadaAoBoleto.Agencia,
                //    DigitoAgencia = contaPendenteVinculadaAoBoleto.DigitoAgencia,
                //    Conta = contaPendenteVinculadaAoBoleto.Conta,
                //    DigitoConta = contaPendenteVinculadaAoBoleto.DigitoConta,
                //    CarteiraPadrao = contaPendenteVinculadaAoBoleto.CarteiraPadrao.Contains('-') ? contaPendenteVinculadaAoBoleto.VariacaoCarteiraPadrao.Split('-')[0] : contaPendenteVinculadaAoBoleto.CarteiraPadrao,
                //    VariacaoCarteiraPadrao = contaPendenteVinculadaAoBoleto.CarteiraPadrao.Contains('-') ? contaPendenteVinculadaAoBoleto.VariacaoCarteiraPadrao.Split('-')[1] : "",
                //    OperacaoConta = contaPendenteVinculadaAoBoleto.OperacaoConta
                //}
            };

            var boletoRetorno = new BoletoModel()
            {
                Id = contaPendenteVinculadaAoBoleto.Id,
                Valor = contaPendenteVinculadaAoBoleto.Valor,
                NomePessoa = sacado.Nome,
                CpfCnpjPessoa = sacado.CPFCNPJ,
                Vencimento = contaPendenteVinculadaAoBoleto.Vencimento,
                LinhaDigitavelBoleto = contaPendenteVinculadaAoBoleto.LinhaDigitavelBoleto
            };

            if (!string.IsNullOrEmpty(boletoRetorno.CpfCnpjPessoa))
            {
                if (cliente.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Fisica)
                    boletoRetorno.CpfCnpjPessoa = $"{boletoRetorno.CpfCnpjPessoa.Substring(0, 5)}**.***-{boletoRetorno.CpfCnpjPessoa.Split('-')[1]}";
                else if (cliente.TipoPessoa == Domain.Enumns.EnumTipoPessoa.Juridica)
                    boletoRetorno.CpfCnpjPessoa = $"{boletoRetorno.CpfCnpjPessoa.Substring(0, 7)}***/****-{boletoRetorno.CpfCnpjPessoa.Split('-')[1]}";

            }

            var htmlContent = File.ReadAllText(Path.Combine(path, "Boleto.html"));
            var base64Image = Helper.GerarImagemCodigoDeBarras(contaPendenteVinculadaAoBoleto.CodigoBarrasBoleto, path);

            htmlContent = htmlContent.Replace("[CodigoBarrasBoleto]", base64Image);
            htmlContent = htmlContent.Replace("[RodapeDocumento]", $"{cedente.Nome} - {cedente.CPFCNPJ}");
            htmlContent = htmlContent.Replace("[LinhaDigitavel]", $"{boletoRetorno.LinhaDigitavelBoleto}");
            htmlContent = htmlContent.Replace("[NossoNumero]", $"{contaPendenteVinculadaAoBoleto.NossoNumero}");
            htmlContent = htmlContent.Replace("[NumeroDocumento]", $"{contaPendenteVinculadaAoBoleto.NossoNumero}");
            htmlContent = htmlContent.Replace("[ValorDocumento]", $"R$ {contaPendenteVinculadaAoBoleto.Valor:N2}");
            htmlContent = htmlContent.Replace("[VencimentoDocumento]", $"{contaPendenteVinculadaAoBoleto.Vencimento:dd/MM/yyyy}");
            htmlContent = htmlContent.Replace("[NomePagador]", $"{boletoRetorno.NomePessoa} - {boletoRetorno.CpfCnpjPessoa}");
            htmlContent = htmlContent.Replace("[NomeBeneficiario]", $"{cedente.Nome} - {cedente.CPFCNPJ}");


            var launchOptions = new LaunchOptions
            {
                Headless = true // Define se o navegador será exibido ou não
            };

            // Inicializar o PuppeteerSharp
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            // Carregar o conteúdo HTML na página
            await page.SetContentAsync(htmlContent);

            var pdfBoletoPath = Path.Combine(path, $"{Guid.NewGuid()}.pdf");
            // Gerar o PDF
            await page.PdfAsync(pdfBoletoPath);

            boletoRetorno.Path = pdfBoletoPath;

            return boletoRetorno;

        }

        private async Task<ContaPendenteBoletoModel?> ContaPendenteDownloaBoleto(string linhaDigitavel)
        {
            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();


            var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds) ? $" and cr.Empresa in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})"
                : $" and emp.Id in ({empreendimentoId}) ";

            var sb = new StringBuilder(@$"Select
                                    crp.Id,
                                    Coalesce(crp.DataHoraCriacao,crp.DataHoraAlteracao) as DataHoraCriacao,
                                    Case when crbp.Status <> 'C' then crbp.IdBoleto else null end as BoletoId,
                                    p.Id as PessoaProviderId,
                                    Case when crbp.Status <> 'C' then Round(Coalesce(crbp.ValorBoleto,crp.Valor),2) else crp.Valor end as Valor,
                                    Case 
                                        when tcr.Id in ({idsTiposContasReceberConsiderarBaixado}) then 'Paga'
                                        when crp.Status = 'P' then 'Em aberto' 
                                        else 'Paga' end as StatusParcela,
                                    crp.Vencimento as Vencimento,
                                    tcr.Codigo as CodigoTipoConta,
                                    tcr.Nome as NomeTipoConta,
                                    Case when crbp.Status <> 'C' then crbp.LinhaDigitavel else null end as LinhaDigitavelBoleto,
                                    Case when crbp.Status <> 'C' then crbp.CodigoBarras else null end as CodigoBarrasBoleto,
                                    cr.Observacao,
                                    p.Nome as NomePessoa,
                                    pemp.CNPJ as EmpreendimentoCnpj,
                                    'MY MABU' as EmpreendimentoNome,
                                    pemp.Id as PessoaEmpreendimentoId,
                                    crbp.LimitePagamentoTransmitido,
                                    crbp.ComLimitePagamentoTra,
                                    crbp.ComLimitePagamento,
                                    crbp.ValorJuroDiario,
                                    crbp.PercentualJuroDiario,
                                    crbp.PercentualJuroMensal,
                                    crbp.ValorJuroMensal,
                                    crbp.PercentualMulta,
                                    crp.PercentualJuroDiario as PercentualJuroDiarioCar,
                                    crp.PercentualMulta as PercentualMultaCar, 
                                    f.Empresa as EmpresaId,
                                    pemp.Nome as EmpresaNome,
                                    tcr.TaxaJuroMensalProcessamento,
                                    tcr.TaxaMultaMensalProcessamento,
                                    (Select Max(av.Codigo) From FrAtendimentoVendaContaRec avcr Inner Join FrAtendimentoVenda av on avcr.FrAtendimentoVenda = av.Id Where avcr.ContaReceber = cr.Id) as Contrato
                                    From 
                                        ContaReceberParcela crp
                                        Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id
                                        Inner Join ContaReceber cr on crp.ContaReceber = cr.Id
                                        Inner Join Filial f on cr.Filial = f.Id
                                        Inner Join Empresa e on f.Empresa = e.Id
                                        Inner Join Pessoa pemp on e.Pessoa = pemp.Id
                                        Inner Join
                                        (
                                        select 
                                            crb.Id as IdBoleto,
                                            crpb.ContaReceberParcela,
                                            cf.AgenciaNumero,
                                            cf.AgenciaDigito,
                                            cf.ContaNumero,
                                            cf.ContaDigito,
                                            b.Codigo as BancoCodigo,
                                            cfcc.Convenio,
                                            cb.Codigo as CarteiraPadrao,
                                            crb.Vencimento,
                                            crb.NossoNumero,
                                            crb.Sequencia as SequenciaBoleto,
                                            crb.LinhaDigitavel,
                                            crb.CodigoBarras,
                                            cfv.Codigo as OperacaoConta,
                                            cf.Cedente,
                                            cf.EnderecoCedente,
                                            cf.CnpjCedente,
                                            crb.ValorBoleto,
                                            crb.LimitePagamentoTransmitido,
                                            crb.ComLimitePagamentoTra,
                                            crb.ComLimitePagamento,
                                            crb.ValorJuroDiario,
                                            crb.PercentualJuroDiario,
                                            crb.PercentualJuroMensal,
                                            crb.ValorJuroMensal,
                                            crb.PercentualMulta,
                                            crb.Status
                                        from
                                            ContaReceberParcelaBoleto crpb
                                            Inner Join ContaReceberBoleto crb on crpb.ContaReceberBoleto = crb.Id
                                            Inner Join ContaFinVariConCob cfcc on crb.ContaFinVariConCob = cfcc.Id
                                            Inner Join ContaFinanceiraVariacao cfv on cfcc.ContaFinanceiraVariacao = cfv.Id
                                            Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id
                                            Inner Join Banco b on cf.Banco = b.Id
                                            Left Outer Join CarteiraBoleto cb on cfcc.CarteiraBoleto = cb.Id
                                        Where
                                            crpb.ContaReceberBoleto = (select Max(crpb1.ContaReceberBoleto) From ContaReceberParcelaBoleto crpb1 Where crpb1.ContaReceberParcela = crpb.ContaReceberParcela)
                                        ) crbp on crp.Id = crbp.ContaReceberParcela
                                        Inner Join Cliente cli on cr.Cliente = cli.Id
                                        Inner Join Pessoa p on cli.Pessoa = p.Id
                                    Where 
                                        crp.Status <> 'B' and
                                        tcr.Id in ({idsTiposContasReceberConsiderar})
                                        and tcr.Id not in ({idsTiposContasReceberConsiderarBaixado})
                                        {filtroEmpresa}
                                        and crbp.LinhaDigitavel = '{linhaDigitavel}' and 
                                        exists(Select co.Proprietario From Cota co INNER JOIN cliente ccli ON co.PROPRIETARIO  = ccli.id Where ccli.PESSOA = p.Id) ");

            var contaPendenteEncontrada = (await _repositoryAccessCenter.FindBySql<ContaPendenteBoletoModel>(sb.ToString())).FirstOrDefault();
            return contaPendenteEncontrada;
        }

        public async Task<BaixaResultModel> BaixarValoresPagosEmPix(PaymentPix item, IStatelessSession? session)
        {
            if (_brokerModel == null || string.IsNullOrEmpty(_brokerModel.TipoBaixaPixId) || string.IsNullOrEmpty(_brokerModel.ContaFinanceiraVariacaoPixId))
                throw new ArgumentException("Não foi configurado o tipo de baixa para lançamento de pagamentos em PIX");

            var usuarioEsolutionUtilizar = _configuration.GetValue<int>("UsuarioEsolutionUtilizar", 1);

            int baixaRetornoId = 0;

            var itensVinculados = (await _repositorySystem.FindByHql<PaymentPixItem>($"From PaymentPixItem ppi Inner Join Fetch ppi.PaymentPix pp Where pp.Id = {item.Id}")).AsList();
            if (itensVinculados != null && itensVinculados.Any())
            {
                try
                {
                    _repositoryAccessCenter.BeginTransaction();

                    var contaReceberParcelas = (await _repositoryAccessCenter.FindByHql<ContaReceberParcela>(@$"From ContaReceberParcela crp Where crp.Id in ({string.Join(",", itensVinculados.Select(b => b.ItemId))}) and crp.Status = 'P'")).AsList();
                    if (!contaReceberParcelas.Any())
                        throw new FileNotFoundException($"Não foi encontrada nenhuma parcela vinculada ao pagamento Id: {item.Id}");

                    var contaReceber = (await _repositoryAccessCenter.FindByHql<ContaReceber>(@$"From ContaReceber cr Where cr.Id = {contaReceberParcelas.First().ContaReceber.GetValueOrDefault()}")).FirstOrDefault();
                    if (contaReceber == null)
                        throw new FileNotFoundException($"Não foi possível identificar a empresa relacionada ao pagamento Id: {item.Id}");

                    var empresa = (await _repositoryAccessCenter.FindByHql<Empresa>($"From Empresa emp Where emp.Id = {contaReceber.Empresa}")).FirstOrDefault();
                    if (empresa == null)
                        throw new FileNotFoundException($"Não foi possível identificar a empresa relacionada ao pagamento Id: {item.Id}");

                    var tipoBaixaPixId = _brokerModel!.GetTipoBaixaPix(contaReceber.Empresa.GetValueOrDefault());
                    var contaFinanceiraVariacaoPixId = _brokerModel!.GetContaFinanceiraVariacaoPixId(contaReceber.Empresa.GetValueOrDefault());
                    var operacaoMovimentacaoFinanceiraPixId = _brokerModel!.GetOperacaoFinanceiraPix(contaReceber.Empresa.GetValueOrDefault());

                    if (string.IsNullOrEmpty(tipoBaixaPixId) || string.IsNullOrEmpty(contaFinanceiraVariacaoPixId) || string.IsNullOrEmpty(operacaoMovimentacaoFinanceiraPixId))
                        throw new ArgumentException("Deve ser configurado os dados: 'TipoBaixaPixId'/'ContaFinanceiraVariacaoPixId'/'OperacaoFinanceiraBaixaPix'");

                    var movimentacaoFinanceira = new MovimentacaoFinanceira()
                    {
                        Data = DateTime.Today,
                        ContaFinanceiraVariacao = int.Parse(contaFinanceiraVariacaoPixId),
                        OperacaoMovFin = int.Parse(operacaoMovimentacaoFinanceiraPixId),
                        Documento = $"Pgto Pix: {item.PaymentId}",
                        Valor = item.Valor,
                        ValorDebitoCredito = "C",
                        Historico = $"Pgto Pix: {item.PaymentId}",
                        HistoricoContabil = $"Pgto Pix: {item.PaymentId}",
                        Observacao = $"Pgto Pix: {item.PaymentId}",
                        UsuarioCriacao = usuarioEsolutionUtilizar,
                        DataHoraCriacao = DateTime.Now,
                    };

                    var maxSequencia = (await _repositoryAccessCenter.FindBySql<MovimentacaoFinanceira>($"Select Max(m.Sequencia) as Sequencia From MovimentacaoFinanceira m Where m.ContaFinanceiraVariacao = {contaFinanceiraVariacaoPixId} ")).FirstOrDefault();
                    if (maxSequencia != null)
                        movimentacaoFinanceira.Sequencia = maxSequencia.Sequencia.GetValueOrDefault(0) + 1;
                    else movimentacaoFinanceira.Sequencia = 1;

                    await _repositoryAccessCenter.Save(movimentacaoFinanceira);

                    var agrupamentoBaixa = new AgrupamConRecParcBai()
                    {
                        ContaFinanceiraVariacao = int.Parse(contaFinanceiraVariacaoPixId),
                        TipoBaixa = int.Parse(tipoBaixaPixId),
                        Empresa = empresa.Id,
                        GrupoEmpresa = empresa?.GrupoEmpresa,
                        DataCredito = DateTime.Today,
                        MovimentacaoFinanceira = movimentacaoFinanceira.Id,
                        ValorRecebidoDebitoCredito = "C",
                        ValorAmortizadoDebitoCredito = "C",
                        UsuarioCriacao = usuarioEsolutionUtilizar,
                        DataHoraCriacao = DateTime.Now,
                        DataBaixa = DateTime.Now
                    };

                    List<ContaReceberParcelaBaixa> parcelasBaixas = new List<ContaReceberParcelaBaixa>();

                    contaReceberParcelas = (await _repositoryAccessCenter.FindByHql<ContaReceberParcela>(@$"From ContaReceberParcela crp Where crp.Id in ({string.Join(",", itensVinculados.Select(b => b.ItemId))}) and crp.Status = 'P'")).AsList();
                    var valorPendenteCar = contaReceberParcelas.Sum(b => b.SaldoPendente.GetValueOrDefault());
                    var jurosAplicarBaixa = item.Valor.GetValueOrDefault() - valorPendenteCar;

                    agrupamentoBaixa.ValorAmortizado = valorPendenteCar;
                    agrupamentoBaixa.ValorRecebido = item.Valor;
                    agrupamentoBaixa.ValorJuro = jurosAplicarBaixa;

                    await _repositoryAccessCenter.Save(agrupamentoBaixa);


                    decimal indiceAplicacaoJuros = jurosAplicarBaixa > 0 ? jurosAplicarBaixa / item.Valor.GetValueOrDefault() * 100 : 0;

                    foreach (var parcela in contaReceberParcelas)
                    {
                        var contaReceberParcelaBaixa = new ContaReceberParcelaBaixa()
                        {
                            ContaReceberParcela = parcela.Id,
                            Valor = parcela.SaldoPendente,
                            TipoContaReceber = parcela.TipoContaReceber,
                            ValorAmortizado = parcela.SaldoPendente,
                            DataBaixa = DateTime.Now,
                            Juro = Math.Round(parcela.SaldoPendente.GetValueOrDefault() * indiceAplicacaoJuros / 100, 2),
                            AgrupamConRecParcBai = agrupamentoBaixa.Id,
                            UsuarioCriacao = usuarioEsolutionUtilizar
                        };

                        contaReceberParcelaBaixa.Valor = Math.Round(contaReceberParcelaBaixa.ValorAmortizado.GetValueOrDefault() + contaReceberParcelaBaixa.Juro.GetValueOrDefault(), 2);
                        parcela.SaldoPendente = 0;
                        parcela.ValorBaixado = contaReceberParcelaBaixa.Valor;
                        parcela.ValorAmortizado = contaReceberParcelaBaixa.ValorAmortizado;
                        parcela.Status = "B";
                        parcela.UsuarioAlteracao = usuarioEsolutionUtilizar;
                        parcela.UsuarioBaixa = usuarioEsolutionUtilizar;
                        parcela.DataHoraBaixa = DateTime.Now;
                        parcelasBaixas.Add(contaReceberParcelaBaixa);
                    }


                    var difBaixadoXRecebido = item.Valor.GetValueOrDefault() - parcelasBaixas.Sum(b => b.Valor.GetValueOrDefault());
                    if (difBaixadoXRecebido != 0)
                    {
                        var parcelaFst = parcelasBaixas.First();
                        parcelaFst.Juro += difBaixadoXRecebido;
                        parcelaFst.Valor = Math.Round(parcelaFst.ValorAmortizado.GetValueOrDefault() + parcelaFst.Juro.GetValueOrDefault(), 2);

                        var parcela = contaReceberParcelas.First(b => b.Id == parcelaFst.ContaReceberParcela);
                        parcela.ValorBaixado = parcelaFst.Valor;

                    }

                    foreach (var itemParcela in contaReceberParcelas)
                    {
                        await _repositoryAccessCenter.Save(itemParcela);
                    }

                    foreach (var parcelaBaixa in parcelasBaixas)
                    {
                        await _repositoryAccessCenter.Save(parcelaBaixa);
                    }

                    var proximasSequencias = await ProximasMovimentacoesFinanceiras(true, movimentacaoFinanceira);
                    var sequenciaAtual = movimentacaoFinanceira.Sequencia;
                    foreach (var itemProxima in proximasSequencias)
                    {
                        itemProxima!.Sequencia = ++sequenciaAtual;
                        await _repositoryAccessCenter.Save(item);
                    }

                    await MovimentarSaldoDiario(movimentacaoFinanceira.Data, movimentacaoFinanceira.DataHoraCriacao, agrupamentoBaixa.ContaFinanceiraVariacao.GetValueOrDefault(), movimentacaoFinanceira, empresa, false);

                    await AtualizarAplicacaoCaixa(agrupamentoBaixa, movimentacaoFinanceira);

                    await BaixarBoletosVinculados(contaReceberParcelas, agrupamentoBaixa.UsuarioCriacao.GetValueOrDefault(1), agrupamentoBaixa.DataBaixa.GetValueOrDefault(DateTime.Now));

                    var commitResult = await _repositoryAccessCenter.CommitAsync();
                    if (commitResult.exception != null)
                        throw commitResult.exception;

                    baixaRetornoId = agrupamentoBaixa.Id.GetValueOrDefault();

                }
                catch (Exception err)
                {
                    _repositoryAccessCenter.Rollback();
                    _logger.LogError(err, err.Message);
                    return new BaixaResultModel()
                    {
                        Erros = new List<string>() { err.Message }
                    };
                }
            }

            return new BaixaResultModel()
            {
                Id = baixaRetornoId
            };
        }

        private async Task BaixarBoletosVinculados(List<ContaReceberParcela> contaReceberParcelas, int usuarioCancelamentoId, DateTime dataCancelamento)
        {
            var boletosVinculados = (await _repositoryAccessCenter.FindBySql<ContaReceberBoleto>(@$"
                Select  
                  crb.* 
                From
                  ContaReceberBoleto crb 
                Where 
                  Exists(Select crpb.ContaReceberBoleto From ContaReceberParcelaBoleto crpb 
                    Where 
                       crpb.ContaReceberParcela in ({string.Join(",", contaReceberParcelas.Select(b => b.Id).AsList())}) and crpb.ContaReceberBoleto = crb.Id) and 
                  crb.Status = 'P'")).AsList();

            foreach (var boleto in boletosVinculados)
            {
                boleto.Status = "C";
                boleto.ManterNossoNumero = "S";
                boleto.DataHoraCancelamento = dataCancelamento;
                boleto.UsuarioCancelamento = usuarioCancelamentoId;
                await _repositoryAccessCenter.Save(boleto);
                await _repositoryAccessCenter.ExecuteSqlCommand($"Delete From IntegracaoBancariaRegistro Where ContaReceberBoleto = {boleto.Id}");
            }
        }

        private async Task MovimentarSaldoDiario(
          DateTime? dataMovTse,
          DateTime? dataCriacao,
          int contaFinanceiraVariacao,
          MovimentacaoFinanceira movimentacaoFinanceira,
          Empresa empresa,
          bool estorno = false)
        {
            var movimentacaoFinanceiraSaldoDiario =
                estorno ?
                new MovimentacaoFinanceiraSalDia()
                {
                    ContaFinanceiraVariacao = contaFinanceiraVariacao,
                    UsuarioCriacao = movimentacaoFinanceira.UsuarioCriacao,
                    DataHoraCriacao = dataCriacao,
                    Tag = movimentacaoFinanceira.Tag,
                    Empresa = empresa.Id,
                    GrupoEmpresa = empresa.GrupoEmpresa,
                    Data = dataMovTse,
                    TotalCredito = Math.Round(movimentacaoFinanceira.ValorDebitoCredito == "D" ? movimentacaoFinanceira.Valor.GetValueOrDefault() : 0.00m, 2),
                    TotalDebito = Math.Round(movimentacaoFinanceira.ValorDebitoCredito == "D" ? 0.00m : movimentacaoFinanceira.Valor.GetValueOrDefault(), 2),
                    TotalSaldo = Math.Round(movimentacaoFinanceira.ValorDebitoCredito == "C" ? movimentacaoFinanceira.Valor.GetValueOrDefault() * (-1) : movimentacaoFinanceira.Valor.GetValueOrDefault(), 2)
                }
                :
                new MovimentacaoFinanceiraSalDia()
                {
                    ContaFinanceiraVariacao = contaFinanceiraVariacao,
                    UsuarioCriacao = movimentacaoFinanceira.UsuarioCriacao,
                    DataHoraCriacao = dataCriacao,
                    Tag = movimentacaoFinanceira.Tag,
                    Empresa = empresa.Id,
                    GrupoEmpresa = empresa.GrupoEmpresa.GetValueOrDefault(1),
                    Data = dataMovTse,
                    TotalCredito = Math.Round(movimentacaoFinanceira.ValorDebitoCredito == "D" ? 0.00m : movimentacaoFinanceira.Valor.GetValueOrDefault(), 2),
                    TotalDebito = Math.Round(movimentacaoFinanceira.ValorDebitoCredito == "D" ? movimentacaoFinanceira.Valor.GetValueOrDefault() : 0.00m, 2),
                    TotalSaldo = Math.Round(movimentacaoFinanceira.ValorDebitoCredito == "D" ? movimentacaoFinanceira.Valor.GetValueOrDefault() * (-1) : movimentacaoFinanceira.Valor.GetValueOrDefault(), 2)
                }
            ;

            await _repositoryAccessCenter.Save(movimentacaoFinanceiraSaldoDiario);
        }

        private async Task AtualizarAplicacaoCaixa(AgrupamConRecParcBai agrupamentoBaixaContaReceberVinculada, MovimentacaoFinanceira movimentacaoFinanceira)
        {
            if (agrupamentoBaixaContaReceberVinculada != null)
            {
                await _repositoryAccessCenter.ExecuteSqlCommand($"Delete From MovimentacaoFinanceiraAplCai Where MovimentacaoFinanceira = {movimentacaoFinanceira.Id}");

                var rateioAplicacaoCaixa = (await _repositoryAccessCenter.FindBySql<dynamic>(@$"Select 
                        crp.AplicacaoCaixa, 
                        Sum(crp.Valor) as Valor 
                        From 
                        ContaReceberParcelaBaixa crpb,  
                        ContaReceberParcela crp 
                        Where 
                        crpb.ContaReceberParcela = crp.Id and 
                        crpb.AgrupamConRecParcBai = {agrupamentoBaixaContaReceberVinculada.Id} 
                        Group by crp.AplicacaoCaixa ", null)).ToList();

                foreach (var rateio in rateioAplicacaoCaixa)
                {
                    var movimentacaoFinanceiraAplicacaoCaixa = new MovimentacaoFinanceiraAplCai()
                    {
                        UsuarioCriacao = movimentacaoFinanceira.UsuarioCriacao,
                        DataHoraCriacao = movimentacaoFinanceira.DataHoraCriacao,
                        Valor = rateio.Valor,
                        AplicacaoCaixa = rateio.AplicacaoCaixa,
                        MovimentacaoFinanceira = movimentacaoFinanceira.Id
                    };

                    await _repositoryAccessCenter.Save<MovimentacaoFinanceiraAplCai>(movimentacaoFinanceiraAplicacaoCaixa);
                }


                agrupamentoBaixaContaReceberVinculada.MovimentacaoFinanceira = movimentacaoFinanceira.Id;
                agrupamentoBaixaContaReceberVinculada.ContaFinanceiraVariacao = movimentacaoFinanceira.ContaFinanceiraVariacao;
                agrupamentoBaixaContaReceberVinculada.ContaFinanceiraSubVariacao = movimentacaoFinanceira.ContaFinanceiraSubVariacao;
                await _repositoryAccessCenter.Save<AgrupamConRecParcBai>(agrupamentoBaixaContaReceberVinculada);
            }
        }

        private async Task<List<MovimentacaoFinanceira?>> ProximasMovimentacoesFinanceiras(bool ignorarSequenciaAtual, MovimentacaoFinanceira movimentacaoFinanceiraAtual)
        {
            if (movimentacaoFinanceiraAtual == null || movimentacaoFinanceiraAtual.ContaFinanceiraVariacao == null ||
                movimentacaoFinanceiraAtual?.ContaFinanceiraVariacao.GetValueOrDefault(0) == 0)
                return new List<MovimentacaoFinanceira?>();

            const string sqlOrdensErradas =
                @"SELECT 
                 Count(mf.Id), 
                 Min(mf.Data) as Data,
                 mf.Sequencia
                 FROM MovimentacaoFinanceira mf INNER JOIN ContaFinanceiraVariacao cfv on mf.ContaFinanceiraVariacao = cfv.Id 
                    Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id
                    Inner JOIN Filial f on cf.Filial = f.Id
                WHERE
                    cfv.Id = :contaFinanceiraVariacaoId AND 
                    (mf.TipoDocumento = 'D' OR (mf.TipoDocumento = 'C' AND mf.ChequeStatus = 'C' )) and
                    mf.Data >= :dataBase
                Group by mf.Sequencia
                Having Count(mf.Id) > 1";

            List<Parameter> parameters = new();
            parameters.Add(new Parameter("contaFinanceiraVariacaoId", movimentacaoFinanceiraAtual?.ContaFinanceiraVariacao.GetValueOrDefault()));
            parameters.Add(new Parameter("dataBase", movimentacaoFinanceiraAtual?.Data.GetValueOrDefault().AddDays(-1).Date));


            var itensComOrdensErradas = (await _repositoryAccessCenter.FindBySql<MovimentacaoFinanceira>(sqlOrdensErradas, parameters.ToArray())).AsList();
            if (itensComOrdensErradas != null && itensComOrdensErradas.Any())
            {
                parameters = new();
                parameters.Add(new Parameter("dataInicialMenos1", itensComOrdensErradas.OrderBy(a => a.Data.GetValueOrDefault()).First().Data.GetValueOrDefault().AddDays(-1).Date));
                var movsAjustar = (await _repositoryAccessCenter.FindBySql<MovimentacaoFinanceira>($"Select m.* From MovimentacaoFinanceira m Where m.ContaFinanceiraVariacao = {movimentacaoFinanceiraAtual?.ContaFinanceiraVariacao.GetValueOrDefault()} and m.Data >= :dataInicialMenos1 Order by m.Data", parameters.ToArray())).AsList();
                Int64 sequencia = itensComOrdensErradas.OrderBy(b => b.Sequencia.GetValueOrDefault()).First().Sequencia.GetValueOrDefault();
                foreach (var itemMov in movsAjustar.OrderBy(a => a.Data))
                {
                    itemMov.Sequencia = sequencia;
                    sequencia = sequencia + 1;
                    await _repositoryAccessCenter.Save(itemMov);
                }
            }


            const string sql =
                " SELECT mf.* " +
                " FROM MovimentacaoFinanceira mf INNER JOIN ContaFinanceiraVariacao cfv on mf.ContaFinanceiraVariacao = cfv.Id " +
                " Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id " +
                " Inner JOIN Filial f on cf.Filial = f.Id " +
                " WHERE " +
                " mf.Data >= :dataItem AND " +
                " mf.Sequencia >= :ultimaSequencia " +
                " AND mf.Id <> :movimentacaoId" +
                " AND cfv.Id = :contaFinanceiraVariacaoId " +
                " AND (mf.TipoDocumento = 'D' OR (mf.TipoDocumento = 'C' AND mf.ChequeStatus = 'C' )) " +
                " ORDER BY mf.Sequencia ";

            parameters = new();
            parameters.Add(new Parameter("dataItem", movimentacaoFinanceiraAtual?.Data.GetValueOrDefault().Date));
            parameters.Add(new Parameter("movimentacaoId", movimentacaoFinanceiraAtual.Id));
            parameters.Add(new Parameter("contaFinanceiraVariacaoId", movimentacaoFinanceiraAtual.ContaFinanceiraVariacao.GetValueOrDefault()));
            if (ignorarSequenciaAtual)
            {
                parameters.Add(new Parameter("ultimaSequencia", (movimentacaoFinanceiraAtual?.Sequencia ?? 0) + 1));
            }
            else
            {
                parameters.Add(new Parameter("ultimaSequencia", (movimentacaoFinanceiraAtual?.Sequencia ?? 0)));
            }

            return (await _repositoryAccessCenter.FindBySql<MovimentacaoFinanceira>(sql, parameters.ToArray())).AsList();

        }

        public async Task<BaixaResultModel> AlterarTipoContaReceberPagasEmCartao(PaymentCardTokenized item, IStatelessSession? session)
        {
            var usuarioEsolutionUtilizar = _configuration.GetValue<int>("UsuarioEsolutionUtilizar", 1);

            var itensVinculados = (await _repositorySystem.FindByHql<PaymentCardTokenizedItem>($"From PaymentCardTokenizedItem ppi Inner Join Fetch ppi.PaymentCardTokenized pp Where pp.Id = {item.Id}")).AsList();
            if (itensVinculados != null && itensVinculados.Any())
            {
                try
                {
                    _repositoryAccessCenter.BeginTransaction();

                    var contaReceberParcelas = (await _repositoryAccessCenter.FindByHql<ContaReceberParcela>(@$"From ContaReceberParcela crp Where crp.Id in ({string.Join(",", itensVinculados.Select(b => b.ItemId))}) and crp.Status = 'P'")).AsList();
                    if (!contaReceberParcelas.Any())
                        throw new FileNotFoundException($"Não foi encontrada nenhuma parcela vinculada ao pagamento Id: {item.Id}");

                    var contaReceber = (await _repositoryAccessCenter.FindByHql<ContaReceber>(@$"From ContaReceber cr Where cr.Id = {contaReceberParcelas.First().ContaReceber.GetValueOrDefault()}")).FirstOrDefault();
                    if (contaReceber == null)
                        throw new FileNotFoundException($"Não foi possível identificar a empresa relacionada ao pagamento Id: {item.Id}");

                    var empresa = (await _repositoryAccessCenter.FindByHql<Empresa>($"From Empresa emp Where emp.Id = {contaReceber.Empresa}")).FirstOrDefault();
                    if (empresa == null)
                        throw new FileNotFoundException($"Não foi possível identificar a empresa relacionada ao pagamento Id: {item.Id}");

                    var valorPendenteCar = contaReceberParcelas.Sum(b => b.SaldoPendente.GetValueOrDefault());
                    var jurosAplicarBaixa = item.Valor.GetValueOrDefault() - valorPendenteCar;

                    decimal indiceAplicacaoJuros = jurosAplicarBaixa > 0 ? jurosAplicarBaixa / item.Valor.GetValueOrDefault() * 100 : 0;

                    Financeira? financeiraUtilizar = null;
                    if (!string.IsNullOrEmpty(item.Adquirente))
                    {
                        if (item.Adquirente.Contains("cielo", StringComparison.CurrentCultureIgnoreCase))
                        {
                            financeiraUtilizar = (await _repositoryAccessCenter.FindBySql<Financeira>($"Select f.* From Financeira f Where f.Empresa = {empresa?.Id} and Lower(f.Nome) like '%cielo%'")).FirstOrDefault();
                            if (financeiraUtilizar == null)
                                throw new FileNotFoundException($"Não foi encontrada a financeira Cielo na empresa de Id: {empresa?.Id}");
                        }
                        else if (item.Adquirente.Contains("rede", StringComparison.CurrentCultureIgnoreCase))
                        {
                            financeiraUtilizar = (await _repositoryAccessCenter.FindBySql<Financeira>($"Select f.* From Financeira f Where f.Empresa = {empresa?.Id} and Lower(f.Nome) like '%rede%'")).FirstOrDefault();
                            if (financeiraUtilizar == null)
                                throw new FileNotFoundException($"Não foi encontrada a financeira Rede na empresa de Id: {empresa?.Id}");
                        }
                        else if (item.Adquirente.Contains("safra", StringComparison.CurrentCultureIgnoreCase))
                        {
                            financeiraUtilizar = (await _repositoryAccessCenter.FindBySql<Financeira>($"Select f.* From Financeira f Where f.Empresa = {empresa?.Id} and Lower(f.Nome) like '%safra%'")).FirstOrDefault();
                            if (financeiraUtilizar == null)
                                throw new FileNotFoundException($"Não foi encontrada a financeira Safra na empresa de Id: {empresa?.Id}");
                        }
                    }

                    if (financeiraUtilizar == null)
                        throw new FileNotFoundException($"Não foi possível identificar a financeira onde foi realizada a transação de cartão id: {item.Id} - adquirente: {item.Adquirente}");

                    var tiposContaReceberFinanceira = (await _repositoryAccessCenter.FindBySql<TipoContaReceber>($"Select tcr.* From TipoContaReceber tcr Where tcr.Financeira = {financeiraUtilizar.Id} and tcr.Empresa = {empresa?.Id} and tcr.Status = 'A' and Lower(tcr.Nome) not like '%recorrente%'")).AsList();
                    var cardTokenized = (await _repositorySystem.FindByHql<CardTokenized>($"From CardTokenized ct Inner Join Fetch ct.Pessoa p Where ct.Id = {item.CardTokenized?.Id}")).FirstOrDefault();
                    if (!tiposContaReceberFinanceira.Any())
                        throw new FileNotFoundException($"Não foi encontrado nenhum tipo de conta a receber vinculado a financeira: {financeiraUtilizar.Nome}");

                    var tipoContaReceberUtilizar = tiposContaReceberFinanceira.FirstOrDefault(b => b.Nome.Contains($"{cardTokenized?.Brand}", StringComparison.CurrentCultureIgnoreCase));
                    if (tipoContaReceberUtilizar == null)
                    {
                        tipoContaReceberUtilizar = tiposContaReceberFinanceira.FirstOrDefault();
                    }
                    if (tipoContaReceberUtilizar == null)
                        throw new FileNotFoundException($"Não foi encontrado o tipo de conta a receber para '{cardTokenized?.Brand}' na financeira: {financeiraUtilizar.Nome}");


                    var parcelasModel = await GetContasPendenteParaBaixa(contaReceberParcelas.Select(b => b.Id.GetValueOrDefault()).Distinct().AsList());
                    foreach (var itemParcela in parcelasModel)
                    {
                        itemParcela.DataHoraBaixa = item.DataHoraCriacao.GetValueOrDefault().Date;

                    }
                    await AtualizarValores(parcelasModel);

                    var totalParcelasVinculadas = contaReceberParcelas.Sum(b => b.SaldoPendente.GetValueOrDefault());
                    decimal totalAcrescimosAplicados = 0.00m;
                    List<ContaReceberParcelaAltVal> alteradoresGerados = new List<ContaReceberParcelaAltVal>();



                    var dif = item.Valor.GetValueOrDefault() - totalParcelasVinculadas;
                    if (dif > 0)
                    {
                        var alteradorJuros = (await _repositoryAccessCenter.FindBySql<AlteradorValor>($"Select av.* From AlteradorValor av Where av.Empresa = {empresa?.Id} and lower(av.Nome) like '%juros%' AND av.AlteradorValorAplicacao = 'R' and av.Categoria = 'J' and av.Status = 'A'")).FirstOrDefault();
                        if (alteradorJuros == null)
                            throw new FileNotFoundException($"Não foi encontrado o alterador de valor para aplicação de juros na empresa: {empresa?.Id}");

                        totalAcrescimosAplicados = await CorrigirValores(item, usuarioEsolutionUtilizar, contaReceberParcelas, tipoContaReceberUtilizar, totalAcrescimosAplicados, alteradoresGerados, alteradorJuros, parcelasModel);

                    }

                    var difAplicarUltimaParcela = dif - totalAcrescimosAplicados;
                    if (difAplicarUltimaParcela != 0)
                    {
                        var alteradorValorModificar = alteradoresGerados.FirstOrDefault();
                        if (alteradorValorModificar == null)
                            throw new Exception("Não foi possível corrigir a(s) parcela(s), não foi encontrado alteradores de valores gerados durante o processo.");

                        var parcelaAplicarAjuste = contaReceberParcelas.FirstOrDefault(b => b.Id == alteradorValorModificar.ContaReceberParcela.GetValueOrDefault());
                        if (parcelaAplicarAjuste == null)
                            throw new Exception("Não foi encontrada a parcela para aplicação da correção de valores");

                        parcelaAplicarAjuste.Valor += difAplicarUltimaParcela;
                        parcelaAplicarAjuste.SaldoPendente += difAplicarUltimaParcela;
                        alteradorValorModificar.Valor += difAplicarUltimaParcela;
                        await _repositoryAccessCenter.Save(alteradorValorModificar);
                        totalAcrescimosAplicados += difAplicarUltimaParcela;
                    }

                    difAplicarUltimaParcela = dif - totalAcrescimosAplicados;
                    if (difAplicarUltimaParcela != 0.00m)
                        throw new Exception($"Erro na correção de parcelas, diferença encontrada: {difAplicarUltimaParcela:N2}");

                    foreach (var parcela in contaReceberParcelas)
                    {
                        var tipoContaAnterior = parcela.TipoContaReceber;
                        parcela.TipoContaReceber = tipoContaReceberUtilizar.Id;
                        parcela.AutorizacaoCartao = item.CodigoAutorizacao;
                        parcela.DocumentoFinanceira = item.Nsu;
                        parcela.Vencimento = item.DataHoraCriacao.GetValueOrDefault().Date.AddDays(30);
                        parcela.Financeira = financeiraUtilizar.Id;
                        parcela.UsuarioAlteracao = usuarioEsolutionUtilizar;
                        parcela.DataHoraAlteracao = DateTime.Now;

                        await _repositoryAccessCenter.Save(parcela);

                        var parcelaBase = parcelasModel.First(a => a.Id == parcela.Id);

                        var contaReceberParcelaAlteracao = new ContaReceberParcelaAlteracao()
                        {
                            ContaReceberParcela = parcela.Id,
                            Data = DateTime.Now,
                            UsuarioCriacao = usuarioEsolutionUtilizar,
                            DataHoraCriacao = DateTime.Now,
                            VencimentoAnterior = parcelaBase.Vencimento,
                            NovoVencimento = parcela.Vencimento,
                            Valor = parcela.Valor,
                            Cliente = parcelaBase.ClienteId,
                            ClienteAnterior = parcelaBase.ClienteId,
                            TipoContaReceber = parcela.TipoContaReceber,
                            TipoContaReceberAnterior = tipoContaAnterior
                        };
                        await _repositoryAccessCenter.Save(contaReceberParcelaAlteracao);
                    }

                    await BaixarBoletosVinculados(contaReceberParcelas, usuarioEsolutionUtilizar, DateTime.Now);

                    item.ParcelasSincronizadas = 1;

                    var commitResult = await _repositoryAccessCenter.CommitAsync();
                    if (commitResult.exception != null)
                        throw commitResult.exception;

                }
                catch (Exception err)
                {
                    _repositoryAccessCenter.Rollback();
                    _logger.LogError(err, err.Message);
                    return new BaixaResultModel()
                    {
                        Erros = new List<string>() { err.Message }
                    };
                }
            }

            return new BaixaResultModel()
            {
                Id = 1,
                Erros = new List<string>()
            };
        }

        private async Task<decimal> CorrigirValores(PaymentCardTokenized item, int usuarioEsolutionUtilizar, List<ContaReceberParcela> contaReceberParcelas, TipoContaReceber tipoContaReceberUtilizar, decimal totalAcrescimosAplicados, List<ContaReceberParcelaAltVal> alteradoresGerados, AlteradorValor alteradorJuros, List<ContaPendenteModel> parcelasModel)
        {
            foreach (var parcela in contaReceberParcelas)
            {

                var parcelaModelBase = parcelasModel.FirstOrDefault(a => a.Id == parcela.Id);
                if (parcelaModelBase != null && parcelaModelBase.ValorAtualizado > parcela.SaldoPendente)
                {
                    decimal difAplicarParcela = parcelaModelBase.ValorAtualizado.GetValueOrDefault() - parcela.SaldoPendente.GetValueOrDefault();
                    var contaReceberParcelaAltVal = new ContaReceberParcelaAltVal()
                    {
                        AlteradorValor = alteradorJuros.Id,
                        ContaReceberParcela = parcela.Id,
                        Data = item.DataHoraCriacao.GetValueOrDefault().Date,
                        Valor = difAplicarParcela,
                        DataProvisao = item.DataHoraCriacao.GetValueOrDefault().Date.AddMonths(1).Date,
                        DataOriginal = item.DataHoraCriacao.GetValueOrDefault().Date,
                        DataProvisaoOriginal = item.DataHoraCriacao.GetValueOrDefault().Date.AddMonths(1).Date,
                        Observacao = $"Juros aplicados para pagamento em cartão na transação: {item.TransactionId}",
                        TipoContaReceber = tipoContaReceberUtilizar.Id,
                        UsuarioCriacao = usuarioEsolutionUtilizar,
                        DataHoraCriacao = DateTime.Now
                    };
                    parcela.Valor += difAplicarParcela;
                    parcela.SaldoPendente += difAplicarParcela;
                    totalAcrescimosAplicados += difAplicarParcela;
                    await _repositoryAccessCenter.Save(contaReceberParcelaAltVal);
                    alteradoresGerados.Add(contaReceberParcelaAltVal);
                }

            }

            return totalAcrescimosAplicados;
        }

        private async Task<List<ContaPendenteModel>> GetContasPendenteParaBaixa(List<int> contaReceberParcelaBaixarIds)
        {
            var sb = new StringBuilder(@$"Select
                                        crp.Id,
                                        Coalesce(crp.DataHoraCriacao,crp.DataHoraAlteracao) as DataHoraCriacao,
                                        Case when crbp.Status <> 'C' then crbp.Id else null end as BoletoId,
                                        p.Id as PessoaProviderId,
                                        Case when crbp.Status <> 'C' then Round(Coalesce(crbp.ValorBoleto,crp.Valor),2) else crp.Valor end as Valor,
                                        Case 
                                            when tcr.Id in ({idsTiposContasReceberConsiderarBaixado}) then 'Paga'
                                            when crp.Status = 'P' then 'Em aberto' 
                                            else 'Paga' end as StatusParcela,
                                        crp.Vencimento as Vencimento,
                                        tcr.Codigo as CodigoTipoConta,
                                        tcr.Nome as NomeTipoConta,
                                        Case when crbp.Status <> 'C' then crbp.LinhaDigitavel else null end as LinhaDigitavelBoleto,
                                        cr.Observacao,
                                        p.Nome as NomePessoa,
                                        pemp.CNPJ as EmpreendimentoCnpj,
                                        emp.Nome as EmpreendimentoNome,
                                        pemp.Id as PessoaEmpreendimentoId,
                                        i.Numero as NumeroImovel,
                                        gctc.Codigo as FracaoCota,
                                        ib.Codigo as BlocoCodigo,
                                        crbp.LimitePagamentoTransmitido,
                                        crbp.ComLimitePagamentoTra,
                                        crbp.ComLimitePagamento,
                                        crbp.ValorJuroDiario,
                                        crbp.PercentualJuroDiario,
                                        crbp.PercentualJuroMensal,
                                        crbp.ValorJuroMensal,
                                        crbp.PercentualMulta,
                                        crp.PercentualJuroDiario as PercentualJuroDiarioCar,
                                        crp.PercentualMulta as PercentualMultaCar, 
                                        tcr.TaxaJuroMensalProcessamento,
                                        tcr.TaxaMultaMensalProcessamento,
                                        (Select Max(av.Codigo) From FrAtendimentoVendaContaRec avcr Inner Join FrAtendimentoVenda av on avcr.FrAtendimentoVenda = av.Id Where avcr.ContaReceber = cr.Id) as Contrato,
                                        cremp.Id as EmpresaId,
                                        empes.Nome as EmpresaNome,
                                        Nvl((Select Max(crpav.Data) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'J' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),crp.Vencimento) as DataBaseAplicacaoJurosMultas,
                                        Case when Nvl((Select Max(crpav.Id) From ContaReceberParcelaAltVal crpav Inner Join AlteradorValor alv on crpav.AlteradorValor = alv.Id and alv.AlteradorValorAplicacao = 'R' and alv.Categoria = 'M' Where crpav.Estornado = 'N' and crpav.ContaReceberParcela = crp.Id),0) > 1 then 'N' else 'S' end as PodeAplicarMulta,
                                        crp.DataHoraBaixa
                                        From 
                                            ContaReceberParcela crp
                                            Inner Join TipoContaReceber tcr on crp.TipoContaReceber = tcr.Id
                                            Inner Join ContaReceber cr on crp.ContaReceber = cr.Id
                                            Inner Join Empresa cremp on cr.Empresa = cremp.Id
                                            Inner Join Pessoa empes on cremp.Pessoa = empes.Id
                                            Inner Join Cliente cli on cr.Cliente = cli.Id
                                            Inner Join Pessoa p on cli.Pessoa = p.Id
                                            Left Outer Join Cota co on cr.Cota = co.Id
                                            Left Outer Join GrupoCotaTipoCota gctc on co.GrupoCotaTipoCota = gctc.Id
                                            Left Outer Join GrupoCota gc on gctc.GrupoCota = gc.Id
                                            Left Outer Join Imovel i on co.Imovel = i.Id
                                            Left Outer Join ImovelBloco ib on i.ImovelBloco = ib.Id
                                            Left Outer Join Empreendimento emp on i.Empreendimento = emp.Id
                                            Left Outer Join Filial f on emp.Filial = f.Id
                                            Left Outer Join Empresa e on f.Empresa = e.Id
                                            Left Outer JOin Pessoa pemp on e.Pessoa = pemp.Id
                                            Left Outer Join
                                            (
                                            select 
	                                            crpb.ContaReceberParcela,
	                                            crb.*
	                                        from
	                                            ContaReceberParcelaBoleto crpb
                                                Inner Join ContaReceberBoleto crb on crpb.ContaReceberBoleto = crb.Id
                                                Inner Join ContaFinVariConCob cfcc on crb.ContaFinVariConCob = cfcc.Id
                                                Inner Join ContaFinanceiraVariacao cfv on cfcc.ContaFinanceiraVariacao = cfv.Id
                                                Inner Join ContaFinanceira cf on cfv.ContaFinanceira = cf.Id
                                                Inner Join Banco b on cf.Banco = b.Id
                                            Where
                                                crpb.ContaReceberBoleto = (select Max(crpb1.ContaReceberBoleto) From ContaReceberParcelaBoleto crpb1 Where crpb1.ContaReceberParcela = crpb.ContaReceberParcela)
                                            ) crbp on crp.Id = crbp.ContaReceberParcela
                                        Where 
                                        crp.Status <> 'B' and
                                        tcr.Id in ({idsTiposContasReceberConsiderar}) and
                                        tcr.Id not in ({idsTiposContasReceberConsiderarBaixado})
                                        and crp.SaldoPendente > 0
                                        and crp.Id in ({string.Join(",", contaReceberParcelaBaixarIds)}) 
                                        and exists(Select co.Proprietario From Cota co INNER JOIN cliente ccli ON co.PROPRIETARIO  = ccli.id Where ccli.PESSOA = p.Id) ");

            return (await _repositoryAccessCenter.FindBySql<ContaPendenteModel>(sb.ToString())).AsList();
        }

        public async Task<bool> VoltarParaTiposOriginais(PaymentCardTokenized item, IStatelessSession? session)
        {
            var usuarioEsolutionUtilizar = _configuration.GetValue<int>("UsuarioEsolutionUtilizar", 1);

            var empresaId = _configuration.GetValue<int>("Empresa", 2);

            var empresa = (await _repositoryAccessCenter.FindByHql<Empresa>($"From Empresa emp Where emp.Id = {empresaId}")).FirstOrDefault();

            var itensVinculados = (await _repositorySystem.FindByHql<PaymentCardTokenizedItem>($"From PaymentCardTokenizedItem ppi Inner Join Fetch ppi.PaymentCardTokenized pp Where pp.Id = {item.Id}")).AsList();
            if (itensVinculados != null && itensVinculados.Any())
            {
                try
                {
                    //_repositoryAccessCenter.BeginTransaction();


                    //var contaReceberParcelas = (await _repositoryAccessCenter.FindByHql<ContaReceberParcela>(@$"From ContaReceberParcela crp Where crp.Id in ({string.Join(",", itensVinculados.Select(b => b.ItemId))}) and crp.Status = 'P'")).AsList();
                    //var valorPendenteCar = contaReceberParcelas.Sum(b => b.SaldoPendente.GetValueOrDefault());
                    //var jurosAplicarBaixa = item.Valor.GetValueOrDefault() - valorPendenteCar;

                    //decimal indiceAplicacaoJuros = jurosAplicarBaixa > 0 ? jurosAplicarBaixa / item.Valor.GetValueOrDefault() * 100 : 0;

                    //foreach (var parcela in contaReceberParcelas)
                    //{

                    //}

                    //var difBaixadoXRecebido = item.Valor.GetValueOrDefault() - parcelasBaixas.Sum(b => b.Valor.GetValueOrDefault());
                    //if (difBaixadoXRecebido != 0)
                    //{
                    //    var parcelaFst = parcelasBaixas.First();
                    //    parcelaFst.Juro += difBaixadoXRecebido;
                    //    parcelaFst.Valor = Math.Round(parcelaFst.ValorAmortizado.GetValueOrDefault() + parcelaFst.Juro.GetValueOrDefault(), 2);

                    //    var parcela = contaReceberParcelas.First(b => b.Id == parcelaFst.ContaReceberParcela);
                    //    parcela.ValorBaixado = parcelaFst.Valor;

                    //}

                    //foreach (var itemParcela in contaReceberParcelas)
                    //{
                    //    await _repositoryAccessCenter.Save(itemParcela);
                    //}


                    //var commitResult = await _repositoryAccessCenter.CommitAsync();
                    //if (commitResult.exception != null)
                    //    throw commitResult.exception;

                }
                catch (Exception err)
                {
                    //_repositoryAccessCenter.Rollback();
                    _logger.LogError(err, err.Message);
                }
            }

            return true;
        }

        public async Task<int> SalvarContaBancaria(ClienteContaBancariaInputModel model)
        {
            try
            {
                _repositoryAccessCenter.BeginTransaction();

                ClienteContaBancaria? clienteContaBancaria = await SalvarContaBancariaInterna(model);

                var resultCommit = await _repositoryAccessCenter.CommitAsync();
                if (resultCommit.executed)
                {
                    return clienteContaBancaria!.Id.GetValueOrDefault();
                }

                throw resultCommit.exception ?? new Exception("Erro na operação");

            }
            catch (Exception)
            {
                _repositoryAccessCenter.Rollback();
                throw;
            }
        }

        public async Task<List<ClienteContaBancariaViewModel>> GetContasBancarias(int pessoaId)
        {

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();
            if (parametrosSistema == null || string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds))
                throw new FileNotFoundException("Não foi possível identificar o parâmetro vinculado a empresa");

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
                                                    ccb.ChavePix
                                                    From 
                                                    ClienteContaBancaria ccb 
                                                    Left Join Cliente cli on ccb.Cliente = cli.Id
                                                    Left Join Banco b on ccb.Banco = b.Id
                                                    Left Join Cidade c on ccb.Cidade = c.Id
                                                    Left Join Estado e on c.Estado = e.Id
                                                    Where cli.Pessoa = {pessoaId} and ccb.Status = 'A' and 
                                                    cli.Empresa in ({string.Join(",", parametrosSistema.ExibirFinanceirosDasEmpresaIds.Split(','))}) and 
                                                    Exists(Select f.Empresa From Empreendimento ef Inner Join Filial f on ef.Filial = f.Id Where f.Empresa = cli.Empresa) ");



            var clienteContaBancaria = (await _repositoryAccessCenter.FindBySql<ClienteContaBancariaViewModel>(sb.ToString())).AsList();

            foreach (var item in clienteContaBancaria)
            {
                if (item.Tipo == "P" && !string.IsNullOrEmpty(item.ChavePix))
                {
                    item.NomeNormalizado = $"Pix {item.DescricaoTipoChavePix} - {item.ChavePix}";
                }
                else if (!string.IsNullOrEmpty(item.NomeBanco))
                {
                    item.NomeNormalizado = $"Banco: {item.CodigoBanco} C/C: {item.ContaNumero}";
                    if (!string.IsNullOrEmpty(item.ContaDigito))
                        item.NomeNormalizado += $"-{item.ContaDigito}";

                    if (!string.IsNullOrEmpty(item.ChavePix))
                    {
                        item.NomeNormalizado += $" (Pix {item.DescricaoTipoChavePix} - {item.ChavePix})";
                    }
                }
            }

            return clienteContaBancaria;
        }

        public async Task<ClienteContaBancaria?> SalvarContaBancariaInterna(ClienteContaBancariaInputModel model, ParametroSistemaViewModel? parametroSistemaDefault = null)
        {
            var usuarioEsolutionUtilizar = _configuration.GetValue<int>("UsuarioEsolutionUtilizar", 1);

            var parametrosSistema = parametroSistemaDefault ?? await _repositorySystem.GetParametroSistemaViewModel();
            if (parametrosSistema == null)
                throw new FileNotFoundException("Não foi encontrado os parâmetros para a empresa logada");

            var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
            if (string.IsNullOrEmpty(empreendimentoId))
                throw new ArgumentException("Empreendimento não configurado.");

            var empreendimento =
                model.EmpresaId.GetValueOrDefault(0) > 0 &&
                model.EmpreendimentoId.GetValueOrDefault(0) > 0 ? new AccessCenterDomain.AccessCenter.Empreendimento() { Id = model.EmpreendimentoId, Empresa = model.EmpresaId } :
                (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Empreendimento>($"From Empreendimento emp Where emp.Id in ({empreendimentoId})")).FirstOrDefault();

            if (empreendimento == null)
                throw new ArgumentException($"Não foi encontrado o empreendimento com o Id: {empreendimentoId}");

            var filtroEmpresa = model.EmpresasIds != null && model.EmpresasIds.Any() ? $" and e.Id in ({model.EmpresasIds}) " : "";

            if (string.IsNullOrEmpty(filtroEmpresa))
            {
                filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds) ? $" and e.Id in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})"
                : $" and e.Id = {empreendimento.Empresa.GetValueOrDefault()} ";
            }

            var cliente = (await _repositoryAccessCenter.FindById<Cliente>(model.ClienteId.GetValueOrDefault()));
            if (cliente == null)
                throw new Exception($"Cliente com id: {model.ClienteId} não encontrado!");

            var chavePreenchida = !string.IsNullOrEmpty(model.ChavePix) && !string.IsNullOrEmpty(model.TipoChavePix);

            if (string.IsNullOrEmpty(model.Agencia) && !chavePreenchida)
                throw new Exception("A agência da conta bancária deve ser informada");

            if (string.IsNullOrEmpty(model.ContaNumero) && !chavePreenchida)
                throw new Exception("O número da conta bancária deve ser informado");

            if (!string.IsNullOrEmpty(model.CodigoBanco))
            {
                var banco = (await _repositoryAccessCenter.FindByHql<Banco>($"From Banco b Where Lower(b.Codigo) = '{model.CodigoBanco}'")).FirstOrDefault();
                if (banco == null && !chavePreenchida)
                    throw new Exception($"Não foi encontrado Banco com código: {model.CodigoBanco}!");

                if (banco != null)
                    model.IdBanco = banco.Id;
            }
            else if (model.IdBanco > 0)
            {
                var banco = (await _repositoryAccessCenter.FindByHql<Banco>($"From Banco b Where b.Id = {model.IdBanco}")).FirstOrDefault();
                if (banco == null && !chavePreenchida)
                    throw new Exception($"Não foi encontrado Banco com id: {model.IdBanco}!");

            }

            if (!string.IsNullOrEmpty(model.TipoChavePix) && !new List<string>() { "F", "J", "E", "T", "A" }.Any(c => c.Equals(model.TipoChavePix, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new ArgumentException($"O tipo de chave informado: '{model.TipoChavePix}' é inválido. Os valores permitidos são: F = CPF, J = CNPJ, E = email, T = Telefone e A = Chave aleatória.");
            }

            if (!string.IsNullOrEmpty(model.TipoChavePix) && string.IsNullOrEmpty(model.ChavePix))
                throw new ArgumentException("Deve ser informada a chave pix");


            if (!string.IsNullOrEmpty(model.TipoChavePix) && !string.IsNullOrEmpty(model.ChavePix))
            {
                if (model.TipoChavePix == "F")
                {
                    var cpfValido = Domain.Functions.Helper.IsCpf(model.ChavePix);
                    if (!cpfValido)
                        throw new ArgumentException("O CPF informado para a chave pix não é válido.");
                }
                else if (model.TipoChavePix == "J")
                {
                    var cnpjValido = Domain.Functions.Helper.IsCnpj(model.ChavePix);
                    if (!cnpjValido)
                        throw new ArgumentException("O CNPJ informado para a chave pix não é válido.");
                }
                else if (model.TipoChavePix == "E")
                {
                    var emailValido = model.ChavePix.Contains("@");
                    if (!emailValido)
                        throw new ArgumentException("O e-mail informado para a chave pix não é válido.");
                }
                else if (model.TipoChavePix == "T")
                {
                    var length = SW_Utils.Functions.Helper.RemoveAccents(model.ChavePix, new List<string>() { "(", ")", "-", " " }).Length;
                    var telefoneValido = length == 11 || length == 10 || length == 13;
                    if (!telefoneValido)
                        throw new ArgumentException("O telefone informado para a chave pix não é válido.");
                }

            }

            List<ClienteContaBancaria> contasBancariasCriadas = new List<ClienteContaBancaria>();
            var clienteBase = await _repositoryAccessCenter.FindById<Cliente>(model.ClienteId.GetValueOrDefault());
            if (clienteBase == null)
                throw new FileNotFoundException($"Não foi encontrado o cliente com o Id informado: {model.ClienteId.GetValueOrDefault()}");

            foreach (var item in parametrosSistema.ExibirFinanceirosDasEmpresaIds!.Split(','))
            {

                var clienteNaEmpresa = (await _repositoryAccessCenter.FindByHql<Cliente>(@$"From 
                                                                                            Cliente cli 
                                                                                           Where
                                                                                            cli.Empresa = {item} and 
                                                                                            cli.Pessoa = {clienteBase.Pessoa.GetValueOrDefault()}")).FirstOrDefault();

                if (clienteNaEmpresa == null) continue;

                var contasBancariasCliente = (await _repositoryAccessCenter.FindByHql<ClienteContaBancaria>($"From ClienteContaBancaria ccb Where ccb.Cliente = {clienteNaEmpresa.Id.GetValueOrDefault()}")).AsList();

                if ((model.IdBanco.GetValueOrDefault(0) > 0 && !string.IsNullOrEmpty(model.ContaNumero)) || !string.IsNullOrEmpty(model.ChavePix))
                {
                    var contaUtilizar = contasBancariasCliente.FirstOrDefault(c => ((c.Banco.GetValueOrDefault(0) == model.IdBanco.GetValueOrDefault(0) &&
                    !string.IsNullOrEmpty(c.Conta) && !string.IsNullOrEmpty(model.ContaNumero) && c.Conta.TrimEnd() == model.ContaNumero?.TrimEnd()) ||
                    (!string.IsNullOrEmpty(c.ChavePix) && !string.IsNullOrEmpty(model.ChavePix) && c.ChavePix == model.ChavePix) && c.Id != model.Id));
                    if (contaUtilizar != null)
                    {
                        contasBancariasCriadas.Add(contaUtilizar);
                        continue;
                    }
                }

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


                var clienteContaBancaria = model.Id > 0 && contasBancariasCliente != null && contasBancariasCliente.Count() > 0 ? contasBancariasCliente.First(c => c.Id == model.Id)
                    : new ClienteContaBancaria()
                    {
                        UsuarioCriacao = usuarioEsolutionUtilizar,
                        DataHoraCriacao = DateTime.Now,
                        Banco = model.IdBanco,
                        Cliente = clienteNaEmpresa.Id,
                        Conta = model.ContaNumero,
                        ContaDigito = !string.IsNullOrWhiteSpace(model.ContaDigito) ? model.ContaDigito.TrimEnd().TrimStart() : "0",
                        Variacao = model.Variacao,
                        TipoConta = model.Variacao ?? "C",
                        Cidade = model.IdCidade,
                        Agencia = model.Agencia,
                        AgenciaDigito = !string.IsNullOrWhiteSpace(model.AgenciaDigito) ? model.AgenciaDigito.TrimEnd().TrimStart() : "0",
                        Preferencial = model.Preferencial.GetValueOrDefault(false) ? "S" : "N",
                        TipoChavePix = model.TipoChavePix,
                        ChavePix = model.ChavePix,
                        Tipo = !string.IsNullOrEmpty(model.ContaNumero) ? "B" : "P",
                        InformaPix = !string.IsNullOrEmpty(model.ChavePix) ? "S" : "N"

                    };

                if (!string.IsNullOrEmpty(model.Status))
                {
                    clienteContaBancaria.Status = model.Status.StartsWith("I", StringComparison.CurrentCultureIgnoreCase) ? "I" : "A";
                }
                else
                {
                    if (clienteContaBancaria.Id == 0)
                        clienteContaBancaria.Status = "A";
                }

                if (!contasBancariasCliente.Any())
                    clienteContaBancaria.Preferencial = "S";

                clienteContaBancaria.Tipo = model.IdBanco > 0 && !string.IsNullOrEmpty(model.ContaNumero) ? "B" : "P";

                if (clienteContaBancaria.Tipo == "P" && !chavePreenchida)
                    throw new ArgumentException("Para cadastrar uma conta bancária do tipo PIX, é necessário preencher o tipo de chave pix e a chave pix");

                if (!string.IsNullOrEmpty(model.ChavePix))
                {
                    var outraContaComMesmaChave = (await _repositoryAccessCenter.FindBySql<ClienteContaBancaria>($@"Select 
                                                                                                                        cb.* 
                                                                                                                    From 
                                                                                                                        ClienteContaBancaria cb 
                                                                                                                    Where 
                                                                                                                         Upper(cb.ChavePix) = '{model.ChavePix.ToUpper()}' and 
                                                                                                                         cb.Id <> {clienteContaBancaria.Id.GetValueOrDefault(0)} and 
                                                                                                                         cb.Cliente = {clienteNaEmpresa.Id}")).FirstOrDefault();
                    if (outraContaComMesmaChave != null)
                        throw new ArgumentException($"A chave pix informada: '{model.ChavePix}' já está em uso na conta bancária: {outraContaComMesmaChave.Id}");
                }


                await _repositoryAccessCenter.Save(clienteContaBancaria);

                if (clienteContaBancaria.Preferencial == "S")
                {
                    foreach (var contaBancariaJaExistente in contasBancariasCliente.Where(c => c.Id != clienteContaBancaria.Id))
                    {
                        clienteContaBancaria.Preferencial = "N";
                        await _repositoryAccessCenter.Save(contaBancariaJaExistente);
                    }
                }

                contasBancariasCriadas.Add(clienteContaBancaria);
            }

            return contasBancariasCriadas.Any() ? contasBancariasCriadas.First() : default;
        }

        public async Task<int> SalvarMinhaContaBancaria(ClienteContaBancariaInputModel model)
        {
            try
            {
                _repositoryAccessCenter.BeginTransaction();

                var loggedUser = await _repositorySystem.GetLoggedUser();

                if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentNullException("Não foi possível identificar o usuário para comunicação com o eSolution!");

                var dadosVinculacaoProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), CommunicationProviderName);
                if (dadosVinculacaoProvider == null)
                    throw new ArgumentNullException($"Não foi possível encontrar a pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

                if (string.IsNullOrEmpty(dadosVinculacaoProvider.PessoaProvider) || !Helper.IsNumeric(dadosVinculacaoProvider.PessoaProvider))
                    throw new ArgumentException("Não foi encontrada a pessoa vinculada ao usuário logado");

                var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();
                if (parametrosSistema == null)
                    throw new FileNotFoundException("Não foi encontrado os parâmetros para a empresa logada");

                var empreendimentoId = _configuration.GetValue<string>("EmpreendimentoId", "1,21");
                if (string.IsNullOrEmpty(empreendimentoId))
                    throw new ArgumentException("Empreendimento não configurado.");

                var empreendimento = (await _repositoryAccessCenter.FindByHql<AccessCenterDomain.AccessCenter.Empreendimento>($"From Empreendimento emp Where emp.Id in ({empreendimentoId})")).FirstOrDefault();

                if (empreendimento == null)
                    throw new ArgumentException($"Não foi encontrado o empreendimento com o Id: {empreendimentoId}");

                model.EmpreendimentoId = empreendimento?.Id;
                model.EmpresaId = empreendimento?.Empresa;

                var filtroEmpresa = !string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds) ? $" and e.Id in ({parametrosSistema.ExibirFinanceirosDasEmpresaIds.TrimEnd()})"
                    : $" and e.Id = {empreendimento!.Empresa.GetValueOrDefault()} ";

                var cliente = (await _repositoryAccessCenter.FindByHql<Cliente>(@$"From 
                                                                                     Cliente cli 
                                                                                     Inner Join FETCH cli.Empresa e 
                                                                                     Inner Join Fetch cli.Pessoa p 
                                                                                   Where 
                                                                                     p.Id = {dadosVinculacaoProvider.PessoaProvider} {parametrosSistema}")).AsList();

                if (cliente == null || !cliente.Any())
                    throw new ArgumentException($"Não foi encontrado um cliente no legado com a pessoa vinculada ao usuário logado: {loggedUser.Value.userId}");

                model.ClienteId = cliente.First().Id;

                var contaBancariaResult = await SalvarContaBancariaInterna(model, parametrosSistema);

                var resultCommit = await _repositoryAccessCenter.CommitAsync();
                if (resultCommit.executed)
                {
                    return contaBancariaResult!.Id.GetValueOrDefault();
                }

                throw resultCommit.exception ?? new Exception("Erro na operação");

            }
            catch (Exception err)
            {
                _repositoryAccessCenter.Rollback();
                throw err;
            }
        }

        public async Task<List<ClienteContaBancariaViewModel>> GetMinhasContasBancarias()
        {
            var loggedUser = await _repositorySystem.GetLoggedUser();

            var parametrosSistema = await _repositorySystem.GetParametroSistemaViewModel();
            if (parametrosSistema == null || string.IsNullOrEmpty(parametrosSistema.ExibirFinanceirosDasEmpresaIds))
                throw new FileNotFoundException("Não foi possível identificar o parâmetro vinculado a empresa");


            if (loggedUser == null || string.IsNullOrEmpty(loggedUser.Value.providerKeyUser) || !loggedUser.Value.providerKeyUser.Contains("PessoaId", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentNullException("Não foi possível identificar o usuário para comunicação com o eSolution!");

            var dadosVinculacaoProvider = await _serviceBase.GetPessoaProviderVinculadaUsuarioSistema(Convert.ToInt32(loggedUser.Value.userId), CommunicationProviderName);
            if (dadosVinculacaoProvider == null)
                throw new ArgumentNullException($"Não foi possível encontrar a pessoa do provider: {CommunicationProviderName} vinculada ao usuário logado: {loggedUser.Value.userId}");

            if (string.IsNullOrEmpty(dadosVinculacaoProvider.PessoaProvider) || !Helper.IsNumeric(dadosVinculacaoProvider.PessoaProvider))
                throw new ArgumentException("Não foi encontrada a pessoa vinculada ao usuário logado");

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
                                                    Where cli.Pessoa = {dadosVinculacaoProvider.PessoaProvider} and ccb.Status = 'A' and 
                                                          cli.Empresa in ({string.Join(",", parametrosSistema.ExibirFinanceirosDasEmpresaIds.Split(','))}) and 
                                                          Exists(Select f.Empresa From Empreendimento ef Inner Join Filial f on ef.Filial = f.Id Where f.Empresa = cli.Empresa) ");



            var clienteContaBancaria = (await _repositoryAccessCenter.FindBySql<ClienteContaBancariaViewModel>(sb.ToString())).AsList();
            foreach (var item in clienteContaBancaria)
            {
                if (item.Tipo == "P" && !string.IsNullOrEmpty(item.ChavePix))
                {
                    item.NomeNormalizado = $"Pix {item.DescricaoTipoChavePix} ({item.ChavePix})";
                }
                else if (!string.IsNullOrEmpty(item.NomeBanco))
                {
                    item.NomeNormalizado = $"Banco: {item.CodigoBanco} C/C: {item.ContaNumero}";
                    if (!string.IsNullOrEmpty(item.ContaDigito))
                        item.NomeNormalizado += $"-{item.ContaDigito}";

                    if (!string.IsNullOrEmpty(item.ChavePix))
                    {
                        item.NomeNormalizado += $" Pix {item.DescricaoTipoChavePix} ({item.ChavePix})";
                    }
                }
            }


            return clienteContaBancaria;
        }

        private async Task<List<StatusCrcContratoModel>?> GetStatusCrcPorTipoStatusIds(List<int> statusCrcIds)
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
                                            avcrc.Status = 'A' and 
                                            st.Id in ({string.Join(",", statusCrcIds)})");

            return (await _repositoryAccessCenter.FindBySql<StatusCrcContratoModel>(sqlStatusCrc.ToString())).AsList();
        }

    }
}
