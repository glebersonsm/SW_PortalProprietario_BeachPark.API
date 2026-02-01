namespace CMDomain.Models.Compras
{
    public class ProcessoCompraAdicionarFornecedorInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? CodProcesso { get; set; }
        public List<int>? FornecedoresAdicionar { get; set; }
    }
}
