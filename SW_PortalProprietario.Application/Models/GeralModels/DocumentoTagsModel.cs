namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class DocumentoTagsModel : ModelBase
    {
        public DocumentoTagsModel()
        { }

        public virtual int? DocumentoId { get; set; }
        public virtual TagsModel Tags { get; set; }

    }
}
