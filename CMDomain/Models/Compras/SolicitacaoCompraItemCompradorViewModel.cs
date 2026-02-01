namespace CMDomain.Models.Compras
{
    public class SolicitacaoCompraItemCompradorViewModel
    {
        public int? IdItemSoli { get; set; }
        public int? Comprador { get; set; }
        public string? Urgente { get; set; }
        public string? CodArtigo { get; set; }
        public string? Descricao { get; set; }
        public string? NumSolCompra { get; set; }
        public decimal? QtdePedida { get; set; }
        public decimal? QtdePendente { get; set; }
        public DateTime? DataEmissao { get; set; }
        public string? CodGrupoProd { get; set; }
        public string? Unidade { get; set; }
        public DateTime? Necessidade { get; set; }
        public string? NomeEmpresa { get; set; }
        public string? Estocavel { get; set; }
        public string? CodCentroCusto { get; set; }
        public string? NomeCentroCusto { get; set; }
        public string? CodCentroRespon { get; set; }
        public string? NomeCentRespon { get; set; }
        public int? UnidNegoc { get; set; }
        public string? NomeUnidNegoc { get; set; }
        public int? IdProcXArt { get; set; }
        public int? CodProcesso { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public string? NomeAlmoxarifado { get; set; }
    }
}
