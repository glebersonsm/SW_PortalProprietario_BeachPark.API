namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscal : EntityBase
    {
        public virtual string SincronizarMovimentacaoEf { get; set; } = "N";
        public virtual string EntradaSaida { get; set; } = "E";
        public virtual int? Filial { get; set; }
        public virtual int? Almoxarifado { get; set; }
        public virtual int? AlmoxarifadoDestino { get; set; }
        public virtual string TipoPessoa { get; set; } = "C";
        public virtual int? Cliente { get; set; }
        public virtual int? ClienteEfetivo { get; set; }
        public virtual int? ClienteAutorizado { get; set; }
        public virtual int? Cartao { get; set; }
        public virtual int? Fazenda { get; set; }
        public virtual int? FazendaClienteEfetivo { get; set; }
        public virtual string EmissaoPropria { get; set; } = "N";
        public virtual string OptanteSimples { get; set; } = "N";
        public virtual string InformarImpostos { get; set; } = "N";
        public virtual string ConferiuTotaisNoRecebimento { get; set; } = "N";
        public virtual string Fomentar { get; set; } = "N";
        public virtual string Contabilizar { get; set; } = "N";
        public virtual string IgnorarChecagemLancFrete { get; set; } = "N";
        public virtual decimal? QtdeItensImpXmlNfe { get; set; }
        public virtual int? ModeloNotaFiscal { get; set; }
        public virtual int? SerieNotaFiscal { get; set; }
        public virtual int? Ecf { get; set; }
        public virtual int? Crz { get; set; }
        public virtual int? PDV { get; set; }
        public virtual string Importacao { get; set; } = "S";
        public virtual string TipoRateio { get; set; } = "P";
        public virtual Int64? Numero { get; set; }
        public virtual int? NaturezaOperacao { get; set; }
        public virtual DateTime? DataHoraEmissao { get; set; }
        public virtual DateTime? DataMovimentacao { get; set; }
        public virtual decimal? ValorBaseCalculoIcms { get; set; }
        public virtual decimal? AliquotaIcmsSimplesNacional { get; set; }
        public virtual decimal? ValorIcms { get; set; }
        public virtual decimal? ValorIcmsOriginal { get; set; }
        public virtual decimal? ValorOutrosIcms { get; set; }
        public virtual decimal? ValorIsentoIcms { get; set; }
        public virtual decimal? ValorBaseCalculoIcmsSub { get; set; }
        public virtual decimal? ValorIcmsSubstituicao { get; set; }
        public virtual decimal? ValorBaseCalculoIPI { get; set; }
        public virtual decimal? ValorIpi { get; set; }
        public virtual decimal? ValorDare { get; set; }
        public virtual decimal? ValorBaseCalculoIss { get; set; }
        public virtual decimal? ValorIss { get; set; }
        public virtual decimal? ValorOutrosIss { get; set; }
        public virtual decimal? ValorIsentoIss { get; set; }
        public virtual decimal? ValorInss { get; set; }
        public virtual decimal? ValorSenar { get; set; }
        public virtual string InssLiminar { get; set; } = "N";
        public virtual decimal? ValorInssTomador { get; set; }
        public virtual decimal? ValorIrrf { get; set; }
        public virtual decimal? ValorPis { get; set; }
        public virtual decimal? ValorCofins { get; set; }
        public virtual decimal? DescontoFinanceiro { get; set; }
        public virtual decimal? ValorContribuicaoSocial { get; set; }
        public virtual string RetemIss { get; set; } = "N";
        public virtual string RetemInss { get; set; } = "N";
        public virtual string RetemIr { get; set; } = "N";
        public virtual string RetemPis { get; set; } = "N";
        public virtual string RetemCofins { get; set; } = "N";
        public virtual string RetemContribuicaoSocial { get; set; } = "N";
        public virtual string MovimentarEstoqueNota { get; set; } = "N";
        public virtual string Socio { get; set; } = "N";
        public virtual string NotaFiscalFolhaProdutor { get; set; } = "N";
        public virtual DateTime? DataPagamentoFolhaProdutor { get; set; }
        public virtual string IgnorarSugestaoInss { get; set; } = "N";
        public virtual string IdentificaTomadorNfse { get; set; } = "S";
        public virtual int? NotaFiscalTransferencia { get; set; }
        public virtual string ObservacaoComplementar { get; set; }
        public virtual decimal? ValorTotalServicos { get; set; }
        public virtual decimal? QuantidadeItens { get; set; }
        public virtual decimal? ValorFrete { get; set; }
        public virtual decimal? ValorSeguro { get; set; }
        public virtual decimal? ValorDespesasAcessorias { get; set; }
        public virtual decimal? ValorAbatimento { get; set; }
        public virtual decimal? ValorDiferencaIcmsFrete { get; set; }
        public virtual decimal? ValorDiferencaIcms { get; set; }
        public virtual decimal? ValorDespesasForaNota { get; set; }
        public virtual decimal? ValorIcmsDesonerado { get; set; }
        public virtual decimal? PercentualDesconto { get; set; }
        public virtual decimal? ValorDesconto { get; set; }
        public virtual decimal? ValorTotalNota { get; set; }
        public virtual decimal? ValorTotalProdutos { get; set; }
        public virtual DateTime? DataHoraSaidaEntrada { get; set; }
        public virtual decimal? ValorFreteEmitente { get; set; }
        public virtual string TipoPagamentoFrete { get; set; } = "9";
        public virtual string VeiculoPlaca { get; set; }
        public virtual string VeiculoUf { get; set; }
        public virtual string VeiculoCodigoAntt { get; set; }
        public virtual int? Transportador { get; set; }
        public virtual int? VolumeQuantidade { get; set; }
        public virtual string VolumeEspecie { get; set; }
        public virtual string VolumeMarca { get; set; }
        public virtual string VolumeNumeracao { get; set; }
        public virtual decimal? VolumePesoBruto { get; set; }
        public virtual decimal? VolumePesoLiquido { get; set; }
        public virtual int? AfterSaved { get; set; } = 0;
        public virtual string InformacoesNotasReferenciadas { get; set; }
        public virtual int? NotaFiscalOrigem { get; set; }
        public virtual string NfeChave { get; set; }
        public virtual int? ContaReceber { get; set; }
        public virtual string PisCofinsSocio { get; set; } = "N";
        public virtual string PisCofinsPessoaTipo { get; set; } = "F";
        public virtual int? PisCofinsRegimeTributacao { get; set; }
        public virtual int? PisCofinsRamoAtividadeSped { get; set; }
        public virtual int? ClienteCooperadoCota { get; set; }
        public virtual string XmlFatura { get; set; }
        public virtual string Concluido { get; set; } = "N";
        public virtual int? UsuarioConclusao { get; set; }
        public virtual DateTime? DataHoraConclusao { get; set; }
        public virtual int? DocumentoFiscalEletronico { get; set; }
        public virtual int? NotaFiscalSequencia { get; set; }
        public virtual int? Distribuidor { get; set; }
        public virtual int? Carreteiro { get; set; }
        public virtual int? ClienteLinha { get; set; }
        public virtual DateTime? DataVenda { get; set; }
        public virtual string VendaComComissao { get; set; } = "N";
        public virtual string EntraCalculoCusto { get; set; } = "N";
        public virtual string PossuiNotaFiscalDeFrete { get; set; } = "N";
        public virtual decimal? ValorFreteForaNota { get; set; }
        public virtual int? Cidade { get; set; }
        public virtual int? Estado { get; set; }
        public virtual string Logradouro { get; set; }
        public virtual string Bairro { get; set; }
        public virtual string Complemento { get; set; }
        public virtual string EnderecoNumero { get; set; }
        public virtual string CuponsReferenciados { get; set; }
        public virtual int? CidadeClienteEfetivo { get; set; }
        public virtual int? EstadoClienteEfetivo { get; set; }
        public virtual string LogradouroClienteEfetivo { get; set; }
        public virtual string BairroClienteEfetivo { get; set; }
        public virtual string ComplementoClienteEfetivo { get; set; }
        public virtual string EnderecoNumeroClienteEfe { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomeFantasia { get; set; }
        public virtual string NomeClienteEfetivo { get; set; }
        public virtual string NomeFantasiaClienteEfetivo { get; set; }
        public virtual string Cep { get; set; }
        public virtual string CepClienteEfetivo { get; set; }
        public virtual decimal? ValorConhecimentoFreteCusto { get; set; }
        public virtual decimal? ValorConhecimentoFreteSemIcms { get; set; }
        public virtual int? NotaFiscalCustoFrete { get; set; }
        public virtual int? NaturezaOpeSugTrib { get; set; }
        public virtual string Cancelada { get; set; } = "N";
        public virtual int? UsuarioCancelamento { get; set; }
        public virtual DateTime? DataHoraCancelamento { get; set; }
        public virtual int? NotaFiscalCupom { get; set; }
        public virtual string NotaFiscalOrigemLancamento { get; set; }
        public virtual string ImportacaoXml { get; set; } = "N";
        public virtual string ConcluirOrdemComprasVinc { get; set; } = "S";
        public virtual string MesAno { get; set; }
        public virtual string InformacaoComplementarImp { get; set; }
        public virtual string ConsumidorFinal { get; set; } = "N";
        public virtual string FinalidadeVenda { get; set; } = "C";
        public virtual string NumeroDare { get; set; }
        public virtual DateTime? DataPagamentoDare { get; set; }
        public virtual Int64? ConsumidorCpf { get; set; }
        public virtual string ObservacoesDiversas { get; set; }
        public virtual decimal? PrazoMedio { get; set; }
        public virtual string NotaFiscalOriginalAvulsa { get; set; } = "N";
        public virtual string ChaveNotaFiscalAvulsa { get; set; }
        public virtual Int64? NumeroNotaFiscalAvulsa { get; set; }
        public virtual int? SerieNotaFiscalAvulsa { get; set; }
        public virtual DateTime? DataEmissaoNotaFiscalAvulsa { get; set; }
        public virtual decimal? PercentualIcmsEstadoOrigem { get; set; }
        public virtual decimal? PercentualIcmsEstadoDestino { get; set; }
        public virtual decimal? ValorIcmsEstadoOrigem { get; set; }
        public virtual decimal? ValorIcmsEstadoDestino { get; set; }
        public virtual decimal? ValorIcmsFundoCombatePobreza { get; set; }
        public virtual string ClienteContribuinteIcms { get; set; } = "N";
        public virtual string BloqueadoFaturamentoRemessa { get; set; } = "N";
        public virtual int? TptAutorizacaoFrete { get; set; }
        public virtual string CfopSituacaoTributariaNotAvu { get; set; }
        public virtual string Xml { get; set; }
        public virtual int? FilialRetirada { get; set; }
        public virtual decimal? ValorSubsidioFrete { get; set; }
        public virtual decimal? ValorFreteRemessa { get; set; }
        public virtual string ChaveNfeVinculadaCte { get; set; }
        public virtual string CredenciamentoTransportador { get; set; }
        public virtual decimal? BaseCalculoFrete { get; set; }
        public virtual decimal? AliquotaFrete { get; set; }
        public virtual decimal? ValorImpostoFrete { get; set; }
        public virtual string IntegracaoId { get; set; }
        public virtual decimal? ValorIcmsAntecipado { get; set; }
        public virtual string FilialCredenciadaFomPro { get; set; } = "N";
        public virtual string AproveitaCreditoIcmsSimNac { get; set; } = "S";
        public virtual string NumeroPedidoVenda { get; set; }
        public virtual string ConsiderarDiferencialAliNoCus { get; set; } = "S";
        public virtual string XmlFaturaClob { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual DateTime? DataMovimentacaoCompleta { get; set; }
        public virtual string NumeroProcessoJudicial { get; set; }
        public virtual string LiminarContraRetencaoInss { get; set; }
        public virtual string LiminarContraRetencaoSenar { get; set; }
        public virtual string LiminarContraRetencaoRat { get; set; }
        public virtual string CodigoDoIndicativoDaSuspensao { get; set; }
        public virtual string ParticipanteOuSegEspEntPaa { get; set; }
        public virtual decimal? ValorBaseCalculoIcmsFcp { get; set; }
        public virtual string ServicoPrestadoMedCesMaoObr { get; set; }
        public virtual string ServicoConstrucaoCivil { get; set; }
        public virtual string TipoEmpreitada { get; set; }
        public virtual string CnoObra { get; set; }
        public virtual int? TipoFrete { get; set; }
        public virtual string SmsGerado { get; set; }
        public virtual decimal? VlrInssCalculadoAliFaz { get; set; }
        public virtual decimal? AliquotaInssFazenda { get; set; }
        public virtual decimal? VlrGilRatCalculadoAliFaz { get; set; }
        public virtual decimal? AliquotaGilRatFazenda { get; set; }
        public virtual decimal? VlrSenarCalculadoAliFaz { get; set; }
        public virtual decimal? AliquotaSenarFazenda { get; set; }
        public virtual int? ContaPagarNotaFiscalLibCan { get; set; }
        public virtual int? Avalista { get; set; }
        public virtual string BloqueadoFaturamentoDevolucao { get; set; }
        public virtual string AproveitaQualquerCreditoIcms { get; set; }
        public virtual int? EquipamentoSat { get; set; }
        public virtual int? PlanoVenda { get; set; }
        public virtual decimal? BaseCalculoSimplesNacional { get; set; }
        public virtual decimal? ValorIcmsSimplesNacional { get; set; }
        public virtual string MotivoDesoneracaoIcms { get; set; }
        public virtual string ClienteRetemInss { get; set; }
        public virtual string CalculaInss { get; set; }
        public virtual string ClienteRetemSenar { get; set; }
        public virtual string CalculaSenar { get; set; }
        public virtual string RetemSenar { get; set; }
        public virtual Int64? NumeroRps { get; set; }
        public virtual int? AlteradorValorPis { get; set; }
        public virtual int? AlteradorValorCofins { get; set; }
        public virtual int? AlteradorValorContribuicao { get; set; }
        public virtual int? AlteradorValorIss { get; set; }
        public virtual int? AlteradorValorIrrf { get; set; }
        public virtual int? AlteradorValorInss { get; set; }
        public virtual int? AlteradorValorSenar { get; set; }
        public virtual decimal? ValorRetencaoPis { get; set; }
        public virtual decimal? ValorRetencaoCofins { get; set; }
        public virtual decimal? ValorRetencaoConSoc { get; set; }
        public virtual decimal? ValorRetencaoIr { get; set; }
        public virtual decimal? ValorRetencaoInss { get; set; }
        public virtual decimal? ValorRetencaoIss { get; set; }
        public virtual decimal? ValorRetencaoSenar { get; set; }

    }
}
