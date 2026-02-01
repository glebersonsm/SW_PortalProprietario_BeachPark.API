namespace CMDomain.Models.Compras
{
    public class ListaPrecoFornecedorViewModel
    {
        public int? IdContratoProd { get; set; }
        public int? IdEmpresa { get; set; }
        public int? IdForCli { get; set; }
        public DateTime? DataInicialVigencia { get; set; }
        public DateTime? DataFinalVigencia { get; set; }
        public string? CodProd { get; set; }//B
        public string? Descricao { get; set; }//C
        public string? Unidade { get; set; }//D
        public decimal? ValorUnitario { get; set; }//F
        public int? DiasParaEntrega { get; set; } = 1;//G
        public int? DiasParaPagamento { get; set; } = 30;//H

    }

}
