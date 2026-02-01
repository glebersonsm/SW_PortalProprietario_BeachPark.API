using EsolutionPortalDomain.Enums;

namespace SW_PortalProprietario.Application.Models
{
    public class GetHtmlValuesModel
    {
        public int? CotaOrContratoId { get; set; }
        public int? UhCondominioId { get; set; }
        public int? PeriodoCotaDisponibilidadeId { get; set; }
        public EnumReportType? reportType { get; set; } = EnumReportType.ContratoSCP;

    }
}
