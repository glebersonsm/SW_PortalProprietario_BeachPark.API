

using CMDomain.Entities;

namespace CMDomain.Models.Cotacao
{
    public class CotacaoViewModel
    {
        public int? CodProcesso { get; set; }
        public string? Status { get; set; }
        public int? IdComprador { get; set; }
        public string? Comprador { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        public List<CotacaoItemViewModel> Items { get; set; } = new List<CotacaoItemViewModel>();

        public static explicit operator CotacaoViewModel(Processo model)
        {
            return new CotacaoViewModel
            {
                CodProcesso = model.CodProcesso,
                Status = model.Status,
                IdComprador = model.IdComprador,
                TrgDtInclusao = model.TrgDtInclusao,
                TrgUserInclusao = model.TrgUserInclusao
            };
        }
    }
}
