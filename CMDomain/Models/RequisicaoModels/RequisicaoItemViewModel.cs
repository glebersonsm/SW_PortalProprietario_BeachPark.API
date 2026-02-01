


using CMDomain.Entities;

namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoItemViewModel
    {
        public Int64? NumRequisicao { get; set; }
        public string? CodArtigo { get; set; }
        public string? CodMedida { get; set; }
        public decimal? ValorUn { get; set; }
        public decimal? QtdePedida { get; set; }
        public decimal? QtdePendente { get; set; }
        public string? FlgSci { get; set; }
        public string? Obs { get; set; }
        public decimal? QtdePendVenda { get; set; }
        public DateTime? DtCancelamento { get; set; }
        public decimal? QtdeCancelada { get; set; }
        public decimal? QtdeAprovadaRad { get; set; }
        public DateTime? DataNecessidade { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        public static explicit operator RequisicaoItemViewModel(ItemPedi model)
        {
            return new RequisicaoItemViewModel
            {
                NumRequisicao = model.NumRequisicao,
                CodArtigo = model.CodArtigo,
                CodMedida = model.CodMedida,
                ValorUn = model.ValorUn,
                QtdePedida = model.QtdePedida,
                QtdePendente = model.QtdePendente,
                FlgSci = model.FlgSci,
                Obs = model.Obs,
                QtdePendVenda = model.QtdePendVenda,
                DtCancelamento = model.DtCancelamento,
                QtdeCancelada = model.QtdeCancelada,
                QtdeAprovadaRad = model.QtdeAprovadaRad,
                DataNecessidade = model.DataNecessidade,
                TrgDtInclusao = model.TrgDtInclusao,
                TrgUserInclusao = model.TrgUserInclusao
            };
        }
    }
}
