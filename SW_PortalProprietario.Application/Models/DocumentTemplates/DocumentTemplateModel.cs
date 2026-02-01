using System.Text.Json.Serialization;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

public class DocumentTemplateModel : ModelBase
{
    public EnumDocumentTemplateType TemplateType { get; set; }
    public string? Name { get; set; }
    public int Version { get; set; }
    public string? ContentHtml { get; set; }
    public bool Active { get; set; }
    public DateTime? DataHoraCriacao { get; set; }
    public int? UsuarioCriacao { get; set; }
    public string? UsuarioCriacaoNome { get; set; }
    public List<DocumentTemplateTagDto>? Tags { get; set; }
}

public class DocumentTemplateTagDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

