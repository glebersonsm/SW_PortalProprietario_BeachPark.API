namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

public class IncentivoAgendamentoDocumentResultModel
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public DadosIncentivoAgendamentoModel? DadosImpressao { get; set; }
    public string HtmlFull { get; set; } = string.Empty;
}