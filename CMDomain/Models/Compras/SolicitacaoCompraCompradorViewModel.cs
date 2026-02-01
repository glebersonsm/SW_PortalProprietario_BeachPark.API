namespace CMDomain.Models.Compras
{
    public class SolicitacaoComprasCompradorViewModel
    {
        public int? IdComprador { get; set; }
        public string? Comprador { get; set; }
        public int? IdEmpresa { get; set; }

        public List<SolicitacaoCompraItemCompradorViewModel> ItensSemComprador { get; set; } = new List<SolicitacaoCompraItemCompradorViewModel>();
        public List<SolicitacaoCompraItemCompradorViewModel> ItensAtribuidosAoComprador { get; set; } = new List<SolicitacaoCompraItemCompradorViewModel>();

    }
}
