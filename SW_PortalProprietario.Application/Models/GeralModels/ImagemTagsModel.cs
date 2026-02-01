namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImagemTagsModel : ModelBase
    {
        public ImagemTagsModel()
        { }

        public virtual int? ImagemId { get; set; }
        public virtual TagsModel Tags { get; set; }

    }
}
