namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class ImageGroupInputModel : CreateUpdateModelBase
    {
        public int? CompanyId { get; set; }
        public string? Name { get; set; }
        public int? Ordem { get; set; }
        public List<int>? TagsRequeridas { get; set; }
        public bool? RemoverTagsNaoEnviadas { get; set; } = false;

    }
}
