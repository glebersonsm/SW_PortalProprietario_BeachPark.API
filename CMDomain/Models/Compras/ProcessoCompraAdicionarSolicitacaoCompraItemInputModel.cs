namespace CMDomain.Models.Compras
{
    public class ProcessoCompraAdicionarSolicitacaoCompraItemInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? CodProcesso { get; set; }
        public List<int>? SolicitacaoCompraItemAdicionar { get; set; }
    }
}
