namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImagemGrupoImagemTagsModel : ModelBase
    {
        public ImagemGrupoImagemTagsModel()
        { }

        public virtual int? ImagemGrupoImagemId { get; set; }
        public virtual TagsModel Tags { get; set; }

    }
}
