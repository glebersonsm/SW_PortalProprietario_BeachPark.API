namespace CMDomain.Models.Compras
{
    public class SearchCompradorSolicitacaoComprasModel : ModelRequestBase
    {
        public int? IdComprador { get; set; }
        public int? IdEmpresa { get; set; }
        public bool? CarregarItensSemComprador { get; set; } = true;
    }
}
