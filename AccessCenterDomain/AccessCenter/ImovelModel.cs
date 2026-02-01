namespace AccessCenterDomain.AccessCenter
{
    public class ImovelModel : EntityBaseEsol
    {
        public string? Numero { get; set; }
        public int? TipoImovel { get; set; }
        public int? ImovelAndar { get; set; }
        public int? ImovelBloco { get; set; }
        public string? CodigoBloco { get; set; }
        public string? NomeBloco { get; set; }
        public string? CodigoTipoImovel { get; set; }
        public string? NomeTipoImovel { get; set; }
        public int? ImovelVista { get; set; }
        public int? ImovelLado { get; set; }
        public int? GrupoCota { get; set; }
        public int? CategoriaCota { get; set; }
        public string? LiberadoVenda { get; set; } = "N";
        public int? Empreendimento { get; set; }
        public decimal? FracaoIdeal { get; set; }
        public decimal? FracaoIdealM2 { get; set; }
        public decimal? AreaPrivativa { get; set; }
        public decimal? AreaComum { get; set; }
        public decimal? AreaTotal { get; set; }
        public int? Capacidade { get; set; }
        public int? QuantidadeQuartos { get; set; }
        public decimal? FracaoIdealPool { get; set; }
        public int? QuantidadeBanheiros { get; set; }
        public int? QuantidadeCamas { get; set; }
        public string? PossuiVaranda { get; set; } = "N";
        public string? PossuiBanheira { get; set; } = "N";
        public string? Pne { get; set; } = "N";
        public string? FormatoDataEntrega { get; set; } = "D";
        public int? QuantidadeMesesEntregaProduto { get; set; }
        public DateTime? DataEntregaProduto { get; set; }
        public string? ApropriarReceitaMensal { get; set; } = "N";
        public DateTime? DataFimApropriacao { get; set; }
        public string? FormatoDataApropriacao { get; set; }
        public int? QuantidadeMesesFimApropriacao { get; set; }

    }
}
