using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

public class DocumentTemplateSummaryModel
{
    public int Id { get; set; }
    public EnumDocumentTemplateType TemplateType { get; set; }
    public string? Name { get; set; }
    public int Version { get; set; }
    public DateTime? DataHoraCriacao { get; set; }
    public bool Active { get; set; }
}

