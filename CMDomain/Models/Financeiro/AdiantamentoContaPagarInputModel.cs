namespace CMDomain.Models.Financeiro
{
    public class AdiantamentoContaPagarInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? IdFornecedor { get; set; }
        public int? CodTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? ComplDocumento { get; set; }
        public DateTime? DataEmissao { get; set; }
        public decimal? ValorDocumento { get; set; }
        public string? ObsLancamento { get; set; }
        public string? HistoricoLanc { get; set; }
        public DateTime? Vencimento { get; set; }
        public DateTime? DataProgramada { get; set; }
        public string? LinhaDigitavelBoleto { get; set; }
        public int? IdContaBancaria { get; set; }
        public int? IdContaBancariaXChavePix { get; set; }
        public int? IdModeloDocumento { get; set; }
        public string? SerieDocumento { get; set; }
        public int? TipoPagamentoId { get; set; }
        public int? ContaCaixaXFormaPagtoId { get; set; }
        public List<ContaPagarAlteradorValorInputModel> AlteradoresDocumento { get; set; } = new List<ContaPagarAlteradorValorInputModel>();
        public List<ContaPagarRateioInputModel> Rateio { get; set; } = new List<ContaPagarRateioInputModel>();

    }
}
