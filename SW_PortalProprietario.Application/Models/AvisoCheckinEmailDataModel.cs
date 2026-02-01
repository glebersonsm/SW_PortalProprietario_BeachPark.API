using CMDomain.Entities;
using CMDomain.Models.AlmoxarifadoModels;
using Dapper;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Models
{
    /// <summary>
    /// Modelo de dados para geração de aviso de check-in
    /// </summary>
    public class AvisoCheckinEmailDataModel
    {
        public string? HtmlContent { get; set; }
        public byte[]? PdfBytes { get; set; }
        public string? PdfFileName { get; set; }
        public ReservaInfo Reserva { get; set; } = null!;
        public DadosImpressaoVoucherResultModel DadosReserva { get; set; } = null!;
        public int DaysBefore { get; set; }

    }
}
