namespace CMDomain.Models.Compras
{
    public class GetListaPrecoFornecedor : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? IdForCli { get; set; }//A
        public bool? ApenasAtivo { get; set; } = true;

    }

}
