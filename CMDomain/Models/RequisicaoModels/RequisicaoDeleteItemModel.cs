namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoDeleteItemModel : RequisicaoDeleteModel
    {
        public List<string> ItensToDelete { get; set; } = new List<string>();
    }
}
