namespace AccessCenterDomain.AccessCenter
{
    public class Imovel : EntityBase
    {
        public virtual string Numero { get; set; }
        public virtual int? TipoImovel { get; set; }
        public virtual int? ImovelAndar { get; set; }
        public virtual int? ImovelBloco { get; set; }
        public virtual int? ImovelVista { get; set; }
        public virtual int? ImovelLado { get; set; }
        public virtual int? GrupoCota { get; set; }
        public virtual int? CategoriaCota { get; set; }
        public virtual string LiberadoVenda { get; set; } = "N";
        public virtual int? Empreendimento { get; set; }
        public virtual decimal? FracaoIdeal { get; set; }
        public virtual decimal? FracaoIdealM2 { get; set; }
        public virtual decimal? AreaPrivativa { get; set; }
        public virtual decimal? AreaComum { get; set; }
        public virtual decimal? AreaTotal { get; set; }
        public virtual int? Capacidade { get; set; }
        public virtual int? QuantidadeQuartos { get; set; }
        public virtual decimal? FracaoIdealPool { get; set; }
        public virtual int? QuantidadeBanheiros { get; set; }
        public virtual int? QuantidadeCamas { get; set; }
        public virtual string PossuiVaranda { get; set; } = "N";
        public virtual string PossuiBanheira { get; set; } = "N";
        public virtual string Pne { get; set; } = "N";
        public virtual string FormatoDataEntrega { get; set; } = "D";
        public virtual int? QuantidadeMesesEntregaProduto { get; set; }
        public virtual DateTime? DataEntregaProduto { get; set; }
        public virtual string ApropriarReceitaMensal { get; set; } = "N";
        public virtual DateTime? DataFimApropriacao { get; set; }
        public virtual string FormatoDataApropriacao { get; set; }
        public virtual int? QuantidadeMesesFimApropriacao { get; set; }

    }
}
