namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class TarifarioResultModel
    {
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public int? IdVendaXContrato { get; set; }
        public int? IdContratoTs { get; set; }
        public int? IdHotel { get; set; }
        public int? IdTemporadaTs { get; set; }
        public int? NumMinPax { get; set; }
        public int? NumMaxPax { get; set; }
        public int? NumeroDias { get; set; }
        public int? MinimoDias { get; set; }
        public string? FlgBloqueiaUso { get; set; }
        public decimal? TaxaManutencao { get; set; }
        public int? Quant { get; set; }
        public int? QuantApto { get; set; }
        public int? IdTipoUh { get; set; } 
        public decimal? NumeroPontos { get; set; }
        public decimal? VlrRepHotel { get; set; }
        public string? FlgFeriado { get; set; }
        public int? IdContrTsXPontos { get; set; }
        public string? TipoPeriodo { get; set; }
        public string? FlgAdultosFree { get; set; }
        public string? FlgCrianca1Free { get; set; }
        public string? FlgCrianca2Free { get; set; }
        public int? AnosFree { get; set; }
        public int? PagantesRegraFree { get; set; }
        public DateTime? TrgDtInclusao { get; set; }

    }
}
