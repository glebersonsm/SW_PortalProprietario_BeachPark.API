namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoInputModel : ModelRequestBase
    {
        public Int64? NumRequisicao { get; set; }
        public int? UnidNegoc { get; set; } = -1;
        public string? CustoTransf { get; set; } = "C";
        public string? CodCentroCusto { get; set; }
        public DateTime? DataEmissao { get; set; } = DateTime.Now.Date;
        public int? CodAlmoxaOrigem { get; set; }
        public int? CodAlmoxaDestino { get; set; }
        public DateTime? DataNecessidade { get; set; } = DateTime.Now.AddDays(1).Date;
        public bool? RealizarAtendimentoAutomatico { get; set; } = false;
        public string? Obs { get; set; }
        public List<RequisicaoItemInputModel> Itens { get; set; } = new List<RequisicaoItemInputModel>();

    }
}
