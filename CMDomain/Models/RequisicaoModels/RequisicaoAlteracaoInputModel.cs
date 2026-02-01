namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoAlteracaoInputModel : ModelRequestBase
    {
        public Int64? NumRequisicao { get; set; }
        public DateTime? DataNecessidade { get; set; } = DateTime.Now.AddDays(1).Date;
        public string? CustoTransf { get; set; } = "T";
        public string? Obs { get; set; }

        public List<RequisicaoItemInputModel> Itens { get; set; } = new List<RequisicaoItemInputModel>();

    }
}
