namespace CMDomain.Models.Cotacao
{
    public class ConclusaoProcessoComprasResult
    {
        public int? CodProcesso { get; set; }
        public int? IdComprador { get; set; }
        public List<ConclusaoProcessoComprasItemResult> OcsGeradas { get; set; } = new List<ConclusaoProcessoComprasItemResult>();

    }
}
