using System.Text.Json.Serialization;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

public class DocumentTemplateSearchModel
{
    public EnumDocumentTemplateType TemplateType { get; set; }
    public int? TemplateId { get; set; }
    
}

