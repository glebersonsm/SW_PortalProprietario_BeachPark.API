namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class TagsInputModel : CreateUpdateModelBase
    {
        public int? TagsParentId { get; set; }
        public string? Nome { get; set; }

    }
}
