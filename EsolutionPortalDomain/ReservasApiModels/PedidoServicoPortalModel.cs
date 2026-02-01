
namespace EsolutionPortalDomain.ReservasApiModels
{
    public class PedidoServicoPortalModel
    {
        public int? Id { get; set; }
        public int? Pedido { get; set; }
        public decimal? Valor { get; set; }
        public decimal? Desconto { get; set; }
        public decimal? Acrescimo { get; set; }
        public decimal ValorLiquido => Valor.GetValueOrDefault(0.00m) - Desconto.GetValueOrDefault(0.00m) + Acrescimo.GetValueOrDefault(0.00m);
    }
}
