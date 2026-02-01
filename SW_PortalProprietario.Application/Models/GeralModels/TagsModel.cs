namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class TagsModel : ModelBase
    {
        public TagsModel()
        { }

        public virtual int? ParentId { get; set; }
        public virtual string? ParentNome { get; set; }
        public virtual string? ParentPath { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? Path { get; set; }

    }
}
