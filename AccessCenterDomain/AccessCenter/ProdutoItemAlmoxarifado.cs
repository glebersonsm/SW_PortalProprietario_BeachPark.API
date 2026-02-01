namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoItemAlmoxarifado : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual ProdutoItem? ProdutoItem { get; set; }
        public virtual Almoxarifado? Almoxarifado { get; set; }
        public virtual decimal? QuantidadeMinima { get; set; }
        public virtual decimal? QuantidadeMaxima { get; set; }
        public virtual int? DiasReposicao { get; set; }
        public virtual string? Status { get; set; } = "A";
        public virtual string? AjustePrecoVenda { get; set; } = "S";
        public virtual decimal? PrecoVenda { get; set; }
        public virtual decimal? PrecoCusto { get; set; }
        public virtual decimal? PercentAcrescPrecoVenda { get; set; }
        public virtual string? ControlaPeca { get; set; } = "N";
        public virtual string? ControlaLote { get; set; } = "N";
        public virtual string? PermiteMaisDeUmLotePendente { get; set; } = "N";
        public virtual string? PermiteEstoquePendente { get; set; } = "N";
        public virtual string? VerificaVariacaoCusto { get; set; } = "N";
        public virtual decimal? PercentualMaximoVarCusParCim { get; set; }
        public virtual decimal? PercentualMaximoVarCusParBai { get; set; }
        public virtual string? PermitirInformarProdutoEstMin { get; set; } = "S";
        public virtual string? ControlaEstoque { get; set; } = "S";

    }
}
