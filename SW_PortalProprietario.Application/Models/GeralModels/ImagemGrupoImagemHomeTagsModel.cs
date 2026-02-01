namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImagemGrupoImagemHomeTagsModel : ModelBase
    {
        public ImagemGrupoImagemHomeTagsModel()
        { }

        public virtual int? ImagemGrupoImagemHomeId { get; set; }
        public virtual TagsModel Tags { get; set; }
    }
}

