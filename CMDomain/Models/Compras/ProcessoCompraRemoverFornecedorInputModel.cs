namespace CMDomain.Models.Compras
{
    public class ProcessoCompraRemoverFornecedorInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? CodProcesso { get; set; }
        public List<int>? FornecedoresRemover { get; set; }
    }
}
