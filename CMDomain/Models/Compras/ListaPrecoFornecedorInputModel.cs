namespace CMDomain.Models.Compras
{
    public class ListaPrecoFornecedorInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? IdForCli { get; set; }//A
        public DateTime? DataInicialVigencia { get; set; }
        public DateTime? DataFinalVigencia { get; set; }

        public List<ListaPrecoFornecedorInputItemModel> Items { get; set; } = new List<ListaPrecoFornecedorInputItemModel>();
    }

    public class ListaPrecoFornecedorInputItemModel
    {
        public string? CodProd { get; set; }//B
        public string? Unidade { get; set; }//D
        public decimal? ValorUnitario { get; set; }//F
        public int? DiasParaEntrega { get; set; } = 1;//G
        public int? DiasParaPagamento { get; set; } = 30;//H
    }
}
