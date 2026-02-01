namespace CMDomain.Models.Financeiro
{
    public class AdiantamentoContaPagarViewModel
    {
        public int? IdDocumento { get; set; }
        public int? IdEmpresa { get; set; }
        public int? IdFornecedor { get; set; }
        public decimal? ValorOriginal { get; set; }
        public string? NomeFornecedor { get; set; }
        public string? RazaoSocialFornecedor { get; set; }
        public string? CpfCnpjFornecedor { get; set; }
        public string? TipoPessoaFornecedor { get; set; }
        public int? CodTipoDocumento { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? ComplDocumento { get; set; }
        public string? AdiantamentoPagoAoFornecedor { get; set; }
        public string? Regularizado { get; set; }
        public int? Usuario { get; set; }
        public string? Operacao { get; set; }
        public string? NomeUsuario { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? DataLancamento { get; set; }
        public DateTime? Vencimento { get; set; }
        public string? ObsLancamento { get; set; }
        public string? HistoricoLanc { get; set; }
        public DateTime? DataProgramada { get; set; }
        public int? TipoPagamentoId { get; set; }
        public int? IdContaBancariaXChavePix { get; set; }
        public string? ChavePix { get; set; }
        public int? IdTipoChave { get; set; }
        public string? TipoChavePix { get; set; }
        public List<ContaPagarRateioViewModel> Rateios { get; set; } = new List<ContaPagarRateioViewModel>();
        public List<ContaPagarParcelaAlteradorValorViewModel>? AlteradoresValoresDocumento { get; set; } = new List<ContaPagarParcelaAlteradorValorViewModel>();

    }
}
