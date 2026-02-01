using NHibernate.Mapping;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class DadosFinanceirosContrato
    {
        public int? IdVendaXContrato { get; set; }
        public string? NumeroContrato { get; set; }
        public int? IdVendaTs { get; set; }
        public decimal? SaldoInadimplente { get; set; }
        public decimal? ValorTotalPago { get; set; }
        public decimal? ValorTotalContrato { get; set; }
        public decimal? PercentualIntegralizacao { get; set; }
        public decimal? NumeroPontos { get; set; }
        public decimal? SaldoPontos { get; set; }
        public DateTime? DataVenda {  get; set; }
        public DateTime? DataPrevistaLancamentoNU { get; set; }
        public decimal? ValorPrevistoDebitoNU { get; set; }
        public int? IdCliente { get; set; }
        public string? Nome { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Email { get; set; }
        public BloqueioTsModel? BloqueioTsModel { get; set; }
        public decimal? ValorDoPonto => ValorTotalContrato.GetValueOrDefault(0) > 0 && NumeroPontos.GetValueOrDefault(0) > 0 ?
            Math.Round(ValorTotalContrato.GetValueOrDefault(0) / NumeroPontos.GetValueOrDefault(0),5) : 0.00000m;
        public decimal? PontosIntegralizadosDisponiveis => PercentualIntegralizacao.GetValueOrDefault(0) > 0 && NumeroPontos.GetValueOrDefault(0) > 0 && SaldoPontos.GetValueOrDefault(0) > 0
            ? Math.Round(((PercentualIntegralizacao.GetValueOrDefault(0) * NumeroPontos.GetValueOrDefault(0) / 100))-(NumeroPontos.GetValueOrDefault(0)-SaldoPontos.GetValueOrDefault(0)),2) : 0.00m;
        public string? Status { get; set; }
    }
}
