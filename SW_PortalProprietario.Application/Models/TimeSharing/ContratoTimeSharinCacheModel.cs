namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ContratoTimeSharinCacheModel
    {
        public string? NumeroContrato { get; set; }
        public int? IdVendaXContrato { get; set; }
        public int? IdVendaTs { get; set; }
        public int? IdPromotor { get; set; }
        public int? IdContratoTs { get; set; }
        public int? IdProjetoTs { get; set; }
        public int? IdCliente { get; set; }
        public string? NomeCliente { get; set; }
        public string? DocumentoCliente { get; set; }
        public string? EmailCliente { get; set; }
        public string? StatusContrato { get; set; }
        public DateTime? DataVenda { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? FlgCancelado { get; set; }
        public string? FlgRevertido { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public DateTime? DataReversao { get; set; }
        public DateTime? DataIntegraliza { get; set; }
        public int? TaxaPrimeiraUtili { get; set; }
        public int? TaxaRci { get; set; }
        public decimal? ValorBase { get; set; }
        public decimal? ValorFinal { get; set; }
        public int? IdAgenciaTs { get; set; }
        public int? Validade { get; set; }
        public string? TipoValidade { get; set; }
        public decimal? NumeroPontos { get; set; }
        public decimal? DescontoAnual { get; set; }
        public int? IdHotel { get; set; }
        public int? AnoInicial { get; set; }

    }
}
