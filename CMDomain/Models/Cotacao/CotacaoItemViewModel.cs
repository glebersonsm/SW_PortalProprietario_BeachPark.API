

using CMDomain.Entities;

namespace CMDomain.Models.Cotacao
{
    public class CotacaoItemViewModel
    {
        public int? CodProcesso { get; set; }
        public int? IdProcXArt { get; set; }
        public decimal? QtdePedida { get; set; }
        public string? CodMedida { get; set; }
        public string? CodigoGrupoProduto { get; set; }
        public string? NomeGrupoProduto { get; set; }
        public string? CodArtigo { get; set; }
        public string? NomeProduto { get; set; }
        public string? Observacao { get; set; }
        public int? CodAlmoxarifado { get; set; }
        public int? NumSolCompra { get; set; }
        public DateTime? DataNecessidade { get; set; }
        public int? IdItemSoli { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }
        public List<CotacoesItemViewModel> Cotacoes { get; set; } = new List<CotacoesItemViewModel>();

        public static explicit operator CotacaoItemViewModel(ProcXArt model)
        {
            return new CotacaoItemViewModel
            {
                CodProcesso = model.CodProcesso,
                IdProcXArt = model.IdProcXArt,
                QtdePedida = model.QtdePedida,
                CodMedida = model.CodMedida,
                CodArtigo = model.CodArtigo,
                Observacao = !string.IsNullOrEmpty(model.Justificativa) && model.Justificativa.Split("\n").Count() > 0 ? model.Justificativa.Split("\n").Last().Replace("\n", "").Replace("\r", "") : model.Justificativa,
                DataNecessidade = model.DataNecessidade,
                TrgUserInclusao = model.TrgUserInclusao,
                TrgDtInclusao = model.TrgDtInclusao
            };
        }
    }
}
