namespace CMDomain.Models.SolicitacaoCompraModels
{
    public class SolicitacaoCompraDeleteItemModel : SolicitacaoCompraDeleteModel
    {
        public List<int> ItensToDelete { get; set; } = new List<int>();
    }
}
