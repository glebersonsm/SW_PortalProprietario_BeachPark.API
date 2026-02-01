using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

public class DocumentTemplateTagsModel : ModelBase
{
    public DocumentTemplateTagsModel()
    { }

    public virtual int? DocumentTemplateId { get; set; }
    public virtual TagsModel Tags { get; set; }
}

