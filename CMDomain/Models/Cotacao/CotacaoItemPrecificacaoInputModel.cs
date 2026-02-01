namespace CMDomain.Models.Cotacao
{
    public class CotacaoItemPrecificacaoInputModel : ModelRequestBase
    {
        public string? CodProcesso { get; set; }
        public string? IdProcXArt { get; set; }
        public string? Proposta { get; set; }
        public string? QuantidadeFornecida { get; set; }
        public string? Observacao { get; set; }
        public string? Contato { get; set; }
        public string? PrecoUnitario { get; set; }
        public string? RemoverOsCustosAgregadosNaoEnviados { get; set; } = "N";
        public List<CotacaoItemPrecificacaoCustoAgregInputModel> CustosAgregados { get; set; } = new List<CotacaoItemPrecificacaoCustoAgregInputModel>();
        public string? RemoverAsDatasDeEntregasNaoEnviadas { get; set; } = "N";
        public List<CotacaoItemPrecificacaoEntregasInputModel> Entregas { get; set; } = new List<CotacaoItemPrecificacaoEntregasInputModel>();
        public string? RemoverOsPrazosDePagamentosNaoEnviados { get; set; } = "N";
        public List<CotacaoItemPrecificacaoPrazoPagamentoInputModel> PrazoPagamento { get; set; } = new List<CotacaoItemPrecificacaoPrazoPagamentoInputModel>();

    }
}
