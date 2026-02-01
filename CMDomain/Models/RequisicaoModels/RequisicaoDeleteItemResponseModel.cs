namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoDeleteItemResponseModel : RequisicaoDeleteModel
    {
        public List<DeleteItemRequisicaoResult> DeleteItensResult { get; set; } = new List<DeleteItemRequisicaoResult>();
    }


    public class DeleteItemRequisicaoResult
    {
        public RequisicaoItemViewModel? Item { get; set; }
        public string? Result { get; set; }
    }
}
