namespace CMDomain.Models.Compras
{
    public class ProcessoCompraInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? IdComprador { get; set; }
        public List<int>? SolicitacaoCompraItemAdicionar { get; set; }
        public List<int>? FornecedoresAdicionar { get; set; }
    }
}
