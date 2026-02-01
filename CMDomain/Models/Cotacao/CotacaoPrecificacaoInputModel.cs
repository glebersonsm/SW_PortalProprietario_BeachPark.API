namespace CMDomain.Models.Cotacao
{
    public class CotacaoPrecificacaoInputModel : ModelRequestBase
    {
        public string? CodProcesso { get; set; }
        public string? Proposta { get; set; }
        public string? IdFornecedor { get; set; }
        public string? IdEmpresa { get; set; }
        public string? ConsiderarListaDePrecosDoFornecedor { get; set; } = "N";
        public string? RemoverOsCustosAgregadosDaNotaNaoEnviados { get; set; } = "N";
        public List<CotacaoPrecificacaoCustoAgregInputModel> CustosAgregadosNota { get; set; } = new List<CotacaoPrecificacaoCustoAgregInputModel>();
        public List<CotacaoItemPrecificacaoInputModel> ItemsDaCotacao { get; set; } = new List<CotacaoItemPrecificacaoInputModel>();

    }
}
