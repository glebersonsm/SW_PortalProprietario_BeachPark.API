namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalProItemAlm : EntityBase
    {
        public virtual int? NotaFiscal { get; set; }
        public virtual int? ProdutoItemAlmoxarifado { get; set; }
        public virtual int? ProdutoUnidadeMedida { get; set; }
        public virtual int? CFOP { get; set; }
        public virtual int? SituacaoTributaria { get; set; }
        public virtual int? TributacaoSitTri { get; set; }
        public virtual int? CentroCusto { get; set; }
        public virtual int? DestinoContabil { get; set; }
        public virtual int? AtividadeProjeto { get; set; }
        public virtual int? ProdutoItemLote { get; set; }
        public virtual int? Sequencia { get; set; }
        public virtual string HistoricoOrdemCompra { get; set; }
        public virtual int? ParametroPisCofins { get; set; }
        public virtual string Fomentar { get; set; } = "N";
        public virtual int? FilialDestino { get; set; }
        public virtual int? TributacaoSitTriBasSug { get; set; }
        public virtual string Observacao { get; set; }
        public virtual string ObservacaoSugestaoTributacao { get; set; }
        public virtual decimal? Quantidade { get; set; }
        public virtual decimal? QuantidadePeca { get; set; }
        public virtual decimal? ValorTotalProduto { get; set; }
        public virtual decimal? ValorTotalCusto { get; set; }
        public virtual decimal? ValorFrete { get; set; }
        public virtual decimal? ValorSeguro { get; set; }
        public virtual decimal? ValorDespesasAcessorias { get; set; }
        public virtual decimal? ValorDespesasAcessoriasOrig { get; set; }
        public virtual decimal? ValorAbatimento { get; set; }
        public virtual decimal? ValorDesconto { get; set; }
        public virtual decimal? ValorDespesasForaNota { get; set; }
        public virtual decimal? ValorDiferencaICMS { get; set; }
        public virtual decimal? ValorDiferencaICMSFrete { get; set; }
        public virtual decimal? ValorBaseCalculoICMS { get; set; }
        public virtual decimal? AliquotaICMS { get; set; }
        public virtual decimal? ValorICMS { get; set; }
        public virtual decimal? ValorOutrosICMS { get; set; }
        public virtual decimal? ValorIsentoICMS { get; set; }
        public virtual decimal? ValorArredondamentoICMS { get; set; }
        public virtual decimal? ValorICMSDesonerado { get; set; } = 0.00m;
        public virtual string MotivoDesoneracaoICMS { get; set; }
        public virtual decimal? ValorBaseCalculoICMSOrigina { get; set; }
        public virtual decimal? AliquotaICMSOriginal { get; set; }
        public virtual decimal? ValorICMSOriginal { get; set; }
        public virtual string CfopOriginal { get; set; }
        public virtual int? SituacaoTributariaOriginal { get; set; }
        public virtual string LancaFinanceiro { get; set; } = "S";
        public virtual string SugeriuTributacao { get; set; } = "N";
        public virtual string SugerirAutomaticamente { get; set; } = "N";
        public virtual decimal? ValorICMSSubstituicao { get; set; }
        public virtual decimal? ValorIsentoIPI { get; set; }
        public virtual decimal? ValorOutrosIPI { get; set; }
        public virtual decimal? ValorBaseCalculoISS { get; set; }
        public virtual decimal? AliquotaISS { get; set; }
        public virtual decimal? ValorISS { get; set; }
        public virtual decimal? ValorOutrosISS { get; set; }
        public virtual decimal? ValorIsentoISS { get; set; }
        public virtual decimal? ValorINSS { get; set; }
        public virtual decimal? ValorSenar { get; set; }
        public virtual decimal? ValorINSSTomador { get; set; }
        public virtual decimal? ValorIRRF { get; set; }
        public virtual decimal? ValorPIS { get; set; }
        public virtual decimal? ValorCOFINS { get; set; }
        public virtual decimal? ValorContribuicaoSocial { get; set; }
        public virtual decimal? ValorDare { get; set; }
        public virtual decimal? ReducaoBaseCalculoICMS { get; set; }
        public virtual decimal? PerBaseCalculoIcms { get; set; }
        public virtual decimal? AliquotaBase { get; set; }
        public virtual decimal? PrecoVendaMargem { get; set; }
        public virtual decimal? PrecoVendaPIS { get; set; }
        public virtual decimal? PrecoVendaCOFINS { get; set; }
        public virtual decimal? PrecoVendaDespesa { get; set; }
        public virtual decimal? PrecoVendaDesconto { get; set; }
        public virtual decimal? PrecoVendaFator { get; set; }
        public virtual decimal? PrecoVendaAliquotaICMS { get; set; }
        public virtual decimal? PrecoVendaBaseCalculo { get; set; }
        public virtual decimal? PrecoVenda { get; set; }
        public virtual decimal? ValorConhecimentoFreteCusto { get; set; }
        public virtual decimal? ValorConhecimentoFreteSemICMS { get; set; }
        public virtual DateTime? DataHoraImportacaoAtivoFixo { get; set; }
        public virtual decimal? CustoMedioTipoEstoquePrincipal { get; set; }
        public virtual int? GeneroItemSpedPISCOFINS { get; set; }
        public virtual int? SituacaoTributariaPIS { get; set; }
        public virtual int? SituacaoTributariaCOFINS { get; set; }
        public virtual string SuspensaoPISCOFINS { get; set; } = "N";
        public virtual string NumeroSeqNFOrigem { get; set; }
        public virtual int? notafisproitealmorigem { get; set; }
        public virtual int? notafisproitealmini { get; set; }
        public virtual int? NotaFiscalInicial { get; set; }
        public virtual decimal? SubTotal { get; set; }
    }
}
