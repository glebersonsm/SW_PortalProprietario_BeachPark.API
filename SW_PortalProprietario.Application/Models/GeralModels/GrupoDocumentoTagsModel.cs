namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class GrupoDocumentoTagsModel : ModelBase
    {
        public GrupoDocumentoTagsModel()
        { }

        public virtual int? GrupoDocumentoId { get; set; }
        public virtual TagsModel Tags { get; set; }

    }
}
