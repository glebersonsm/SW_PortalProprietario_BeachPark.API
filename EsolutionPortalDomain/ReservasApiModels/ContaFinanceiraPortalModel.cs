namespace EsolutionPortalDomain.ReservasApiModels
{
    public class ContaFinanceiraPortalModel
    {
        public int? Id { get; set; }
        public string? Numero { get; set; }
        public string? Tipo { get; set; }
        public int? Empresa { get; set; }
        public string? CaixaVirtual { get; set; }
        public string? CaixaVenda { get; set; }
        public int? CaixaFechamento { get; set; }
        public int? ExibirFluxoCaixa { get; set; }

    }
}
