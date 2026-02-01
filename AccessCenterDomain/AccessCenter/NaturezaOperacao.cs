namespace AccessCenterDomain.AccessCenter
{
    public class NaturezaOperacao : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual int? NotaFiscalSequencia { get; set; }
        public virtual string Status { get; set; }
        public virtual string LancaFinanceiro { get; set; }
        public virtual string ConsideraIcmsDesonerado { get; set; }
        public virtual string InformaNotaFiscalOrigem { get; set; }
        public virtual string LivroICMS { get; set; }
        public virtual string LivroISS { get; set; }
        public virtual string OptantePisCofins { get; set; } = "N";
        public virtual string CalculaPisCofinsAut { get; set; } = "N";
        public virtual string BloqueiaSemParametroPisCofins { get; set; } = "N";
        public virtual string Tipo { get; set; } = "E";
        public virtual string TipoNotaFiscalOrigem { get; set; }
        public virtual string ExigeCentroCusto { get; set; } = "N";
        public virtual string ExigeOrdemCompra { get; set; } = "N";
        public virtual string Venda { get; set; } = "N";
        public virtual string Bonificacao { get; set; } = "N";
        public virtual string SugerePrecoVenda { get; set; } = "N";
        public virtual string PermiteInformarImpostosMan { get; set; } = "N";
        public virtual string Requisicao { get; set; } = "N";
        public virtual string Transferencia { get; set; } = "N";
        public virtual string PermiteDevolucao { get; set; } = "N";
        public virtual string EfetuaPedido { get; set; } = "N";
        public virtual string ExigeClienteEfetivo { get; set; } = "N";
        public virtual string PermiteLancarDespesa { get; set; } = "N";
        public virtual string PermiteRecebimentoMercadoria { get; set; } = "N";
        public virtual string EntradaAtivoFixo { get; set; } = "N";
        public virtual string Devolucao { get; set; } = "N";
        public virtual int? NaturezaOperacaoEntradaTrans { get; set; }
        public virtual int? NaturezaOperacaoReferenciada { get; set; }
        public virtual string TipoContaPagarReceber { get; set; } = "P";
        public virtual string Contabilizar { get; set; } = "S";
        public virtual string ExigeDestinacaoContabil { get; set; } = "N";
        public virtual int? OperacaoFinanceira { get; set; }
        public virtual int? TipoCadastroContabil { get; set; }
        public virtual string SugereINSS { get; set; } = "N";
        public virtual string Frete { get; set; } = "N";
        public virtual string RelatorioGerencial { get; set; } = "N";
        public virtual string Compra { get; set; } = "N";
        public virtual string Retorno { get; set; } = "N";
        public virtual string NotaFiscalOrigemLancamentoUti { get; set; } = "M";
        public virtual string ControlaLote { get; set; } = "N";
        public virtual string PermiteFiliaisDiferentes { get; set; } = "N";
        public virtual string PermiteFiliaisIguais { get; set; } = "N";
        public virtual string BaseSugestaoTributacao { get; set; } = "V";
        public virtual string PermitirSomenteUmTipoProduto { get; set; } = "N";
        public virtual string PermitirCriarAutorizacao { get; set; } = "N";
        public virtual int? IndiceFinanceiro { get; set; } = 1;
        public virtual string CompraEnergiaEletricaConLiv { get; set; } = "N";
        public virtual string PermitirInformarFrete { get; set; } = "N";
        public virtual string PermitirInformarValorFrete { get; set; } = "N";
        public virtual string ControlaPeca { get; set; } = "N";
        public virtual string Desfazimento { get; set; } = "N";

    }
}
