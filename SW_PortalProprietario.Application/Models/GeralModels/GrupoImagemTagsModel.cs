namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class GrupoImagemTagsModel : ModelBase
    {
        public GrupoImagemTagsModel()
        { }

        public virtual int? GrupoImagemId { get; set; }
        public virtual TagsModel Tags { get; set; }

    }
}
