namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class FaqTagsModel : ModelBase
    {
        public FaqTagsModel()
        { }

        public virtual int? FaqId { get; set; }
        public virtual TagsModel Tags { get; set; }

    }
}
