namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImageGroupModel : ModelBase
    {
        public int? CompanyId { get; set; }
        public string? Name { get; set; }
        public int? Ordem { get; set; }
        public List<ImageGroupImageModel>? Images { get; set; }
        public List<GrupoImagemTagsModel>? TagsRequeridas { get; set; }

    }
}
