namespace CMDomain.Models.Compras
{
    public class IncluirRemoverCompradorSolicitacaoCompraItemInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? IdComprador { get; set; }
        public List<int>? SolicitacaoCompraItemAdicionar { get; set; }
        public List<int>? SolicitacaoCompraItemRemover { get; set; }
    }
}
