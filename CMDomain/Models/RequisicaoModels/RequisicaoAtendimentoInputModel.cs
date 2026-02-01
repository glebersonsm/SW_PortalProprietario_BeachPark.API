namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoAtendimentoInputModel : ModelRequestBase
    {
        public Int64? NumRequisicao { get; set; }
        public string? Obs { get; set; }

        public List<RequisicaoAtendimentoItemInputModel> ItensAtender { get; set; } = new List<RequisicaoAtendimentoItemInputModel>();

    }
}
