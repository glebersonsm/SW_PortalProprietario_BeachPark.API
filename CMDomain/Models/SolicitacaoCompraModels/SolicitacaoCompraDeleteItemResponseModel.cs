namespace CMDomain.Models.SolicitacaoCompraModels
{
    public class SolicitacaoCompraDeleteItemResponseModel : SolicitacaoCompraDeleteModel
    {
        public List<DeleteItemSolicitacaoCompraResult> DeleteItensResult { get; set; } = new List<DeleteItemSolicitacaoCompraResult>();
    }


    public class DeleteItemSolicitacaoCompraResult
    {
        public SolicitacaoCompraItemViewModel? Item { get; set; }
        public string? Result { get; set; }
    }
}
