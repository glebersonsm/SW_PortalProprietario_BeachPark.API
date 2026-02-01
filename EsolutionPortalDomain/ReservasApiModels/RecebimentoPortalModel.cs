namespace EsolutionPortalDomain.ReservasApiModels
{
    public class RecebimentoPortalModel
    {
        public int? Id { get; set; }
        public decimal? Valor { get; set; }
        public string? DebitoCredito { get; set; }
        public string? LancaContaFinanceiraRecebimento { get; set; }
        public int? ContaFinanceira { get; set; }
        public string? Finalizadora { get; set; }

    }
}
