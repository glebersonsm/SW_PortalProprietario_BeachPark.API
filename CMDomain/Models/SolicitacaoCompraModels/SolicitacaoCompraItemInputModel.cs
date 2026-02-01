
namespace CMDomain.Models.SolicitacaoCompraModels
{
    public class SolicitacaoCompraItemInputModel
    {
        public string? CodArtigo { get; set; }
        public string? CodMedida { get; set; }
        public int? IdContratoProd { get; set; }
        public int? IdProdVari { get; set; }
        public decimal? QtdePedida { get; set; }
        public string? ObsItemSolic { get; set; }
    }
}
