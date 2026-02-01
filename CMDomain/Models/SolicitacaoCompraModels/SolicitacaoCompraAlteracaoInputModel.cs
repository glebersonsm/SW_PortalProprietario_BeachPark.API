namespace CMDomain.Models.SolicitacaoCompraModels
{
    public class SolicitacaoCompraAlteracaoInputModel : ModelRequestBase
    {
        public int? NumSolCompra { get; set; }
        public string? CustoEstoque { get; set; } = "E";
        public string? FlgUrgente { get; set; } = "N";

        public List<SolicitacaoCompraItemAlteracaoInputModel> Itens { get; set; } = new List<SolicitacaoCompraItemAlteracaoInputModel>();

    }
}
