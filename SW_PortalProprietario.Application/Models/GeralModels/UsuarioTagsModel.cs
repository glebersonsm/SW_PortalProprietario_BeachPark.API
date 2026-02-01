namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class UsuarioTagsModel : ModelBase
    {
        public UsuarioTagsModel()
        { }

        public virtual int? UsuarioId { get; set; }
        public virtual TagsModel Tags { get; set; }

    }
}
