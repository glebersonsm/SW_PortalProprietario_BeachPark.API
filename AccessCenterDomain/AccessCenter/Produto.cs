namespace AccessCenterDomain.AccessCenter
{
    public class Produto : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string DadosComplementares { get; set; }
        public virtual GrupoProduto? GrupoProduto { get; set; }
        public virtual int? GrupoTributacao { get; set; }
        public virtual TipoProduto? TipoProduto { get; set; }
        public virtual string PermitirEstoquePendente { get; set; } = "N";
        public virtual string Status { get; set; } = "A";
        public virtual string FichaTecnica { get; set; } = "N";
        public virtual string ProdutoVenda { get; set; } = "N";
        public virtual int? GrupoProdutoPdv { get; set; } = 1;
        public virtual Ncm? Ncm { get; set; }
        public virtual string AtivoFixo { get; set; } = "N";
        public virtual int? AtivoFixoTipo { get; set; }
        public virtual string Vacina { get; set; } = "N";
        public virtual string VacinaAgrodefesa { get; set; } = "N";
        public virtual int? TipoItemSped { get; set; }
        public virtual int? GeneroItemSped { get; set; }
        public virtual string ProibidoVendaMenorIdade { get; set; } = "N";
        public virtual string CombustivelLubrificante { get; set; } = "N";
        public virtual decimal? AliquotaIcms { get; set; }
        public virtual decimal? PercentualDesconto { get; set; }
        public virtual decimal? PercentualDespesaOperacional { get; set; }
        public virtual decimal? FatorCalculo { get; set; }
        public virtual decimal? Peso { get; set; }
        public virtual decimal? VariacaoParaMais { get; set; }
        public virtual decimal? VariacaoParaMenos { get; set; }
        public virtual ProdutoUnidadeMedida? UnidadeMedidaVenda { get; set; }
        public virtual string PermiteReterInss { get; set; } = "N";
        public virtual string TipoComposicao { get; set; } = "S";
        public virtual ProdutoUnidadeMedida? UnidadeMedidaBase { get; set; }
        public virtual ProdutoUnidadeMedida? ProdutoUnidadeMedidaRelatorio { get; set; }
        public virtual string PermiteSuspensaoPisCofins { get; set; } = "N";
        public virtual ProdutoUnidadeMedida? UnidadeMedidaEntradaNota { get; set; }
        public virtual decimal? QuantidadeMaximaVenda { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Energia { get; set; } = "N";
        public virtual int? NaturezaReceitaPis { get; set; }
        public virtual int? NaturezaReceitaCofins { get; set; }
        public virtual int? ClassificacaoServicoPrestado { get; set; }
        public virtual string DescricaoProdutoANP { get; set; }
        public virtual string RepasseInterestadual { get; set; } = "N";
        public virtual string PermiteDesconto { get; set; } = "N";
        public virtual int? DestinoContabil { get; set; }
        public virtual int? CategoriaProduto { get; set; } = 1;
        public virtual string ControlaEstoque { get; set; } = "N";
        public virtual string BloqueadoProcessoCompras { get; set; } = "N";
        public virtual string ConsideraCustoCargaDescarga { get; set; } = "N";
        public virtual int? CEST { get; set; }
        public virtual string DesmembraComposicaoPortal { get; set; } = "N";
        public virtual string BaixaIndividualProdutoCom { get; set; } = "N";
        public virtual int? TempoPreparo { get; set; } = 0;

    }
}
