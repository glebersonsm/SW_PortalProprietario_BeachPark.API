namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ContratoTimeSharingModel
    {
        public int? IdVendaTs { get; set; }
        public int? IdVendaXContrato { get; set; }
        public int? IdCliente { get; set; }
        public string? NomeCliente { get; set; }
        public string? EmailCliente { get; set; }
        public string? DocumentoCliente { get; set; }
        public string? ProjetoXContrato { get; set; }
        public string? TipoContrato { get; set; }
        public string? Cidade_Estado { get; set; }
        public DateTime? DataVenda { get; set; }
        public string? Status { get; set; }
        public string? NumeroContrato { get; set; }
        public string? Cancelado { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public string? Revertido { get; set; }
        public DateTime? DataReversao { get; set; }
        public string? SalaVendas { get; set; }
        public int? SaldoPontos { get; set; }
        public int? PessoaProviderId { get; set; }
        public decimal? ValorEntrada { get; set; }
        public int? QtdeParcelasEntrada { get; set; }
        public decimal? ValorFinanciado { get; set; }
        public int? QtdeParcelasFinanciamento { get; set; }
        public int? QtdeParcelasPagas { get; set; }
        public decimal? ValorTotalVenda { get; set; }
        public decimal? PercentualIntegralizacao { get; set; }
        public DateTime? DataValidade { get; set; }
        public decimal? TotalPontos { get; set; }
        public string? IdRCI { get; set; }
        public DadosFinanceirosContratoModel? DadosUtilizacaoContrato { get; set; }

    }
}
