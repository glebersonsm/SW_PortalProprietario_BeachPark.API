namespace CMDomain.Models.Cotacao
{
    public class SumarioCotacaoInputModel : ModelRequestBase
    {
        public int? CodProcesso { get; set; }
        public int? IdComprador { get; set; }
        public int? IdEmpresa { get; set; }
        public List<SumarioCotacaoProdutoInputModel> CotacoesGanhadoras { get; set; } = new List<SumarioCotacaoProdutoInputModel>();

    }
}
