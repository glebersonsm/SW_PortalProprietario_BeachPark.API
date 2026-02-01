namespace CMDomain.Models.Compras
{
    public class ItemOcAlteracaoInputModel : ModelRequestBase
    {
        public int? IdItemOc { get; set; }
        public decimal? QuantidadePedida { get; set; }
        public decimal? ValorUnitario { get; set; }

    }
}
