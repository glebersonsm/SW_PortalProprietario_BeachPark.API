namespace CMDomain.Models.Compras
{
    public class RemoverListaPrecoFornecedorModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? IdForCli { get; set; }
    }

}
