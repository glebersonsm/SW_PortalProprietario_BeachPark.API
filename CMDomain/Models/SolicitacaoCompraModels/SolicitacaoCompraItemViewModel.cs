


using CMDomain.Entities;

namespace CMDomain.Models.SolicitacaoCompraModels
{
    public class SolicitacaoCompraItemViewModel
    {
        public int? NumSolCompra { get; set; }
        public int? IdItemSoli { get; set; }
        public string? CodArtigo { get; set; }
        public string? CodMedida { get; set; }
        public int? CodProcesso { get; set; }
        public int? IdContratoProd { get; set; }
        public decimal? QtdePedida { get; set; }
        public decimal? SaldoAComprar { get; set; }
        public decimal? QtdePendente { get; set; }
        public int? IdComprador { get; set; }
        public string? ObsItemSolic { get; set; }
        public string? StatusItem { get; set; }
        public DateTime? DataCancel { get; set; }
        public int? IdUsuarioCancel { get; set; }
        public int? IdProcXArt { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        public static explicit operator SolicitacaoCompraItemViewModel(ItemSoli model)
        {
            return new SolicitacaoCompraItemViewModel
            {
                NumSolCompra = model.NumSolCompra,
                IdItemSoli = model.IdItemSoli,
                CodArtigo = model.CodArtigo,
                CodMedida = model.CodMedida,
                CodProcesso = model.CodProcesso,
                IdContratoProd = model.IdContratoProd,
                QtdePedida = model.QtdePedida,
                SaldoAComprar = model.SaldoAComprar,
                QtdePendente = model.QtdePendente,
                IdComprador = model.IdComprador,
                ObsItemSolic = model.ObsItemSolic,
                StatusItem = model.StatusItem,
                DataCancel = model.DataCancel,
                IdUsuarioCancel = model.IdUsuarioCancel,
                IdProcXArt = model.IdProcXArt,
                TrgDtInclusao = model.TrgDtInclusao,
                TrgUserInclusao = model.TrgUserInclusao
            };


        }
    }
}
