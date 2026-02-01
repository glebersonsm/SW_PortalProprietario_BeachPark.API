namespace CMDomain.Models.Compras
{
    public class OrdemCompraItemViewModel
    {
        public int? IdItemOc { get; set; }
        public int? NumOc { get; set; }
        public string? CodProduto { get; set; }
        public decimal? QuantidadePedida { get; set; }
        public decimal? QuantidadeRecebida { get; set; }
        public decimal? ValorUnitario { get; set; }
        public string? CodMedida { get; set; }
        public string? NomeProduto { get; set; }
        public string? Status { get; set; }
        public decimal? ValorTotalRecebido => Math.Round(QuantidadeRecebida.GetValueOrDefault() * ValorUnitario.GetValueOrDefault(), 2);
    }
}
