using SW_PortalProprietario.Application.Models.Empreendimento;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

public class VoucherDocumentResultModel
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public DadosImpressaoVoucherResultModel? DadosImpressao { get; set; }
    public string? HtmlFull { get; set; }
}

