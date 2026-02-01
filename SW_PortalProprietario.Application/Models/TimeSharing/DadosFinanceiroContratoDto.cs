namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class DadosFinanceiroContratoDto
    {
        public DateTime? DataVenda { get; set; }
        public int? IdVendaTs { get; set; }
        public int? IdVendaXContrato { get; set; }
        public string? NumeroContrato { get; set; }
        public string? NumProjetoContrato { get; set; }
        public string? NomeProduto { get; set; }
        public int? IdPessoa { get; set; }
        public string? NomeCliente { get; set; }
        public decimal? PontosBaixadosNaOperacao { get; set; }
        public string? DebCred { get; set; }
        public DateTime? DataOperacaoLancamento { get; set; }
        public int? IdTipoLancPontoTs { get; set; }
        public string? DescricaoTipoLanc { get; set; }
        public string? MotivoLancamento { get; set; }
        public int? IdReservasFront { get; set; }
        public int? IdReservaMigrada { get; set; }
        public string? NumReservaVhf { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public decimal? TotalPontos { get; set; }
        public decimal? ValorCompra { get; set; }
        public decimal? ValorPonto { get; set; }
        public decimal? ValorUtilizacao { get; set; }
        public decimal? PontosBaixadosAtual { get; set; }
        public decimal? SaldoPontosAtual { get; set; }
        public decimal? ValorSaldoAtual { get; set; }
        public string? StatusReserva { get; set; }
        public string? Rci { get; set; }
        public string? Fracionamento { get; set; }
        public string? Status_Book { get; set; }
        public DateTime? ValidadeCredito { get; set; }
        public decimal? DescontoAnual { get; set; }
        public int? Validade { get; set; }
        public string? TipoValidade { get; set; }
        public decimal? TaxaManutencao { get; set; }
        public decimal? PontosComprados { get; set; }
        public string? DataDebitoPeriodo { get; set; }
        public int? IdLancPontosTs { get; set; }
        public decimal? TotalTaxa  { get; set; }
        public decimal? TotalPagtoTaxa { get; set; }
        public string? Hotel { get; set; }

    }
}
