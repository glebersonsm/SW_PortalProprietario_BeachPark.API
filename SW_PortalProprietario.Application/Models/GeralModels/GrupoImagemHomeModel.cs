namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class GrupoImagemHomeModel : ModelBase
    {
        public int? CompanyId { get; set; }
        public string? Name { get; set; }
        public int? Ordem { get; set; }
        public List<ImagemGrupoImagemHomeModel>? Images { get; set; }
        public List<GrupoImagemHomeTagsModel>? TagsRequeridas { get; set; }
    }
}

