namespace CMDomain.Models.Compras
{
    public class ProcessoCompraRemoverSolicitacaoCompraItemInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? CodProcesso { get; set; }
        public List<int>? SolicitacaoCompraItemRemover { get; set; }
    }
}
