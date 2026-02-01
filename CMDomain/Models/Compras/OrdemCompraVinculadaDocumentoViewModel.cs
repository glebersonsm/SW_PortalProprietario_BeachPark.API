namespace CMDomain.Models.Compras
{
    public class OrdemCompraVinculadaDocumentoViewModel
    {
        public int? IdDocumento { get; set; }
        public int? NumOc { get; set; }
        public string? CodProduto { get; set; }
        public decimal? Quantidade { get; set; }
        public decimal? ValorUnitario { get; set; }
        public string? CodMedida { get; set; }
        public string? NomeProduto { get; set; }
        public string? TipoOc { get; set; } = "Normal";
        public decimal? ValorTotalItem => Math.Round(Quantidade.GetValueOrDefault() * ValorUnitario.GetValueOrDefault(), 2);
    }
}
