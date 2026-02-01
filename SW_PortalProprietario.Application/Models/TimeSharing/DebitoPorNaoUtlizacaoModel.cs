using NHibernate.Mapping;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class DebitoPorNaoUtlizacaoModel
    {
        public DateTime? DataVenda { get; set; }
        public DateTime? ValidadeCredito { get; set; }
        public decimal? PontosUtilizados { get; set; }
        public decimal? CreditoPontos { get; set; }
        public decimal? DescontoAnual { get; set; }
        public decimal? TaxaAnual { get; set; }
        public int? IdContratoTs { get; set; }
        public string? NumeroContrato { get; set; }
        public int? IdVendaXContrato { get; set; }
        public int? AnoInicial {  get; set; }
        public string? FlgGeraCredNUtil { get; set; }
        
    }
}
