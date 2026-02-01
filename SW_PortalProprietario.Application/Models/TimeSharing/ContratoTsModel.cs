namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ContratoTsModel
    {
        public int? IdContratoTs { get; set; }
        public int? IdHotel {  get; set; }
        public int? IdTipoDcTaxa { get; set; }
        public int? IdTipoDcDebNUtil { get; set; }
        public int? IdTipoDcCredNUtil { get; set; }
        public int? IdTipoDcMultaCance { get; set; }
        public int? Validade { get; set; }
        public string? TipoValidade { get; set; }
        public decimal? NumeroPontos { get; set; }
        public int? DescontoAnual { get; set; }
        public decimal? PercTxManutPriUti { get; set; }
        public decimal? PercPontosPriUti { get; set; }
        public string? FlgGeraCredInutil { get; set; }
        public int? AnoInicial { get; set; }
        public int? NumMinDiasCancRes { get; set; }
        public int? NumMaxDiasFecFrac { get; set; }
        public int? UtilAltaTemp { get; set; }
        public string? FlgUsaPaxFree { get; set; }
        public int? PagantesRegraFree { get; set; }
        public int? HospedesFree { get; set; }
        public string? FlgCrianca1Free { get; set; }
        public string? FlgCrianca2Free { get; set; }
        public int? AnosFree { get; set; }
        public int? MaxPaxFreeReserva { get; set; }
        public int? FlgDtAniversario { get; set; }
        public string? FlgPrimUtilGratui { get; set; }
        public string? FlgUtilAltaTempAnoCompra { get; set; }
        public string? FlgSaldoInsuficiente { get; set; }
        public string? FlgCalculaPontosDiaADia { get; set; }
        public DateTime? DataVencimentoContrato { get; set; }

    }
}
