namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class ConfigContratoModel
    {
        public decimal? Valor { get; set; }
        public int? Validade { get; set; }
        public string? TipoValidade { get; set; }
        public decimal? NumeroPontos { get; set; }
        public int? IdTipoDcContrato { get; set; }
        public int? IdTipoDcTaxa { get; set; }
        public int? IdTipoDcMultaCanc { get; set; }
        public decimal? PercIntegraliza { get; set; }
        public decimal? PercPontosPriUti { get; set; }
        public int? IdTipoDcDebNUtil { get; set; }
        public int? AnoInicial { get; set; }
        public string? FlgPrimUtilGratui { get; set; }
        public string? FlgUtilVlrProp { get; set; }
        public int? IdTipoDcTaxaPensao { get; set; }
        public string? FlgUtilPontosProp { get; set; }

    }
}
