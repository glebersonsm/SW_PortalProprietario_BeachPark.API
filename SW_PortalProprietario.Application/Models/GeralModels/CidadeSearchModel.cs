namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class CidadeSearchModel
    {
        public int? Id { get; set; }
        public string? CodigoIbge { get; set; }
        public string? Nome { get; set; }
        public string? Search { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }

    }
}
