namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class GrupoFaqTagsModel : ModelBase
    {
        public GrupoFaqTagsModel()
        { }

        public virtual int? GrupoFaqId { get; set; }
        public virtual TagsModel Tags { get; set; }

    }
}
