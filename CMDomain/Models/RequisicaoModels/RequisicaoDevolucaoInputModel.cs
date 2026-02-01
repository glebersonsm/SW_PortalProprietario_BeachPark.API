namespace CMDomain.Models.RequisicaoModels
{
    public class RequisicaoDevolucaoInputModel : ModelRequestBase
    {
        public Int64? NumRequisicao { get; set; }
        public List<string> ItensDevolver { get; set; } = new List<string>();

    }
}
