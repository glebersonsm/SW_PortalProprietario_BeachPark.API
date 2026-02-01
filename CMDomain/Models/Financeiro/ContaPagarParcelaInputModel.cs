namespace CMDomain.Models.Financeiro
{
    public class ContaPagarParcelaInputModel
    {
        public int? IdContaBancaria { get; set; }
        public DateTime? Vencimento { get; set; }
        public DateTime? DataProgramada { get; set; }
        public decimal? ValorParcela { get; set; }
        public string? LinhaDigitavelBoleto { get; set; }
        public int? IdContaBancariaXChavePix { get; set; }
        public int? TipoPagamentoId { get; set; }
        public int? ContaCaixaXFormaPagtoId { get; set; }
        public List<ContaPagarParcelaAlteradorValorInputModel> Alteradores { get; set; } = new List<ContaPagarParcelaAlteradorValorInputModel>();

    }
}
