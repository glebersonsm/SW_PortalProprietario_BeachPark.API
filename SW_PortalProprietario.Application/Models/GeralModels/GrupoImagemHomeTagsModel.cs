namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class GrupoImagemHomeTagsModel : ModelBase
    {
        public GrupoImagemHomeTagsModel()
        { }

        public virtual int? GrupoImagemHomeId { get; set; }
        public virtual TagsModel Tags { get; set; }
    }
}

