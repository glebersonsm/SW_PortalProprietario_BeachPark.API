namespace CMDomain.Models.Cotacao
{
    public class GerarOcsDoSumarioCotacaoInputModel : ModelRequestBase
    {
        public int? CodProcesso { get; set; }
        public int? IdComprador { get; set; }
        public int? IdEmpresa { get; set; }

    }
}
