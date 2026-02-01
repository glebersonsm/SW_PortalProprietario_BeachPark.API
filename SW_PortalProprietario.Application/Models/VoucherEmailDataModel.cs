using SW_PortalProprietario.Application.Models.DocumentTemplates;
using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalProprietario.Application.Models
{
    public class VoucherEmailDataModel
    {
        public VoucherDocumentResultModel VoucherPdf { get; set; } = null!;
        public DadosImpressaoVoucherResultModel DadosReserva { get; set; } = null!;
        public string? VoucherHtml { get; set; }
    }
}
