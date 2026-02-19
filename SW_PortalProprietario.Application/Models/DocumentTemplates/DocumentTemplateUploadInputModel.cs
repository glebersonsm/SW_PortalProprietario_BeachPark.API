using System.Text.Json.Serialization;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

public class DocumentTemplateUploadInputModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EnumDocumentTemplateType TemplateType { get; set; }
    public string? Name { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public List<int>? TagsIds { get; set; }
    public List<int>? EmpresasIds { get; set; }
}

