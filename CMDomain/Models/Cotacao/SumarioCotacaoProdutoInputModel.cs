namespace CMDomain.Models.Cotacao
{
    public class SumarioCotacaoProdutoInputModel
    {
        public int? CodProcesso { get; set; }
        public int? IdProcXArt { get; set; }
        public SumarioCotacaoItemInputModel? CotacaoGanhadora { get; set; }

    }
}
